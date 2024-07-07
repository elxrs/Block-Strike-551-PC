using System.Collections.Generic;
using UnityEngine;

public class UIChangeTeam : MonoBehaviour
{
	public UISprite ChangeTeamSprite;

	private bool isChangeTeam;

	private float time;

	private bool ChangeOnlyDead;

	private static UIChangeTeam instance;

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		UIGameManager.PauseEvent += OnPause;
	}

	private void OnDisable()
	{
		UIGameManager.PauseEvent -= OnPause;
	}

	public static void SetChangeTeam(bool active)
	{
		SetChangeTeam(active, false);
	}

	public static void SetChangeTeam(bool active, bool onlyDead)
	{
		instance.isChangeTeam = active;
		instance.ChangeOnlyDead = onlyDead;
	}

	private void OnPause()
	{
		if (PhotonNetwork.offlineMode || !isChangeTeam)
		{
			return;
		}
		if (ChangeOnlyDead && !PhotonNetwork.player.GetDead())
		{
			ChangeTeamSprite.cachedGameObject.SetActive(false);
			return;
		}
		if (time > Time.time)
		{
			ChangeTeamSprite.cachedGameObject.SetActive(false);
			return;
		}
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		byte b = 0;
		byte b2 = 0;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() == Team.Blue)
			{
				b++;
			}
			else if (playerList[i].GetTeam() == Team.Red)
			{
				b2++;
			}
		}
		if (PhotonNetwork.player.GetTeam() == Team.Blue)
		{
			ChangeTeamSprite.cachedGameObject.SetActive(b >= b2);
		}
		else if (PhotonNetwork.player.GetTeam() == Team.Red)
		{
			ChangeTeamSprite.cachedGameObject.SetActive(b2 >= b);
		}
	}

	public void ChangeTeam()
	{
		if (PhotonNetwork.offlineMode || !isChangeTeam || time > Time.time)
		{
			return;
		}
		if (ChangeOnlyDead && !PhotonNetwork.player.GetDead())
		{
			ChangeTeamSprite.cachedGameObject.SetActive(false);
			return;
		}
		Team team = PhotonNetwork.player.GetTeam();
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		List<PhotonPlayer> list2 = new List<PhotonPlayer>();
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].GetTeam() == Team.Blue)
			{
				list.Add(playerList[i]);
			}
		}
		for (int j = 0; j < playerList.Length; j++)
		{
			if (playerList[j].GetTeam() == Team.Red)
			{
				list2.Add(playerList[j]);
			}
		}
		switch (team)
		{
		case Team.Blue:
			if (list.Count >= list2.Count)
			{
				time = Time.time + 60f;
				GameManager.UpdatePlayerTeam(Team.Red);
				EventManager.Dispatch("AutoBalance", Team.Red);
				OnPause();
			}
			break;
		case Team.Red:
			if (list2.Count >= list.Count)
			{
				time = Time.time + 60f;
				GameManager.UpdatePlayerTeam(Team.Blue);
				EventManager.Dispatch("AutoBalance", Team.Blue);
				OnPause();
			}
			break;
		}
	}
}
