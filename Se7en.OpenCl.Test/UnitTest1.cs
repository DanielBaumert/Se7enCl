using Microsoft.VisualStudio.TestTools.UnitTesting;
using Se7en.OpenCl.Api.Enum;
using Se7en.OpenCl.Api.Native;
using System;
using System.Diagnostics;
using System.Linq;

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
            Cl.GetPlatformIDs(0, null, out _platformCount);
            _platforms = new Platform[_platformCount];
            Cl.GetPlatformIDs(_platformCount, _platforms, out _);
            _device = Device.GetDevice(_platforms, "GTX");
            _context = _device.CreateContext();
            _commandQueue = Cl.CreateCommandQueue(_context, _device, CommandQueueProperties.None, out _);
        }

        [TestMethod]
        public void GetPlatformCount()
        {
            Cl.GetPlatformIDs(0, null, out uint platformCount);
            Assert.IsTrue(platformCount > 0, $"{platformCount}");
        }
        [TestMethod]
        public void GetPlatforms()
        {
            Cl.GetPlatformIDs(0, null, out uint platformCount);
            Platform[] platforms = new Platform[platformCount];
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

            ErrorCode  error = Cl.EnqueueSVMUnmap(_commandQueue, svmMemory, 0, null, out Event @event1);
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
        public void TestMethod1()
        {

            const string SOURCE_CODE = @"
                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }";

            const int length = 10;

            using (OpenClCompiler builder = new OpenClCompiler(SOURCE_CODE, DeviceType.Gpu)) // <- device selector ("*GTX*")
            using (OpenClBridge bridge = builder.GetMethode("add_array"))                   // methode capture
            using (SvmPointer<float> a = builder.AllocSvmMemory<float>(length))             // buffer init
            using (SvmPointer<float> b = builder.AllocSvmMemory<float>(length))             // buffer init
            using (SvmPointer<float> c = builder.AllocSvmMemory<float>(length))             // buffer init
            {
                //init values
                for (int i = 0; i < length; i++)
                {
                    a[i] = b[i] = i;
                }

                bridge.Execute(new IntPtr[] { (IntPtr)length }, new SvmPointer[] { a, b, c } );        // exec
                
                //check values
                for (int i = 0; i < length; i++)
                {
                    Assert.AreEqual(a[i] + b[i], c[i]);
                }
            }
        }
    }
}
