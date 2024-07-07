using UnityEngine;

public class MeetingMode : MonoBehaviour
{
	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.Meeting)
		{
			Destroy(this);
		}
	}

	private void Start()
	{
		GameManager.UpdateRoundState(RoundState.PlayRound);
		GameManager.SetStartDamageTime(nValue.int2);
		UIGameManager.SetActiveScore(false);
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
		GameManager.GetController().ActivePlayer(GameManager.GetTeamSpawn().GetSpawnPosition(), GameManager.GetTeamSpawn().GetSpawnRotation());
		WeaponManager.SetSelectWeapon(WeaponType.Pistol, nValue.int0);
		WeaponManager.SetSelectWeapon(WeaponType.Rifle, nValue.int0);
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
		playerInput.PlayerWeapon.CanFire = false;
	}

	private void OnDeadPlayer(DamageInfo damageInfo)
	{
		UIDeathScreen.Show(damageInfo);
		UIStatus.Add(Utils.KillerStatus(damageInfo));
		Vector3 ragdollForce = Utils.GetRagdollForce(GameManager.GetController().PlayerInput.PlayerTransform.position, damageInfo.AttackPosition);
		CameraManager.ActiveDeadCamera(GameManager.GetController().PlayerInput.FPCamera.Transform.position, GameManager.GetController().PlayerInput.FPCamera.Transform.eulerAngles, ragdollForce * nValue.int100);
		GameManager.GetController().DeactivePlayer(ragdollForce, damageInfo.HeadShot);
		TimerManager.In(nValue.int3, delegate
		{
			OnRevivalPlayer();
		});
	}

	public void OnClimb(bool active)
	{
		PlayerInput.instance.SetClimb(active);
	}

	public void OnIce(bool active)
	{
		PlayerInput.instance.SetMoveIce(active);
	}

	public void OnTrampoline(float force)
	{
		PlayerInput.instance.FPController.AddForce(new Vector3(0f, force, 0f));
	}

	public void OnSize(float size)
	{
		PlayerSkin[] array = FindObjectsOfType<PlayerSkin>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Controller.transform.localScale = Vector3.one * size;
		}
	}

	public void GiveKnife(int weaponID)
	{
		WeaponManager.SetSelectWeapon(WeaponType.Knife, weaponID);
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Knife);
	}
}
