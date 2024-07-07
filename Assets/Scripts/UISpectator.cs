using System.Collections.Generic;
using Photon;
using UnityEngine;

public class UISpectator : PunBehaviour
{
	public GameObject Panel;

	public UIGrid BlueGrid;

	public UIGrid RedGrid;

	public UISpectatorElement[] BlueTeam;

	public UISpectatorElement[] RedTeam;

	public UISprite InfoSprite;

	public UISprite GradientSprite;

	public UILabel NameLabel;

	public UITexture AvatarTexture;

	public UILabel PlayerIdLabel;

	public UILabel ClanLabel;

	public UILabel KillsLabel;

	public UILabel DeathsLabel;

	public UILabel PingLabel;

	public UILabel HealthLabel;

	private PhotonPlayer selectPlayer;

	private bool isActive;

	private int InfoTimerId = -1;

	private static UISpectator instance;

	private void Start()
	{
		PhotonClassesManager.Add(this);
		instance = this;
	}

	private void GetButtonDown(string name)
	{
		if (name == "Reload")
		{
			BlueGrid.gameObject.SetActive(!BlueGrid.gameObject.activeSelf);
			RedGrid.gameObject.SetActive(!RedGrid.gameObject.activeSelf);
		}
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		if (isActive)
		{
			UpdateList();
		}
	}

	public static void SetActive(bool active)
	{
		instance.Panel.SetActive(active);
		instance.isActive = active;
		if (active)
		{
			ControllerManager.SetWeaponEvent += instance.SetWeapon;
			ControllerManager.SetDeadEvent += instance.SetDead;
			ControllerManager.SetTeamEvent += instance.SetTeam;
			ControllerManager.SetHealthEvent += instance.SetHealth;
			InputManager.GetButtonDownEvent += instance.GetButtonDown;
			CameraManager.SelectPlayerEvent += SetSelectPlayer;
			instance.UpdateList();
		}
		else
		{
			instance.OnDisable();
		}
	}

	public static bool GetActive()
	{
		return instance.isActive;
	}

	public static void SetSelectPlayer(int playerID)
	{
		if (!instance.isActive)
		{
			return;
		}
		instance.selectPlayer = PhotonPlayer.Find(playerID);
		if (instance.selectPlayer == null)
		{
			instance.InfoSprite.cachedGameObject.SetActive(false);
			TimerManager.Cancel(instance.InfoTimerId);
			return;
		}
		instance.InfoSprite.cachedGameObject.SetActive(true);
		if (instance.selectPlayer.GetTeam() == Team.Blue)
		{
			instance.GradientSprite.color = new Color32(0, 128, byte.MaxValue, 128);
		}
		else
		{
			instance.GradientSprite.color = new Color32(byte.MaxValue, 0, 0, 128);
		}
		instance.NameLabel.text = instance.selectPlayer.NickName;
		instance.AvatarTexture.mainTexture = AvatarManager.Get(instance.selectPlayer.GetAvatarUrl());
		instance.PlayerIdLabel.text = instance.selectPlayer.GetPlayerID().ToString();
		instance.ClanLabel.text = instance.selectPlayer.GetClan();
		instance.HealthLabel.text = "+" + ControllerManager.FindController(instance.selectPlayer.ID).PlayerSkin.Health;
		instance.UpdateDataSelectPlayer();
		if (instance.InfoTimerId != -1)
		{
			TimerManager.Cancel(instance.InfoTimerId);
		}
		instance.InfoTimerId = TimerManager.In(1f, -1, 1f, instance.UpdateDataSelectPlayer);
	}

	private void UpdateDataSelectPlayer()
	{
		if (selectPlayer == null)
		{
			InfoSprite.cachedGameObject.SetActive(false);
			TimerManager.Cancel(InfoTimerId);
		}
		else
		{
			KillsLabel.text = selectPlayer.GetKills().ToString();
			DeathsLabel.text = selectPlayer.GetDeaths().ToString();
			PingLabel.text = selectPlayer.GetPing().ToString();
		}
	}

	private void OnDisable()
	{
		ControllerManager.SetWeaponEvent -= SetWeapon;
		ControllerManager.SetDeadEvent -= SetDead;
		ControllerManager.SetTeamEvent -= SetTeam;
		ControllerManager.SetHealthEvent -= SetHealth;
		InputManager.GetButtonDownEvent -= GetButtonDown;
		CameraManager.SelectPlayerEvent -= SetSelectPlayer;
	}

