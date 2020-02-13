using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct CommandQueue : IRefCountedHandle
    {
        public readonly IntPtr Handle;

        internal CommandQueue(IntPtr handle)
            => Handle = handle;

        public Context Context 
            => Utils.GetTInfo<CommandQueueInfo, Context>(Handle, CommandQueueInfo.Context, OpenCl.GetCommandQueueInfo);
        public Device Device
            => Utils.GetTInfo<CommandQueueInfo, Device>(Handle, CommandQueueInfo.Device, OpenCl.GetCommandQueueInfo);
        public uint ReferenceCount
            => Utils.GetTInfo<CommandQueueInfo, uint>(Handle, CommandQueueInfo.ReferenceCount, OpenCl.GetCommandQueueInfo);
        public CommandQueueProperties Properties
            => Utils.GetTInfo<CommandQueueInfo, CommandQueueProperties>(Handle, CommandQueueInfo.Properties, OpenCl.GetCommandQueueInfo);
        public uint Size
            => Utils.GetTInfo<CommandQueueInfo, uint>(Handle, CommandQueueInfo.Size, OpenCl.GetCommandQueueInfo);





        #region IRefCountedHandle Members

        public ErrorCode Retain()
           => OpenCl.RetainCommandQueue(Handle);

        public ErrorCode Release()
            => OpenCl.ReleaseCommandQueue(Handle);

        #endregion

        #region IDisposable Members

        public void Dispose()
            => Release();

        #endregion

        public static implicit operator IntPtr(CommandQueue queue)
            => queue.Handle;
    }
}
