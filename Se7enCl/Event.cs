using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Event : IRefCountedHandle
    {
        public readonly IntPtr Handle;

        internal Event(IntPtr handle)
            => Handle = handle;

        public CommandExecutionStatus CommandExecutionStatus => Utils.GetTInfo<EventInfo, CommandExecutionStatus>(Handle, EventInfo.CommandExecutionStatus, OpenCl.GetEventInfo);
        public CommandQueue CommandQueue => Utils.GetTInfo<EventInfo, CommandQueue>(Handle, EventInfo.CommandQueue, OpenCl.GetEventInfo);
        public CommandType CommandType => Utils.GetTInfo<EventInfo, CommandType>(Handle, EventInfo.CommandType, OpenCl.GetEventInfo);
        public Context Context => Utils.GetTInfo<EventInfo, Context>(Handle, EventInfo.Context, OpenCl.GetEventInfo);
        public uint ReferenceCount => Utils.GetTInfo<EventInfo, uint>(Handle, EventInfo.ReferenceCount, OpenCl.GetEventInfo);

        public void WaitForComplete() => OpenCl.WaitForEvents(1, new Event []{ this });

        #region IRefCountedHandle Members

        public ErrorCode Retain()
            => OpenCl.RetainEvent(Handle);

        public ErrorCode Release()
            => OpenCl.ReleaseEvent(Handle);

        #endregion

        #region IDisposable Members

        public void Dispose()
            => Release();

        #endregion
    }
}