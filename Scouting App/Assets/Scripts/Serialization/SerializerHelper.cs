using System;
using System.IO;
using System.Text;

namespace ScoutingApp
{
	public class SerializerHelper
	{
		public static void WriteString(BinaryWriter writer, string s)
		{
			byte[] data = Encoding.UTF8.GetBytes(s);
			if (data.Length > ushort.MaxValue)
				throw new ArgumentException("String is too long, greater than " + ushort.MaxValue.ToString() + " bytes!");
			writer.Write((ushort)data.Length);
			writer.Write(data);
		}

		public static string ReadString(BinaryReader reader)
		{
			ushort len = reader.ReadUInt16();
			byte[] data = reader.ReadBytes(len);
			return Encoding.UTF8.GetString(data);
		}

		public static byte[] PackBools(params bool[] bools)
		{
			int len = (int)Math.Ceiling(bools.Length / 8F);
			byte[] bytes = new byte[len];

			for (int i = 0; i < bools.Length; i++)
				if (bools[i])
					bytes[i / 8] |= (byte)(1 << (i % 8));

			return bytes;
		}

		public static bool[] UnpackBools(byte[] bytes, int count)
		{
			bool[] bools = new bool[count];

			for (int i = 0; i < count; i++)
				bools[i] = ((bytes[i / 8] >> (i % 8)) & 0x1) == 1;

			return bools;
		}
	}

	public abstract class BaseSerializableData
	{
		public abstract void Serialize(BinaryWriter writer);
		public abstract void Deserialize(BinaryReader reader, bool value1 = false);
	}
}
