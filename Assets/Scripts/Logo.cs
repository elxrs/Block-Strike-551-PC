using System.Diagnostics;
using System.IO;
using Photon;
using UnityEngine;

public class Logo : PunBehaviour
{
	public GameObject RexetPanel;

	public GameObject RedictPanel;

	public CryptoString[] Permissions;

	private bool isLoadMenu = true;

	private void Start()
	{
		Settings.Load();
		isLoadMenu = PlayerPrefs.HasKey("Tutorial");
		Screen.sleepTimeout = -1;
		ConsoleManager.SetActive(Settings.Console);
		OnAnimation();
	}

	private void OnAnimation()
	{
		TimerManager.In(0.5f, delegate
		{
			TweenAlpha.Begin(RexetPanel, 1f, 1f);
			TimerManager.In(1.8f, delegate
			{
				TweenAlpha.Begin(RexetPanel, 1f, 0f);
				TimerManager.In(1.2f, delegate
				{
					if (isLoadMenu)
					{
						LevelManager.LoadLevel("Menu");
					}
					else
					{
						LoadTutorial();
					}
				});
			});
		});
	}

	private void LoadTutorial()
	{
		PhotonClassesManager.Add(this);
		PhotonNetwork.offlineMode = true;
		PhotonNetwork.CreateRoom("tutorial");
	}

	public override void OnJoinedRoom()
	{
		LevelManager.LoadLevel("MainTutorial");
	}
}
