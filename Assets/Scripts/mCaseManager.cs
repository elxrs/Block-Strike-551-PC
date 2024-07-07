using System;
using System.Collections.Generic;
using System.Text;
using FreeJSON;
using Photon;
using UnityEngine;

public class mCaseManager : PunBehaviour
{
	[Serializable]
	public class Case
	{
		public string Name;

		public GameCurrency Currency;

		public CryptoInt Price;

		public UILabel NameLabel;

		public UILabel InfoLabel;

		public CryptoBool Money;

		public CryptoInt Normal;

		public CryptoInt Base;

		public CryptoInt Professional;

		public CryptoInt Legendary;

		public CryptoInt FireStat;

		public CryptoInt SecretWeapon;
	}

	[Header("Cases")]
	public Case[] SkinCases;

	public Case[] StickerCases;

	private bool isSkinCases = true;

	[Header("Case Wheel")]
	public UIPanel CaseWheelPanel;

	public bool CaseRotate;

	private string SelectCaseName;

	private float StartRotate;

	public AnimationCurve Curve;

	public float Duration = 8f;

	public float Lerp = 1f;

	public Vector2 FinishInterval;

	private float FinishPosition;

	public AudioSource SoundSource;

	public float SoundInterval;

	private float LastSoundInterval;

	public mCaseItem[] CaseItems;

	public mCaseItem FinishItem;

	public Transform CaseItemsRoot;

	[Header("Finish Panel")]
	public UIPanel FinishPanel;

	public UISprite FinishBackground;

	public UITexture FinishEffect1;

	public UITexture FinishEffect2;

	public UILabel FinishLabel;

	public UILabel FinishQualityLabel;

	public UISprite FinishWeaponTexture;

	public UITexture FinishGoldTexture;

	public GameObject FinishAlreadyAvailable;

	public UITexture FinishAlreadyAvailableTexture;

	public UILabel FinishAlreadyAvailableLabel;

	public UITexture FinishFireStatEffect;

	public UITexture FinishSecretWeaponEffect;

	private bool isFinishWeapon;

	private WeaponData FinishWeapon;

	private WeaponSkinData FinishWeaponSkin;

	private StickerData FinishSticker;

	private bool FinishWeaponFireStat;

	private bool FinishWeaponAlready;

	private List<KeyValuePair<int, int>> NormalSkins = new List<KeyValuePair<int, int>>();

	private List<KeyValuePair<int, int>> BaseSkins = new List<KeyValuePair<int, int>>();

	private List<KeyValuePair<int, int>> ProfessionalSkins = new List<KeyValuePair<int, int>>();

	private List<KeyValuePair<int, int>> LegendarySkins = new List<KeyValuePair<int, int>>();

	private List<KeyValuePair<int, int>> SecretWeaponSkins = new List<KeyValuePair<int, int>>();

	private List<int> BaseStickers = new List<int>();

	private List<int> ProfessionalStickers = new List<int>();

	private List<int> LegendaryStickers = new List<int>();

	private TweenAlpha FinishTween;

	private TweenAlpha CaseWheelTween;

	[Header("Others")]
	public GameObject InAppPanel;

	public GameObject ShareButton;

	public GameObject BackButton;

	private void Start()
	{
		PhotonClassesManager.Add(this);
		UIEventListener uIEventListener = UIEventListener.Get(FinishBackground.cachedGameObject);
		uIEventListener.onDrag = RotateWeapon;
	}

	private void RotateWeapon(GameObject go, Vector2 drag)
	{
		mWeaponCamera.Rotate(drag, false);
	}

	public override void OnDisconnectedFromPhoton()
	{
		Close();
	}

	private void Update()
	{
		UpdateCaseWheel();
	}

