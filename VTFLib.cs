using System.Runtime.InteropServices;
namespace VTFLib {
   /// <summary>
   /// P/Invoke bindings for VTFLib.dll (Version 1.3.2)
   /// </summary>
   public static class VTFLib {
      private const string DllName = "lib/VTFLib.dll";

      // === Type Aliases (from vl* to native) ===
      [return: MarshalAs(UnmanagedType.U1)]
      private delegate void PReadCloseProc(IntPtr userData);
      [return: MarshalAs(UnmanagedType.U1)]
      private delegate byte PReadOpenProc(IntPtr userData);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PReadReadProc(IntPtr userData, uint size, IntPtr buffer);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PReadSeekProc(int offset, VLSeekMode origin, IntPtr userData);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PReadSizeProc(IntPtr userData);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PReadTellProc(IntPtr userData);

      [return: MarshalAs(UnmanagedType.U1)]
      private delegate void PWriteCloseProc(IntPtr userData);
      [return: MarshalAs(UnmanagedType.U1)]
      private delegate byte PWriteOpenProc(IntPtr userData);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PWriteWriteProc(IntPtr userData, uint size, IntPtr buffer);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PWriteSeekProc(int offset, VLSeekMode origin, IntPtr userData);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PWriteSizeProc(IntPtr userData);
      [return: MarshalAs(UnmanagedType.U4)]
      private delegate uint PWriteTellProc(IntPtr userData);

      // === Enums ===

      public enum VTFLibOption {
         DXT_QUALITY = 0,
         LUMINANCE_WEIGHT_R,
         LUMINANCE_WEIGHT_G,
         LUMINANCE_WEIGHT_B,
         BLUESCREEN_MASK_R,
         BLUESCREEN_MASK_G,
         BLUESCREEN_MASK_B,
         BLUESCREEN_CLEAR_R,
         BLUESCREEN_CLEAR_G,
         BLUESCREEN_CLEAR_B,
         FP16_HDR_KEY,
         FP16_HDR_SHIFT,
         FP16_HDR_GAMMA,
         UNSHARPEN_RADIUS,
         UNSHARPEN_AMOUNT,
         UNSHARPEN_THRESHOLD,
         XSHARPEN_STRENGTH,
         XSHARPEN_THRESHOLD,
         VMT_PARSE_MODE
      }
      public enum VTFImageFormat {
         IMAGE_FORMAT_RGBA8888 = 0,
         IMAGE_FORMAT_ABGR8888,
         IMAGE_FORMAT_RGB888,
         IMAGE_FORMAT_BGR888,
         IMAGE_FORMAT_RGB565,
         IMAGE_FORMAT_I8,
         IMAGE_FORMAT_IA88,
         IMAGE_FORMAT_P8,
         IMAGE_FORMAT_A8,
         IMAGE_FORMAT_RGB888_BLUESCREEN,
         IMAGE_FORMAT_BGR888_BLUESCREEN,
         IMAGE_FORMAT_ARGB8888,
         IMAGE_FORMAT_BGRA8888,
         IMAGE_FORMAT_DXT1,
         IMAGE_FORMAT_DXT3,
         IMAGE_FORMAT_DXT5,
         IMAGE_FORMAT_BGRX8888,
         IMAGE_FORMAT_BGR566,
         IMAGE_FORMAT_BGRX5551,
         IMAGE_FORMAT_BGRA4444,
         IMAGE_FORMAT_DXT1_ONEBITALPHA,
         IMAGE_FORMAT_BGRA5551,
         IMAGE_FORMAT_UV88,
         IMAGE_FORMAT_UVWQ8888,
         IMAGE_FORMAT_RGBA16161616F,
         IMAGE_FORMAT_RGBA16161616,
         IMAGE_FORMAT_UVLX8888,
         IMAGE_FORMAT_R32F,
         IMAGE_FORMAT_RGB323232F,
         IMAGE_FORMAT_RGBA32323232F,
         IMAGE_FORMAT_NV_DST16,
         IMAGE_FORMAT_NV_DST24,
         IMAGE_FORMAT_NV_INTZ,
         IMAGE_FORMAT_NV_RAWZ,
         IMAGE_FORMAT_ATI_DST16,
         IMAGE_FORMAT_ATI_DST24,
         IMAGE_FORMAT_NV_NULL,
         IMAGE_FORMAT_ATI2N,
         IMAGE_FORMAT_ATI1N,
         IMAGE_FORMAT_COUNT,
         IMAGE_FORMAT_NONE = -1
      }

