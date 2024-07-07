using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

public class mPhotonSettings : PunBehaviour
{
	public const string regionKey = "Region";

	private string SelectMap;

	private bool newRegion;

	private RoomInfo queueRoom;

	private int queueTimer;

	private static mPhotonSettings instance;

	public static bool hasRegion
	{
		get
		{
			return PlayerPrefs.HasKey("Region");
		}
	}

	public static string region
	{
		get
		{
			if (hasRegion)
			{
				return PlayerPrefs.GetString("Region");
			}
			return "Russia";
		}
		set
		{
			PlayerPrefs.SetString("Region", value);
		}
	}

	private void Start()
	{
		instance = this;
		PhotonClassesManager.Add(this);
		PhotonNetwork.automaticallySyncScene = true;
		PhotonNetwork.offlineMode = false;
		PhotonNetwork.UseRpcMonoBehaviourCache = true;
	}

	public void OnConnectToPhoton()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		if (PhotonNetwork.connected && !newRegion)
		{
			mPanelManager.ShowPanel("Server", true);
			return;
		}
		if (PhotonNetwork.connected)
		{
			PhotonNetwork.Disconnect();
		}
		mPopUp.ShowText(Localization.Get("Connecting to the region") + "...");
		string bundleVersion = VersionManager.bundleVersion;
		string appID = GameSettings.instance.PhotonID;
		if (hasRegion)
		{
			CloudRegionCode cloudRegionCode = Region.Parse(region);
			PhotonNetwork.ConnectToRegion(cloudRegionCode, bundleVersion, appID);
		}
		else
		{
			CloudRegionCode cloudRegionCode = Region.Parse("ru");
			PhotonNetwork.ConnectToRegion(cloudRegionCode, bundleVersion, appID);
		}
		newRegion = false;
	}

	public void OnSelectBestRegion(string region)
	{
		if (!(mPhotonSettings.region == region))
		{
			mPhotonSettings.region = region;
			newRegion = true;
			mVersionManager.UpdateRegion();
			if (PhotonNetwork.connected)
			{
				PhotonNetwork.Disconnect();
			}
		}
	}

	public override void OnConnectedToPhoton()
	{
		mVersionManager.UpdateRegion();
		PhotonNetwork.playerName = SaveLoadManager.GetPlayerName();
		PhotonNetwork.player.SetLevel(SaveLoadManager.GetPlayerLevel());
		PhotonNetwork.player.SetClan(SaveLoadManager.GetClan());
		PhotonNetwork.player.SetAvatarUrl((!Settings.ShowAvatar) ? string.Empty : AccountManager.instance.Data.AvatarUrl);
		PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.playerName);
		mPopUp.HideAll("Server");
	}

	public override void OnDisconnectedFromPhoton()
	{
		mPopUp.HideAll("Menu");
	}

	public override void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		UIToast.Show("Failed: " + cause);
	}

	public override void OnConnectionFail(DisconnectCause cause)
	{
		UIToast.Show("Fail: " + cause);
	}

	public static void OnCreateServer()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		mPopUp.ShowText(Localization.Get("Creating Server") + "...");
		PhotonNetwork.player.SetPlayerID(SaveLoadManager.GetPlayerID());
		PhotonNetwork.player.ClearProperties();
		GameMode gameMode = mCreateServer.GetGameMode();
		string serverName = mCreateServer.GetServerName();
		int maxPlayers = mCreateServer.GetMaxPlayers();
#if !UNITY_EDITOR
		maxPlayers = (!(mCreateServer.GetMap() == "50Traps")) ? Mathf.Clamp(maxPlayers, 4, 12) : Mathf.Clamp(maxPlayers, 4, 32);
