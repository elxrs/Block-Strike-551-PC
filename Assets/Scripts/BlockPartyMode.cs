using Photon;
using UnityEngine;

public class BlockPartyMode : PunBehaviour
{
	public nGameObject[] blocks;

	public Color32[] colors;

	public int selectColor;

	public MeshFilter line;

	private System.Random rand = new System.Random(5);

	private int[] timers = new int[2];

	private float duration = 5f;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		GameManager.SetPauseInterval(200);
		UIGameManager.SetActiveScore(false);
		GameManager.SetStartDamageTime(nValue.float02);
		UIPanelManager.ShowPanel("Display");
		CameraManager.ActiveStaticCamera();
		UIPlayerStatistics.isOnlyBluePanel = true;
		GameManager.SetChangeWeapons(false);
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		TimerManager.In(nValue.float05, delegate
		{
			PlayerInput.instance.AfkEnabled = true;
			PlayerInput.instance.AfkDuration = 15f;
			PlayerInput.instance.StartAFK();
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
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.SetHealth(nValue.int100);
			CameraManager.DeactiveAll();
			DrawElements teamSpawn = GameManager.GetTeamSpawn(Team.Blue);
			GameManager.GetController().ActivePlayer(teamSpawn.GetSpawnPosition(), new Vector3(nValue.int0, Random.Range(nValue.int0, nValue.int360), nValue.int0));
			playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
		}
	}

	private void OnStartRound()
	{
		if (PhotonNetwork.playerList.Length <= nValue.int1)
		{
			ActivationWaitPlayer();
		}
		else if (PhotonNetwork.isMasterClient)
		{
			GameManager.UpdateRoundState(RoundState.PlayRound);
			photonView.RPC("PhotonStartRound", PhotonTargets.All, Random.Range(0, 100000));
		}
	}

	[PunRPC]
	private void PhotonStartRound(int seed)
	{
		rand = new System.Random(seed);
		duration = nValue.int5;
		OnCreatePlayer();
		SelectRandomColor();
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
		PlayerRoundManager.SetXP(PhotonNetwork.playerList.Length);
		PlayerRoundManager.SetMoney(Mathf.Clamp(Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / nValue.int12 * nValue.int7), 0, 7));
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		GameManager.UpdateRoundState(RoundState.EndRound);
		UIGameManager.StopDuration();
		TimerManager.Cancel(timers);
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

	private void SetColor()
	{
		Color32[] array = new Color32[blocks[nValue.int0].cachedMeshFilter.mesh.vertices.Length];
		for (int i = nValue.int0; i < blocks.Length; i++)
		{
			int num = rand.Next(nValue.int0, colors.Length);
			for (int j = nValue.int0; j < array.Length; j++)
			{
				array[j] = colors[num];
			}
			blocks[i].cachedMeshFilter.mesh.colors32 = array;
			blocks[i].cachedGameObject.name = num.ToString();
		}
	}

	private void SetLineColor()
	{
		Color32[] array = new Color32[line.mesh.vertices.Length];
		for (int i = nValue.int0; i < array.Length; i++)
		{
			array[i] = colors[selectColor];
		}
		line.mesh.colors32 = array;
	}

	private void SelectRandomColor()
	{
		ResetBlocks();
		SetColor();
		selectColor = rand.Next(nValue.int0, colors.Length);
		SetLineColor();
		duration -= nValue.float01;
		duration = Mathf.Clamp(duration, 2f, 5f);
		timers[nValue.int0] = TimerManager.In(duration, DeactiveBlocks);
		UIGameManager.GetDurationSprite().color = new Color32((byte)(colors[selectColor].r - 40), (byte)(colors[selectColor].g - 40), (byte)(colors[selectColor].b - 40), byte.MaxValue);
		UIGameManager.StartDuration(duration);
	}

	private void DeactiveBlocks()
	{
		for (byte b = 0; b < blocks.Length; b++)
		{
			if (blocks[b].cachedGameObject.name != selectColor.ToString())
			{
				blocks[b].cachedGameObject.SetActive(false);
			}
		}
		timers[nValue.int1] = TimerManager.In(nValue.int2, SelectRandomColor);
	}
}
