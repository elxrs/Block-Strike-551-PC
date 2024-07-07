using System;
using UnityEngine;

public class UISettings : MonoBehaviour
{
	[Header("General")]
	private static int SelectLanguage;

	public UILabel RegionLabel;

	private CloudRegionCode RegionCode;

	public UILabel FontLabel;

	private int SelectFont;

	public UIToggle FPSMeter;

	public UIToggle Chat;

	public UIToggle Console;

	public UIToggle ShowDamage;

	public UIToggle BulletHole;

	public UIToggle Blood;

	public UIToggle HitMarker;

	public UIToggle HUD;

	public UIToggle ShowWeapon;

	public UISprite СolorСrosshair;

	public UIToggle Shell;

	public UIToggle ProjectileEffect;

	public UIToggle FilterChat;

	public UIToggle ShowAvatar;

	private int SelectСolorСrosshair;

	public UISlider RenderDistance;

	public UILabel RenderDistanceLabel;

	[Header("Control")]
	public UISlider Sensitivity;

	public UILabel SensitivityLabel;

	public UISlider ButtonAlpha;

	public UILabel ButtonAlphaLabel;

	public UIToggle Lefty;

	[Header("Sound")]
	public UISlider Volume;

	public UILabel VolumeLabel;

	public UIToggle Sound;

	public UIToggle AmbientSound;

	private void Start()
	{
		if (!nConsole.Contains("save_settings_in_device"))
		{
			nConsole.AddCommand("save_settings_in_device", Settings.SaveInDevice);
			nConsole.AddCommand("load_settings_in_device", Settings.LoadInDevice);
		}
		Load();
	}

	private void Load()
	{
		Settings.Load();
		UpdateLanguage();
		SelectFont = Settings.Font;
		FontLabel.text = Localization.Get("Font") + ": " + UIFontManager.GetFonts()[SelectFont].name.Split("-"[0])[1];
		FPSMeter.value = Settings.FPSMeter;
		Chat.value = Settings.Chat;
		Console.value = Settings.Console;
		UpdateConsole();
		ShowDamage.value = Settings.ShowDamage;
		BulletHole.value = Settings.BulletHole;
		Blood.value = Settings.Blood;
		HitMarker.value = Settings.HitMarker;
		HUD.value = Settings.HUD;
		ShowWeapon.value = Settings.ShowWeapon;
		Shell.value = Settings.Shell;
		ProjectileEffect.value = Settings.ProjectileEffect;
		FilterChat.value = Settings.FilterChat;
		ShowAvatar.value = Settings.ShowAvatar;
		UpdateColorCrosshair();
		UpdateRenderDistance();
		UpdateSensitivity();
		UpdateButtonAlpha();
		Lefty.value = Settings.Lefty;
		UpdateVolume();
		Sound.value = Settings.Sound;
		AmbientSound.value = Settings.AmbientSound;
	}

	public void Save()
	{
		Settings.Font = SelectFont;
		Settings.FPSMeter = FPSMeter.value;
		Settings.Chat = Chat.value;
		Settings.Console = Console.value;
		UpdateConsole();
		Settings.ShowDamage = ShowDamage.value;
		Settings.BulletHole = BulletHole.value;
		Settings.Blood = Blood.value;
		Settings.HitMarker = HitMarker.value;
		Settings.HUD = HUD.value;
		Settings.ShowWeapon = ShowWeapon.value;
		Settings.Shell = Shell.value;
		Settings.ProjectileEffect = ProjectileEffect.value;
		Settings.FilterChat = FilterChat.value;
		Settings.ShowAvatar = ShowAvatar.value;
		if (PhotonNetwork.connectedAndReady)
		{
			PhotonNetwork.player.SetAvatarUrl((!ShowAvatar.value) ? string.Empty : AccountManager.instance.Data.AvatarUrl);
		}
		LODObject.SetDistance(RenderDistance.value);
		Settings.Lefty = Lefty.value;
		Settings.Sound = Sound.value;
		Settings.AmbientSound = AmbientSound.value;
		Settings.Save();
		EventManager.Dispatch("UpdateSettings");
	}

	public void Default()
	{
		Settings.Font = 0;
		Settings.FPSMeter = false;
		Settings.Chat = true;
		Settings.Console = false;
		UpdateConsole();
		Settings.ShowDamage = false;
		Settings.BulletHole = true;
		Settings.Blood = true;
		Settings.HitMarker = true;
		Settings.HUD = true;
		Settings.ShowWeapon = true;
		Settings.FilterChat = true;
		Settings.Shell = true;
		Settings.ShowAvatar = true;
		Settings.ProjectileEffect = true;
		Settings.RenderDistance = 0.2f;
		Settings.Sensitivity = 0.2f;
		Settings.ButtonAlpha = 1f;
		Settings.Lefty = false;
		Settings.Volume = 0.8f;
		Settings.Sound = true;
		Settings.AmbientSound = true;
		Settings.Save();
		Load();
		EventManager.Dispatch("UpdateSettings");
	}

