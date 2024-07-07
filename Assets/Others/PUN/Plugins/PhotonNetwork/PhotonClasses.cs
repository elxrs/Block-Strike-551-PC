using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using System;

public interface IPunObservable
{
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
}

public interface IPunCallbacks
{
	void OnConnectedToPhoton();

	void OnLeftRoom();

	void OnMasterClientSwitched(PhotonPlayer newMasterClient);

	void OnPhotonCreateRoomFailed(object[] codeAndMsg);

	void OnPhotonJoinRoomFailed(object[] codeAndMsg);

	void OnCreatedRoom();

	void OnJoinedLobby();

	void OnLeftLobby();

	void OnFailedToConnectToPhoton(DisconnectCause cause);

	void OnConnectionFail(DisconnectCause cause);

	void OnDisconnectedFromPhoton();

	void OnPhotonInstantiate(PhotonMessageInfo info);

	void OnReceivedRoomListUpdate();

	void OnJoinedRoom();

	void OnPhotonPlayerConnected(PhotonPlayer newPlayer);

	void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer);

	void OnPhotonRandomJoinFailed(object[] codeAndMsg);

	void OnConnectedToMaster();

	void OnPhotonMaxCccuReached();

	void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged);

	void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps);

	void OnUpdatedFriendList();

	void OnCustomAuthenticationFailed(string debugMessage);

	void OnCustomAuthenticationResponse(Dictionary<string, object> data);

	void OnWebRpcResponse(OperationResponse response);

	void OnOwnershipRequest(object[] viewAndPlayer);

	void OnLobbyStatisticsUpdate();

	void OnPhotonPlayerActivityChanged(PhotonPlayer otherPlayer);

	void OnOwnershipTransfered(object[] viewAndPlayers);
}

public interface IPunPrefabPool
{
	GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation);

	void Destroy(GameObject gameObject);
}

namespace Photon
{
	public class MonoBehaviour : UnityEngine.MonoBehaviour
	{
		private PhotonView pvCache;

		public PhotonView photonView
		{
			get
			{
				if (pvCache == null)
				{
					pvCache = PhotonView.Get(this);
				}
				return pvCache;
			}
		}

		[Obsolete("Use a photonView")]
		public new PhotonView networkView
		{
			get
			{
				Debug.LogWarning("Why are you still using networkView? should be PhotonView?");
				return PhotonView.Get(this);
			}
		}
	}
	
	public class PunBehaviour : MonoBehaviour, IPunCallbacks
	{
		public virtual void OnConnectedToPhoton()
		{
		}

		public virtual void OnLeftRoom()
		{
		}

		public virtual void OnMasterClientSwitched(PhotonPlayer newMasterClient)
		{
		}

		public virtual void OnPhotonCreateRoomFailed(object[] codeAndMsg)
		{
		}

		public virtual void OnPhotonJoinRoomFailed(object[] codeAndMsg)
		{
		}

		public virtual void OnCreatedRoom()
		{
		}

		public virtual void OnJoinedLobby()
		{
		}

		public virtual void OnLeftLobby()
		{
		}

		public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
		}

		public virtual void OnDisconnectedFromPhoton()
		{
		}

		public virtual void OnConnectionFail(DisconnectCause cause)
		{
		}

		public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
		{
		}

		public virtual void OnReceivedRoomListUpdate()
		{
		}

		public virtual void OnJoinedRoom()
		{
		}

		public virtual void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
		{
		}

		public virtual void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
		{
		}

		public virtual void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
		}

		public virtual void OnConnectedToMaster()
		{
		}

		public virtual void OnPhotonMaxCccuReached()
		{
		}

		public virtual void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
		{
		}

		public virtual void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
		{
		}

		public virtual void OnUpdatedFriendList()
		{
		}

		public virtual void OnCustomAuthenticationFailed(string debugMessage)
		{
		}

		public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
		}

		public virtual void OnWebRpcResponse(OperationResponse response)
		{
		}

		public virtual void OnOwnershipRequest(object[] viewAndPlayer)
		{
		}

		public virtual void OnLobbyStatisticsUpdate()
		{
		}

		public virtual void OnPhotonPlayerActivityChanged(PhotonPlayer otherPlayer)
		{
		}

		public virtual void OnOwnershipTransfered(object[] viewAndPlayers)
		{
		}
	}
}

public struct PhotonMessageInfo
{
	private readonly int timeInt;

	public readonly PhotonPlayer sender;

	public readonly PhotonView photonView;

	public double timestamp
	{
		get
		{
			uint num = (uint)timeInt;
			double num2 = num;
			return num2 / 1000.0;
		}
	}

	public PhotonMessageInfo(PhotonPlayer player, int timestamp, PhotonView view)
	{
		sender = player;
		timeInt = timestamp;
		photonView = view;
	}

	public override string ToString()
	{
		return string.Format("[PhotonMessageInfo: Sender='{1}' Senttime={0}]", timestamp, sender);
	}
}

