using UnityEngine;

public class mServerInfo : NGUIBehaviour
{
	public UILabel ServerNameLabel;

	public UILabel ModeLabel;

	public UILabel MapNameLabel;

	public UILabel PlayersLabel;

	public GameObject Password;

	private RoomInfo room;

	private void Start()
	{
		NGUIEvents.Add(gameObject, this);
	}

	public void SetData(RoomInfo info)
	{
		room = info;
		ServerNameLabel.text = info.Name;
		if (info.GetGameMode() == GameMode.Only)
		{
			ModeLabel.text = Localization.Get(info.GetGameMode().ToString()) + " (" + WeaponManager.GetWeaponName(info.GetOnlyWeapon()) + ")";
		}
		else if (info.GetMapHash() != 0)
		{
			ModeLabel.text = Localization.Get(info.GetGameMode().ToString()) + "[*]";
		}
		else
		{
			ModeLabel.text = Localization.Get(info.GetGameMode().ToString());
		}
		MapNameLabel.text = info.GetSceneName();
		PlayersLabel.text = info.PlayerCount + "/" + info.MaxPlayers;
		if (string.IsNullOrEmpty(info.GetPassword()))
		{
			Password.SetActive(false);
		}
		else
		{
			Password.SetActive(true);
		}
	}

	public override void OnClick()
	{
		mJoinServer.room = room;
		mJoinServer.onBack = OnBack;
		mJoinServer.Join();
	}

	private void OnBack()
	{
		mPopUp.HideAll("ServerList", false);
	}
}
