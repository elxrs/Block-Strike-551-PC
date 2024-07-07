using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;

public class mPanelManager : MonoBehaviour
{
	public List<GameObject> PanelList = new List<GameObject>();

	public GameObject PlayerDataPanel;

	private string ActivePanel = "Menu";

	private string LastPanel = "Menu";

	private static mPanelManager instance;

	private void Awake()
	{
		instance = this;
	}

	public static void ShowPanel(string panel, bool playerDataPanel)
	{
		instance.SetActivePlayerDataPanel(playerDataPanel);
		for (int i = 0; i < instance.PanelList.Count; i++)
		{
			if (instance.PanelList[i].name == panel)
			{
				if (!string.IsNullOrEmpty(instance.ActivePanel))
				{
					instance.LastPanel = instance.ActivePanel;
				}
				instance.ActivePanel = instance.PanelList[i].name;
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
		ShowPanel(panel.name, true);
	}

	public static void ShowLastPanel(bool playerDataPanel)
	{
		ShowPanel(GetLastPanel(), playerDataPanel);
	}

	public static void HidePanels()
	{
		for (int i = 0; i < instance.PanelList.Count; i++)
		{
			instance.PanelList[i].SetActive(false);
		}
		instance.ActivePanel = string.Empty;
		instance.SetActivePlayerDataPanel(false);
	}

	public static bool HasActivePanel()
	{
		if (instance.ActivePanel != string.Empty)
		{
			return true;
		}
		return false;
	}

	public static string GetActivePanel()
	{
		return instance.ActivePanel;
	}

	public static string GetLastPanel()
	{
		return instance.LastPanel;
	}

	[SkipRename]
	public void SetActivePlayerDataPanel(bool active)
	{
		PlayerDataPanel.SetActive(active);
	}

	public static void SetActivePlayerData(bool active)
	{
		instance.SetActivePlayerDataPanel(active);
	}
}
