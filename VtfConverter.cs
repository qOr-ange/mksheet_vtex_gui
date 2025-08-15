using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;

namespace VTFLib {
   public sealed class VtfConverter : IDisposable {
      private bool _initialized = false;
      private uint _imageHandle;

      public VtfConverter() {
         if (!VTFLib.vlInitialize())
            throw new InvalidOperationException("Failed to initialize VTFLib: " + GetError());

         _initialized = true;
         _imageHandle = 0;

         VTFLib.vlSetInteger(VTFLib.VTFLibOption.DXT_QUALITY, (int)VTFLib.VTFDXTQuality.DXT_QUALITY_HIGHEST);
      }
      ~VtfConverter() {
         Dispose(false);
      }
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }
      private void Dispose(bool disposing) {
         if (_imageHandle != 0) {
            VTFLib.vlDeleteImage(_imageHandle);
            _imageHandle = 0;
         }

         if (_initialized) {
            VTFLib.vlShutdown();
            _initialized = false;
         }
      }
      private string GetError() => Marshal.PtrToStringAnsi(VTFLib.vlGetLastError()) ?? "Unknown error";
      private bool BindNewImage() {
         if (_imageHandle == 0) {
            if (!VTFLib.vlCreateImage(out _imageHandle))
               throw new InvalidOperationException("Failed to create image handle: " + GetError());
         }

         if (!VTFLib.vlBindImage(_imageHandle))
            throw new InvalidOperationException("Failed to bind image: " + GetError());

         return true;
      }

      public void ConvertToVtf(
          string inputFile,
          string outputFile,
          bool generateMipmaps = true,
          bool thumbnail = true,
          VTFLib.VTFImageFormat format = VTFLib.VTFImageFormat.IMAGE_FORMAT_BGRA8888) {
         using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(inputFile);
         ConvertToVtf(image, outputFile, generateMipmaps, thumbnail, format);
      }

