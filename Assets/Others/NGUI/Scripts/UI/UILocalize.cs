using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Localize")]
[RequireComponent(typeof(UIWidget))]
public class UILocalize : MonoBehaviour
{
	public string key;

	public string addon;

	private UILabel lbl;

	private bool mStarted;

	public string value
	{
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (lbl == null)
				{
					lbl = GetComponent<UILabel>();
				}
				lbl.text = value;
#if UNITY_EDITOR
				if (!Application.isPlaying) NGUITools.SetDirty(lbl);
#endif
			}
		}
	}

	private void OnEnable()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		if (mStarted)
		{
			OnLocalize();
		}
	}

	private void Start()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		mStarted = true;
		OnLocalize();
	}

	private void OnLocalize()
	{
		if (string.IsNullOrEmpty(key))
		{
			UILabel component = GetComponent<UILabel>();
			if (component != null)
			{
				key = component.text;
			}
		}
		if (!string.IsNullOrEmpty(key))
		{
			value = Localization.Get(key) + addon;
		}
	}
}
