using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Mem : IRefCountedHandle
    {
        public readonly IntPtr Handle;

        internal Mem(IntPtr handle)
            => Handle = handle;

        public readonly MemObjectType Type => Utils.GetTInfo<MemInfo, MemObjectType>(Handle, MemInfo.Type, OpenCl.GetMemObjectInfo);
        public readonly MemFlags Flags => Utils.GetTInfo<MemInfo, MemFlags>(Handle, MemInfo.Flags, OpenCl.GetMemObjectInfo);
        public readonly uint Size => Utils.GetTInfo<MemInfo, uint>(Handle, MemInfo.Size, OpenCl.GetMemObjectInfo);
        public readonly IntPtr HostPtr => Utils.GetTInfo<MemInfo, IntPtr>(Handle, MemInfo.HostPtr, OpenCl.GetMemObjectInfo);
        public readonly uint MapCount => Utils.GetTInfo<MemInfo, uint>(Handle, MemInfo.MapCount, OpenCl.GetMemObjectInfo);
        public readonly uint ReferenceCount => Utils.GetTInfo<MemInfo, uint>(Handle, MemInfo.ReferenceCount, OpenCl.GetMemObjectInfo);
        public readonly Context Context => Utils.GetTInfo<MemInfo, Context>(Handle, MemInfo.Context, OpenCl.GetMemObjectInfo);
        public readonly IntPtr AssociatedMemObject => Utils.GetTInfo<MemInfo, IntPtr>(Handle, MemInfo.Context, OpenCl.GetMemObjectInfo);
        public readonly uint Offset => Utils.GetTInfo<MemInfo, uint>(Handle, MemInfo.Offset, OpenCl.GetMemObjectInfo);
        public readonly IntPtr UsesSvmPointer => Utils.GetTInfo<MemInfo, IntPtr>(Handle, MemInfo.UsesSvmPointer, OpenCl.GetMemObjectInfo);

        #region IRefCountedHandle Members

        public ErrorCode Retain()
            => OpenCl.RetainMemObject(this);

        public ErrorCode Release()
            => OpenCl.ReleaseMemObject(this);

        #endregion

        #region IDisposable Members
        public void Dispose()
            => Release();

        #endregion

        public static implicit operator IntPtr(Mem memObj)
            => memObj.Handle;
    }

}
