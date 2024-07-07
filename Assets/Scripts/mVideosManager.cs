using Beebyte.Obfuscator;
using FreeJSON;
using UnityEngine;

public class mVideosManager : MonoBehaviour
{
	public mVideoElement[] elements;

	public GameObject backButton;

	public TweenAlpha newVideoTween;

	private static JsonArray jsonVideos;

	public static mVideosManager instance;

	private void Start()
	{
		instance = this;
		EventManager.AddListener("AccountConnected", AccountConnected);
	}

	private void AccountConnected()
	{
	}

	[SkipRename]
	public void Open()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		newVideoTween.gameObject.SetActive(false);
		mPanelManager.ShowPanel("Videos", true);
		if (jsonVideos != null)
		{
			UpdateList();
			return;
		}
		Firebase firebase = new Firebase();
		firebase.Child("Videos").Child("Global").GetValue(LoadSuccess, LoadFailed);
		mPopUp.SetActiveWait(true, Localization.Get("Loading") + "...");
		backButton.SetActive(false);
	}

	private void UpdateList()
	{
		if (jsonVideos != null)
		{
			mPopUp.SetActiveWait(false);
			JsonArray jsonArray = new JsonArray();
			int num;
			for (num = 0; num < jsonVideos.Length; num++)
			{
				int index = Random.Range(0, jsonVideos.Length);
				jsonArray.Add(jsonVideos.Get<string>(index));
				jsonVideos.RemoveAt(index);
				num = -1;
			}
			jsonVideos = jsonArray;
			for (int i = 0; i < jsonVideos.Length && i < 11; i++)
			{
				elements[i].SetData(jsonVideos.Get<string>(i), i + 1f);
			}
		}
	}

	private void LoadSuccess(string json)
	{
		backButton.SetActive(true);
		jsonVideos = JsonArray.Parse(json);
		UpdateList();
	}

	private void LoadFailed(string error)
	{
		backButton.SetActive(true);
		mPopUp.SetActiveWait(false);
		UIToast.Show("Error: " + error, 3f);
	}

	[SkipRename]
	public void SendVideo()
	{
		AndroidNativeFunctions.SendEmail(Localization.Get("Link to video") + ":", "Video", "video@blockstrike.org");
	}
}
