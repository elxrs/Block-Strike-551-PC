using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag Scroll View")]
public class UIDragScrollView : NGUIBehaviour
{
	public UIScrollView scrollView;

	[HideInInspector]
	[SerializeField]
	private UIScrollView draggablePanel;

	private Transform mTrans;

	private UIScrollView mScroll;

	private bool mAutoFind;

	private bool mStarted;

	private void OnEnable()
	{
		mTrans = transform;
		if (scrollView == null && draggablePanel != null)
		{
			scrollView = draggablePanel;
			draggablePanel = null;
		}
		if (mStarted && (mAutoFind || mScroll == null))
		{
			FindScrollView();
		}
	}

	private void Start()
	{
		NGUIEvents.Add(gameObject, this);
		mStarted = true;
		FindScrollView();
	}

	private void FindScrollView()
	{
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(mTrans);
		if (scrollView == null || (mAutoFind && uIScrollView != scrollView))
		{
			scrollView = uIScrollView;
			mAutoFind = true;
		}
		else if (scrollView == uIScrollView)
		{
			mAutoFind = true;
		}
		mScroll = scrollView;
	}

	public override void OnPress(bool pressed)
	{
		if (mAutoFind && mScroll != scrollView)
		{
			mScroll = scrollView;
			mAutoFind = false;
		}
		if (scrollView && enabled && NGUITools.GetActive(gameObject))
		{
			scrollView.Press(pressed);
			if (!pressed && mAutoFind)
			{
				scrollView = NGUITools.FindInParents<UIScrollView>(mTrans);
				mScroll = scrollView;
			}
		}
	}

	public override void OnDrag(Vector2 delta)
	{
		if (scrollView && NGUITools.GetActive(this))
		{
			scrollView.Drag();
		}
	}

	public override void OnScroll(float delta)
	{
		if (scrollView && NGUITools.GetActive(this))
		{
			scrollView.Scroll(delta);
		}
	}

	public override void OnPan(Vector2 delta)
	{
		if (scrollView && NGUITools.GetActive(this))
		{
			scrollView.OnPan(delta);
		}
	}
}
