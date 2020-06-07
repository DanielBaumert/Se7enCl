using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Se7en.OpenCl.Api.Enum;
using Se7en.OpenCl.Api.Native;

namespace Se7en.OpenCl
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GPUBuffer
    {
        public Mem MemoryObject;
        public long Length;
        public MemFlags MemoryFlags;
    }

    /// <summary>
    /// Hold the GPU program ready to run
    /// </summary>
    public readonly unsafe struct OpenClBridge : IDisposable
    {
        private readonly GPUBuffer[] _gpuBuffers;

        public readonly Context Context;
        public readonly Kernel Kernel;
        public readonly Device Device;
        public readonly int Arguments;
        public readonly CommandQueue CommandQueue;
        


        internal OpenClBridge(Context ctx, Device device, Kernel kernel)
        {
            Context = ctx;
            Kernel = kernel;
            Device = device;

            CommandQueue = new CommandQueue(Cl.CreateCommandQueue(ctx, device, CommandQueueProperties.None, out _));
            Arguments = Kernel.NumArgs;
            _gpuBuffers = new GPUBuffer[Arguments];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArgumentValue<T>(int argumentIndex, T[] source, MemFlags flags)
            where T : unmanaged
        {
            fixed (T* sourcePtr = source)
            {
                SetArgumentValue(argumentIndex, sourcePtr, flags, sizeof(T) * source.Length);
                
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArgumentValue<T>(int argumentIndex, T[] source, MemFlags flags, long count)
            where T : unmanaged
        {
            fixed (T* sourcePtr = source)
            {
                SetArgumentValue(argumentIndex, sourcePtr, flags, sizeof(T) * count);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArgumentValue<T>(int argumentIndex, T[] source, MemFlags flags, int offset, long count)
            where T : unmanaged
        {
            fixed (T* sourcePtr = source)
            {
                SetArgumentValue(argumentIndex, sourcePtr + offset, flags, sizeof(T) * count);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArgumentValue<T>(int argumentIndex, T* item, MemFlags flags, long count)
            where T : unmanaged
        {
            SetArgumentValue(argumentIndex, item, flags, sizeof(T) * count);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArgumentValue(int argumentIndex, void* item, MemFlags flags, long byteCount)
        {
            fixed (GPUBuffer* gpuBufferPtr = _gpuBuffers)
            {
                ErrorCode err;
                GPUBuffer buffer = *(gpuBufferPtr + argumentIndex);

                if (buffer.MemoryObject == IntPtr.Zero)
                {
                    buffer.MemoryObject.Dispose();
                }
                if (buffer.Length != byteCount)
                {
                    buffer.Length = byteCount;
                }

                buffer.MemoryObject = Cl.CreateBuffer(Context, flags, byteCount, (IntPtr)item, out err);
                err = Cl.SetKernelArg(Kernel, (uint)argumentIndex, IntPtr.Size, buffer.MemoryObject);
                if ((IntPtr)item != IntPtr.Zero)
                {
                    Cl.EnqueueWriteBuffer(CommandQueue, buffer.MemoryObject, 1, IntPtr.Zero, byteCount, (IntPtr)item, 0, null, out Event @event);
                    @event.WaitForComplete();
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSvmArgs(params SvmPointer[] args)
        {
            ErrorCode err;
            int count = args.Length;
            fixed (SvmPointer* argsPtr = args)
            {
                for (uint i = 0; i < count; i++)
                {
                    if ((err = Cl.SetKernelArgSVMPointer(Kernel, i, *(argsPtr + i))) != ErrorCode.Success)
                    {
                        throw new Exception($"{err}");
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSvmArgs(uint[] argsIndex, params SvmPointer[] args)
        {
            ErrorCode err;
            int count = args.Length;
            if (argsIndex == null)
            {
                if (count != argsIndex.Length)
                {
                    throw new IndexOutOfRangeException($"{nameof(argsIndex)}.length != {nameof(args)}.length");
                }

                fixed (uint* argIndexPtr = argsIndex)
                fixed (SvmPointer* argsPtr = args)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if ((err = Cl.SetKernelArgSVMPointer(Kernel, *(argIndexPtr + i), *(argsPtr + i))) != ErrorCode.Success)
                        {
                            throw new Exception($"{err}");
                        }
                    }
                }
            }
            else
            {
                SetSvmArgs(args);
            }
        }

        public void LockSvmForGPU(params SvmPointer[] pointers)
        {
            foreach (var svm in pointers)
            {
                svm.Lock(this);
            }
        }
        public void UnlockSvmGPU(params SvmPointer[] pointers)
        {
            foreach (var svm in pointers)
            {
                svm.Unlock(this);
            }
        }
        /// <summary>
        /// Runing the GPU program
        /// </summary>
        /// <param name="workGroupSizePtr">image dimention in 3D</param>
        /// <param name="workingDim">global_id(n) (n := { (1D := 1), (2D := 2), (3D := 3) }</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(IntPtr[] workGroupSizePtr, uint workingDim = 1)
        {
            ErrorCode err;
            if ((err = Cl.EnqueueNDRangeKernel(CommandQueue, Kernel, workingDim, null, workGroupSizePtr, null, 0, null, out Event @event)) != ErrorCode.Success)
            {
                throw new Exception(err.ToString());
            }
            @event.WaitForComplete();
        }
        /// <summary>
        /// Runing the GPU program
        /// </summary>
        /// <param name="workGroupSizePtr">image dimention in 3D</param>
        /// <param name="workingDim">global_id(n) (n := { (1D := 1), (2D := 2), (3D := 3) }</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(IntPtr[] workGroupSizePtr, SvmPointer[] args, uint workingDim = 1)
        {
            LockSvmForGPU(args);
            
            SetSvmArgs(args);

            ErrorCode err;
            if ((err = Cl.EnqueueNDRangeKernel(CommandQueue, Kernel, workingDim, null, workGroupSizePtr, null, 0, null, out Event @event)) != ErrorCode.Success)
            {
                throw new Exception(err.ToString());
            }
            @event.WaitForComplete();

            UnlockSvmGPU(args);
        }

        /// <summary>
        /// Runing the GPU program
        /// </summary>
        /// <param name="workGroupSizePtr">image dimention in 3D</param>
        /// <param name="workingDim">global_id(n) (n := { (1D := 1), (2D := 2), (3D := 3) }</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(IntPtr[] workGroupSizePtr, uint[] argsIndex, SvmPointer[] args, uint workingDim = 1)
        {
            SetSvmArgs(argsIndex, args);
            LockSvmForGPU(args);

            ErrorCode err;
            if ((err = Cl.EnqueueNDRangeKernel(CommandQueue, Kernel, workingDim, null, workGroupSizePtr, null, 0, null, out Event @event)) != ErrorCode.Success)
            {
                throw new Exception(err.ToString());
            }
            @event.WaitForComplete();

            UnlockSvmGPU(args);
        }

        public void Dispose()
        {
            CommandQueue.Dispose();
            fixed (GPUBuffer* gpuBufferPtr = _gpuBuffers)
            {
                for (int iMemObj = 0; iMemObj < Arguments; iMemObj++)
                {
                    (gpuBufferPtr + iMemObj)->MemoryObject.Dispose();
                }
            }
        }
    }
}
