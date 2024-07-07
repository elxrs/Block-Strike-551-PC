using System.Collections.Generic;
using UnityEngine;

public class DropWeaponManager : Photon.MonoBehaviour
{
	private List<DropWeapon> list = new List<DropWeapon>();

	private static DropWeaponManager instance;

	private void Start()
	{
		instance = this;
		PhotonEvent.AddListener(PhotonEventTag.CreateWeapon, PhotonCreateWeapon);
		PhotonEvent.AddListener(161, PhotonMasterPickupWeapon);
		PhotonEvent.AddListener(162, PhotonPickupWeapon);
	}

	public static void ClearScene()
	{
		for (int i = 0; i < instance.list.Count; i++)
		{
			if (instance.list[i].Weapon.activeSelf)
			{
				instance.list[i].DestroyWeapon();
			}
		}
	}

	public static void CreateWeapon(bool local, byte weaponID, byte weaponSkin, bool checkPos, Vector3 pos, Vector3 rot, byte[] stickers, int firestat, int ammo, int ammoMax)
	{
		if (checkPos)
		{
			RaycastHit hitInfo;
			if (!Physics.Raycast(pos, Vector3.down, out hitInfo, 50f))
			{
				return;
			}
			pos = hitInfo.point + hitInfo.normal * 0.01f;
			rot = Quaternion.LookRotation(hitInfo.normal).eulerAngles;
		}
		if (local)
		{
			PhotonEvent.RPC(PhotonEventTag.CreateWeapon, true, PhotonNetwork.player, weaponID, weaponSkin, pos, rot, stickers, firestat, ammo, ammoMax);
		}
		else
		{
			PhotonEvent.RPC(PhotonEventTag.CreateWeapon, true, PhotonTargets.All, weaponID, weaponSkin, pos, rot, stickers, firestat, ammo, ammoMax);
		}
	}

	private void PhotonCreateWeapon(PhotonEventData data)
	{
		byte b = (byte)data.parameters[0];
		byte b2 = (byte)data.parameters[1];
		Vector3 position = (Vector3)data.parameters[2];
		Vector3 eulerAngles = (Vector3)data.parameters[3];
		byte[] stickers = (byte[])data.parameters[4];
		int num = (int)data.parameters[5];
		int num2 = (int)data.parameters[6];
		int num3 = (int)data.parameters[7];
		if (WeaponManager.GetWeaponData(b).Type != WeaponType.Knife)
		{
			GameObject gameObject = PoolManager.Spawn(b + "-Drop", GameSettings.instance.Weapons[b - 1].DropPrefab);
			DropWeapon component = gameObject.GetComponent<DropWeapon>();
			gameObject.transform.position = position;
			gameObject.transform.eulerAngles = eulerAngles;
			component.ID = (int)(data.timestamp * 100.0);
			component.Data.Ammo = num2;
			component.Data.AmmoMax = num3;
			component.Data.Skin = b2;
			component.Data.FireStat = num >= 0;
			component.Data.FireStatCounter = num;
			component.Data.Stickers = ConvertStickers(stickers);
			component.DestroyDrop = true;
			component.CustomData = true;
			component.UpdateWeapon();
			if (!list.Contains(component))
			{
				list.Add(component);
			}
		}
	}

	public static void PickupWeapon(int id)
	{
		PhotonEvent.RPC(161, PhotonTargets.MasterClient, id);
	}

	private void PhotonMasterPickupWeapon(PhotonEventData data)
	{
		int num = (int)data.parameters[0];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].ID == num && list[i].Weapon.activeSelf)
			{
				PhotonEvent.RPC(162, PhotonTargets.All, data.senderID, num);
				break;
			}
		}
	}

	private void PhotonPickupWeapon(PhotonEventData data)
	{
		int num = (int)data.parameters[0];
		int id = (int)data.parameters[1];
		DropWeapon weapon = GetWeapon(id);
		if (!(weapon == null))
		{
			if (PhotonNetwork.player.ID == num)
			{
				weapon.OnDropWeapon();
			}
			else
			{
				weapon.DestroyWeapon();
			}
		}
	}

	private DropWeapon GetWeapon(int id)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].ID == id)
			{
				return list[i];
			}
		}
		return null;
	}

	private CryptoInt[] ConvertStickers(byte[] stickers)
	{
		if (stickers == null)
		{
			return new CryptoInt[0];
		}
		CryptoInt[] array = new CryptoInt[stickers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = stickers[i];
		}
		return array;
	}
}
