using Photon;
using UnityEngine;

public class BuildBattleMode : PunBehaviour
{
	private int SelectID;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.BuildBattle)
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
		UIGameManager.SetActiveScore(true, 20);
		GameManager.SetStartDamageTime(1f);
		UIPanelManager.ShowPanel("Display");
		GameManager.MaxScore = 20;
		CameraManager.ActiveStaticCamera();
		TimerManager.In(0.5f, delegate
		{
			GameManager.OnSelectTeam(Team.Blue);
			if (PhotonNetwork.isMasterClient)
			{
				OnStartRound();
			}
			OnCreatePlayer();
		});
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
					GameManager.UpdateRoundState(RoundState.StartRound);
					TimerManager.In(4f, delegate
					{
						OnStartRound();
					});
				}
			}
		});
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (PhotonNetwork.isMasterClient && GameManager.GetRoundState() != 0 && UIGameManager.instance.isScoreTimer)
		{
			photonView.RPC("UpdateTimer", playerConnect, UIGameManager.instance.ScoreTimer - Time.time);
		}
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		if (!PhotonNetwork.isMasterClient)
		{
		}
	}

	private void OnStartRound()
	{
		DecalsManager.ClearBulletHoles();
		if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.PlayRound);
			photonView.RPC("StartTimer", PhotonTargets.All, "Слово");
		}
	}

	[PunRPC]
	private void StartTimer(string word, PhotonMessageInfo info)
	{
		EventManager.Dispatch("StartRound");
		PaintManager.Clear();
		UIMainStatus.Add(word, true);
		OnCreatePlayer();
		float num = 10f;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		UIGameManager.StartScoreTimer(num, StopTimer);
	}

	[PunRPC]
	private void UpdateTimer(float time, PhotonMessageInfo info)
	{
		TimerManager.In(1.5f, delegate
		{
			time -= (float)(PhotonNetwork.time - info.timestamp);
			UIGameManager.StartScoreTimer(time, StopTimer);
		});
	}

	private void StopTimer()
	{
		if (PhotonNetwork.playerList.Length <= 1)
		{
			OnStartRound();
			return;
		}
		if (SelectID == 0)
		{
			SelectID = -1;
		}
		ShowData();
	}

	private void ShowData()
	{
	}

	private void OnCreatePlayer()
	{
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(100);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		WeaponManager.SetSelectWeapon(WeaponType.Knife, 0);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, SaveLoadManager.GetWeaponSelected(WeaponType.Pistol));
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, 0);
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Pistol);
		playerInput.FPController.PhysicsGravityModifier = 0f;
		playerInput.FPController.MotorFreeFly = true;
	}

	private static int SortPlayerID(PhotonPlayer a, PhotonPlayer b)
	{
		return a.ID.CompareTo(b.ID);
	}
}
