using UnityEngine;

public class UIMainStatus : MonoBehaviour
{
	public UILabel Label;

	private int TimerID;

	private static UIMainStatus instance;

	private void Start()
	{
		instance = this;
		PhotonEvent.AddListener(PhotonEventTag.ShowMainStatus, PhotonShow);
	}

	public static void Add(string text)
	{
		Add(text, false, 2f, string.Empty);
	}

	public static void Add(string text, bool local)
	{
		Add(text, local, 2f, string.Empty);
	}

	public static void Add(string text, bool local, float duration)
	{
		Add(text, local, duration, string.Empty);
	}

	public static void Add(string text, bool local, float duration, string localize)
	{
		if (local)
		{
			Show(text, duration);
			return;
		}
		PhotonEvent.RPC(PhotonEventTag.ShowMainStatus, PhotonTargets.All, text, duration, localize);
	}

	private void PhotonShow(PhotonEventData data)
	{
		string text = (string)data.parameters[0];
		float duration = (float)data.parameters[1];
		string text2 = (string)data.parameters[2];
		if (string.IsNullOrEmpty(text2))
		{
			Show(text, duration);
			return;
		}
		text2 = Localization.Get(text2);
		text = text.Replace("[@]", text2);
		Show(text, duration);
	}

	public static void Show(string text)
	{
		Show(text, 5f);
	}

	public static void Show(string text, float duration)
	{
		instance.Label.text = text;
		TimerManager.Cancel(instance.TimerID);
		instance.TimerID = TimerManager.In(duration, instance.Clear);
	}

	private void Clear()
	{
		instance.Label.text = string.Empty;
	}
}
