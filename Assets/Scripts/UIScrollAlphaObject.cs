using UnityEngine;

public class UIScrollAlphaObject : MonoBehaviour
{
	private UIWidget widget;

	private Transform mTrans;

	private Transform mParent;

	private Vector3 pos;

	private Vector3 scale;

	private Vector3 position;

	public float cellWidth = 160f;

	public Vector3 downPosition = new Vector3(0f, 20f, 0f);

	public float downAlpha = 0.5f;

	public float downScale = 0.5f;

	private void Start()
	{
		mTrans = transform;
		scale = mTrans.localScale;
		position = mTrans.localPosition;
		mParent = mTrans.parent;
		widget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		pos = mTrans.localPosition + mParent.localPosition;
		float num = Mathf.Clamp(Mathf.Abs(pos.x), 0f, cellWidth);
		mTrans.localScale = (cellWidth - num * downScale) / cellWidth * scale;
		mTrans.localPosition = num * downPosition / 100f + position;
		widget.alpha = 1f - downAlpha * num / 100f;
	}
}
