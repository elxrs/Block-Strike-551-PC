using Photon;
using UnityEngine;

public class TDMMode : PunBehaviour
{
	public CryptoInt MaxScore = 100;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != 0)
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
		GameManager.UpdateRoundState(RoundState.PlayRound);
		CameraManager.ActiveStaticCamera();
		UIGameManager.SetActiveScore(true, MaxScore);
		UISelectTeam.OnSpectator();
		UISelectTeam.OnStart();
		GameManager.MaxScore = MaxScore;
		GameManager.StartAutoBalance();
		EventManager.AddListener<Team>("SelectTeam", OnSelectTeam);
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
		EventManager.AddListener<Team>("AutoBalance", OnAutoBalance);
	}

	private void OnSelectTeam(Team team)
	{
		UIPanelManager.ShowPanel("Display");
		if (team == Team.None)
		{
			CameraManager.ActiveSpectateCamera();
			UIControllerList.Chat.cachedGameObject.SetActive(false);
			UIControllerList.SelectWeapon.cachedGameObject.SetActive(false);
			UISpectator.SetActive(true);
		}
		else
		{
			OnRevivalPlayer();
		}
	}

	private void OnAutoBalance(Team team)
	{
		GameManager.UpdatePlayerTeam(team);
		OnRevivalPlayer();
	}

	private void OnRevivalPlayer()
	{
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(nValue.int100);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		PlayerRoundManager.SetDeaths1();
		UIDeathScreen.Show(damageInfo);
		UIStatus.Add(Utils.KillerStatus(damageInfo));
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		if (damageInfo.isPlayerID)
		{
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
			OnScore(damageInfo.AttackerTeam);
		}
		TimerManager.In(nValue.int3, delegate
		{
			OnRevivalPlayer();
		});
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateScore(playerConnect);
		}
	}

	[PunRPC]
	private void OnKilledPlayer(DamageInfo damageInfo)
	{
		EventManager.Dispatch("KillPlayer", damageInfo);
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(damageInfo.PlayerID);
		if (damageInfo.HeadShot)
		{
			PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int10), 0, 10));
			PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int7), 0, 7));
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int5), 0, 5));
			PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int3), 0, 3));
		}
	}

	private void OnKilledPlayer(object[] data)
	{
		OnKilledPlayer((DamageInfo)data[0]);
	}

	public void OnScore(Team team)
	{
		photonView.RPC("PhotonOnScore", PhotonTargets.MasterClient, (byte)team);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Z))
		{
			OnScore(Team.Blue);
		}
	}

	[PunRPC]
	private void PhotonOnScore(byte intTeam)
	{
		switch ((Team)intTeam)
		{
		case Team.Blue:
			++GameManager.BlueScore;
			break;
		case Team.Red:
			++GameManager.RedScore;
			break;
		}
		GameManager.UpdateScore();
		if (GameManager.CheckScore())
		{
			GameManager.UpdateRoundState(RoundState.EndRound);
			if (GameManager.WinTeam() == Team.Blue)
			{
				UIMainStatus.Add("[@]", false, nValue.int5, "Blue Win");
			}
			else if (GameManager.WinTeam() == Team.Red)
			{
				UIMainStatus.Add("[@]", false, nValue.int5, "Red Win");
			}
			photonView.RPC("PhotonNextLevel", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void PhotonNextLevel()
	{
		GameManager.LoadNextLevel(GameMode.TeamDeathmatch);
	}
}
