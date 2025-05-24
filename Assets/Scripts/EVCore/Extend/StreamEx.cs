using System;
// using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Everlasting.Extend
{
    public static class StreamEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, int i)
        {
            stream.Write(BitConverter.GetBytes(i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Read(this Stream stream, byte[] bytes)
        {
            return stream.Read(bytes, 0, bytes.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<int> ReadAsync(this Stream stream, byte[] bytes)
        {
            return stream.ReadAsync(bytes, 0, bytes.Length);
        }
        
        //依赖System.Buffers.dll
        // public static TStruct ReadStruct<TStruct>(this Stream stream) where TStruct : struct
        // {
        //     var arrayPool = ArrayPool<byte>.Shared;
        //     var size = Unsafe.SizeOf<TStruct>();
        //     var buff = arrayPool.Rent(size);
        //     stream.Read(buff, 0, buff.Length);
        //     TStruct @struct = Unsafe.ReadUnaligned<TStruct>(ref buff[0]);
        //     arrayPool.Return(buff);
        //     return @struct;
        // }
        //
        // public static void WriteStruct<TStruct>(this Stream stream, in TStruct @struct) where TStruct : struct
        // {
        //     var arrayPool = ArrayPool<byte>.Shared;
        //     var size = Unsafe.SizeOf<TStruct>();
        //     var buff = arrayPool.Rent(size);
        //     Unsafe.WriteUnaligned(ref buff[0], @struct);
        //     stream.Write(buff, 0, buff.Length);
        //     arrayPool.Return(buff);
        // }

        public static byte[] ReadToEnd(this Stream stream)
        {
            if (stream.CanSeek)
            {
                long length = stream.Length;
                long position = stream.Position;
                byte[] bytes = new byte[length - position];
                stream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
            else
            {
                using (var ms = new MemoryStream(2048))
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static async Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            if (stream.CanSeek)
            {
                long length = stream.Length;
                long position = stream.Position;
                byte[] bytes = new byte[length - position];
                await stream.ReadAsync(bytes, 0, bytes.Length);
                return bytes;
            }
            else
            {
                using (var ms = new MemoryStream(2048))
                {
                    await stream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
        }

        //拷贝指定数量字节到目标stream
        public static void CopyToByLength(this Stream stream, Stream targetStream, long length, int bufferSize = 1024 * 40)
        {
            byte[] buffer = new byte[bufferSize];
            CopyToByLength(stream, targetStream, length, buffer);
        }
        
        public static void CopyToByLength(this Stream stream, Stream targetStream, long length, byte[] buffer)
        {
            var bufferSize = buffer.Length;
            int count;
            while (length > 0 && (count = stream.Read(buffer, 0, length < bufferSize ? (int)length : bufferSize)) != 0)
            {
                targetStream.Write(buffer, 0, count);
                length -= count;
            }
        }
        
        public static Task CopyToByLengthAsync(this Stream stream, Stream targetStream, long length, int bufferSize = 1024 * 40)
        {
            byte[] buffer = new byte[bufferSize];
            return CopyToByLengthAsync(stream, targetStream, length, buffer);
        }
        
        public static async Task CopyToByLengthAsync(this Stream stream, Stream targetStream, long length, byte[] buffer)
        {
            var bufferSize = buffer.Length;
            while (length > 0)
            {
                var count = await stream.ReadAsync(buffer, 0, length < bufferSize ? (int)length : bufferSize);
                if(count <= 0) break;
                targetStream.Write(buffer, 0, count);
                length -= count;
            }
        }
    }
}