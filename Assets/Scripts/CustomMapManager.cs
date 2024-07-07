using System.Collections.Generic;
using System.IO;
using CodeStage.AntiCheat.Utils;
using UnityEngine;

public static class CustomMapManager
{
	public static bool enabled = true;

	public static int hash;

	public static string mapUrl;

	private static AssetBundle bundle;

	private static Dictionary<GameMode, List<string>> SceneList = new Dictionary<GameMode, List<string>>();

	private static string DirectoryPath = AndroidNativeFunctions.GetAbsolutePath() + "/Block Strike/Custom Maps";

	public static void Init()
	{
		if (!enabled)
		{
			return;
		}
		CheckDirectoryPath();
		SceneList = new Dictionary<GameMode, List<string>>();
		string[] files = Directory.GetFiles(DirectoryPath, "*.bsm");
		for (int i = 0; i < files.Length; i++)
		{
			if (!CheckFileName(files[i]) || new FileInfo(files[i]).Length > 6000000)
			{
				continue;
			}
			byte[] bytes = File.ReadAllBytes(files[i]);
			GameMode[] gameModes = GetGameModes(bytes);
			for (int j = 0; j < gameModes.Length; j++)
			{
				if (!SceneList.ContainsKey(gameModes[j]))
				{
					SceneList.Add(gameModes[j], new List<string>());
				}
				SceneList[gameModes[j]].Add(files[i]);
			}
		}
	}

	private static bool CheckFileName(string path)
	{
		if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(path)))
		{
			return false;
		}
		if (LevelManager.HasScene(Path.GetFileNameWithoutExtension(path)))
		{
			return false;
		}
		return true;
	}

	public static bool HasCustomMaps()
	{
		return (SceneList.Count != 0) ? true : false;
	}

	public static string[] GetMapsList(GameMode mode)
	{
		if (!SceneList.ContainsKey(mode))
		{
			return new string[0];
		}
		return SceneList[mode].ToArray();
	}

	public static void Load(string path)
	{
		byte[] array = File.ReadAllBytes(path);
		hash = (int)(xxHash.CalculateHash(array, array.Length, 0u) - int.MaxValue);
		array = GetAssetBundle(array);
		string path2 = string.Concat(Directory.GetParent(path), "/", Path.GetFileNameWithoutExtension(path), ".txt");
		if (File.Exists(path2))
		{
			mapUrl = File.ReadAllText(path2);
			if (mapUrl.Length > 50)
			{
				mapUrl = string.Empty;
			}
		}
		Unload();
		bundle = AssetBundle.LoadFromMemory(array);
	}

	public static void Unload()
	{
		if (bundle != null)
		{
			bundle.Unload(true);
		}
	}

	public static string GetMapName(string path)
	{
		return Path.GetFileNameWithoutExtension(path);
	}

	public static int GetMapHash(string name)
	{
		if (File.Exists(DirectoryPath + "/" + name + ".bsm"))
		{
			byte[] array = File.ReadAllBytes(DirectoryPath + "/" + name + ".bsm");
			return (int)(xxHash.CalculateHash(array, array.Length, 0u) - int.MaxValue);
		}
		return 0;
	}

	public static string GetMapPath(string name)
	{
		if (File.Exists(DirectoryPath + "/" + name + ".bsm"))
		{
			return DirectoryPath + "/" + name + ".bsm";
		}
		return string.Empty;
	}

	public static string SaveMap(string name, string url, byte[] map)
	{
		CheckDirectoryPath();
		File.WriteAllBytes(DirectoryPath + "/" + name + ".bsm", map);
		File.WriteAllText(DirectoryPath + "/" + name + ".txt", url);
		return DirectoryPath + "/" + name + ".bsm";
	}

	public static bool DeleteMap(string name)
	{
		if (File.Exists(DirectoryPath + "/" + name + ".bsm"))
		{
			File.Delete(DirectoryPath + "/" + name + ".bsm");
			File.Delete(DirectoryPath + "/" + name + ".txt");
			Init();
			return true;
		}
		return false;
	}

	private static void CheckDirectoryPath()
	{
		if (!Directory.Exists(DirectoryPath))
		{
			Directory.CreateDirectory(DirectoryPath);
		}
	}

	private static bool HasGameMode(byte[] bytes, GameMode mode)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < 20 && bytes[i] != 250; i++)
		{
			if (bytes[i] != 205)
			{
				list.Add(bytes[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == (byte)mode)
			{
				return true;
			}
		}
		return false;
	}

	private static GameMode[] GetGameModes(byte[] bytes)
	{
		List<GameMode> list = new List<GameMode>();
		for (int i = 0; i < 20 && bytes[i] != 250; i++)
		{
			if (bytes[i] != 205)
			{
				list.Add((GameMode)bytes[i]);
			}
		}
		return list.ToArray();
	}

	private static byte[] GetAssetBundle(byte[] bytes)
	{
		List<byte> list = new List<byte>();
		bool flag = false;
		for (int i = 0; i < bytes.Length; i++)
		{
			if (flag)
			{
				list.Add(bytes[i]);
			}
			else if (bytes[i] == 250)
			{
				flag = true;
			}
		}
		return list.ToArray();
	}
}