	private void UpdateList()
	{
		for (int i = 0; i < BlueTeam.Length; i++)
		{
			BlueTeam[i].LineSprite.cachedGameObject.SetActive(false);
		}
		for (int j = 0; j < RedTeam.Length; j++)
		{
			RedTeam[j].LineSprite.cachedGameObject.SetActive(false);
		}
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		List<PhotonPlayer> list2 = new List<PhotonPlayer>();
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		for (int k = 0; k < playerList.Length; k++)
		{
			if (playerList[k].GetTeam() == Team.Blue && !playerList[k].IsLocal)
			{
				list.Add(playerList[k]);
			}
			else if (playerList[k].GetTeam() == Team.Red && !playerList[k].IsLocal)
			{
				list2.Add(playerList[k]);
			}
		}
		list.Sort(SortByKills);
		list2.Sort(SortByKills);
		ControllerManager controllerManager = null;
		if (PhotonNetwork.player.GetTeam() != Team.Red)
		{
			for (int l = 0; l < list.Count && l <= 5; l++)
			{
				BlueTeam[l].LineSprite.cachedGameObject.SetActive(true);
				BlueTeam[l].SetData(list[l]);
				controllerManager = ControllerManager.FindController(list[l].ID);
				if (controllerManager != null && controllerManager.PlayerSkin != null)
				{
					BlueTeam[l].SetHealth(controllerManager.PlayerSkin.Health);
					if (controllerManager.PlayerSkin.SelectWeapon != null)
					{
						BlueTeam[l].SetWeapon(controllerManager.PlayerSkin.SelectWeapon.WeaponID, controllerManager.PlayerSkin.SelectWeapon.WeaponSkin);
					}
				}
			}
			BlueGrid.repositionNow = true;
		}
		if (PhotonNetwork.player.GetTeam() == Team.Blue)
		{
			return;
		}
		for (int m = 0; m < list2.Count && m <= 5; m++)
		{
			RedTeam[m].LineSprite.cachedGameObject.SetActive(true);
			RedTeam[m].SetData(list2[m]);
			controllerManager = ControllerManager.FindController(list2[m].ID);
			if (controllerManager != null && controllerManager.PlayerSkin != null)
			{
				RedTeam[m].SetHealth(controllerManager.PlayerSkin.Health);
				if (controllerManager.PlayerSkin.SelectWeapon != null)
				{
					RedTeam[m].SetWeapon(controllerManager.PlayerSkin.SelectWeapon.WeaponID, controllerManager.PlayerSkin.SelectWeapon.WeaponSkin);
				}
			}
		}
		RedGrid.repositionNow = true;
	}

	public static int SortByKills(PhotonPlayer a, PhotonPlayer b)
	{
		if (a.GetKills() == b.GetKills())
		{
			if (a.GetDeaths() == b.GetDeaths())
			{
				if (a.GetLevel() == b.GetLevel())
				{
					return b.NickName.CompareTo(a.NickName);
				}
				return b.GetLevel().CompareTo(a.GetLevel());
			}
			return a.GetDeaths().CompareTo(b.GetDeaths());
		}
		return b.GetKills().CompareTo(a.GetKills());
	}

	private void SetWeapon(int playerID, int weaponID, int skinID)
	{
		if (!isActive)
		{
			return;
		}
		for (int i = 0; i < BlueTeam.Length; i++)
		{
			if (BlueTeam[i].SetWeapon(playerID, weaponID, skinID))
			{
				return;
			}
		}
		for (int j = 0; j < RedTeam.Length && !RedTeam[j].SetWeapon(playerID, weaponID, skinID); j++)
		{
		}
	}

	private void SetDead(int playerID, bool dead)
	{
		if (!isActive)
		{
			return;
		}
		if (selectPlayer != null && selectPlayer.ID == playerID && CameraManager.GetSelectCameraType() == CameraManager.CameraType.FirstPerson)
		{
			CameraManager.ActiveSpectateCamera(selectPlayer);
		}
		for (int i = 0; i < BlueTeam.Length; i++)
		{
			if (BlueTeam[i].SetDead(playerID, dead))
			{
				return;
			}
		}
		for (int j = 0; j < RedTeam.Length && !RedTeam[j].SetDead(playerID, dead); j++)
		{
		}
	}

	private void SetHealth(int playerID, byte health)
	{
		if (!isActive)
		{
			return;
		}
		if (selectPlayer != null && selectPlayer.ID == playerID)
		{
			HealthLabel.text = "+" + health;
		}
		for (int i = 0; i < BlueTeam.Length; i++)
		{
			if (BlueTeam[i].SetHealth(playerID, health))
			{
				return;
			}
		}
		for (int j = 0; j < RedTeam.Length && !RedTeam[j].SetHealth(playerID, health); j++)
		{
		}
	}

	private void SetTeam(int playerID, Team team)
	{
		if (isActive)
		{
			UpdateList();
		}
	}
}
