using System.Collections.Generic;
using Photon;
using UnityEngine;

public class SpleefMode : PunBehaviour
{
	public SpleefBlock[] blocks;

	private List<short> activeBlocks = new List<short>();

	private List<short> deactiveBlocks = new List<short>();

	public static SpleefMode instance;

	private void Awake()
	{
		instance = this;
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		UIGameManager.SetActiveScore(false);
		GameManager.SetStartDamageTime(nValue.float02);
		UIPanelManager.ShowPanel("Display");
		CameraManager.ActiveStaticCamera();
		GameManager.SetChangeWeapons(false);
		WeaponManager.SetSelectWeapon(WeaponType.Knife, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int3);
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

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
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
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Pistol);
		}
		activeBlocks.Clear();
		deactiveBlocks.Clear();
		for (short num = 0; num < blocks.Length; num++)
		{
			activeBlocks.Add(num);
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
			photonView.RPC("OnCreatePlayer", PhotonTargets.All);
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
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
			blocks[i].cachedGameObject.SetActive(true);
		}
	}

	public void Damage(int id)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			photonView.RPC("PhotonDamage", PhotonTargets.All, (short)id);
		}
	}

	[PunRPC]
	private void PhotonDamage(short id)
	{
		activeBlocks.Remove(id);
		deactiveBlocks.Add(id);
		SpleefBlock spleefBlock = blocks[id];
		spleefBlock.cachedGameObject.SetActive(false);
	}

	[PunRPC]
	private void PhotonHideBlock(short[] ids)
	{
		for (int i = nValue.int0; i < ids.Length; i++)
		{
			activeBlocks.Remove(ids[i]);
			deactiveBlocks.Add(ids[i]);
			SpleefBlock spleefBlock = blocks[ids[i]];
			spleefBlock.cachedGameObject.SetActive(false);
		}
	}
}
