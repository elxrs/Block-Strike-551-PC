using System;
using System.Collections.Generic;
using FreeJSON;
using Photon;
using UnityEngine;

public class mFriendsManager : PunBehaviour
{
	private static Dictionary<int, string> cacheFriendInfo = new Dictionary<int, string>();

	public UIGrid grid;

	public UIGrid friendStatsGrid;

	public UIPanel friendStatsPanel;

	public GameObject playerStatsPanel;

	public UISprite playerStatsInRoomSprite;

	public UILabel playerStatsNameLabel;

	public UILabel playerStatsIDLabel;

	public UILabel playerStatsLevelLabel;

	public UILabel playerStatsXPLabel;

	public UILabel playerStatsDeathsLabel;

	public UILabel playerStatsKillsLabel;

	public UILabel playerStatsHeadshotLabel;

	public UILabel playerStatsTimeLabel;

	public UILabel playerStatsLastLoginLabel;

	public GameObject roomStatsPanel;

	public UILabel roomStatsRoomLabel;

	public UILabel roomStatsModeLabel;

	public UILabel roomStatsMapLabel;

	public UILabel roomStatsMaxPlayersLabel;

	public UILabel roomStatsPasswordLabel;

	public UILabel roomStatsCustomMapsLabel;

	public UILabel infoLabel;

	public mFriendsElement container;

	private mFriendsElement selectFriend;

	private List<mFriendsElement> activeContainers = new List<mFriendsElement>();

	private List<mFriendsElement> deactiveContainers = new List<mFriendsElement>();

	private void Start()
	{
		PhotonClassesManager.Add(this);
	}

	public override void OnUpdatedFriendList()
	{
		if (PhotonNetwork.Friends != null)
		{
			UpdateList();
		}
	}

