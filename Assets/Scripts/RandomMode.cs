using Photon;
using UnityEngine;

public class RandomMode : PunBehaviour
{
	public CryptoInt MaxScore = 100;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.RandomMode)
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
		UISelectTeam.OnStart();
		GameManager.MaxScore = MaxScore;
		GameManager.SetChangeWeapons(false);
		GameManager.StartAutoBalance();
		EventManager.AddListener<Team>("SelectTeam", OnSelectTeam);
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
		EventManager.AddListener<Team>("AutoBalance", OnAutoBalance);
	}

	private void OnSelectTeam(Team team)
	{
		UIPanelManager.ShowPanel("Display");
		OnRevivalPlayer();
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
		WeaponType selectWeaponType = WeaponType.Rifle;
		int num = Random.Range(nValue.int0, nValue.int100);
		if (num >= 45)
		{
			selectWeaponType = WeaponType.Rifle;
		}
		else if (num >= nValue.int5)
		{
			selectWeaponType = WeaponType.Pistol;
		}
		else
		{
			selectWeaponType = WeaponType.Knife;
		}
		int randomWeaponID = WeaponManager.GetRandomWeaponID(selectWeaponType);
		WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		WeaponManager.SetSelectWeapon(selectWeaponType, randomWeaponID);
		playerInput.PlayerWeapon.UpdateWeaponAll(selectWeaponType);
		if (selectWeaponType != WeaponType.Knife)
		{
			TimerManager.In(nValue.float01, delegate
			{
				PlayerWeapons.PlayerWeaponData weaponData = playerInput.PlayerWeapon.GetWeaponData(selectWeaponType);
				weaponData.AmmoMax *= nValue.int2;
				UIGameManager.SetAmmoLabel(playerInput.PlayerWeapon.GetSelectedWeaponData().Ammo, playerInput.PlayerWeapon.GetSelectedWeaponData().AmmoMax);
			});
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
			OnScore(damageInfo.AttackerTeam);
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
		}
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
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
	private void OnKilledPlayer(DamageInfo damageInfo, PhotonMessageInfo info)
	{
		EventManager.Dispatch("KillPlayer", damageInfo);
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(info.sender.ID);
		if (damageInfo.HeadShot)
		{
			PlayerRoundManager.SetXP(nValue.int10);
			PlayerRoundManager.SetMoney(nValue.int7);
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(nValue.int5);
			PlayerRoundManager.SetMoney(nValue.int3);
		}
	}

	public void OnScore(Team team)
	{
		photonView.RPC("PhotonOnScore", PhotonTargets.MasterClient, (byte)team);
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
		GameManager.LoadNextLevel(GameMode.RandomMode);
	}
}
