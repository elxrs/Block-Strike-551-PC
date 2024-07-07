using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;

public class UISelectWeapon : MonoBehaviour
{
	public WeaponType Weapon;

	public UILabel WeaponNameLabel;

	public UISprite WeaponSprite;

	public float Size = 1f;

	public UILabel ChangeButtonLabel;

	private bool SkinMode;

	private List<string> WeaponList = new List<string>();

	private List<int> SkinList = new List<int>();

	private int SelectWeapon;

	private int SelectSkin;

	private static bool isChange = false;

	public static CryptoBool AllWeapons = false;

	public static CryptoBool SelectedUpdateWeaponManager = false;

	private void Start()
	{
		UpdateWeaponsName();
		GetSelectWeapon();
		UpdateSelectedWeapon();
		if (ChangeButtonLabel != null)
		{
			ChangeButtonLabel.text = Localization.Get("Weapons");
		}
	}

	[SkipRename]
	public void Close()
	{
		if (SelectedUpdateWeaponManager && isChange)
		{
			PlayerInput.instance.PlayerWeapon.UpdateWeaponAll();
			isChange = false;
		}
	}

	[SkipRename]
	public void Left()
	{
		if (SkinMode)
		{
			SelectSkin--;
			if (SelectSkin < 0)
			{
				SelectSkin = SkinList.Count - 1;
			}
			UpdateSelectedSkin();
			return;
		}
		SelectWeapon--;
		if (SelectWeapon < 0)
		{
			SelectWeapon = WeaponList.Count - 1;
		}
		if (!AllWeapons)
		{
			SaveLoadManager.SetWeaponSelected(Weapon, WeaponManager.GetWeaponID(WeaponList[SelectWeapon]));
		}
		UpdateSelectedWeapon();
	}

	[SkipRename]
	public void Right()
	{
		if (SkinMode)
		{
			SelectSkin++;
			if (SelectSkin > SkinList.Count - 1)
			{
				SelectSkin = 0;
			}
			UpdateSelectedSkin();
			return;
		}
		SelectWeapon++;
		if (SelectWeapon > WeaponList.Count - 1)
		{
			SelectWeapon = 0;
		}
		if (!AllWeapons)
		{
			SaveLoadManager.SetWeaponSelected(Weapon, WeaponManager.GetWeaponID(WeaponList[SelectWeapon]));
		}
		UpdateSelectedWeapon();
	}

	private void GetSelectWeapon()
	{
		int weaponSelected = SaveLoadManager.GetWeaponSelected(Weapon);
		string weaponName = WeaponManager.GetWeaponName(weaponSelected);
		for (int i = 0; i < WeaponList.Count; i++)
		{
			if (WeaponList[i] == weaponName)
			{
				SelectWeapon = i;
				break;
			}
		}
	}

	private void UpdateWeaponsName()
	{
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			if (GameSettings.instance.Weapons[i].Type != Weapon)
			{
				continue;
			}
			int num = GameSettings.instance.Weapons[i].ID;
			string item = GameSettings.instance.Weapons[i].Name;
			if (!SaveLoadManager.GetWeapon(num) && !AllWeapons)
			{
				continue;
			}
			if (num == 4 || num == 3 || num == 12)
			{
				WeaponList.Insert(0, item);
			}
			else
			{
				if ((bool)GameSettings.instance.Weapons[i].Lock)
				{
					continue;
				}
				if ((bool)GameSettings.instance.Weapons[i].Secret)
				{
					if (SaveLoadManager.GetWeapon(GameSettings.instance.Weapons[i].ID))
					{
						WeaponList.Add(item);
					}
				}
				else
				{
					WeaponList.Add(item);
				}
			}
		}
	}

	private void UpdateSelectedWeapon()
	{
		WeaponData weaponData = WeaponManager.GetWeaponData(WeaponList[SelectWeapon]);
		WeaponManager.SetSelectWeapon(Weapon, weaponData.ID);
		WeaponSkinData weaponSkin = WeaponManager.GetWeaponSkin(weaponData.ID, SaveLoadManager.GetWeaponSkinSelected(weaponData.ID));
		WeaponNameLabel.text = string.Concat(weaponData.Name, "  |  ", GetWeaponSkinRarityColor(weaponSkin));
		WeaponSprite.spriteName = string.Concat(weaponData.ID, "-", weaponSkin.ID);
		WeaponSprite.width = (int)(GameSettings.instance.WeaponsCaseSize[weaponData.ID - 1].x * Size);
		WeaponSprite.height = (int)(GameSettings.instance.WeaponsCaseSize[weaponData.ID - 1].y * Size);
		if ((bool)SelectedUpdateWeaponManager)
		{
			isChange = true;
		}
	}

	private string GetWeaponSkinRarityColor(WeaponSkinData skin)
	{
		switch (skin.Quality)
		{
		case WeaponSkinQuality.Default:
		case WeaponSkinQuality.Normal:
			return Utils.ColorToHex(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), skin.Name);
		case WeaponSkinQuality.Basic:
			return Utils.ColorToHex(new Color32(54, 189, byte.MaxValue, byte.MaxValue), skin.Name);
		case WeaponSkinQuality.Professional:
			return Utils.ColorToHex(new Color32(byte.MaxValue, 0, 0, byte.MaxValue), skin.Name);
		case WeaponSkinQuality.Legendary:
			return Utils.ColorToHex(new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue), skin.Name);
		default:
			return skin.Name;
		}
	}

	[SkipRename]
	public void ChangeMode()
	{
		SkinMode = !SkinMode;
		if (SkinMode)
		{
			GetWeaponSkins();
			if (ChangeButtonLabel != null)
			{
				ChangeButtonLabel.text = Localization.Get("Skins");
			}
		}
		else if (ChangeButtonLabel != null)
		{
			ChangeButtonLabel.text = Localization.Get("Weapons");
		}
	}

	private void GetWeaponSkins()
	{
		SkinList.Clear();
		int weaponID = WeaponManager.GetWeaponID(WeaponList[SelectWeapon]);
		int weaponSkinSelected = SaveLoadManager.GetWeaponSkinSelected(weaponID);
		WeaponStoreData weaponStoreData = WeaponManager.GetWeaponStoreData(weaponID);
		for (int i = 0; i < weaponStoreData.Skins.Count; i++)
		{
			if (SaveLoadManager.GetWeaponSkin(weaponID, weaponStoreData.Skins[i].ID))
			{
				SkinList.Add(weaponStoreData.Skins[i].ID);
				if (weaponSkinSelected == weaponStoreData.Skins[i].ID)
				{
					SelectSkin = i;
				}
			}
		}
	}

	private void UpdateSelectedSkin()
	{
		WeaponData weaponData = WeaponManager.GetWeaponData(WeaponList[SelectWeapon]);
		WeaponManager.SetSelectWeapon(Weapon, weaponData.ID);
		SaveLoadManager.SetWeaponSkinSelected(weaponData.ID, SkinList[SelectSkin]);
		WeaponSkinData weaponSkin = WeaponManager.GetWeaponSkin(weaponData.ID, SaveLoadManager.GetWeaponSkinSelected(weaponData.ID));
		WeaponNameLabel.text = string.Concat(weaponData.Name, "  |  ", GetWeaponSkinRarityColor(weaponSkin));
		WeaponSprite.spriteName = string.Concat(weaponData.ID, "-", weaponSkin.ID);
		if ((bool)SelectedUpdateWeaponManager)
		{
			isChange = true;
		}
	}
}
