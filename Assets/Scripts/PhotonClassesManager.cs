using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;

public static class PhotonClassesManager
{
	private static List<PunBehaviour> behaviourList = new List<PunBehaviour>();

	private static int count = 0;

	public static void Add(PunBehaviour behaviour)
	{
		behaviourList.Add(behaviour);
		count = behaviourList.Count;
	}

	public static void SendMessage(PhotonNetworkingMessage method, params object[] parameters)
	{
		behaviourList.RemoveAll((PunBehaviour x) => x == null);
		count = behaviourList.Count;
		switch (method)
		{
		case PhotonNetworkingMessage.OnConnectedToPhoton:
		{
			for (int num18 = 0; num18 < count; num18++)
			{
				behaviourList[num18].OnConnectedToPhoton();
			}
			break;
		}
		case PhotonNetworkingMessage.OnLeftRoom:
		{
			for (int num2 = 0; num2 < count; num2++)
			{
				behaviourList[num2].OnLeftRoom();
			}
			break;
		}
		case PhotonNetworkingMessage.OnMasterClientSwitched:
		{
			for (int num10 = 0; num10 < count; num10++)
			{
				behaviourList[num10].OnMasterClientSwitched((PhotonPlayer)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonCreateRoomFailed:
		{
			for (int num22 = 0; num22 < count; num22++)
			{
				behaviourList[num22].OnPhotonCreateRoomFailed(parameters);
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonJoinRoomFailed:
		{
			for (int num14 = 0; num14 < count; num14++)
			{
				behaviourList[num14].OnPhotonJoinRoomFailed(parameters);
			}
			break;
		}
		case PhotonNetworkingMessage.OnCreatedRoom:
		{
			for (int num6 = 0; num6 < count; num6++)
			{
				behaviourList[num6].OnCreatedRoom();
			}
			break;
		}
		case PhotonNetworkingMessage.OnJoinedLobby:
		{
			for (int l = 0; l < count; l++)
			{
				behaviourList[l].OnJoinedLobby();
			}
			break;
		}
		case PhotonNetworkingMessage.OnLeftLobby:
		{
			for (int num20 = 0; num20 < count; num20++)
			{
				behaviourList[num20].OnLeftLobby();
			}
			break;
		}
		case PhotonNetworkingMessage.OnFailedToConnectToPhoton:
		{
			for (int num16 = 0; num16 < count; num16++)
			{
				behaviourList[num16].OnFailedToConnectToPhoton((DisconnectCause)(int)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnDisconnectedFromPhoton:
		{
			for (int num12 = 0; num12 < count; num12++)
			{
				behaviourList[num12].OnDisconnectedFromPhoton();
			}
			break;
		}
		case PhotonNetworkingMessage.OnConnectionFail:
		{
			for (int num8 = 0; num8 < count; num8++)
			{
				behaviourList[num8].OnConnectionFail((DisconnectCause)(int)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonInstantiate:
		{
			for (int num4 = 0; num4 < count; num4++)
			{
				behaviourList[num4].OnPhotonInstantiate((PhotonMessageInfo)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnReceivedRoomListUpdate:
		{
			for (int n = 0; n < count; n++)
			{
				behaviourList[n].OnReceivedRoomListUpdate();
			}
			break;
		}
		case PhotonNetworkingMessage.OnJoinedRoom:
		{
			for (int j = 0; j < count; j++)
			{
				behaviourList[j].OnJoinedRoom();
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonPlayerConnected:
		{
			for (int num21 = 0; num21 < count; num21++)
			{
				behaviourList[num21].OnPhotonPlayerConnected((PhotonPlayer)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonPlayerDisconnected:
		{
			for (int num19 = 0; num19 < count; num19++)
			{
				behaviourList[num19].OnPhotonPlayerDisconnected((PhotonPlayer)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonRandomJoinFailed:
		{
			for (int num17 = 0; num17 < count; num17++)
			{
				behaviourList[num17].OnPhotonRandomJoinFailed(parameters);
			}
			break;
		}
		case PhotonNetworkingMessage.OnConnectedToMaster:
		{
			for (int num15 = 0; num15 < count; num15++)
			{
				behaviourList[num15].OnConnectedToMaster();
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonMaxCccuReached:
		{
			for (int num13 = 0; num13 < count; num13++)
			{
				behaviourList[num13].OnPhotonMaxCccuReached();
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonCustomRoomPropertiesChanged:
		{
			for (int num11 = 0; num11 < count; num11++)
			{
				behaviourList[num11].OnPhotonCustomRoomPropertiesChanged((Hashtable)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged:
		{
			for (int num9 = 0; num9 < count; num9++)
			{
				behaviourList[num9].OnPhotonPlayerPropertiesChanged(parameters);
			}
			break;
		}
		case PhotonNetworkingMessage.OnUpdatedFriendList:
		{
			for (int num7 = 0; num7 < count; num7++)
			{
				behaviourList[num7].OnUpdatedFriendList();
			}
			break;
		}
		case PhotonNetworkingMessage.OnCustomAuthenticationFailed:
		{
			for (int num5 = 0; num5 < count; num5++)
			{
				behaviourList[num5].OnCustomAuthenticationFailed((string)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnCustomAuthenticationResponse:
		{
			for (int num3 = 0; num3 < count; num3++)
			{
				behaviourList[num3].OnCustomAuthenticationResponse((Dictionary<string, object>)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnWebRpcResponse:
		{
			for (int num = 0; num < count; num++)
			{
				behaviourList[num].OnWebRpcResponse((OperationResponse)parameters[0]);
			}
			break;
		}
		case PhotonNetworkingMessage.OnOwnershipRequest:
		{
			for (int m = 0; m < count; m++)
			{
				behaviourList[m].OnOwnershipRequest(parameters);
			}
			break;
		}
		case PhotonNetworkingMessage.OnOwnershipTransfered:
		{
			for (int k = 0; k < count; k++)
			{
				behaviourList[k].OnOwnershipTransfered(parameters);
			}
			break;
		}
		case PhotonNetworkingMessage.OnLobbyStatisticsUpdate:
		{
			for (int i = 0; i < count; i++)
			{
				behaviourList[i].OnLobbyStatisticsUpdate();
			}
			break;
		}
		case PhotonNetworkingMessage.OnPhotonSerializeView:
		case PhotonNetworkingMessage.OnPhotonPlayerActivityChanged:
			break;
		}
	}
}
