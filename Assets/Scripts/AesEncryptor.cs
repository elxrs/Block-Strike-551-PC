using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Beebyte.Obfuscator;

[SkipRename]
public static class AesEncryptor
{
	private const int keySize = 16;

	private const string keyString = "defaultKeyString";

	private const int IvLength = 16;

	private static UTF8Encoding encoder;

	private static AesManaged aes;

	static AesEncryptor()
	{
		encoder = new UTF8Encoding();
		aes = new AesManaged
		{
			Key = encoder.GetBytes("defaultKeyString").Take(16).ToArray()
		};
		aes.BlockSize = 128;
	}

	public static byte[] GenerateIV()
	{
		aes.GenerateIV();
		return aes.IV;
	}

	public static byte[] Encrypt(byte[] buffer)
	{
		aes.GenerateIV();
		using (ICryptoTransform cryptoTransform = aes.CreateEncryptor())
		{
			byte[] second = cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
			return aes.IV.Concat(second).ToArray();
		}
	}

	public static byte[] Decrypt(byte[] buffer)
	{
		byte[] rgbIV = buffer.Take(16).ToArray();
		using (ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, rgbIV))
		{
			return cryptoTransform.TransformFinalBlock(buffer, 16, buffer.Length - 16);
		}
	}

	public static byte[] EncryptIV(byte[] buffer, byte[] IV)
	{
		return EncryptKeyIV(buffer, aes.Key, IV);
	}

	public static byte[] DecryptIV(byte[] buffer, byte[] IV)
	{
		return DecryptKeyIV(buffer, aes.Key, IV);
	}

	public static byte[] EncryptKeyIV(byte[] buffer, byte[] key, byte[] IV)
	{
		using (ICryptoTransform cryptoTransform = aes.CreateEncryptor(key, IV))
		{
			return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
		}
	}

	public static byte[] DecryptKeyIV(byte[] buffer, byte[] key, byte[] IV)
	{
		using (ICryptoTransform cryptoTransform = aes.CreateDecryptor(key, IV))
		{
			return cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
		}
	}

	public static string Encrypt(string unencrypted)
	{
		return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
	}

	[Obsolete("Decrypt(string) has been made obsolete. Please use the DecryptString(string).")]
	public static string Decrypt(string encrypted)
	{
		return DecryptString(encrypted);
	}

	[SkipRename]
	public static string DecryptString(string encrypted)
	{
		return DecryptString(Convert.FromBase64String(encrypted));
	}

	public static string DecryptString(byte[] encrypted)
	{
		byte[] array = Decrypt(encrypted);
		return encoder.GetString(array, 0, array.Length);
	}

	public static string EncryptIV(string unencrypted, byte[] vector)
	{
		return Convert.ToBase64String(EncryptIV(encoder.GetBytes(unencrypted), vector));
	}

	public static string DecryptIV(string encrypted, byte[] vector)
	{
		byte[] array = DecryptIV(Convert.FromBase64String(encrypted), vector);
		return encoder.GetString(array, 0, array.Length);
	}

	public static string Encrypt(bool unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static bool DecryptBool(string encrypted)
	{
		return DecryptBool(Convert.FromBase64String(encrypted));
	}

	public static bool DecryptBool(byte[] encrypted)
	{
		return BitConverter.ToBoolean(Decrypt(encrypted), 0);
	}

	public static string Encrypt(char unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static char DecryptChar(string encrypted)
	{
		return DecryptChar(Convert.FromBase64String(encrypted));
	}

	public static char DecryptChar(byte[] encrypted)
	{
		return BitConverter.ToChar(Decrypt(encrypted), 0);
	}

	public static string Encrypt(double unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static double DecryptDouble(string encrypted)
	{
		return DecryptDouble(Convert.FromBase64String(encrypted));
	}

	public static double DecryptDouble(byte[] encrypted)
	{
		return BitConverter.ToDouble(Decrypt(encrypted), 0);
	}

	public static string Encrypt(float unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static float DecryptFloat(string encrypted)
	{
		return DecryptFloat(Convert.FromBase64String(encrypted));
	}

	public static float DecryptFloat(byte[] encrypted)
	{
		return BitConverter.ToSingle(Decrypt(encrypted), 0);
	}

	public static string Encrypt(int unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static int DecryptInt(string encrypted)
	{
		return DecryptInt(Convert.FromBase64String(encrypted));
	}

	public static int DecryptInt(byte[] encrypted)
	{
		return BitConverter.ToInt32(Decrypt(encrypted), 0);
	}

	public static string Encrypt(long unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static long DecryptLong(string encrypted)
	{
		return DecryptLong(Convert.FromBase64String(encrypted));
	}

	public static long DecryptLong(byte[] encrypted)
	{
		return BitConverter.ToInt64(Decrypt(encrypted), 0);
	}

	public static string Encrypt(short unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static short DecryptShort(string encrypted)
	{
		return DecryptShort(Convert.FromBase64String(encrypted));
	}

	public static short DecryptShort(byte[] encrypted)
	{
		return BitConverter.ToInt16(Decrypt(encrypted), 0);
	}

	public static string Encrypt(uint unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static uint DecryptUInt(string encrypted)
	{
		return DecryptUInt(Convert.FromBase64String(encrypted));
	}

	public static uint DecryptUInt(byte[] encrypted)
	{
		return BitConverter.ToUInt32(Decrypt(encrypted), 0);
	}

	public static string Encrypt(ulong unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static ulong DecryptULong(string encrypted)
	{
		return DecryptULong(Convert.FromBase64String(encrypted));
	}

	public static ulong DecryptULong(byte[] encrypted)
	{
		return BitConverter.ToUInt64(Decrypt(encrypted), 0);
	}

	public static string Encrypt(ushort unencrypted)
	{
		return Convert.ToBase64String(Encrypt(BitConverter.GetBytes(unencrypted)));
	}

	public static ushort DecryptUShort(string encrypted)
	{
		return DecryptUShort(Convert.FromBase64String(encrypted));
	}

	public static ushort DecryptUShort(byte[] encrypted)
	{
		return BitConverter.ToUInt16(Decrypt(encrypted), 0);
	}
}
