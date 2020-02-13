﻿using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    public unsafe static partial class OpenCl
    {
        [DllImport(Library, EntryPoint = "clCreateCommandQueue")]
        public static extern IntPtr CreateCommandQueue(IntPtr context, IntPtr device,
                                                        [MarshalAs(UnmanagedType.U8)] CommandQueueProperties properties,
                                                        [Out] [MarshalAs(UnmanagedType.I4)] out ErrorCode error);

        [DllImport(Library, EntryPoint = "clGetCommandQueueInfo")]
        public static extern ErrorCode GetCommandQueueInfo(IntPtr commandQueue,
                                                             [MarshalAs(UnmanagedType.U4)] CommandQueueInfo paramName,
                                                             uint paramValueSize,
                                                             void* paramValue,
                                                             out uint paramValueSizeRet);


        [DllImport(Library, EntryPoint = "clRetainCommandQueue")]
        public static extern ErrorCode RetainCommandQueue(IntPtr commandQueue);

        [DllImport(Library, EntryPoint = "clReleaseCommandQueue")]
        public static extern ErrorCode ReleaseCommandQueue(IntPtr commandQueue);


        [DllImport(Library, EntryPoint = "clFinish")]
        public static extern ErrorCode Finish(IntPtr commandQueue);
    }
}


/**
 * \[DllImport\(Library\)\]\s*\n\s+((?:[^\s]+\s){4})(cl([^(]+))
 * [DllImport(Library, EntryPoint = "$2")]\n$1$3
 **/
