using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    public unsafe static partial class OpenCl
    {

        [DllImport(Library, EntryPoint = "clGetDeviceIDs")]
        public static extern ErrorCode GetDeviceIDs(IntPtr platform,
                                                      DeviceType deviceType,
                                                      uint numEntries,
                                                      [Out] [MarshalAs(UnmanagedType.LPArray)] Device[] devices,
                                                      out uint numDevices);

        [DllImport(Library, EntryPoint = "clGetDeviceInfo")]
        public static extern ErrorCode GetDeviceInfo(IntPtr device,
                                                       DeviceInfo paramName,
                                                       uint paramValueSize,
                                                       void* paramValue,
                                                       out uint paramValueSizeRet);

        [DllImport(Library, EntryPoint = "clReleaseDevice")]
        public static extern ErrorCode ReleaseDevice(IntPtr device);

        [DllImport(Library, EntryPoint = "clRetainDevice")]
        public static extern ErrorCode RetainDevice(IntPtr device);

    }
}


/**
 * \[DllImport\(Library\)\]\s*\n\s+((?:[^\s]+\s){4})(cl([^(]+))
 * [DllImport(Library, EntryPoint = "$2")]\n$1$3
 **/