      [Flags]
      public enum VTFImageFlag : uint {
         TEXTUREFLAGS_POINTSAMPLE = 0x00000001,
         TEXTUREFLAGS_TRILINEAR = 0x00000002,
         TEXTUREFLAGS_CLAMPS = 0x00000004,
         TEXTUREFLAGS_CLAMPT = 0x00000008,
         TEXTUREFLAGS_ANISOTROPIC = 0x00000010,
         TEXTUREFLAGS_HINT_DXT5 = 0x00000020,
         TEXTUREFLAGS_SRGB = 0x00000040,
         TEXTUREFLAGS_DEPRECATED_NOCOMPRESS = 0x00000040,
         TEXTUREFLAGS_NORMAL = 0x00000080,
         TEXTUREFLAGS_NOMIP = 0x00000100,
         TEXTUREFLAGS_NOLOD = 0x00000200,
         TEXTUREFLAGS_MINMIP = 0x00000400,
         TEXTUREFLAGS_PROCEDURAL = 0x00000800,
         TEXTUREFLAGS_ONEBITALPHA = 0x00001000,
         TEXTUREFLAGS_EIGHTBITALPHA = 0x00002000,
         TEXTUREFLAGS_ENVMAP = 0x00004000,
         TEXTUREFLAGS_RENDERTARGET = 0x00008000,
         TEXTUREFLAGS_DEPTHRENDERTARGET = 0x00010000,
         TEXTUREFLAGS_NODEBUGOVERRIDE = 0x00020000,
         TEXTUREFLAGS_SINGLECOPY = 0x00040000,
         TEXTUREFLAGS_UNUSED0 = 0x00080000,
         TEXTUREFLAGS_DEPRECATED_ONEOVERMIPLEVELINALPHA = 0x00080000,
         TEXTUREFLAGS_UNUSED1 = 0x00100000,
         TEXTUREFLAGS_DEPRECATED_PREMULTCOLORBYONEOVERMIPLEVEL = 0x00100000,
         TEXTUREFLAGS_UNUSED2 = 0x00200000,
         TEXTUREFLAGS_DEPRECATED_NORMALTODUDV = 0x00200000,
         TEXTUREFLAGS_UNUSED3 = 0x00400000,
         TEXTUREFLAGS_DEPRECATED_ALPHATESTMIPGENERATION = 0x00400000,
         TEXTUREFLAGS_NODEPTHBUFFER = 0x00800000,
         TEXTUREFLAGS_UNUSED4 = 0x01000000,
         TEXTUREFLAGS_DEPRECATED_NICEFILTERED = 0x01000000,
         TEXTUREFLAGS_CLAMPU = 0x02000000,
         TEXTUREFLAGS_VERTEXTEXTURE = 0x04000000,
         TEXTUREFLAGS_SSBUMP = 0x08000000,
         TEXTUREFLAGS_UNUSED5 = 0x10000000,
         TEXTUREFLAGS_DEPRECATED_UNFILTERABLE_OK = 0x10000000,
         TEXTUREFLAGS_BORDER = 0x20000000,
         TEXTUREFLAGS_DEPRECATED_SPECVAR_RED = 0x40000000,
         TEXTUREFLAGS_DEPRECATED_SPECVAR_ALPHA = 0x80000000,
         TEXTUREFLAGS_LAST = 0x20000000,
         TEXTUREFLAGS_COUNT = 30
      }

