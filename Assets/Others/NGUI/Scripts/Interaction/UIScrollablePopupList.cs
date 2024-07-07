using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Scrollable Popup List")]
[ExecuteInEditMode]
public class UIScrollablePopupList : NGUIBehaviour
{
	public enum Position
	{
		Auto,
		Above,
		Below
	}

	public delegate void LegacyEvent(string val);

	private const float animSpeed = 0.15f;

	public static UIScrollablePopupList current;

	public UIAtlas atlas;

	public UIAtlas scrollbarAtlas;

	public string scrollbarSpriteName;

	public string scrollbarForegroundName;

	public UIFont bitmapFont;

	public Font trueTypeFont;

	public int fontSize = 16;

	public FontStyle fontStyle;

	public string backgroundSprite;

	public string highlightSprite;

	public Position position;

	public List<string> items = new List<string>();

	public Vector2 padding = new Vector3(4f, 4f);

	public Color textColor = Color.white;

	public Color backgroundColor = Color.white;

	public Color highlightColor = new Color(0.88235295f, 40f / 51f, 0.5882353f, 1f);

	public Color scrollbarBgDefColour;

	public Color scrollbarBgHovColour;

	public Color scrollbarBgPrsColour;

	public Color scrollbarFgDefColour;

	public Color scrollbarFgHovColour;

	public Color scrollbarFgPrsColour;

	public bool isAnimated = true;

	public bool isLocalized;

	public int maxHeight = 100;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	[HideInInspector]
	[SerializeField]
	private string mSelectedItem;

	private UIPanel mPanel;

	private GameObject mChild;

	private UISprite mBackground;

	private UISprite mHighlight;

	private UILabel mHighlightedLabel;

	private List<UILabel> mLabelList = new List<UILabel>();

	private float mBgBorder;

	private UIPanel mClippingPanel;

	private UISprite scrollbarSprite;

	private UISprite scrForegroundSprite;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[SerializeField]
	[HideInInspector]
	private string functionName = "OnSelectionChange";

	[SerializeField]
	[HideInInspector]
	private float textScale;

	[HideInInspector]
	[SerializeField]
	private UIFont font;

	[SerializeField]
	[HideInInspector]
	private UILabel textLabel;

	private LegacyEvent mLegacyEvent;

	private bool mUseDynamicFont;

	public UnityEngine.Object ambigiousFont
	{
		get
		{
			if (trueTypeFont != null)
			{
				return trueTypeFont;
			}
			if (bitmapFont != null)
			{
				return bitmapFont;
			}
			return font;
		}
		set
		{
			if (value is Font)
			{
				trueTypeFont = value as Font;
				bitmapFont = null;
				font = null;
			}
			else if (value is UIFont)
			{
				bitmapFont = value as UIFont;
				trueTypeFont = null;
				font = null;
			}
		}
	}

	[Obsolete("Use EventDelegate.Add(popup.onChange, YourCallback) instead, and UIPopupList.current.value to determine the state")]
	public LegacyEvent onSelectionChange
	{
		get
		{
			return mLegacyEvent;
		}
		set
		{
			mLegacyEvent = value;
		}
	}

	public bool isOpen
	{
		get
		{
			return mChild != null;
		}
	}

	public string value
	{
		get
		{
			return mSelectedItem;
		}
		set
		{
			mSelectedItem = value;
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif
			if (mSelectedItem != null && mSelectedItem != null)
			{
				TriggerCallbacks();
			}
		}
	}

	[Obsolete("Use 'value' instead")]
	public string selection
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	private bool handleEvents
	{
		get
		{
			UIKeyNavigation component = GetComponent<UIKeyNavigation>();
			return component == null || !component.enabled;
		}
		set
		{
			UIKeyNavigation component = GetComponent<UIKeyNavigation>();
			if (component != null)
			{
				component.enabled = !value;
			}
		}
	}

	private bool isValid
	{
		get
		{
			return bitmapFont != null || trueTypeFont != null;
		}
	}

