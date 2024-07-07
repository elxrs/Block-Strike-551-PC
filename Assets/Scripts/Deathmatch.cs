using UnityEngine;

public class Deathmatch : Photon.MonoBehaviour
{
	public CryptoInt MaxScore = 50;

	private CryptoInt BlueScore = 0;

	private CryptoInt RedScore = 0;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.Deathmatch)
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		GameManager.UpdateRoundState(RoundState.PlayRound);
		GameManager.SetStartDamageTime(nValue.int2);
		GameManager.SetFriendDamage(true);
		UIGameManager.SetActiveScore(true, MaxScore);
		GameManager.MaxScore = MaxScore;
		UIPanelManager.ShowPanel("Display");
		UIPlayerStatistics.isOnlyBluePanel = true;
		CameraManager.ActiveStaticCamera();
		TimerManager.In(nValue.float05, delegate
		{
			GameManager.OnSelectTeam(Team.Blue);
			OnRevivalPlayer();
		});
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
	}

	private void OnRevivalPlayer()
	{
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(nValue.int100);
		CameraManager.DeactiveAll();
		DrawElements randomSpawn = GameManager.GetRandomSpawn();
		GameManager.GetController().ActivePlayer(randomSpawn.GetSpawnPosition(), randomSpawn.GetSpawnRotation());
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		PhotonNetwork.player.SetDeaths1();
		PlayerRoundManager.SetDeaths1();
		++RedScore;
		UIGameManager.UpdateScoreLabel(MaxScore, BlueScore, RedScore);
		UIStatus.Add(Utils.KillerStatus(damageInfo));
		UIDeathScreen.Show(damageInfo);
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		if (damageInfo.isPlayerID)
		{
			photonView.RPC("OnKilledPlayer", PhotonPlayer.Find(damageInfo.PlayerID), damageInfo);
		}
		TimerManager.In(nValue.int3, delegate
		{
			OnRevivalPlayer();
		});
	}

	[PunRPC]
	private void OnKilledPlayer(DamageInfo damageInfo, PhotonMessageInfo info)
	{
		EventManager.Dispatch("KillPlayer", damageInfo);
		PhotonNetwork.player.SetKills1();
		PlayerRoundManager.SetKills1();
		UIDeathScreen.AddKill(info.sender.ID);
		++BlueScore;
		UIGameManager.UpdateScoreLabel(MaxScore, BlueScore, RedScore);
		if (BlueScore >= MaxScore)
		{
			OnScore(PhotonNetwork.player);
		}
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
	}

	public void OnScore(PhotonPlayer player)
	{
		photonView.RPC("PhotonOnScore", PhotonTargets.MasterClient, player.ID);
	}

	[PunRPC]
	private void PhotonOnScore(int playerID)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound)
		{
			PhotonPlayer photonPlayer = PhotonPlayer.Find(playerID);
			GameManager.UpdateRoundState(RoundState.EndRound);
			UIMainStatus.Add(photonPlayer.NickName + " [@]", false, nValue.int5, "Win");
			photonView.RPC("PhotonNextLevel", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void PhotonNextLevel()
	{
		GameManager.LoadNextLevel(GameMode.Deathmatch);
	}
}
