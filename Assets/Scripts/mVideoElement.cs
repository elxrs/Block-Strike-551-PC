using System.Collections;
using FreeJSON;
using UnityEngine;

public class mVideoElement : MonoBehaviour
{
	public UITexture Icon;

	public UILabel TitleLabel;

	public UILabel ChannelLabel;

	private string id;

	public void SetData(string data, float delay)
	{
		TimerManager.In(delay / 10f, delegate
		{
			JsonObject jsonObject = JsonObject.Parse(data);
			id = jsonObject.Get<string>("id");
			StartCoroutine(LoadImage());
			TitleLabel.text = jsonObject.Get<string>("title");
			ChannelLabel.text = jsonObject.Get<string>("channel");
			TweenAlpha.Begin(gameObject, 0.5f, 1f);
		});
	}

	private IEnumerator LoadImage()
	{
		WWW www = new WWW("http://img.youtube.com/vi/" + id + "/0.jpg");
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			Icon.mainTexture = www.texture;
			CacheManager.SaveAsync(data: www.texture.EncodeToPNG(), name: id, path: "Videos", md5: true);
		}
	}

	private void CreateIcon(byte[] bytes)
	{
		print("CreateIcon");
		Texture2D texture2D = new Texture2D(480, 360);
		texture2D.LoadImage(bytes);
		Icon.mainTexture = texture2D;
	}

	private void OnClick()
	{
		Application.OpenURL("https://youtu.be/" + id);
	}
}