	private int activeFontSize
	{
		get
		{
			return (!(trueTypeFont != null) && !(bitmapFont == null)) ? bitmapFont.defaultSize : fontSize;
		}
	}

	private float activeFontScale
	{
		get
		{
			return (!(trueTypeFont != null) && !(bitmapFont == null)) ? (fontSize / bitmapFont.defaultSize) : 1f;
		}
	}

	protected void TriggerCallbacks()
	{
		if (current != this)
		{
			UIScrollablePopupList uIScrollablePopupList = current;
			current = this;
			if (mLegacyEvent != null)
			{
				mLegacyEvent(mSelectedItem);
			}
			if (EventDelegate.IsValid(onChange))
			{
				EventDelegate.Execute(onChange);
			}
			else if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
			{
				eventReceiver.SendMessage(functionName, mSelectedItem, SendMessageOptions.DontRequireReceiver);
			}
			current = uIScrollablePopupList;
		}
	}

	private void OnEnable()
	{
		if (EventDelegate.IsValid(onChange))
		{
			eventReceiver = null;
			functionName = null;
		}
		if (font != null)
		{
			if (font.isDynamic)
			{
				trueTypeFont = font.dynamicFont;
				fontStyle = font.dynamicFontStyle;
				mUseDynamicFont = true;
			}
			else if (bitmapFont == null)
			{
				bitmapFont = font;
				mUseDynamicFont = false;
			}
			font = null;
		}
		if (textScale != 0f)
		{
			fontSize = ((!(bitmapFont != null)) ? 16 : Mathf.RoundToInt(bitmapFont.defaultSize * textScale));
			textScale = 0f;
		}
		if (trueTypeFont == null && bitmapFont != null && bitmapFont.isDynamic)
		{
			trueTypeFont = bitmapFont.dynamicFont;
			bitmapFont = null;
		}
	}

	private void OnValidate()
	{
		Font font = trueTypeFont;
		UIFont uIFont = bitmapFont;
		bitmapFont = null;
		trueTypeFont = null;
		if (font != null && (uIFont == null || !mUseDynamicFont))
		{
			bitmapFont = null;
			trueTypeFont = font;
			mUseDynamicFont = true;
		}
		else if (uIFont != null)
		{
			if (uIFont.isDynamic)
			{
				trueTypeFont = uIFont.dynamicFont;
				fontStyle = uIFont.dynamicFontStyle;
				fontSize = uIFont.defaultSize;
				mUseDynamicFont = true;
			}
			else
			{
				bitmapFont = uIFont;
				mUseDynamicFont = false;
			}
		}
		else
		{
			trueTypeFont = font;
			mUseDynamicFont = true;
		}
	}

	private void Start()
	{
		if (textLabel != null)
		{
			EventDelegate.Add(onChange, textLabel.SetCurrentSelection);
			textLabel = null;
#if UNITY_EDITOR
			NGUITools.SetDirty(this);
#endif
		}
		if (Application.isPlaying)
		{
			if (string.IsNullOrEmpty(mSelectedItem))
			{
				if (items.Count > 0)
				{
					value = items[0];
				}
			}
			else
			{
				string text = mSelectedItem;
				mSelectedItem = null;
				value = text;
			}
		}
		NGUIEvents.Add(gameObject, this);
	}

	private void OnLocalize()
	{
		if (isLocalized)
		{
			TriggerCallbacks();
		}
	}

	private void Highlight(UILabel lbl, bool instant)
	{
		if (!(mHighlight != null))
		{
			return;
		}
		TweenPosition component = lbl.GetComponent<TweenPosition>();
		if (component != null && component.enabled)
		{
			return;
		}
		mHighlightedLabel = lbl;
		UISpriteData atlasSprite = mHighlight.GetAtlasSprite();
		if (atlasSprite != null)
		{
			float pixelSize = atlas.pixelSize;
			float num = atlasSprite.borderLeft * pixelSize;
			float y = atlasSprite.borderTop * pixelSize;
			Vector3 vector = lbl.cachedTransform.localPosition + new Vector3(0f - num, y, 1f);
			if (instant || !isAnimated)
			{
				mHighlight.cachedTransform.localPosition = vector;
			}
			else
			{
				TweenPosition.Begin(mHighlight.gameObject, 0.1f, vector).method = UITweener.Method.EaseOut;
			}
		}
	}

