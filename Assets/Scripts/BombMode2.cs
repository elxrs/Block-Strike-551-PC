using System.Collections.Generic;
using Photon;
using UnityEngine;

public class BombMode2 : PunBehaviour
{
	public static BombMode2 instance;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
			return;
		}
		if (PhotonNetwork.room.GetGameMode() != GameMode.Bomb2)
		{
			Destroy(this);
			return;
		}
		PhotonClassesManager.Add(this);
		instance = this;
	}

	private void Start()
	{
		UIGameManager.SetActiveScore(true, nValue.int20);
		GameManager.SetStartDamageTime(nValue.int1);
		GameManager.SetGlobalChat(false);
		WeaponManager.SetSelectWeapon(WeaponType.Knife, SaveLoadManager.GetWeaponSelected(WeaponType.Knife));
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int3);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		UIPanelManager.ShowPanel("Display");
		CameraManager.ActiveStaticCamera();
		CameraManager.teamCamera = true;
		GameManager.SetChangeWeapons(false);
		UIBuyWeapon.SetActive(true);
		UIBuyWeapon.Money = nValue.int500;
		UIChangeTeam.SetChangeTeam(true, true);
		if (PhotonNetwork.isMasterClient)
		{
			TimerManager.In(nValue.float05, delegate
			{
				ActivationWaitPlayer();
			});
		}
		else
		{
			UISelectTeam.OnSpectator();
			UISelectTeam.OnStart();
		}
		EventManager.AddListener<Team>("SelectTeam", OnSelectTeam);
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void OnSelectTeam(Team team)
	{
		UIPanelManager.ShowPanel("Display");
		if (team == Team.None)
		{
			UIBuyWeapon.SetActive(false);
			CameraManager.teamCamera = false;
			CameraManager.ActiveSpectateCamera();
			UIControllerList.Chat.cachedGameObject.SetActive(false);
			UIControllerList.SelectWeapon.cachedGameObject.SetActive(false);
			UISpectator.SetActive(true);
		}
		else if (GameManager.GetController().PlayerInput.Dead)
		{
			CameraManager.ActiveSpectateCamera();
			UISpectator.SetActive(true);
		}
	}

	private void ActivationWaitPlayer()
	{
		EventManager.Dispatch("WaitPlayer");
		GameManager.UpdateRoundState(RoundState.WaitPlayer);
		GameManager.OnSelectTeam(Team.Red);
		OnWaitPlayer();
		OnCreatePlayer();
		PlayerInput.instance.SetMove(true);
	}

	private void OnWaitPlayer()
	{
		UIStatus.Add(Localization.Get("Waiting for other players"), true);
		TimerManager.In(nValue.int4, delegate
		{
			if (GameManager.GetRoundState() == RoundState.WaitPlayer)
			{
				if (GetPlayers().Length <= nValue.int1)
				{
					OnWaitPlayer();
				}
				else
				{
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
		GameManager.UpdateScore(playerConnect);
		if (GameManager.GetRoundState() == RoundState.WaitPlayer)
		{
			return;
		}
		CheckPlayers();
		if (UIGameManager.instance.isScoreTimer && !BombManager.BombPlaced)
		{
			TimerManager.In(nValue.float05, delegate
			{
				photonView.RPC("UpdateTimer", playerConnect, UIGameManager.instance.ScoreTimer - Time.time);
			});
		}
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
		if (GetPlayers().Length <= nValue.int1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.PlayRound);
			photonView.RPC("PhotonStartRound", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void PhotonStartRound(PhotonMessageInfo info)
	{
		DropWeaponManager.ClearScene();
		EventManager.Dispatch("StartRound");
		float num = nValue.int7;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		UIGameManager.StartScoreTimer(num, StartTimer);
		OnCreatePlayer(false);
		BombManager.BuyTime = true;
	}

	private void StartTimer()
	{
		if (GameManager.GetRoundState() != RoundState.EndRound)
		{
			float time = nValue.int120;
			UIGameManager.StartScoreTimer(time, StopTimer);
			PlayerInput.instance.SetMove(true);
		}
		BombManager.BuyTime = false;
	}

	private void StopTimer()
	{
		if (PhotonNetwork.isMasterClient)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Blue Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All, (byte)1, false);
		}
	}

	public void Boom()
	{
		if (PhotonNetwork.isMasterClient && GameManager.GetRoundState() == RoundState.PlayRound)
		{
			++GameManager.RedScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Red Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All, (byte)2, true);
		}
	}

	public void DeactiveBoom()
	{
		if (PhotonNetwork.isMasterClient && GameManager.GetRoundState() == RoundState.PlayRound)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Blue Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All, (byte)1, true);
		}
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

	private void OnCreatePlayer()
	{
		OnCreatePlayer(true);
	}

	private void OnCreatePlayer(bool move)
	{
		if (PhotonNetwork.player.GetTeam() != 0)
		{
			UISpectator.SetActive(false);
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(nValue.int100);
			CameraManager.DeactiveAll();
			GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
			if (WeaponManager.GetSelectWeapon(WeaponType.Rifle) != 0)
			{
				playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
			}
			else
			{
				playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Pistol);
			}
			playerInput.SetMove(move);
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		PlayerRoundManager.SetDeaths1();
		UIStatus.Add(Utils.KillerStatus(damageInfo));
		UIDeathScreen.Show(damageInfo);
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		Vector3 ragdollForce = Utils.GetRagdollForce(playerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, playerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		UIDefuseKit.SetDefuseKit(false);
		if (damageInfo.isPlayerID)
		{
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
		}
		photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
		GameManager.BalanceTeam(true);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int3);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		TimerManager.In(nValue.int3, delegate
		{
			if (GameManager.GetController().PlayerInput.Dead)
			{
				UISpectator.SetActive(true);
				CameraManager.ActiveSpectateCamera();
			}
		});
		BombManager.DeadPlayer();
	}

	[PunRPC]
	private void OnKilledPlayer(DamageInfo damageInfo, PhotonMessageInfo info)
	{
		EventManager.Dispatch("KillPlayer", damageInfo);
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(info.sender.ID);
		UIBuyWeapon.Money += nValue.int150;
		if (damageInfo.HeadShot)
		{
			PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int12), 0, 12));
			PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int10), 0, 10));
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int6), 0, 6));
			PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int5), 0, 5));
		}
	}

	[PunRPC]
	private void OnFinishRound(byte team, bool bomb, PhotonMessageInfo info)
	{
		UIGameManager.instance.isScoreTimer = false;
		GameManager.UpdateRoundState(RoundState.EndRound);
		GameManager.BalanceTeam(true);
		if ((Team)team == PhotonNetwork.player.GetTeam())
		{
			UIBuyWeapon.Money += ((!bomb) ? 2500 : 2750);
		}
		else
		{
			UIBuyWeapon.Money += ((!bomb) ? 1750 : 2000);
		}
		if (GameManager.BlueScore + GameManager.RedScore == GameManager.MaxScore / nValue.int2)
		{
			if (PhotonNetwork.isMasterClient)
			{
				int num = GameManager.BlueScore;
				GameManager.BlueScore = GameManager.RedScore;
				GameManager.RedScore = num;
				GameManager.UpdateScore();
			}
			if (GameManager.GetPlayerTeam() == Team.Blue)
			{
				GameManager.UpdatePlayerTeam(Team.Red);
			}
			else if (GameManager.GetPlayerTeam() == Team.Red)
			{
				GameManager.UpdatePlayerTeam(Team.Blue);
			}
			UIBuyWeapon.Money = nValue.int500;
			WeaponManager.SetSelectWeapon(WeaponType.Knife, SaveLoadManager.GetWeaponSelected(WeaponType.Knife));
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int3);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
			PlayerInput.instance.PlayerWeapon.UpdateWeaponAll(WeaponType.Pistol);
		}
		else
		{
			GameManager.BalanceTeam(true);
		}
		if (GameManager.CheckScore())
		{
			GameManager.LoadNextLevel(GameMode.Bomb);
			return;
		}
		float delay = nValue.int8 - (float)(PhotonNetwork.time - info.timestamp);
		TimerManager.In(delay, delegate
		{
			OnStartRound();
		});
	}

	[PunRPC]
	private void CheckPlayers()
	{
		if (!PhotonNetwork.isMasterClient || GameManager.GetRoundState() == RoundState.EndRound)
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
		if (!BombManager.BombPlaced)
		{
			for (int j = nValue.int0; j < playerList.Length; j++)
			{
				if (playerList[j].GetTeam() == Team.Red && !playerList[j].GetDead())
				{
					flag2 = true;
					break;
				}
			}
		}
		else
		{
			flag2 = true;
		}
		if (!flag)
		{
			++GameManager.RedScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Red Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All, (byte)2, false);
		}
		else if (!flag2)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Blue Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All, (byte)1, false);
		}
	}

	private PhotonPlayer[] GetPlayers()
	{
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() != 0)
			{
				list.Add(playerList[i]);
			}
		}
		return list.ToArray();
	}
}
