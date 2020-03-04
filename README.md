# Se7enCl
OpenCl in a easy way to use
 
## Compile 
### Linux
------------------
#### Linux release 
```cmd
> dotnet run --configuration Linux
```
#### Linux release obfuscate
```cmd
> dotnet run --configuration Linux-Obfuscate
```
#### Linux debug
```cmd
> dotnet run --configuration Linux-Debug
```
### Windows
------------------
#### Windows release 
```cmd
> dotnet run --configuration Windows
```
#### Windows release obfuscate
```cmd
> dotnet run --configuration Windows-Obfuscate
```
#### Windows debug
```cmd
> dotnet run --configuration Windows-Debug
```

## Example 

```csharp 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Se7enCl;
namespace Se7enCl.Test
{
    class Program
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
                
        unsafe static void Main(string[] args)
        {

            const int length = 90_000_000;

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

                while (true)
                {
                    watch.Restart();

                    bridge.SetSvmArgs(null, a, b, c);                       // Parameter set
                    bridge.Execute(new IntPtr[] { (IntPtr)length });        // exec
                    // finish
                    watch.Stop();
                    Console.WriteLine("OpenCl-Svm:" + watch.Elapsed.TotalMilliseconds);

                    watch.Restart();
                    ParallelLoopResult result = Parallel.For(0, length, x => c[x] = a[x] + b[x]);
                    watch.Stop();
                    Console.WriteLine("Parallel:" + watch.Elapsed.TotalMilliseconds);
                }
            }
        }
    }
}
```
