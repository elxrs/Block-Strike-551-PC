using System.Collections.Generic;
using System.Linq;
using Photon;
using UnityEngine;

public class ZombieMode : PunBehaviour
{
	public ZombieBlock[] Blocks;

	private bool isEscape;

	private int StartZombieTimerID;

	private static ZombieMode instance;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
			return;
		}
		if (PhotonNetwork.room.GetGameMode() != GameMode.ZombieSurvival)
		{
			Destroy(this);
			return;
		}
		PhotonClassesManager.Add(this);
		instance = this;
		if (LevelManager.GetSceneName() == "Escape")
		{
			isEscape = true;
		}
	}

	private void Start()
	{
		UIGameManager.SetActiveScore(true, nValue.int20);
		GameManager.SetStartDamageTime(nValue.int1);
		UIPanelManager.ShowPanel("Display");
		GameManager.MaxScore = nValue.int20;
		CameraManager.ActiveStaticCamera();
		TimerManager.In(nValue.int1, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				ActivationWaitPlayer();
			}
			else if (GameManager.GetRoundState() == RoundState.WaitPlayer || GameManager.GetRoundState() == RoundState.StartRound)
			{
				OnCreatePlayer();
			}
			else
			{
				OnCreateZombie();
			}
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void ActivationWaitPlayer()
	{
		EventManager.Dispatch("WaitPlayer");
		GameManager.UpdateRoundState(RoundState.WaitPlayer);
		OnWaitPlayer();
		OnCreatePlayer();
	}

	private void OnWaitPlayer()
	{
		UIStatus.Add(Localization.Get("Waiting for other players"), true);
		TimerManager.In(nValue.int4, delegate
		{
			if (GameManager.GetRoundState() == RoundState.WaitPlayer)
			{
				if (PhotonNetwork.playerList.Length <= nValue.int1)
				{
					OnWaitPlayer();
				}
				else
				{
					GameManager.UpdateRoundState(RoundState.StartRound);
					TimerManager.In(nValue.int4, delegate
					{
						OnStartRound();
					});
				}
			}
		});
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		TimerManager.In(nValue.float05, delegate
		{
			GameManager.UpdateScore(playerConnect);
		});
		if (GameManager.GetRoundState() != 0)
		{
			CheckPlayers();
			if (UIGameManager.instance.isScoreTimer)
			{
				photonView.RPC("UpdateTimer", playerConnect, UIGameManager.instance.ScoreTimer - Time.time);
			}
		}
		if (Blocks == null || Blocks.Length <= nValue.int0)
		{
			return;
		}
		List<byte> list = new List<byte>();
		for (int i = nValue.int0; i < Blocks.Length; i++)
		{
			if (!Blocks[i].actived)
			{
				list.Add((byte)Blocks[i].ID);
			}
		}
		photonView.RPC("DeactiveBlocks", playerConnect, list.ToArray());
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			CheckPlayers();
		}
	}

	private void OnStartRound()
	{
		DecalsManager.ClearBulletHoles();
		if (PhotonNetwork.playerList.Length <= nValue.int1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.StartRound);
			photonView.RPC("StartTimer", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void StartTimer(PhotonMessageInfo info)
	{
		EventManager.Dispatch("StartRound");
		OnCreatePlayer();
		UIToast.Show(Localization.Get("Infestation will start in 20 seconds"));
		float num = nValue.int20;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		StartZombieTimerID = TimerManager.In(num, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				List<PhotonPlayer> list = PhotonNetwork.playerList.ToList();
				int num2 = OnSelectMaxDeaths(list.Count);
				string text = string.Empty;
				for (int i = 0; i < num2; i++)
				{
					int index = Random.Range(nValue.int0, list.Count);
					text = text + list[index].ID + "#";
					list.RemoveAt(index);
				}
				photonView.RPC("OnSendKillerInfo", PhotonTargets.All, text);
			}
			GameManager.UpdateRoundState(RoundState.PlayRound);
		});
	}

	[PunRPC]
	private void UpdateTimer(float time, PhotonMessageInfo info)
	{
		TimerManager.In(nValue.float15, delegate
		{
			time -= (float)(PhotonNetwork.time - info.timestamp);
			UIGameManager.StartScoreTimer(time, StopTimer);
		});
	}

	private void StopTimer()
	{
		if (PhotonNetwork.isMasterClient)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Survivors Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void OnSendKillerInfo(string text, PhotonMessageInfo info)
	{
		string[] array = text.Split("#"[nValue.int0]);
		bool flag = false;
		for (int i = nValue.int0; i < array.Length - nValue.int1; i++)
		{
			if (PhotonNetwork.player.ID == int.Parse(array[i]))
			{
				flag = true;
				break;
			}
		}
		UIToast.Show(Localization.Get("Infestation started"));
		float num = nValue.int300;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		UIGameManager.StartScoreTimer(num, StopTimer);
		if (flag)
		{
			OnCreateZombie();
		}
		TimerManager.In(nValue.int3, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				CheckPlayers();
			}
		});
	}

	private void OnCreatePlayer()
	{
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.Zombie = false;
		playerInput.DamageSpeed = false;
		GameManager.OnSelectTeam(Team.Blue);
		if (!isEscape)
		{
			playerInput.DamageForce = nValue.int0;
		}
		playerInput.MaxHealth = nValue.int100;
		playerInput.SetHealth(nValue.int100);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		playerInput.UpdatePlayerSpeed(nValue.float018);
		playerInput.FPCamera.GetComponent<Camera>().fieldOfView = nValue.int60;
		WeaponManager.SetSelectWeapon(WeaponType.Knife, SaveLoadManager.GetWeaponSelected(WeaponType.Knife));
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, SaveLoadManager.GetWeaponSelected(WeaponType.Pistol));
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, SaveLoadManager.GetWeaponSelected(WeaponType.Rifle));
		WeaponCustomData weaponCustomData = new WeaponCustomData();
		weaponCustomData.CustomData = false;
		weaponCustomData.AmmoMax = WeaponManager.GetSelectWeaponData(WeaponType.Rifle).MaxAmmo * ((!isEscape) ? nValue.int5 : nValue.int10);
		WeaponCustomData weaponCustomData2 = new WeaponCustomData();
		weaponCustomData2.CustomData = false;
		weaponCustomData2.AmmoMax = WeaponManager.GetSelectWeaponData(WeaponType.Pistol).MaxAmmo * ((!isEscape) ? nValue.int5 : nValue.int10);
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle, null, weaponCustomData2, weaponCustomData);
	}

	private void OnCreateZombie()
	{
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.Zombie = true;
		playerInput.DamageSpeed = true;
		GameManager.OnSelectTeam(Team.Red);
		if (isEscape)
		{
			playerInput.DamageForce = nValue.int15;
		}
		playerInput.MaxHealth = nValue.int1000;
		playerInput.SetHealth(nValue.int1000);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		playerInput.UpdatePlayerSpeed(nValue.float019);
		playerInput.FPCamera.GetComponent<Camera>().fieldOfView = nValue.int100;
		WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int17);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound || GameManager.GetRoundState() == RoundState.StartRound)
		{
			PhotonNetwork.player.SetDeaths1();
			PlayerRoundManager.SetDeaths1();
		}
		if (GameManager.GetRoundState() == RoundState.PlayRound || GameManager.GetRoundState() == RoundState.StartRound)
		{
			UIStatus.Add(Utils.KillerStatus(damageInfo));
			if (damageInfo.AttackerTeam == Team.Blue)
			{
				UIDeathScreen.Show(damageInfo);
			}
		}
		if (GameManager.GetRoundState() == RoundState.PlayRound || GameManager.GetRoundState() == RoundState.StartRound)
		{
			Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
			CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
			GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		}
		else
		{
			OnCreatePlayer();
		}
		if (damageInfo.isPlayerID)
		{
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
		}
		if (GameManager.GetRoundState() != RoundState.PlayRound && GameManager.GetRoundState() != RoundState.StartRound)
		{
			return;
		}
		photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
		TimerManager.In((!damageInfo.isPlayerID) ? nValue.int1 : nValue.int3, delegate
		{
			if (GameManager.GetController().PlayerInput.Dead)
			{
				OnCreateZombie();
			}
		});
	}

	[PunRPC]
	private void OnKilledPlayer(DamageInfo damageInfo, PhotonMessageInfo info)
	{
		EventManager.Dispatch("KillPlayer", damageInfo);
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(info.sender.ID);
		if (damageInfo.HeadShot)
		{
			if (damageInfo.AttackerTeam == Team.Red)
			{
				PlayerRoundManager.SetXP(nValue.int10);
				PlayerRoundManager.SetMoney(nValue.int5);
			}
			else
			{
				PlayerRoundManager.SetXP(nValue.int15);
				PlayerRoundManager.SetMoney(nValue.int10);
			}
			PlayerRoundManager.SetHeadshot1();
		}
		else if (damageInfo.AttackerTeam == Team.Red)
		{
			PlayerRoundManager.SetXP(nValue.int5);
			PlayerRoundManager.SetMoney(nValue.int4);
		}
		else
		{
			PlayerRoundManager.SetXP(nValue.int5);
			PlayerRoundManager.SetMoney(nValue.int8);
		}
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		TimerManager.Cancel(StartZombieTimerID);
		UIGameManager.instance.isScoreTimer = false;
		GameManager.UpdateRoundState(RoundState.EndRound);
		if (GameManager.CheckScore())
		{
			GameManager.LoadNextLevel(GameMode.ZombieSurvival);
			return;
		}
		float delay = nValue.int6 - (float)(PhotonNetwork.time - info.timestamp);
		TimerManager.In(delay, delegate
		{
			OnStartRound();
		});
	}

	[PunRPC]
	private void CheckPlayers()
	{
		if (!PhotonNetwork.isMasterClient || GameManager.GetRoundState() != RoundState.PlayRound)
		{
			return;
		}
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		bool flag = false;
		bool flag2 = false;
		for (int i = nValue.int0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() == Team.Blue && !playerList[i].GetDead())
			{
				flag = true;
				break;
			}
		}
		for (int j = nValue.int0; j < playerList.Length; j++)
		{
			if (playerList[j].GetTeam() == Team.Red)
			{
				flag2 = true;
				break;
			}
		}
		if (!flag)
		{
			++GameManager.RedScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Zombie Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
		else if (!flag2)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Survivors Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	private int OnSelectMaxDeaths(int maxPlayers)
	{
		if (maxPlayers >= nValue.int8)
		{
			return nValue.int2;
		}
		return nValue.int1;
	}

	public static void AddDamage(byte id)
	{
		instance.photonView.RPC("PhotonAddDamage", PhotonTargets.All, id);
	}

	[PunRPC]
	private void PhotonAddDamage(byte id)
	{
		for (int i = nValue.int0; i < Blocks.Length; i++)
		{
			if (Blocks[i].ID == id)
			{
				Blocks[i].Attack();
				if (PhotonNetwork.isMasterClient && Blocks[i].CountAttack == nValue.int0)
				{
					photonView.RPC("DeactiveBlock", PhotonTargets.All, id);
				}
			}
		}
	}

	[PunRPC]
	private void DeactiveBlock(byte id)
	{
		for (int i = nValue.int0; i < Blocks.Length; i++)
		{
			if (Blocks[i].ID == id)
			{
				Blocks[i].actived = false;
			}
		}
	}

	[PunRPC]
	private void DeactiveBlocks(byte[] ids)
	{
		for (int i = nValue.int0; i < Blocks.Length; i++)
		{
			for (int j = nValue.int0; j < ids.Length; j++)
			{
				if (Blocks[i].ID == ids[j])
				{
					Blocks[i].actived = false;
				}
			}
		}
	}
}
