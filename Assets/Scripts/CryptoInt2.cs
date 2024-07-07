using System;
using UnityEngine;

[Serializable]
public struct CryptoInt2 : IFormattable, IEquatable<CryptoInt2>
{
	[SerializeField]
	private int cryptoKey;

	[SerializeField]
	private byte[] hiddenValue;

	[SerializeField]
	private byte[] fakeValue;

	[SerializeField]
	private bool inited;

	private CryptoInt2(int value)
	{
		do
		{
			cryptoKey = CryptoManager.random.Next(int.MinValue, int.MaxValue);
		}
		while (cryptoKey == 0);
		hiddenValue = AesEncryptor.Encrypt(BitConverter.GetBytes(value ^ cryptoKey));
		for (int i = 0; i < hiddenValue.Length; i++)
		{
			hiddenValue[i] = (byte)(hiddenValue[i] ^ cryptoKey);
		}
		fakeValue = BitConverter.GetBytes(value ^ cryptoKey);
		inited = true;
	}

	public void SetValue(int value)
	{
		do
		{
			cryptoKey = CryptoManager.random.Next(int.MinValue, int.MaxValue);
		}
		while (cryptoKey == 0);
		hiddenValue = AesEncryptor.Encrypt(BitConverter.GetBytes(value ^ cryptoKey));
		for (int i = 0; i < hiddenValue.Length; i++)
		{
			hiddenValue[i] = (byte)(hiddenValue[i] ^ cryptoKey);
		}
		fakeValue = BitConverter.GetBytes(value ^ cryptoKey);
	}

	public void UpdateValue()
	{
		SetValue(GetValue());
	}

	private int GetValue()
	{
		if (!inited)
		{
			do
			{
				cryptoKey = CryptoManager.random.Next(int.MinValue, int.MaxValue);
			}
			while (cryptoKey == 0);
			hiddenValue = AesEncryptor.Encrypt(BitConverter.GetBytes(cryptoKey));
			for (int i = 0; i < hiddenValue.Length; i++)
			{
				hiddenValue[i] = (byte)(hiddenValue[i] ^ cryptoKey);
			}
			fakeValue = BitConverter.GetBytes(cryptoKey);
			inited = true;
		}
		byte[] array = new byte[hiddenValue.Length];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = (byte)(hiddenValue[j] ^ cryptoKey);
		}
		array = AesEncryptor.Decrypt(array);
		int num = BitConverter.ToInt32(array, 0) ^ cryptoKey;
		if (num != (BitConverter.ToInt32(fakeValue, 0) ^ cryptoKey) || cryptoKey == 0)
		{
			CryptoManager.CheatingDetected();
			return 0;
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CryptoInt2))
		{
			return false;
		}
		return Equals((CryptoInt2)obj);
	}

	public bool Equals(CryptoInt2 obj)
	{
		return GetValue() == obj.GetValue();
	}

	public override int GetHashCode()
	{
		return GetValue().GetHashCode();
	}

	public override string ToString()
	{
		return GetValue().ToString();
	}

	public string ToString(string format)
	{
		return GetValue().ToString(format);
	}

	public string ToString(IFormatProvider provider)
	{
		return GetValue().ToString(provider);
	}

	public string ToString(string format, IFormatProvider provider)
	{
		return GetValue().ToString(format, provider);
	}

	public static implicit operator CryptoInt2(int value)
	{
		return new CryptoInt2(value);
	}

	public static implicit operator int(CryptoInt2 value)
	{
		return value.GetValue();
	}

	public static CryptoInt2 operator ++(CryptoInt2 value)
	{
		value.SetValue(value.GetValue() + 1);
		return value;
	}

	public static CryptoInt2 operator --(CryptoInt2 value)
	{
		value.SetValue(value.GetValue() - 1);
		return value;
	}
}
