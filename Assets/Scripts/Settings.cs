using System.IO;
using FreeJSON;
using UnityEngine;

public class Settings
{
	public static int Font;

	public static bool FPSMeter;

	public static bool Console;

	public static bool Chat = true;

	public static bool ShowDamage;

	public static bool BulletHole = true;

	public static bool Blood = true;

	public static bool HitMarker = true;

	public static int ColorCrosshair;

	public static float RenderDistance = 0.2f;

	public static float Sensitivity = 0.2f;

	public static float Volume = 0.8f;

	public static bool Sound = true;

	public static bool AmbientSound = true;

	public static bool Lefty;

	public static float ButtonAlpha = 1f;

	public static bool HUD = true;

	public static bool ShowWeapon = true;

	public static bool Shell = true;

	public static bool ProjectileEffect = true;

	public static bool FilterChat = true;

	public static bool ShowAvatar;

	public static void Load()
	{
		Font = GetInt("Font", 0);
		FPSMeter = GetBool("FPSMeter", false);
		Console = GetBool("Console", false);
		Chat = GetBool("Chat", true);
		ShowDamage = GetBool("ShowDamage", false);
		BulletHole = GetBool("BulletHole", true);
		Blood = GetBool("Blood", true);
		HitMarker = GetBool("HitMarker", true);
		ColorCrosshair = GetInt("ColorCrosshair", 0);
		RenderDistance = GetFloat("RenderDistance", 0.2f);
		Sensitivity = GetFloat("Sensitivity", 0.2f);
		Volume = GetFloat("Volume", 0.8f);
		Sound = GetBool("Sound", true);
		AmbientSound = GetBool("AmbientSound", true);
		Lefty = GetBool("Lefty", false);
		ButtonAlpha = Mathf.Clamp(GetFloat("ButtonAlpha", 1f), 0.01f, 1f);
		HUD = GetBool("HUD", true);
		ShowWeapon = GetBool("ShowWeapon", true);
		Shell = GetBool("Shell", true);
		ProjectileEffect = GetBool("ProjectileEffect", true);
		FilterChat = GetBool("FilterChat", true);
		ShowAvatar = GetBool("ShowAvatar", true);
		AudioListener.volume = Volume;
	}

	public static void Save()
	{
		SetInt("Font", Font);
		SetBool("FPSMeter", FPSMeter);
		SetBool("Console", Console);
		SetBool("Chat", Chat);
		SetBool("ShowDamage", ShowDamage);
		SetBool("BulletHole", BulletHole);
		SetBool("Blood", Blood);
		SetBool("HitMarker", HitMarker);
		SetInt("ColorCrosshair", ColorCrosshair);
		SetFloat("RenderDistance", RenderDistance);
		SetFloat("Sensitivity", Sensitivity);
		SetFloat("Volume", Volume);
		SetBool("Sound", Sound);
		SetBool("AmbientSound", AmbientSound);
		SetBool("Lefty", Lefty);
		SetFloat("ButtonAlpha", Mathf.Clamp(ButtonAlpha, 0.01f, 1f));
		SetBool("HUD", HUD);
		SetBool("ShowWeapon", ShowWeapon);
		SetBool("Shell", Shell);
		SetBool("ProjectileEffect", ProjectileEffect);
		SetBool("FilterChat", FilterChat);
		SetBool("ShowAvatar", ShowAvatar);
		AudioListener.volume = Volume;
	}

