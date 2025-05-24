using System;
using System.IO;
using System.IO.Compression;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;

namespace Everlasting.Config.Loader
{
    public static class ConfigLoader
    {
        private const int MAGIC = (byte) 'E' | ((byte) 'V' << 8) | ((byte) 'L' << 16) | ('C' << 24);
        public static async UniTask<ConfigBinary> LoadConfigBytesAsync(string relativePath)
        {
            var fullPath = ZString.Concat(Application.streamingAssetsPath, "/", relativePath);
            var bytes = await ResourceManager.LoadBytes(fullPath);
            if (bytes.Length < 8)
                throw new Exception("Invalid Config Binary");
            int magic = BitConverter.ToInt32(bytes, 0);
            if (magic != MAGIC)
                throw new Exception("Invalid Config Magic");
            int length = BitConverter.ToInt32(bytes, 4);
            int key = length ^ 0x6BF08C13;
            byte[] keyBin = BitConverter.GetBytes(key);
            for (int i = 8; i < bytes.Length; i++)
            {
                byte k = keyBin[i & 3];
                bytes[i] ^= k;
            }
        
            // var decoded = new byte[length];
            // using (var ms = new MemoryStream(bytes, 8, bytes.Length - 8))
            // {
            //     using (var zstd = new ZstandardStream(ms, CompressionMode.Decompress))
            //     {
            //         if (await zstd.ReadAsync(decoded, 0, length) != length)
            //         {
            //             throw new Exception("Invalid Config Binary");
            //         }
            //
            //         return new ConfigBinary(decoded);
            //     }
            // }
            
            if (length != bytes.Length - 8)
            {
                throw new Exception("Invalid Config Binary");
            }

            var dataBytes = new byte[length];
            Buffer.BlockCopy(bytes, 8, dataBytes, 0, length);
            return new ConfigBinary(dataBytes);
        }
    }
    
}