using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mServerList : MonoBehaviour
{
	private int SelectMode = -1;

	public UILabel ServerInfoLabel;

	public UIPopupList ModePopupList;

	public GameObject ServerListElement;

	public GameObject ServerListSwitch;

	public UILabel ServerListSwitchLabel;

	public GameObject ServerListSwitch2;

	public UILabel ServerListSwitchLabel2;

	public UIInput SearchInput;

	public GameObject ServerListParent;

	public GameObject SettingsPanel;

	public UIToggle FullServersToogle;

	public UIToggle PasswordServersToogle;

	public UIToggle SortServersToogle;

	public UIPopupList CustomMapsPopupList;

	public UIPopupList ScenesPopupList;

	private string SelectMap;

	private bool activeSettingsPanel;

	public bool showFullServers = true;

	public bool showPasswordServers = true;

	public bool sortServers;

	private List<GameObject> ServerList = new List<GameObject>();

	private List<GameObject> ServerListPool = new List<GameObject>();

	private bool isCreatingServerList;

	private int MaxPlayers;

	private int MaxServers;

	private int MaxPlayersTotal;

	private int MaxServersTotal;

	private RoomInfo[] RoomList;

	private int SelectAllModeList = 1;

	private int MaxAllModeList;

	private void Start()
	{
		showFullServers = PlayerPrefs.GetInt("showFullServers", 1) == 1;
		showPasswordServers = PlayerPrefs.GetInt("showPasswordServers", 1) == 1;
		sortServers = PlayerPrefs.GetInt("sortServers") == 1;
	}

	public void Open()
	{
		ModePopupList.Clear();
		ModePopupList.AddItem("All", -1);
		GameMode[] gameModeList = GameModeManager.GetGameModeList();
		for (int i = 0; i < gameModeList.Length; i++)
		{
			ModePopupList.AddItem(gameModeList[i].ToString(), (int)gameModeList[i]);
		}
		ModePopupList.value = "All";
		ScenesPopupList.Clear();
		ScenesPopupList.AddItem(Localization.Get("All"), -1);
		List<string> allScenes = LevelManager.GetAllScenes();
		allScenes.Remove("Meeting");
		allScenes.Remove("Build Battle");
		allScenes.Remove("Dropper");
		allScenes.Remove("Office (Beta)");
		for (int j = 0; j < allScenes.Count; j++)
		{
			ScenesPopupList.AddItem(allScenes[j], j);
		}
		ScenesPopupList.value = Localization.Get("All");
		CustomMapsPopupList.Clear();
		CustomMapsPopupList.AddItem("All", -1);
		CustomMapsPopupList.AddItem("Official Maps", 0);
		CustomMapsPopupList.AddItem("Custom Maps", 1);
		CustomMapsPopupList.value = "Official Maps";
	}

	public void UpdateServerList()
	{
		MaxAllModeList = 0;
		SelectAllModeList = 1;
		if (!isCreatingServerList)
		{
			ServerListParent.GetComponent<UIScrollView>().ResetPosition();
			StartCoroutine(CreateServerList(true));
		}
	}

	private IEnumerator CreateServerList(bool updateRoomList)
	{
		isCreatingServerList = true;
		yield return new WaitForSeconds(0.01f);
		ClearServerList();
		MaxPlayers = 0;
		MaxServers = 0;
		MaxPlayersTotal = 0;
		MaxServersTotal = 0;
		if (updateRoomList)
		{
			RoomList = PhotonNetwork.GetRoomList();
		}
		RoomInfo[] roomList = GetRooms();
		int count = -1;
		for (int j = 0; j < roomList.Length; j++)
		{
			MaxServers++;
			MaxPlayers += roomList[j].PlayerCount;
		}
		ServerListSwitch2.SetActive(false);
		UpdateServerInfo();
		if (roomList.Length / 30 >= 1)
		{
			if (MaxAllModeList == 0)
			{
				MaxAllModeList = Mathf.CeilToInt(roomList.Length / 30f);
			}
			ServerListSwitch.SetActive(true);
			ServerListSwitch.transform.localPosition = Vector3.up * 150f;
			int startIndex = (SelectAllModeList - 1) * 30;
			int maxLength = 30;
			if (SelectAllModeList * 30 > roomList.Length)
			{
				maxLength = roomList.Length - (SelectAllModeList - 1) * 30;
			}
			ServerListSwitchLabel.text = startIndex + 1 + "-" + (startIndex + maxLength);
			ServerListSwitchLabel2.text = startIndex + 1 + "-" + (startIndex + maxLength);
			for (int i = startIndex; i < startIndex + maxLength; i++)
			{
				Transform element = GetElement();
				count++;
				element.GetComponent<mServerInfo>().SetData(roomList[i]);
				element.localPosition = Vector3.up * (102 - 48 * count);
				element.gameObject.SetActive(true);
				ServerList.Add(element.gameObject);
				yield return new WaitForSeconds(0.01f);
			}
			ServerListSwitch2.SetActive(true);
			ServerListSwitch2.transform.localPosition = Vector3.up * (102 - 48 * (count + 1));
		}
		else
		{
			ServerListSwitch.SetActive(false);
			for (int k = 0; k < roomList.Length; k++)
			{
				if (SelectMode == -1 || SelectMode == (int)roomList[k].GetGameMode())
				{
					Transform element2 = GetElement();
					count++;
					element2.GetComponent<mServerInfo>().SetData(roomList[k]);
					element2.localPosition = Vector3.up * (150 - 48 * count);
					element2.gameObject.SetActive(true);
					ServerList.Add(element2.gameObject);
					yield return new WaitForSeconds(0.01f);
				}
			}
		}
		isCreatingServerList = false;
	}

	private RoomInfo[] GetRooms()
	{
		List<RoomInfo> roomList = FilterRoomsMap(RoomList);
		roomList = FilterRoomsSearch(roomList);
		roomList = FilterRoomsSelectMode(roomList);
		roomList = FilterRoomsShowFullServers(roomList);
		roomList = FilterRoomsShowPasswordServers(roomList);
		roomList = FilterRoomsCustomMaps(roomList);
		roomList = SortRooms(roomList);
		MaxServersTotal = RoomList.Length;
		for (int i = 0; i < MaxServersTotal; i++)
		{
			MaxPlayersTotal += RoomList[i].PlayerCount;
		}
		return roomList.ToArray();
	}

	private List<RoomInfo> FilterRoomsMap(RoomInfo[] rooms)
	{
		List<RoomInfo> list = new List<RoomInfo>();
		if (SelectMap == Localization.Get("All"))
		{
			for (int i = 0; i < rooms.Length; i++)
			{
				list.Add(rooms[i]);
			}
		}
		else
		{
			for (int j = 0; j < rooms.Length; j++)
			{
				if (rooms[j].GetSceneName() == SelectMap)
				{
					list.Add(rooms[j]);
				}
			}
		}
		return list;
	}

	private List<RoomInfo> FilterRoomsSearch(List<RoomInfo> roomList)
	{
		if (!string.IsNullOrEmpty(SearchInput.value))
		{
			for (int i = 0; i < roomList.Count; i++)
			{
				if (!roomList[i].Name.ToLower().Contains(SearchInput.value.ToLower()))
				{
					roomList.RemoveAt(i);
					i--;
				}
			}
		}
		return roomList;
	}

	private List<RoomInfo> FilterRoomsSelectMode(List<RoomInfo> roomList)
	{
		if (SelectMode != -1)
		{
			for (int i = 0; i < roomList.Count; i++)
			{
				if (SelectMode != (int)roomList[i].GetGameMode())
				{
					roomList.RemoveAt(i);
					i--;
				}
			}
		}
		return roomList;
	}

	private List<RoomInfo> FilterRoomsShowFullServers(List<RoomInfo> roomList)
	{
		if (!showFullServers)
		{
			for (int i = 0; i < roomList.Count; i++)
			{
				if (roomList[i].MaxPlayers == roomList[i].PlayerCount)
				{
					roomList.RemoveAt(i);
					i--;
				}
			}
		}
		return roomList;
	}

	private List<RoomInfo> FilterRoomsShowPasswordServers(List<RoomInfo> roomList)
	{
		if (!showPasswordServers)
		{
			for (int i = 0; i < roomList.Count; i++)
			{
				if (roomList[i].HasPassword())
				{
					roomList.RemoveAt(i);
					i--;
				}
			}
		}
		return roomList;
	}

	private List<RoomInfo> FilterRoomsCustomMaps(List<RoomInfo> roomList)
	{
		switch (CustomMapsPopupList.value)
		{
		case "Official Maps":
		{
			for (int j = 0; j < roomList.Count; j++)
			{
				if (roomList[j].isCustomMap())
				{
					roomList.RemoveAt(j);
					j--;
				}
			}
			break;
		}
		case "Custom Maps":
		{
			for (int k = 0; k < roomList.Count; k++)
			{
				if (!roomList[k].isCustomMap() || string.IsNullOrEmpty(roomList[k].GetSceneName()) || !GameModeManager.HasCustomMapMode(roomList[k].GetGameMode()))
				{
					roomList.RemoveAt(k);
					k--;
				}
			}
			break;
		}
		case "All":
		{
			for (int i = 0; i < roomList.Count; i++)
			{
				if (roomList[i].isCustomMap() && !GameModeManager.HasCustomMapMode(roomList[i].GetGameMode()))
				{
					roomList.RemoveAt(i);
					i--;
				}
			}
			break;
		}
		}
		return roomList;
	}

	private List<RoomInfo> SortRooms(List<RoomInfo> roomList)
	{
		if (sortServers)
		{
			roomList.Sort((RoomInfo x, RoomInfo y) => y.PlayerCount.CompareTo(x.PlayerCount));
		}
		return roomList;
	}

	private Transform GetElement()
	{
		GameObject gameObject = null;
		if (ServerListPool.Count != 0)
		{
			gameObject = ServerListPool[0];
			ServerListPool.RemoveAt(0);
		}
		else
		{
			gameObject = NGUITools.AddChild(ServerListParent, ServerListElement);
		}
		return gameObject.transform;
	}

	private void ClearServerList()
	{
		for (int i = 0; i < ServerList.Count; i++)
		{
			ServerList[i].SetActive(false);
			ServerListPool.Add(ServerList[i]);
		}
		ServerList.Clear();
	}

	private void UpdateServerInfo()
	{
		string empty = string.Empty;
		empty = ((MaxPlayers != MaxPlayersTotal) ? (Localization.Get("Players") + ": " + MaxPlayers + " (" + MaxPlayersTotal + ") \n" + Localization.Get("Servers") + ": " + MaxServers + " (" + MaxServersTotal + ") \n" + Localization.Get("Ping") + ": " + PhotonNetwork.GetPing()) : (Localization.Get("Players") + ": " + MaxPlayers + "\n" + Localization.Get("Servers") + ": " + MaxServers + "\n" + Localization.Get("Ping") + ": " + PhotonNetwork.GetPing()));
		ServerInfoLabel.text = empty;
	}

	public void OnSelectMode()
	{
		if (SelectMode != (int)ModePopupList.data)
		{
			SelectMode = (int)ModePopupList.data;
			MaxAllModeList = 0;
			SelectAllModeList = 1;
			if (isCreatingServerList)
			{
				isCreatingServerList = false;
				StopCoroutine(CreateServerList(false));
			}
			ServerListParent.GetComponent<UIScrollView>().ResetPosition();
			StartCoroutine(CreateServerList(true));
		}
	}

	public void OnSelectMap()
	{
		SelectMap = ScenesPopupList.value;
	}

	public void RightSwitch()
	{
		SelectAllModeList++;
		if (SelectAllModeList > MaxAllModeList)
		{
			SelectAllModeList = 1;
		}
		if (isCreatingServerList)
		{
			isCreatingServerList = false;
			StopCoroutine(CreateServerList(false));
		}
		ServerListParent.GetComponent<UIScrollView>().ResetPosition();
		StartCoroutine(CreateServerList(false));
	}

	public void LeftSwitch()
	{
		SelectAllModeList--;
		if (SelectAllModeList <= 0)
		{
			SelectAllModeList = MaxAllModeList;
		}
		if (isCreatingServerList)
		{
			isCreatingServerList = false;
			StopCoroutine(CreateServerList(false));
		}
		ServerListParent.GetComponent<UIScrollView>().ResetPosition();
		StartCoroutine(CreateServerList(false));
	}

	public void SettingsClick()
	{
		activeSettingsPanel = !activeSettingsPanel;
		SettingsPanel.SetActive(activeSettingsPanel);
		if (activeSettingsPanel)
		{
			FullServersToogle.value = showFullServers;
			PasswordServersToogle.value = showPasswordServers;
			SortServersToogle.value = sortServers;
		}
	}

	public void SettingsToogle()
	{
		showFullServers = FullServersToogle.value;
		showPasswordServers = PasswordServersToogle.value;
		sortServers = SortServersToogle.value;
		PlayerPrefs.SetInt("showFullServers", FullServersToogle.value ? 1 : 0);
		PlayerPrefs.SetInt("showPasswordServers", PasswordServersToogle.value ? 1 : 0);
		PlayerPrefs.SetInt("sortServers", SortServersToogle.value ? 1 : 0);
	}
}
