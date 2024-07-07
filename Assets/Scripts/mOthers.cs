using System;
using UnityEngine;

public class mOthers : MonoBehaviour
{
	public GameObject InAppPanel;

	private bool isExit;

	private void Start()
	{
		for (int i = 0; i < Camera.allCameras.Length; i++)
		{
			Camera.allCameras[i].eventMask = 0;
		}
		//Application.targetFrameRate = 60;
		Application.targetFrameRate = PlayerPrefs.GetInt("FpsMax", 60);
		InputManager.Init();
		if (PlayerPrefs.HasKey("LeaveRoomText"))
		{
			TimerManager.In(0.5f, delegate
			{
				UIToast.Show(PlayerPrefs.GetString("LeaveRoomText"), 3f);
				PlayerPrefs.DeleteKey("LeaveRoomText");
			});
		}
		BadWordsManager.Init();
		WeaponManager.Init();
		UISelectWeapon.AllWeapons = true;
		UISelectWeapon.SelectedUpdateWeaponManager = false;
		TimerManager.In(1f, delegate
		{
			isExit = true;
		});
		if (!PlayerPrefs.HasKey("InAppInfo"))
		{
			string message = Localization.Get("Block Strike is a free game, however it has paid goods that can be bought for real money. In-game purchases can be disabled in your device's settings.");
			AndroidNativeFunctions.ShowAlert(message, Localization.Get("In-game purchases"), "Ok", string.Empty, string.Empty, delegate
			{
				PlayerPrefs.SetInt("InAppInfo", 1);
			});
		}
		if (AccountManager.isConnect)
		{
			EventManager.AddListener("AccountConnected", CheckFireStats);
			EventManager.AddListener("AccountConnected", CheckSecretWeapons);
		}
		GC.Collect();
	}

	private void CheckFireStats()
	{
		for (int i = 0; i < GameSettings.instance.WeaponsStore.Count; i++)
		{
			for (int j = 0; j < GameSettings.instance.WeaponsStore[i].Skins.Count; j++)
			{
				WeaponSkinData weaponSkinData = GameSettings.instance.WeaponsStore[i].Skins[j];
				if (SaveLoadManager.GetFireStat(i + 1, weaponSkinData.ID) && (weaponSkinData.Quality == WeaponSkinQuality.Default || weaponSkinData.Quality == WeaponSkinQuality.Normal || weaponSkinData.Price != 0 || !SaveLoadManager.GetWeaponSkin(i + 1, weaponSkinData.ID)))
				{
					SaveLoadManager.SetFireStatCounter(i + 1, GameSettings.instance.WeaponsStore[i].Skins[j].ID, -1);
				}
			}
		}
	}

	private void CheckSecretWeapons()
	{
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			if (!GameSettings.instance.Weapons[i].Secret || GameSettings.instance.Weapons[i].Lock || SaveLoadManager.GetWeapon(GameSettings.instance.Weapons[i].ID))
			{
				continue;
			}
			int num = GameSettings.instance.Weapons[i].ID;
			for (int j = 0; j < GameSettings.instance.WeaponsStore[i].Skins.Count; j++)
			{
				if (j != 0 && SaveLoadManager.GetWeaponSkin(num, GameSettings.instance.WeaponsStore[i].Skins[j].ID))
				{
					SaveLoadManager.SetWeapon(num);
					break;
				}
			}
		}
	}

	private void Update()
	{
		if (!isExit && Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	public void ExitGame()
	{
		if (isExit)
		{
			mPopUp.ShowPopup(Localization.Get("ExitText"), Localization.Get("Exit"), Localization.Get("Yes"), delegate
			{
				Application.Quit();
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif
			}, Localization.Get("No"), delegate
			{
				mPopUp.HideAll("Menu");
			});
		}
	}

	public void ShowOthersGames()
	{
		Application.OpenURL("https://play.google.com/store/apps/dev?id=6363329851677974248");
	}

	public void ComingSoon()
	{
		UIToast.Show(Localization.Get("Coming soon"));
	}

	public void OpenStore()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
		}
		else
		{
			mPanelManager.ShowPanel("Store", true);
		}
	}

	public void AccountUpdate()
	{
		AccountManager.UpdateData(null);
	}

	public static void Stream(string url)
	{
		mPopUp.ShowPopup(Localization.Get("Stream Toast"), Localization.Get("Watch the video"), Localization.Get("No"), delegate
		{
			mPopUp.HideAll("Menu");
		}, Localization.Get("Yes"), delegate
		{
			mPopUp.HideAll("Menu");
			Application.OpenURL(url);
		});
	}
}
