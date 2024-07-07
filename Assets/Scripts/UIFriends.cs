using System.Collections.Generic;
using UnityEngine;

public class UIFriends : MonoBehaviour
{
	public const int maxFriends = 20;

	private static List<int> requestPlayers = new List<int>();

	private void Start()
	{
		PhotonEvent.AddListener(PhotonEventTag.AddFriend, PhotonAddFriend);
	}

	public void AddFriend()
	{
		if (requestPlayers.Contains(UIPlayerStatistics.SelectPlayer.GetPlayerID()))
		{
			UIToast.Show(Localization.Get("Request has already been sent"));
			return;
		}
		if (AccountManager.instance.Data.Friends.Contains(UIPlayerStatistics.SelectPlayer.GetPlayerID()))
		{
			UIToast.Show(Localization.Get("Player is already in your friends"));
			return;
		}
		if (AccountManager.instance.Data.Friends.Count >= 20)
		{
			UIToast.Show(Localization.Get("Reached the maximum number of friends"));
			return;
		}
		PhotonEvent.RPC(PhotonEventTag.AddFriend, UIPlayerStatistics.SelectPlayer, (byte)1);
		requestPlayers.Add(UIPlayerStatistics.SelectPlayer.GetPlayerID());
	}

	private void PhotonAddFriend(PhotonEventData data)
	{
		switch ((byte)data.parameters[0])
		{
		case 1:
		{
			PhotonPlayer player = PhotonPlayer.Find(data.senderID);
			if (player == null || AccountManager.instance.Data.Friends.Contains(player.GetPlayerID()) || AccountManager.instance.Data.Friends.Count >= 20)
			{
				break;
			}
			UIMessage.Add(Localization.Get("Add friend") + ": " + player.NickName, Localization.Get("Friends"), data.senderID, delegate(bool result, object obj)
			{
				if (AccountManager.instance.Data.Friends.Count >= 20)
				{
					UIToast.Show(Localization.Get("Reached the maximum number of friends"));
				}
				else
				{
					player = PhotonPlayer.Find((int)obj);
					if (player != null)
					{
						if (result)
						{
							CryptoPrefs.SetString("Friend_#" + (int)obj, player.NickName);
							AccountManager.AddFriend(player.GetPlayerID(), delegate
							{
								UIToast.Show(Localization.Get("Added friend"));
							}, delegate(string error)
							{
								UIToast.Show(error);
							});
						}
						PhotonEvent.RPC(PhotonEventTag.AddFriend, player, (byte)2, result);
						requestPlayers.Add(player.GetPlayerID());
					}
				}
			});
			break;
		}
		case 2:
			if ((bool)data.parameters[1])
			{
				if (AccountManager.instance.Data.Friends.Count < 20)
				{
					PhotonPlayer photonPlayer = PhotonPlayer.Find(data.senderID);
					CryptoPrefs.SetString("Friend_#" + photonPlayer.GetPlayerID(), photonPlayer.NickName);
					AccountManager.AddFriend(photonPlayer.GetPlayerID(), delegate
					{
						UIToast.Show(Localization.Get("Added friend"));
					}, delegate(string error)
					{
						UIToast.Show(error);
					});
				}
			}
			else
			{
				UIToast.Show("declined the request");
			}
			break;
		}
	}
}
