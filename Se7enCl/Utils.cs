using System;
using System.Runtime.CompilerServices;

namespace Se7enCl
{

    ///=>[^<]+([^>]+)[^<]+([^>]+)>\(([^,]+), out _\)\[0\];
    ///Utils.GetTInfo<$1, $2>(_handle, $3, _getInfoHandler, out _).First();
    ///=>[^<]+<([^>]+)[^<]+<([^>]+)>\(([^,]+), out _\)\.ToStrg\(\);
    ///=> Utils.GetTInfo<$1, $2>(_handle, $3, _getInfoHandler, out _).ToStrg();
    ///((IHandleObjInfo<DeviceInfo>)this).GetTInfo<uint>(DeviceInfo.PreferredLocalAtomicAlignment, out _)[0];

    internal unsafe delegate ErrorCode GetInfoHandler<TInfo>(IntPtr handle, TInfo info, uint paramValSize, void* paramVal, out uint paramValSizeRet);
    internal static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static string ToStrg(this byte[] source)
        {
            fixed(byte* sourcePtr = source)
            {
                return new string((sbyte*) sourcePtr).TrimEnd('\0');
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T First<T>(this T[] source) where T : unmanaged
        {
            fixed (T* sourcePtr = source)
                return *sourcePtr;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T First<T>(this T[] source, Func<T, bool> comp) where T : unmanaged
        {
            fixed (T* sourcePtr = source)
            {
                for (int i = 0, n = source.Length; i < n; i++)
                {
                    T* obj = sourcePtr + i;
                    if (comp(*obj))
                        return *sourcePtr;
                }
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IndexElement<T> FirstWithIndex<T>(this T[] source, Func<T, bool> comp)
            where T : unmanaged
        {
            fixed (T* sourcePtr = source)
            {
                for (int i = 0, n = source.Length; i < n; i++)
                {
                    T* obj = sourcePtr + i;
                    if (comp(*obj))
                        return new IndexElement<T>(i, *obj);
                }
                return default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static TOut[] GetTInfo<TInfo, TOut>(IntPtr handle, TInfo info, GetInfoHandler<TInfo> getInfoHandler, out uint length)
            where TInfo : Enum
            where TOut : unmanaged
        {
            ErrorCode err;
            if ((err = getInfoHandler(handle, info, 0, null, out length)) == ErrorCode.Success)
            {
                TOut[] target = new TOut[length];
                fixed (TOut* targetPtr = target)
                {
                    if ((err = getInfoHandler(handle, info, length, targetPtr, out _)) == ErrorCode.Success)
                    {
                        return target;
                    }
                    throw new Exception($"{err}");
                }
            }
            throw new Exception($"{err}");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static TOut GetTInfo<TInfo, TOut>(IntPtr handle, TInfo info, GetInfoHandler<TInfo> getInfoHandler)
          where TInfo : Enum
          where TOut : unmanaged
        {
            ErrorCode err;
            if ((err = getInfoHandler(handle, info, 0, null, out uint length)) == ErrorCode.Success)
            {
                TOut @out/* = default*/;
                if ((err = getInfoHandler(handle, info, length, &@out, out _)) == ErrorCode.Success)
                {
                    return @out;
                }
                throw new Exception($"{err}");
            }
            throw new Exception($"{err}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static TOut[] SelectUnsafe<TIn, TOut>(this TIn[] source, Func<TIn, TOut> selectAction)
            where TIn : unmanaged
            where TOut : unmanaged
        {
            int length = source.Length;
            TOut[] target = new TOut[length];
            fixed (TOut* targetPtr = target)
            {
                fixed (TIn* sourcePtr = source)
                {
                    for (int iElement = 0; iElement < length; iElement++)
                    {
                        *(targetPtr + iElement) = selectAction(*(sourcePtr + iElement));
                    }
                }
            }
            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int ToInt(this bool val) => *(int*)&val;
    }
}
