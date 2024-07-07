using System.Collections.Generic;
using UnityEngine;

public class PlayerRoundManager
{
	public class cFireStat
	{
		public CryptoInt weapon;

		public List<CryptoInt> skins = new List<CryptoInt>();

		public List<CryptoInt> counts = new List<CryptoInt>();
	}

	private static GameMode Mode;

	private static CryptoInt2 XP = 0;

	private static CryptoInt2 Money = 0;

	private static CryptoInt2 Kills = 0;

	private static CryptoInt2 Headshot = 0;

	private static CryptoInt2 Deaths = 0;

	private static List<cFireStat> FireStat = new List<cFireStat>();

	private static float StartTime;

	private static float FinishTime;

	public static GameMode GetMode()
	{
		return Mode;
	}

	public static int GetXP()
	{
		return XP;
	}

	public static int GetMoney()
	{
		return Money;
	}

	public static int GetKills()
	{
		return Kills;
	}

	public static int GetHeadshot()
	{
		return Headshot;
	}

	public static int GetDeaths()
	{
		return Deaths;
	}

	public static float GetTime()
	{
		return FinishTime - StartTime;
	}

	public static void SetMode(GameMode mode)
	{
		Mode = mode;
		if (StartTime == 0f)
		{
			StartTime = Time.time;
		}
	}

	public static void SetXP(int xp)
	{
		if (!LevelManager.CustomMap)
		{
			XP += xp;
		}
	}

	public static void SetMoney(int money)
	{
		if (!LevelManager.CustomMap)
		{
			Money += money;
		}
	}

	public static void SetKills1()
	{
		if (!LevelManager.CustomMap)
		{
			Kills += nValue.int1;
		}
	}

	public static void SetHeadshot1()
	{
		if (!LevelManager.CustomMap)
		{
			Headshot += nValue.int1;
		}
	}

	public static void SetDeaths1()
	{
		if (!LevelManager.CustomMap)
		{
			Deaths += nValue.int1;
		}
	}

	public static void SetFireStat1(int weapon, int skin)
	{
		if (LevelManager.CustomMap)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < FireStat.Count; i++)
		{
			if (FireStat[i].weapon != weapon)
			{
				continue;
			}
			if (FireStat[i].skins.Contains(skin))
			{
				for (int j = 0; j < FireStat[i].skins.Count; j++)
				{
					if (FireStat[i].skins[j] == skin)
					{
						List<CryptoInt> counts;
						List<CryptoInt> list = (counts = FireStat[i].counts);
						int index;
						int index2 = (index = j);
						CryptoInt cryptoInt = counts[index];
						CryptoInt cryptoInt2 = cryptoInt;
						list[index2] = ++cryptoInt2;
					}
				}
			}
			else
			{
				FireStat[i].skins.Add(skin);
				FireStat[i].counts.Add(nValue.int1);
			}
			flag = true;
			break;
		}
		if (!flag)
		{
			cFireStat cFireStat = new cFireStat();
			cFireStat.weapon = weapon;
			cFireStat.skins.Add(skin);
			cFireStat.counts.Add(nValue.int1);
			FireStat.Add(cFireStat);
		}
	}

	public static bool HasValue()
	{
		if (GetXP() != 0)
		{
			return true;
		}
		if (GetMoney() != 0)
		{
			return true;
		}
		if (GetKills() != 0)
		{
			return true;
		}
		if (GetHeadshot() != 0)
		{
			return true;
		}
		if (GetDeaths() != 0)
		{
			return true;
		}
		return false;
	}

	public static void Show()
	{
		if (PhotonNetwork.offlineMode)
		{
			return;
		}
		FinishTime = Time.time;
		TimerManager.In(0.5f, false, delegate
		{
			if (AccountManager.GetInAppPurchase().Count > 0)
			{
				ShowPopup();
			}
			else
			{
				TimerManager.In(0.5f, ShowPopup);
			}
		});
	}

	private static void ShowPopup()
	{
		if (HasValue())
		{
			mPlayerRoundManager.Show();
			SetData();
		}
	}

	private static void SetData()
	{
		SaveLoadManager.SetMoney1(GetMoney());
		SaveLoadManager.SetPlayerXP1(GetXP());
		SaveLoadManager.SetKills(SaveLoadManager.GetKills() + GetKills());
		SaveLoadManager.SetDeaths(SaveLoadManager.GetDeaths() + GetDeaths());
		SaveLoadManager.SetHeadshot(SaveLoadManager.GetHeadshot() + GetHeadshot());
	}

	private static void Failed(string error)
	{
		mPopUp.ShowPopup(Localization.Get("Error") + ": " + error, Localization.Get("Error"), Localization.Get("Retry"), delegate
		{
			mPopUp.HideAll();
			SetData();
		});
	}

	public static void Clear()
	{
		XP = nValue.int0;
		Money = nValue.int0;
		Kills = nValue.int0;
		Headshot = nValue.int0;
		Deaths = nValue.int0;
		StartTime = nValue.int0;
		FinishTime = nValue.int0;
		FireStat.Clear();
	}
}
