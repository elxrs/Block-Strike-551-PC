using UnityEngine;

public class UIHUDManager : MonoBehaviour
{
	public UIWidget[] list;

	private void Start()
	{
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
	}

	private void UpdateSettings()
	{
		TimerManager.In(0.1f, delegate
		{
			for (int i = 0; i < list.Length; i++)
			{
				list[i].isCalculateFinalAlpha = !Settings.HUD;
			}
		});
	}
}
