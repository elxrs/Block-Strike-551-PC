using System;
using UnityEngine;

public class mPlayerStats : MonoBehaviour
{
	public UILabel NameLabel;

	public UILabel PlayerIDLabel;

	public UILabel LevelLabel;

	public UILabel XPLabel;

	public UILabel DeathsLabel;

	public UILabel KillsLabel;

	public UILabel HeadshotKillsLabel;

	public UILabel OpenCaseLabel;

	public UILabel TotalSkinLabel;

	public UILabel LegendarySkinLabel;

	public UILabel ProfessionalSkinLabel;

	public UILabel BasicSkinLabel;

	public UILabel NormalSkinLabel;

	public UILabel RateLabel;

	public GameObject Panel;

	public void UpdateData()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		Panel.SetActive(true);
		NameLabel.text = SaveLoadManager.GetPlayerName();
		PlayerIDLabel.text = "ID: " + SaveLoadManager.GetPlayerID().ToString();
		LevelLabel.text = SaveLoadManager.GetPlayerLevel().ToString();
		XPLabel.text = SaveLoadManager.GetPlayerXP() + "/" + SaveLoadManager.GetMaxXP();
		DeathsLabel.text = SaveLoadManager.GetDeaths().ToString();
		KillsLabel.text = SaveLoadManager.GetKills().ToString();
		HeadshotKillsLabel.text = SaveLoadManager.GetHeadshot().ToString();
		OpenCaseLabel.text = ConvertTime(AccountManager.instance.Data.Time);
		TotalSkinLabel.text = GetOpenSkins(WeaponSkinQuality.Default) + "/" + GetTotalSkins(WeaponSkinQuality.Default);
		LegendarySkinLabel.text = GetOpenSkins(WeaponSkinQuality.Legendary) + "/" + GetTotalSkins(WeaponSkinQuality.Legendary);
		ProfessionalSkinLabel.text = GetOpenSkins(WeaponSkinQuality.Professional) + "/" + GetTotalSkins(WeaponSkinQuality.Professional);
		BasicSkinLabel.text = GetOpenSkins(WeaponSkinQuality.Basic) + "/" + GetTotalSkins(WeaponSkinQuality.Basic);
		NormalSkinLabel.text = GetOpenSkins(WeaponSkinQuality.Normal) + "/" + GetTotalSkins(WeaponSkinQuality.Normal);
		RateLabel.text = GetRate();
	}

	private int GetOpenSkins(WeaponSkinQuality quality)
	{
		int num = 0;
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			for (int j = 1; j < GameSettings.instance.WeaponsStore[i].Skins.Count; j++)
			{
				if ((GameSettings.instance.WeaponsStore[i].Skins[j].Quality == quality || quality == WeaponSkinQuality.Default) && SaveLoadManager.GetWeaponSkin(i + 1, j))
				{
					num++;
				}
			}
		}
		return num;
	}

	private int GetTotalSkins(WeaponSkinQuality quality)
	{
		int num = 0;
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			for (int j = 1; j < GameSettings.instance.WeaponsStore[i].Skins.Count; j++)
			{
				if (GameSettings.instance.WeaponsStore[i].Skins[j].Quality == quality || quality == WeaponSkinQuality.Default)
				{
					num++;
				}
			}
		}
		return num;
	}

	private string GetRate()
	{
		float value = SaveLoadManager.GetKills() * 100f / SaveLoadManager.GetDeaths();
		float value2 = SaveLoadManager.GetHeadshot() * 100f / SaveLoadManager.GetKills();
		value = Mathf.Clamp(value, 0f, 100f);
		value2 = Mathf.Clamp(value2, 0f, 100f);
		float num = (value + value2) / 2f;
		if (num >= 85f)
		{
			return "A";
		}
		if (num >= 70f)
		{
			return "B";
		}
		if (num >= 50f)
		{
			return "C";
		}
		return "D";
	}

	private string ConvertTime(long time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		return string.Format("{0:0}:{1:00}:{2:00}", timeSpan.Days * 24 + timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	}
}
