using Photon;
using UnityEngine;

public class BunnyHop : PunBehaviour
{
	private Vector3 StartSpawnPosition;

	private Quaternion StartSpawnRotation;

	private static BunnyHop instance;

	private void Awake()
	{
		if (PhotonNetwork.room.GetGameMode() != GameMode.BunnyHop)
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
		GameManager.UpdateRoundState(RoundState.PlayRound);
		UIGameManager.SetActiveScore(true, nValue.int0);
		GameManager.SetStartDamageTime(nValue.float01);
		UIPanelManager.ShowPanel("Display");
		GameManager.SetChangeWeapons(false);
		UIPlayerStatistics.isOnlyBluePanel = true;
		CameraManager.ActiveStaticCamera();
		TimerManager.In(nValue.float05, delegate
		{
			GameManager.OnSelectTeam(Team.Blue);
			StartSpawnPosition = GameManager.GetTeamSpawn(Team.Blue).GetTransform().position;
			StartSpawnRotation = GameManager.GetTeamSpawn(Team.Blue).GetTransform().rotation;
			PlayerInput playerInput = GameManager.GetController().PlayerInput;
			playerInput.BunnyHopEnabled = true;
			playerInput.FPController.MotorJumpForce = nValue.float02;
			playerInput.FPController.MotorAirSpeed = nValue.int1;
			OnRevivalPlayer();
			TimerManager.In(nValue.float15, delegate
			{
				if (PhotonNetwork.isMasterClient && !LevelManager.CustomMap)
				{
					photonView.RPC("StartTimer", PhotonTargets.All);
				}
			});
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void OnSpawnPlayer()
	{
		GameManager.GetController().SpawnPlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
	}

	private void OnRevivalPlayer()
	{
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(nValue.int100);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		++GameManager.RedScore;
		UIGameManager.UpdateScoreLabel(nValue.int0, GameManager.BlueScore, GameManager.RedScore);
		OnSpawnPlayer();
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (!LevelManager.CustomMap && PhotonNetwork.isMasterClient && UIGameManager.instance.isScoreTimer)
		{
			photonView.RPC("UpdateTimer", playerConnect, UIGameManager.instance.ScoreTimer - Time.time);
		}
	}

	[PunRPC]
	private void StartTimer(PhotonMessageInfo info)
	{
		float num = nValue.int900;
		num -= (float)(PhotonNetwork.time - info.timestamp);
		UIGameManager.StartScoreTimer(num, StopTimer);
	}

	[PunRPC]
	private void UpdateTimer(float time, PhotonMessageInfo info)
	{
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
			GameManager.UpdateRoundState(RoundState.EndRound);
			UIMainStatus.Add("[@]", false, nValue.int5, "Next Map");
			photonView.RPC("OnFinishRound", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void OnFinishRound(PhotonMessageInfo info)
	{
		GameManager.LoadNextLevel(GameMode.BunnyHop);
	}

	public static void FinishMap(int xp, int money)
	{
		Transform transform = GameManager.GetTeamSpawn().GetTransform();
		transform.position = instance.StartSpawnPosition;
		transform.rotation = instance.StartSpawnRotation;
		UIMainStatus.Add(PhotonNetwork.player.NickName + " [@]", false, nValue.int5, "Finished map");
		PlayerRoundManager.SetXP(xp);
		PlayerRoundManager.SetMoney(money);
		GameManager.GetController().SpawnPlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), Vector3.up * Random.Range(nValue.int0, nValue.int360));
		PhotonNetwork.player.SetKills1();
		++GameManager.BlueScore;
		UIGameManager.UpdateScoreLabel(nValue.int0, GameManager.BlueScore, GameManager.RedScore);
	}

	public static void SpawnDead()
	{
		instance.OnSpawnPlayer();
	}
}
