using UnityEngine;

public class mPlayerData : MonoBehaviour
{
	public UILabel PlayerNameLabel;

	public UILabel PlayerLevelLabel;

	public UIProgressBar PlayerXP;

	public UILabel MoneyLabel;

	public UILabel GoldLabel;

	public UITexture AvatarTexture;

	private void Start()
	{
		EventManager.AddListener("AccountUpdate", AccountUpdate);
		EventManager.AddListener("AvatarUpdate", AvatarUpdate);
		AccountUpdate();
		AvatarUpdate();
	}

	private void AccountUpdate()
	{
		if (string.IsNullOrEmpty(SaveLoadManager.GetClan().ToString()))
		{
			PlayerNameLabel.text = SaveLoadManager.GetPlayerName();
		}
		else
		{
			PlayerNameLabel.text = string.Concat(SaveLoadManager.GetPlayerName(), " - ", SaveLoadManager.GetClan());
		}
		PlayerLevelLabel.text = Localization.Get("Level") + " - " + SaveLoadManager.GetPlayerLevel();
		PlayerXP.value = SaveLoadManager.GetPlayerXP() / GetMaxXP();
		GoldLabel.text = SaveLoadManager.GetGold().ToString("n0");
		MoneyLabel.text = SaveLoadManager.GetMoney().ToString("n0");
	}

	public static int GetMaxXP()
	{
		return 150 + 150 * SaveLoadManager.GetPlayerLevel();
	}

	private void AvatarUpdate()
	{
		AvatarTexture.mainTexture = AccountManager.instance.Data.Avatar;
	}
}