      public enum VTFCubeMapFace {
         CUBEMAP_FACE_RIGHT = 0,
         CUBEMAP_FACE_LEFT,
         CUBEMAP_FACE_BACK,
         CUBEMAP_FACE_FRONT,
         CUBEMAP_FACE_UP,
         CUBEMAP_FACE_DOWN,
         CUBEMAP_FACE_SPHERE_MAP,
         CUBEMAP_FACE_COUNT
      }
      public enum VTFMipmapFilter {
         MIPMAP_FILTER_POINT = 0,
         MIPMAP_FILTER_BOX,
         MIPMAP_FILTER_TRIANGLE,
         MIPMAP_FILTER_QUADRATIC,
         MIPMAP_FILTER_CUBIC,
         MIPMAP_FILTER_CATROM,
         MIPMAP_FILTER_MITCHELL,
         MIPMAP_FILTER_GAUSSIAN,
         MIPMAP_FILTER_SINC,
         MIPMAP_FILTER_BESSEL,
         MIPMAP_FILTER_HANNING,
         MIPMAP_FILTER_HAMMING,
         MIPMAP_FILTER_BLACKMAN,
         MIPMAP_FILTER_KAISER,
         MIPMAP_FILTER_COUNT
      }
      public enum VTFSharpenFilter {
         SHARPEN_FILTER_NONE = 0,
         SHARPEN_FILTER_NEGATIVE,
         SHARPEN_FILTER_LIGHTER,
         SHARPEN_FILTER_DARKER,
         SHARPEN_FILTER_CONTRASTMORE,
         SHARPEN_FILTER_CONTRASTLESS,
         SHARPEN_FILTER_SMOOTHEN,
         SHARPEN_FILTER_SHARPENSOFT,
         SHARPEN_FILTER_SHARPENMEDIUM,
         SHARPEN_FILTER_SHARPENSTRONG,
         SHARPEN_FILTER_FINDEDGES,
         SHARPEN_FILTER_CONTOUR,
         SHARPEN_FILTER_EDGEDETECT,
         SHARPEN_FILTER_EDGEDETECTSOFT,
         SHARPEN_FILTER_EMBOSS,
         SHARPEN_FILTER_MEANREMOVAL,
         SHARPEN_FILTER_UNSHARP,
         SHARPEN_FILTER_XSHARPEN,
         SHARPEN_FILTER_WARPSHARP,
         SHARPEN_FILTER_COUNT
      }
      public enum VTFDXTQuality {
         DXT_QUALITY_LOW = 0,
         DXT_QUALITY_MEDIUM,
         DXT_QUALITY_HIGH,
         DXT_QUALITY_HIGHEST,
         DXT_QUALITY_COUNT
      }
      public enum VTFKernelFilter {
         KERNEL_FILTER_4X = 0,
         KERNEL_FILTER_3X3,
         KERNEL_FILTER_5X5,
         KERNEL_FILTER_7X7,
         KERNEL_FILTER_9X9,
         KERNEL_FILTER_DUDV,
         KERNEL_FILTER_COUNT
      }
      public enum VTFHeightConversionMethod {
         HEIGHT_CONVERSION_METHOD_ALPHA = 0,
         HEIGHT_CONVERSION_METHOD_AVERAGE_RGB,
         HEIGHT_CONVERSION_METHOD_BIASED_RGB,
         HEIGHT_CONVERSION_METHOD_RED,
         HEIGHT_CONVERSION_METHOD_GREEN,
         HEIGHT_CONVERSION_METHOD_BLUE,
         HEIGHT_CONVERSION_METHOD_MAX_RGB,
         HEIGHT_CONVERSION_METHOD_COLORSPACE,
         HEIGHT_CONVERSION_METHOD_COUNT
      }
      public enum VTFNormalAlphaResult {
         NORMAL_ALPHA_RESULT_NOCHANGE = 0,
         NORMAL_ALPHA_RESULT_HEIGHT,
         NORMAL_ALPHA_RESULT_BLACK,
         NORMAL_ALPHA_RESULT_WHITE,
         NORMAL_ALPHA_RESULT_COUNT
      }
      public enum VTFResizeMethod {
         RESIZE_NEAREST_POWER2 = 0,
         RESIZE_BIGGEST_POWER2,
         RESIZE_SMALLEST_POWER2,
         RESIZE_SET,
         RESIZE_COUNT
      }
      public enum VLProc {
         PROC_READ_CLOSE = 0,
         PROC_READ_OPEN,
         PROC_READ_READ,
         PROC_READ_SEEK,
         PROC_READ_TELL,
         PROC_READ_SIZE,
         PROC_WRITE_CLOSE,
         PROC_WRITE_OPEN,
         PROC_WRITE_WRITE,
         PROC_WRITE_SEEK,
         PROC_WRITE_SIZE,
         PROC_WRITE_TELL,
         PROC_COUNT
      }
      public enum VLSeekMode {
         SEEK_MODE_BEGIN = 0,
         SEEK_MODE_CURRENT,
         SEEK_MODE_END
      }
      public enum VMTParseMode {
         PARSE_MODE_STRICT = 0,
         PARSE_MODE_LOOSE,
         PARSE_MODE_COUNT
      }
      public enum VMTNodeType {
         NODE_TYPE_GROUP = 0,
         NODE_TYPE_GROUP_END,
         NODE_TYPE_STRING,
         NODE_TYPE_INTEGER,
         NODE_TYPE_SINGLE,
         NODE_TYPE_COUNT
      }

