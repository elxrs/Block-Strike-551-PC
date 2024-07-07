using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

public class mg_100TrapsManager : PunBehaviour
{
	[Serializable]
	public struct LevelData
	{
		public GameObject Map;

		public Transform Spawn;
	}

	public CryptoInt SelectLevel = 1;

	public List<LevelData> Levels;

	private float LastTime;

	private byte pIndex;

	private PhotonView pView;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		GameManager.UpdateRoundState(RoundState.PlayRound);
		UIGameManager.SetActiveScore(true, nValue.int0);
		GameManager.SetStartDamageTime(nValue.float01);
		UIPanelManager.ShowPanel("Display");
		GameManager.SetChangeWeapons(false);
		UIPlayerStatistics.isOnlyBluePanel = true;
		CameraManager.ActiveStaticCamera();
		TimerManager.In(nValue.float005, delegate
		{
			byte[] array = new byte[nValue.int100];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (byte)(i + 1);
			}
			PhotonNetwork.SetSendingEnabled(null, array);
			pView = GameManager.GetController().photonView;
			UpdateGroup();
			InvokeRepeating("UpdateActiveGroup", nValue.int0, nValue.float1 / PhotonNetwork.sendRateOnSerialize);
		});
		TimerManager.In(nValue.float05, delegate
		{
			GameManager.OnSelectTeam(Team.Blue);
			GameManager.GetController().PlayerInput.FPController.MotorDoubleJump = true;
			OnRevivalPlayer();
			UIGameManager.UpdateScoreLabel(Levels.Count, GameManager.BlueScore, GameManager.RedScore);
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void OnSpawnPlayer()
	{
		GameManager.GetController().SpawnPlayer(Levels[SelectLevel - nValue.int1].Spawn.position, Levels[SelectLevel - nValue.int1].Spawn.eulerAngles);
	}

	private void OnRevivalPlayer()
	{
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(nValue.int100);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(Levels[SelectLevel - nValue.int1].Spawn.position, Levels[SelectLevel - nValue.int1].Spawn.eulerAngles);
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
		if (playerInput.PlayerTeam != Team.Blue)
		{
			GameManager.OnSelectTeam(Team.Blue);
		}
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		++GameManager.RedScore;
		UIGameManager.UpdateScoreLabel(Levels.Count, GameManager.BlueScore, GameManager.RedScore);
		OnSpawnPlayer();
	}

	public void NextLevel()
	{
		if (LastTime + 2f > Time.time)
		{
			PhotonNetwork.LeaveRoom();
		}
		LastTime = Time.time;
		++SelectLevel;
		if (Levels.Count + nValue.int1 <= SelectLevel)
		{
			SelectLevel = nValue.int1;
			UIMainStatus.Add(PhotonNetwork.player.NickName + " [@]", false, nValue.int5, "Finished map");
		}
		UpdateGroup();
		Levels[SelectLevel - nValue.int1].Map.SetActive(true);
		OnSpawnPlayer();
		PhotonNetwork.player.SetKills(SelectLevel);
		GameManager.BlueScore = SelectLevel;
		PlayerRoundManager.SetXP(nValue.int3);
		PlayerRoundManager.SetMoney(nValue.int5);
		UIGameManager.UpdateScoreLabel(Levels.Count, GameManager.BlueScore, GameManager.RedScore);
		if (SelectLevel == nValue.int1)
		{
			Levels[Levels.Count - nValue.int1].Map.SetActive(false);
		}
		else
		{
			Levels[SelectLevel - nValue.int2].Map.SetActive(false);
		}
	}

	private void UpdateGroup()
	{
		if (pView != null)
		{
			pView.group = (byte)(int)SelectLevel;
		}
		if (SelectLevel == nValue.int1)
		{
			PhotonNetwork.SetInterestGroups(new byte[1] { (byte)Levels.Count }, new byte[1] { (byte)(int)SelectLevel });
		}
		else
		{
			PhotonNetwork.SetInterestGroups(new byte[1] { (byte)(SelectLevel - nValue.int1) }, new byte[1] { (byte)(int)SelectLevel });
		}
	}

	private void UpdateActiveGroup()
	{
		pIndex++;
		if (pIndex >= 5)
		{
			pIndex = 0;
		}
		switch (pIndex)
		{
		case 0:
			pView.group = 0;
			break;
		case 1:
			pView.group = (byte)(int)SelectLevel;
			break;
		}
	}
}
