using UnityEngine;

public class InputTouchLook : MonoBehaviour
{
	private Rect touchZone = new Rect(50f, 0f, 50f, 100f);

	private bool move;

	private int id = -1;

	private float dpi;

	private Vector2 value;

	private bool Lefty;

	private Touch touch;

	private bool fixController;

	private Vector2 fixPos;

	private void Start()
	{
		dpi = Screen.dpi / 100f;
		if (dpi == 0f)
		{
			dpi = 1.6f;
		}
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
		fixController = PlayerPrefs.GetInt("fixtouchlook", 0) == 1;
	}

	private void OnEnable()
	{
		nConsole.AddCommand("fixtouchlook", nValueType.Bool, FixTouchLook);
	}

	private void OnDisable()
	{
		nConsole.Remove("fixtouchlook");
		UpdateValue(Vector2.zero);
		id = -1;
		move = false;
	}

	private void Update()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			touch = Input.GetTouch(i);
			if (touch.phase == TouchPhase.Began && touchZone.Contains(new Vector3(touch.position.x, Screen.height - touch.position.y, 0f)))
			{
				move = true;
				id = touch.fingerId;
				if (fixController)
				{
					fixPos = touch.position;
				}
			}
			if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && id == touch.fingerId && move)
			{
				if (fixController)
				{
					UpdateValue((touch.position - fixPos) / dpi);
					fixPos = touch.position;
				}
				else
				{
					UpdateValue(touch.deltaPosition / dpi);
				}
			}
			if ((touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) && id == touch.fingerId && move)
			{
				id = -1;
				move = false;
				UpdateValue(Vector2.zero);
				if (fixController)
				{
					fixPos = touch.position;
				}
			}
		}
	}

	private void UpdateValue(Vector2 v)
	{
		value = v;
		InputManager.SetAxis("Mouse X", value.x);
		InputManager.SetAxis("Mouse Y", value.y);
	}

	private void UpdateSettings()
	{
		Lefty = Settings.Lefty;
		if (Lefty)
		{
			touchZone = NewRect(new Rect(0f, 0f, 50f, 100f));
		}
		else
		{
			touchZone = NewRect(new Rect(50f, 0f, 50f, 100f));
		}
	}

	private Rect NewRect(float x, float y, float w, float h)
	{
		return NewRect(new Rect(x, y, w, h));
	}

	private Rect NewRect(Rect rect)
	{
		float left = Screen.width * rect.x / 100f;
		float top = Screen.height * rect.y / 100f;
		float width = Screen.width * rect.width / 100f;
		float height = Screen.height * rect.height / 100f;
		return new Rect(left, top, width, height);
	}

	private void FixTouchLook(string command, object data)
	{
		if (!(command != "fixtouchlook"))
		{
			fixController = (bool)data;
			PlayerPrefs.SetInt("fixtouchlook", fixController ? 1 : 0);
		}
	}
}
