using System;
using UnityEngine;

public class DropWeaponStatic : MonoBehaviour
{
	[SelectedWeapon]
	public int weaponID;

	public bool useCustomData;

	public bool updatePlayerData = true;

	public WeaponCustomData customData;

	public MeshAtlas[] weaponMeshes;

	private bool isActive;

	public event Action onDropWeaponEvent;

	private void OnEnable()
	{
		EventManager.AddListener<DamageInfo>("DeadPlayer", OnDeadPlayer);
		if (useCustomData && customData.Skin != -1)
		{
			for (int i = 0; i < weaponMeshes.Length; i++)
			{
				weaponMeshes[i].spriteName = weaponID + "-" + customData.Skin;
			}
		}
	}

	private void GetButtonDown(string name)
	{
		if (isActive && name == "Use")
		{
			if (updatePlayerData)
			{
				WeaponType type = WeaponManager.GetWeaponData(weaponID).Type;
				WeaponManager.SetSelectWeapon(type, weaponID);
				PlayerInput.instance.PlayerWeapon.UpdateWeapon(type, true, (!useCustomData) ? null : customData);
			}
			Deactive();
			if (onDropWeaponEvent != null)
			{
				onDropWeaponEvent();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerInput component = other.GetComponent<PlayerInput>();
			if (component != null)
			{
				isActive = true;
				InputManager.GetButtonDownEvent += GetButtonDown;
				UIControllerList.Use.cachedGameObject.SetActive(true);
				UIControllerList.UseText.text = Localization.Get("Pick up") + " " + WeaponManager.GetWeaponName(weaponID);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (isActive && other.CompareTag("Player"))
		{
			PlayerInput component = other.GetComponent<PlayerInput>();
			if (component != null)
			{
				Deactive();
			}
		}
	}

	private void OnDeadPlayer(DamageInfo info)
	{
		if (isActive)
		{
			Deactive();
		}
	}

	private void Deactive()
	{
		isActive = false;
		InputManager.GetButtonDownEvent -= GetButtonDown;
		UIControllerList.Use.cachedGameObject.SetActive(false);
		UIControllerList.UseText.text = string.Empty;
	}
}