	public void Show(bool skin)
	{
		isSkinCases = skin;
		if (isSkinCases)
		{
			for (int i = 0; i < SkinCases.Length; i++)
			{
				SkinCases[i].NameLabel.text = Localization.Get(SkinCases[i].Name);
				SkinCases[i].InfoLabel.text = GetCaseInfo(SkinCases[i]);
			}
			UpdateSkinsList();
		}
		else
		{
			for (int j = 0; j < StickerCases.Length; j++)
			{
				StickerCases[j].NameLabel.text = Localization.Get(StickerCases[j].Name);
				StickerCases[j].InfoLabel.text = GetCaseInfo(StickerCases[j]);
			}
			UpdateStickerList();
		}
	}

	public void Close()
	{
		CaseRotate = false;
		FinishPanel.alpha = 0f;
		if (FinishTween != null && FinishTween.enabled)
		{
			FinishTween.enabled = false;
		}
		if (CaseWheelTween != null && CaseWheelTween.enabled)
		{
			CaseWheelTween.enabled = false;
		}
		mWeaponCamera.Close();
		if (isSkinCases)
		{
			mPanelManager.ShowPanel("SkinCases", true);
		}
		else
		{
			mPanelManager.ShowPanel("StickerCases", true);
		}
	}

	private string GetCaseInfo(Case selectCase)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Localization.Get("ChanceOfDrop") + ":");
		stringBuilder.AppendLine(string.Empty);
		if ((bool)selectCase.Money)
		{
			stringBuilder.AppendLine("BS Coins -  50%");
		}
		if (selectCase.Normal != 0)
		{
			stringBuilder.AppendLine(string.Concat(Localization.Get("Normal quality"), " - ", selectCase.Normal, "%"));
		}
		if (selectCase.Base != 0)
		{
			stringBuilder.AppendLine(string.Concat("[2098ff]", Localization.Get("Basic quality"), "[-] - ", selectCase.Base, "%"));
		}
		if (selectCase.Professional != 0)
		{
			stringBuilder.AppendLine(string.Concat("[ff3838]", Localization.Get("Professional quality"), "[-] - ", selectCase.Professional, "%"));
		}
		if (selectCase.Legendary != 0)
		{
			stringBuilder.AppendLine(string.Concat("[ff2093]", Localization.Get("Legendary quality"), "[-] - ", selectCase.Legendary, "%"));
		}
		if (selectCase.SecretWeapon != 0)
		{
			stringBuilder.AppendLine(string.Concat("[757575]", Localization.Get("Secret Weapon"), "[-] - ", selectCase.SecretWeapon, "%"));
		}
		return stringBuilder.ToString();
	}

	private Case GetCase(string caseName)
	{
		if (isSkinCases)
		{
			for (int i = 0; i < SkinCases.Length; i++)
			{
				if (SkinCases[i].Name == caseName)
				{
					return SkinCases[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < StickerCases.Length; j++)
			{
				if (StickerCases[j].Name == caseName)
				{
					return StickerCases[j];
				}
			}
		}
		return null;
	}

	private int GetCaseIndex(string caseName)
	{
		if (isSkinCases)
		{
			for (int i = 0; i < SkinCases.Length; i++)
			{
				if (SkinCases[i].Name == caseName)
				{
					return i;
				}
			}
		}
		else
		{
			for (int j = 0; j < StickerCases.Length; j++)
			{
				if (StickerCases[j].Name == caseName)
				{
					return j;
				}
			}
		}
		return -1;
	}

	private void UpdateSkinsList()
	{
		if (NormalSkins.Count != 0)
		{
			return;
		}
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			if (GameSettings.instance.Weapons[i].Lock || GameSettings.instance.Weapons[i].Secret)
			{
				continue;
			}
			for (int j = 0; j < GameSettings.instance.WeaponsStore[i].Skins.Count; j++)
			{
				if (GameSettings.instance.WeaponsStore[i].Skins[j].Price == 0)
				{
					switch (GameSettings.instance.WeaponsStore[i].Skins[j].Quality)
					{
					case WeaponSkinQuality.Normal:
						NormalSkins.Add(new KeyValuePair<int, int>(GameSettings.instance.Weapons[i].ID, GameSettings.instance.WeaponsStore[i].Skins[j].ID));
						break;
					case WeaponSkinQuality.Basic:
						BaseSkins.Add(new KeyValuePair<int, int>(GameSettings.instance.Weapons[i].ID, GameSettings.instance.WeaponsStore[i].Skins[j].ID));
						break;
					case WeaponSkinQuality.Professional:
						ProfessionalSkins.Add(new KeyValuePair<int, int>(GameSettings.instance.Weapons[i].ID, GameSettings.instance.WeaponsStore[i].Skins[j].ID));
						break;
					case WeaponSkinQuality.Legendary:
						LegendarySkins.Add(new KeyValuePair<int, int>(GameSettings.instance.Weapons[i].ID, GameSettings.instance.WeaponsStore[i].Skins[j].ID));
						break;
					}
				}
			}
		}
		for (int k = 0; k < GameSettings.instance.Weapons.Count; k++)
		{
			if (GameSettings.instance.Weapons[k].Lock || !GameSettings.instance.Weapons[k].Secret)
			{
				continue;
			}
			for (int l = 0; l < GameSettings.instance.WeaponsStore[k].Skins.Count; l++)
			{
				if (GameSettings.instance.WeaponsStore[k].Skins[l].Quality != 0 && GameSettings.instance.WeaponsStore[k].Skins[l].Price == 0)
				{
					SecretWeaponSkins.Add(new KeyValuePair<int, int>(GameSettings.instance.Weapons[k].ID, GameSettings.instance.WeaponsStore[k].Skins[l].ID));
				}
			}
		}
	}

	private void UpdateStickerList()
	{
		if (BaseStickers.Count != 0)
		{
			return;
		}
		for (int i = 0; i < GameSettings.instance.Stickers.Count; i++)
		{
			switch (GameSettings.instance.Stickers[i].Quality)
			{
			case StickerQuality.Basic:
				BaseStickers.Add(GameSettings.instance.Stickers[i].ID);
				break;
			case StickerQuality.Professional:
				ProfessionalStickers.Add(GameSettings.instance.Stickers[i].ID);
				break;
			case StickerQuality.Legendary:
				LegendaryStickers.Add(GameSettings.instance.Stickers[i].ID);
				break;
			}
		}
	}

	public void OnSpeed()
	{
		Duration = 1f;
		Lerp = 10f;
	}

	public void StartCaseWheel(string caseName)
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			UIToast.Show("Not Internet");
			return;
		}
		Duration = 8f;
		Lerp = 1f;
		SelectCaseName = caseName;
		Case selectCase = GetCase(caseName);
		if (selectCase.Price != 0)
		{
			int num = ((selectCase.Currency != GameCurrency.Gold) ? SaveLoadManager.GetMoney() : SaveLoadManager.GetGold());
			if (selectCase.Price > num)
			{
				InAppPanel.SetActive(true);
				UIToast.Show(Localization.Get("Not enough money"));
				return;
			}
		}
		FinishWeaponFireStat = false;
		FinishWeaponAlready = false;
		UpdateOthersCaseItems();
		if (isSkinCases)
		{
			mPopUp.SetActiveWaitPanel(false);
			if (selectCase.Money)
			{
				SetFinishMoneyData();
			}
			else
			{
				SetFinishWeaponData(selectCase);
			}
			FinishTween = FinishPanel.GetComponent<TweenAlpha>();
			if (FinishTween != null)
			{
				FinishTween.enabled = false;
			}
			if (selectCase.Currency == GameCurrency.Gold)
			{
				SaveLoadManager.SetGold1(-selectCase.Price);
			}
			else
			{
				SaveLoadManager.SetMoney1(-selectCase.Price);
			}
			CaseWheelPanel.alpha = 1f;
			FinishPanel.alpha = 0f;
			mPanelManager.ShowPanel("CaseWheel", false);
			CaseItemsRoot.localPosition = new Vector3(0f, CaseItemsRoot.localPosition.y, 0f);
			StartRotate = Time.time;
			CaseRotate = true;
			LastSoundInterval = SoundInterval / 2f;
			FinishPosition = (int)UnityEngine.Random.Range(FinishInterval.x, FinishInterval.y);
		}
	}

	private void UpdateOthersCaseItems()
	{
		Case @case = GetCase(SelectCaseName);
		bool flag = @case.Money;
		int max = GameSettings.instance.Stickers.Count - 1;
		for (int i = 0; i < CaseItems.Length; i++)
		{
			flag = @case.Money;
			if (flag && UnityEngine.Random.value < 0.4f)
			{
				flag = false;
			}
			if (flag)
			{
				SetCaseMoneyItem(CaseItems[i]);
			}
			else if (isSkinCases)
			{
				int num = UnityEngine.Random.Range(0, 100);
				KeyValuePair<int, int> keyValuePair = default(KeyValuePair<int, int>);
				keyValuePair = ((num < 25) ? ((@case.Normal == 0) ? BaseSkins[UnityEngine.Random.Range(0, BaseSkins.Count)] : NormalSkins[UnityEngine.Random.Range(0, NormalSkins.Count)]) : ((num < 50) ? BaseSkins[UnityEngine.Random.Range(0, BaseSkins.Count)] : ((num < 75) ? ProfessionalSkins[UnityEngine.Random.Range(0, ProfessionalSkins.Count)] : ((i <= 10 || @case.SecretWeapon == 0 || num <= 98) ? LegendarySkins[UnityEngine.Random.Range(0, LegendarySkins.Count)] : SecretWeaponSkins[UnityEngine.Random.Range(0, SecretWeaponSkins.Count)]))));
				SetCaseWeaponItem(CaseItems[i], keyValuePair.Key, keyValuePair.Value);
			}
			else
			{
				int index = UnityEngine.Random.Range(0, max);
				SetCaseStickerItem(CaseItems[i], GameSettings.instance.Stickers[index]);
			}
		}
	}

	private void UpdateCaseWheel()
	{
		if (CaseRotate)
		{
			float time = (Time.time - StartRotate) / Duration;
			float num = Curve.Evaluate(time);
			float x = num * FinishPosition;
			float num2 = 1f - num + Lerp;
			CaseItemsRoot.localPosition = Vector3.Lerp(CaseItemsRoot.localPosition, new Vector3(x, CaseItemsRoot.localPosition.y, 0f), Time.deltaTime * num2);
			if (0f - CaseItemsRoot.localPosition.x > LastSoundInterval && Duration != 1f)
			{
				SoundSource.PlayOneShot(SoundSource.clip);
				LastSoundInterval += SoundInterval;
			}
			if (CaseItemsRoot.localPosition.x <= FinishPosition + 2f)
			{
				CaseRotate = false;
				StartFinishPanel();
			}
		}
	}

	private void SetFinishWeaponData(Case selectCase)
	{
		int randomQualty = GetRandomQualty(selectCase);
		if (isSkinCases)
		{
			isFinishWeapon = true;
			bool flag = selectCase.SecretWeapon >= UnityEngine.Random.Range(nValue.int1, nValue.int100);
			KeyValuePair<int, int> keyValuePair = default(KeyValuePair<int, int>);
			if (flag)
			{
				keyValuePair = SecretWeaponSkins[UnityEngine.Random.Range(nValue.int0, SecretWeaponSkins.Count)];
				if (keyValuePair.Key == 40 && UnityEngine.Random.Range(nValue.int0, nValue.int150) != 40)
				{
					do
					{
						keyValuePair = SecretWeaponSkins[UnityEngine.Random.Range(nValue.int0, SecretWeaponSkins.Count)];
					}
					while (keyValuePair.Key == 40);
				}
				if (keyValuePair.Key == 45 && UnityEngine.Random.Range(nValue.int0, nValue.int150) != 45)
				{
					do
					{
						keyValuePair = SecretWeaponSkins[UnityEngine.Random.Range(nValue.int0, SecretWeaponSkins.Count)];
					}
					while (keyValuePair.Key == 45);
				}
			}
			else
			{
				switch (randomQualty)
				{
					case 1:
						keyValuePair = NormalSkins[UnityEngine.Random.Range(nValue.int0, NormalSkins.Count)];
						break;
					case 2:
						keyValuePair = BaseSkins[UnityEngine.Random.Range(nValue.int0, BaseSkins.Count)];
						break;
					case 3:
						keyValuePair = ProfessionalSkins[UnityEngine.Random.Range(nValue.int0, ProfessionalSkins.Count)];
						break;
					case 4:
						keyValuePair = LegendarySkins[UnityEngine.Random.Range(nValue.int0, LegendarySkins.Count)];
						break;
				}
			}
			FinishWeapon = WeaponManager.GetWeaponData(keyValuePair.Key);
			FinishWeaponSkin = WeaponManager.GetWeaponSkin(keyValuePair.Key, keyValuePair.Value);
			if (FinishWeaponSkin.Quality != WeaponSkinQuality.Default && FinishWeaponSkin.Quality != WeaponSkinQuality.Normal && FinishWeapon.Type != WeaponType.Knife && GetCase(SelectCaseName).FireStat >= UnityEngine.Random.Range(nValue.int1, nValue.int100))
			{
				FinishWeaponFireStat = true;
				if (!SaveLoadManager.GetFireStat(FinishWeapon.ID, FinishWeaponSkin.ID))
				{
					SaveLoadManager.SetFireStat(FinishWeapon.ID, FinishWeaponSkin.ID);
				}
			}
			else
			{
				FinishWeaponFireStat = false;
			}
			FinishWeaponAlready = SaveLoadManager.GetWeaponSkin(FinishWeapon.ID, FinishWeaponSkin.ID);
			if (FinishWeaponAlready)
			{
				SetQualityMoney(FinishWeaponSkin.Quality, FinishWeaponFireStat, FinishWeapon.Secret, GetCase(SelectCaseName).Money);
			}
			SaveLoadManager.SetWeaponSkin(FinishWeapon.ID, FinishWeaponSkin.ID);
			if (FinishWeapon.Secret)
			{
				SaveLoadManager.SetWeapon(FinishWeapon.ID);
				SaveLoadManager.SetWeaponSkin(FinishWeapon.ID, FinishWeaponSkin.ID);
			}
			SetCaseWeaponItem(FinishItem, keyValuePair.Key, keyValuePair.Value);
			return;
		}
		int index = -1;
		switch (randomQualty)
		{
			case 1:
				index = BaseStickers[UnityEngine.Random.Range(nValue.int0, BaseStickers.Count)];
				break;
			case 2:
				index = ProfessionalStickers[UnityEngine.Random.Range(nValue.int0, ProfessionalStickers.Count)];
				break;
			case 3:
				index = LegendaryStickers[UnityEngine.Random.Range(nValue.int0, LegendaryStickers.Count)];
				break;
		}
		FinishSticker = GameSettings.instance.Stickers[index];
		SetCaseStickerItem(FinishItem, FinishSticker);
	}

	private void SetQualityMoney(WeaponSkinQuality quality, bool firestat, bool secret, bool moneyCase)
	{
		int num = 0;
		int gold = 0;
		switch (quality)
		{
			case WeaponSkinQuality.Default:
			case WeaponSkinQuality.Normal:
				if (secret)
				{
					gold = 15;
				}
				else
				{
					num = 100;
				}
				break;
			case WeaponSkinQuality.Basic:
				if (secret)
				{
					gold = 20;
				}
				else if (firestat)
				{
					gold = 8;
				}
				else
				{
					num = 300;
				}
				break;
			case WeaponSkinQuality.Professional:
				if (secret)
				{
					gold = 25;
				}
				else if (firestat)
				{
					gold = 12;
				}
				else
				{
					gold = 5;
				}
				break;
			case WeaponSkinQuality.Legendary:
				if (secret)
				{
					gold = 30;
				}
				else if (firestat)
				{
					gold = 16;
				}
				else
				{
					gold = 8;
				}
				break;
		}
		if (moneyCase)
		{
			FinishAlreadyAvailableTexture.cachedGameObject.SetActive(false);
		}
		else
		{
			FinishAlreadyAvailableTexture.cachedGameObject.SetActive(true);
			SaveLoadManager.SetGold1(gold);
			FinishAlreadyAvailableLabel.text = gold.ToString();
		}
	}

	private int GetRandomQualty(Case selectCase)
	{
		int num = UnityEngine.Random.Range(nValue.int0, nValue.int100);
		if (isSkinCases)
		{
			int num2 = (!selectCase.Money) ? 1 : 2;
			if ((selectCase.Normal + selectCase.Base + selectCase.Professional) * num2 < num)
			{
				return 4;
			}
			if ((selectCase.Normal + selectCase.Base) * num2 < num)
			{
				return 3;
			}
			if (selectCase.Normal * num2 < num)
			{
				return 2;
			}
			if (selectCase.SecretWeapon != nValue.int0)
			{
				return 2;
			}
			return 1;
		}
		else
		{
			if (selectCase.Base + selectCase.Professional < num)
			{
				return 3;
			}
			if (selectCase.Base < num)
			{
				return 2;
			}
			return 1;
		}
	}

	private void SetFinishMoneyData()
	{
		isFinishWeapon = false;
		SaveLoadManager.SetGold1(2);
		SetCaseMoneyItem(FinishItem);
	}

	private void SetCaseWeaponItem(mCaseItem caseItem, int weaponID, int ID)
	{
		WeaponSkinData weaponSkin = WeaponManager.GetWeaponSkin(weaponID, ID);
		caseItem.ItemWeaponTexture.atlas = GameSettings.instance.WeaponIconAtlas;
		caseItem.ItemWeaponTexture.cachedGameObject.SetActive(true);
		caseItem.ItemGoldMoneyTexture.cachedGameObject.SetActive(false);
		caseItem.ItemWeaponTexture.spriteName = weaponID + "-" + ID;
		caseItem.ItemWeaponTexture.width = (int)GameSettings.instance.WeaponsCaseSize[weaponID - 1].x;
		caseItem.ItemWeaponTexture.height = (int)GameSettings.instance.WeaponsCaseSize[weaponID - 1].y;
		caseItem.ItemLabel.text = weaponSkin.Name;
		caseItem.ItemQualityLabel.text = Localization.Get(weaponSkin.Quality.ToString() + " quality");
		caseItem.ItemLabelSprite.color = GetSkinQualityColor(weaponSkin.Quality);
	}

	private void SetCaseMoneyItem(mCaseItem caseItem)
	{
		caseItem.ItemGoldMoneyTexture.cachedGameObject.SetActive(true);
		caseItem.ItemWeaponTexture.cachedGameObject.SetActive(false);
		caseItem.ItemGoldMoneyTexture.width = 80;
		caseItem.ItemGoldMoneyTexture.height = 80;
		caseItem.ItemGoldMoneyTexture.uvRect = new Rect(0f, 0f, 1f, 1f);
		caseItem.ItemLabel.text = "BS Coins";
		caseItem.ItemQualityLabel.text = string.Empty;
		caseItem.ItemLabelSprite.color = GetSkinQualityColor(WeaponSkinQuality.Normal);
	}

	private void SetCaseStickerItem(mCaseItem caseItem, StickerData sticker)
	{
		caseItem.ItemWeaponTexture.atlas = GameSettings.instance.StickersAtlas;
		caseItem.ItemWeaponTexture.cachedGameObject.SetActive(true);
		caseItem.ItemGoldMoneyTexture.cachedGameObject.SetActive(false);
		caseItem.ItemWeaponTexture.spriteName = sticker.ID.ToString();
		caseItem.ItemWeaponTexture.width = 64;
		caseItem.ItemWeaponTexture.height = 64;
		caseItem.ItemLabel.text = sticker.Name;
		caseItem.ItemQualityLabel.text = Localization.Get(sticker.Quality.ToString() + " quality");
		caseItem.ItemLabelSprite.color = GetStickerQualityColor(sticker.Quality);
	}

	private Color GetSkinQualityColor(WeaponSkinQuality quality)
	{
		switch (quality)
		{
		case WeaponSkinQuality.Default:
		case WeaponSkinQuality.Normal:
			return new Color(0.63f, 0.63f, 0.63f, 1f);
		case WeaponSkinQuality.Basic:
			return new Color(0.07f, 0.65f, 0.87f, 1f);
		case WeaponSkinQuality.Professional:
			return new Color(0.9f, 0f, 0f, 1f);
		case WeaponSkinQuality.Legendary:
			return new Color(0.87f, 0f, 0.38f, 1f);
		default:
			return new Color(0.63f, 0.63f, 0.63f, 1f);
		}
	}

	private Color GetStickerQualityColor(StickerQuality quality)
	{
		switch (quality)
		{
		case StickerQuality.Basic:
			return new Color(0.07f, 0.65f, 0.87f, 1f);
		case StickerQuality.Professional:
			return new Color(0.9f, 0f, 0f, 1f);
		case StickerQuality.Legendary:
			return new Color(0.87f, 0f, 0.38f, 1f);
		default:
			return new Color(0.63f, 0.63f, 0.63f, 1f);
		}
	}

	public void StartFinishPanel()
	{
		CaseWheelTween = TweenAlpha.Begin(CaseWheelPanel.cachedGameObject, 0.25f, 0f);
		FinishTween = TweenAlpha.Begin(FinishPanel.cachedGameObject, 0.5f, 1f);
		ShareButton.SetActive(isFinishWeapon);
		if (isSkinCases)
		{
			if (isFinishWeapon)
			{
				Color skinQualityColor = GetSkinQualityColor(FinishWeaponSkin.Quality);
				if (FinishWeaponSkin.Quality != 0 && FinishWeaponSkin.Quality != WeaponSkinQuality.Normal && FinishWeapon.Type != WeaponType.Knife)
				{
					if (FinishWeaponFireStat)
					{
						FinishFireStatEffect.cachedTransform.parent.gameObject.SetActive(true);
						FinishFireStatEffect.color = skinQualityColor;
					}
					else
					{
						FinishFireStatEffect.cachedTransform.parent.gameObject.SetActive(false);
					}
				}
				else
				{
					FinishFireStatEffect.cachedTransform.parent.gameObject.SetActive(false);
				}
				if ((bool)FinishWeapon.Secret)
				{
					FinishSecretWeaponEffect.cachedTransform.parent.gameObject.SetActive(true);
					FinishSecretWeaponEffect.color = skinQualityColor;
				}
				else
				{
					FinishSecretWeaponEffect.cachedTransform.parent.gameObject.SetActive(false);
				}
				FinishBackground.color = skinQualityColor;
				FinishBackground.alpha = 0.6f;
				FinishEffect1.color = skinQualityColor;
				FinishEffect1.alpha = 0.7f;
				FinishEffect2.color = skinQualityColor;
				FinishLabel.text = string.Concat(FinishWeapon.Name, " | ", FinishWeaponSkin.Name);
				FinishQualityLabel.text = Localization.Get(FinishWeaponSkin.Quality.ToString() + " quality");
				FinishWeaponTexture.atlas = GameSettings.instance.WeaponIconAtlas;
				FinishWeaponTexture.cachedGameObject.SetActive(true);
				FinishGoldTexture.cachedGameObject.SetActive(false);
				FinishWeaponTexture.cachedGameObject.SetActive(false);
				mWeaponCamera.SetViewportRect(new Rect(0f, 0f, 1f, 1f), 0f);
				mWeaponCamera.Show(FinishWeapon.Name);
				mWeaponCamera.SetRotation(new Vector3(0f, -145f, 0f));
				mWeaponCamera.SetFieldOfView(180f, 75f, 0.25f);
				mWeaponCamera.SetSkin(FinishWeapon.ID, FinishWeaponSkin.ID);
				FinishAlreadyAvailable.SetActive(FinishWeaponAlready);
			}
			else
			{
				Color skinQualityColor2 = GetSkinQualityColor(WeaponSkinQuality.Normal);
				FinishBackground.color = skinQualityColor2;
				FinishBackground.alpha = 0.6f;
				FinishEffect1.color = skinQualityColor2;
				FinishEffect1.alpha = 0.7f;
				FinishEffect2.color = skinQualityColor2;
				FinishLabel.text = "BS Coins | +2";
				FinishQualityLabel.text = string.Empty;
				mWeaponCamera.SetViewportRect(new Rect(0f, 0f, 1f, 1f), 0f);
				mWeaponCamera.Show("BS Gold");
				mWeaponCamera.SetRotation(new Vector3(0f, 0f, 0f));
				mWeaponCamera.SetFieldOfView(180f, 75f, 0.25f);
				FinishWeaponTexture.cachedGameObject.SetActive(false);
				FinishAlreadyAvailable.SetActive(false);
				FinishFireStatEffect.cachedTransform.parent.gameObject.SetActive(false);
				FinishSecretWeaponEffect.cachedTransform.parent.gameObject.SetActive(false);
			}
		}
		else
		{
			Color stickerQualityColor = GetStickerQualityColor(FinishSticker.Quality);
			FinishFireStatEffect.cachedTransform.parent.gameObject.SetActive(false);
			FinishSecretWeaponEffect.cachedTransform.parent.gameObject.SetActive(false);
			FinishBackground.color = stickerQualityColor;
			FinishBackground.alpha = 0.6f;
			FinishEffect1.color = stickerQualityColor;
			FinishEffect1.alpha = 0.7f;
			FinishEffect2.color = stickerQualityColor;
			FinishLabel.text = FinishSticker.Name;
			FinishQualityLabel.text = Localization.Get(FinishSticker.Quality.ToString() + " quality");
			FinishWeaponTexture.atlas = GameSettings.instance.StickersAtlas;
			FinishGoldTexture.cachedGameObject.SetActive(false);
			mWeaponCamera.SetViewportRect(new Rect(0f, 0f, 1f, 1f), 0f);
			mWeaponCamera.Show("Sticker");
			mWeaponCamera.SetRotation(new Vector3(15f, -20f, 0f));
			mWeaponCamera.SetFieldOfView(180f, 75f, 0.25f);
			mWeaponCamera.SetSkin(0, FinishSticker.ID);
			FinishAlreadyAvailable.SetActive(false);
		}
		EventManager.Dispatch("AccountUpdate");
	}

	public void StartRewardedCase()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		mPopUp.SetActiveWaitPanel(true, Localization.Get("Loading") + "...");
		TimerManager.In(0.5f, delegate
		{
			RewardedVideoComplete();
		});
	}

	private void RewardedVideoComplete()
	{
		TimerManager.In(0.3f, delegate
		{
			mPopUp.SetActiveWaitPanel(false);
			StartCaseWheel("Free");
		});
	}

	private void RewardedVideoAborted()
	{
		TimerManager.In(0.3f, delegate
		{
			UIToast.Show(Localization.Get("Cancel"));
			mPopUp.SetActiveWaitPanel(false);
		});
	}

	private void RewardedVideoFailed()
	{
		TimerManager.In(0.3f, delegate
		{
			UIToast.Show(Localization.Get("Video not available"));
			mPopUp.SetActiveWaitPanel(false);
		});
	}

	public void ShareScreenshot()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		BackButton.SetActive(false);
		ShareButton.SetActive(false);
		TimerManager.In(0.5f, delegate
		{
			string text = string.Concat(FinishWeapon.Name, " | ", FinishWeaponSkin.Name, "_", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
			AndroidNativeFunctions.TakeScreenshot(text, delegate(string path)
			{
				BackButton.SetActive(true);
				ShareButton.SetActive(true);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("#BlockStrike #BS");
				stringBuilder.AppendLine(string.Concat("I just got ", FinishWeapon.Name, " | ", FinishWeaponSkin.Name, " in game Block Strike"));
				stringBuilder.AppendLine("http://bit.ly/blockstrike");
				AndroidNativeFunctions.ShareScreenshot(stringBuilder.ToString(), "Block Strike", Localization.Get("Share"), path);
			});
		});
	}
}