	private void OnItemHover(GameObject go, bool isOver)
	{
		if (isOver)
		{
			UILabel component = go.GetComponent<UILabel>();
			Highlight(component, false);
		}
	}

	private void Select(UILabel lbl, bool instant)
	{
		Highlight(lbl, instant);
		UIEventListener component = lbl.gameObject.GetComponent<UIEventListener>();
		value = component.parameter as string;
		UIPlaySound[] components = GetComponents<UIPlaySound>();
		int i = 0;
		for (int num = components.Length; i < num; i++)
		{
			UIPlaySound uIPlaySound = components[i];
			if (uIPlaySound.trigger == UIPlaySound.Trigger.OnClick)
			{
				NGUITools.PlaySound(uIPlaySound.audioClip, uIPlaySound.volume, 1f);
			}
		}
		Close();
	}

	private void OnItemPress(GameObject go, bool isPressed)
	{
		if (isPressed)
		{
			Select(go.GetComponent<UILabel>(), true);
		}
	}

	private void OnItemClick(GameObject go)
	{
		Select(go.GetComponent<UILabel>(), true);
	}

	public override void OnKey(KeyCode key)
	{
		if (!enabled || !NGUITools.GetActive(gameObject) || !handleEvents)
		{
			return;
		}
		int num = mLabelList.IndexOf(mHighlightedLabel);
		if (num == -1)
		{
			num = 0;
		}
		switch (key)
		{
		case KeyCode.UpArrow:
			if (num > 0)
			{
				Select(mLabelList[--num], false);
			}
			break;
		case KeyCode.DownArrow:
			if (num + 1 < mLabelList.Count)
			{
				Select(mLabelList[++num], false);
			}
			break;
		case KeyCode.Escape:
			OnSelect(false);
			break;
		}
	}

	public override void OnSelect(bool isSelected)
	{
		if (!isSelected)
		{
			Close();
		}
	}

