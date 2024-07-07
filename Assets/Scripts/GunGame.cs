using Photon;
using UnityEngine;

public class GunGame : PunBehaviour
{
	public CryptoInt MaxScore = 100;

	private int PlayerKills;

	private int[] Weapons = new int[35]
	{
		3, 27, 13, 49, 36, 6, 2, 42, 21, 37,
		9, 25, 26, 14, 24, 12, 7, 50, 18, 29,
		19, 28, 1, 5, 15, 8, 30, 41, 23, 38,
		11, 10, 16, 4, 22
	};

	private int SelectWeaponIndex;

	private WeaponData SelectWeapon;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.GunGame)
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
		WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int3);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		GameManager.MaxScore = MaxScore;
		GameManager.SetChangeWeapons(false);
		GameManager.StartAutoBalance();
		SelectWeapon = WeaponManager.GetWeaponData(nValue.int3);
		UISelectTeam.OnStart();
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
		playerInput.PlayerWeapon.UpdateWeaponAll(SelectWeapon.Type);
		if (SelectWeapon.Type != WeaponType.Knife)
		{
			TimerManager.In(nValue.float01, delegate
			{
				PlayerWeapons.PlayerWeaponData weaponData = playerInput.PlayerWeapon.GetWeaponData(SelectWeapon.Type);
				weaponData.AmmoMax = weaponData.AmmoMax * nValue.int2;
				UIGameManager.SetAmmoLabel(playerInput.PlayerWeapon.GetSelectedWeaponData().Ammo, playerInput.PlayerWeapon.GetSelectedWeaponData().AmmoMax);
			});
		}
	}

	private void OnUpdateWeapon()
	{
		if (SelectWeaponIndex >= Weapons.Length - nValue.int1)
		{
			SelectWeaponIndex = nValue.int0;
		}
		else
		{
			SelectWeaponIndex++;
		}
		WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		SelectWeapon = WeaponManager.GetWeaponData(Weapons[SelectWeaponIndex]);
		UIToast.Show(SelectWeapon.Name);
		SoundManager.Play2D("UpWeapon");
		switch (SelectWeapon.Type)
		{
		case WeaponType.Knife:
			WeaponManager.SetSelectWeapon(WeaponType.Knife, SelectWeapon.ID);
			break;
		case WeaponType.Pistol:
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, SelectWeapon.ID);
			break;
		case WeaponType.Rifle:
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, SelectWeapon.ID);
			break;
		}
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		if (playerInput.Dead)
		{
			return;
		}
		playerInput.PlayerWeapon.CanFire = false;
		TimerManager.In(nValue.float02, delegate
		{
			if (!playerInput.Dead)
			{
				playerInput.PlayerWeapon.UpdateWeaponAll(SelectWeapon.Type);
				TimerManager.In(nValue.float01, delegate
				{
					playerInput.PlayerWeapon.CanFire = true;
					if (SelectWeapon.Type != WeaponType.Knife)
					{
						PlayerWeapons.PlayerWeaponData weaponData = playerInput.PlayerWeapon.GetWeaponData(SelectWeapon.Type);
						weaponData.AmmoMax = weaponData.AmmoMax * nValue.int2;
						UIGameManager.SetAmmoLabel(playerInput.PlayerWeapon.GetSelectedWeaponData().Ammo, playerInput.PlayerWeapon.GetSelectedWeaponData().AmmoMax);
					}
				});
			}
			else
			{
				playerInput.PlayerWeapon.CanFire = true;
			}
		});
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
			PlayerRoundManager.SetMoney(nValue.int6);
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(nValue.int5);
			PlayerRoundManager.SetMoney(nValue.int3);
		}
		if (PlayerKills >= nValue.int1 || (PlayerKills >= nValue.int0 && SelectWeapon.Type == WeaponType.Knife))
		{
			PlayerKills = nValue.int0;
			OnUpdateWeapon();
		}
		else
		{
			PlayerKills++;
		}
	}

	public void OnScore(Team team)
	{
		photonView.RPC("PhotonOnScore", PhotonTargets.MasterClient, (int)team);
	}

	[PunRPC]
	private void PhotonOnScore(int intTeam)
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
		GameManager.LoadNextLevel(GameMode.GunGame);
	}
}
