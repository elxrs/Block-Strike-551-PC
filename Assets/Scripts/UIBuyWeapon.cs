using System;
using UnityEngine;

public class UIBuyWeapon : MonoBehaviour
{
	[Serializable]
	public class Weapon
	{
		[SelectedWeapon]
		public int weapon;

		public CryptoInt money;
	}

	public Weapon[] Weapons;

	public UILabel WeaponNameLabel;

	public UISprite WeaponSprite;

	public int SelectWeapon;

	private static UILabel MoneyLabel;

	private static CryptoInt money = 800;

	public static int Money
	{
		get
		{
			return money;
		}
		set
		{
			money = value;
			money = Mathf.Clamp(money, nValue.int0, nValue.int15000);
			MoneyLabel.text = "$ " + money.ToString();
		}
	}

	private void Start()
	{
		SelectWeapon = 0;
		UpdateSelectedWeapon();
	}

	public void Left()
	{
		SelectWeapon--;
		if (SelectWeapon < 0)
		{
			SelectWeapon = Weapons.Length - 1;
		}
		UpdateSelectedWeapon();
	}

	public void Right()
	{
		SelectWeapon++;
		if (SelectWeapon > Weapons.Length - 1)
		{
			SelectWeapon = 0;
		}
		UpdateSelectedWeapon();
	}

	public void Buy()
	{
		if (Weapons[SelectWeapon].money > Money)
		{
			UIToast.Show(Localization.Get("Not enough money"));
			return;
		}
		if (Weapons[SelectWeapon].weapon == PlayerInput.instance.PlayerWeapon.GetWeaponData(WeaponManager.GetWeaponData(Weapons[SelectWeapon].weapon).Type).ID)
		{
			UIToast.Show(Localization.Get("Already available"));
			return;
		}
		Money -= Weapons[SelectWeapon].money;
		WeaponManager.SetSelectWeapon(Weapons[SelectWeapon].weapon);
		PlayerInput.instance.PlayerWeapon.UpdateWeaponAll(WeaponManager.GetWeaponData(Weapons[SelectWeapon].weapon).Type);
		UpdateSelectedWeapon();
	}

	private void UpdateSelectedWeapon()
	{
		WeaponData weaponData = WeaponManager.GetWeaponData(Weapons[SelectWeapon].weapon);
		WeaponSkinData weaponSkin = WeaponManager.GetWeaponSkin(weaponData.ID, SaveLoadManager.GetWeaponSkinSelected(weaponData.ID));
		WeaponNameLabel.text = weaponData.Name + "  |  $" + Weapons[SelectWeapon].money;
		WeaponSprite.spriteName = string.Concat(weaponData.ID, "-", weaponSkin.ID);
		WeaponSprite.width = (int)GameSettings.instance.WeaponsCaseSize[weaponData.ID - 1].x;
		WeaponSprite.height = (int)GameSettings.instance.WeaponsCaseSize[weaponData.ID - 1].y;
	}

	public static void SetActive(bool active)
	{
		if (MoneyLabel == null)
		{
			MoneyLabel = UIElements.Get<UILabel>("MoneyLabel");
		}
		MoneyLabel.cachedGameObject.SetActive(active);
	}
}
