using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mCheckUpdateGame : MonoBehaviour
{
	private void Start()
	{
		if (!AccountManager.isConnect)
		{
			StartCoroutine(CheckGame());
		}
	}

	private void Show()
	{
		AndroidNativeFunctions.ShowAlert(Localization.Get("Available new version of the game"), Localization.Get("New Version"), Localization.Get("Download"), string.Empty, string.Empty, Download);
		GameSettings.instance.PhotonID = string.Empty;
		TimerManager.In(0.1f, -1, 0.1f, delegate
		{
			AccountManager.isConnect = false;
		});
		TimerManager.In(20f, delegate
		{
			Application.Quit();
		});
	}

	private IEnumerator CheckGame()
	{
		string url = "https://play.google.com/store/apps/details?id=com.rexetstudio.blockstrike&hl=en";
		Dictionary<string, string> headers = new Dictionary<string, string> { { "User-Agent", "Golang_Spider_Bot/3.0" } };
		WWW www = new WWW(url);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			try
			{
				if (CheckVersion(www.text))
				{
					Show();
				}
				yield break;
			}
			catch
			{
				yield break;
			}
		}
		StartCoroutine(CheckGame());
	}

	private void Download(DialogInterface dialog)
	{
		AndroidNativeFunctions.OpenGooglePlay("com.rexetstudio.blockstrike");
		AndroidNativeFunctions.ShowAlert(Localization.Get("Available new version of the game"), Localization.Get("New Version"), Localization.Get("Download"), string.Empty, string.Empty, Download);
	}

	private bool CheckVersion(string data)
	{
		string text = data;
		int num = text.LastIndexOf("softwareVersion");
		if (num == -1)
		{
			num = text.LastIndexOf("Current Version");
			num += 46;
			text = text.Remove(0, num);
			num = text.IndexOf("</span>");
			return Utils.CompareVersion(VersionManager.bundleVersion, text.Remove(num));
		}
		num += 18;
		text = text.Remove(0, num);
		num = text.IndexOf("</div>") - 2;
		return Utils.CompareVersion(VersionManager.bundleVersion, text.Remove(num));
	}
}
