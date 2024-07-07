using UnityEngine;

public class UIFade : MonoBehaviour
{
	public UISprite FadeSprite;

	public TweenAlpha Tween;

	private static UIFade instance;

	private void Start()
	{
		instance = this;
	}

	public static void In(float delay, float duration)
	{
		instance.FadeSprite.cachedGameObject.SetActive(true);
		instance.Tween.delay = delay;
		instance.Tween.duration = duration;
		instance.Tween.PlayForward();
	}

	public static void Clear()
	{
		instance.FadeSprite.cachedGameObject.SetActive(false);
		instance.FadeSprite.alpha = 0f;
	}
}
