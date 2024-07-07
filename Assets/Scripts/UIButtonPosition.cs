using UnityEngine;

public class UIButtonPosition : NGUIBehaviour
{
	public string Button;

	public Vector2 Size = new Vector2(25f, 80f);

	private Vector3 defaultPosition;

	private int defaultSize;

	private UISprite sprite;

	private void Start()
	{
		sprite = GetComponent<UISprite>();
		defaultPosition = sprite.transform.localPosition;
		defaultSize = sprite.width;
		EventManager.AddListener("DefaultButton", OnDefaultButton);
		EventManager.AddListener("SaveButton", OnSaveButton);
		TimerManager.In(0.1f, delegate
		{
			if (PlayerPrefs.HasKey("ButtonX" + Button))
			{
				sprite.cachedTransform.localPosition = Utils.GetVector3(PlayerPrefs.GetString("ButtonX" + Button));
				int @int = PlayerPrefs.GetInt("ButtonSizeX" + Button);
				sprite.width = @int;
				sprite.height = @int;
			}
		});
		NGUIEvents.Add(gameObject, this);
	}

	public override void OnDrag(Vector2 delta)
	{
		sprite.cachedTransform.localPosition += new Vector3(delta.x, delta.y, 0f);
	}

	public override void OnDoubleClick()
	{
		sprite.width += 5;
		sprite.height += 5;
		if (sprite.width >= Size.y)
		{
			sprite.width = (int)Size.x;
			sprite.height = (int)Size.x;
		}
	}

	private void OnDefaultButton()
	{
		sprite.transform.localPosition = defaultPosition;
		sprite.width = defaultSize;
		sprite.height = defaultSize;
	}

	private void OnSaveButton()
	{
		PlayerPrefs.SetString("ButtonX" + Button, sprite.cachedTransform.localPosition.ToString());
		PlayerPrefs.SetInt("ButtonSizeX" + Button, sprite.width);
	}
}
