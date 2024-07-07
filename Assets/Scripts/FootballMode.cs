using System.Collections.Generic;
using System.Linq;
using Photon;

public class FootballMode : PunBehaviour
{
	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		GameManager.MaxScore = nValue.int0;
		UIGameManager.SetActiveScore(true, nValue.int0);
		GameManager.SetStartDamageTime(-nValue.int1);
		UIPanelManager.ShowPanel("Display");
		CameraManager.ActiveStaticCamera();
		GameManager.GetController().PlayerInput.PlayerWeapon.PushRigidbody = true;
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
		TimerManager.In(nValue.float05, delegate
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.BunnyHopEnabled = true;
			playerInput.BunnyHopSpeed = nValue.float025;
			playerInput.FPController.MotorJumpForce = nValue.float02;
			playerInput.FPController.MotorAirSpeed = nValue.int1;
		});
		EventManager.AddListener<Team>("SelectTeam", OnSelectTeam);
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
		FootballManager.StartRound();
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

	public void OnCreatePlayer()
	{
		OnCreatePlayer(default(PhotonMessageInfo));
	}

	[PunRPC]
	private void OnCreatePlayer(PhotonMessageInfo info)
	{
		if (PhotonNetwork.player.GetTeam() != 0)
		{
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(nValue.int100);
			if (info.timestamp != nValue.int0)
			{
				float duration = (float)(PhotonNetwork.time - info.timestamp + nValue.int3);
				playerInput.SetMove(false, duration);
			}
			CameraManager.DeactiveAll();
			int num = UIPlayerStatistics.GetPlayerStatsPosition(PhotonNetwork.player) - nValue.int1;
			if (PhotonNetwork.player.GetTeam() == Team.Red)
			{
				num += nValue.int6;
			}
			GameManager.GetController().ActivePlayer(GameManager.GetSpawn(num).GetSpawnPosition(), GameManager.GetSpawn(num).GetSpawnRotation());
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
		}
	}

	public void Goal(PhotonPlayer player, Team gateTeam)
	{
		bool flag = false;
		UIMainStatus.Add(player.NickName + " [@]", false, nValue.int5, "scored a goal");
		if (player.GetTeam() == Team.Red)
		{
			if (gateTeam == Team.Blue)
			{
				++GameManager.RedScore;
				GameManager.UpdateScore();
			}
			else
			{
				++GameManager.BlueScore;
				GameManager.UpdateScore();
				flag = true;
			}
		}
		else if (gateTeam == Team.Red)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
		}
		else
		{
			++GameManager.RedScore;
			GameManager.UpdateScore();
			flag = true;
		}
		photonView.RPC("OnFinishRound", PhotonTargets.All);
		if (flag)
		{
			TimerManager.In(nValue.float15, delegate
			{
				UIMainStatus.Add("[@]", false, nValue.int3, "Autogoal");
			});
			photonView.RPC("AutoGoal", player);
		}
		else
		{
			photonView.RPC("GoalPlayer", player);
		}
	}

	[PunRPC]
	private void GoalPlayer()
	{
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetXP(nValue.int10 + nValue.int2 * PhotonNetwork.playerList.Length);
		PlayerRoundManager.SetMoney(nValue.int4 + PhotonNetwork.playerList.Length - nValue.int1);
	}

	[PunRPC]
	private void AutoGoal()
	{
		PhotonNetwork.player.SetDeaths1();
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		GameManager.UpdateRoundState(RoundState.EndRound);
		GameManager.BalanceTeam(true);
		float delay = nValue.int5 - (float)(PhotonNetwork.time - info.timestamp);
		TimerManager.In(delay, delegate
		{
			OnStartRound();
		});
	}

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
		if (!flag || !flag2)
		{
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	private void UpdateMasterServer()
	{
		if (PhotonNetwork.isMasterClient && GameManager.GetRoundState() == RoundState.PlayRound)
		{
			List<PhotonPlayer> list = PhotonNetwork.playerList.ToList();
			list.Sort(SortByPing);
			if (list[nValue.int0].ID != PhotonNetwork.player.ID)
			{
				PhotonNetwork.SetMasterClient(list[nValue.int0]);
			}
		}
	}

	public static int SortByPing(PhotonPlayer a, PhotonPlayer b)
	{
		return a.GetPing().CompareTo(b.GetPing());
	}
}
