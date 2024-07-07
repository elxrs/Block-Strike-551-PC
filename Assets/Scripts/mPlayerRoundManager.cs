using System;
using UnityEngine;

public class mPlayerRoundManager : MonoBehaviour
{
	public GameObject Panel;

	public UILabel ModeLabel;

	public UILabel MapLabel;

	public UILabel MoneyLabel;

	public UILabel XpLabel;

	public UILabel KillsLabel;

	public UILabel HeadshotLabel;

	public UILabel DeathsLabel;

	public UILabel TimeLabel;

	public UILabel TotalXpLabel;

	private static mPlayerRoundManager instance;

	private void Start()
	{
		instance = this;
	}

	public static void Show()
	{
		instance.Panel.SetActive(true);
		instance.ModeLabel.text = Localization.Get(PlayerRoundManager.GetMode().ToString());
		instance.MoneyLabel.text = PlayerRoundManager.GetMoney().ToString();
		instance.XpLabel.text = PlayerRoundManager.GetXP().ToString();
		instance.KillsLabel.text = PlayerRoundManager.GetKills().ToString();
		instance.HeadshotLabel.text = PlayerRoundManager.GetHeadshot().ToString();
		instance.DeathsLabel.text = PlayerRoundManager.GetDeaths().ToString();
		instance.TimeLabel.text = ConvertTime(PlayerRoundManager.GetTime());
	}

	public void Next()
	{
	}

	public void Last()
	{
	}

	public void Close()
	{
		PlayerRoundManager.Clear();
		EventManager.Dispatch("AccountUpdate");
	}

	private static string ConvertTime(float time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		return string.Format("{0:0}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	}
}
