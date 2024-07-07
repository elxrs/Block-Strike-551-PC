using UnityEngine;

public class TNTRunMode : Photon.MonoBehaviour
{
	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		UIGameManager.SetActiveScore(true, 20);
		GameManager.SetStartDamageTime(0.1f);
		UIPanelManager.ShowPanel("Display");
		CameraManager.ActiveStaticCamera();
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, 0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, 0);
		GameManager.SetChangeWeapons(false);
		TimerManager.In(0.5f, delegate
		{
			GameManager.OnSelectTeam(Team.Blue);
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
		TimerManager.In(4f, delegate
		{
			if (GameManager.GetRoundState() == RoundState.WaitPlayer)
			{
				if (PhotonNetwork.playerList.Length <= 1)
				{
					OnWaitPlayer();
				}
				else
				{
					TimerManager.In(4f, delegate
					{
						OnStartRound();
					});
				}
			}
		});
	}

	private void OnStartRound()
	{
		TNTRunManager.ResetBlocks();
		if (PhotonNetwork.playerList.Length <= 1)
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
		OnCreatePlayer();
		float num = (float)(PhotonNetwork.time - info.timestamp);
		TimerManager.In(3f - num, delegate
		{
			UIMainStatus.Add("3", true);
		});
		TimerManager.In(4f - num, delegate
		{
			UIMainStatus.Add("2", true);
		});
		TimerManager.In(5f - num, delegate
		{
			UIMainStatus.Add("1", true);
		});
		TimerManager.In(6f - num, delegate
		{
			UIMainStatus.Add("Go", true);
			DrawElements playerIDSpawn = GameManager.GetPlayerIDSpawn();
			GameManager.GetController().SetPosition(playerIDSpawn.GetSpawnPosition());
			GameManager.UpdateRoundState(RoundState.PlayRound);
		});
	}

	private void OnCreatePlayer()
	{
		if (PhotonNetwork.player.GetTeam() != 0)
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(100);
			CameraManager.DeactiveAll();
			DrawElements playerIDSpawn = GameManager.GetPlayerIDSpawn();
			GameManager.GetController().ActivePlayer(playerIDSpawn.GetSpawnPosition(), playerIDSpawn.GetSpawnRotation());
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound || GameManager.GetRoundState() == RoundState.StartRound)
		{
			PhotonNetwork.player.SetDeaths1();
			PlayerRoundManager.SetDeaths1();
		}
		string text = Utils.GetTeamHexColor(PhotonNetwork.player) + " [@]";
		UIStatus.Add(text, false, "died");
		if (GameManager.GetRoundState() == RoundState.PlayRound || GameManager.GetRoundState() == RoundState.StartRound)
		{
			Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
			CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * 100f);
			GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
			photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
			TimerManager.In(3f, delegate
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
	private void OnFinishRound(PhotonMessageInfo info)
	{
		GameManager.UpdateRoundState(RoundState.EndRound);
		if (!GameManager.CheckScore())
		{
			float delay = 8f - (float)(PhotonNetwork.time - info.timestamp);
			TimerManager.In(delay, delegate
			{
				OnStartRound();
			});
		}
	}

	[PunRPC]
	private void CheckPlayers()
	{
		if (!PhotonNetwork.isMasterClient || GameManager.GetRoundState() == RoundState.EndRound)
		{
			return;
		}
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		PhotonPlayer photonPlayer = null;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (!playerList[i].GetDead())
			{
				if (photonPlayer != null)
				{
					return;
				}
				photonPlayer = playerList[i];
			}
		}
		if (photonPlayer != null)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add(photonPlayer.NickName + " [@]", false, 5f, "Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}
}
