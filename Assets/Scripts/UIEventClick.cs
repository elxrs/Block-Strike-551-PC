using UnityEngine.Events;

public class UIEventClick : NGUIBehaviour
{
	public UnityEvent onClick;

	private void Start()
	{
		NGUIEvents.Add(gameObject, this);
	}

	public override void OnClick()
	{
		if (onClick != null)
		{
			onClick.Invoke();
		}
	}
}
