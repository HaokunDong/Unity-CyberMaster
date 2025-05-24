using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace EverlastingEditor.Base
{
	public class AlignedUnsafeMemoryStreamWriter
	{
		private byte[] bytes = new byte[1024];
		private int offset = 0;

		private void CheckCapacity(int num)
		{
			if (offset + num > bytes.Length)
			{
				var newBytes = new byte[bytes.Length * 2];
				Array.Copy(bytes, newBytes, bytes.Length);
				bytes = newBytes;
			}
		}

		public void Write(int i)
		{
			CheckCapacity(4);
			Unsafe.WriteUnaligned(ref bytes[offset], i);
			offset += 4;
		}

		public void Write(uint i)
		{
			CheckCapacity(4);
			Unsafe.WriteUnaligned(ref bytes[offset], i);
			offset += 4;
		}

		public void Write(long l)
		{
			CheckCapacity(8);
			Unsafe.WriteUnaligned(ref bytes[offset], l);
			offset += 8;
		}

		public void Write(ulong l)
		{
			CheckCapacity(8);
			Unsafe.WriteUnaligned(ref bytes[offset], l);
			offset += 8;
		}

		public void Write(bool b)
		{
			Write(b ? 1 : 0);
		}

		public void Write(float f)
		{
			CheckCapacity(4);
			Unsafe.WriteUnaligned(ref bytes[offset], f);
			offset += 4;
		}

		public void Write(string s)
		{
			byte[] strBytes = Encoding.UTF8.GetBytes(s);
			int length = strBytes.Length;
			ushort safesize = (ushort)((length + 2 + (4 - 1)) & -4);
			CheckCapacity(safesize);

			Unsafe.WriteUnaligned(ref bytes[offset], (ushort)length);
			offset += 2;
			if (length != 0)
				Unsafe.CopyBlockUnaligned(ref bytes[offset], ref strBytes[0], (uint)length);
			offset += safesize - 2;
		}

		public void Write<T>(in T s) where T : struct
		{
			int size = Unsafe.SizeOf<T>();
			int alignedSize = (size + 3) & (~3);
			CheckCapacity(alignedSize);
			Unsafe.WriteUnaligned(ref bytes[offset], s);
			offset += alignedSize;
		}

		public byte[] ToArray()
		{
			byte[] data = new byte[offset];
			Array.Copy(bytes, data, offset);
			return data;
		}

		public void WriteToStream(Stream sw)
		{
			sw.Write(bytes, 0, offset);
		}

		public void Clear()
		{
			offset = 0;
		}
	}
}