using Everlasting.Base;

namespace Everlasting.Config.Loader
{
	public interface IConfigBinary
	{
		int ReadInt();
		uint ReadUInt();
		long ReadLong();
		ulong ReadULong();
		float ReadFloat();
		bool ReadBool();
		string ReadString();
	}

	public class ConfigBinary : IConfigBinary
	{
		private readonly AlignedUnsafeMemoryStreamReader reader;
		public ConfigBinary(byte[] data)
		{
			reader = new AlignedUnsafeMemoryStreamReader(data);
		}
		public int ReadInt()
		{
			return reader.ReadInt();
		}

		public uint ReadUInt()
		{
			return reader.ReadUInt();
		}

		public long ReadLong()
		{
			return reader.ReadLong();
		}

		public ulong ReadULong()
		{
			return reader.ReadULong();
		}

		public float ReadFloat()
		{
			return reader.ReadFloat();
		}

		public bool ReadBool()
		{
			return reader.ReadBool();
		}

		public string ReadString()
		{
			return reader.ReadString();
		}
	}
}