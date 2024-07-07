using System.Collections.Generic;
using UnityEngine;

public class UIToast : MonoBehaviour
{
	public UILabel label;

	public UISprite background;

	private int Timer;

	private List<string> textList = new List<string>();

	private List<float> timeList = new List<float>();

	private bool isShow;

	private static UIToast instance;

	private void Awake()
	{
		instance = this;
	}

	public static void Show(string text)
	{
		Show(text, 2f, false);
	}

	public static void Show(string text, float duration)
	{
		Show(text, duration, false);
	}

	public static void Show(string text, float duration, bool queue)
	{
		if (queue && instance.isShow)
		{
			instance.textList.Add(text);
			instance.timeList.Add(duration);
			return;
		}
		TimerManager.Cancel(instance.Timer);
		instance.label.alpha = 0f;
		TweenAlpha.Begin(instance.label.cachedGameObject, 0.2f, 1f);
		instance.label.text = text;
		instance.isShow = true;
		instance.background.UpdateAnchors();
		instance.Timer = TimerManager.In(duration, delegate
		{
			if (instance.textList.Count != 0)
			{
				Show(instance.textList[0], instance.timeList[0]);
				instance.textList.RemoveAt(0);
				instance.timeList.RemoveAt(0);
			}
			else
			{
				instance.isShow = false;
				TweenAlpha.Begin(instance.label.cachedGameObject, 0.2f, 0f);
			}
		});
	}
}
