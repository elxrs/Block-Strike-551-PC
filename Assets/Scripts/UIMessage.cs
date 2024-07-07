using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMessage : MonoBehaviour
{
	public class MessageClass
	{
		public string title;

		public string text;

		public Action<bool> callback;

		public Action<bool, object> callback2;

		public object data;
	}

	public UISprite MessageSprite;

	public UILabel MessageCountLabel;

	public UILabel PauseMessageCountLabel;

	public List<MessageClass> Messages = new List<MessageClass>();

	private static UIMessage instance;

	private void Start()
	{
		instance = this;
	}

	public static void Add(string text, string title, Action<bool> callback)
	{
		MessageClass messageClass = new MessageClass();
		messageClass.callback = callback;
		messageClass.title = title;
		messageClass.text = text;
		instance.Messages.Add(messageClass);
		instance.MessageSprite.alpha = 1f;
		instance.MessageCountLabel.text = instance.Messages.Count.ToString();
		instance.PauseMessageCountLabel.cachedGameObject.SetActive(true);
		instance.PauseMessageCountLabel.text = instance.Messages.Count.ToString();
	}

	public static void Add(string text, string title, object data, Action<bool, object> callback)
	{
		MessageClass messageClass = new MessageClass();
		messageClass.data = data;
		messageClass.callback2 = callback;
		messageClass.title = title;
		messageClass.text = text;
		instance.Messages.Add(messageClass);
		instance.MessageSprite.alpha = 1f;
		instance.MessageCountLabel.text = instance.Messages.Count.ToString();
		instance.PauseMessageCountLabel.cachedGameObject.SetActive(true);
		instance.PauseMessageCountLabel.text = instance.Messages.Count.ToString();
	}

	public void Click()
	{
		if (Messages.Count == 0)
		{
			MessageSprite.alpha = 0.7f;
		}
		else
		{
			UIPopUp.ShowPopUp(Messages[0].text, Messages[0].title, Localization.Get("No"), No, Localization.Get("Yes"), Yes);
		}
	}

	private void No()
	{
		UIPanelManager.ShowPanel("Pause");
		if (Messages[0].callback != null)
		{
			Messages[0].callback(false);
		}
		if (Messages[0].callback2 != null)
		{
			Messages[0].callback2(false, Messages[0].data);
		}
		Messages.RemoveAt(0);
		MessageCountLabel.text = Messages.Count.ToString();
		PauseMessageCountLabel.text = MessageCountLabel.text;
		if (Messages.Count == 0)
		{
			MessageSprite.alpha = 0.7f;
			PauseMessageCountLabel.cachedGameObject.SetActive(false);
		}
	}

	private void Yes()
	{
		UIPanelManager.ShowPanel("Pause");
		if (Messages[0].callback != null)
		{
			Messages[0].callback(true);
		}
		if (Messages[0].callback2 != null)
		{
			Messages[0].callback2(true, Messages[0].data);
		}
		Messages.RemoveAt(0);
		MessageCountLabel.text = Messages.Count.ToString();
		PauseMessageCountLabel.text = MessageCountLabel.text;
		if (Messages.Count == 0)
		{
			MessageSprite.alpha = 0.7f;
			PauseMessageCountLabel.cachedGameObject.SetActive(false);
		}
	}
}
