using System.Collections.Generic;
using Photon;
using UnityEngine;

public class HungerGames : PunBehaviour
{
	private List<int> UsedBox = new List<int>();

	private static HungerGames instance;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.HungerGames)
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
		instance = this;
		UIGameManager.SetActiveScore(true, nValue.int20);
		GameManager.SetStartDamageTime(nValue.int3);
		GameManager.SetFriendDamage(true);
		UIPanelManager.ShowPanel("Display");
		GameManager.SetChangeWeapons(false);
		GameManager.SetGlobalChat(false);
		CameraManager.ActiveStaticCamera();
		UIPlayerStatistics.isOnlyBluePanel = true;
		TimerManager.In(nValue.float05, delegate
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
		if (GameManager.GetRoundState() != 0)
		{
			CheckPlayers();
		}
		TimerManager.In(nValue.float15, delegate
		{
			string text = Utils.ArrayToString(UsedBox.ToArray());
			if (!string.IsNullOrEmpty(text))
			{
				photonView.RPC("PhotonHideBoxes", playerConnect, text);
			}
		});
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
		EventManager.Dispatch("StartRound");
		UsedBox.Clear();
		if (PhotonNetwork.player.GetTeam() != 0)
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(nValue.int100);
			CameraManager.DeactiveAll();
			playerInput.FPCamera.GetComponent<Camera>().farClipPlane = nValue.int300;
			DrawElements playerIDSpawn = GameManager.GetPlayerIDSpawn();
			GameManager.GetController().ActivePlayer(playerIDSpawn.GetSpawnPosition(), playerIDSpawn.GetSpawnRotation());
			WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int4);
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		PlayerRoundManager.SetDeaths1();
		UIStatus.Add(Utils.KillerStatus(damageInfo));
		UIDeathScreen.Show(damageInfo);
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
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
		GameManager.UpdateRoundState(RoundState.EndRound);
		if (GameManager.CheckScore())
		{
			GameManager.LoadNextLevel(GameMode.HungerGames);
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
		int num = -nValue.int1;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (!playerList[i].GetDead())
			{
				if (num != -nValue.int1)
				{
					flag = false;
					break;
				}
				num = playerList[i].ID;
				flag = true;
			}
		}
		if (flag)
		{
			++GameManager.BlueScore;
			GameManager.UpdateScore();
			photonView.RPC("OnFinishRound", PhotonTargets.All);
			UIMainStatus.Add(PhotonPlayer.Find(num).NickName + " [@]", false, nValue.int5, "Win");
		}
	}

	public static void PickupBox(int id)
	{
		instance.photonView.RPC("MasterPickupBox", PhotonTargets.MasterClient, id);
	}

	[PunRPC]
	private void MasterPickupBox(int id, PhotonMessageInfo info)
	{
		if (!UsedBox.Contains(id))
		{
			photonView.RPC("EventPickupBox", PhotonTargets.All, id, info.sender.ID);
		}
	}

	[PunRPC]
	private void EventPickupBox(int id, int pickupPlayer)
	{
		UsedBox.Add(id);
		EventManager.Dispatch("EventPickupBox", id, pickupPlayer);
	}

	public static void HideBoxes(int[] idBoxes)
	{
		string text = Utils.ArrayToString(idBoxes);
		instance.photonView.RPC("PhotonHideBoxes", PhotonTargets.All, text);
	}

	[PunRPC]
	private void PhotonHideBoxes(string data)
	{
		int[] ids = Utils.StringToArrayInt(data);
		TimerManager.In(nValue.float01, delegate
		{
			for (int i = nValue.int0; i < ids.Length; i++)
			{
				UsedBox.Add(ids[i]);
				EventManager.Dispatch("EventPickupBox", ids[i], -nValue.int1);
			}
		});
	}

	public static void SetWeapon(int weaponID)
	{
		WeaponType type = WeaponManager.GetWeaponData(weaponID).Type;
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		WeaponManager.SetSelectWeapon(weaponID);
		WeaponCustomData weaponCustomData = new WeaponCustomData();
		weaponCustomData.AmmoMax = nValue.int0;
		playerInput.PlayerWeapon.UpdateWeapon(type, true, weaponCustomData);
	}
}