	public void Close()
	{
		if (!(mChild != null) || (UICamera.hoveredObject && (UICamera.hoveredObject == scrollbarSprite.cachedGameObject || UICamera.hoveredObject == scrForegroundSprite.cachedGameObject || UICamera.hoveredObject == mBackground.cachedGameObject)))
		{
			return;
		}
		mLabelList.Clear();
		handleEvents = false;
		if (isAnimated)
		{
			UIWidget[] componentsInChildren = mChild.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				UIWidget uIWidget = componentsInChildren[i];
				Color color = uIWidget.color;
				color.a = 0f;
				TweenColor.Begin(uIWidget.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
			}
			Collider[] componentsInChildren2 = mChild.GetComponentsInChildren<Collider>();
			int j = 0;
			for (int num2 = componentsInChildren2.Length; j < num2; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			Destroy(mChild, 0.15f);
		}
		else
		{
			Destroy(mChild);
		}
		mBackground = null;
		mHighlight = null;
		mChild = null;
		mClippingPanel = null;
		scrollbarSprite = null;
		scrForegroundSprite = null;
	}

	private void AnimateColor(UIWidget widget)
	{
		Color color = widget.color;
		widget.color = new Color(color.r, color.g, color.b, 0f);
		TweenColor.Begin(widget.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
	}

	private void AnimatePosition(UIWidget widget, bool placeAbove, float bottom)
	{
		Vector3 localPosition = widget.cachedTransform.localPosition;
		Vector3 localPosition2 = ((!placeAbove) ? new Vector3(localPosition.x, 0f, localPosition.z) : new Vector3(localPosition.x, bottom, localPosition.z));
		widget.cachedTransform.localPosition = localPosition2;
		GameObject go = widget.gameObject;
		TweenPosition.Begin(go, 0.15f, localPosition).method = UITweener.Method.EaseOut;
	}

	private void AnimateScale(UIWidget widget, bool placeAbove, float bottom)
	{
		GameObject go = widget.gameObject;
		Transform cachedTransform = widget.cachedTransform;
		float num = activeFontSize * activeFontScale + mBgBorder * 2f;
		cachedTransform.localScale = new Vector3(1f, num / widget.height, 1f);
		TweenScale.Begin(go, 0.15f, Vector3.one).method = UITweener.Method.EaseOut;
		if (placeAbove)
		{
			Vector3 localPosition = cachedTransform.localPosition;
			cachedTransform.localPosition = new Vector3(localPosition.x, localPosition.y - widget.height + num, localPosition.z);
			TweenPosition.Begin(go, 0.15f, localPosition).method = UITweener.Method.EaseOut;
		}
	}

	private void Animate(UIWidget widget, bool placeAbove, float bottom)
	{
		AnimateColor(widget);
		AnimatePosition(widget, placeAbove, bottom);
	}

	public override void OnClick()
	{
		if (!enabled || !NGUITools.GetActive(base.gameObject) || !(mChild == null) || !(atlas != null) || !isValid || items.Count <= 0)
		{
			return;
		}
		mLabelList.Clear();
		if (mPanel == null)
		{
			mPanel = UIPanel.Find(base.transform);
			if (mPanel == null)
			{
				return;
			}
		}
		handleEvents = true;
		Transform transform = base.transform;
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(transform.parent, transform);
		mChild = new GameObject("Drop-down List");
		mChild.layer = base.gameObject.layer;
		Transform transform2 = mChild.transform;
		transform2.parent = transform.parent;
		transform2.localPosition = bounds.min;
		transform2.localRotation = Quaternion.identity;
		transform2.localScale = Vector3.one;
		mBackground = NGUITools.AddSprite(mChild, atlas, backgroundSprite);
		mBackground.pivot = UIWidget.Pivot.TopLeft;
		mBackground.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
		mBackground.color = backgroundColor;
		mBackground.gameObject.name = "SpriteBackground";
		NGUITools.AddWidgetCollider(mBackground.cachedGameObject);
		UIDragScrollView uIDragScrollView = mBackground.gameObject.AddComponent<UIDragScrollView>();
		GameObject gameObject = new GameObject("Panel");
		gameObject.layer = base.gameObject.layer;
		Transform transform3 = gameObject.transform;
		transform3.parent = mChild.transform;
		transform3.localPosition = Vector3.zero;
		transform3.localRotation = Quaternion.identity;
		transform3.localScale = Vector3.one;
		mClippingPanel = gameObject.AddComponent<UIPanel>();
		mClippingPanel.clipping = UIDrawCall.Clipping.SoftClip;
		mClippingPanel.leftAnchor.target = mBackground.transform;
		mClippingPanel.rightAnchor.target = mBackground.transform;
		mClippingPanel.topAnchor.target = mBackground.transform;
		mClippingPanel.bottomAnchor.target = mBackground.transform;
		mClippingPanel.ResetAnchors();
		mClippingPanel.UpdateAnchors();
		mClippingPanel.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
		UIScrollView uIScrollView = gameObject.AddComponent<UIScrollView>();
		uIScrollView.contentPivot = UIWidget.Pivot.TopLeft;
		uIScrollView.movement = UIScrollView.Movement.Vertical;
		uIScrollView.scrollWheelFactor = 0.25f;
		uIScrollView.disableDragIfFits = true;
		uIScrollView.dragEffect = UIScrollView.DragEffect.None;
		uIScrollView.ResetPosition();
		uIScrollView.UpdatePosition();
		uIScrollView.RestrictWithinBounds(true);
		uIDragScrollView.scrollView = uIScrollView;
		Vector4 border = mBackground.border;
		mBgBorder = border.y;
		mBackground.cachedTransform.localPosition = new Vector3(0f, border.y, 0f);
		mHighlight = NGUITools.AddSprite(gameObject, atlas, highlightSprite);
		mHighlight.pivot = UIWidget.Pivot.TopLeft;
		mHighlight.color = highlightColor;
		mHighlight.gameObject.name = "Highlighter";
		UISpriteData atlasSprite = mHighlight.GetAtlasSprite();
		if (atlasSprite == null)
		{
			return;
		}
		float num = atlasSprite.borderTop;
		float num2 = activeFontSize;
		float num3 = activeFontScale;
		float num4 = num2 * num3;
		float a = 0f;
		float num5 = 0f - padding.y;
		int num6 = ((!(bitmapFont != null)) ? fontSize : bitmapFont.defaultSize);
		List<UILabel> list = new List<UILabel>();
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			string text = items[i];
			UILabel uILabel = NGUITools.AddWidget<UILabel>(gameObject);
			uILabel.pivot = UIWidget.Pivot.TopLeft;
			uILabel.bitmapFont = bitmapFont;
			uILabel.trueTypeFont = trueTypeFont;
			uILabel.fontSize = num6;
			uILabel.fontStyle = fontStyle;
			uILabel.text = ((!isLocalized) ? text : Localization.Get(text));
			uILabel.color = textColor;
			uILabel.cachedTransform.localPosition = new Vector3(border.x + padding.x, num5, -1f);
			uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			uILabel.MakePixelPerfect();
			uILabel.gameObject.AddComponent<UIDragScrollView>();
			if (num3 != 1f)
			{
				uILabel.cachedTransform.localScale = Vector3.one * num3;
			}
			list.Add(uILabel);
			num5 -= num4;
			num5 -= padding.y;
			a = Mathf.Max(a, uILabel.printedSize.x);
			UIEventListener uIEventListener = UIEventListener.Get(uILabel.gameObject);
			uIEventListener.onHover = OnItemHover;
			uIEventListener.onClick = OnItemClick;
			uIEventListener.parameter = text;
			if (mSelectedItem == text || (i == 0 && string.IsNullOrEmpty(mSelectedItem)))
			{
				Highlight(uILabel, true);
			}
			mLabelList.Add(uILabel);
		}
		a = Mathf.Max(a, bounds.size.x * num3 - (border.x + padding.x) * 2f);
		float num7 = a / num3;
		Vector3 center = new Vector3(num7 * 0.5f, (0f - num2) * 0.5f, 0f);
		Vector3 size = new Vector3(num7, (num4 + padding.y) / num3, 1f);
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			UILabel uILabel2 = list[j];
			NGUITools.AddWidgetCollider(uILabel2.gameObject);
			BoxCollider component = uILabel2.GetComponent<BoxCollider>();
			center.z = component.center.z;
			component.center = center;
			component.size = size;
		}
		a += (border.x + padding.x) * 2f;
		num5 -= border.y;
		mBackground.width = Mathf.RoundToInt(a);
		int num8 = Mathf.RoundToInt(0f - num5 + border.y);
		if (maxHeight == 0)
		{
			maxHeight = num8;
		}
		mBackground.height = ((num8 <= maxHeight) ? num8 : maxHeight);
		uIScrollView.ResetPosition();
		uIScrollView.UpdatePosition();
		uIScrollView.RestrictWithinBounds(true, true, true);
		transform3.localPosition = Vector3.zero;
		float num9 = 2f * atlas.pixelSize;
		float f = a - (border.x + padding.x) * 2f + atlasSprite.borderLeft * num9;
		float f2 = num4 + num * num9;
		mHighlight.width = Mathf.RoundToInt(f) - 5;
		mHighlight.height = Mathf.RoundToInt(f2);
		bool flag = position == Position.Above;
		if (position == Position.Auto)
		{
			UICamera uICamera = UICamera.FindCameraForLayer(base.gameObject.layer);
			if (uICamera != null)
			{
				flag = uICamera.cachedCamera.WorldToViewportPoint(transform.position).y < 0.5f;
			}
		}
		if (isAnimated)
		{
			float bottom = num5 + num4;
			Animate(mHighlight, flag, bottom);
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				Animate(list[k], flag, bottom);
			}
			AnimateColor(mBackground);
			AnimateScale(mBackground, flag, bottom);
		}
		if (flag)
		{
			transform2.localPosition = new Vector3(bounds.min.x, bounds.min.y + mBackground.height + transform.GetComponent<UISprite>().height - border.y, bounds.min.z);
		}
		scrollbarSprite = NGUITools.AddSprite(mBackground.gameObject, scrollbarAtlas, scrollbarSpriteName);
		scrollbarSprite.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
		scrollbarSprite.color = scrollbarBgDefColour;
		scrollbarSprite.gameObject.name = "Scrollbar";
		UIButtonColor uIButtonColor = scrollbarSprite.gameObject.AddComponent<UIButtonColor>();
		uIButtonColor.defaultColor = scrollbarSprite.color;
		uIButtonColor.hover = scrollbarBgHovColour;
		uIButtonColor.pressed = scrollbarBgPrsColour;
		NGUITools.AddWidgetCollider(scrollbarSprite.gameObject);
		scrollbarSprite.leftAnchor.target = mBackground.transform;
		scrollbarSprite.leftAnchor.relative = 1f;
		scrollbarSprite.leftAnchor.absolute = -11;
		scrollbarSprite.rightAnchor.target = mBackground.transform;
		scrollbarSprite.rightAnchor.relative = 1f;
		scrollbarSprite.rightAnchor.absolute = -1;
		scrollbarSprite.topAnchor.target = mBackground.transform;
		scrollbarSprite.topAnchor.relative = 1f;
		scrollbarSprite.topAnchor.absolute = 0;
		scrollbarSprite.bottomAnchor.target = mBackground.transform;
		scrollbarSprite.bottomAnchor.relative = 0f;
		scrollbarSprite.bottomAnchor.absolute = 0;
		scrollbarSprite.ResetAnchors();
		scrollbarSprite.UpdateAnchors();
		scrForegroundSprite = NGUITools.AddSprite(scrollbarSprite.gameObject, scrollbarAtlas, scrollbarForegroundName);
		scrForegroundSprite.depth = NGUITools.CalculateNextDepth(mPanel.gameObject);
		scrForegroundSprite.color = scrollbarFgDefColour;
		scrForegroundSprite.gameObject.name = "Foreground";
		UIButtonColor uIButtonColor2 = scrForegroundSprite.gameObject.AddComponent<UIButtonColor>();
		uIButtonColor2.defaultColor = scrForegroundSprite.color;
		uIButtonColor2.hover = scrollbarFgHovColour;
		uIButtonColor2.pressed = scrollbarFgPrsColour;
		NGUITools.AddWidgetCollider(scrForegroundSprite.gameObject);
		scrForegroundSprite.leftAnchor.target = scrollbarSprite.transform;
		scrForegroundSprite.rightAnchor.target = scrollbarSprite.transform;
		scrForegroundSprite.topAnchor.target = scrollbarSprite.transform;
		scrForegroundSprite.bottomAnchor.target = scrollbarSprite.transform;
		scrForegroundSprite.ResetAnchors();
		scrForegroundSprite.UpdateAnchors();
		UIScrollBar uIScrollBar = scrollbarSprite.gameObject.AddComponent<UIScrollBar>();
		uIScrollBar.fillDirection = UIProgressBar.FillDirection.TopToBottom;
		uIScrollBar.backgroundWidget = scrollbarSprite;
		uIScrollBar.foregroundWidget = scrForegroundSprite;
		uIScrollView.verticalScrollBar = uIScrollBar;
		uIScrollView.ResetPosition();
		uIScrollBar.value = 0f;
	}
}
