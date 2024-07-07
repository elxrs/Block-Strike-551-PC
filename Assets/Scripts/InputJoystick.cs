using UnityEngine;

public class InputJoystick : MonoBehaviour
{
	public Camera uiCamera;

	public UISprite stick;

	public UISprite background;

	public float distance = 0.3f;

	private float positionMultiplier;

	private bool move;

	private int id = -1;

	private bool activeJoystick = true;

	private Rect touchZone;

	private Vector2 value;

	private bool Lefty;

	private void Start()
	{
		positionMultiplier = 1f / distance;
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
		Hide();
	}

	private void OnDisable()
	{
		Hide();
		move = false;
		id = -1;
		UpdateValue(Vector2.zero);
	}

	private void Update()
	{
		if (!activeJoystick)
		{
			return;
		}
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (touch.phase == TouchPhase.Began && touchZone.Contains(touch.position))
			{
				id = touch.fingerId;
				if (!move)
				{
					move = true;
				}
				Vector2 position = touch.position;
				position.x = Mathf.Clamp01(position.x / Screen.width);
				position.y = Mathf.Clamp01(position.y / Screen.height);
				background.cachedTransform.position = uiCamera.ViewportToWorldPoint(position);
				stick.cachedTransform.position = uiCamera.ViewportToWorldPoint(position);
				Show();
			}
			if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && id == touch.fingerId)
			{
				Vector3 position2 = touch.position;
				position2.x = Mathf.Clamp01(position2.x / Screen.width);
				position2.y = Mathf.Clamp01(position2.y / Screen.height);
				stick.cachedTransform.position = uiCamera.ViewportToWorldPoint(position2);
				stick.cachedTransform.position = Clamp(background.cachedTransform.position, stick.cachedTransform.position);
				UpdateValue((stick.cachedTransform.position - background.cachedTransform.position) * positionMultiplier);
			}
			if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && id == touch.fingerId)
			{
				move = false;
				id = -1;
				Hide();
				UpdateValue(Vector2.zero);
			}
		}
	}

	private Vector3 Clamp(Vector3 p, Vector3 y)
	{
		if (Vector3.Distance(p, y) > distance)
		{
			Vector3 vector = y - p;
			vector.Normalize();
			return vector * distance + p;
		}
		return y;
	}

	private void Show()
	{
		stick.cachedGameObject.SetActive(true);
		background.cachedGameObject.SetActive(true);
	}

	private void Hide()
	{
		try
		{
			stick.cachedGameObject.SetActive(false);
			background.cachedGameObject.SetActive(false);
		}
		catch
		{
		}
	}

	private void UpdateValue(Vector2 v)
	{
		value = v;
		InputManager.SetAxis("Horizontal", value.x);
		InputManager.SetAxis("Vertical", value.y);
	}

	public void SetActiveJoystick(bool active)
	{
		activeJoystick = active;
		if (!activeJoystick)
		{
			OnDisable();
		}
	}

	private void UpdateSettings()
	{
		Lefty = Settings.Lefty;
		if (Lefty)
		{
			touchZone = new Rect(Screen.width / 2, 0f, Screen.width / 2.5f, Screen.height / 2);
		}
		else
		{
			touchZone = new Rect(0f, 0f, Screen.width / 2.5f, Screen.height / 2);
		}
		float buttonAlpha = Settings.ButtonAlpha;
		stick.alpha = buttonAlpha;
		background.alpha = buttonAlpha;
	}
}
