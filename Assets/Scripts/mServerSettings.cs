using UnityEngine;

public class mServerSettings : MonoBehaviour
{
	public GameObject SettingsButton;

	[Header("Only")]
	public GameObject OnlyPanel;

	public UIPopupList OnlyWeaponPopupList;

	private static mServerSettings instance;

	private void Start()
	{
		instance = this;
	}

	public static void Check(GameMode mode, string map)
	{
		switch (mode)
		{
		case GameMode.Only:
			instance.SettingsButton.SetActive(true);
			instance.gameObject.SendMessage("SetDefaultMaxPlayers");
			break;
		case GameMode.MiniGames:
			if (map == "50Traps")
			{
				instance.gameObject.SendMessage("SetMaxPlayers", new int[5] { 4, 8, 16, 24, 32 });
			}
			else
			{
				instance.gameObject.SendMessage("SetDefaultMaxPlayers");
			}
			break;
		default:
			instance.gameObject.SendMessage("SetDefaultMaxPlayers");
			instance.SettingsButton.SetActive(false);
			break;
		}
	}

	public void Open()
	{
		GameMode gameMode = mCreateServer.GetGameMode();
		if (gameMode == GameMode.Only)
		{
			StartOnlyMode();
		}
	}

	public void Close()
	{
		GameMode gameMode = mCreateServer.GetGameMode();
		if (gameMode == GameMode.Only)
		{
			OnlyPanel.gameObject.SetActive(false);
		}
	}

	private void StartOnlyMode()
	{
		OnlyPanel.gameObject.SetActive(true);
		OnlyWeaponPopupList.Clear();
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			if (!GameSettings.instance.Weapons[i].Lock && !GameSettings.instance.Weapons[i].Secret)
			{
				OnlyWeaponPopupList.AddItem(GameSettings.instance.Weapons[i].Name);
			}
		}
		OnlyWeaponPopupList.value = OnlyWeaponPopupList.items[0];
	}

	public static int GetOnlyWeapon()
	{
		int num = WeaponManager.GetWeaponID(instance.OnlyWeaponPopupList.value);
		if (num <= 0)
		{
			num = 1;
		}
		return num;
	}
}
