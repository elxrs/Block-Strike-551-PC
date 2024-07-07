using System.Collections.Generic;
using UnityEngine;

public class UIChat : MonoBehaviour
{
	public UILabel Label;

	public UIInput _Input;

	public UISprite Background;

	private List<string> List = new List<string>();

	private static UIChat instance;

	private float time;

	private float maxTime;

	public static bool isChat;

	private int easterEggIndex;

	private void Start()
	{
		instance = this;
		PhotonEvent.AddListener(PhotonEventTag.ChatNewLine, PhotonNewLine);
	}

	private void OnEnable()
	{
		InputManager.GetButtonDownEvent += GetButtonDown;
	}

	private void OnDisable()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
	}

	private void GetButtonDown(string name)
	{
		if (name == "Chat")
		{
			Show();
		}
	}

	public static void Add(string text)
	{
		if (instance.time + instance.maxTime + 2f > Time.time)
		{
			instance.maxTime += 1f;
		}
		else
		{
			instance.maxTime = 0f;
		}
		instance.time = Time.time;
		bool flag = text[0] == '.';
		if (isCommand(text))
		{
			if (PhotonNetwork.isMasterClient)
			{
				EventManager.Dispatch("ServerCommand", text);
			}
		}
		else if (isEasterEgg(text))
		{
			PhotonEvent.RPC(PhotonEventTag.ChatNewLine, PhotonNetwork.player, text, flag);
		}
		else
		{
			PhotonEvent.RPC(PhotonEventTag.ChatNewLine, PhotonTargets.All, text, flag);
		}
	}

	private void PhotonNewLine(PhotonEventData data)
	{
		string text = (string)data.parameters[0];
		bool flag = (bool)data.parameters[1];
		PhotonPlayer photonPlayer = PhotonPlayer.Find(data.senderID);
		text = ((!Settings.FilterChat) ? NGUIText.StripSymbols(text) : BadWordsManager.Check(NGUIText.StripSymbols(text)));
		if (flag)
		{
			text = text.Remove(0, 1);
			text = Utils.GetTeamHexColor("[Team] " + photonPlayer.NickName, photonPlayer.GetTeam()) + ": " + text;
		}
		else
		{
			text = Utils.GetTeamHexColor(photonPlayer) + ": " + text;
		}
		if (GameManager.GetGlobalChat())
		{
			if (flag)
			{
				if (PhotonNetwork.player.GetTeam() == photonPlayer.GetTeam())
				{
					NewLine(text);
				}
			}
			else
			{
				NewLine(text);
			}
		}
		else if (photonPlayer.GetDead())
		{
			text = text.Insert(text.IndexOf("]") + 1, "[Dead] ");
			if (!PhotonNetwork.player.GetDead())
			{
				return;
			}
			if (flag)
			{
				if (PhotonNetwork.player.GetTeam() == photonPlayer.GetTeam())
				{
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
			if (PhotonNetwork.player.GetTeam() == photonPlayer.GetTeam())
			{
				NewLine(text);
			}
		}
		else
		{
			NewLine(text);
		}
	}

	private void Show()
	{
		if (PhotonNetwork.offlineMode || GameManager.GetLoadedLevel())
		{
			return;
		}
		if (time + maxTime > Time.time)
		{
			NewLine("Message sending limit " + Mathf.CeilToInt(time + maxTime - Time.time) + " sec");
		}
		else if (!UICamera.inputHasFocus)
		{
			Label.text = string.Empty;
			_Input.value = string.Empty;
			TimerManager.In(0.1f, delegate
			{
				UICamera.selectedObject = _Input.gameObject;
				isChat = true;
			});
		}
	}

	public void OnSubmit()
	{
		string value = _Input.value;
		value = value.Replace("\n", string.Empty);
		if (!string.IsNullOrWhiteSpace(value))
		{
			_Input.value = string.Empty;
			_Input.isSelected = false;
			TimerManager.In(0.1f, delegate
			{
				isChat = false;
			});
			Add(value);
		}
	}

	public static void NewLine(string text)
	{
		if (Settings.Chat)
		{
			instance.Label.supportEncoding = true;
			instance.List.Add(text);
			instance.UpdateLabel(true);
		}
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
			TimerManager.In(8f, RemoveLabel);
		}
	}

	private void RemoveLabel()
	{
		List.RemoveAt(0);
		UpdateLabel(false);
	}

	private static bool isCommand(string text)
	{
		if (text == "!checkserver")
		{
			return true;
		}
		return false;
	}

	private static bool isEasterEgg(string text)
	{
		switch (instance.easterEggIndex)
		{
		case 0:
			if (text == "Фамилия, имя, отчество?")
			{
				instance.easterEggIndex = 1;
				UIStatus.Add("Зубенко Михаил Петрович");
				return true;
			}
			return false;
		case 1:
			if (text == "Кем являетесь?")
			{
				instance.easterEggIndex = 2;
				UIStatus.Add("Вор в законе.");
				return true;
			}
			return false;
		case 2:
			if (text == "Где именно?")
			{
				instance.easterEggIndex = 3;
				UIStatus.Add("Шумиловский городок.");
				return true;
			}
			return false;
		case 3:
			if (text == "Кличка?")
			{
				instance.easterEggIndex = 0;
				UIStatus.Add("Мафиозник.");
				return true;
			}
			return false;
		default:
			return false;
		}
	}
}
