using Photon;
using UnityEngine;

public class JuggernautMode : PunBehaviour
{
	private int NextJuggernaut = -1;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.Juggernaut)
		{
			Destroy(this);
		}
		else
		{
			PhotonClassesManager.Add(this);
		}
	}

	private void Start()
	{
		UIGameManager.SetActiveScore(true, nValue.int20);
		GameManager.SetStartDamageTime(nValue.int1);
		UIPanelManager.ShowPanel("Display");
		GameManager.MaxScore = nValue.int20;
		GameManager.SetChangeWeapons(true);
		GameManager.SetGlobalChat(true);
		CameraManager.ActiveStaticCamera();
		TimerManager.In(nValue.float05, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				ActivationWaitPlayer();
			}
			else if (GameManager.GetController().PlayerInput.Dead)
			{
				CameraManager.ActiveSpectateCamera();
			}
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void ActivationWaitPlayer()
	{
		EventManager.Dispatch("WaitPlayer");
		GameManager.UpdateRoundState(RoundState.WaitPlayer);
		GameManager.OnSelectTeam(Team.Blue);
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
		if (UIGameManager.instance.isScoreTimer)
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
		if (PhotonNetwork.playerList.Length <= nValue.int1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.PlayRound);
			if (!HasNextJuggernaut())
			{
				NextJuggernaut = PhotonNetwork.playerList[Random.Range(nValue.int0, PhotonNetwork.playerList.Length)].ID;
			}
			photonView.RPC("OnSendKillerInfo", PhotonTargets.All, NextJuggernaut);
		}
	}

	private bool HasNextJuggernaut()
	{
		for (int i = nValue.int0; i < PhotonNetwork.playerList.Length; i++)
		{
			if (NextJuggernaut == PhotonNetwork.playerList[i].ID)
			{
				return true;
			}
		}
		return false;
	}

	[PunRPC]
	private void OnSendKillerInfo(int id, PhotonMessageInfo info)
	{
		float num = nValue.int150;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		UIGameManager.StartScoreTimer(num, StopTimer);
		if (PhotonNetwork.player.ID == id)
		{
			GameManager.OnSelectTeam(Team.Red);
		}
		else
		{
			GameManager.OnSelectTeam(Team.Blue);
		}
		EventManager.Dispatch("StartRound");
		OnCreatePlayer();
		TimerManager.In(nValue.int3, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				CheckPlayers();
			}
		});
	}

	private void StopTimer()
	{
		if (PhotonNetwork.isMasterClient)
		{
			++GameManager.RedScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Red Win");
			photonView.RPC("SetNextJuggernaut", PhotonTargets.All, PhotonNetwork.playerList[Random.Range(nValue.int0, PhotonNetwork.playerList.Length)].ID);
			photonView.RPC("OnFinishRound", PhotonTargets.All);
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
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		if (playerInput.PlayerTeam == Team.Red)
		{
			int num = nValue.int500 + nValue.int150 * PhotonNetwork.otherPlayers.Length;
			playerInput.MaxHealth = num;
			playerInput.SetHealth(num);
			WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int0);
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int23);
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
			TimerManager.In(nValue.float01, delegate
			{
				playerInput.FPController.MotorAcceleration = nValue.float01;
			});
		}
		else
		{
			playerInput.MaxHealth = nValue.int100;
			playerInput.SetHealth(nValue.int100);
			WeaponManager.SetSelectWeapon(WeaponType.Knife, SaveLoadManager.GetWeaponSelected(WeaponType.Knife));
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, SaveLoadManager.GetWeaponSelected(WeaponType.Pistol));
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, SaveLoadManager.GetWeaponSelected(WeaponType.Rifle));
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		PlayerRoundManager.SetDeaths1();
		UIStatus.Add(Utils.KillerStatus(damageInfo));
		UIDeathScreen.Show(damageInfo);
		if (damageInfo.isPlayerID)
		{
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
			photonView.RPC("SetNextJuggernaut", PhotonTargets.All, damageInfo.PlayerID);
		}
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
		TimerManager.In(nValue.int3, delegate
		{
			if (GameManager.GetController().PlayerInput.Dead)
			{
				CameraManager.ActiveSpectateCamera();
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
			PlayerRoundManager.SetXP(nValue.int12);
			PlayerRoundManager.SetMoney(nValue.int10);
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(nValue.int6);
			PlayerRoundManager.SetMoney(nValue.int5);
		}
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		UIGameManager.instance.isScoreTimer = false;
		GameManager.UpdateRoundState(RoundState.EndRound);
		if (GameManager.CheckScore())
		{
			GameManager.LoadNextLevel(GameMode.Juggernaut);
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
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() == Team.Blue && !playerList[i].GetDead())
			{
				flag = true;
				break;
			}
		}
		for (int j = 0; j < playerList.Length; j++)
		{
			if (playerList[j].GetTeam() == Team.Red && !playerList[j].GetDead())
			{
				flag2 = true;
				break;
			}
		}
		if (!flag)
		{
			++GameManager.RedScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Red Win");
			photonView.RPC("SetNextJuggernaut", PhotonTargets.All, PhotonNetwork.playerList[Random.Range(nValue.int0, PhotonNetwork.playerList.Length)].ID);
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
		else if (!flag2)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, nValue.int5, "Blue Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void SetNextJuggernaut(int id)
	{
		NextJuggernaut = id;
	}
}
