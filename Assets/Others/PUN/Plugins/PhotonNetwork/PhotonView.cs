using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum ViewSynchronization
{
	Off,
	ReliableDeltaCompressed,
	Unreliable,
	UnreliableOnChange
}

public enum OnSerializeTransform
{
	OnlyPosition,
	OnlyRotation,
	OnlyScale,
	PositionAndRotation,
	All
}

public enum OnSerializeRigidBody
{
	OnlyVelocity,
	OnlyAngularVelocity,
	All
}

public enum OwnershipOption
{
	Fixed,
	Takeover,
	Request
}

[AddComponentMenu("Photon Networking/Photon View &v")]
public class PhotonView : Photon.MonoBehaviour
{
	public struct RpcEvent
	{
		public string key;

		public bool info;

		public Delegate _delegate;

		public RpcEvent(string k, bool i, Delegate d)
		{
			key = k;
			info = i;
			_delegate = d;
		}
	}

	public delegate void Callback();

	public delegate void Callback2(object[] arg1);

	public delegate void Callback3(PhotonMessageInfo arg1);

	public delegate void Callback4(object[] arg1, PhotonMessageInfo arg2);

	public delegate void PhotonSerializeView(PhotonStream arg1, PhotonMessageInfo arg2);

	public int ownerId;

	public byte group;

	protected internal bool mixedModeIsReliable;

	public IPunObservable[] punObservables = new IPunObservable[0];

	public RpcEvent[] rpcEvents = new RpcEvent[0];

	public bool OwnerShipWasTransfered;

	public int prefixBackup = -1;

	internal object[] instantiationDataField;

	protected internal object[] lastOnSerializeDataSent;

	protected internal object[] lastOnSerializeDataReceived;

	public ViewSynchronization synchronization;

	public OnSerializeTransform onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;

	public OnSerializeRigidBody onSerializeRigidBodyOption = OnSerializeRigidBody.All;

	public OwnershipOption ownershipTransfer;

	public List<Component> ObservedComponents;

	private Dictionary<Component, MethodInfo> m_OnSerializeMethodInfos = new Dictionary<Component, MethodInfo>(3);

	[SerializeField]
	private int viewIdField;

	public int instantiationId;

	public int currentMasterID = -1;

	protected internal bool didAwake;

	[SerializeField]
	protected internal bool isRuntimeInstantiated;

	protected internal bool removedFromLocalViewList;

	internal MonoBehaviour[] RpcMonoBehaviours;

	private MethodInfo OnSerializeMethodInfo;

	private bool failedToFindOnSerialize;

	public int prefix
	{
		get
		{
			if (prefixBackup == -1 && PhotonNetwork.networkingPeer != null)
			{
				prefixBackup = PhotonNetwork.networkingPeer.currentLevelPrefix;
			}
			return prefixBackup;
		}
		set
		{
			prefixBackup = value;
		}
	}

	public object[] instantiationData
	{
		get
		{
			if (!didAwake)
			{
				instantiationDataField = PhotonNetwork.networkingPeer.FetchInstantiationData(instantiationId);
			}
			return instantiationDataField;
		}
		set
		{
			instantiationDataField = value;
		}
	}

	public int viewID
	{
		get
		{
			return viewIdField;
		}
		set
		{
			bool flag = didAwake && viewIdField == 0;
			ownerId = value / PhotonNetwork.MAX_VIEW_IDS;
			viewIdField = value;
			if (flag)
			{
				PhotonNetwork.networkingPeer.RegisterPhotonView(this);
			}
		}
	}

	public bool isSceneView
	{
		get
		{
			return CreatorActorNr == 0;
		}
	}

	public PhotonPlayer owner
	{
		get
		{
			return PhotonPlayer.Find(ownerId);
		}
	}

	public int OwnerActorNr
	{
		get
		{
			return ownerId;
		}
	}

	public bool isOwnerActive
	{
		get
		{
			return ownerId != 0 && PhotonNetwork.networkingPeer.mActors.ContainsKey(ownerId);
		}
	}

	public int CreatorActorNr
	{
		get
		{
			return viewIdField / PhotonNetwork.MAX_VIEW_IDS;
		}
	}

	public bool isMine
	{
		get
		{
			return ownerId == PhotonNetwork.player.ID || (!isOwnerActive && PhotonNetwork.isMasterClient);
		}
	}

	protected internal void Awake()
	{
		if (viewID != 0)
		{
			PhotonNetwork.networkingPeer.RegisterPhotonView(this);
			instantiationDataField = PhotonNetwork.networkingPeer.FetchInstantiationData(instantiationId);
		}
		didAwake = true;
	}

	public void RequestOwnership()
	{
		PhotonNetwork.networkingPeer.RequestOwnership(viewID, ownerId);
	}

	public void TransferOwnership(PhotonPlayer newOwner)
	{
		TransferOwnership(newOwner.ID);
	}