#endif
		string password = mCreateServer.GetPassword();
		instance.SelectMap = mCreateServer.GetMap();
		Hashtable hashtable = PhotonNetwork.room.CreateRoomHashtable(password, gameMode);
		if (gameMode == GameMode.Only)
		{
			hashtable[PhotonCustomValue.onlyWeaponKey] = (byte)mServerSettings.GetOnlyWeapon();
		}
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = (byte)maxPlayers;
		roomOptions.IsOpen = true;
		roomOptions.IsVisible = true;
		roomOptions.CustomRoomProperties = hashtable;
		if (gameMode == GameMode.Only)
		{
			roomOptions.CustomRoomPropertiesForLobby = new string[4]
			{
				PhotonCustomValue.sceneNameKey,
				PhotonCustomValue.passwordKey,
				PhotonCustomValue.gameModeKey,
				PhotonCustomValue.onlyWeaponKey
			};
		}
		else
		{
			roomOptions.CustomRoomPropertiesForLobby = new string[3]
			{
				PhotonCustomValue.sceneNameKey,
				PhotonCustomValue.passwordKey,
				PhotonCustomValue.gameModeKey
			};
		}
		LevelManager.CustomMap = false;
		PhotonNetwork.CreateRoom(serverName, roomOptions, null);
	}

	public static void OnCreateCustomServer()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		mPopUp.ShowText(Localization.Get("Creating Server") + "...");
		PhotonNetwork.player.SetPlayerID(SaveLoadManager.GetPlayerID());
		PhotonNetwork.player.ClearProperties();
		GameMode gameMode = mCreateCustomServer.GetGameMode();
		string serverName = mCreateCustomServer.GetServerName();
		int maxPlayers = mCreateCustomServer.GetMaxPlayers();
		string password = mCreateCustomServer.GetPassword();
		instance.SelectMap = mCreateCustomServer.GetMap();
		Hashtable hashtable = PhotonNetwork.room.CreateRoomHashtable(password, gameMode);
		hashtable[PhotonCustomValue.hashKey] = CustomMapManager.hash;
		hashtable[PhotonCustomValue.mapUrl] = CustomMapManager.mapUrl;
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = (byte)maxPlayers;
		roomOptions.IsOpen = true;
		roomOptions.IsVisible = true;
		roomOptions.CustomRoomProperties = hashtable;
		roomOptions.CustomRoomPropertiesForLobby = new string[5]
		{
			PhotonCustomValue.sceneNameKey,
			PhotonCustomValue.passwordKey,
			PhotonCustomValue.gameModeKey,
			PhotonCustomValue.hashKey,
			PhotonCustomValue.mapUrl
		};
		LevelManager.CustomMap = true;
		PhotonNetwork.CreateRoom(serverName, roomOptions, null);
	}

	public static void OnQueueServer(RoomInfo room)
	{
		instance.queueRoom = room;
		mPopUp.ShowPopup(Localization.Get("Please wait") + "...", Localization.Get("Queue"), Localization.Get("Exit"), delegate
		{
			instance.queueRoom = null;
			TimerManager.Cancel(instance.queueTimer);
			mJoinServer.onBack();
		});
		instance.queueTimer = TimerManager.In(1f, -1, 1f, delegate
		{
			if (instance.queueRoom == null)
			{
				TimerManager.Cancel(instance.queueTimer);
			}
			else
			{
				RoomInfo[] roomList = PhotonNetwork.GetRoomList();
				for (int i = 0; i < roomList.Length; i++)
				{
					if (roomList[i].Name == instance.queueRoom.Name)
					{
						if (roomList[i].PlayerCount != roomList[i].MaxPlayers)
						{
							TimerManager.Cancel(instance.queueTimer);
							OnJoinServer(instance.queueRoom);
						}
						break;
					}
				}
			}
		});
	}

	public static void OnJoinChat(string chatName)
	{
		mPopUp.ShowText(Localization.Get("Please wait") + "...");
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 0;
		roomOptions.IsOpen = true;
		roomOptions.IsVisible = false;
		PhotonNetwork.JoinOrCreateRoom(chatName, roomOptions, null);
	}

	public void OnCreateServerOffline(string scene)
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		if (PhotonNetwork.connected)
		{
			PhotonNetwork.Disconnect();
		}
		SelectMap = scene;
		PhotonNetwork.offlineMode = true;
		LevelManager.CustomMap = false;
		PhotonNetwork.CreateRoom(scene);
	}

	public static void OnJoinServer(RoomInfo room)
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		if (instance.queueRoom == null)
		{
			mPopUp.ShowText(Localization.Get("Connecting") + "...");
		}
		PhotonNetwork.player.SetPlayerID(SaveLoadManager.GetPlayerID());
		PhotonNetwork.player.ClearProperties();
		instance.SelectMap = room.GetSceneName();
		PhotonNetwork.JoinRoom(room.Name);
	}

	public override void OnJoinedRoom()
	{
		PlayerRoundManager.Clear();
		PlayerRoundManager.SetMode(PhotonNetwork.room.GetGameMode());
		mPopUp.ShowText(Localization.Get("Loading") + "...");
		if (PhotonNetwork.offlineMode)
		{
			LevelManager.LoadLevel(SelectMap);
			return;
		}
		PhotonNetwork.isMessageQueueRunning = false;
		PhotonNetwork.LoadLevel(SelectMap);
	}

	public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
	{
		short num = (short)codeAndMsg[0];
		string text = (string)codeAndMsg[1];
		mJoinServer.onBack();
		if (text == "Game full")
		{
			UIToast.Show(Localization.Get("The server is full"));
			return;
		}
		UIToast.Show("Error Code: " + num + " Message: " + text);
	}

	public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
	{
		short num = (short)codeAndMsg[0];
		string text = (string)codeAndMsg[1];
		mPopUp.HideAll("CreateServer");
		if (num == 32766)
		{
			UIToast.Show(Localization.Get("Server with this name already exists"));
			return;
		}
		UIToast.Show("Error Code: " + num + " Message: " + text);
	}
}
