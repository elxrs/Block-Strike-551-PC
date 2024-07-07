using System;
using System.Collections.Generic;
using UnityEngine;

public class mCreateServer : MonoBehaviour
{
	public GameMode SelectMode;

	public UIPopupList SelectModePopupList;

	public UIPopupList SelectMapPopupList;

	public UIInput ServerName;

	public GameObject SelectedMaxPlayers;

	public int MaxPlayers = 4;

	public UILabel[] MaxPlayersList;

	public UIInput Password;

	public GameObject CustomMaps;

	private bool defaultMaxPlayers = true;

	private static mCreateServer instance;

	private void Start()
	{
		instance = this;
	}

	public void Open()
	{
		CustomMapManager.Init();
		CustomMaps.SetActive(SaveLoadManager.GetPlayerLevel() >= 10 && CustomMapManager.HasCustomMaps());
		ServerName.value = "Room " + UnityEngine.Random.Range(0, 99999);
		SelectModePopupList.Clear();
		GameMode[] gameModeList = GameModeManager.GetGameModeList();
		for (int i = 0; i < gameModeList.Length; i++)
		{
			SelectModePopupList.AddItem(gameModeList[i].ToString());
		}
		SelectModePopupList.value = SelectModePopupList.items[0];
		UpdateMaps();
	}

	public static void OpenPanel()
	{
		instance.Open();
	}

	private void UpdateMaps()
	{
		List<string> gameModeScenes = LevelManager.GetGameModeScenes(SelectMode);
		SelectMapPopupList.Clear();
		for (int i = 0; i < gameModeScenes.Count; i++)
		{
			SelectMapPopupList.AddItem(gameModeScenes[i]);
		}
		SelectMapPopupList.value = SelectMapPopupList.items[0];
	}

	public void OnSelectGameMode()
	{
		SelectMode = (GameMode)(int)Enum.Parse(typeof(GameMode), SelectModePopupList.value);
		UpdateMaps();
		mServerSettings.Check(SelectMode, GetMap());
	}

	public void OnSelectMap()
	{
		mServerSettings.Check(SelectMode, GetMap());
	}

	public void OnCheckServerName()
	{
		if (ServerName.value.Length < 4 || Utils.IsNullOrWhiteSpace(ServerName.value) || BadWordsManager.Contains(ServerName.value))
		{
			ServerName.value = "Room " + UnityEngine.Random.Range(0, 99999);
		}
		ServerName.value = NGUIText.StripSymbols(ServerName.value);
		RoomInfo[] roomList = PhotonNetwork.GetRoomList();
		for (int i = 0; i < roomList.Length; i++)
		{
			if (roomList[i].Name == ServerName.value)
			{
				ServerName.value = "Room " + UnityEngine.Random.Range(0, 99999);
				UIToast.Show(Localization.Get("Name already taken"));
				break;
			}
		}
	}

	public void SetMaxPlayer(GameObject go)
	{
		MaxPlayers = int.Parse(go.name);
		TweenPosition.Begin(SelectedMaxPlayers, 0.2f, go.transform.localPosition);
	}

	public void SetDefaultMaxPlayers()
	{
		if (!defaultMaxPlayers)
		{
			SetMaxPlayers(new int[5] { 4, 6, 8, 10, 12 });
			defaultMaxPlayers = true;
		}
	}

	public void SetMaxPlayers(int[] list)
	{
		if (list.Length > 5)
		{
			Debug.LogError("Max list  <=5");
			return;
		}
		defaultMaxPlayers = false;
		for (int i = 0; i < MaxPlayersList.Length; i++)
		{
			MaxPlayersList[i].cachedGameObject.SetActive(false);
		}
		for (int j = 0; j < list.Length; j++)
		{
			MaxPlayersList[j].cachedGameObject.SetActive(true);
			MaxPlayersList[j].cachedGameObject.name = list[j].ToString();
			MaxPlayersList[j].text = list[j].ToString();
		}
		SetMaxPlayer(MaxPlayersList[0].cachedGameObject);
	}

	public static GameMode GetGameMode()
	{
		return instance.SelectMode;
	}

	public static string GetMap()
	{
		return instance.SelectMapPopupList.value;
	}

	public static string GetServerName()
	{
		return instance.ServerName.value;
	}

	public static int GetMaxPlayers()
	{
		return instance.MaxPlayers;
	}

	public static string GetPassword()
	{
		return instance.Password.value;
	}

	public void CreateServer()
	{
		mPhotonSettings.OnCreateServer();
	}
}
