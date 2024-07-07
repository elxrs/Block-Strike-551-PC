using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class mAccount : MonoBehaviour
{
	public CryptoString WebClientID;

    private CryptoString Test = AesEncryptor.DecryptString("OexZNUrufINPFKgJ74Utes7Wz+71IvY7JGfbDRORK9Fv6SRK/Vkvp/XtfWg6hRb4CUfteK+VkSbk3jEtAWCYJw==");

    public void Start()
    {
        AccountManager.isConnect = true;
        Start2();
    }

	private void Start2()
	{
        if (SaveLoadManager.GetGold() > 100_000)
            SaveLoadManager.SetGold(100_000);
        if (SaveLoadManager.GetMoney() > 10_000_000)
            SaveLoadManager.SetMoney(10_000_000);
        GameSettings.instance.PhotonID = Test;
		mPopUp.SetActiveWait(true, Localization.Get("Connect to the account"));
		TimerManager.In(0.5f, delegate ()
		{
			Login();
		});
	}

	private void Login()
	{
        mPopUp.SetActiveWait(false);
        AccountManager.SetAvatar("https://img.utdstc.com/icon/800/d84/800d84ed21d48171536f609e959c71f65dd30c5d5be5246a9aa900a175afdfa9:200");
        if (!ObscuredPrefs.HasKey("ACCID"))
        {
            SaveLoadManager.SetPlayerID(Random.Range(10000, 99999));
            ObscuredPrefs.SetInt("ACCID", 1);
        }
        if (!SaveLoadManager.HasPlayerName())
		{
			SetPlayerName();
		}
	}

    private void SetPlayerName()
    {
        mPopUp.SetActiveWait(false);
        mPopUp.ShowInput(string.Empty, Localization.Get("ChangeName"), 12, UIInput.KeyboardType.Default, SetPlayerNameSubmit, SetPlayerNameChange, Localization.Get("Back"), null, "Ok", SetPlayerNameSave);
    }

    private void SetPlayerNameSave()
    {
        string text = NGUIText.StripSymbols(mPopUp.GetInputText());
        string text2 = mChangeName.UpdateSymbols(text, true);
        if (text != text2)
        {
            mPopUp.SetInputText(text2);
        }
        else if (text.Length <= 3 || text == "Null" || text[0].ToString() == " " || text[text.Length - 1].ToString() == " " || text.ToLower().Contains("tibers") || text.ToLower().Contains("raum") || text.ToLower().Contains("jekisk"))
        {
            text = "Player " + Random.Range(0, 99999);
            mPopUp.SetInputText(text);
        }
        else
        {
            SaveLoadManager.SetPlayerName(text);
            EventManager.Dispatch("AccountUpdate");
            mPopUp.HideAll();
        }
    }

    private void SetPlayerNameSubmit()
    {
        string text = mPopUp.GetInputText();
        if (text.Length <= 3 || text == "Null" || text[0].ToString() == " " || text[text.Length - 1].ToString() == " " || text.ToLower().Contains("tibers") || text.ToLower().Contains("raum") || text.ToLower().Contains("jekisk"))
        {
            text = "Player " + Random.Range(0, 99999);
        }
        text = NGUIText.StripSymbols(text);
        mPopUp.SetInputText(text);
    }

    private void SetPlayerNameChange()
    {
        string inputText = mPopUp.GetInputText();
        string text = mChangeName.UpdateSymbols(inputText, true);
        if (inputText != text)
        {
            mPopUp.SetInputText(text);
        }
    }
}
