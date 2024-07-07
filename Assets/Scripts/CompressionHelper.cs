using System;
using System.IO;
using System.Text;

public static class CompressionHelper
{
	public struct OutputBuffer
	{
		public byte[] bytes;

		public int Length
		{
			get
			{
				return bytes.Length;
			}
		}

		public static implicit operator OutputBuffer(byte[] input)
		{
			OutputBuffer result = default(OutputBuffer);
			result.bytes = input;
			return result;
		}
	}

	private static readonly uint HLOG = 14u;

	private static readonly uint HSIZE = 16384u;

	private static readonly uint MAX_LIT = 32u;

	private static readonly uint MAX_OFF = 8192u;

	private static readonly uint MAX_REF = 264u;

	private static readonly long[] HashTable = new long[HSIZE];

	public static byte[] CompressBytes(byte[] input)
	{
		return ActOnBytes(input, LZFCompress);
	}

	public static byte[] DecompressBytes(byte[] input)
	{
		return ActOnBytes(input, LZFDecompress);
	}

	private static byte[] ActOnBytes(byte[] inputBytes, Func<byte[], OutputBuffer, int> act)
	{
		int num = inputBytes.Length;
		int num2 = 0;
		OutputBuffer arg;
		do
		{
			num *= 2;
			arg = new byte[num];
			num2 = act(inputBytes, arg);
		}
		while (num2 == 0);
		byte[] array = new byte[num2];
		Buffer.BlockCopy(arg.bytes, 0, array, 0, num2);
		return array;
	}

	public static string CompressString(string input)
	{
		return ActOnString(input, CompressBytes);
	}

	public static string DecompressString(string input)
	{
		return ActOnString(input, DecompressBytes);
	}

	private static string ActOnString(string input, Func<byte[], byte[]> act)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(input);
		byte[] bytes2 = act(bytes);
		return Encoding.Unicode.GetString(bytes2);
	}

	public static void CompressFile(string path)
	{
		ActOnFile(path, CompressBytes);
	}

	public static void DecompressFile(string path)
	{
		ActOnFile(path, DecompressBytes);
	}

	private static void ActOnFile(string path, Func<byte[], byte[]> act)
	{
		byte[] arg = File.ReadAllBytes(path);
		byte[] bytes = act(arg);
		File.WriteAllBytes(path, bytes);
	}

	public static int LZFCompress(byte[] input, OutputBuffer buffer)
	{
		byte[] bytes = buffer.bytes;
		int num = input.Length;
		int num2 = bytes.Length;
		Array.Clear(HashTable, 0, (int)HSIZE);
		uint num3 = 0u;
		uint num4 = 0u;
		uint num5 = (uint)((input[num3] << 8) | input[num3 + 1]);
		int num6 = 0;
		while (true)
		{
			if (num3 < num - 2)
			{
				num5 = (num5 << 8) | input[num3 + 2];
				long num7 = ((num5 ^ (num5 << 5)) >> (int)(24 - HLOG - num5 * 5)) & (HSIZE - 1);
				long num8 = HashTable[num7];
				HashTable[num7] = num3;
				long num9;
				if ((num9 = num3 - num8 - 1) < MAX_OFF && num3 + 4 < num && num8 > 0 && input[num8] == input[num3] && input[num8 + 1] == input[num3 + 1] && input[num8 + 2] == input[num3 + 2])
				{
					uint num10 = 2u;
					uint num11 = (uint)(num - (int)num3) - num10;
					num11 = ((num11 <= MAX_REF) ? num11 : MAX_REF);
					if (num4 + num6 + 1 + 3 >= num2)
					{
						return 0;
					}
					do
					{
						num10++;
					}
					while (num10 < num11 && input[num8 + num10] == input[num3 + num10]);
					if (num6 != 0)
					{
						bytes[num4++] = (byte)(num6 - 1);
						num6 = -num6;
						do
						{
							bytes[num4++] = input[num3 + num6];
						}
						while (++num6 != 0);
					}
					num10 -= 2;
					num3++;
					if (num10 < 7)
					{
						bytes[num4++] = (byte)((num9 >> 8) + (num10 << 5));
					}
					else
					{
						bytes[num4++] = (byte)((num9 >> 8) + 224);
						bytes[num4++] = (byte)(num10 - 7);
					}
					bytes[num4++] = (byte)num9;
					num3 += num10 - 1;
					num5 = (uint)((input[num3] << 8) | input[num3 + 1]);
					num5 = (num5 << 8) | input[num3 + 2];
					HashTable[((num5 ^ (num5 << 5)) >> (int)(24 - HLOG - num5 * 5)) & (HSIZE - 1)] = num3;
					num3++;
					num5 = (num5 << 8) | input[num3 + 2];
					HashTable[((num5 ^ (num5 << 5)) >> (int)(24 - HLOG - num5 * 5)) & (HSIZE - 1)] = num3;
					num3++;
					continue;
				}
			}
			else if (num3 == num)
			{
				break;
			}
			num6++;
			num3++;
			if (num6 == MAX_LIT)
			{
				if (num4 + 1 + MAX_LIT >= num2)
				{
					return 0;
				}
				bytes[num4++] = (byte)(MAX_LIT - 1);
				num6 = -num6;
				do
				{
					bytes[num4++] = input[num3 + num6];
				}
				while (++num6 != 0);
			}
		}
		if (num6 != 0)
		{
			if (num4 + num6 + 1 >= num2)
			{
				return 0;
			}
			bytes[num4++] = (byte)(num6 - 1);
			num6 = -num6;
			do
			{
				bytes[num4++] = input[num3 + num6];
			}
			while (++num6 != 0);
		}
		return (int)num4;
	}

	public static int LZFDecompress(byte[] input, OutputBuffer buffer)
	{
		byte[] bytes = buffer.bytes;
		int num = input.Length;
		int num2 = bytes.Length;
		uint num3 = 0u;
		uint num4 = 0u;
		do
		{
			uint num5 = input[num3++];
			if (num5 < 32)
			{
				num5++;
				if (num4 + num5 > num2)
				{
					return 0;
				}
				do
				{
					bytes[num4++] = input[num3++];
				}
				while (--num5 != 0);
				continue;
			}
			uint num6 = num5 >> 5;
			int num7 = (int)(num4 - ((num5 & 0x1F) << 8) - 1);
			if (num6 == 7)
			{
				num6 += input[num3++];
			}
			num7 -= input[num3++];
			if (num4 + num6 + 2 > num2)
			{
				return 0;
			}
			if (num7 < 0)
			{
				return 0;
			}
			bytes[num4++] = bytes[num7++];
			bytes[num4++] = bytes[num7++];
			do
			{
				bytes[num4++] = bytes[num7++];
			}
			while (--num6 != 0);
		}
		while (num3 < num);
		return (int)num4;
	}
}
