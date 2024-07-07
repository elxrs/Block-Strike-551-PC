using System;
using System.Collections.Generic;
using FreeJSON;
using Photon;
using UnityEngine;

public class UIKick : PunBehaviour
{
	private float lastKickTime;

	private Dictionary<string, List<string>> kickList = new Dictionary<string, List<string>>();

	private List<string> kicked = new List<string>();

	private void Start()
	{
		PhotonClassesManager.Add(this);
		PhotonEvent.AddListener(PhotonEventTag.KickPlayer, PhotonKickPlayer);
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (!PhotonNetwork.isMasterClient)
		{
			return;
		}
		if (kicked.Contains(playerConnect.GetPlayerID().ToString()) || string.IsNullOrEmpty(playerConnect.GetPlayerID().ToString()))
		{
			PhotonNetwork.CloseConnection(playerConnect);
			return;
		}
		TimerManager.In(1f, delegate
		{
			JsonObject jsonObject = new JsonObject();
			if (kickList.Count > 0)
			{
				jsonObject.Add("1", kickList);
			}
			if (kicked.Count > 0)
			{
				jsonObject.Add("2", kicked);
			}
			if (jsonObject.Length > 0)
			{
				PhotonEvent.RPC(PhotonEventTag.KickPlayer, playerConnect, 3, jsonObject.ToString());
			}
		});
	}

	public void Kick()
	{
		if (lastKickTime > Time.time)
		{
			UIToast.Show(ConvertTime(lastKickTime - Time.time));
			return;
		}
		if (kickList.ContainsKey(UIPlayerStatistics.SelectPlayer.GetPlayerID().ToString()) && kickList[UIPlayerStatistics.SelectPlayer.GetPlayerID().ToString()].Contains(PhotonNetwork.player.GetPlayerID().ToString()))
		{
			UIToast.Show("Error");
			return;
		}
		PhotonEvent.RPC(PhotonEventTag.KickPlayer, PhotonTargets.All, 1, UIPlayerStatistics.SelectPlayer.ID);
		lastKickTime = Time.time + 300f;
		UIToast.Show("Ok");
	}

	private void PhotonKickPlayer(PhotonEventData data)
	{
		if ((int)data.parameters[0] == 1)
		{
			PhotonPlayer player = PhotonPlayer.Find((int)data.parameters[1]);
			PhotonPlayer photonPlayer = PhotonPlayer.Find(data.senderID);
			if (player == null || photonPlayer == null)
			{
				return;
			}
			if (kickList.ContainsKey(player.GetPlayerID().ToString()))
			{
				kickList[player.GetPlayerID().ToString()].Add(photonPlayer.GetPlayerID().ToString());
			}
			else
			{
				kickList[player.GetPlayerID().ToString()] = new List<string> { photonPlayer.GetPlayerID().ToString() };
			}
			if (!PhotonNetwork.isMasterClient)
			{
				return;
			}
			int num = kickList[player.GetPlayerID().ToString()].Count * 100;
			if (num / PhotonNetwork.room.PlayerCount >= 60)
			{
				PhotonEvent.RPC(PhotonEventTag.KickPlayer, PhotonTargets.All, 2, player.ID);
				TimerManager.In(1.5f, delegate
				{
					PhotonNetwork.CloseConnection(player);
				});
			}
		}
		else if ((int)data.parameters[0] == 2)
		{
			PhotonPlayer photonPlayer2 = PhotonPlayer.Find((int)data.parameters[1]);
			if (photonPlayer2 != null)
			{
				kicked.Add(photonPlayer2.GetPlayerID().ToString());
				if (PhotonNetwork.player.ID == photonPlayer2.ID)
				{
					GameManager.SetLeaveRoomText(Localization.Get("You kicked from the server"));
				}
			}
		}
		else if ((int)data.parameters[0] == 3)
		{
			JsonObject jsonObject = JsonObject.Parse((string)data.parameters[1]);
			if (jsonObject.ContainsKey("1"))
			{
				kickList = jsonObject.Get<Dictionary<string, List<string>>>("1");
			}
			if (jsonObject.ContainsKey("2"))
			{
				kicked = jsonObject.Get<List<string>>("2");
			}
		}
	}

	private static string ConvertTime(float time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		return string.Format("{0:0}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
	}
}
