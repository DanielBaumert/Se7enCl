using Se7en.OpenCl.Api.Enum;
using System;
using System.Runtime.InteropServices;

namespace Se7en.OpenCl.Api.Native
{
    public unsafe static partial class Cl
    {
        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clCreateKernel")]
        public static extern IntPtr CreateKernel(IntPtr program,
                                                   [In] [MarshalAs(UnmanagedType.LPStr)] string kernelName,
                                                   [Out] [MarshalAs(UnmanagedType.I4)] out ErrorCode errcodeRet);

        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clCreateKernelsInProgram")]
        public static extern ErrorCode CreateKernelsInProgram(IntPtr program,
                                                               uint numKernels,
                                                               [Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysUInt, SizeParamIndex = 1)] IntPtr[] kernels,
                                                               out uint numKernelsRet);
        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clCreateKernelsInProgram")]
        public static extern ErrorCode CreateKernelsInProgram(IntPtr program,
                                                            uint numKernels,
                                                            [Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysUInt, SizeParamIndex = 1)] Kernel[] kernels,
                                                            out uint numKernelsRet);

        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clRetainKernel")]
        public static extern ErrorCode RetainKernel(IntPtr kernel);
        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clReleaseKernel")]
        public static extern ErrorCode ReleaseKernel(IntPtr kernel);

        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clSetKernelArg")]
        public static extern ErrorCode SetKernelArg(IntPtr kernel, uint argIndex, int argSize, IntPtr argValue);
        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clGetKernelInfo")]
        public static extern ErrorCode GetKernelInfo(IntPtr kernel,
                                                        KernelInfo paramName,
                                                        uint paramValueSize,
                                                        void* paramValue,
                                                        out uint paramValueSizeRet);
        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clGetKernelInfo")]
        public static extern ErrorCode GetKernelInfo(IntPtr kernel,
                                                        KernelInfo paramName,
                                                        uint paramValueSize,
                                                        IntPtr paramValue,
                                                        out uint paramValueSizeRet);
        /// <summary>
        /// To enqueue a command to execute a kernel on a device
        /// </summary>
        /// <param name="commandQueue">
        /// command_queue is a valid host command-queue.<br/>
        /// The kernel will be queued for execution on the device associated with command_queue.
        /// </param>
        /// <param name="kernel">
        /// kernel is a valid kernel object.<br/>
        /// The OpenCL context associated with kernel and command-queue must be the same
        /// </param>
        /// <param name="workDim">
        /// work_dim is the number of dimensions used to specify the global work-items and work-items in the work-group.<br/>
        /// work_dim must be greater than zero and less than or equal to CL_​DEVICE_​MAX_​WORK_​ITEM_​DIMENSIONS.<br/>
        /// If global_work_size is NULL, or the value in any passed dimension is 0 then the kernel command will trivially succeed after its event dependencies<br/>
        /// are satisfied and subsequently update its completion event.<br/>
        /// The behavior in this situation is similar to that of an enqueued marker, except that unlike a marker,<br/>
        /// an enqueued kernel with no events passed to event_wait_list may run at any time.
        /// </param>
        /// <param name="globalWorkOffset">
        /// global_work_offset can be used to specify an array of work_dim unsigned<br/>
        /// values that describe the offset used to calculate the global ID of a work-item.<br/>
        /// If global_work_offset is NULL, the global IDs start at offset (0, 0, 0).</param>
        /// <param name="globalWorkSize">
        /// global_work_size points to an array of work_dim unsigned values that describe the number of global work-items in work_dim dimensions<br/>
        /// that will execute the kernel function.<br/>
        /// The total number of global work-items is computed as global_work_size[0] × …​ × global_work_size[work_dim - 1].</param>
        /// <param name="localWorkSize">
        /// local_work_size points to an array of work_dim unsigned values that describe the number of work-items<br/>
        /// that make up a work-group (also referred to as the size of the work-group) that will execute the kernel specified by kernel.<br/>
        /// The total number of work-items in a work-group is computed as local_work_size[0] × …​ × local_work_size[work_dim - 1].<br/>
        /// The total number of work-items in the work-group must be less than or equal to the CL_​KERNEL_​WORK_​GROUP_​SIZE<br/>
        /// value specified in the Kernel Object Device Queries table,<br/>
        /// and the number of work-items specified in local_work_size[0], …​, local_work_size[work_dim - 1]<br/>
        /// must be less than or equal to the corresponding values specified by CL_​DEVICE_​MAX_​WORK_​ITEM_​SIZES[0], …​, CL_​DEVICE_​MAX_​WORK_​ITEM_​SIZES[work_dim - 1].<br/>
        /// The explicitly specified local_work_size will be used to determine how to break the global<br/> 
        /// work-items specified by global_work_size into appropriate work-group instances.
        /// </param>
        /// <param name="numEventsInWaitList">
        /// event_wait_list and num_events_in_wait_list specify events that need to complete before this particular command can be executed.<br/>
        /// If event_wait_list is NULL, then this particular command does not wait on any event to complete.<br/>
        /// If event_wait_list is NULL, num_events_in_wait_list must be 0. If event_wait_list is not NULL,<br/>
        /// the list of events pointed to by event_wait_list must be valid and num_events_in_wait_list must be greater than 0.<br/>
        /// The events specified in event_wait_list act as synchronization points.<br/>
        /// The context associated with events in event_wait_list and command_queue must be the same.<br/>
        /// The memory associated with event_wait_list can be reused or freed after the function returns.
        /// </param>
        /// <param name="eventWaitList">
        /// event_wait_list and num_events_in_wait_list specify events that need to complete before this particular command can be executed.<br/>
        /// If event_wait_list is NULL, then this particular command does not wait on any event to complete.<br/>
        /// If event_wait_list is NULL, num_events_in_wait_list must be 0. If event_wait_list is not NULL,<br/>
        /// the list of events pointed to by event_wait_list must be valid and num_events_in_wait_list must be greater than 0.<br/>
        /// The events specified in event_wait_list act as synchronization points.<br/>
        /// The context associated with events in event_wait_list and command_queue must be the same.<br/>
        /// The memory associated with event_wait_list can be reused or freed after the function returns.
        /// </param>
        /// <param name="e">
        /// event returns an event object that identifies this particular kernel-instance.<br/>
        /// Event objects are unique and can be used to identify a particular kernel-instance later on.<br/>
        /// If event is NULL, no event will be created for this kernel-instance and therefore it will not be possible for<br/>
        /// the application to query or queue a wait for this particular kernel-instance.<br/>
        /// If the event_wait_list and the event arguments are not NULL, the event argument should not refer to an element of the event_wait_list array.
        /// </param>
        /// <returns></returns>
        [DllImport(InternalLibLoader.OpenCL, EntryPoint = "clEnqueueNDRangeKernel")]
        public static extern ErrorCode EnqueueNDRangeKernel(IntPtr commandQueue,
                                                               IntPtr kernel,
                                                               uint workDim,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] globalWorkOffset,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] globalWorkSize,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] localWorkSize,
                                                               uint numEventsInWaitList,
                                                               [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysUInt, SizeParamIndex = 6)] Event[] eventWaitList,
                                                               [Out] [MarshalAs(UnmanagedType.Struct)] out Event e);
    }
}


/*
 * \[DllImport\(Library\)\]\s*\n\s+((?:[^\s]+\s){4})(cl([^(]+))
 * [DllImport(Library, EntryPoint = "$2")]\n$1$3
 */
