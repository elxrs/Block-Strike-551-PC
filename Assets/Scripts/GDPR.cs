using UnityEngine;

public class GDPR : MonoBehaviour
{
	public GameObject panel;

	private void Start()
	{
		if (PlayerPrefs.GetInt("GDPR", 0) == 0)
		{
			ShowGDPR();
		}
		else
		{
			LevelManager.LoadLevel("Logo");
		}
	}

	private void ShowGDPR()
	{
		panel.SetActive(true);
		UISettings.UpdateLanguage();
	}

	public void OnAccept()
	{
		panel.SetActive(false);
		PlayerPrefs.SetInt("GDPR", 1);
		LevelManager.LoadLevel("Logo");
	}

	public void OnPrivacyPolicy()
	{
		Application.OpenURL("http://rexetstudio.com/en/privacypolicy");
	}
}
