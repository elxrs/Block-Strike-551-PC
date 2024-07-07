using UnityEngine;

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : NGUIBehaviour
{
	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	public object parameter;

	public VoidDelegate onSubmit;

	public VoidDelegate onClick;

	public VoidDelegate onDoubleClick;

	public BoolDelegate onHover;

	public BoolDelegate onPress;

	public BoolDelegate onSelect;

	public FloatDelegate onScroll;

	public VoidDelegate onDragStart;

	public VectorDelegate onDrag;

	public VoidDelegate onDragOver;

	public VoidDelegate onDragOut;

	public VoidDelegate onDragEnd;

	public ObjectDelegate onDrop;

	public KeyCodeDelegate onKey;

	public BoolDelegate onTooltip;

	private bool isColliderEnabled
	{
		get
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				return component.enabled;
			}
			Collider2D component2 = GetComponent<Collider2D>();
			return component2 != null && component2.enabled;
		}
	}

	private void Start()
	{
		NGUIEvents.Add(gameObject, this);
	}

	public override void OnSubmit()
	{
		if (isColliderEnabled && onSubmit != null)
		{
			onSubmit(gameObject);
		}
	}

	public override void OnClick()
	{
		if (isColliderEnabled && onClick != null)
		{
			onClick(gameObject);
		}
	}

	public override void OnDoubleClick()
	{
		if (isColliderEnabled && onDoubleClick != null)
		{
			onDoubleClick(gameObject);
		}
	}

	public override void OnHover(bool isOver)
	{
		if (isColliderEnabled && onHover != null)
		{
			onHover(gameObject, isOver);
		}
	}

	public override void OnPress(bool isPressed)
	{
		if (isColliderEnabled && onPress != null)
		{
			onPress(gameObject, isPressed);
		}
	}

	public override void OnSelect(bool selected)
	{
		if (isColliderEnabled && onSelect != null)
		{
			onSelect(gameObject, selected);
		}
	}

	public override void OnScroll(float delta)
	{
		if (isColliderEnabled && onScroll != null)
		{
			onScroll(gameObject, delta);
		}
	}

	public override void OnDragStart()
	{
		if (onDragStart != null)
		{
			onDragStart(gameObject);
		}
	}

	public override void OnDrag(Vector2 delta)
	{
		if (onDrag != null)
		{
			onDrag(gameObject, delta);
		}
	}

	public override void OnDragOver()
	{
		if (isColliderEnabled && onDragOver != null)
		{
			onDragOver(gameObject);
		}
	}

	public override void OnDragOut()
	{
		if (isColliderEnabled && onDragOut != null)
		{
			onDragOut(gameObject);
		}
	}

	public override void OnDragEnd()
	{
		if (onDragEnd != null)
		{
			onDragEnd(gameObject);
		}
	}

	public override void OnDrop(GameObject go)
	{
		if (isColliderEnabled && onDrop != null)
		{
			onDrop(gameObject, go);
		}
	}

	public override void OnKey(KeyCode key)
	{
		if (isColliderEnabled && onKey != null)
		{
			onKey(gameObject, key);
		}
	}

	public override void OnTooltip(bool show)
	{
		if (isColliderEnabled && onTooltip != null)
		{
			onTooltip(gameObject, show);
		}
	}

	public static UIEventListener Get(GameObject go)
	{
		UIEventListener uIEventListener = go.GetComponent<UIEventListener>();
		if (uIEventListener == null)
		{
			uIEventListener = go.AddComponent<UIEventListener>();
		}
		return uIEventListener;
	}
}
