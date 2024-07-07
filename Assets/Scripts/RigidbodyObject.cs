using Photon;
using UnityEngine;

public class RigidbodyObject : PunBehaviour, IPunObservable
{
	public Rigidbody mRigidbody;

	private Transform mTransform;

	public SyncBuffer syncBuffer;

	private float timeSinceLastSync;

	private Vector3 lastSentVelocity;

	private Vector3 lastSentPosition;

	private bool forceSync;

	private PhotonPlayer LastContactPlayer;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
		photonView.AddPunObservable(this);
		photonView.AddRpcEvent(PhotonForce);
		mTransform = transform;
	}

	public void Force(Vector3 force)
	{
		photonView.RPC("PhotonForce", PhotonTargets.MasterClient, force, PhotonNetwork.player);
	}

	public void Force(Vector3 force, PhotonPlayer player)
	{
		photonView.RPC("PhotonForce", PhotonTargets.MasterClient, force, player);
	}

	[PunRPC]
	private void PhotonForce(Vector3 force, PhotonPlayer player, PhotonMessageInfo info)
	{
		if (info.timestamp + 0.5 > PhotonNetwork.time)
		{
			LastContactPlayer = player;
			mRigidbody.AddForce(force);
		}
	}

	private void PhotonForce(object[] data, PhotonMessageInfo info)
	{
		PhotonForce((Vector3)data[0], (PhotonPlayer)data[1], info);
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		forceSync = true;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			if (forceSync || !(mRigidbody.velocity == lastSentVelocity) || !(mRigidbody.position == lastSentPosition))
			{
				forceSync = false;
				stream.SendNext(timeSinceLastSync);
				stream.SendNext(mRigidbody.position);
				stream.SendNext(mRigidbody.rotation);
				stream.SendNext(mRigidbody.velocity);
				lastSentVelocity = mRigidbody.velocity;
				lastSentPosition = mRigidbody.position;
				timeSinceLastSync = 0f;
			}
		}
		else
		{
			float interpolationTime = Mathf.Max((float)stream.ReceiveNext(), 0.001f);
			Vector3 position = (Vector3)stream.ReceiveNext();
			Quaternion rotation = (Quaternion)stream.ReceiveNext();
			Vector3 value = (Vector3)stream.ReceiveNext();
			syncBuffer.AddKeyframe(interpolationTime, position, value, new Vector3(), rotation, default(Vector3), default(Vector3));
		}
	}

	private void FixedUpdate()
	{
		if (photonView.isMine)
		{
			timeSinceLastSync += Time.deltaTime;
		}
	}

	private void Update()
	{
		if (!photonView.isMine && syncBuffer.HasKeyframes)
		{
			syncBuffer.UpdatePlayback(Time.deltaTime);
			mTransform.position = syncBuffer.Position;
			mTransform.rotation = syncBuffer.Rotation;
		}
	}

	public PhotonPlayer GetLastContactPlayer()
	{
		return LastContactPlayer;
	}
}
