using Photon;
using UnityEngine;

public class TennisMode : PunBehaviour
{
	public DrawElements[] Spawns;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		GameManager.MaxScore = 0;
		UIGameManager.SetActiveScore(true, 0);
		GameManager.SetStartDamageTime(nValue.int1);
		UIPanelManager.ShowPanel("Display");
		CameraManager.ActiveStaticCamera();
		GameManager.SetChangeWeapons(false);
		if (PhotonNetwork.isMasterClient)
		{
			TimerManager.In(nValue.float05, delegate
			{
				ActivationWaitPlayer();
			});
		}
		else
		{
			UISelectTeam.OnStart();
		}
		EventManager.AddListener<Team>("SelectTeam", OnSelectTeam);
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void OnSelectTeam(Team team)
	{
		UIPanelManager.ShowPanel("Display");
		if (GameManager.GetController().PlayerInput.Dead)
		{
			CameraManager.ActiveSpectateCamera();
		}
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
		if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateScore(playerConnect);
			if (GameManager.GetRoundState() != 0)
			{
				CheckPlayers();
			}
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
			photonView.RPC("OnCreatePlayer", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void OnCreatePlayer()
	{
		if (PhotonNetwork.player.GetTeam() == Team.None)
		{
			return;
		}
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(nValue.int100);
		CameraManager.DeactiveAll();
		if (PhotonNetwork.room.PlayerCount <= 4)
		{
			if (playerInput.PlayerTeam == Team.Blue)
			{
				GameManager.GetController().ActivePlayer(Spawns[0].GetSpawnPosition(), Spawns[0].GetSpawnRotation());
			}
			else if (playerInput.PlayerTeam == Team.Red)
			{
				GameManager.GetController().ActivePlayer(Spawns[1].GetSpawnPosition(), Spawns[1].GetSpawnRotation());
			}
		}
		else if (PhotonNetwork.room.PlayerCount <= 8)
		{
			if (playerInput.PlayerTeam == Team.Blue)
			{
				GameManager.GetController().ActivePlayer(Spawns[2].GetSpawnPosition(), Spawns[2].GetSpawnRotation());
			}
			else if (playerInput.PlayerTeam == Team.Red)
			{
				GameManager.GetController().ActivePlayer(Spawns[3].GetSpawnPosition(), Spawns[3].GetSpawnRotation());
			}
		}
		else if (PhotonNetwork.room.PlayerCount <= 12)
		{
			if (playerInput.PlayerTeam == Team.Blue)
			{
				GameManager.GetController().ActivePlayer(Spawns[4].GetSpawnPosition(), Spawns[4].GetSpawnRotation());
			}
			else if (playerInput.PlayerTeam == Team.Red)
			{
				GameManager.GetController().ActivePlayer(Spawns[5].GetSpawnPosition(), Spawns[5].GetSpawnRotation());
			}
		}
		WeaponManager.SetSelectWeapon(WeaponType.Knife, 46);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			PhotonNetwork.player.SetDeaths1();
			PlayerRoundManager.SetDeaths1();
			UIStatus.Add(Utils.KillerStatus(damageInfo));
			UIDeathScreen.Show(damageInfo);
			Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
			CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
			GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
			if (damageInfo.isPlayerID)
			{
				photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
			}
			photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
			GameManager.BalanceTeam(true);
			TimerManager.In(nValue.int3, delegate
			{
				if (GameManager.GetController().PlayerInput.Dead)
				{
					CameraManager.ActiveSpectateCamera();
				}
			});
		}
		else
		{
			OnCreatePlayer();
		}
	}

	[PunRPC]
	private void OnKilledPlayer(DamageInfo damageInfo, PhotonMessageInfo info)
	{
		EventManager.Dispatch("KillPlayer", damageInfo);
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(info.sender.ID);
		PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int7), 0, 7));
		PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int5), 0, 5));
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		GameManager.UpdateRoundState(RoundState.EndRound);
		GameManager.BalanceTeam(true);
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
		for (int j = nValue.int0; j < playerList.Length; j++)
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
}
