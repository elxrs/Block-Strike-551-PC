using UnityEngine;

public class PickupWeaponTrigger : MonoBehaviour
{
	[SelectedWeapon(WeaponType.Rifle)]
	public int Weapon;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player"))
		{
			return;
		}
		PlayerInput player = other.GetComponent<PlayerInput>();
		if (player != null && !player.PlayerWeapon.GetWeaponData(WeaponType.Rifle).Enabled)
		{
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, Weapon);
			player.PlayerWeapon.UpdateWeapon(WeaponType.Rifle, true);
			TimerManager.In(0.05f, delegate
			{
				player.PlayerWeapon.SetWeapon(WeaponType.Rifle, false);
			});
		}
	}
}