	public static void SaveInDevice(string command, object obj)
	{
		if (!(command != "save_settings_in_device"))
		{
			JsonObject jsonObject = new JsonObject();
			jsonObject.Add("Font", GetInt("Font", 0));
			jsonObject.Add("FPSMeter", GetBool("FPSMeter", false));
			jsonObject.Add("Console", GetBool("Console", false));
			jsonObject.Add("Chat", GetBool("Chat", true));
			jsonObject.Add("ShowDamage", GetBool("ShowDamage", false));
			jsonObject.Add("BulletHole", GetBool("BulletHole", true));
			jsonObject.Add("Blood", GetBool("Blood", true));
			jsonObject.Add("HitMarker", GetBool("HitMarker", true));
			jsonObject.Add("ColorCrosshair", GetInt("ColorCrosshair", 0));
			jsonObject.Add("RenderDistance", GetFloat("RenderDistance", 0.2f));
			jsonObject.Add("Sensitivity", GetFloat("Sensitivity", 0.2f));
			jsonObject.Add("Volume", GetFloat("Volume", 0.8f));
			jsonObject.Add("Sound", GetBool("Sound", true));
			jsonObject.Add("AmbientSound", GetBool("AmbientSound", true));
			jsonObject.Add("Lefty", GetBool("Lefty", false));
			jsonObject.Add("ButtonAlpha", Mathf.Clamp(GetFloat("ButtonAlpha", 1f), 0.01f, 1f));
			jsonObject.Add("HUD", GetBool("HUD", true));
			jsonObject.Add("ShowWeapon", GetBool("ShowWeapon", true));
			jsonObject.Add("Shell", GetBool("Shell", true));
			jsonObject.Add("ProjectileEffect", GetBool("ProjectileEffect", true));
			jsonObject.Add("FilterChat", GetBool("FilterChat", true));
			string path = AndroidNativeFunctions.GetAbsolutePath() + "/Block Strike/Settings.json";
			File.WriteAllText(path, jsonObject.ToString());
		}
	}

	public static void LoadInDevice(string command, object obj)
	{
		if (command != "load_settings_in_device")
		{
			return;
		}
		string text = AndroidNativeFunctions.GetAbsolutePath() + "/Block Strike/Settings.json";
		if (File.Exists(text))
		{
			string jsonString = File.ReadAllText(text);
			if (JsonObject.isJson(text))
			{
				JsonObject jsonObject = JsonObject.Parse(jsonString);
				SetInt("Font", jsonObject.Get<int>("Font"));
				SetBool("FPSMeter", jsonObject.Get<bool>("FPSMeter"));
				SetBool("Console", jsonObject.Get<bool>("Console"));
				SetBool("Chat", jsonObject.Get<bool>("Chat"));
				SetBool("ShowDamage", jsonObject.Get<bool>("ShowDamage"));
				SetBool("BulletHole", jsonObject.Get<bool>("BulletHole"));
				SetBool("Blood", jsonObject.Get<bool>("Blood"));
				SetBool("HitMarker", jsonObject.Get<bool>("HitMarker"));
				SetInt("ColorCrosshair", jsonObject.Get<int>("ColorCrosshair"));
				SetFloat("RenderDistance", jsonObject.Get<float>("RenderDistance"));
				SetFloat("Sensitivity", jsonObject.Get<float>("Sensitivity"));
				SetFloat("Volume", jsonObject.Get<float>("Volume"));
				SetBool("Sound", jsonObject.Get<bool>("Sound"));
				SetBool("AmbientSound", jsonObject.Get<bool>("AmbientSound"));
				SetBool("Lefty", jsonObject.Get<bool>("Lefty"));
				SetFloat("ButtonAlpha", Mathf.Clamp(jsonObject.Get<float>("ButtonAlpha"), 0.01f, 1f));
				SetBool("HUD", jsonObject.Get<bool>("HUD"));
				SetBool("ShowWeapon", jsonObject.Get<bool>("ShowWeapon"));
				SetBool("Shell", jsonObject.Get<bool>("Shell"));
				SetBool("ProjectileEffect", jsonObject.Get<bool>("ProjectileEffect"));
				SetBool("FilterChat", jsonObject.Get<bool>("FilterChat"));
			}
		}
	}

	private static bool GetBool(string key, bool defaultValue)
	{
		if (PlayerPrefs.HasKey(key))
		{
			return PlayerPrefs.GetInt(key) == 1;
		}
		return defaultValue;
	}

	private static int GetInt(string key, int defaultValue)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	private static float GetFloat(string key, float defaultValue)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	private static void SetBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
	}

	private static void SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	private static void SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}
}
