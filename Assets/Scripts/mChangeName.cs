using System.Linq;
using Beebyte.Obfuscator;
using UnityEngine;

public class mChangeName : MonoBehaviour
{
	[SkipRename]
	public void ChangeName()
	{
		if (mPanelManager.GetActivePanel() == "Weapons" || mPanelManager.GetActivePanel() == "PlayerSkin")
		{
			return;
		}
		if (!AccountManager.isConnect)
		{
			if (SaveLoadManager.GetPlayerLevel() < 3)
			{
				UIToast.Show(Localization.Get("Requires Level") + " 3");
			}
			else
			{
				UIToast.Show(Localization.Get("Connection account"));
			}
		}
		else
		{
			mPopUp.ShowPopup(Localization.Get("Cost of change name 100 bs coins"), Localization.Get("ChangeName"), Localization.Get("Back"), ChangeNameCancel, "Ok", ChangeNameInput);
		}
	}

	private void ChangeNameInput()
	{
		if (SaveLoadManager.GetGold() < 100)
		{
			UIToast.Show(Localization.Get("Not enough money"));
		}
		else
		{
			mPopUp.ShowInput(SaveLoadManager.GetPlayerName(), Localization.Get("ChangeName"), 12, UIInput.KeyboardType.Default, OnSubmit, OnChange, Localization.Get("Back"), ChangeNameCancel, "Ok", OnYes);
		}
	}

	private void ChangeNameCancel()
	{
		mPopUp.HideAll();
	}

	private void OnSubmit()
	{
		string text = mPopUp.GetInputText();
		if (text.Length <= 3 || text == "Null" || text[0].ToString() == " " || text[text.Length - 1].ToString() == " " || text.ToLower().Contains("tibers") || text.ToLower().Contains("raum") || text.ToLower().Contains("jekisk"))
		{
			text = "Player " + Random.Range(0, 99999);
		}
		text = NGUIText.StripSymbols(text);
		mPopUp.SetInputText(text);
	}

	private void OnChange()
	{
		string inputText = mPopUp.GetInputText();
		string text = UpdateSymbols(inputText, true);
		if (inputText != text)
		{
			mPopUp.SetInputText(text);
		}
	}

	private void OnYes()
	{
		string inputText = mPopUp.GetInputText();
		string text = UpdateSymbols(inputText, true);
		if (inputText != text)
		{
			mPopUp.SetInputText(text);
		}
		else if (inputText != NGUIText.StripSymbols(inputText))
		{
			mPopUp.SetInputText(NGUIText.StripSymbols(inputText));
		}
		else if (inputText.Length <= 3 || inputText == "Null" || inputText[0].ToString() == " " || inputText[inputText.Length - 1].ToString() == " " || text.ToLower().Contains("tibers") || text.ToLower().Contains("raum") || text.ToLower().Contains("jekisk"))
		{
			inputText = "Player " + Random.Range(0, 99999);
			mPopUp.SetInputText(inputText);
		}
		else if (SaveLoadManager.GetPlayerName() == inputText)
		{
			return;
		}
		mPopUp.HideAll("Menu");
		SaveLoadManager.SetGold1(-100);
		SaveLoadManager.SetPlayerName(inputText);
		EventManager.Dispatch("AccountUpdate");
	}

	[SkipRename]
	public void ChangeClanTag()
	{
		if (!(mPanelManager.GetActivePanel() == "Weapons") && !(mPanelManager.GetActivePanel() == "PlayerSkin"))
		{
			if (!AccountManager.isConnect)
			{
				UIToast.Show(Localization.Get("Connection account"));
			}
			else if (SaveLoadManager.GetPlayerLevel() < 15)
			{
				UIToast.Show(Localization.Get("Requires Level") + " 15");
			}
			else
			{
				mPopUp.ShowInput(SaveLoadManager.GetClan(), Localization.Get("ClanTag"), 4, UIInput.KeyboardType.Default, OnSubmitClanTag, OnChangeClanTag, Localization.Get("Back"), OnChangeClanTagCancel, "Ok", OnChangeClanTagComplete);
			}
		}
	}

	private void OnSubmitClanTag()
	{
		string inputText = mPopUp.GetInputText();
		inputText = UpdateSymbols(inputText, false);
		inputText = inputText.Replace(" ", string.Empty);
		inputText = inputText.ToUpper();
		if (inputText.Contains("DEV"))
		{
			inputText = string.Empty;
		}
		mPopUp.SetInputText(inputText);
	}

	private void OnChangeClanTag()
	{
		string inputText = mPopUp.GetInputText();
		inputText = UpdateSymbols(inputText, false);
		inputText = inputText.Replace(" ", string.Empty);
		inputText = inputText.ToUpper();
		mPopUp.SetInputText(inputText);
	}

	private void OnChangeClanTagComplete()
	{
		if (mPopUp.GetInputText().Contains("DEV"))
		{
			mPopUp.SetInputText(string.Empty);
			return;
		}
		SaveLoadManager.SetClan(mPopUp.GetInputText());
		if (PhotonNetwork.connected)
		{
			PhotonNetwork.player.SetClan(SaveLoadManager.GetClan());
		}
		mPopUp.HideAll();
		EventManager.Dispatch("AccountUpdate");
	}

	private void OnChangeClanTagCancel()
	{
		mPopUp.HideAll();
	}

	public static string UpdateSymbols(string text, bool isSymbol)
	{
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			if (CheckSymbol(text[i].ToString(), isSymbol))
			{
				text2 += text[i];
			}
		}
		return text2;
	}

	public static bool CheckSymbols(string text, bool isSymbol)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (!CheckSymbol(text[i].ToString(), isSymbol))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CheckSymbol(string symbol, bool isSymbol)
	{
		string[] source = new string[14]
		{
			"-", "_", "'", " ", "!", "@", "№", ";", "%", "^",
			":", "&", "?", "*"
		};
		string[] source2 = new string[10] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
		string[] source3 = new string[6] { "a", "e", "i", "o", "u", "y" };
		string[] source4 = new string[20]
		{
			"b", "c", "d", "f", "g", "h", "j", "k", "l", "m",
			"n", "p", "q", "r", "s", "t", "v", "w", "x", "z"
		};
		symbol = symbol.ToLower();
		if (source.Contains(symbol))
		{
			return isSymbol;
		}
		if (source2.Contains(symbol))
		{
			return true;
		}
		if (source3.Contains(symbol))
		{
			return true;
		}
		if (source4.Contains(symbol))
		{
			return true;
		}
		return false;
	}
}
