using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class SaveLoadManager
{
	public static bool HasPlayerName()
	{
		return ObscuredPrefs.HasKey("PlayerName");
	}

	public static void SetPlayerName(string name)
	{
		ObscuredPrefs.SetString("PlayerName", name);
	}

	public static string GetPlayerName()
	{
		return ObscuredPrefs.GetString("PlayerName", "Player " + Random.Range(10000, 99999));
	}

	public static void SetMoney(int money)
	{
		ObscuredPrefs.SetInt("Money", money);
	}

	public static void SetMoney1(int money)
	{
		ObscuredPrefs.SetInt("Money", Mathf.Clamp(GetMoney() + money, 0, 10_000_000));
	}

	public static int GetMoney()
	{
		return ObscuredPrefs.GetInt("Money", 100);
	}

	public static void SetGold(int gold)
	{
		ObscuredPrefs.SetInt("Gold", gold);
	}

	public static void SetGold1(int gold)
	{
		ObscuredPrefs.SetInt("Gold", Mathf.Clamp(GetGold() + gold, 0, 100_000));
	}

	public static int GetGold()
	{
		return ObscuredPrefs.GetInt("Gold", 10);
	}

	public static int GetWeaponSelected(WeaponType weaponType)
	{
		switch (weaponType)
		{
			case WeaponType.Knife:
				return ObscuredPrefs.GetInt("SelectKnife", 4);
			case WeaponType.Pistol:
				return ObscuredPrefs.GetInt("SelectPistol", 3);
			case WeaponType.Rifle:
				return ObscuredPrefs.GetInt("SelectRifle", 12);
			default:
				return 0;
		}
	}

	public static void SetWeaponSelected(WeaponType weaponType, int weaponID)
	{
		switch (weaponType)
		{
			case WeaponType.Knife:
				ObscuredPrefs.SetInt("SelectKnife", weaponID);
				return;
			case WeaponType.Pistol:
				ObscuredPrefs.SetInt("SelectPistol", weaponID);
				return;
			case WeaponType.Rifle:
				ObscuredPrefs.SetInt("SelectRifle", weaponID);
				return;
			default:
				return;
		}
	}

	public static void SetDeaths(int deaths)
	{
		ObscuredPrefs.SetInt("Deaths", deaths);
	}

	public static void SetDeaths1()
	{
		ObscuredPrefs.SetInt("Deaths", GetDeaths() + 1);
	}

	public static int GetDeaths()
	{
		return ObscuredPrefs.GetInt("Deaths", 0);
	}

	public static void SetKills(int kills)
	{
		ObscuredPrefs.SetInt("Kills", kills);
	}

	public static void SetKills1()
	{
		ObscuredPrefs.SetInt("Kills", GetKills() + 1);
	}

	public static int GetKills()
	{
		return ObscuredPrefs.GetInt("Kills", 0);
	}

	public static void SetHeadshot(int headshot)
	{
		ObscuredPrefs.SetInt("Headshot", headshot);
	}

	public static void SetHeadshot1()
	{
		ObscuredPrefs.SetInt("Headshot", GetHeadshot() + 1);
	}

	public static int GetHeadshot()
	{
		return ObscuredPrefs.GetInt("Headshot", 0);
	}

	public static bool GetWeapon(int weaponID)
	{
		for (int i = 0; i < 50; i++)
		{
			SetWeaponSkin(i, 0);//чтобы standard скины были сразу открыты
		}
		return ObscuredPrefs.GetBool("Weapon" + weaponID, false);
	}

	public static void SetWeapon(int weaponID)
	{
		ObscuredPrefs.SetBool("Weapon" + weaponID, true);
	}

	public static void SetClan(string clan)
	{
		ObscuredPrefs.SetString("ClanTag", clan);
	}

	public static string GetClan()
	{
		return ObscuredPrefs.GetString("ClanTag", "");
	}

	public static void SetPlayerXP(int xp)
	{
		ObscuredPrefs.SetInt("PlayerXP", xp);
	}

	public static int GetPlayerXP()
	{
		return ObscuredPrefs.GetInt("PlayerXP", 0);
	}

	public static void SetPlayerLevel(int level)
	{
		ObscuredPrefs.SetInt("PlayerLevel", level);
	}

	public static int GetPlayerLevel()
	{
		return ObscuredPrefs.GetInt("PlayerLevel", 1);
	}

	public static void SetWeaponSkin(int weaponID, int skin)
	{
		ObscuredPrefs.SetBool(string.Concat(new object[]
		{
			"WeaponSkin",
			weaponID,
			"-",
			skin
		}), true);
	}

	public static bool GetWeaponSkin(int weaponID, int skin)
	{
		return ObscuredPrefs.GetBool(string.Concat(new object[]
		{
			"WeaponSkin",
			weaponID,
			"-",
			skin
		}), false);
	}

	public static int GetWeaponSkinSelected(int weaponID)
	{
		return ObscuredPrefs.GetInt("WeaponSkinSelected" + weaponID, 0);
	}

	public static void SetWeaponSkinSelected(int WeaponID, int skin)
	{
		ObscuredPrefs.SetInt("WeaponSkinSelected" + WeaponID, skin);
	}

	public static int GetMaxXP()
	{
		return 500 + 500 * GetPlayerLevel();
	}

	public static void SetPlayerXP1(int xp)
	{
		int num = GetPlayerLevel();
		int num2 = GetPlayerXP() + xp;
		int num3 = GetMaxXP();
		if (num2 >= num3)
		{
			if (num == 1000)
			{
				num2 = num3;
			}
			else
			{
				num++;
				num2 -= num3;
				num2 = Mathf.Max(num2, 0);
				num3 = 500 + 500 * num;
				SetPlayerLevel(num);
				if (LevelManager.GetSceneName() == "Menu")
				{
					UIToast.Show(Localization.Get("New Level") + " " + num);
					SetMoney1(500);
					SetGold1(10);
				}
			}
		}
		SetPlayerXP(num2);
	}

	public static bool GetFireStat(int weaponID, int skin)
	{
		return ObscuredPrefs.GetInt(string.Concat(new object[]
		{
			"FireStatCounter",
			weaponID,
			"-",
			skin
		}), -1) != -1;
	}

	public static void SetFireStat(int weaponID, int skin)
	{
		ObscuredPrefs.SetInt(string.Concat(new object[]
		{
			"FireStatCounter",
			weaponID,
			"-",
			skin
		}), 0);
	}

	public static int GetFireStatCounter(int weaponID, int skin)
	{
		return ObscuredPrefs.GetInt(string.Concat(new object[]
		{
			"FireStatCounter",
			weaponID,
			"-",
			skin
		}), -1);
	}

	public static void SetFireStatCounter(int weaponID, int skin, int value)
	{
		ObscuredPrefs.SetInt(string.Concat(new object[]
		{
			"FireStatCounter",
			weaponID,
			"-",
			skin
		}), value);
	}

	public static void SetPlayerID(int id)
	{
		PlayerPrefs.SetInt("PlayerID", id);
	}

	public static int GetPlayerID()
	{
		return PlayerPrefs.GetInt("PlayerID", Random.Range(10000, 99999));
	}

	public static bool GetPlayerSkin(int skinID, BodyParts part)
	{
		switch (part)
		{
			case BodyParts.Head:
				return ObscuredPrefs.GetBool("PlayerSkin" + part + skinID, false);
			case BodyParts.Body:
				return ObscuredPrefs.GetBool("PlayerSkin" + part + skinID, false);
			case BodyParts.Legs:
				return ObscuredPrefs.GetBool("PlayerSkin" + part + skinID, false);
			default:
				return true;
		}
	}

	public static void SetPlayerSkin(int skinID, BodyParts part)
	{
		switch (part)
		{
			case BodyParts.Head:
				ObscuredPrefs.SetBool("PlayerSkin" + part + skinID, true);
				return;
			case BodyParts.Body:
				ObscuredPrefs.SetBool("PlayerSkin" + part + skinID, true);
				return;
			case BodyParts.Legs:
				ObscuredPrefs.SetBool("PlayerSkin" + part + skinID, true);
				return;
			default:
				return;
		}
	}

	public static int GetPlayerSkinSelected(BodyParts part)
	{
		switch (part)
		{
			case BodyParts.Head:
				return ObscuredPrefs.GetInt("SelectPlayerSkin" + part, 1);
			case BodyParts.Body:
				return ObscuredPrefs.GetInt("SelectPlayerSkin" + part, 1);
			case BodyParts.Legs:
				return ObscuredPrefs.GetInt("SelectPlayerSkin" + part, 1);
			default:
				return 0;
		}
	}

	public static void SetPlayerSkinSelected(int skinID, BodyParts part)
	{
		switch (part)
		{
			case BodyParts.Head:
				ObscuredPrefs.SetInt("SelectPlayerSkin" + part, skinID);
				return;
			case BodyParts.Body:
				ObscuredPrefs.SetInt("SelectPlayerSkin" + part, skinID);
				return;
			case BodyParts.Legs:
				ObscuredPrefs.SetInt("SelectPlayerSkin" + part, skinID);
				return;
			default:
				return;
		}
	}
}
