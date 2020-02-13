using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    public unsafe static partial class OpenCl
    {

        [DllImport(Library, EntryPoint = "clGetPlatformIDs")]
        public static extern ErrorCode GetPlatformIDs(uint numEntries,
                                                         [Out] [MarshalAs(UnmanagedType.LPArray)] Platform[] platforms,
                                                         out uint numPlatforms);
        [DllImport(Library, EntryPoint = "clGetPlatformInfo")]
        public static extern ErrorCode GetPlatformInfo(IntPtr platform,
                                                PlatformInfo paramName,
                                                uint paramValueSize,
                                                void* paramValue,
                                                out uint paramValueSizeRet);
    }
}


/**
 * \[DllImport\(Library\)\]\s*\n\s+((?:[^\s]+\s){4})(cl([^(]+))
 * [DllImport(Library, EntryPoint = "$2")]\n$1$3
 **/