      public void ConvertToVtf(
          Image<Rgba32> image,
          string outputFile,
          bool generateMipmaps = true,
          bool thumbnail = true,
          VTFLib.VTFImageFormat format = VTFLib.VTFImageFormat.IMAGE_FORMAT_A8) {
         BindNewImage();

         var width = (uint)image.Width;
         var height = (uint)image.Height;

         var pixelData = new byte[width * height * 4];
         int index = 0;

         image.ProcessPixelRows(accessor =>
         {
            for (int y = 0; y < accessor.Height; y++) {
               var row = accessor.GetRowSpan(y);
               for (int x = 0; x < accessor.Width; x++) {
                  var pixel = row[x];
                  pixelData[index++] = pixel.R;
                  pixelData[index++] = pixel.G;
                  pixelData[index++] = pixel.B;
                  pixelData[index++] = pixel.A;
               }
            }
         });


         var options = VTFLib.DefaultCreateOptions;
         options.ImageFormat = format;
         options.bMipmaps = generateMipmaps;
         options.bThumbnail = thumbnail;
         options.bResize = false;
         options.bGammaCorrection = false;

         var pinnedData = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
         try {
            if (!VTFLib.vlImageCreateSingle(width, height, pinnedData.AddrOfPinnedObject(), ref options))
               throw new InvalidOperationException("Failed to create VTF: " + GetError());
         } finally {
            pinnedData.Free();
         }

         if (!VTFLib.vlImageSave(outputFile))
            throw new InvalidOperationException("Failed to save VTF: " + GetError());
      }
      public void ConvertToPng(string vtfFile, string pngOutput, bool extractThumbnail = false) {
         if (!File.Exists(vtfFile))
            throw new FileNotFoundException("VTF file not found.", vtfFile);

         BindNewImage();

         if (!VTFLib.vlImageLoad(vtfFile, false))
            throw new InvalidOperationException("Failed to load VTF: " + GetError());

         IntPtr sourceDataPtr;
         uint width, height;
         VTFLib.VTFImageFormat sourceFormat;

         if (extractThumbnail && VTFLib.vlImageGetHasThumbnail()) {
            sourceDataPtr = VTFLib.vlImageGetThumbnailData();
            width = VTFLib.vlImageGetThumbnailWidth();
            height = VTFLib.vlImageGetThumbnailHeight();
            sourceFormat = VTFLib.vlImageGetThumbnailFormat();
         } else {
            sourceDataPtr = VTFLib.vlImageGetData(0, 0, 0, 0);
            width = VTFLib.vlImageGetWidth();
            height = VTFLib.vlImageGetHeight();
            sourceFormat = VTFLib.vlImageGetFormat();
         }

         if (sourceDataPtr == IntPtr.Zero)
            throw new InvalidOperationException("No image data available.");

         uint sourceDataSize = VTFLib.vlImageComputeMipmapSize(width, height, 1, 0, sourceFormat);

         int rgbaDataSize = (int)(width * height * 4);
         byte[] rgbaData = new byte[rgbaDataSize];

         GCHandle destHandle = GCHandle.Alloc(rgbaData, GCHandleType.Pinned);
         IntPtr destPtr = destHandle.AddrOfPinnedObject();

         try {
            if (sourceFormat == VTFLib.VTFImageFormat.IMAGE_FORMAT_RGBA8888) {
               Marshal.Copy(sourceDataPtr, rgbaData, 0, rgbaDataSize);
            } else {
               byte[] sourceTempBuffer = new byte[sourceDataSize];
               Marshal.Copy(sourceDataPtr, sourceTempBuffer, 0, (int)sourceDataSize);
               GCHandle sourceHandle = GCHandle.Alloc(sourceTempBuffer, GCHandleType.Pinned);
               IntPtr sourceTempPtr = sourceHandle.AddrOfPinnedObject();

               try {
                  bool conversionSuccess = VTFLib.vlImageConvertToRGBA8888(
                      sourceTempPtr,
                      destPtr, 
                      width, 
                      height,
                      sourceFormat
                  );

                  if (!conversionSuccess) {
                     throw new InvalidOperationException("Failed to convert VTF data to RGBA8888: " + GetError());
                  }
               } finally {
                  if (sourceHandle.IsAllocated)
                     sourceHandle.Free();
               }
            }

            using (var img = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(rgbaData, (int)width, (int)height)) {
               img.SaveAsPng(pngOutput, new PngEncoder { TransparentColorMode = PngTransparentColorMode.Preserve });
            }
         } finally {
            if (destHandle.IsAllocated)
               destHandle.Free();
         }
      }
      public void ConvertToPng(string vtfFile, bool extractThumbnail, Stream outputStream)
      {
         if (!File.Exists(vtfFile))
            throw new FileNotFoundException("VTF file not found.", vtfFile);

         BindNewImage();

         if (!VTFLib.vlImageLoad(vtfFile, false))
            throw new InvalidOperationException("Failed to load VTF: " + GetError());

         IntPtr sourceDataPtr;
         uint width, height;
         VTFLib.VTFImageFormat sourceFormat;

         if (extractThumbnail && VTFLib.vlImageGetHasThumbnail()) {
            sourceDataPtr = VTFLib.vlImageGetThumbnailData();
            width = VTFLib.vlImageGetThumbnailWidth();
            height = VTFLib.vlImageGetThumbnailHeight();
            sourceFormat = VTFLib.vlImageGetThumbnailFormat();
         } else {
            sourceDataPtr = VTFLib.vlImageGetData(0, 0, 0, 0);
            width = VTFLib.vlImageGetWidth();
            height = VTFLib.vlImageGetHeight();
            sourceFormat = VTFLib.vlImageGetFormat();
         }

         if (sourceDataPtr == IntPtr.Zero)
            throw new InvalidOperationException("No image data available.");

         uint sourceDataSize = VTFLib.vlImageComputeMipmapSize(width, height, 1, 0, sourceFormat);
         int rgbaDataSize = (int)(width * height * 4);
         byte[] rgbaData = new byte[rgbaDataSize];

         GCHandle destHandle = GCHandle.Alloc(rgbaData, GCHandleType.Pinned);
         IntPtr destPtr = destHandle.AddrOfPinnedObject();

         try {
            if (sourceFormat == VTFLib.VTFImageFormat.IMAGE_FORMAT_RGBA8888) {
               Marshal.Copy(sourceDataPtr, rgbaData, 0, rgbaDataSize);
            } else {
               byte[] sourceTempBuffer = new byte[sourceDataSize];
               Marshal.Copy(sourceDataPtr, sourceTempBuffer, 0, (int)sourceDataSize);
               GCHandle sourceHandle = GCHandle.Alloc(sourceTempBuffer, GCHandleType.Pinned);
               IntPtr sourceTempPtr = sourceHandle.AddrOfPinnedObject();
               try {
                  bool conversionSuccess = VTFLib.vlImageConvertToRGBA8888(
                      sourceTempPtr,
                      destPtr,
                      width,
                      height,
                      sourceFormat
                  );
                  if (!conversionSuccess) {
                     throw new InvalidOperationException("Failed to convert VTF data to RGBA8888: " + GetError());
                  }
               } finally {
                  if (sourceHandle.IsAllocated)
                     sourceHandle.Free();
               }
            }

            using (var img = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(rgbaData, (int)width, (int)height)) {
               img.SaveAsPng(outputStream, new PngEncoder { TransparentColorMode = PngTransparentColorMode.Preserve });
            }
            outputStream.Flush();
         } finally {
            if (destHandle.IsAllocated)
               destHandle.Free();
         }
      }
      public VtfInfo GetInfo(string vtfFile) {
         BindNewImage();

         if (!VTFLib.vlImageLoad(vtfFile, true))
            throw new InvalidOperationException("Failed to load VTF header: " + GetError());

         return new VtfInfo {
            Width = VTFLib.vlImageGetWidth(),
            Height = VTFLib.vlImageGetHeight(),
            Depth = VTFLib.vlImageGetDepth(),
            Frames = VTFLib.vlImageGetFrameCount(),
            Faces = VTFLib.vlImageGetFaceCount(),
            Mipmaps = VTFLib.vlImageGetMipmapCount(),
            Format = VTFLib.vlImageGetFormat(),
            HasThumbnail = VTFLib.vlImageGetHasThumbnail(),
            IsCubeMap = VTFLib.vlImageGetFaceCount() == 6,
            Version = $"{VTFLib.vlImageGetMajorVersion()}.{VTFLib.vlImageGetMinorVersion()}"
         };
      }
   }

   public class VtfInfo {
      public uint Width { get; set; }
      public uint Height { get; set; }
      public uint Depth { get; set; }
      public uint Frames { get; set; }
      public uint Faces { get; set; }
      public uint Mipmaps { get; set; }
      public VTFLib.VTFImageFormat Format { get; set; }
      public bool HasThumbnail { get; set; }
      public bool IsCubeMap { get; set; }
      public string Version { get; set; }

      public override string ToString() {
         return $"VTF {Width}x{Height}x{Depth}, Format={Format}, Mipmaps={Mipmaps}, Frames={Frames}, Faces={Faces}, Thumbnail={HasThumbnail}, Version={Version}";
      }
   }
}