	public void TransferOwnership(int newOwnerId)
	{
		PhotonNetwork.networkingPeer.TransferOwnership(viewID, newOwnerId);
		ownerId = newOwnerId;
	}

	public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		if (CreatorActorNr == 0 && !OwnerShipWasTransfered && (currentMasterID == -1 || ownerId == currentMasterID))
		{
			ownerId = newMasterClient.ID;
		}
		currentMasterID = newMasterClient.ID;
	}

	protected internal void OnDestroy()
	{
		if (!removedFromLocalViewList)
		{
			bool flag = PhotonNetwork.networkingPeer.LocalCleanPhotonView(this);
			bool flag2 = false;
			flag2 = Application.isLoadingLevel;
			if (flag && !flag2 && instantiationId > 0 && !PhotonHandler.AppQuits && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log("PUN-instantiated '" + gameObject.name + "' got destroyed by engine. This is OK when loading levels. Otherwise use: PhotonNetwork.Destroy().");
			}
		}
	}

	public void SerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		for (int i = 0; i < punObservables.Length; i++)
		{
			punObservables[i].OnPhotonSerializeView(stream, info);
		}
	}

	public void DeserializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		for (int i = 0; i < punObservables.Length; i++)
		{
			punObservables[i].OnPhotonSerializeView(stream, info);
		}
	}

	protected internal void DeserializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component == null)
		{
			return;
		}
		if (component is MonoBehaviour)
		{
			ExecuteComponentOnSerialize(component, stream, info);
		}
		else if (component is Transform)
		{
			Transform transform = (Transform)component;
			switch (onSerializeTransformOption)
			{
			case OnSerializeTransform.All:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				transform.localScale = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeTransform.OnlyPosition:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeTransform.OnlyRotation:
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				break;
			case OnSerializeTransform.OnlyScale:
				transform.localScale = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeTransform.PositionAndRotation:
				transform.localPosition = (Vector3)stream.ReceiveNext();
				transform.localRotation = (Quaternion)stream.ReceiveNext();
				break;
			}
		}
		else if (component is Rigidbody)
		{
			Rigidbody rigidbody = (Rigidbody)component;
			switch (onSerializeRigidBodyOption)
			{
			case OnSerializeRigidBody.All:
				rigidbody.velocity = (Vector3)stream.ReceiveNext();
				rigidbody.angularVelocity = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeRigidBody.OnlyAngularVelocity:
				rigidbody.angularVelocity = (Vector3)stream.ReceiveNext();
				break;
			case OnSerializeRigidBody.OnlyVelocity:
				rigidbody.velocity = (Vector3)stream.ReceiveNext();
				break;
			}
		}
		else if (component is Rigidbody2D)
		{
			Rigidbody2D rigidbody2D = (Rigidbody2D)component;
			switch (onSerializeRigidBodyOption)
			{
			case OnSerializeRigidBody.All:
				rigidbody2D.velocity = (Vector2)stream.ReceiveNext();
				rigidbody2D.angularVelocity = (float)stream.ReceiveNext();
				break;
			case OnSerializeRigidBody.OnlyAngularVelocity:
				rigidbody2D.angularVelocity = (float)stream.ReceiveNext();
				break;
			case OnSerializeRigidBody.OnlyVelocity:
				rigidbody2D.velocity = (Vector2)stream.ReceiveNext();
				break;
			}
		}
		else
		{
			Debug.LogError("Type of observed is unknown when receiving.");
		}
	}

	protected internal void SerializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component == null)
		{
			return;
		}
		if (component is MonoBehaviour)
		{
			ExecuteComponentOnSerialize(component, stream, info);
		}
		else if (component is Transform)
		{
			Transform transform = (Transform)component;
			switch (onSerializeTransformOption)
			{
			case OnSerializeTransform.All:
				stream.SendNext(transform.localPosition);
				stream.SendNext(transform.localRotation);
				stream.SendNext(transform.localScale);
				break;
			case OnSerializeTransform.OnlyPosition:
				stream.SendNext(transform.localPosition);
				break;
			case OnSerializeTransform.OnlyRotation:
				stream.SendNext(transform.localRotation);
				break;
			case OnSerializeTransform.OnlyScale:
				stream.SendNext(transform.localScale);
				break;
			case OnSerializeTransform.PositionAndRotation:
				stream.SendNext(transform.localPosition);
				stream.SendNext(transform.localRotation);
				break;
			}
		}
		else if (component is Rigidbody)
		{
			Rigidbody rigidbody = (Rigidbody)component;
			switch (onSerializeRigidBodyOption)
			{
			case OnSerializeRigidBody.All:
				stream.SendNext(rigidbody.velocity);
				stream.SendNext(rigidbody.angularVelocity);
				break;
			case OnSerializeRigidBody.OnlyAngularVelocity:
				stream.SendNext(rigidbody.angularVelocity);
				break;
			case OnSerializeRigidBody.OnlyVelocity:
				stream.SendNext(rigidbody.velocity);
				break;
			}
		}
		else if (component is Rigidbody2D)
		{
			Rigidbody2D rigidbody2D = (Rigidbody2D)component;
			switch (onSerializeRigidBodyOption)
			{
			case OnSerializeRigidBody.All:
				stream.SendNext(rigidbody2D.velocity);
				stream.SendNext(rigidbody2D.angularVelocity);
				break;
			case OnSerializeRigidBody.OnlyAngularVelocity:
				stream.SendNext(rigidbody2D.angularVelocity);
				break;
			case OnSerializeRigidBody.OnlyVelocity:
				stream.SendNext(rigidbody2D.velocity);
				break;
			}
		}
		else
		{
			Debug.LogError("Observed type is not serializable: " + component.GetType());
		}
	}

	protected internal void ExecuteComponentOnSerialize(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		IPunObservable punObservable = component as IPunObservable;
		if (punObservable != null)
		{
			punObservable.OnPhotonSerializeView(stream, info);
		}
		else
		{
			if (!(component != null))
			{
				return;
			}
			MethodInfo value = null;
			if (!m_OnSerializeMethodInfos.TryGetValue(component, out value))
			{
				if (!NetworkingPeer.GetMethod(component as MonoBehaviour, PhotonNetworkingMessage.OnPhotonSerializeView.ToString(), out value))
				{
					Debug.LogError("The observed monobehaviour (" + component.name + ") of this PhotonView does not implement OnPhotonSerializeView()!");
					value = null;
				}
				m_OnSerializeMethodInfos.Add(component, value);
			}
			if (value != null)
			{
				value.Invoke(component, new object[2] { stream, info });
			}
		}
	}

	public void RefreshRpcMonoBehaviourCache()
	{
		RpcMonoBehaviours = GetComponents<MonoBehaviour>();
	}

	public void RPC(string methodName, PhotonTargets target, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, target, false, parameters);
	}

	public void RpcSecure(string methodName, PhotonTargets target, bool encrypt, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, target, encrypt, parameters);
	}

	public void RPC(string methodName, PhotonPlayer targetPlayer, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, targetPlayer, false, parameters);
	}

	public void RpcSecure(string methodName, PhotonPlayer targetPlayer, bool encrypt, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, targetPlayer, encrypt, parameters);
	}

	public static PhotonView Get(Component component)
	{
		return component.GetComponent<PhotonView>();
	}

	public static PhotonView Get(GameObject gameObj)
	{
		return gameObj.GetComponent<PhotonView>();
	}

	public static PhotonView Find(int viewID)
	{
		return PhotonNetwork.networkingPeer.GetPhotonView(viewID);
	}

	public override string ToString()
	{
		return string.Format("View ({3}){0} on {1} {2}", viewID, (!(gameObject != null)) ? "GO==null" : gameObject.name, (!isSceneView) ? string.Empty : "(scene)", prefix);
	}

	public void AddRpcEvent(Callback callback)
	{
		OnAddRpcEvent(callback.Method.Name, false, callback);
	}

	public void AddRpcEvent(Callback2 callback)
	{
		OnAddRpcEvent(callback.Method.Name, false, callback);
	}

	public void AddRpcEvent(Callback3 callback)
	{
		OnAddRpcEvent(callback.Method.Name, true, callback);
	}

	public void AddRpcEvent(Callback4 callback)
	{
		OnAddRpcEvent(callback.Method.Name, true, callback);
	}

	private void OnAddRpcEvent(string key, bool info, Delegate del)
	{
		RpcEvent rpcEvent = new RpcEvent(key, info, del);
		RpcEvent[] array = new RpcEvent[rpcEvents.Length + 1];
		for (int i = 0; i < rpcEvents.Length; i++)
		{
			array[i] = rpcEvents[i];
		}
		array[array.Length - 1] = rpcEvent;
		rpcEvents = array;
	}

	public bool DispatchRpcEvent(string key, object[] data, int senderID, int senderTime)
	{
		for (int i = 0; i < rpcEvents.Length; i++)
		{
			if (!(rpcEvents[i].key == key))
			{
				continue;
			}
			if (rpcEvents[i].info)
			{
				if (data.Length > 0)
				{
					((Callback4)rpcEvents[i]._delegate)(data, new PhotonMessageInfo(PhotonPlayer.Find(senderID), senderTime, this));
				}
				else
				{
					((Callback3)rpcEvents[i]._delegate)(new PhotonMessageInfo(PhotonPlayer.Find(senderID), senderTime, this));
				}
			}
			else if (data.Length > 0)
			{
				((Callback2)rpcEvents[i]._delegate)(data);
			}
			else
			{
				((Callback)rpcEvents[i]._delegate)();
			}
			return true;
		}
		return false;
	}

	public void AddPunObservable(IPunObservable observable)
	{
		IPunObservable[] array = new IPunObservable[punObservables.Length + 1];
		for (int i = 0; i < punObservables.Length; i++)
		{
			array[i] = punObservables[i];
		}
		array[array.Length - 1] = observable;
		punObservables = array;
	}
}
