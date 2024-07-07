using System.Collections.Generic;
using Photon;
using UnityEngine;

public class mStorePlayerSkin : PunBehaviour
{
	[Header("Player Skin")]
	public GameObject PlayerSkinPanel;

	public GameObject PlayerSkinBackground;

	public BodyParts SelectBodyParts;

	public UILabel SelectBodyPartsLabel;

	public GameObject SelectSkinButton;

	public UILabel SelectSkinButtonLabel;

	public GameObject BuySkinButton;

	public UILabel BuySkinButtonLabel;

	public UITexture BuySkinButtonTexture;

	public UISprite ChangeTeamButton;

	public UILabel InfoLabel;

	[Header("Others")]
	public Texture2D MoneyTexture;

	public Texture2D GoldTexture;

	public GameObject InAppPanel;

	public int HeadSkin;

	public int BodySkin;

	public int LegsSkin;

	private Team SkinTeam = Team.Blue;

	private PlayerStoreSkinData HeadData;

	private PlayerStoreSkinData BodyData;

	private PlayerStoreSkinData LegsData;

	private List<int> HeadList = new List<int>();

	private List<int> BodyList = new List<int>();

	private List<int> LegsList = new List<int>();

	private bool Active;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(PlayerSkinBackground);
		uIEventListener.onDrag = RotateWeapon;
	}

	public override void OnDisconnectedFromPhoton()
	{
		Close();
	}

	private void RotateWeapon(GameObject go, Vector2 drag)
	{
		mPlayerCamera.Rotate(drag);
	}

	public void Show()
	{
		Active = true;
		mPanelManager.SetActivePlayerData(false);
		SelectBodyPartsLabel.text = Localization.Get(SelectBodyParts.ToString());
		GetSkinsList();
		HeadSkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Head), BodyParts.Head);
		BodySkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Body), BodyParts.Body);
		LegsSkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Legs), BodyParts.Legs);
		UpdateSkin();
	}

	public void Close()
	{
		if (Active)
		{
			mPanelManager.SetActivePlayerData(true);
			mPlayerCamera.Close();
			Active = false;
		}
	}

	private void UpdateSkin()
	{
		GetPlayerSkinData();
		mPlayerCamera.Show();
		mPlayerCamera.SetSkin(SkinTeam, HeadList[HeadSkin].ToString(), BodyList[BodySkin].ToString(), LegsList[LegsSkin].ToString());
		UpdateInfoLabel();
		UpdateButtons();
	}

	private void UpdateButtons()
	{
		bool flag = SaveLoadManager.GetPlayerSkin(GetSelectSkinData().ID, SelectBodyParts);
		if (GetSelectSkinData().Price == 0)
		{
			flag = true;
		}
		if (flag)
		{
			SelectSkinButton.SetActive(true);
			BuySkinButton.SetActive(false);
			if (SaveLoadManager.GetPlayerSkinSelected(SelectBodyParts) == GetSelectSkinData().ID)
			{
				SelectSkinButtonLabel.text = Localization.Get("Selected");
				SelectSkinButton.GetComponent<UISprite>().alpha = 0.8f;
			}
			else
			{
				SelectSkinButtonLabel.text = Localization.Get("Select");
				SelectSkinButton.GetComponent<UISprite>().alpha = 1f;
			}
		}
		else
		{
			SelectSkinButton.SetActive(false);
			BuySkinButton.SetActive(true);
			BuySkinButtonLabel.text = GetSelectSkinData().Price.ToString("n0");
			BuySkinButtonTexture.mainTexture = ((GetSelectSkinData().Currency != 0) ? GoldTexture : MoneyTexture);
		}
	}

	public void NextSkin()
	{
		switch (SelectBodyParts)
		{
		case BodyParts.Head:
			HeadSkin++;
			if (HeadSkin >= HeadList.Count)
			{
				HeadSkin = 0;
			}
			break;
		case BodyParts.Body:
			BodySkin++;
			if (BodySkin >= BodyList.Count)
			{
				BodySkin = 0;
			}
			break;
		case BodyParts.Legs:
			LegsSkin++;
			if (LegsSkin >= LegsList.Count)
			{
				LegsSkin = 0;
			}
			break;
		}
		UpdateSkin();
	}

	public void LastSkin()
	{
		switch (SelectBodyParts)
		{
		case BodyParts.Head:
			HeadSkin--;
			if (HeadSkin <= -1)
			{
				HeadSkin = HeadList.Count - 1;
			}
			break;
		case BodyParts.Body:
			BodySkin--;
			if (BodySkin <= -1)
			{
				BodySkin = BodyList.Count - 1;
			}
			break;
		case BodyParts.Legs:
			LegsSkin--;
			if (LegsSkin <= -1)
			{
				LegsSkin = LegsList.Count - 1;
			}
			break;
		}
		UpdateSkin();
	}

	public void ChangeTeam()
	{
		if (SkinTeam == Team.Blue)
		{
			SkinTeam = Team.Red;
			ChangeTeamButton.color = new Color32(237, 44, 45, byte.MaxValue);
		}
		else
		{
			SkinTeam = Team.Blue;
			ChangeTeamButton.color = new Color32(70, 136, 231, byte.MaxValue);
		}
		UpdateSkin();
	}

	public void LastBodyParts()
	{
		int selectBodyParts = (int)SelectBodyParts;
		selectBodyParts--;
		if (selectBodyParts < 0)
		{
			selectBodyParts = 2;
		}
		SelectBodyParts = (BodyParts)selectBodyParts;
		SelectBodyPartsLabel.text = Localization.Get(SelectBodyParts.ToString());
		HeadSkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Head), BodyParts.Head);
		BodySkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Body), BodyParts.Body);
		LegsSkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Legs), BodyParts.Legs);
		UpdateSkin();
	}

	public void NextBodyParts()
	{
		int selectBodyParts = (int)SelectBodyParts;
		selectBodyParts++;
		if (selectBodyParts > 2)
		{
			selectBodyParts = 0;
		}
		SelectBodyParts = (BodyParts)selectBodyParts;
		SelectBodyPartsLabel.text = Localization.Get(SelectBodyParts.ToString());
		HeadSkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Head), BodyParts.Head);
		BodySkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Body), BodyParts.Body);
		LegsSkin = GetSkinIndex(SaveLoadManager.GetPlayerSkinSelected(BodyParts.Legs), BodyParts.Legs);
		UpdateSkin();
	}

	public void SelectSkin()
	{
		if (SaveLoadManager.GetPlayerSkinSelected(SelectBodyParts) != GetSelectSkinData().ID)
		{
			SaveLoadManager.SetPlayerSkinSelected(GetSelectSkinData().ID, SelectBodyParts);
			UpdateButtons();
		}
	}

	public void BuySkin()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		int num = ((GetSelectSkinData().Currency != GameCurrency.Gold) ? SaveLoadManager.GetMoney() : SaveLoadManager.GetGold());
		if (GetSelectSkinData().Price > num)
		{
			InAppPanel.SetActive(true);
			UIToast.Show(Localization.Get("Not enough money"));
			return;
		}
		if (GetSelectSkinData().Currency == GameCurrency.Gold)
		{
			SaveLoadManager.SetGold(num - GetSelectSkinData().Price);
		}
		else
		{
			SaveLoadManager.SetMoney(num - GetSelectSkinData().Price);
		}
		SaveLoadManager.SetPlayerSkin(GetSelectSkinData().ID, SelectBodyParts);
		SaveLoadManager.SetPlayerSkinSelected(GetSelectSkinData().ID, SelectBodyParts);
		UpdateButtons();
		EventManager.Dispatch("AccountUpdate");
	}

	private void GetSkinsList()
	{
		HeadList.Clear();
		BodyList.Clear();
		LegsList.Clear();
		for (int i = 0; i < GameSettings.instance.PlayerStoreHead.Count; i++)
		{
			HeadList.Add(GameSettings.instance.PlayerStoreHead[i].ID);
		}
		for (int j = 0; j < GameSettings.instance.PlayerStoreBody.Count; j++)
		{
			BodyList.Add(GameSettings.instance.PlayerStoreBody[j].ID);
		}
		for (int k = 0; k < GameSettings.instance.PlayerStoreLegs.Count; k++)
		{
			LegsList.Add(GameSettings.instance.PlayerStoreLegs[k].ID);
		}
	}

	private void GetPlayerSkinData()
	{
		switch (SelectBodyParts)
		{
		case BodyParts.Head:
		{
			for (int j = 0; j < GameSettings.instance.PlayerStoreHead.Count; j++)
			{
				if (HeadList[HeadSkin] == GameSettings.instance.PlayerStoreHead[j].ID)
				{
					HeadData = GameSettings.instance.PlayerStoreHead[j];
					break;
				}
			}
			break;
		}
		case BodyParts.Body:
		{
			for (int k = 0; k < GameSettings.instance.PlayerStoreBody.Count; k++)
			{
				if (BodyList[BodySkin] == GameSettings.instance.PlayerStoreBody[k].ID)
				{
					BodyData = GameSettings.instance.PlayerStoreBody[k];
					break;
				}
			}
			break;
		}
		case BodyParts.Legs:
		{
			for (int i = 0; i < GameSettings.instance.PlayerStoreLegs.Count; i++)
			{
				if (LegsList[LegsSkin] == GameSettings.instance.PlayerStoreLegs[i].ID)
				{
					LegsData = GameSettings.instance.PlayerStoreLegs[i];
					break;
				}
			}
			break;
		}
		}
	}

	private void UpdateInfoLabel()
	{
		string text = Localization.Get("Head") + ": " + (HeadSkin + 1) + "/" + HeadList.Count + "\n" + Localization.Get("Body") + ": " + (BodySkin + 1) + "/" + BodyList.Count + "\n" + Localization.Get("Legs") + ": " + (LegsSkin + 1) + "/" + LegsList.Count;
		InfoLabel.text = text;
	}

	private PlayerStoreSkinData GetSelectSkinData()
	{
		switch (SelectBodyParts)
		{
		case BodyParts.Head:
			return HeadData;
		case BodyParts.Body:
			return BodyData;
		case BodyParts.Legs:
			return LegsData;
		default:
			return BodyData;
		}
	}

	private int GetSkinIndex(int skinID, BodyParts part)
	{
		switch (part)
		{
		case BodyParts.Head:
		{
			for (int j = 0; j < HeadList.Count; j++)
			{
				if (skinID == HeadList[j])
				{
					return j;
				}
			}
			break;
		}
		case BodyParts.Body:
		{
			for (int k = 0; k < BodyList.Count; k++)
			{
				if (skinID == BodyList[k])
				{
					return k;
				}
			}
			break;
		}
		case BodyParts.Legs:
		{
			for (int i = 0; i < LegsList.Count; i++)
			{
				if (skinID == LegsList[i])
				{
					return i;
				}
			}
			break;
		}
		}
		return 0;
	}
}
