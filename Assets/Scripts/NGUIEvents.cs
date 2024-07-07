using UnityEngine;

public class NGUIEvents : MonoBehaviour
{
	public struct EventData
	{
		public GameObject go;

		public NGUIBehaviour script;

		public EventData(GameObject g, NGUIBehaviour s)
		{
			go = g;
			script = s;
		}
	}

	private BetterList<EventData> list = new BetterList<EventData>();

	private static NGUIEvents instance;

	private void Awake()
	{
		instance = this;
	}

	public static void Init()
	{
		if (Application.isPlaying && !(instance != null))
		{
			GameObject gameObject = new GameObject("NGUIEvents");
			gameObject.AddComponent<NGUIEvents>();
			DontDestroyOnLoad(gameObject);
		}
	}

	public static void Add(GameObject go, NGUIBehaviour c)
	{
		if (Application.isPlaying)
		{
			Init();
			instance.list.Add(new EventData(go, c));
		}
	}

	public static void Dispatch(GameObject go, string func, object obj)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Init();
		for (int i = 0; i < instance.list.size; i++)
		{
			if (instance.list[i].go == go && instance.list[i].script != null)
			{
				switch (func)
				{
				case "OnClick":
					instance.list[i].script.OnClick();
					break;
				case "OnSubmit":
					instance.list[i].script.OnSubmit();
					break;
				case "OnDoubleClick":
					instance.list[i].script.OnDoubleClick();
					break;
				case "OnHover":
					instance.list[i].script.OnHover((bool)obj);
					break;
				case "OnPress":
					instance.list[i].script.OnPress((bool)obj);
					break;
				case "OnSelect":
					instance.list[i].script.OnSelect((bool)obj);
					break;
				case "OnScroll":
					instance.list[i].script.OnScroll((float)obj);
					break;
				case "OnDragStart":
					instance.list[i].script.OnDragStart();
					break;
				case "OnDrag":
					instance.list[i].script.OnDrag((Vector2)obj);
					break;
				case "OnDragOver":
					instance.list[i].script.OnDragOver();
					break;
				case "OnDragOut":
					instance.list[i].script.OnDragOut();
					break;
				case "OnDragEnd":
					instance.list[i].script.OnDragEnd();
					break;
				case "OnDrop":
					instance.list[i].script.OnDrop(go);
					break;
				case "OnKey":
					instance.list[i].script.OnKey((KeyCode)(int)obj);
					break;
				case "OnTooltip":
					instance.list[i].script.OnTooltip((bool)obj);
					break;
				case "OnNavigate":
					instance.list[i].script.OnNavigate((KeyCode)(int)obj);
					break;
				case "OnPan":
					instance.list[i].script.OnPan((Vector2)obj);
					break;
				case "OnOnLongPress":
					instance.list[i].script.OnOnLongPress();
					break;
				}
			}
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		list.Clear();
	}
}
