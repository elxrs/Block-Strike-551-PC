using System.Collections.Generic;
using UnityEngine;

public class MurderModeManager : MonoBehaviour
{
	public GameObject Pistol;

	private bool canPickupPistol = true;

	private static MurderModeManager instance;

	private void Awake()
	{
		if (PhotonNetwork.offlineMode)
		{
			Destroy(this);
		}
		else if (PhotonNetwork.room.GetGameMode() != GameMode.Murder)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
		}
	}

	private void Start()
	{
		PhotonEvent.AddListener(1, PhotonMasterPickupPistol);
		PhotonEvent.AddListener(2, PhotonPickupPistol);
		PhotonEvent.AddListener(3, PhotonSetPosition);
		EventManager.AddListener("StartRound", StartRound);
	}

	private void StartRound()
	{
		Pistol.SetActive(false);
		canPickupPistol = true;
	}

	public static void SetRandomPlayerPistol()
	{
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			if (!PhotonNetwork.playerList[i].GetDead() && PhotonNetwork.playerList[i].ID != MurderMode.Murder)
			{
				list.Add(PhotonNetwork.playerList[i]);
			}
		}
		if (list.Count > nValue.int0)
		{
			PhotonPlayer photonPlayer = list[Random.Range(nValue.int0, list.Count)];
			PhotonEvent.RPC(2, PhotonTargets.All, photonPlayer.ID);
		}
	}

	public static void DeadPlayer()
	{
		if (PhotonNetwork.player.ID == MurderMode.Detective)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(PlayerInput.instance.PlayerTransform.position, Vector3.down, out hitInfo, nValue.int50))
			{
				PhotonEvent.RPC(3, PhotonTargets.All, hitInfo.point, hitInfo.normal);
			}
			else
			{
				SetRandomPlayerPistol();
			}
		}
	}

	public static void DeadBystander()
	{
		instance.canPickupPistol = false;
		TimerManager.In(30f, delegate
		{
			instance.canPickupPistol = true;
		});
		RaycastHit hitInfo;
		if (Physics.Raycast(PlayerInput.instance.PlayerTransform.position, Vector3.down, out hitInfo, nValue.int50))
		{
			PhotonEvent.RPC(3, PhotonTargets.All, hitInfo.point, hitInfo.normal);
		}
		else
		{
			SetRandomPlayerPistol();
		}
	}

	private void PhotonSetPosition(PhotonEventData data)
	{
		MurderMode.Detective = -1;
		SetPosition((Vector3)data.parameters[nValue.int0], (Vector3)data.parameters[nValue.int1]);
	}

	public static void SetPosition(Vector3 pos, Vector3 normal)
	{
		instance.Pistol.transform.position = pos + normal * nValue.float001;
		instance.Pistol.transform.rotation = Quaternion.LookRotation(normal);
		instance.Pistol.SetActive(true);
	}

	public static void SetPosition(Vector3 pos)
	{
		instance.Pistol.transform.position = pos;
		instance.Pistol.transform.rotation = Quaternion.Euler(-nValue.int90, Random.Range(nValue.int0, nValue.int360), nValue.int0);
		instance.Pistol.SetActive(true);
	}

	public void OnTriggerEnterPistol()
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound && canPickupPistol && PhotonNetwork.player.ID != MurderMode.Murder)
		{
			PhotonEvent.RPC(1, PhotonTargets.MasterClient);
		}
	}

	private void PhotonMasterPickupPistol(PhotonEventData data)
	{
		if (Pistol.activeSelf)
		{
			Pistol.SetActive(false);
			PhotonEvent.RPC(2, PhotonTargets.All, data.senderID);
		}
	}

	private void PhotonPickupPistol(PhotonEventData data)
	{
		Pistol.SetActive(false);
		int num = (MurderMode.Detective = (int)data.parameters[0]);
		if (PhotonNetwork.player.ID == num)
		{
			WeaponManager.SetSelectWeapon(WeaponType.Knife, 0);
			WeaponManager.SetSelectWeapon(WeaponType.Pistol, 2);
			WeaponManager.SetSelectWeapon(WeaponType.Rifle, 47);
			WeaponCustomData weaponCustomData = new WeaponCustomData();
			weaponCustomData.Ammo = 1;
			weaponCustomData.AmmoTotal = 1;
			weaponCustomData.AmmoMax = 99;
			weaponCustomData.BodyDamage = 100;
			weaponCustomData.FaceDamage = 100;
			weaponCustomData.HandDamage = 100;
			weaponCustomData.LegDamage = 100;
			weaponCustomData.Skin = 0;
			weaponCustomData.FireStat = false;
			weaponCustomData.CustomData = false;
			PlayerInput.instance.PlayerWeapon.UpdateWeaponAll(WeaponType.Rifle, null, weaponCustomData, null);
		}
	}
}
