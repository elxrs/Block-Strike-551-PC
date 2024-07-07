using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
	public List<GameObject> PanelList = new List<GameObject>();

	private static UIPanelManager instance;

	private void Awake()
	{
		instance = this;
	}

	public static void ShowPanel(string panelName)
	{
		for (int i = 0; i < instance.PanelList.Count; i++)
		{
			if (instance.PanelList[i].name == panelName)
			{
				instance.PanelList[i].SetActive(true);
			}
			else
			{
				instance.PanelList[i].SetActive(false);
			}
		}
	}

	[SkipRename]
	public void ShowPanel(GameObject panel)
	{
		if (panel.name == "Display")
		{
			InputManager.instance.isCursor = false;
		}
		ShowPanel(panel.name);
	}

	public static void HidePanels()
	{
		for (int i = 0; i < instance.PanelList.Count; i++)
		{
			instance.PanelList[i].SetActive(false);
		}
	}
}
