using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Kernel : IRefCountedHandle
    {
        public readonly IntPtr Handle;

        internal Kernel(IntPtr handle)
            => Handle = handle;

        public readonly string FunctionName => Utils.GetTInfo<KernelInfo, byte>(Handle, KernelInfo.FunctionName, OpenCl.GetKernelInfo, out _).ToStrg();
        public readonly int NumArgs => Utils.GetTInfo<KernelInfo, int>(Handle, KernelInfo.NumArgs, OpenCl.GetKernelInfo);
        public readonly uint ReferenceCount => Utils.GetTInfo<KernelInfo, uint>(Handle, KernelInfo.ReferenceCount, OpenCl.GetKernelInfo);
        public readonly Context Context => Utils.GetTInfo<KernelInfo, Context>(Handle, KernelInfo.Context, OpenCl.GetKernelInfo);
        public readonly Program Program => Utils.GetTInfo<KernelInfo, Program>(Handle, KernelInfo.Program, OpenCl.GetKernelInfo);
        public readonly byte[] Attributes => Utils.GetTInfo<KernelInfo, byte>(Handle, KernelInfo.Attributes, OpenCl.GetKernelInfo, out _);
        public readonly uint MaxNumSubGroups => Utils.GetTInfo<KernelInfo, uint>(Handle, KernelInfo.MaxNumSubGroups, OpenCl.GetKernelInfo);
        public readonly uint CompileNumSubGroups => Utils.GetTInfo<KernelInfo, uint>(Handle, KernelInfo.CompileNumSubGroups, OpenCl.GetKernelInfo);


        #region IRefCountedHandle Members

        public ErrorCode Retain() 
            => OpenCl.RetainKernel(this);

        public ErrorCode Release() 
            => OpenCl.ReleaseKernel(this);

        #endregion

        #region IDisposable Members

        public void Dispose() 
            => Release();

        #endregion


        public static implicit operator IntPtr(Kernel kernel) => kernel.Handle;
    }
}