internal class PunEvent
{
	public const byte RPC = 200;

	public const byte SendSerialize = 201;

	public const byte Instantiation = 202;

	public const byte CloseConnection = 203;

	public const byte Destroy = 204;

	public const byte RemoveCachedRPCs = 205;

	public const byte SendSerializeReliable = 206;

	public const byte DestroyPlayer = 207;

	public const byte AssignMaster = 208;

	public const byte OwnershipRequest = 209;

	public const byte OwnershipTransfer = 210;

	public const byte VacantViewIds = 211;
}

public class PhotonStream
{
	private bool write;

	private Queue<object> writeData;

	private object[] readData;

	internal byte currentItem;

	public bool isWriting
	{
		get
		{
			return write;
		}
	}

	public bool isReading
	{
		get
		{
			return !write;
		}
	}

	public int Count
	{
		get
		{
			return (!isWriting) ? readData.Length : writeData.Count;
		}
	}

	public PhotonStream(bool write, object[] incomingData)
	{
		this.write = write;
		if (incomingData == null)
		{
			writeData = new Queue<object>(10);
		}
		else
		{
			readData = incomingData;
		}
	}

	public void SetReadStream(object[] incomingData, byte pos = 0)
	{
		readData = incomingData;
		currentItem = pos;
		write = false;
	}

	internal void ResetWriteStream()
	{
		writeData.Clear();
	}

	public object ReceiveNext()
	{
		if (write)
		{
			Debug.LogError("Error: you cannot read this stream that you are writing!");
			return null;
		}
		object result = readData[currentItem];
		currentItem++;
		return result;
	}

	public object PeekNext()
	{
		if (write)
		{
			Debug.LogError("Error: you cannot read this stream that you are writing!");
			return null;
		}
		return readData[currentItem];
	}

	public void SendNext(object obj)
	{
		if (!write)
		{
			Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
		}
		else
		{
			writeData.Enqueue(obj);
		}
	}

	public object[] ToArray()
	{
		return (!isWriting) ? readData : writeData.ToArray();
	}

	public void Serialize(ref bool myBool)
	{
		if (write)
		{
			writeData.Enqueue(myBool);
		}
		else if (readData.Length > currentItem)
		{
			myBool = (bool)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref int myInt)
	{
		if (write)
		{
			writeData.Enqueue(myInt);
		}
		else if (readData.Length > currentItem)
		{
			myInt = (int)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref string value)
	{
		if (write)
		{
			writeData.Enqueue(value);
		}
		else if (readData.Length > currentItem)
		{
			value = (string)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref char value)
	{
		if (write)
		{
			writeData.Enqueue(value);
		}
		else if (readData.Length > currentItem)
		{
			value = (char)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref short value)
	{
		if (write)
		{
			writeData.Enqueue(value);
		}
		else if (readData.Length > currentItem)
		{
			value = (short)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref float obj)
	{
		if (write)
		{
			writeData.Enqueue(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (float)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref PhotonPlayer obj)
	{
		if (write)
		{
			writeData.Enqueue(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (PhotonPlayer)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref Vector3 obj)
	{
		if (write)
		{
			writeData.Enqueue(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (Vector3)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref Vector2 obj)
	{
		if (write)
		{
			writeData.Enqueue(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (Vector2)readData[currentItem];
			currentItem++;
		}
	}

	public void Serialize(ref Quaternion obj)
	{
		if (write)
		{
			writeData.Enqueue(obj);
		}
		else if (readData.Length > currentItem)
		{
			obj = (Quaternion)readData[currentItem];
			currentItem++;
		}
	}
}

public class SceneManagerHelper
{
	public static string ActiveSceneName
	{
		get
		{
			return LevelManager.GetSceneName();
		}
	}

	public static int ActiveSceneBuildIndex
	{
		get
		{
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
		}
	}

#if UNITY_EDITOR
	public static string EditorActiveSceneName
	{
		get
		{
#if UNITY_MIN_5_3
            return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
#else
			return System.IO.Path.GetFileNameWithoutExtension(UnityEditor.EditorApplication.currentScene);
#endif
		}
	}
#endif
}

public class WebRpcResponse
{
	public string Name { get; private set; }

	public int ReturnCode { get; private set; }

	public string DebugMessage { get; private set; }

	public Dictionary<string, object> Parameters { get; private set; }

	public WebRpcResponse(OperationResponse response)
	{
		object value;
		response.Parameters.TryGetValue(209, out value);
		Name = value as string;
		response.Parameters.TryGetValue(207, out value);
		ReturnCode = ((value == null) ? (-1) : ((byte)value));
		response.Parameters.TryGetValue(208, out value);
		Parameters = value as Dictionary<string, object>;
		response.Parameters.TryGetValue(206, out value);
		DebugMessage = value as string;
	}

	public string ToStringFull()
	{
		return string.Format("{0}={2}: {1} \"{3}\"", Name, SupportClass.DictionaryToString(Parameters), ReturnCode, DebugMessage);
	}
}
