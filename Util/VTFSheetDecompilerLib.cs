using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TGASharpLib;
using VTFLib;
namespace VTFSheetDecompilerLib;
public static class VTFSheetDecompiler {
   public record FrameInfo(string FileName, float Duration, (float uMin, float vMin, float uMax, float vMax) UV);
   public record SequenceInfo(int SequenceId, bool Clamp, List<FrameInfo> Frames);
   public static byte[] DumpVtfSheetResource(string path) {
      if (!File.Exists(path)) return null;
      if (!VTFLib.VTFLib.vlInitialize()) return null;
      uint handle = 0;
      try {
         if (!VTFLib.VTFLib.vlCreateImage(out handle)) return null;
         if (!VTFLib.VTFLib.vlBindImage(handle)) return null;
         if (!VTFLib.VTFLib.vlImageLoad(path, false)) return null;
         uint resourceCount = VTFLib.VTFLib.vlImageGetResourceCount();
         for (uint i = 0; i < resourceCount; i++) {
            uint type = VTFLib.VTFLib.vlImageGetResourceType(i);
            if (type != 0x10) continue;
            if (!VTFLib.VTFLib.vlImageGetHasResource(type)) continue;
            uint dataSize;
            IntPtr ptr = VTFLib.VTFLib.vlImageGetResourceData(type, out dataSize);
            if (ptr == IntPtr.Zero || dataSize == 0) return null;
            byte[] rawData = new byte[dataSize];
            Marshal.Copy(ptr, rawData, 0, (int)dataSize);
            return rawData;
         }
         return null;
      } finally {
         if (handle != 0) VTFLib.VTFLib.vlDeleteImage(handle);
         VTFLib.VTFLib.vlShutdown();
      }
   }
   public static List<SequenceInfo> ProcessVtfSheetResource(byte[] rawData, string mksOutputPath = null) {
      if (rawData == null || rawData.Length < 8) throw new InvalidDataException("Invalid sheet resource");
      var sequences = new List<SequenceInfo>();
      using var ms = new MemoryStream(rawData);
      using var br = new BinaryReader(ms);
      int version = br.ReadInt32();
      int numSequences = br.ReadInt32();
      var mksLines = new List<string>
      {
         "// Reconstructed .mks from VTF_RSRC_SHEET",
         "// NOTE: This is not a 1:1 reconstruction of the original .mks file.",
         "// Complex channel-packed textures may result in an incomplete or incorrect representation.",
         $"// Generated on {DateTime.Now}",
         "",
      };
      for (int i = 0; i < numSequences; i++) {
         int seqId = br.ReadInt32();
         bool clamp = br.ReadInt32() == 1;
         int numFrames = br.ReadInt32();
         float totalDuration = br.ReadSingle();
         mksLines.Add($"// Sequence {seqId}, Clamp: {clamp}, Frames: {numFrames}");
         mksLines.Add($"sequence {seqId}");
         if (!clamp) mksLines.Add("loop");
         var frames = new List<FrameInfo>();
         for (int f = 0; f < numFrames; f++) {
            float duration = br.ReadSingle();
            var uvs = new (float, float, float, float)[4];
            for (int t = 0; t < 4; t++) {
               uvs[t] = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }
            string frameName = $"frame_{seqId}_{f}.tga";
            mksLines.Add($"frame {frameName} {duration:F3}");
            frames.Add(new FrameInfo(frameName, duration, uvs[0]));
         }
         mksLines.Add("");
         sequences.Add(new SequenceInfo(seqId, clamp, frames));
      }
      if (mksOutputPath != null) {
         Directory.CreateDirectory(Path.GetDirectoryName(mksOutputPath));
         File.WriteAllLines(mksOutputPath, mksLines);
      }
      return sequences;
   }
   public static void ExtractPNGSpritesFromAtlas(string vtfPath, List<SequenceInfo> sequences, string outputDir) {
      Directory.CreateDirectory(outputDir);
      using var converter = new VtfConverter();
      using var ms = new MemoryStream();
      converter.ConvertToPng(vtfPath, false, ms);
      ms.Seek(0, SeekOrigin.Begin);
      using var atlas = Image.Load<Rgba32>(ms);
      int width = atlas.Width;
      int height = atlas.Height;
      foreach (var seq in sequences) {
         foreach (var frame in seq.Frames) {
            var (uMin, vMin, uMax, vMax) = frame.UV;
            int x = (int)Math.Floor(uMin * width);
            int y = (int)Math.Floor(vMin * height);
            int w = (int)Math.Ceiling(uMax * width) - x;
            int h = (int)Math.Ceiling(vMax * height) - y;
            w = Math.Clamp(w, 1, width - x);
            h = Math.Clamp(h, 1, height - y);
            var rect = new Rectangle(x, y, w, h);
            using var frameImage = atlas.Clone(ctx => ctx.Crop(rect));
            string pngName = Path.ChangeExtension(frame.FileName, ".png");
            string framePath = Path.Combine(outputDir, pngName);
            frameImage.SaveAsPng(framePath);
         }
      }
   }
   public static void ExtractTGASpritesFromAtlas(string vtfPath, List<SequenceInfo> sequences, string outputDir) {
      Directory.CreateDirectory(outputDir);
      using var converter = new VtfConverter();
      using var ms = new MemoryStream();
      converter.ConvertToPng(vtfPath, false, ms);
      ms.Seek(0, SeekOrigin.Begin);
      using var atlas = Image.Load<Rgba32>(ms);
      int width = atlas.Width;
      int height = atlas.Height;
      Rgba32[] pixels = new Rgba32[width * height];
      atlas.CopyPixelDataTo(pixels);
      foreach (var seq in sequences) {
         foreach (var frame in seq.Frames) {
            var (uMin, vMin, uMax, vMax) = frame.UV;
            int x = (int)Math.Floor(uMin * width);
            int y = (int)Math.Floor(vMin * height);
            int w = (int)Math.Ceiling(uMax * width) - x;
            int h = (int)Math.Ceiling(vMax * height) - y;
            w = Math.Clamp(w, 1, width - x);
            h = Math.Clamp(h, 1, height - y);
            var croppedPixels = new Rgba32[w * h];
            for (int dy = 0; dy < h; dy++) {
               int srcY = y + dy;
               for (int dx = 0; dx < w; dx++) {
                  int srcX = x + dx;
                  croppedPixels[dy * w + dx] = pixels[srcY * width + srcX];
               }
            }
            var tgaBytes = new byte[w * h * 4];
            for (int iPx = 0; iPx < croppedPixels.Length; iPx++) {
               var px = croppedPixels[iPx];
               tgaBytes[iPx * 4 + 0] = px.B;
               tgaBytes[iPx * 4 + 1] = px.G;
               tgaBytes[iPx * 4 + 2] = px.R;
               tgaBytes[iPx * 4 + 3] = px.A;
            }
            var tga = new TGA {
               Header =
                {
                        ImageType = TgaImageType.RLE_TrueColor,
                        ColorMapType = TgaColorMapType.NoColorMap,
                        ImageSpec =
                        {
                            ImageWidth = (ushort)w,
                            ImageHeight = (ushort)h,
                            PixelDepth = TgaPixelDepth.Bpp32
                        }
                    }
            };
            tga.ImageOrColorMapArea.ImageData = tgaBytes;
            string framePath = Path.Combine(outputDir, frame.FileName);
            tga.Save(framePath);
         }
      }
   }
}