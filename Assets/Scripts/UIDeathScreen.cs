using System.Collections.Generic;
using UnityEngine;

public class UIDeathScreen : MonoBehaviour
{
	public UIPanel Panel;

	public UISprite BackgroundSprite;

	public UILabel PlayerLabel;

	public UILabel WeaponLabel;

	public UILabel DamageLabel;

	public UILabel HeadshotLabel;

	public UISprite WeaponSprite;

	public UILabel ScoreLabel;

	public UITexture AvatarTexture;

	public float sizeWeapon = 1f;

	private Dictionary<int, int> kills = new Dictionary<int, int>();

	private Dictionary<int, int> deaths = new Dictionary<int, int>();

	private int Timer;

	private static UIDeathScreen instance;

	private void Start()
	{
		instance = this;
	}

	public static void Show(DamageInfo damageInfo)
	{
		if (!damageInfo.isPlayerID || damageInfo.WeaponID == 46)
		{
			return;
		}
		AddDeath(damageInfo.PlayerID);
		TimerManager.Cancel(instance.Timer);
		instance.Panel.cachedGameObject.SetActive(true);
		instance.Panel.alpha = 0f;
		TweenAlpha.Begin(instance.Panel.cachedGameObject, 0.2f, 1f);
		PhotonPlayer photonPlayer = PhotonPlayer.Find(damageInfo.PlayerID);
		instance.PlayerLabel.text = photonPlayer.NickName;
		instance.HeadshotLabel.text = ((!damageInfo.HeadShot) ? string.Empty : Localization.Get("Headshot"));
		instance.SetWeaponData(damageInfo);
		int num = 0;
		int num2 = 0;
		if (instance.kills.ContainsKey(damageInfo.PlayerID))
		{
			num = instance.kills[damageInfo.PlayerID];
		}
		if (instance.deaths.ContainsKey(damageInfo.PlayerID))
		{
			num2 = instance.deaths[damageInfo.PlayerID];
		}
		instance.DamageLabel.text = Localization.Get("Damage") + ": " + damageInfo.Damage + "  [" + num + " - " + num2 + "]";
		instance.AvatarTexture.mainTexture = AvatarManager.Get(photonPlayer.GetAvatarUrl());
		instance.Timer = TimerManager.In(3f, delegate
		{
			TweenAlpha.Begin(instance.Panel.cachedGameObject, 0.2f, 0f);
			TimerManager.In(0.2f, delegate
			{
				instance.Panel.cachedGameObject.SetActive(false);
			});
		});
	}

	public static void AddKill(int playerID)
	{
		if (instance.kills.ContainsKey(playerID))
		{
			Dictionary<int, int> dictionary;
			Dictionary<int, int> dictionary2 = (dictionary = instance.kills);
			int key;
			int key2 = (key = playerID);
			key = dictionary[key];
			dictionary2[key2] = key + 1;
		}
		else
		{
			instance.kills.Add(playerID, 1);
		}
	}

	private static void AddDeath(int playerID)
	{
		if (instance.deaths.ContainsKey(playerID))
		{
			Dictionary<int, int> dictionary;
			Dictionary<int, int> dictionary2 = (dictionary = instance.deaths);
			int key;
			int key2 = (key = playerID);
			key = dictionary[key];
			dictionary2[key2] = key + 1;
		}
		else
		{
			instance.deaths.Add(playerID, 1);
		}
	}

	private void SetWeaponData(DamageInfo damageInfo)
	{
		WeaponData weaponData = WeaponManager.GetWeaponData(damageInfo.WeaponID);
		WeaponSkinData weaponSkin = WeaponManager.GetWeaponSkin(damageInfo.WeaponID, damageInfo.WeaponSkinID);
		WeaponLabel.text = string.Concat(weaponData.Name, " | ", weaponSkin.Name);
		WeaponLabel.color = GetWeaponSkinQualityColor(weaponSkin.Quality);
		WeaponSprite.spriteName = damageInfo.WeaponID + "-" + damageInfo.WeaponSkinID;
		WeaponSprite.width = (int)(GameSettings.instance.WeaponsCaseSize[weaponData.ID - 1].x * sizeWeapon);
		WeaponSprite.height = (int)(GameSettings.instance.WeaponsCaseSize[weaponData.ID - 1].y * sizeWeapon);
	}

	private Color GetWeaponSkinQualityColor(WeaponSkinQuality quality)
	{
		switch (quality)
		{
		case WeaponSkinQuality.Default:
		case WeaponSkinQuality.Normal:
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		case WeaponSkinQuality.Basic:
			return new Color32(54, 189, byte.MaxValue, byte.MaxValue);
		case WeaponSkinQuality.Professional:
			return new Color32(byte.MaxValue, 0, 0, byte.MaxValue);
		case WeaponSkinQuality.Legendary:
			return new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue);
		default:
			return Color.white;
		}
	}
}
