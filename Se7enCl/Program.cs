using System;
using System.Runtime.InteropServices;

namespace Se7enCl
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Program : IRefCountedHandle
    {
        public readonly IntPtr Handle;

        internal Program(IntPtr handle) 
            => Handle = handle;

        public readonly bool ReferenceCount => Utils.GetTInfo<ProgramInfo, bool>(Handle, ProgramInfo.ReferenceCount, OpenCl.GetProgramInfo);
        public readonly Context Context => Utils.GetTInfo<ProgramInfo, Context>(Handle, ProgramInfo.Context, OpenCl.GetProgramInfo);
        public readonly uint NumDevices => Utils.GetTInfo<ProgramInfo, uint>(Handle, ProgramInfo.NumDevices, OpenCl.GetProgramInfo);
        public readonly Device[] Devices => Utils.GetTInfo<ProgramInfo, Device>(Handle, ProgramInfo.Devices, OpenCl.GetProgramInfo, out _);
        public readonly string Source => Utils.GetTInfo<ProgramInfo, byte>(Handle, ProgramInfo.Source, OpenCl.GetProgramInfo, out _).ToStrg();
        public readonly uint[] BinarySizes => Utils.GetTInfo<ProgramInfo, uint>(Handle, ProgramInfo.BinarySizes, OpenCl.GetProgramInfo, out _);
        public readonly byte[] Binaries => Utils.GetTInfo<ProgramInfo, byte>(Handle, ProgramInfo.Binaries, OpenCl.GetProgramInfo, out _);
        public readonly uint NumKernels => Utils.GetTInfo<ProgramInfo, uint>(Handle, ProgramInfo.NumKernels, OpenCl.GetProgramInfo);
        public readonly string[] KernelNames => Utils.GetTInfo<ProgramInfo, byte>(Handle, ProgramInfo.KernelNames, OpenCl.GetProgramInfo, out _ ).ToStrg().Split(';');
        public readonly string IL => Utils.GetTInfo<ProgramInfo, byte>(Handle, ProgramInfo.IL, OpenCl.GetProgramInfo, out _).ToStrg();
        public readonly bool ScopeGlobalCtorsPresent => Utils.GetTInfo<ProgramInfo, bool>(Handle, ProgramInfo.ScopeGlobalCtorsPresent, OpenCl.GetProgramInfo);
        public readonly bool ScopeGlobalDtorsPresent => Utils.GetTInfo<ProgramInfo, bool>(Handle, ProgramInfo.ScopeGlobalDtorsPresent, OpenCl.GetProgramInfo);

        #region IRefCountedHandle Members

        public ErrorCode Retain() 
            => OpenCl.RetainProgram(Handle);

        public ErrorCode Release() 
            => OpenCl.ReleaseProgram(Handle);

        #endregion

        #region IDisposable Members

        public void Dispose() 
            => Release();

        #endregion

        public static implicit operator IntPtr(Program program) 
            => program.Handle;
    }
}