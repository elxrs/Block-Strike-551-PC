using System.Collections.Generic;
using Photon;
using UnityEngine;

public class GrenadeObject : PunBehaviour, IPunObservable
{
	public static List<GrenadeObject> list = new List<GrenadeObject>();

	public float time = 3f;

	public float speed = 5f;

	public ParticleSystem effect;

	public Transform effectTransform;

	public GameObject cachedGameObject;

	public Transform cachedTransform;

	public Rigidbody cachedRigidbody;

	public GameObject cachedGameObjectModel;

	private Vector3 PhotonPosition = Vector3.zero;

	private Quaternion PhotonRotation = Quaternion.identity;

	private bool isMine;

	private bool isActive;

	private void Awake()
	{
		photonView.AddPunObservable(this);
	}

	private void Start()
	{
		isMine = photonView.isMine;
		cachedRigidbody.isKinematic = !photonView.isMine;
		cachedRigidbody.useGravity = photonView.isMine;
		if (isMine)
		{
			photonView.RPC("SetTime", PhotonTargets.All);
			PhotonPosition = cachedRigidbody.position;
			PhotonRotation = cachedRigidbody.rotation;
		}
	}

	private void OnEnable()
	{
		list.Add(this);
		isActive = true;
	}

	private void OnDisable()
	{
		list.Remove(this);
		isActive = false;
	}

	[PunRPC]
	private void SetTime(PhotonMessageInfo info)
	{
		if (isActive)
		{
			float num = time - (float)(PhotonNetwork.time - info.timestamp);
			if (num > 0f)
			{
				TimerManager.In(time - (float)(PhotonNetwork.time - info.timestamp), Bomb);
			}
			else
			{
				Clear();
			}
		}
		else
		{
			Clear();
		}
	}

	private void Bomb()
	{
		int num = (int)Vector3.Distance(PlayerInput.instance.PlayerTransform.position, cachedTransform.position);
		int value = (nValue.int12 - num) * nValue.int6;
		value = Mathf.Clamp(value, nValue.int0, nValue.int80);
		if (value > nValue.int0 && PlayerInput.instance.PlayerTeam != photonView.owner.GetTeam())
		{
			DamageInfo damageInfo = DamageInfo.Get(value, Vector3.zero, photonView.owner.GetTeam(), 46, nValue.int0, photonView.owner.ID, false);
			PlayerInput.instance.Damage(damageInfo);
			PlayerInput.instance.FPCamera.AddRollForce(Random.Range(-value * 0.03f, value * 0.03f));
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != this && list[i].photonView.isMine)
			{
				list[i].cachedRigidbody.AddExplosionForce(150f, cachedTransform.position, 15f);
			}
		}
		cachedGameObjectModel.SetActive(false);
		effectTransform.eulerAngles = new Vector3(-90f, 0f, 0f);
		effect.Play(true);
		SoundClip soundClip = SoundManager.Play3D("Explosion", cachedTransform.position);
		if (soundClip != null)
		{
			soundClip.GetSource().rolloffMode = AudioRolloffMode.Linear;
		}
		TimerManager.In(1f, Clear);
	}

	private void Clear()
	{
		if (isActive && photonView.isMine)
		{
			PhotonNetwork.Destroy(cachedGameObject);
		}
	}

	private void Update()
	{
		if (!isMine)
		{
			cachedRigidbody.MovePosition(Vector3.Lerp(cachedRigidbody.position, PhotonPosition, Time.deltaTime * speed));
			cachedRigidbody.MoveRotation(Quaternion.Lerp(cachedRigidbody.rotation, PhotonRotation, Time.deltaTime * speed));
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(cachedRigidbody.position);
			stream.SendNext(cachedRigidbody.rotation);
		}
		else
		{
			PhotonPosition = (Vector3)stream.ReceiveNext();
			PhotonRotation = (Quaternion)stream.ReceiveNext();
		}
	}
}
