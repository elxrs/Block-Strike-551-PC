using UnityEngine;

public class TrapButtonActive : MonoBehaviour
{
	[Range(1f, 30f)]
	public int Key;

	public TrapButton[] Buttons;

	private void Start()
	{
		PhotonEvent.AddListener(PhotonEventTag.ClickButton, DeactiveButtons);
	}

	private void DeactiveButtons(PhotonEventData data)
	{
		if ((int)data.parameters[0] == Key)
		{
			for (int i = 0; i < Buttons.Length; i++)
			{
				Buttons[i].DeactiveButton();
			}
		}
	}
}
