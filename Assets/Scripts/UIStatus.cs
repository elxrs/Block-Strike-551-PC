using System.Collections.Generic;
using UnityEngine;

public class UIStatus : MonoBehaviour
{
	public UILabel Label;

	public UISprite Background;

	private List<string> List = new List<string>();

	private static UIStatus instance;

	private void Start()
	{
		instance = this;
		PhotonEvent.AddListener(PhotonEventTag.StatusNewLine, PhotonNewLine);
	}

	public static void Add(string text)
	{
		Add(text, false, string.Empty);
	}

	public static void Add(string text, bool local)
	{
		Add(text, local, string.Empty);
	}

	public static void Add(string text, bool local, string localize)
	{
		if (local)
		{
			NewLine(text);
			return;
		}
		bool flag = text[0] == '.';
		PhotonEvent.RPC(PhotonEventTag.StatusNewLine, PhotonTargets.All, text, localize, flag);
	}

	private void PhotonNewLine(PhotonEventData data)
	{
		string text = (string)data.parameters[0];
		string text2 = (string)data.parameters[1];
		bool flag = (bool)data.parameters[2];
		if (string.IsNullOrEmpty(text2))
		{
			if (flag)
			{
				if (PhotonNetwork.player.GetTeam() == PhotonPlayer.Find(data.senderID).GetTeam())
				{
					text = text.Remove(0, 1);
					NewLine(text);
				}
			}
			else
			{
				NewLine(text);
			}
		}
		else if (flag)
		{
			if (PhotonNetwork.player.GetTeam() == PhotonPlayer.Find(data.senderID).GetTeam())
			{
				text2 = Localization.Get(text2);
				text = text.Replace("[@]", text2);
				text = text.Remove(0, 1);
				NewLine(text);
			}
		}
		else
		{
			text2 = Localization.Get(text2);
			text = text.Replace("[@]", text2);
			NewLine(text);
		}
	}

	public static void NewLine(string text)
	{
		instance.List.Add(text);
		instance.UpdateLabel(true);
	}

	private void UpdateLabel(bool clear)
	{
		string text = string.Empty;
		for (int i = 0; i < List.Count; i++)
		{
			if (i > 0)
			{
				text += "\n";
			}
			text += List[List.Count - 1 - i];
		}
		Label.text = text;
		if (string.IsNullOrEmpty(text))
		{
			Background.alpha = 0f;
		}
		else
		{
			Background.alpha = 0.1f;
			Background.UpdateAnchors();
		}
		if (clear)
		{
			TimerManager.In(5f, RemoveLabel);
		}
	}

	private void RemoveLabel()
	{
		List.RemoveAt(0);
		UpdateLabel(false);
	}
}
