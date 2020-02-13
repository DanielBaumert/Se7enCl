using System;
using System.Runtime.InteropServices;
namespace Se7enCl
{  
    ///=>[^<]+([^>]+)[^<]+([^>]+)>\(([^,]+), out _\)\[0\];
    ///Utils.GetTInfo<$1, $2>(_handle, $3, _getInfoHandler, out _).First();
    ///=>[^<]+<([^>]+)[^<]+<([^>]+)>\(([^,]+), out _\)\.ToStrg\(\);
    ///=> Utils.GetTInfo<$1, $2>(_handle, $3, _getInfoHandler, out _).ToStrg();
    ///((IHandleObjInfo<DeviceInfo>)this).GetTInfo<uint>(DeviceInfo.PreferredLocalAtomicAlignment, out _)[0];
    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct Platform
    {
        public readonly IntPtr Handle;
        internal Platform(IntPtr handle)
            => Handle = handle;
        public string GetProfile()
           => Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Profile, OpenCl.GetPlatformInfo, out _).ToStrg();
        public string GetVersion()
           => Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Version, OpenCl.GetPlatformInfo, out _).ToStrg();
        public string GetName()
           => Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Name, OpenCl.GetPlatformInfo, out _).ToStrg();
        public string GetVendor()
           => Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Vendor, OpenCl.GetPlatformInfo, out _).ToStrg();
        public string GetExtensions()
           => Utils.GetTInfo<PlatformInfo, byte>(Handle, PlatformInfo.Extensions, OpenCl.GetPlatformInfo, out _).ToStrg();
        public ulong GetHosttimerresolution()
           => Utils.GetTInfo<PlatformInfo, ulong>(Handle, PlatformInfo.Vendor, OpenCl.GetPlatformInfo);

        public Device[] GetDevices(DeviceType type = DeviceType.All)
        {
            OpenCl.GetDeviceIDs(Handle, type, 0, null, out uint deviceCount);
            Device[] devices = new Device[deviceCount];
            OpenCl.GetDeviceIDs(Handle, type, deviceCount, devices, out _);
            return devices;
        }

        public static implicit operator IntPtr(Platform platform) => platform.Handle;

    }
}