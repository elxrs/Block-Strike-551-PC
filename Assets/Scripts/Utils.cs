using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Utils
{
	public static string test = string.Empty;

	public static Vector3 GetVector3(string vector)
	{
		string[] array = vector.Substring(1, vector.Length - 2).Split(',');
		float x = float.Parse(array[0]);
		float y = float.Parse(array[1]);
		float z = float.Parse(array[2]);
		return new Vector3(x, y, z);
	}

	public static Vector2 GetVector2(string vector)
	{
		string[] array = vector.Substring(1, vector.Length - 2).Split(',');
		float x = float.Parse(array[0]);
		float y = float.Parse(array[1]);
		return new Vector2(x, y);
	}

	public static GameObject AddChild(GameObject inst, GameObject parent, [Optional] Vector3 position, [Optional] Quaternion rotation)
	{
		return AddChild(inst, parent.transform, position, rotation);
	}

	public static GameObject AddChild(GameObject inst, Transform parent, [Optional] Vector3 position, [Optional] Quaternion rotation)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(inst, Vector3.zero, Quaternion.identity) as GameObject;
		gameObject.transform.SetParent(parent);
		gameObject.transform.localPosition = position;
		gameObject.transform.localRotation = rotation;
		gameObject.transform.localScale = Vector3.one;
		gameObject.name = gameObject.name.Replace("(Clone)", string.Empty);
		return gameObject;
	}

	public static Vector3 GetRagdollForce(Vector3 playerPosition, Vector3 attackPosition)
	{
		Vector3 result = playerPosition - attackPosition;
		result.x = Mathf.Clamp(result.x, -1f, 1f);
		result.y = Mathf.Clamp(result.y, -1f, 1f);
		result.z = Mathf.Clamp(result.z, -1f, 1f);
		return result;
	}

	public static string GetTeamHexColor(PhotonPlayer player)
	{
		return GetTeamHexColor(player.NickName, player.GetTeam());
	}

	public static string GetTeamHexColor(PhotonPlayer player, PhotonPlayer helper)
	{
		return GetTeamHexColor(player.NickName, player.GetTeam());
	}

	public static string GetTeamHexColor(string text, Team team)
	{
		switch (team)
		{
		case Team.Blue:
			text = "[4688E7]" + text + "[-]";
			break;
		case Team.Red:
			text = "[ED2C2D]" + text + "[-]";
			break;
		}
		return text;
	}

	public static string KillerStatus(DamageInfo damageInfo)
	{
		string empty = string.Empty;
		if (damageInfo.isPlayerID)
		{
			PhotonPlayer player = PhotonPlayer.Find(damageInfo.PlayerID);
			string teamHexColor = GetTeamHexColor(player, PhotonPlayer.Find(PlayerInput.PlayerHelperID));
			return teamHexColor + "   " + GetSpecialSymbol(damageInfo.WeaponID) + ((!damageInfo.HeadShot) ? string.Empty : ("   " + GetSpecialSymbol(99))) + "   " + GetTeamHexColor(PhotonNetwork.player);
		}
		return PhotonNetwork.player.NickName + " " + Localization.Get("died");
	}

	public static string GetSpecialSymbol(int weapon)
	{
		switch (weapon)
		{
		case 1:
			return 'ࠀ'.ToString();
		case 2:
			return 'ࠁ'.ToString();
		case 3:
			return 'ࠂ'.ToString();
		case 4:
			return 'ࠃ'.ToString();
		case 5:
			return 'ࠄ'.ToString();
		case 6:
			return 'ࠅ'.ToString();
		case 7:
			return 'ࠆ'.ToString();
		case 8:
			return 'ࠇ'.ToString();
		case 9:
			return 'ࠈ'.ToString();
		case 10:
			return 'ࠉ'.ToString();
		case 11:
			return 'ࠊ'.ToString();
		case 12:
			return 'ࠋ'.ToString();
		case 13:
			return 'ࠌ'.ToString();
		case 14:
			return 'ࠍ'.ToString();
		case 15:
			return 'ࠎ'.ToString();
		case 16:
			return 'ࠏ'.ToString();
		case 18:
			return 'ࠐ'.ToString();
		case 19:
			return 'ࠑ'.ToString();
		case 21:
			return 'ࠒ'.ToString();
		case 22:
			return 'ࠓ'.ToString();
		case 23:
			return 'ࠔ'.ToString();
		case 24:
			return 'ࠕ'.ToString();
		case 25:
			return '\u0816'.ToString();
		case 26:
			return '\u0817'.ToString();
		case 27:
			return '\u0818'.ToString();
		case 28:
			return '\u0819'.ToString();
		case 29:
			return 'ࠚ'.ToString();
		case 30:
			return '\u081b'.ToString();
		case 31:
			return '\u081c'.ToString();
		case 32:
			return '\u081d'.ToString();
		case 33:
			return '\u081e'.ToString();
		case 34:
			return '\u081f'.ToString();
		case 35:
			return '\u0820'.ToString();
		case 36:
			return '\u0821'.ToString();
		case 37:
			return '\u0822'.ToString();
		case 38:
			return '\u0823'.ToString();
		case 39:
			return 'ࠤ'.ToString();
		case 40:
			return '\u0825'.ToString();
		case 41:
			return '\u0826'.ToString();
		case 42:
			return '\u0827'.ToString();
		case 43:
			return 'ࠨ'.ToString();
		case 44:
			return '\u0829'.ToString();
		case 45:
			return '\u082a'.ToString();
		case 46:
			return '\u082b'.ToString();
		case 48:
			return '\u082c'.ToString();
		case 49:
			return '\u082d'.ToString();
		case 50:
			return '\u082e'.ToString();
		case 98:
			return '࡞'.ToString();
		case 99:
			return '\u085f'.ToString();
		default:
			return string.Empty;
		}
	}

	public static Color GetColor(int color)
	{
		switch (color)
		{
		case 0:
			return Color.white;
		case 1:
			return Color.red;
		case 2:
			return Color.yellow;
		case 3:
			return Color.green;
		case 4:
			return Color.cyan;
		case 5:
			return Color.blue;
		case 6:
			return Color.magenta;
		case 7:
			return Color.gray;
		case 8:
			return Color.black;
		case 9:
			return Color.clear;
		default:
			return Color.white;
		}
	}

	public static string ArrayToString(int[] array)
	{
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text = text + array[i] + "#";
		}
		return text;
	}

	public static int[] StringToArrayInt(string text)
	{
		string[] array = text.Split("#"[0]);
		int[] array2 = new int[array.Length - 1];
		for (int i = 0; i < array.Length - 1; i++)
		{
			array2[i] = int.Parse(array[i]);
		}
		return array2;
	}

	public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
	{
		value.x = Mathf.Clamp(value.x, min.x, max.x);
		value.y = Mathf.Clamp(value.y, min.y, max.y);
		value.z = Mathf.Clamp(value.z, min.z, max.z);
		return value;
	}

	public static string ColorToHex(Color32 color)
	{
		return "[" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + "]";
	}

	public static string ColorToHex(Color32 color, string text)
	{
		return "[" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + "]" + text + "[-]";
	}

	public static List<Vector3> GenerateBlockRoad(int maxBlocks)
	{
		List<Vector3> list = new List<Vector3>();
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < maxBlocks; i++)
		{
			int num = UnityEngine.Random.Range(1, 5);
			int num2 = UnityEngine.Random.Range(1, 5);
			int num3 = UnityEngine.Random.Range(1, 20);
			if (num >= 4)
			{
				zero.x += 1f;
			}
			if (num <= 3)
			{
				zero.x -= 1f;
			}
			if (num2 >= 4)
			{
				zero.z += 1f;
			}
			if (num2 <= 3)
			{
				zero.z -= 1f;
			}
			if (num3 == 1)
			{
				zero.y += 1f;
			}
			if (num3 == 19)
			{
				zero.y -= 1f;
			}
			if (num3 >= 6)
			{
				list.Add(zero);
			}
		}
		return list;
	}

	public static string NameGenerator(int line)
	{
		string empty = string.Empty;
		string[] array = new string[6] { "a", "e", "i", "o", "u", "y" };
		string[] array2 = new string[20]
		{
			"b", "c", "d", "f", "g", "h", "j", "k", "l", "m",
			"n", "p", "q", "r", "s", "t", "v", "w", "x", "z"
		};
		bool flag = false;
		if (UnityEngine.Random.value > 0.5f)
		{
			empty = array2[UnityEngine.Random.Range(0, array2.Length)];
		}
		else
		{
			empty = array[UnityEngine.Random.Range(0, array.Length)];
			flag = true;
		}
		empty = empty.ToUpper();
		for (int i = 0; i < line - 1; i++)
		{
			if (flag)
			{
				if (UnityEngine.Random.value < 0.2f)
				{
					empty += array[UnityEngine.Random.Range(0, array.Length)];
					continue;
				}
				empty += array2[UnityEngine.Random.Range(0, array2.Length)];
				flag = false;
			}
			else if (UnityEngine.Random.value < 0.2f)
			{
				empty += array2[UnityEngine.Random.Range(0, array2.Length)];
			}
			else
			{
				empty += array[UnityEngine.Random.Range(0, array.Length)];
				flag = true;
			}
		}
		return empty;
	}

	public static string MD5(string text)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(text);
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes);
		string text2 = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			text2 += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text2.PadLeft(32, '0');
	}

	public static byte[] SerializeWeaponStickers(int weapon, int skin)
	{
		AccountWeaponStickers weaponStickers = AccountManager.GetWeaponStickers(weapon, skin);
		if (weaponStickers == null)
		{
			return new byte[0];
		}
		byte[] array = new byte[weaponStickers.StickerData.Count * 2];
		int num = 0;
		for (int i = 0; i < weaponStickers.StickerData.Count; i++)
		{
			array[num] = (byte)(int)weaponStickers.StickerData[i].Index;
			num++;
			array[num] = (byte)(int)weaponStickers.StickerData[i].StickerID;
			num++;
		}
		return array;
	}

	private static AccountWeaponStickers DeserializeWeaponStickers(byte[] bytes)
	{
		AccountWeaponStickers accountWeaponStickers = new AccountWeaponStickers();
		List<AccountWeaponStickerData> list = new List<AccountWeaponStickerData>();
		int num = 0;
		for (int i = 0; i < bytes.Length / 2; i++)
		{
			AccountWeaponStickerData accountWeaponStickerData = new AccountWeaponStickerData();
			accountWeaponStickerData.Index = bytes[num];
			num++;
			accountWeaponStickerData.StickerID = bytes[num];
			num++;
			list.Add(accountWeaponStickerData);
		}
		accountWeaponStickers.StickerData = list;
		return accountWeaponStickers;
	}

	public static bool IsNullOrWhiteSpace(string value)
	{
		if (value == null)
		{
			return true;
		}
		if (value.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i].ToString() == " ")
			{
				return true;
			}
		}
		return false;
	}

	public static string XOR(string text)
	{
		return XOR(text, GameSettings.instance.Keys[0], false);
	}

	public static string XOR(string text, bool encrypt)
	{
		return XOR(text, GameSettings.instance.Keys[0], encrypt);
	}

	public static string XOR(string text, string key, bool encrypt)
	{
		if (encrypt)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			byte[] bytes2 = Encoding.UTF8.GetBytes(key);
			return Convert.ToBase64String(EncryptDecrypt(bytes, bytes2));
		}
		byte[] data = Convert.FromBase64String(text);
		byte[] bytes3 = Encoding.UTF8.GetBytes(key);
		return Encoding.UTF8.GetString(EncryptDecrypt(data, bytes3));
	}

	public static byte[] XOR(byte[] bytes)
	{
		byte[] bytes2 = Encoding.UTF8.GetBytes(GameSettings.instance.Keys[0]);
		return EncryptDecrypt(bytes, bytes2);
	}

	private static byte[] EncryptDecrypt(byte[] data, byte[] key)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		byte b = 0;
		byte[] array = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			array[i] = (byte)i;
		}
		for (int j = 0; j < 256; j++)
		{
			num3 = (num3 + array[j] + key[j % key.Length]) % 256;
			b = array[j];
			array[j] = array[num3];
			array[num3] = b;
		}
		byte[] array2 = new byte[data.Length];
		for (int k = 0; k < data.Length; k++)
		{
			num = (num + 1) % 256;
			num2 = (num2 + array[num]) % 256;
			b = array[num];
			array[num] = array[num2];
			array[num2] = b;
			array2[k] = (byte)(data[k] ^ array[(array[num] + array[num2]) % 256]);
		}
		return array2;
	}

	public static bool CompareVersion(string a, string b)
	{
		string[] array = a.Split("."[0]);
		string[] array2 = b.Split("."[0]);
		for (int i = 0; i < array.Length; i++)
		{
			if (toInt(array[i]) != toInt(array2[i]))
			{
				return (toInt(array[i]) < toInt(array2[i])) ? true : false;
			}
		}
		return false;
	}

	public static int toInt(string text)
	{
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			if (char.IsDigit(text[i]))
			{
				text2 += text[i];
			}
		}
		return int.Parse(text2);
	}

	public static CryptoInt[] ArrayIntToArrayCryptoInt(int[] array)
	{
		if (array == null)
		{
			return new CryptoInt[0];
		}
		CryptoInt[] array2 = new CryptoInt[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public static int[] ArrayCryptoIntToArrayInt(CryptoInt[] array)
	{
		if (array == null)
		{
			return new int[0];
		}
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}
}
