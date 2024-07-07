using System;
using UnityEngine;

public class mPopUp : MonoBehaviour
{
	[Header("Popup")]
	public GameObject PopupPanel;

	public UILabel PopupTitleLabel;

	public UILabel PopupTextLabel;

	public UILabel PopupButton1;

	public UILabel PopupButton2;

	public static bool ActivePopup;

	[Header("Text Line")]
	public GameObject TextLinePanel;

	public UILabel TextLineLabel;

	[Header("Input")]
	public GameObject InputPanel;

	public UILabel InputTitleLabel;

	public UILabel InputTextLabel;

	public UIInput InputField;

	public UILabel InputButton1;

	public UILabel InputButton2;

	private Action InputAction;

	private Action InputChange;

	[Header("Wait")]
	public GameObject WaitSprite;

	public UILabel WaitLabel;

	public GameObject WaitPanel;

	public UILabel WaitPanelLabel;

	private Action Button1Action;

	private Action Button2Action;

	private static mPopUp instance;

	private void Awake()
	{
		instance = this;
	}

	public static void SetActiveWait(bool active)
	{
		SetActiveWait(active, string.Empty);
	}

	public static void SetActiveWait(bool active, string text)
	{
		instance.WaitSprite.SetActive(active);
		if (active)
		{
			instance.WaitLabel.text = text;
		}
	}

	public static void SetActiveWaitPanel(bool active)
	{
		SetActiveWaitPanel(active, string.Empty);
	}

	public static void SetActiveWaitPanel(bool active, string text)
	{
		instance.WaitPanel.SetActive(active);
		if (active)
		{
			instance.WaitPanelLabel.text = text;
		}
	}

	public static void ShowText(string text)
	{
		ShowText(text, 0f, string.Empty);
	}

	public static void ShowText(string text, float duration, string panel)
	{
		mPanelManager.HidePanels();
		instance.Clear();
		instance.TextLinePanel.SetActive(true);
		instance.TextLineLabel.text = text;
		if (duration != 0f)
		{
			TimerManager.In(duration, delegate
			{
				HideAll(panel);
			});
		}
	}

	public static void ShowPopup(string text, string title, string buttonText, Action callbackButton)
	{
		ShowPopup(text, title, string.Empty, null, buttonText, callbackButton);
	}

	public static void ShowPopup(string text, string title, string button1Text, Action callbackButton1, string button2Text, Action callbackButton2)
	{
		instance.Clear();
		ActivePopup = true;
		instance.PopupPanel.SetActive(true);
		instance.PopupTextLabel.text = text;
		instance.PopupTitleLabel.text = title;
		if (button1Text == string.Empty)
		{
			instance.PopupButton1.cachedGameObject.SetActive(false);
			Vector3 localPosition = instance.PopupButton2.cachedTransform.localPosition;
			localPosition.x = 0f;
			instance.PopupButton2.cachedTransform.localPosition = localPosition;
		}
		else
		{
			instance.PopupButton1.cachedGameObject.SetActive(true);
			if (instance.PopupButton2.cachedTransform.localPosition.x == 0f)
			{
				Vector3 localPosition2 = instance.PopupButton2.cachedTransform.localPosition;
				localPosition2.x = 110f;
				instance.PopupButton2.cachedTransform.localPosition = localPosition2;
			}
			instance.PopupButton1.text = button1Text;
			instance.Button1Action = callbackButton1;
		}
		instance.PopupButton2.text = button2Text;
		instance.Button2Action = callbackButton2;
	}

	public void OnClickButton(int button)
	{
		switch (button)
		{
		case 1:
			if (Button1Action != null)
			{
				Button1Action();
			}
			break;
		case 2:
			if (Button2Action != null)
			{
				Button2Action();
			}
			break;
		}
	}

	public static void ShowInput(string text, string title, int limit, UIInput.KeyboardType keyboardType, Action callbackInput, Action callbackChange, string button1Text, Action callbackButton1, string button2Text, Action callbackButton2)
	{
		instance.Clear();
		instance.InputPanel.SetActive(true);
		instance.InputTextLabel.text = text;
		instance.InputTitleLabel.text = title;
		instance.InputField.characterLimit = limit;
		instance.InputField.keyboardType = keyboardType;
		instance.InputAction = callbackInput;
		instance.InputChange = callbackChange;
		instance.InputField.value = text;
		instance.InputButton1.text = button1Text;
		instance.Button1Action = callbackButton1;
		instance.InputButton2.text = button2Text;
		instance.Button2Action = callbackButton2;
	}

	public void OnInputSubmit()
	{
		if (InputAction != null)
		{
			InputAction();
		}
	}

	public void OnInputChange()
	{
		if (InputChange != null)
		{
			InputChange();
		}
	}

	public static string GetInputText()
	{
		return instance.InputField.value;
	}

	public static void SetInputText(string text)
	{
		instance.InputField.value = text;
	}

	public static void HideAll()
	{
		HideAll(string.Empty);
	}

	public static void HideAll(string panel)
	{
		HideAll(panel, true);
	}

	public static void HideAll(string panel, bool playerDataActive)
	{
		instance.Clear();
		ActivePopup = false;
		if (!string.IsNullOrEmpty(panel))
		{
			mPanelManager.ShowPanel(panel, playerDataActive);
		}
		else if (!mPanelManager.HasActivePanel())
		{
			if (string.IsNullOrEmpty(mPanelManager.GetLastPanel()))
			{
				mPanelManager.ShowPanel("Menu", playerDataActive);
			}
			else
			{
				mPanelManager.ShowLastPanel(playerDataActive);
			}
		}
	}

	private void Clear()
	{
		PopupPanel.SetActive(false);
		TextLinePanel.SetActive(false);
		InputPanel.SetActive(false);
	}
}