	public static void UpdateLanguage()
	{
		if (PlayerPrefs.HasKey("Language"))
		{
			SetLanguage(PlayerPrefs.GetString("Language"));
		}
		else if (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Ukrainian || Application.systemLanguage == SystemLanguage.Belarusian)
		{
			SetLanguage("Russia");
		}
		else if (Application.systemLanguage == SystemLanguage.English)
		{
			SetLanguage("English");
		}
		else if (Application.systemLanguage == SystemLanguage.Korean)
		{
			SetLanguage("Korean");
		}
		else if (Application.systemLanguage == SystemLanguage.Spanish)
		{
			SetLanguage("Spanish");
		}
		else if (Application.systemLanguage == SystemLanguage.Portuguese)
		{
			SetLanguage("Portuguese");
		}
		else if (Application.systemLanguage == SystemLanguage.French)
		{
			SetLanguage("French");
		}
		else if (Application.systemLanguage == SystemLanguage.Japanese)
		{
			SetLanguage("Japan");
		}
		else if (Application.systemLanguage == SystemLanguage.Polish)
		{
			SetLanguage("Polish");
		}
	}

	private static void SetLanguage(string language)
	{
		for (int i = 0; i < Localization.knownLanguages.Length; i++)
		{
			if (Localization.knownLanguages[i] == language)
			{
				Localization.language = Localization.knownLanguages[i];
				SelectLanguage = i;
				break;
			}
		}
	}

	public void NextLanguage()
	{
		SelectLanguage++;
		if (SelectLanguage > Localization.knownLanguages.Length - 1)
		{
			SelectLanguage = 0;
		}
		SetLanguage(Localization.knownLanguages[SelectLanguage]);
	}

	public void LastLanguage()
	{
		SelectLanguage--;
		if (SelectLanguage < 0)
		{
			SelectLanguage = Localization.knownLanguages.Length - 1;
		}
		SetLanguage(Localization.knownLanguages[SelectLanguage]);
	}

	public void NextFont()
	{
		SelectFont++;
		if (SelectFont > UIFontManager.GetFonts().Length - 1)
		{
			SelectFont = 0;
		}
		UIFontManager.SetFont(SelectFont);
		FontLabel.text = Localization.Get("Font") + ": " + UIFontManager.GetFonts()[SelectFont].name.Split("-"[0])[1];
	}

	public void LastFont()
	{
		SelectFont--;
		if (SelectFont < 0)
		{
			SelectFont = UIFontManager.GetFonts().Length - 1;
		}
		UIFontManager.SetFont(SelectFont);
		FontLabel.text = Localization.Get("Font") + ": " + UIFontManager.GetFonts()[SelectFont].name.Split("-"[0])[1];
	}

	private void UpdateConsole()
	{
		ConsoleManager.SetActive(Settings.Console);
	}

	private void UpdateColorCrosshair()
	{
		SelectСolorСrosshair = Settings.ColorCrosshair;
		Color color = Utils.GetColor(SelectСolorСrosshair);
		if (color == Color.clear)
		{
			color = new Color(1f, 1f, 1f, 0.5f);
		}
		СolorСrosshair.color = color;
	}

	public void SetСolorСrosshair()
	{
		SelectСolorСrosshair++;
		if (9 < SelectСolorСrosshair)
		{
			SelectСolorСrosshair = 0;
		}
		Color color = Utils.GetColor(SelectСolorСrosshair);
		if (color.a == 0f)
		{
			color = new Color(1f, 1f, 1f, 0.05f);
		}
		СolorСrosshair.color = color;
		Settings.ColorCrosshair = SelectСolorСrosshair;
	}

	private void UpdateRenderDistance()
	{
		RenderDistance.value = Settings.RenderDistance;
		RenderDistanceLabel.text = Localization.Get("RenderDistance") + ": " + Mathf.FloorToInt(10f + RenderDistance.value * 40f) + "m";
	}

	public void SetRenderDistance()
	{
		Settings.RenderDistance = RenderDistance.value;
		RenderDistanceLabel.text = Localization.Get("RenderDistance") + ": " + Mathf.FloorToInt(10f + RenderDistance.value * 40f) + "m";
	}

	private void UpdateSensitivity()
	{
		Sensitivity.value = Settings.Sensitivity;
		SensitivityLabel.text = Localization.Get("Sensitivity") + ": " + Mathf.RoundToInt(Sensitivity.value * 100f) + "%";
	}

	public void SetSensitivity()
	{
		Settings.Sensitivity = Sensitivity.value;
		SensitivityLabel.text = Localization.Get("Sensitivity") + ": " + Mathf.RoundToInt(Sensitivity.value * 100f) + "%";
	}

	private void UpdateButtonAlpha()
	{
		ButtonAlpha.value = Settings.ButtonAlpha;
		ButtonAlphaLabel.text = Localization.Get("Button Alpha") + ": " + Mathf.RoundToInt(ButtonAlpha.value * 100f) + "%";
	}

	public void SetButtonAlpha()
	{
		Settings.ButtonAlpha = Mathf.Clamp(ButtonAlpha.value, 0.01f, 1f);
		ButtonAlphaLabel.text = Localization.Get("Button Alpha") + ": " + Mathf.RoundToInt(ButtonAlpha.value * 100f) + "%";
	}

	private void UpdateVolume()
	{
		Volume.value = Settings.Volume;
		VolumeLabel.text = Localization.Get("Volume") + ": " + Mathf.RoundToInt(Volume.value * 100f) + "%";
	}

	public void SetVolume()
	{
		Settings.Volume = Volume.value;
		VolumeLabel.text = Localization.Get("Volume") + ": " + Mathf.RoundToInt(Volume.value * 100f) + "%";
	}

	public void DefaultButtons()
	{
		EventManager.Dispatch("DefaultButton");
	}

	public void SaveButtons()
	{
		EventManager.Dispatch("SaveButton");
	}
}
