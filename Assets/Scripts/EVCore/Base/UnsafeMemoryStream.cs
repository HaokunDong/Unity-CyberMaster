
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Everlasting.Base
{
	public class AlignedUnsafeMemoryStreamReader
	{
		private readonly byte[] bytes;
		private int offset = 0;

		private void CheckSize(int size)
		{
			if (offset + size > bytes.Length)
			{
				throw new InvalidDataException();
			}
		}

		public AlignedUnsafeMemoryStreamReader(byte[] bytes)
		{
			this.bytes = bytes;
		}

		public int ReadInt()
		{
			CheckSize(4);
			int result = Unsafe.ReadUnaligned<int>(ref this.bytes[offset]);
			offset += 4;
			return result;
		}

		public uint ReadUInt()
		{
			CheckSize(4);
			uint result = Unsafe.ReadUnaligned<uint>(ref this.bytes[offset]);
			offset += 4;
			return result;
		}

		public float ReadFloat()
		{
			CheckSize(4);
			float result = Unsafe.ReadUnaligned<float>(ref this.bytes[offset]);
			offset += 4;
			return result;
		}

		public bool ReadBool()
		{
			return ReadInt() != 0;
		}

		public long ReadLong()
		{
			CheckSize(8);
			long result = Unsafe.ReadUnaligned<long>(ref this.bytes[offset]);
			offset += 8;
			return result;
		}

		public ulong ReadULong()
		{
			CheckSize(8);
			ulong result = Unsafe.ReadUnaligned<ulong>(ref this.bytes[offset]);
			offset += 8;
			return result;
		}

		public string ReadString()
		{
			CheckSize(2);
			ushort size = Unsafe.ReadUnaligned<ushort>(ref this.bytes[offset]);
			offset += 2;
			ushort safesize = (ushort) (((size + 2 + (4 - 1)) & -4) - 2);
			CheckSize(safesize);
			string s = Encoding.UTF8.GetString(bytes, offset, size);
			offset += safesize;
			return s;
		}

		public T ReadStruct<T>() where T : struct
		{
			int size = Unsafe.SizeOf<T>();
			int alignedSize = (size + 3) & (~3);
			CheckSize(alignedSize);
			T s = Unsafe.ReadUnaligned<T>(ref this.bytes[offset]);
			offset += alignedSize;
			return s;
		}
	}
}