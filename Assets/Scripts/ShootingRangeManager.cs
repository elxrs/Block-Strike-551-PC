using UnityEngine;

public class ShootingRangeManager : MonoBehaviour
{
	public Transform Spawn;

	public GameObject ChatButton;

	public GameObject StatsButton;

	public static bool ShowDamage;

	private void Start()
	{
		CameraManager.ActiveStaticCamera();
		TimerManager.In(0.05f, delegate
		{
			ShowDamage = Settings.ShowDamage;
			UIPanelManager.ShowPanel("Display");
			InputManager.Init();
			WeaponManager.Init();
			CreatePlayer();
			UISelectWeapon.AllWeapons = true;
			UISelectWeapon.SelectedUpdateWeaponManager = true;
			UIGameManager.SetActiveScore(false, 0);
			ChatButton.SetActive(false);
			StatsButton.SetActive(false);
		});
	}

	private void CreatePlayer()
	{
		PlayerInput playerInput = GameManager.GetController().PlayerInput;
		playerInput.SetHealth(0);
		CameraManager.DeactiveAll();
		GameManager.GetController().ActivePlayer(Spawn.position, Spawn.eulerAngles);
		playerInput.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle);
	}
}