	public void Open()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
			return;
		}
		if (!PhotonNetwork.connected)
		{
			UIToast.Show(Localization.Get("Connect to the region"));
			return;
		}
		mPanelManager.ShowPanel("Friends", true);
		UpdateList();
	}

	public void Connect()
	{
		if (selectFriend.info.IsInRoom)
		{
			RoomInfo room = GetRoom(selectFriend.info.Room);
			if (room != null)
			{
				friendStatsPanel.cachedGameObject.SetActive(false);
				mJoinServer.room = room;
				mJoinServer.onBack = OnBack;
				mJoinServer.Join();
			}
		}
	}

	private void OnBack()
	{
		mPopUp.HideAll("Friends", true);
	}

	private RoomInfo GetRoom(string name)
	{
		RoomInfo[] roomList = PhotonNetwork.GetRoomList();
		for (int i = 0; i < roomList.Length; i++)
		{
			if (roomList[i].Name == name)
			{
				return roomList[i];
			}
		}
		return null;
	}

	public void DeletePlayer()
	{
		mPopUp.ShowPopup(Localization.Get("Do you want to remove") + ": " + selectFriend.Name.text, Localization.Get("Delete"), Localization.Get("No"), delegate
		{
			mPopUp.HideAll();
		}, Localization.Get("Yes"), delegate
		{
			mPopUp.HideAll();
			AccountManager.instance.Data.Friends.Remove(selectFriend.ID);
			AccountManager.DeleteFriend(selectFriend.ID, delegate
			{
				UIToast.Show("Delete Friend Complete");
			}, delegate(string error)
			{
				UIToast.Show("Delete Friend Error: " + error);
			});
			friendStatsPanel.cachedGameObject.SetActive(false);
			UpdateList();
		});
	}

	public void SelectPlayer(mFriendsElement element)
	{
		if (element == null && element.info != null)
		{
			return;
		}
		selectFriend = element;
		friendStatsPanel.cachedGameObject.SetActive(true);
		if (PhotonNetwork.Friends != null && PhotonNetwork.connected)
		{
			roomStatsPanel.SetActive(element.info.IsInRoom);
			if (element.info.IsInRoom)
			{
				RoomInfo room = GetRoom(element.info.Room);
				roomStatsRoomLabel.text = room.Name;
				roomStatsModeLabel.text = Localization.Get(room.GetGameMode().ToString());
				roomStatsMapLabel.text = room.GetSceneName();
				roomStatsMaxPlayersLabel.text = room.PlayerCount + "/" + room.MaxPlayers;
				roomStatsPasswordLabel.text = ((!room.HasPassword()) ? Localization.Get("No") : Localization.Get("Yes"));
				roomStatsCustomMapsLabel.text = ((!room.isCustomMap()) ? Localization.Get("No") : Localization.Get("Yes"));
			}
			playerStatsInRoomSprite.color = ((!element.info.IsOnline) ? new Color(0.83f, 0.18f, 0.18f, 1f) : new Color(0.31f, 0.71f, 0.32f, 1f));
		}
		else
		{
			roomStatsPanel.SetActive(false);
			playerStatsInRoomSprite.color = new Color(0.83f, 0.18f, 0.18f, 1f);
		}
		playerStatsNameLabel.text = element.Name.text;
		playerStatsIDLabel.text = element.ID.ToString();
		playerStatsLevelLabel.text = string.Empty;
		playerStatsXPLabel.text = string.Empty;
		playerStatsDeathsLabel.text = string.Empty;
		playerStatsKillsLabel.text = string.Empty;
		playerStatsHeadshotLabel.text = string.Empty;
		playerStatsTimeLabel.text = string.Empty;
		playerStatsLastLoginLabel.text = string.Empty;
		if (cacheFriendInfo.ContainsKey(element.ID))
		{
			UpdateFriendInfo(cacheFriendInfo[element.ID]);
		}
		else
		{
			AccountManager.GetFriendsInfo(element.ID, delegate(string result)
			{
				cacheFriendInfo.Add(element.ID, result);
				UpdateFriendInfo(result);
			}, delegate(string error)
			{
				UIToast.Show("Get Friend Info Error: " + error);
			});
		}
		friendStatsGrid.repositionNow = true;
	}

	private void UpdateFriendInfo(string result)
	{
		JsonObject jsonObject = JsonObject.Parse(result);
		playerStatsNameLabel.text = jsonObject.Get<string>("0");
		playerStatsIDLabel.text = jsonObject.Get<string>("1");
		playerStatsLevelLabel.text = jsonObject.Get("2", "1");
		playerStatsXPLabel.text = jsonObject.Get("3", "0");
		playerStatsDeathsLabel.text = jsonObject.Get("4", "0");
		playerStatsKillsLabel.text = jsonObject.Get("5", "0");
		playerStatsHeadshotLabel.text = jsonObject.Get("6", "0");
		playerStatsTimeLabel.text = ConvertTime(jsonObject.Get("7", 0L));
		playerStatsLastLoginLabel.text = ConvertLastLoginTime(jsonObject.Get<long>("8"));
		CryptoPrefs.SetString("Friend_#" + playerStatsIDLabel.text, playerStatsNameLabel.text);
	}

	public void Refresh()
	{
		if (PhotonNetwork.connected)
		{
			string[] array = GetFriends().ToArray();
			if (array.Length > 0)
			{
				PhotonNetwork.FindFriends(array);
			}
		}
		UpdateList();
	}

	public void UpdateList()
	{
		if (PhotonNetwork.Friends == null && PhotonNetwork.connected)
		{
			string[] array = GetFriends().ToArray();
			if (array.Length > 0)
			{
				PhotonNetwork.FindFriends(array);
			}
		}
		DeactiveAll();
		for (int i = 0; i < AccountManager.instance.Data.Friends.Count; i++)
		{
			GetContainer().SetData(AccountManager.instance.Data.Friends[i]);
		}
		grid.repositionNow = true;
		if (!PhotonNetwork.connected)
		{
			infoLabel.text = Localization.Get("Connect to the region");
			return;
		}
		byte b = 0;
		byte b2 = 0;
		if (PhotonNetwork.Friends != null)
		{
			for (int j = 0; j < PhotonNetwork.Friends.Count; j++)
			{
				if (PhotonNetwork.Friends[j].IsOnline)
				{
					b++;
				}
				if (PhotonNetwork.Friends[j].IsInRoom)
				{
					b2++;
				}
			}
		}
		string text = Localization.Get("Friends") + ": " + AccountManager.instance.Data.Friends.Count + "/" + 20 + "\n" + Localization.Get("Online") + ": " + b + "/" + AccountManager.instance.Data.Friends.Count + "\n" + Localization.Get("Play in server") + ": " + b2 + "/" + b;
		infoLabel.text = text;
	}

	private mFriendsElement GetContainer()
	{
		if (deactiveContainers.Count != 0)
		{
			mFriendsElement mFriendsElement2 = deactiveContainers[0];
			deactiveContainers.RemoveAt(0);
			mFriendsElement2.Widget.cachedGameObject.SetActive(true);
			activeContainers.Add(mFriendsElement2);
			return mFriendsElement2;
		}
		GameObject gameObject = NGUITools.AddChild(grid.gameObject, container.Widget.cachedGameObject);
		gameObject.SetActive(true);
		mFriendsElement component = gameObject.GetComponent<mFriendsElement>();
		activeContainers.Add(component);
		return component;
	}

	private void DeactiveAll()
	{
		for (int i = 0; i < activeContainers.Count; i++)
		{
			activeContainers[i].Widget.cachedGameObject.SetActive(false);
			deactiveContainers.Add(activeContainers[i]);
		}
		activeContainers.Clear();
	}

	public static List<string> GetFriends()
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		string empty = string.Empty;
		for (int i = 0; i < AccountManager.instance.Data.Friends.Count; i++)
		{
			empty = CryptoPrefs.GetString("Friend_#" + AccountManager.instance.Data.Friends[i], "null");
			if (empty == "null")
			{
				list2.Add(AccountManager.instance.Data.Friends[i]);
			}
			else
			{
				list.Add(empty);
			}
		}
		if (list2.Count != 0)
		{
			GetFriendsNames(list2.ToArray());
		}
		return list;
	}

	public static void GetFriendsNames(int[] ids)
	{
		AccountManager.GetFriendsName(ids, delegate
		{
			PhotonNetwork.FindFriends(GetFriends().ToArray());
		}, delegate(string error)
		{
			UIToast.Show("Get Friends Name Error: " + error);
		});
	}

	private string ConvertTime(long time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		return string.Format("{0:0}:{1:00}:{2:00}", timeSpan.Days * 24 + timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	}

	private string ConvertLastLoginTime(long time)
	{
		time += NTPManager.GetMilliSeconds(DateTime.Now) - NTPManager.GetMilliSeconds(DateTime.UtcNow);
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(time).ToLocalTime();
		return dateTime.Day.ToString("D2") + "/" + dateTime.Month.ToString("D2") + "/" + dateTime.Year + " " + dateTime.Hour.ToString("D2") + ":" + dateTime.Minute.ToString("D2") + ":" + dateTime.Second.ToString("D2");
	}
}
