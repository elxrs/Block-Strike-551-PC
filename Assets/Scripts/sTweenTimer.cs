using Photon;
using UnityEngine;

public class sTweenTimer : PunBehaviour
{
	private static float delay;

	public static float time
	{
		get
		{
			return delay + Time.timeSinceLevelLoad;
		}
	}

	private void Start()
	{
		PhotonClassesManager.Add(this);
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (PhotonNetwork.isMasterClient)
		{
			TimerManager.In(1.5f, delegate
			{
				photonView.RPC("PhotonSendTime", playerConnect, time);
			});
		}
	}

	[PunRPC]
	private void PhotonSendTime(float t, PhotonMessageInfo info)
	{
		delay = t;
		delay += (float)(PhotonNetwork.time - info.timestamp);
	}
}
