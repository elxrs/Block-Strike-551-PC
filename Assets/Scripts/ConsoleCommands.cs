using System.Text;
using UnityEngine;

public class ConsoleCommands : MonoBehaviour
{
	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		nConsole.AddCommand("quit", OnQuit);
		nConsole.AddCommand("leave_room", OnLeaveRoom);
		nConsole.AddCommand("game_info", OnGameInfo);
		nConsole.AddCommand("texture_quality", nValueType.Int, OnTextureQuality);
		nConsole.AddCommand("fps_max", nValueType.Int, FpsMax);
	}

	private void FpsMax(string command, object value)
    {
		PlayerPrefs.SetInt("FpsMax", Mathf.Clamp((int)value, 30, 360));
		Application.targetFrameRate = Mathf.Clamp((int)value, 30, 360);
		print("Fps Max: " + (int)value + " [30-360]");
	}

	private void OnQuit(string command, object value)
	{
		if (PhotonNetwork.inRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
		Application.Quit();
	}

	private void OnLeaveRoom(string command, object value)
	{
		if (PhotonNetwork.inRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
	}

	private void OnShowToast(string command, object value)
	{
		UIToast.Show((string)value);
	}

	private void OnGameInfo(string command, object value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(VersionManager.bundleVersion);
		stringBuilder.AppendLine(VersionManager.bundleVersionCode.ToString());
		print(stringBuilder.ToString());
	}

	private void OnTextureQuality(string command, object value)
	{
		QualitySettings.masterTextureLimit = Mathf.Clamp((int)value, 0, 3);
		print("Texture Quality: " + (int)value + " [0-3]");
	}

	private void OnPriceAccount(string command, object value)
	{
		int num = 0;
		for (int i = 0; i < GameSettings.instance.WeaponsStore.Count; i++)
		{
			WeaponData weaponData = GameSettings.instance.Weapons[i];
			for (int j = 0; j < GameSettings.instance.WeaponsStore[i].Skins.Count; j++)
			{
				WeaponSkinData weaponSkinData = GameSettings.instance.WeaponsStore[i].Skins[j];
				if (SaveLoadManager.GetWeaponSkin(i + 1, weaponSkinData.ID))
				{
					switch (weaponSkinData.Quality)
					{
					case WeaponSkinQuality.Normal:
						num++;
						break;
					case WeaponSkinQuality.Basic:
						num += 2;
						break;
					case WeaponSkinQuality.Professional:
						num += 3;
						break;
					case WeaponSkinQuality.Legendary:
						num += 4;
						break;
					}
				}
				if (SaveLoadManager.GetFireStat(i + 1, weaponSkinData.ID))
				{
					num += SaveLoadManager.GetFireStatCounter(i + 1, weaponSkinData.ID) / 100;
				}
			}
			if (weaponData.Secret && SaveLoadManager.GetWeapon(weaponData.ID))
			{
				num += 175;
			}
		}
		print("Price Account: " + num + " BS Gold");
	}
}
