using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

public class PlayerAI2 : PunBehaviour, IPunObservable
{
	public static List<PlayerAI2> list = new List<PlayerAI2>();

	public Transform target;

	public int health = 1000;

	public bool offlineMode;

	public bool dead;

	public int attackDamage = 20;

	public float attackDistance = 5f;

	public float attackWalkDistance = 1.5f;

	public float attackIdleDistance = 3f;

	public float attackSpeed = 0.5f;

	public Vector3 attackPosition;

	public float checkPlayers = 1.5f;

	public UnityEngine.AI.NavMeshAgent nav;

	public PlayerAnimator playerAnimator;

	public GameObject attackEffect;

	public float photonSpeed = 11f;

	private Vector3 photonPosition = Vector3.zero;

	private Quaternion photonRotation = Quaternion.identity;

	public Transform head;

	public float rotateAngle = 5f;

	public nTimer Timer;

	private Vector3 customVector3;

	private bool update1;

	private Transform cachedTransform;

	private bool isJump;

	public static event Action<PlayerAI2> startEvent;

	public static event Action<PlayerAI2> deadEvent;

	private void Start()
	{
		PhotonClassesManager.Add(this);
		cachedTransform = transform;
		playerAnimator.grounded = true;
		playerAnimator.SetWeapon(WeaponType.Knife);
		nav.enabled = photonView.isMine || offlineMode;
		Timer.In(attackSpeed + UnityEngine.Random.Range(-0.25f, 0.25f), true, CheckDistance);
		Timer.In(0.1f, true, UpdateNavigator);
		Timer.In(checkPlayers, true, CheckPlayers);
	}

	private void OnEnable()
	{
		list.Add(this);
		if (startEvent != null)
		{
			startEvent(this);
		}
	}

	private void OnDisable()
	{
		list.Remove(this);
	}

	private void UpdateNavigator()
	{
		if (!photonView.isMine && !offlineMode)
		{
			return;
		}
		if (target == null)
		{
			if (PlayerInput.instance != null)
			{
				target = PlayerInput.instance.PlayerTransform;
			}
			return;
		}
		if (nav.isActiveAndEnabled)
		{
			print("asd");
			nav.SetDestination(target.position);
		}
		playerAnimator.move = Mathf.Clamp01(nav.velocity.sqrMagnitude);
		nav.updateRotation = (target.position - cachedTransform.position).sqrMagnitude >= 10f;
	}

	private void Update()
	{
		if (photonView.isMine || offlineMode)
		{
			if (target != null)
			{
				if (!nav.updateRotation)
				{
					customVector3 = target.position - cachedTransform.position;
					customVector3.y = 0f;
					cachedTransform.rotation = Quaternion.LookRotation(customVector3);
				}
				customVector3 = target.position - cachedTransform.position;
				playerAnimator.rotate = customVector3.y / customVector3.sqrMagnitude * rotateAngle;
				if (nav.isOnOffMeshLink && !isJump)
				{
					playerAnimator.grounded = false;
					isJump = true;
				}
				else if (!nav.isOnOffMeshLink && isJump)
				{
					playerAnimator.grounded = true;
					isJump = false;
				}
			}
		}
		else
		{
			cachedTransform.position = Vector3.Lerp(cachedTransform.position, photonPosition, Time.deltaTime * photonSpeed);
			cachedTransform.rotation = Quaternion.Lerp(cachedTransform.rotation, photonRotation, Time.deltaTime * photonSpeed);
		}
	}

	private void CheckDistance()
	{
		if (!photonView.isMine || target == null)
		{
			return;
		}
		if (nav.velocity.sqrMagnitude <= 0.1f)
		{
			if (Vector3.Distance(target.position, cachedTransform.position) <= attackIdleDistance)
			{
				photonView.RPC("PhotonFire", PhotonTargets.All);
			}
		}
		else if (Vector3.Distance(target.position, cachedTransform.position) <= attackWalkDistance)
		{
			photonView.RPC("PhotonFire", PhotonTargets.All);
		}
	}

	[PunRPC]
	private void PhotonFire()
	{
		attackEffect.SetActive(true);
		TimerManager.In(0.05f, delegate
		{
			attackEffect.SetActive(false);
		});
		RaycastHit hitInfo;
		if ((photonView.isMine || offlineMode) && Physics.Raycast(head.position + attackPosition, head.forward, out hitInfo, attackDistance) && hitInfo.transform == PlayerInput.instance.PlayerTransform)
		{
			DamageInfo damageInfo = DamageInfo.Get(attackDamage + UnityEngine.Random.Range(-5, 5), cachedTransform.position, Team.None, 0, 0, -1, false);
			PlayerInput.instance.Damage(damageInfo);
		}
	}

	public void Damage(DamageInfo damageInfo)
	{
		if (!dead)
		{
			photonView.RPC("PhotonDamage", PhotonTargets.All, damageInfo);
			UICrosshair.Hit();
		}
	}

	[PunRPC]
	private void PhotonDamage(DamageInfo damageInfo)
	{
		if (photonView.isMine || offlineMode)
		{
			health -= damageInfo.Damage;
			if (health <= 0)
			{
				dead = true;
				photonView.RPC("PhotonDead", PhotonTargets.All);
			}
		}
	}

	[PunRPC]
	private void PhotonDead()
	{
		health = 0;
		dead = true;
		PoolManager.Despawn("Zombie", gameObject);
		if (deadEvent != null)
		{
			deadEvent(this);
		}
	}

	private void CheckPlayers()
	{
		if (offlineMode || !photonView.isMine)
		{
			return;
		}
		ControllerManager controllerManager = null;
		float num = Vector3.Distance(target.position, cachedTransform.position) + 1f;
		float num2 = num;
		for (int i = 0; i < ControllerManager.ControllerList.Count; i++)
		{
			if (ControllerManager.ControllerList[i] != null && ControllerManager.ControllerList[i].PlayerSkin != null && !ControllerManager.ControllerList[i].PlayerSkin.Dead)
			{
				num2 = Vector3.Distance(ControllerManager.ControllerList[i].PlayerSkin.PlayerTransform.position, cachedTransform.position);
				if (num2 + 4f < num)
				{
					num = num2;
					controllerManager = ControllerManager.ControllerList[i];
				}
			}
		}
		if (!(controllerManager == null))
		{
			photonView.TransferOwnership(controllerManager.photonView.owner);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			photonPosition = cachedTransform.position;
			photonRotation = cachedTransform.rotation;
			stream.SendNext(cachedTransform.position);
			stream.SendNext(cachedTransform.rotation);
			stream.SendNext(nav.velocity.sqrMagnitude);
			stream.SendNext(health);
		}
		else
		{
			photonPosition = (Vector3)stream.ReceiveNext();
			photonRotation = (Quaternion)stream.ReceiveNext();
			playerAnimator.move = (float)stream.ReceiveNext();
			health = (int)stream.ReceiveNext();
		}
	}

	public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		nav.enabled = newMasterClient.ID == PhotonNetwork.player.ID;
	}

	public override void OnOwnershipTransfered(object[] viewAndPlayers)
	{
		nav.enabled = ((PhotonPlayer)viewAndPlayers[1]).ID == PhotonNetwork.player.ID;
	}
}
