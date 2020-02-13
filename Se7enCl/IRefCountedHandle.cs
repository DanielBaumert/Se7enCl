using System;

namespace Se7enCl
{
    internal interface IRefCountedHandle: IDisposable
    {
        ErrorCode Retain();
        ErrorCode Release();

    }
}