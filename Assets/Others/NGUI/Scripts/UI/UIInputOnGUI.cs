#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY || UNITY_WINRT)
#define MOBILE
#endif

using UnityEngine;

[RequireComponent(typeof(UIInput))]
public class UIInputOnGUI : MonoBehaviour
{
#if !MOBILE
	[System.NonSerialized] UIInput mInput;

	void Awake()
	{ 
		mInput = GetComponent<UIInput>(); 
	}

	void OnGUI()
	{
		if (Event.current.rawType == EventType.KeyDown)
			mInput.ProcessEvent(Event.current);
	}
#endif
}
