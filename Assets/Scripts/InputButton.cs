using UnityEngine;

public class InputButton : NGUIBehaviour
{
	public string button;

	public float fadeAlpha = 0.5f;

	public float fadeDuration = 0.1f;

	public bool useSettings = true;

	private bool mPressed;

	private GameObject mGameObject;

	private float alphaButton;

	private void Start()
	{
		mGameObject = gameObject;
		if (useSettings)
		{
			EventManager.AddListener("SaveButton", OnSetPosition);
			OnSetPosition();
		}
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
		NGUIEvents.Add(gameObject, this);
		UISprite sprite = mGameObject.GetComponent<UISprite>();
		if (sprite.name != "Use")
		{
			sprite.cachedTransform.localPosition = new Vector3(0, 1000, 0);
		}
		UIAnchor anchor = mGameObject.GetComponent<UIAnchor>();
		Destroy(anchor);
	}

	private void OnEnable()
	{
		if (fadeAlpha != 0f && mGameObject != null)
		{
			TweenAlpha.Begin(mGameObject, fadeDuration, alphaButton);
		}
	}

	private void OnDisable()
	{
		if (mPressed)
		{
			InputManager.SetButtonUp(button);
		}
	}

	public override void OnPress(bool pressed)
	{
		mPressed = pressed;
		if (pressed)
		{
			if (fadeAlpha != 0f)
			{
				TweenAlpha.Begin(mGameObject, fadeDuration, fadeAlpha * alphaButton);
			}
			InputManager.SetButtonDown(button);
		}
		else
		{
			if (fadeAlpha != 0f)
			{
				TweenAlpha.Begin(mGameObject, fadeDuration, alphaButton);
			}
			InputManager.SetButtonUp(button);
		}
	}

	private void OnSetPosition()
	{
		TimerManager.In(0.1f, delegate
		{
			if (PlayerPrefs.HasKey("ButtonX" + button))
			{
				UISprite component = mGameObject.GetComponent<UISprite>();
				component.cachedTransform.localPosition = Utils.GetVector3(PlayerPrefs.GetString("ButtonX" + button));
				int height = (component.width = PlayerPrefs.GetInt("ButtonSizeX" + button));
				component.height = height;
				component.UpdateTransform();
				UISprite sprite = mGameObject.GetComponent<UISprite>();
				if (sprite.name != "Use")
				{
					sprite.cachedTransform.localPosition = new Vector3(0, 1000, 0);
				}
				UIAnchor anchor = mGameObject.GetComponent<UIAnchor>();
				Destroy(anchor);
			}
		});
	}

	private void UpdateSettings()
	{
		alphaButton = Settings.ButtonAlpha;
		if (!Settings.HUD)
		{
			alphaButton = 1f;
		}
		mGameObject.GetComponent<UISprite>().alpha = alphaButton;
		UISprite sprite = mGameObject.GetComponent<UISprite>();
		if (sprite.name != "Use")
		{
			sprite.cachedTransform.localPosition = new Vector3(0, 1000, 0);
		}
		UIAnchor anchor = mGameObject.GetComponent<UIAnchor>();
		Destroy(anchor);
	}
}
