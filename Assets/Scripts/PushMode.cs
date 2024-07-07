using System.Collections.Generic;
using Photon;
using UnityEngine;

public class PushMode : PunBehaviour
{
	public GameObject[] blocks;

	private List<byte> activeBlocks = new List<byte>();

	private List<byte> deactiveBlocks = new List<byte>();

	public Material defaultBlock;

	public Material damageBlock;

	private int mainTimer;

	private int damageTimer;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		UIGameManager.SetActiveScore(false);
		GameManager.SetStartDamageTime(nValue.float02);
		GameManager.SetFriendDamage(true);
		UIPanelManager.ShowPanel("Display");
		CameraManager.ActiveStaticCamera();
		GameManager.SetChangeWeapons(false);
		WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int2);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
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
		if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateScore(playerConnect);
			photonView.RPC("PhotonHideBlock", playerConnect, deactiveBlocks.ToArray());
		}
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer playerDisconnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			CheckPlayers();
		}
	}

	[PunRPC]
	private void OnCreatePlayer()
	{
		if (PhotonNetwork.player.GetTeam() != 0)
		{
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(nValue.int100);
			CameraManager.DeactiveAll();
			DrawElements teamSpawn = GameManager.GetTeamSpawn(Team.Blue);
			GameManager.GetController().ActivePlayer(teamSpawn.GetSpawnPosition(), new Vector3(nValue.int0, Random.Range(nValue.int0, nValue.int360), nValue.int0));
			playerInput.PlayerWeapon.InfiniteAmmo = true;
			WeaponCustomData weaponCustomData = new WeaponCustomData();
			weaponCustomData.BodyDamage = nValue.int0;
			weaponCustomData.FaceDamage = nValue.int0;
			weaponCustomData.HandDamage = nValue.int0;
			weaponCustomData.LegDamage = nValue.int0;
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Pistol, null, weaponCustomData, null);
			playerInput.DamageForce = nValue.int40;
			playerInput.FPController.MotorDoubleJump = true;
		}
	}

	private void OnStartRound()
	{
		ResetBlocks();
		if (PhotonNetwork.playerList.Length <= nValue.int1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.PlayRound);
			photonView.RPC("PhotonStartRound", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void PhotonStartRound()
	{
		StartTimer();
		OnCreatePlayer();
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			if (damageInfo.PlayerID != -nValue.int1)
			{
				return;
			}
			PhotonNetwork.player.SetDeaths1();
			PlayerRoundManager.SetDeaths1();
			string text = Utils.GetTeamHexColor(PhotonNetwork.player) + " [@]";
			UIStatus.Add(text, false, "died");
			Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
			CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
			GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
			photonView.RPC("CheckPlayers", PhotonTargets.MasterClient);
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
	private void OnWinPlayer()
	{
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetXP(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int12), 0, 12));
		PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int8), 0, 8));
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		TimerManager.Cancel(mainTimer);
		TimerManager.Cancel(damageTimer);
		GameManager.UpdateRoundState(RoundState.EndRound);
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
		PhotonPlayer photonPlayer = null;
		for (int i = nValue.int0; i < playerList.Length; i++)
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
			UIMainStatus.Add(photonPlayer.NickName + " [@]", false, nValue.int5, "Win");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
			photonView.RPC("OnWinPlayer", photonPlayer);
		}
	}

	private void ResetBlocks()
	{
		for (int i = nValue.int0; i < blocks.Length; i++)
		{
			blocks[i].SetActive(true);
			blocks[i].GetComponent<Renderer>().material = defaultBlock;
		}
	}

	public void StartTimer()
	{
		activeBlocks.Clear();
		deactiveBlocks.Clear();
		for (byte b = 0; b < blocks.Length; b++)
		{
			activeBlocks.Add(b);
		}
		mainTimer = TimerManager.In(nValue.int3, -nValue.int1, nValue.int3, delegate
		{
			if (PhotonNetwork.isMasterClient && activeBlocks.Count != nValue.int1)
			{
				byte[] array = ((activeBlocks.Count > nValue.int200) ? new byte[nValue.int5] : ((activeBlocks.Count > nValue.int150) ? new byte[nValue.int4] : ((activeBlocks.Count <= nValue.int100) ? new byte[nValue.int2] : new byte[nValue.int3])));
				if (activeBlocks.Count <= nValue.int2)
				{
					array = new byte[nValue.int1];
				}
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = activeBlocks[Random.Range(nValue.int0, activeBlocks.Count)];
					activeBlocks.Remove(array[i]);
				}
				photonView.RPC("PhotonHideBlock", PhotonTargets.All, array);
			}
		});
	}

	[PunRPC]
	private void PhotonHideBlock(byte[] ids)
	{
		if (ids.Length > nValue.int5)
		{
			if (GameManager.GetRoundState() == RoundState.PlayRound)
			{
				for (int i = nValue.int0; i < ids.Length; i++)
				{
					blocks[ids[i]].SetActive(false);
				}
			}
			return;
		}
		for (int j = nValue.int0; j < ids.Length; j++)
		{
			byte id = ids[j];
			blocks[id].GetComponent<Renderer>().material = damageBlock;
			activeBlocks.Remove(id);
			deactiveBlocks.Add(id);
			damageTimer = TimerManager.In(nValue.float15, delegate
			{
				if (GameManager.GetRoundState() == RoundState.PlayRound)
				{
					blocks[id].SetActive(false);
				}
			});
		}
	}
}