      // === Structures ===

      [StructLayout(LayoutKind.Sequential)]
      public struct SVTFImageFormatInfo {
         public IntPtr lpName; // char*
         public uint uiBitsPerPixel;
         public uint uiBytesPerPixel;
         public uint uiRedBitsPerPixel;
         public uint uiGreenBitsPerPixel;
         public uint uiBlueBitsPerPixel;
         public uint uiAlphaBitsPerPixel;
         [MarshalAs(UnmanagedType.U1)]
         public bool bIsCompressed;
         [MarshalAs(UnmanagedType.U1)]
         public bool bIsSupported;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct SVTFCreateOptions {
         [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
         public uint[] uiVersion;
         public VTFImageFormat ImageFormat;
         public uint uiFlags;
         public uint uiStartFrame;
         public float sBumpScale;
         [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
         public float[] sReflectivity;
         [MarshalAs(UnmanagedType.U1)]
         public bool bMipmaps;
         public VTFMipmapFilter MipmapFilter;
         public VTFSharpenFilter MipmapSharpenFilter;
         [MarshalAs(UnmanagedType.U1)]
         public bool bThumbnail;
         [MarshalAs(UnmanagedType.U1)]
         public bool bReflectivity;
         [MarshalAs(UnmanagedType.U1)]
         public bool bResize;
         public VTFResizeMethod ResizeMethod;
         public VTFMipmapFilter ResizeFilter;
         public VTFSharpenFilter ResizeSharpenFilter;
         public uint uiResizeWidth;
         public uint uiResizeHeight;
         [MarshalAs(UnmanagedType.U1)]
         public bool bResizeClamp;
         public uint uiResizeClampWidth;
         public uint uiResizeClampHeight;
         [MarshalAs(UnmanagedType.U1)]
         public bool bGammaCorrection;
         public float sGammaCorrection;
         [MarshalAs(UnmanagedType.U1)]
         public bool bNormalMap;
         public VTFKernelFilter KernelFilter;
         public VTFHeightConversionMethod HeightConversionMethod;
         public VTFNormalAlphaResult NormalAlphaResult;
         public byte bNormalMinimumZ;
         public float sNormalScale;
         [MarshalAs(UnmanagedType.U1)]
         public bool bNormalWrap;
         [MarshalAs(UnmanagedType.U1)]
         public bool bNormalInvertX;
         [MarshalAs(UnmanagedType.U1)]
         public bool bNormalInvertY;
         [MarshalAs(UnmanagedType.U1)]
         public bool bNormalInvertZ;
         [MarshalAs(UnmanagedType.U1)]
         public bool bSphereMap;
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      public struct SVTFTextureLODControlResource {
         public byte ResolutionClampU;
         public byte ResolutionClampV;
         [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
         public byte[] Padding;
      }

      // === Helper Methods ===

      static VTFLib() {
         // default values for arrays in SVTFCreateOptions
         DefaultCreateOptions.uiVersion = [7, 5];
         DefaultCreateOptions.sReflectivity = [0f, 0f, 0f];
      }

      public static SVTFCreateOptions DefaultCreateOptions = new SVTFCreateOptions();

      // === Core Library Functions ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlGetVersion();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
      public static extern IntPtr vlGetVersionString(); // Returns const char*

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
      public static extern IntPtr vlGetLastError(); // Returns const char*

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlInitialize();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlShutdown();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlGetBoolean(VTFLibOption Option);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlSetBoolean(VTFLibOption Option, [MarshalAs(UnmanagedType.U1)] bool bValue);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern int vlGetInteger(VTFLibOption Option);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlSetInteger(VTFLibOption Option, int iValue);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern float vlGetFloat(VTFLibOption Option);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlSetFloat(VTFLibOption Option, float sValue);

      // === Proc System (for custom I/O) ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlSetProc(VLProc Proc, IntPtr pProc);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern IntPtr vlGetProc(VLProc Proc);

      // === Image Management ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageIsBound();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlBindImage(uint uiImage);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlCreateImage(out uint uiImage);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlDeleteImage(uint uiImage);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageCreateDefaultCreateStructure(ref SVTFCreateOptions VTFCreateOptions);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageCreate(
          uint uiWidth, uint uiHeight, uint uiFrames, uint uiFaces, uint uiSlices,
          VTFImageFormat ImageFormat, [MarshalAs(UnmanagedType.U1)] bool bThumbnail,
          [MarshalAs(UnmanagedType.U1)] bool bMipmaps, [MarshalAs(UnmanagedType.U1)] bool bNullImageData);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageCreateSingle(
          uint uiWidth, uint uiHeight, IntPtr lpImageDataRGBA8888,
          ref SVTFCreateOptions VTFCreateOptions);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageCreateMultiple(
          uint uiWidth, uint uiHeight, uint uiFrames, uint uiFaces, uint uiSlices,
          IntPtr lpImageDataRGBA8888, ref SVTFCreateOptions VTFCreateOptions);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageDestroy();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageIsLoaded();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageLoad(string cFileName, [MarshalAs(UnmanagedType.U1)] bool bHeaderOnly);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageLoadLump(IntPtr lpData, uint uiBufferSize, [MarshalAs(UnmanagedType.U1)] bool bHeaderOnly);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageLoadProc(IntPtr pUserData, [MarshalAs(UnmanagedType.U1)] bool bHeaderOnly);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageSave(string cFileName);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageSaveLump(IntPtr lpData, uint uiBufferSize, out uint uiSize);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageSaveProc(IntPtr pUserData);

      // === Image Query Functions ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetHasImage();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetMajorVersion();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetMinorVersion();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetSize();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetWidth();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetHeight();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetDepth();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetFrameCount();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetFaceCount();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetMipmapCount();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetStartFrame();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageSetStartFrame(uint uiStartFrame);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetFlags();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageSetFlags(uint uiFlags);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGetFlag(VTFImageFlag ImageFlag);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageSetFlag(VTFImageFlag ImageFlag, [MarshalAs(UnmanagedType.U1)] bool bState);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern float vlImageGetBumpmapScale();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageSetBumpmapScale(float sBumpmapScale);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageGetReflectivity(out float sX, out float sY, out float sZ);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageSetReflectivity(float sX, float sY, float sZ);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern VTFImageFormat vlImageGetFormat();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern IntPtr vlImageGetData(uint uiFrame, uint uiFace, uint uiSlice, uint uiMipmapLevel);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageSetData(uint uiFrame, uint uiFace, uint uiSlice, uint uiMipmapLevel, IntPtr lpData);

      // === Thumbnail Functions ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGetHasThumbnail();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetThumbnailWidth();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetThumbnailHeight();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern VTFImageFormat vlImageGetThumbnailFormat();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern IntPtr vlImageGetThumbnailData();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageSetThumbnailData(IntPtr lpData);

      // === Resource Functions ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGetSupportsResources();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetResourceCount();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageGetResourceType(uint uiIndex);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGetHasResource(uint uiType);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern IntPtr vlImageGetResourceData(uint uiType, out uint uiSize);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern IntPtr vlImageSetResourceData(uint uiType, uint uiSize, IntPtr lpData);

      // === Helper Functions ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGenerateMipmaps(
          uint uiFace, uint uiFrame,
          VTFMipmapFilter MipmapFilter,
          VTFSharpenFilter SharpenFilter);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGenerateAllMipmaps(
          VTFMipmapFilter MipmapFilter,
          VTFSharpenFilter SharpenFilter);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGenerateThumbnail();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGenerateNormalMap(
          uint uiFrame,
          VTFKernelFilter KernelFilter,
          VTFHeightConversionMethod HeightConversionMethod,
          VTFNormalAlphaResult NormalAlphaResult);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGenerateAllNormalMaps(
          VTFKernelFilter KernelFilter,
          VTFHeightConversionMethod HeightConversionMethod,
          VTFNormalAlphaResult NormalAlphaResult);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGenerateSphereMap();

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageComputeReflectivity();

      // === Conversion Functions ===

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern IntPtr vlImageGetImageFormatInfo(VTFImageFormat ImageFormat); // returns SVTFImageFormatInfo*

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageGetImageFormatInfoEx(VTFImageFormat ImageFormat, ref SVTFImageFormatInfo VTFImageFormatInfo);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageComputeImageSize(
          uint uiWidth, uint uiHeight, uint uiDepth,
          uint uiMipmaps, VTFImageFormat ImageFormat);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageComputeMipmapCount(
          uint uiWidth, uint uiHeight, uint uiDepth);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageComputeMipmapDimensions(
          uint uiWidth, uint uiHeight, uint uiDepth,
          uint uiMipmapLevel,
          out uint uiMipmapWidth, out uint uiMipmapHeight, out uint uiMipmapDepth);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern uint vlImageComputeMipmapSize(
          uint uiWidth, uint uiHeight, uint uiDepth,
          uint uiMipmapLevel, VTFImageFormat ImageFormat);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageConvertToRGBA8888(
          IntPtr lpSource, IntPtr lpDest,
          uint uiWidth, uint uiHeight, VTFImageFormat SourceFormat);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageConvertFromRGBA8888(
          IntPtr lpSource, IntPtr lpDest,
          uint uiWidth, uint uiHeight, VTFImageFormat DestFormat);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageConvert(
          IntPtr lpSource, IntPtr lpDest,
          uint uiWidth, uint uiHeight,
          VTFImageFormat SourceFormat, VTFImageFormat DestFormat);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageConvertToNormalMap(
          IntPtr lpSourceRGBA8888, IntPtr lpDestRGBA8888,
          uint uiWidth, uint uiHeight,
          VTFKernelFilter KernelFilter,
          VTFHeightConversionMethod HeightConversionMethod,
          VTFNormalAlphaResult NormalAlphaResult,
          byte bMinimumZ, float sScale, [MarshalAs(UnmanagedType.U1)] bool bWrap,
          [MarshalAs(UnmanagedType.U1)] bool bInvertX, [MarshalAs(UnmanagedType.U1)] bool bInvertY);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.U1)]
      public static extern bool vlImageResize(
          IntPtr lpSourceRGBA8888, IntPtr lpDestRGBA8888,
          uint uiSourceWidth, uint uiSourceHeight,
          uint uiDestWidth, uint uiDestHeight,
          VTFMipmapFilter ResizeFilter,
          VTFSharpenFilter SharpenFilter);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageCorrectImageGamma(
          IntPtr lpImageDataRGBA8888,
          uint uiWidth, uint uiHeight,
          float sGammaCorrection);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageComputeImageReflectivity(
          IntPtr lpImageDataRGBA8888,
          uint uiWidth, uint uiHeight,
          out float sX, out float sY, out float sZ);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageFlipImage(
          IntPtr lpImageDataRGBA8888,
          uint uiWidth, uint uiHeight);

      [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
      public static extern void vlImageMirrorImage(
          IntPtr lpImageDataRGBA8888,
          uint uiWidth, uint uiHeight);
   }
}