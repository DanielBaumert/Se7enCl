using System;
using System.IO;
using System.Reflection;

namespace Se7en
{
    static class InternalLibLoader
    {
        public const string OpenCL = "OpenCL";

        private const string OpenClWin32NT = "OpenCL.dll";
        private const string OpenClLinux = "libOpenCL.so";

        static InternalLibLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                return e.Name switch
                {
                    OpenCL => LoadLib(
                                      sender,
                                      Environment.OSVersion.Platform == PlatformID.Win32NT
                                            ? OpenClWin32NT
                                            : OpenClLinux
                                     ),
                    _ => null,
                };
            };
        }

        private static Assembly LoadLib(object obj, string lib)
        {

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(lib))
            using (MemoryStream ms = new MemoryStream())
            {
                s.CopyTo(ms);
                return ((AppDomain)obj).Load(ms.ToArray());
            }
        }

    }
}