using Microsoft.VisualStudio.TestTools.UnitTesting;
using Se7en.OpenCl.Api.Enum;
using Se7en.OpenCl.Api.Native;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Se7en.OpenCl.Test
{
    [TestClass]
    public class UnitTest1
    {
        //private const string SOURCE_CODE = @"
        //        // Simple test; c[i] = a[i] + b[i]


        //        __kernel void sub_array(__global float *a, __global float *b, __global float *c)
        //        {
        //            int xid = get_global_id(0);
        //            c[xid] = a[xid] - b[xid];
        //        }
        //        ";

        uint _platformCount;
        Platform[] _platforms;
        Device _device;
        Context _context;
        CommandQueue _commandQueue;
        [TestInitialize]
        public void Init()
        {
            Cl.GetPlatformIDs(0, _platforms, out _platformCount);
            _platforms = new Platform[_platformCount];
            Cl.GetPlatformIDs(_platformCount, _platforms, out _);
            _device = Device.GetDevice(_platforms, "GTX");
            _context = _device.CreateContext();
            _commandQueue = Cl.CreateCommandQueue(_context, _device, CommandQueueProperties.None, out _);
        }

        [TestMethod]
        public void GetPlatformCount()
        {

            Cl.GetPlatformIDs(0, (Platform[])null, out uint platformCount);
            Assert.IsTrue(platformCount > 0, $"{platformCount}");
        }
        [TestMethod]
        public void GetPlatforms()
        {
            Platform[] platforms = null;
            Cl.GetPlatformIDs(0, platforms, out uint platformCount);
            platforms = new Platform[platformCount];
            Cl.GetPlatformIDs(platformCount, platforms, out _);

            Assert.IsTrue(
                platforms.Count(platform => platform.Handle != IntPtr.Zero) > 0,
                string.Join(Environment.NewLine, platforms.Select(platform => platform.GetName()))
            );
        }
        [TestMethod]
        public void GetGpuDeviceByType()
        {
            Device device = Device.GetDevice(_platforms, DeviceType.Gpu);
            Assert.IsTrue(device.Name.Contains("GeForce GTX 1080"));
        }
        [TestMethod]
        public void GetGpuDeviceByName()
        {
            Device device = Device.GetDevice(_platforms, "GTX");
            Assert.IsTrue(device.Name.Contains("GeForce GTX 1080"));
        }

        [TestMethod]
        public void GetCommandQueue()
        {
            CommandQueue commandQueue = Cl.CreateCommandQueue(_context, _device, CommandQueueProperties.None, out _);
            Assert.IsTrue(commandQueue != IntPtr.Zero);
            commandQueue.Dispose();
        }

        [TestMethod]
        public void GetContextFromDevice()
        {
            Context context = _device.CreateContext();
        }
        [TestMethod]
        public void GetContextByCl()
        {
            IntPtr context = Cl.CreateContext(null, 1, new Device[] { _device }, null, IntPtr.Zero, out ErrorCode _);
            Assert.IsTrue(context != IntPtr.Zero);
            Cl.ReleaseContext(context);
        }
        [TestMethod]
        public void GetContextByClWithPlattform()
        {
            throw new NotSupportedException("");
            IntPtr context = Cl.CreateContext(
                new ContextProperty[] {
                    new ContextProperty(ContextProperties.Platform, _platforms.First().Handle)
                },
                1,
                new Device[] {
                    _device
                },
                null,
                IntPtr.Zero,
                out ErrorCode _
            );
            Assert.IsTrue(context != IntPtr.Zero);
            Cl.ReleaseContext(context);
        }

        [TestMethod]
        public void AllocSVMMemory()
        {
            IntPtr svmMemory = Cl.SVMAlloc(_context, SVMMemFlags.ReadWrite, new IntPtr(10), sizeof(int) * 4);
            Assert.IsTrue(svmMemory != IntPtr.Zero, $"{(svmMemory.ToInt64().ToString("X16"))}");
            Cl.SVMFree(_context, svmMemory);
        }

        [TestMethod]
        public void AllocedLockCheck()
        {
            IntPtr length = new IntPtr(10);
            IntPtr svmMemory = Cl.SVMAlloc(_context, SVMMemFlags.ReadWrite, length, sizeof(int) * 4);

            ErrorCode error = Cl.EnqueueSVMMap(_commandQueue, 1, MapFlags.Read | MapFlags.Write, svmMemory, length, 0, null, out Event @event);
            Assert.IsTrue(error == ErrorCode.Success, $"{error}");
            @event.WaitForComplete();

            _ = Cl.EnqueueSVMUnmap(_commandQueue, svmMemory, 0, null, out Event @event1);
            @event1.WaitForComplete();

            Cl.SVMFree(_context, svmMemory);
        }

        [TestMethod]
        public void AllocedUnlockCheck()
        {
            IntPtr length = new IntPtr(10);
            IntPtr svmMemory = Cl.SVMAlloc(_context, SVMMemFlags.ReadWrite, length, sizeof(int) * 4);

            _ = Cl.EnqueueSVMMap(_commandQueue, 1, MapFlags.Read | MapFlags.Write, svmMemory, length, 0, null, out Event @event);
            @event.WaitForComplete();

            ErrorCode error = Cl.EnqueueSVMUnmap(_commandQueue, svmMemory, 0, null, out Event @event1);
            Assert.IsTrue(error == ErrorCode.Success, $"{error}");
            @event1.WaitForComplete();


            Cl.SVMFree(_context, svmMemory);
        }
        [TestMethod]
        public void AllocSVMMemoryFineGrainBuffer()
        {
            IntPtr svmMemory = Cl.SVMAlloc(_context, SVMMemFlags.ReadWrite | SVMMemFlags.FineGrainBuffer, new IntPtr(10), sizeof(int) * 4);
            Assert.IsTrue(svmMemory != IntPtr.Zero, $"0x{(svmMemory.ToInt64().ToString("X16"))}, FineGrainBufferSupported: {_device.IsFineGrainBufferSupported}");
            Cl.SVMFree(_context, svmMemory);
        }

        [TestMethod]
        public void AllocSVMMemoryFineGrainBufferAtomic()
        {
            IntPtr svmMemory = Cl.SVMAlloc(_context, SVMMemFlags.ReadWrite | SVMMemFlags.FineGrainBuffer | SVMMemFlags.Atomic, new IntPtr(10), sizeof(int) * 4);
            Assert.IsTrue(
                svmMemory != IntPtr.Zero,
                $"0x{(svmMemory.ToInt64().ToString("X16"))}, FineGrainBufferSupported with Atomic: {_device.IsFineGrainBufferSupported && _device.IsAtomicSupported}"
            );
            Cl.SVMFree(_context, svmMemory);
        }

        [TestMethod]
        public void TestCode1()
        {

            const string SOURCE_CODE = @"
                __kernel void add_array(global float *a, global float *b, global float *c)
                {
                    int xid = get_global_id(0);
                    *(c + xid) = *(a + xid) + *(b + xid) + (float)7;
                }";

            const int length = 10;

            using (OpenClCompiler builder = new OpenClCompiler(SOURCE_CODE, DeviceType.Gpu)) // <- device selector ("*GTX*")
            using (OpenClBridge bridge = builder.GetMethode("add_array"))                   // methode capture
            using (SvmPointer<float> a = builder.AllocSvmMemory<float>(length))             // buffer init
            using (SvmPointer<float> b = builder.AllocSvmMemory<float>(length))             // buffer init
            using (SvmPointer<float> c = builder.AllocSvmMemory<float>(length))             // buffer init
            {

                //init values
                bridge.LockSvmForGPU(a, b);
                for (int i = 1; i <= length; i++)
                {
                    a[i - 1] = (float)i;
                    b[i - 1] = (float)i;
                }
                bridge.UnlockSvmGPU(b, a);

                bridge.SetSvmArgs(a, b, c);
                bridge.Execute(new IntPtr[] { (IntPtr)length });        // exec


                bridge.LockSvmForGPU(c);
                //check values
                for (int i = 0; i < length; i++)
                {
                    Assert.AreEqual(c[i], 0);
                }
                bridge.UnlockSvmGPU(c);
            }
        }

        [TestMethod]
        public void TestCode2()
        {

            const string SOURCE_CODE = @"
                __constant float4 GRAY_SCALE_FILTER_MATRIX = (float4) (0.30, 0.59, 0.11, 0);

                __kernel void grayscale(__global char4* a, __global char4* b)
                {
	                int pos = get_global_id(0);

	                a[pos] = b[pos];
                }";

            /*
                      char4 inPixelPtr = input[pos];
                      char4 grayScaledPixel = convert_char4(convert_float4(inPixelPtr) * GRAY_SCALE_FILTER_MATRIX);
                  */

            const int length = 10;

            using (OpenClCompiler builder = new OpenClCompiler(SOURCE_CODE, DeviceType.Gpu)) // <- device selector ("*GTX*")
            using (OpenClBridge bridge = builder.GetMethode("grayscale"))                   // methode capture
            using (SvmPointer<int> a = builder.AllocSvmMemory<int>(length))             // buffer init
            using (SvmPointer<int> b = builder.AllocSvmMemory<int>(length))             // buffer init
            {
                bridge.LockSvmForGPU(a, b);
                //init values
                for (int i = 1; i <= length; i++)
                {
                    a[i - 1] = i;
                }
                bridge.UnlockSvmGPU(a, b);


                bridge.SetSvmArgs(a, b);
                bridge.Execute(new IntPtr[] { (IntPtr)length });        // exec

                bridge.LockSvmForGPU(a, b);
                //check values
                for (int i = 0; i < length; i++)
                {
                    Console.WriteLine(a[i]);
                    Assert.AreEqual(a[i], b[i]);
                }
                bridge.UnlockSvmGPU(a, b);
            }
        }


        [TestMethod]
        public void TestCode3()
        {
            const int ITEM_COUNT = 10;
            const string SOURCE_CODE = @"
                __kernel void add_array(global float *a, global float *b, global float *c)
                {
                    int xid = get_global_id(0);
                    *(c + xid) = *(a + xid) + *(b + xid) + (float)7;
                }";

            ErrorCode error;
            Event @event;
            // get platforms
            if ((error = Cl.GetPlatformIDs(0, (IntPtr[])null, out uint platformCount)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            IntPtr[] platforms = new IntPtr[platformCount];
            if ((error = Cl.GetPlatformIDs(platformCount, platforms, out _)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // get device and platform
            IntPtr platform = IntPtr.Zero;
            IntPtr gpuDevice = IntPtr.Zero;
            foreach (IntPtr selectPlatform in platforms)
            {
                if ((error = Cl.GetDeviceIDs(selectPlatform, DeviceType.Gpu, 0, null, out uint deviceCount)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }
                if (deviceCount == 0)
                {
                    continue;
                }
                Device[] devices = new Device[deviceCount];
                if ((error = Cl.GetDeviceIDs(selectPlatform, DeviceType.Gpu, deviceCount, devices, out _)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }

                foreach (Device device in devices)
                {
                    DeviceType type;
                    unsafe
                    {
                        if ((error = Cl.GetDeviceInfo(device, DeviceInfo.Type, 0, null, out uint length)) != ErrorCode.Success)
                        {
                            throw new Exception(error.ToString());
                        }

                        if ((error = Cl.GetDeviceInfo(device, DeviceInfo.Type, length, &type, out _)) != ErrorCode.Success)
                        {
                            throw new Exception(error.ToString());
                        }
                    }
                    if (type == DeviceType.Gpu)
                    {
                        gpuDevice = device;
                        goto next1;
                    }
                    Cl.ReleaseDevice(device);
                }
            }
        next1:
            if (gpuDevice == IntPtr.Zero)
            {
                throw new Exception("target device type not found");
            }

            //get context form device
            IntPtr context = Cl.CreateContext(null, 1, new IntPtr[] { gpuDevice }, null, IntPtr.Zero, out error);
            if (error != ErrorCode.Success)
            {
                throw new Exception($"clCreateContext failed: {error}");
            }

            // get command queue
            IntPtr commandQueue = Cl.CreateCommandQueueWithProperties(context, gpuDevice, null, out error);
            if (error != ErrorCode.Success)
            {
                throw new Exception($"Unable to create command queue: { error}");
            }
            // compile gpu code
            IntPtr program = Cl.CreateProgramWithSource(context, 1, new string[] { SOURCE_CODE }, null, out error);
            if (error != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            if ((error = Cl.BuildProgram(program, 1, new IntPtr[] { gpuDevice }, string.Empty, null, IntPtr.Zero)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // set execution kernel
            IntPtr[] kernels = null;
            if ((error = Cl.CreateKernelsInProgram(program, 0, kernels, out uint kernelCount)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if (kernelCount == 0)
            {
                throw new Exception("No kernel methodes found - please marke your methode with __kernel");
            }
            kernels = new IntPtr[kernelCount];
            if ((error = Cl.CreateKernelsInProgram(program, kernelCount, kernels, out _)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // get kernel from methode
            IntPtr methodeKernel = IntPtr.Zero;
            foreach (IntPtr kernel in kernels)
            {
                if ((error = Cl.GetKernelInfo(kernel, KernelInfo.FunctionName, 0, IntPtr.Zero, out uint functionNameLength)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }
                if (functionNameLength == 0)
                {
                    throw new Exception("Unknow error in GetKernelInfo-FunctionName");
                }
                IntPtr functionNameBuffer = Marshal.AllocHGlobal((int)functionNameLength);
                if ((error = Cl.GetKernelInfo(kernel, KernelInfo.FunctionName, functionNameLength, functionNameBuffer, out _)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }

                string methodeName = Marshal.PtrToStringAnsi(functionNameBuffer);
                if (methodeName == "add_array")
                {
                    methodeKernel = kernel;
                    break;
                }
                Marshal.FreeHGlobal(functionNameBuffer);
            }

            if (methodeKernel == IntPtr.Zero)
            {
                throw new Exception("No method found");
            }
            // alloc memory 
            // alloc buffer a
            int bufferALength = ITEM_COUNT * sizeof(int);
            IntPtr bufferA = Cl.SVMAlloc(context, SVMMemFlags.ReadWrite, new IntPtr(bufferALength), sizeof(int));
            if (bufferA == IntPtr.Zero)
            {
                throw new Exception("SVMBuffer alloc failed - a");
            }
            // alloc buffer b
            int bufferBLength = ITEM_COUNT * sizeof(int);
            IntPtr bufferB = Cl.SVMAlloc(context, SVMMemFlags.ReadWrite, new IntPtr(bufferBLength), sizeof(int));
            if (bufferB == IntPtr.Zero)
            {
                throw new Exception("SVMBuffer alloc failed - b");
            }
            // alloc buffer c
            int bufferCLength = ITEM_COUNT * sizeof(int);
            IntPtr bufferC = Cl.SVMAlloc(context, SVMMemFlags.ReadWrite, new IntPtr(bufferCLength), sizeof(int));
            if (bufferC == IntPtr.Zero)
            {
                throw new Exception("SVMBuffer alloc failed - c");
            }

            // lock buffer for host
            if ((error = Cl.EnqueueSVMMap(commandQueue, 1, MapFlags.Read | MapFlags.Write, bufferA, new IntPtr(bufferALength), 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //write to buffer a
            for (int i = 0; i < ITEM_COUNT; i++)
            {
                Marshal.WriteInt32(bufferA, sizeof(int) * i, i);
            }
            // unlock buffer for host
            if ((error = Cl.EnqueueSVMUnmap(commandQueue, bufferA, 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // lock buffer for host
            if ((error = Cl.EnqueueSVMMap(commandQueue, 1, MapFlags.Read | MapFlags.Write, bufferB, new IntPtr(bufferBLength), 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //write to buffer b
            for (int i = 0; i < ITEM_COUNT; i++)
            {
                Marshal.WriteInt32(bufferB, sizeof(int) * i, i);
            }
            // unlock buffer for host
            if ((error = Cl.EnqueueSVMUnmap(commandQueue, bufferB, 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // set arguments
            //set buffer a to argument index 0
            if ((error = Cl.SetKernelArgSVMPointer(methodeKernel, 0, bufferA)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //set buffer b to argument index 1
            if ((error = Cl.SetKernelArgSVMPointer(methodeKernel, 1, bufferB)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //set buffer c to argument index 2
            if ((error = Cl.SetKernelArgSVMPointer(methodeKernel, 2, bufferC)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            long localSize = sizeof(int);
            long globalSize = (long)(Math.Ceiling(ITEM_COUNT / (double)localSize) * localSize);

            // run the programm 
            if ((error = Cl.EnqueueNDRangeKernel(commandQueue, methodeKernel, 1, null, new IntPtr[] { new IntPtr(globalSize) }, new IntPtr[] { new IntPtr(localSize) }, 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            @event.WaitForComplete();

            // check buffer c

            for (int i = 0; i < ITEM_COUNT; i++)
            {
                Assert.AreEqual(i + i, Marshal.ReadInt32(bufferA, i));
                Assert.AreEqual(i + i, Marshal.ReadInt32(bufferB, i));
                Assert.AreEqual(i + i, Marshal.ReadInt32(bufferC, i));
            }

            // Clean up
            //  SVM buffer
            if ((error = Cl.SVMFree(context, bufferA)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.SVMFree(context, bufferB)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.SVMFree(context, bufferC)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //  kernels
            foreach (IntPtr kernel in kernels)
            {
                if ((error = Cl.ReleaseEvent(kernel)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }
            }
            //  prorgram
            if ((error = Cl.ReleaseProgram(program)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // command queue
            if ((error = Cl.ReleaseCommandQueue(commandQueue)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // context
            if ((error = Cl.ReleaseContext(gpuDevice)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // Devices
            if ((error = Cl.ReleaseDevice(gpuDevice)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
        }

        [TestMethod]
        public void TestCode4()
        {
            const int ITEM_COUNT = 10;
            const string SOURCE_CODE = @"
                __kernel void add_array(global float *a, global float *b, global float *c)
                {
                    int xid = get_global_id(0);
                    *(c + xid) = *(a + xid) + *(b + xid) + (float)7;
                }";

            ErrorCode error;
            Event @event;
            // get platforms
            if ((error = Cl.GetPlatformIDs(0, (IntPtr[])null, out uint platformCount)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            IntPtr[] platforms = new IntPtr[platformCount];
            if ((error = Cl.GetPlatformIDs(platformCount, platforms, out _)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // get device and platform
            IntPtr platform = IntPtr.Zero;
            IntPtr gpuDevice = IntPtr.Zero;
            foreach (IntPtr selectPlatform in platforms)
            {
                if ((error = Cl.GetDeviceIDs(selectPlatform, DeviceType.Gpu, 0, null, out uint deviceCount)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }
                if (deviceCount == 0)
                {
                    continue;
                }
                Device[] devices = new Device[deviceCount];
                if ((error = Cl.GetDeviceIDs(selectPlatform, DeviceType.Gpu, deviceCount, devices, out _)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }

                foreach (Device device in devices)
                {
                    DeviceType type;
                    unsafe
                    {
                        if ((error = Cl.GetDeviceInfo(device, DeviceInfo.Type, 0, null, out uint length)) != ErrorCode.Success)
                        {
                            throw new Exception(error.ToString());
                        }

                        if ((error = Cl.GetDeviceInfo(device, DeviceInfo.Type, length, &type, out _)) != ErrorCode.Success)
                        {
                            throw new Exception(error.ToString());
                        }
                    }
                    if (type == DeviceType.Gpu)
                    {
                        gpuDevice = device;
                        goto next1;
                    }
                    Cl.ReleaseDevice(device);
                }
            }
        next1:
            if (gpuDevice == IntPtr.Zero)
            {
                throw new Exception("target device type not found");
            }

            //get context form device
            IntPtr context = Cl.CreateContext(null, 1, new IntPtr[] { gpuDevice }, null, IntPtr.Zero, out error);
            if (error != ErrorCode.Success)
            {
                throw new Exception($"clCreateContext failed: {error}");
            }

            // get command queue
            IntPtr commandQueue = Cl.CreateCommandQueueWithProperties(context, gpuDevice, null, out error);
            if (error != ErrorCode.Success)
            {
                throw new Exception($"Unable to create command queue: { error}");
            }
            // compile gpu code
            IntPtr program = Cl.CreateProgramWithSource(context, 1, new string[] { SOURCE_CODE }, null, out error);
            if (error != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            if ((error = Cl.BuildProgram(program, 1, new IntPtr[] { gpuDevice }, string.Empty, null, IntPtr.Zero)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // set execution kernel
            IntPtr[] kernels = null;
            if ((error = Cl.CreateKernelsInProgram(program, 0, kernels, out uint kernelCount)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if (kernelCount == 0)
            {
                throw new Exception("No kernel methodes found - please marke your methode with __kernel");
            }
            kernels = new IntPtr[kernelCount];
            if ((error = Cl.CreateKernelsInProgram(program, kernelCount, kernels, out _)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // get kernel from methode
            IntPtr methodeKernel = IntPtr.Zero;
            foreach (IntPtr kernel in kernels)
            {
                if ((error = Cl.GetKernelInfo(kernel, KernelInfo.FunctionName, 0, IntPtr.Zero, out uint functionNameLength)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }
                if (functionNameLength == 0)
                {
                    throw new Exception("Unknow error in GetKernelInfo-FunctionName");
                }
                IntPtr functionNameBuffer = Marshal.AllocHGlobal((int)functionNameLength);
                if ((error = Cl.GetKernelInfo(kernel, KernelInfo.FunctionName, functionNameLength, functionNameBuffer, out _)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }

                string methodeName = Marshal.PtrToStringAnsi(functionNameBuffer);
                if (methodeName == "add_array")
                {
                    methodeKernel = kernel;
                    break;
                }
                Marshal.FreeHGlobal(functionNameBuffer);
            }

            if (methodeKernel == IntPtr.Zero)
            {
                throw new Exception("No method found");
            }
            // alloc memory 
            // alloc buffer a
            int bufferALength = ITEM_COUNT * sizeof(int);
            IntPtr bufferA = Cl.SVMAlloc(context, SVMMemFlags.ReadWrite, new IntPtr(bufferALength), sizeof(int));
            if (bufferA == IntPtr.Zero)
            {
                throw new Exception("SVMBuffer alloc failed - a");
            }
            // alloc buffer b
            int bufferBLength = ITEM_COUNT * sizeof(int);
            IntPtr bufferB = Cl.SVMAlloc(context, SVMMemFlags.ReadWrite, new IntPtr(bufferBLength), sizeof(int));
            if (bufferB == IntPtr.Zero)
            {
                throw new Exception("SVMBuffer alloc failed - b");
            }
            // alloc buffer c
            int bufferCLength = ITEM_COUNT * sizeof(int);
            IntPtr bufferC = Cl.SVMAlloc(context, SVMMemFlags.ReadWrite, new IntPtr(bufferCLength), sizeof(int));
            if (bufferC == IntPtr.Zero)
            {
                throw new Exception("SVMBuffer alloc failed - c");
            }

            // lock buffer for host
            if ((error = Cl.EnqueueSVMMap(commandQueue, 1, MapFlags.Read | MapFlags.Write, bufferA, new IntPtr(bufferALength), 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //write to buffer a
            for (int i = 0; i < ITEM_COUNT; i++)
            {
                Marshal.WriteInt32(bufferA, sizeof(int) * i, i);
            }
            // unlock buffer for host
            if ((error = Cl.EnqueueSVMUnmap(commandQueue, bufferA, 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // lock buffer for host
            if ((error = Cl.EnqueueSVMMap(commandQueue, 1, MapFlags.Read | MapFlags.Write, bufferB, new IntPtr(bufferBLength), 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //write to buffer b
            for (int i = 0; i < ITEM_COUNT; i++)
            {
                Marshal.WriteInt32(bufferB, sizeof(int) * i, i);
            }
            // unlock buffer for host
            if ((error = Cl.EnqueueSVMUnmap(commandQueue, bufferB, 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.WaitForEvents(1, new Event[] { @event })) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            // set arguments
            //set buffer a to argument index 0
            if ((error = Cl.CreateBuffer(context, MemFlags.ReadWrite | MemFlags.(methodeKernel, 0, bufferA)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //set buffer b to argument index 1
            if ((error = Cl.SetKernelArgSVMPointer(methodeKernel, 1, bufferB)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //set buffer c to argument index 2
            if ((error = Cl.SetKernelArgSVMPointer(methodeKernel, 2, bufferC)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }

            long localSize = sizeof(int);
            long globalSize = (long)(Math.Ceiling(ITEM_COUNT / (double)localSize) * localSize);

            // run the programm 
            if ((error = Cl.EnqueueNDRangeKernel(commandQueue, methodeKernel, 1, null, new IntPtr[] { new IntPtr(globalSize) }, new IntPtr[] { new IntPtr(localSize) }, 0, null, out @event)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            @event.WaitForComplete();

            // check buffer c

            for (int i = 0; i < ITEM_COUNT; i++)
            {
                Assert.AreEqual(i + i, Marshal.ReadInt32(bufferA, i));
                Assert.AreEqual(i + i, Marshal.ReadInt32(bufferB, i));
                Assert.AreEqual(i + i, Marshal.ReadInt32(bufferC, i));
            }

            // Clean up
            //  SVM buffer
            if ((error = Cl.SVMFree(context, bufferA)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.SVMFree(context, bufferB)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            if ((error = Cl.SVMFree(context, bufferC)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            //  kernels
            foreach (IntPtr kernel in kernels)
            {
                if ((error = Cl.ReleaseEvent(kernel)) != ErrorCode.Success)
                {
                    throw new Exception(error.ToString());
                }
            }
            //  prorgram
            if ((error = Cl.ReleaseProgram(program)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // command queue
            if ((error = Cl.ReleaseCommandQueue(commandQueue)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // context
            if ((error = Cl.ReleaseContext(gpuDevice)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
            // Devices
            if ((error = Cl.ReleaseDevice(gpuDevice)) != ErrorCode.Success)
            {
                throw new Exception(error.ToString());
            }
        }
    }
}
