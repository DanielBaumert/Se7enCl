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
        private const string SOURCE_CODE = @"
                // Simple test; c[i] = a[i] + b[i]

                __kernel void add_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] + b[xid];
                }
                
                __kernel void sub_array(__global float *a, __global float *b, __global float *c)
                {
                    int xid = get_global_id(0);
                    c[xid] = a[xid] - b[xid];
                }
                ";

        uint _platformCount;
        Platform[] _platforms;
        Device _device;

        [TestInitialize]
        public void Init()
        {
            Cl.GetPlatformIDs(0, null, out _platformCount);
            _platforms = new Platform[_platformCount];
            Cl.GetPlatformIDs(_platformCount, _platforms, out _);
            _device = Device.GetDevice(_platforms, "GTX");
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
        public void TestMethod1()
        {

            const int length = 10;

            using (OpenClCompiler builder = new OpenClCompiler(SOURCE_CODE, "GTX")) // <- device selector ("*GTX*")
            using (OpenClBridge bridge = builder.GetMethode("add_array"))                           // methode capture
            using (SvmPointer<float> a = builder.AllocSvmMemory<float>((IntPtr)length))             // buffer init
            using (SvmPointer<float> b = builder.AllocSvmMemory<float>((IntPtr)length))             // buffer init
            using (SvmPointer<float> c = builder.AllocSvmMemory<float>((IntPtr)length))             // buffer init
            {
                //init values
                for (int i = 0; i < length; i++)
                {
                    a[i] = b[i] = i;
                }

                bridge.SetSvmArgs(a, b, c);                       // Parameter set
                bridge.Execute(new IntPtr[] { (IntPtr)length });        // exec

                //init values
                for (int i = 0; i < length; i++)
                {
                    Assert.AreEqual(a[i] + b[i], c[i]);
                }
            }
        }
    }
}
