using Photon;
using UnityEngine;

public class MurderMode : PunBehaviour
{
	public CryptoInt MaxScore = 20;

	public static int Murder;

	public static int Detective;

	private int TimerID = -1;

	private void Awake()
	{
		if (PhotonNetwork.room.GetGameMode() != GameMode.Murder)
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
		UIPlayerStatistics.isOnlyBluePanel = true;
		UIGameManager.SetActiveScore(true, 20);
		GameManager.SetStartDamageTime(1f);
		GameManager.SetFriendDamage(true);
		UIPanelManager.ShowPanel("Display");
		WeaponManager.MaxDamage = true;
		CameraManager.ActiveStaticCamera();
		GameManager.SetChangeWeapons(false);
		GameManager.SetGlobalChat(false);
		TimerManager.In(0.5f, delegate
		{
			GameManager.OnSelectTeam(Team.Blue);
			if (PhotonNetwork.isMasterClient)
			{
				ActivationWaitPlayer();
			}
			else
			{
				CameraManager.ActiveSpectateCamera();
			}
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", DeadPlayer);
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
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		GameManager.UpdateScore(playerConnect);
		if (GameManager.GetRoundState() != 0)
		{
			CheckPlayers();
		}
		if (UIGameManager.instance.isScoreTimer)
		{
			TimerManager.In(1f, delegate
			{
				photonView.RPC("UpdateTimer", playerConnect, UIGameManager.instance.ScoreTimer - Time.time, Murder, Detective);
			});
		}
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			if (playerDisconnect.ID == Detective)
			{
				MurderModeManager.SetRandomPlayerPistol();
			}
			CheckPlayers();
		}
	}

	private void OnStartRound()
	{
		DecalsManager.ClearBulletHoles();
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
		EventManager.Dispatch("StartRound");
		TimerManager.Cancel(TimerID);
		Murder = -1;
		Detective = -1;
		OnCreatePlayer();
		UIToast.Show(Localization.Get("Roles will be given in 15 seconds"));
		float num = 15f;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		TimerID = TimerManager.In(num, delegate
		{
			if (PhotonNetwork.isMasterClient)
			{
				PhotonPlayer photonPlayer;
				do
				{
					photonPlayer = PhotonNetwork.playerList[Random.Range(0, PhotonNetwork.playerList.Length)];
				}
				while (photonPlayer.GetDead());
				PhotonPlayer photonPlayer2;
				do
				{
					photonPlayer2 = PhotonNetwork.playerList[Random.Range(0, PhotonNetwork.playerList.Length)];
				}
				while (PhotonNetwork.playerList.Length > 1 && (photonPlayer.ID == photonPlayer2.ID || photonPlayer2.GetDead()));
				photonView.RPC("SetRoles", PhotonTargets.All, photonPlayer.ID, photonPlayer2.ID);
			}
			GameManager.UpdateRoundState(RoundState.PlayRound);
		});
	}

	[PunRPC]
	private void SetRoles(int murder, int detective, PhotonMessageInfo info)
	{
		float num = 240f;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		UIGameManager.StartScoreTimer(num, StopTimer);
		Murder = murder;
		Detective = detective;
		if (PhotonNetwork.player.GetTeam() != 0 && !PhotonNetwork.player.GetDead())
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			if (PhotonNetwork.player.ID == Murder)
			{
				UIToast.Show(Localization.Get("Murder"));
				WeaponManager.SetSelectWeapon(WeaponType.Knife, 4);
				WeaponManager.SetSelectWeapon(WeaponType.Pistol, 0);
				WeaponManager.SetSelectWeapon(WeaponType.Rifle, 47);
				WeaponCustomData weaponCustomData = new WeaponCustomData();
				weaponCustomData.BodyDamage = 100;
				weaponCustomData.FaceDamage = 100;
				weaponCustomData.HandDamage = 100;
				weaponCustomData.LegDamage = 100;
				weaponCustomData.CustomData = false;
				WeaponCustomData weaponCustomData2 = new WeaponCustomData();
				weaponCustomData2.Mass = 0.02f;
				weaponCustomData.CustomData = false;
				playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle, weaponCustomData, null, weaponCustomData2);
			}
			else if (PhotonNetwork.player.ID == Detective)
			{
				UIToast.Show(Localization.Get("Bystander") + " + Deagle");
				WeaponManager.SetSelectWeapon(WeaponType.Knife, 0);
				WeaponManager.SetSelectWeapon(WeaponType.Pistol, 2);
				WeaponManager.SetSelectWeapon(WeaponType.Rifle, 47);
				WeaponCustomData weaponCustomData3 = new WeaponCustomData();
				weaponCustomData3.Ammo = 1;
				weaponCustomData3.AmmoTotal = 1;
				weaponCustomData3.AmmoMax = 99;
				weaponCustomData3.BodyDamage = 100;
				weaponCustomData3.FaceDamage = 100;
				weaponCustomData3.HandDamage = 100;
				weaponCustomData3.LegDamage = 100;
				weaponCustomData3.Skin = 0;
				weaponCustomData3.FireStat = false;
				weaponCustomData3.FireStatCounter = -1;
				weaponCustomData3.CustomData = false;
				WeaponCustomData weaponCustomData4 = new WeaponCustomData();
				weaponCustomData4.Mass = 0.02f;
				weaponCustomData4.CustomData = false;
				playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle, null, weaponCustomData3, weaponCustomData4);
			}
			else
			{
				UIToast.Show(Localization.Get("Bystander"));
			}
		}
	}

	[PunRPC]
	private void UpdateTimer(float time, int murder, int detective, PhotonMessageInfo info)
	{
		Murder = murder;
		Detective = detective;
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
			UIMainStatus.Add("[@]", false, nValue.int5, "Bystander Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	private void OnCreatePlayer()
	{
		if (PhotonNetwork.player.GetTeam() != 0)
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(100);
			CameraManager.DeactiveAll();
			DrawElements teamSpawn = GameManager.GetTeamSpawn(Team.Red);
			GameManager.GetController().ActivePlayer(teamSpawn.GetSpawnPosition(), teamSpawn.GetSpawnRotation());
			WeaponManager.SetSelectWeapon(WeaponType.Knife, 0);
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, 0);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, 47);
			WeaponCustomData weaponCustomData = new WeaponCustomData();
			weaponCustomData.Ammo = 0;
			weaponCustomData.AmmoMax = 0;
			weaponCustomData.Mass = 0.02f;
			weaponCustomData.CustomData = false;
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle, null, null, weaponCustomData);
		}
	}

	private void DeadPlayer(DamageInfo damageInfo)
	{
		PlayerRoundManager.SetDeaths1();
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * 100f);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
		photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
		MurderModeManager.DeadPlayer();
		TimerManager.In(3f, delegate
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
		if (PhotonNetwork.player.ID == Detective && info.sender.ID != Murder)
		{
			WeaponManager.SetSelectWeapon(WeaponType.Knife, 0);
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, 0);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, 47);
			PlayerInput.instance.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle, null, null, null);
			MurderModeManager.DeadBystander();
			return;
		}
		PlayerRoundManager.SetKills1();
		if (damageInfo.HeadShot)
		{
			PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int10), 0, 10));
			PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int7), 0, 7));
			PlayerRoundManager.SetHeadshot1();
		}
		else
		{
			PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int7), 0, 7));
			PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int5), 0, 5));
		}
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		UIGameManager.instance.isScoreTimer = false;
		TimerManager.Cancel(TimerID);
		GameManager.UpdateRoundState(RoundState.EndRound);
		if (GameManager.CheckScore())
		{
			GameManager.LoadNextLevel(GameMode.Murder);
			return;
		}
		float delay = 8f - (float)(PhotonNetwork.time - info.timestamp);
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
		byte b = 2;
		bool flag = false;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].ID == Murder)
			{
				flag = true;
				if (playerList[i].GetDead())
				{
					b = 1;
					break;
				}
			}
		}
		if (GameManager.GetRoundState() != RoundState.StartRound && (!flag || b == 1))
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, 5f, "Bystander Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
			return;
		}
		for (int j = 0; j < playerList.Length; j++)
		{
			if (!playerList[j].GetDead() && playerList[j].ID != Murder)
			{
				b = 0;
				break;
			}
		}
		if (b == 2)
		{
			++GameManager.RedScore;
			GameManager.UpdateScore();
			UIMainStatus.Add("[@]", false, 5f, "Murder Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}
}
