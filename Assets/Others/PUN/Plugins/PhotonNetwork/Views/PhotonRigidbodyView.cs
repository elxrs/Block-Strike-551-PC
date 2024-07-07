using UnityEngine;

[AddComponentMenu("Photon Networking/Photon Rigidbody View")]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]
public class PhotonRigidbodyView : MonoBehaviour, IPunObservable
{
	private float m_Distance;

	private float m_Angle;

	private Rigidbody m_Body;

	private PhotonView m_PhotonView;

	private Vector3 m_NetworkPosition;

	private Quaternion m_NetworkRotation;

	private float sendRate = 0.1f;

	public bool m_SynchronizeVelocity = true;

	public bool m_SynchronizeAngularVelocity;

	public void Awake()
	{
		m_Body = GetComponent<Rigidbody>();
		m_PhotonView = GetComponent<PhotonView>();
		m_NetworkPosition = default(Vector3);
		m_NetworkRotation = default(Quaternion);
		sendRate = 1f / PhotonNetwork.sendRateOnSerialize;
	}

	public void FixedUpdate()
	{
		if (!m_PhotonView.isMine)
		{
			m_Body.position = Vector3.MoveTowards(m_Body.position, m_NetworkPosition, m_Distance * sendRate);
			m_Body.rotation = Quaternion.RotateTowards(m_Body.rotation, m_NetworkRotation, m_Angle * sendRate);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(m_Body.position);
			stream.SendNext(m_Body.rotation);
			if (m_SynchronizeVelocity)
			{
				stream.SendNext(m_Body.velocity);
			}
			if (m_SynchronizeAngularVelocity)
			{
				stream.SendNext(m_Body.angularVelocity);
			}
			return;
		}
		m_NetworkPosition = (Vector3)stream.ReceiveNext();
		m_NetworkRotation = (Quaternion)stream.ReceiveNext();
		if (m_SynchronizeVelocity || m_SynchronizeAngularVelocity)
		{
			float num = Mathf.Abs((float)(PhotonNetwork.time - info.timestamp));
			if (m_SynchronizeVelocity)
			{
				m_Body.velocity = (Vector3)stream.ReceiveNext();
				m_NetworkPosition += m_Body.velocity * num;
				m_Distance = Vector3.Distance(m_Body.position, m_NetworkPosition);
			}
			if (m_SynchronizeAngularVelocity)
			{
				m_Body.angularVelocity = (Vector3)stream.ReceiveNext();
				m_NetworkRotation = Quaternion.Euler(m_Body.angularVelocity * num) * m_NetworkRotation;
				m_Angle = Quaternion.Angle(m_Body.rotation, m_NetworkRotation);
			}
		}
	}
}
