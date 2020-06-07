using Se7en.OpenCl.Api.Enum;
using Se7en.OpenCl.Api.Native;
using System;
using System.Runtime.InteropServices;
namespace Se7en.OpenCl
{  
    
     // =>[^<]+([^>]+)[^<]+([^>]+)>\(([^,]+), out _\)\[0\];
     // Utils.GetTInfo<$1, $2>(_handle, $3, _getInfoHandler, out _).First();
     // =>[^<]+<([^>]+)[^<]+<([^>]+)>\(([^,]+), out _\)\.ToStrg\(\);
     // => Utils.GetTInfo<$1, $2>(_handle, $3, _getInfoHandler, out _).ToStrg();
     // ((IHandleObjInfo<DeviceInfo>)this).GetTInfo<uint>(DeviceInfo.PreferredLocalAtomicAlignment, out _)[0];
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Platform
    {
        public readonly IntPtr Handle;
        internal Platform(IntPtr handle)
        {
            Handle = handle;
        }

        public string GetProfile()
        {
            return Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Profile, Cl.GetPlatformInfo, out _).ToStrg();
        }

        public string GetVersion()
        {
            return Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Version, Cl.GetPlatformInfo, out _).ToStrg();
        }

        public string GetName()
        {
            return Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Name, Cl.GetPlatformInfo, out _).ToStrg();
        }

        public string GetVendor()
        {
            return Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Vendor, Cl.GetPlatformInfo, out _).ToStrg();
        }

        public string GetExtensions()
        {
            return Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Extensions, Cl.GetPlatformInfo, out _).ToStrg();
        }

        public ulong GetHosttimerresolution()
        {
            return Utils.GetTInfo<PlatformInfo, ulong>(Handle, PlatformInfo.Vendor, Cl.GetPlatformInfo);
        }

        public Device[] GetDevices(DeviceType type = DeviceType.All)
        {
            Cl.GetDeviceIDs(Handle, type, 0, null, out uint deviceCount);
            Device[] devices = new Device[deviceCount];
            Cl.GetDeviceIDs(Handle, type, deviceCount, devices, out _);
            return devices;
        }

        public static implicit operator IntPtr(Platform platform) => platform.Handle;

    }
}