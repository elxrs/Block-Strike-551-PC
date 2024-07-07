using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using FreeJSON;

public class LevelManager
{
	public static bool CustomMap = false;

	private static Dictionary<GameMode, List<string>> SceneList = new Dictionary<GameMode, List<string>>();

	private static List<string> AllScenes = new List<string>();

	public static void LoadFile()
	{
		if (SceneList.Count != 0)
		{
			return;
		}
		TextAsset textAsset = Resources.Load("Others/SceneManager") as TextAsset;
		JsonArray jsonArray = JsonArray.Parse(Utils.XOR(textAsset.text));
		for (int i = 0; i < jsonArray.Length; i++)
		{
			GameMode key = jsonArray.Get<JsonObject>(i).Get<GameMode>("GameMode");
			List<string> list = new List<string>();
			JsonArray jsonArray2 = jsonArray.Get<JsonObject>(i).Get<JsonArray>("Scenes");
			for (int j = 0; j < jsonArray2.Length; j++)
			{
				list.Add(jsonArray2.Get<string>(j));
				if (!AllScenes.Contains(jsonArray2.Get<string>(j)))
				{
					AllScenes.Add(jsonArray2.Get<string>(j));
				}
			}
			SceneList.Add(key, list);
		}
	}

	public static List<string> GetGameModeScenes(GameMode mode)
	{
		LoadFile();
		return SceneList[mode];
	}

	public static List<string> GetAllScenes()
	{
		LoadFile();
		return AllScenes;
	}

	public static bool HasScene(string scene)
	{
		LoadFile();
		return AllScenes.Contains(scene);
	}

	public static string GetNextScene(GameMode mode)
	{
		return GetNextScene(mode, GetSceneName());
	}

	public static string GetNextScene(GameMode mode, string scene)
	{
		LoadFile();
		if (CustomMap)
		{
			return scene;
		}
		List<string> list = SceneList[mode];
		for (int i = 0; i < list.Count; i++)
		{
			if (scene == list[i])
			{
				if (list.Count - 1 == i)
				{
					return list[0];
				}
				return list[i + 1];
			}
		}
		return string.Empty;
	}

	public static bool HasSceneInGameMode(GameMode mode)
	{
		return HasSceneInGameMode(mode, GetSceneName());
	}

	public static bool HasSceneInGameMode(GameMode mode, string sceneName)
	{
		List<string> gameModeScenes = GetGameModeScenes(mode);
		return gameModeScenes.Contains(sceneName);
	}

	public static string GetSceneName()
	{
		return SceneManager.GetActiveScene().name;
	}

	private static string GetEncryptSceneName(string name)
	{
		string text = Utils.XOR(name, Utils.test, true);
		return text.Replace("/", "#");
	}

	public static void LoadLevel(string name)
	{
		SceneManager.LoadScene(name);
	}
}
