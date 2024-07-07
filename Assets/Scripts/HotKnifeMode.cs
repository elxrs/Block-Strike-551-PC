using System.Collections.Generic;
using Photon;
using UnityEngine;

public class HotKnifeMode : PunBehaviour
{
	private int KnifeID;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
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
		PlayerTriggerDetector.isKick = true;
		UIGameManager.SetActiveScore(true);
		GameManager.SetStartDamageTime(1f);
		UIPanelManager.ShowPanel("Display");
		WeaponManager.MaxDamage = true;
		GameManager.SetChangeWeapons(false);
		CameraManager.ActiveStaticCamera();
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
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void ActivationWaitPlayer()
	{
		EventManager.Dispatch("WaitPlayer");
		GameManager.UpdateRoundState(RoundState.WaitPlayer);
		OnWaitPlayer();
		OnCreatePlayer(true);
		HitPlayer(-2);
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
		if (PhotonNetwork.playerList.Length <= 1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.PlayRound);
			int iD = PhotonNetwork.playerList[Random.Range(0, PhotonNetwork.playerList.Length)].ID;
			photonView.RPC("StartTimer", PhotonTargets.All, iD, true);
		}
	}

	[PunRPC]
	private void StartTimer(int knife, bool spawn, PhotonMessageInfo info)
	{
		UIGameManager.instance.isScoreTimer = false;
		KnifeID = knife;
		float num = 45f;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		UIGameManager.StartScoreTimer(num, StopTimer);
		OnCreatePlayer(spawn);
	}

	private void StopTimer()
	{
		if (PhotonNetwork.player.ID == KnifeID)
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			DamageInfo damageInfo = DamageInfo.Get(101, Vector3.zero, Team.Blue, playerInput.PlayerWeapon.GetSelectedWeaponData().ID, 0, PhotonNetwork.player.ID, false);
			playerInput.Damage(damageInfo);
		}
	}

	private void OnCreatePlayer(bool spawn)
	{
		if (PhotonNetwork.player.GetTeam() != 0)
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			if (spawn)
			{
				playerInput.SetHealth(nValue.int100);
				CameraManager.DeactiveAll();
				DrawElements playerIDSpawn = GameManager.GetPlayerIDSpawn();
				GameManager.GetController().ActivePlayer(playerIDSpawn.GetSpawnPosition(), playerIDSpawn.GetSpawnRotation());
				HitPlayer(KnifeID);
			}
			else if (!PhotonNetwork.player.GetDead())
			{
				HitPlayer(KnifeID);
			}
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		if (damageInfo.PlayerID == PhotonNetwork.player.ID)
		{
			++GameManager.RedScore;
			PhotonNetwork.player.SetDeaths1();
			PlayerRoundManager.SetDeaths1();
			UIStatus.Add(Utils.KillerStatus(damageInfo));
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
			UIStatus.Add("[@]: " + Utils.GetTeamHexColor(PhotonNetwork.player), false, "HotKnife");
			photonView.RPC("HitPlayer", PhotonTargets.All, PhotonNetwork.player.ID);
		}
	}

	[PunRPC]
	private void HitPlayer(int knife)
	{
		KnifeID = knife;
		if (PhotonNetwork.player.GetTeam() != 0 && !PhotonNetwork.player.GetDead())
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(nValue.int100);
			if (PhotonNetwork.player.ID == knife)
			{
				GameManager.OnSelectTeam(Team.Red);
				GameManager.GetController().SetTeam(Team.Red);
				WeaponManager.SetSelectWeapon(WeaponType.Knife, 4);
				WeaponManager.SetSelectWeapon(WeaponType.Pistol, 0);
				WeaponManager.SetSelectWeapon(WeaponType.Rifle, 0);
				playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
				UIGameManager.SetHealthLabel(0);
			}
			else if (WeaponManager.HasSelectWeapon(WeaponType.Knife))
			{
				GameManager.OnSelectTeam(Team.Blue);
				GameManager.GetController().SetTeam(Team.Blue);
				WeaponManager.SetSelectWeapon(WeaponType.Knife, 0);
				WeaponManager.SetSelectWeapon(WeaponType.Pistol, 3);
				WeaponManager.SetSelectWeapon(WeaponType.Rifle, 0);
				TimerManager.In(0.1f, delegate
				{
					playerInput.PlayerWeapon.GetWeaponData(WeaponType.Pistol).AmmoMax = 0;
					playerInput.PlayerWeapon.GetWeaponData(WeaponType.Pistol).Ammo = 0;
					UIGameManager.SetAmmoLabel(0, -1);
					UIGameManager.SetHealthLabel(0);
				});
				playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Pistol);
			}
		}
		if (PhotonNetwork.isMasterClient && !UIGameManager.instance.isScoreTimer && GameManager.GetRoundState() == RoundState.PlayRound)
		{
			CheckPlayers();
		}
	}

	[PunRPC]
	private void OnFinishRound(int id, PhotonMessageInfo info)
	{
		UIGameManager.instance.isScoreTimer = false;
		GameManager.UpdateRoundState(RoundState.EndRound);
		if (PhotonNetwork.player.ID == id)
		{
			PhotonNetwork.player.SetKills1();
			PlayerRoundManager.SetXP(PhotonNetwork.playerList.Length);
			PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int7), 0, 7));
		}
		float delay = 8f - (float)(PhotonNetwork.time - info.timestamp);
		TimerManager.In(delay, delegate
		{
			OnStartRound();
		});
	}

	private void SelectKnife()
	{
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		List<int> list = new List<int>();
		for (int i = 0; i < playerList.Length; i++)
		{
			if (!playerList[i].GetDead())
			{
				list.Add(i);
			}
		}
		int iD = playerList[list[Random.Range(0, list.Count)]].ID;
		photonView.RPC("StartTimer", PhotonTargets.All, iD, false);
		UIStatus.Add("[@]: " + Utils.GetTeamHexColor(PhotonPlayer.Find(iD)), false, "HotKnife");
	}

	[PunRPC]
	private void CheckPlayers()
	{
		if (!PhotonNetwork.isMasterClient || GameManager.GetRoundState() == RoundState.EndRound)
		{
			return;
		}
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		int num = -1;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (!playerList[i].GetDead())
			{
				num = ((num != -1) ? (-2) : playerList[i].ID);
			}
		}
		if (num == -1)
		{
			UIMainStatus.Add("[@]", false, 5f, "Draw");
			photonView.RPC("OnFinishRound", PhotonTargets.All, -2);
			return;
		}
		if (num >= 0)
		{
			++GameManager.BlueScore;
			PhotonPlayer photonPlayer = PhotonPlayer.Find(num);
			if (photonPlayer.ID == PhotonNetwork.player.ID)
			{
				++GameManager.BlueScore;
			}
			UIMainStatus.Add(photonPlayer.NickName + " [@]", false, 5f, "Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All, photonPlayer.ID);
			return;
		}
		bool flag = false;
		for (int j = 0; j < playerList.Length; j++)
		{
			if (playerList[j].ID == KnifeID && !playerList[j].GetDead())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SelectKnife();
		}
	}
}
