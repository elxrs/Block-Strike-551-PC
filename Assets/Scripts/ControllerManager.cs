using System;
using System.Collections.Generic;
using Photon;
using UnityEngine;

public class ControllerManager : PunBehaviour, IPunObservable
{
	private Transform PlayerTransform;

	[HideInInspector]
	public PlayerInput PlayerInput;

	[HideInInspector]
	public PlayerSkin PlayerSkin;

	private Vector3 PlayerPositionWidth = new Vector3(nValue.int0, 0f - nValue.float008, nValue.int0);

	private int falsePositives;

	private byte pGroup;

	public static List<ControllerManager> ControllerList = new List<ControllerManager>();

	public static event Action<int, int, int> SetWeaponEvent;

	public static event Action<int, bool> SetDeadEvent;

	public static event Action<int, Team> SetTeamEvent;

	public static event Action<int, byte> SetHealthEvent;

	public static event Action<int> SetFireEvent;

	public static event Action<int> SetReloadEvent;

	private void Awake()
	{
		PhotonClassesManager.Add(this);
		photonView.AddPunObservable(this);
		photonView.AddRpcEvent(PhotonConnected);
		photonView.AddRpcEvent(PhotonSetWeapon);
		photonView.AddRpcEvent(PhotonFireWeapon);
		photonView.AddRpcEvent(PhotonReloadWeapon);
		photonView.AddRpcEvent(PhotonActivePlayer);
		photonView.AddRpcEvent(PhotonDeactivePlayer);
		photonView.AddRpcEvent(PhotonSetPosition);
		photonView.AddRpcEvent(PhotonSetRotation);
		photonView.AddRpcEvent(PhotonSpawnPlayer);
		photonView.AddRpcEvent(PhotonDamage);
		photonView.AddRpcEvent(PhotonSetHealth);
		photonView.AddRpcEvent(PhotonSetTeam);
		photonView.AddRpcEvent(PhotonUpdateFireStatValue);
		name = photonView.owner.NickName;
		if (photonView.isMine)
		{
			PlayerTransform = Utils.AddChild(GameSettings.instance.PlayerController, transform).transform;
			PlayerInput = PlayerTransform.GetComponent<PlayerInput>();
			PlayerInput.Controller = this;
		}
		else
		{
			PlayerTransform = Utils.AddChild(GameSettings.instance.PlayerSkin, transform).transform;
			PlayerSkin = PlayerTransform.GetComponent<PlayerSkin>();
			PlayerSkin.PlayerRagdoll = Utils.AddChild(GameSettings.instance.PlayerRagdoll, transform, new Vector3(nValue.int0, 2000f, photonView.ownerId)).GetComponent<RagdollObject>();
			PlayerSkin.PlayerRagdoll.defaultPosition = new Vector3(nValue.int0, 2000f, photonView.ownerId);
		}
	}

	private void OnEnable()
	{
		if (!photonView.isMine)
		{
			ControllerList.Add(this);
		}
	}

	private void OnDisable()
	{
		if (!photonView.isMine)
		{
			ControllerList.Remove(this);
		}
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer playerConnect)
	{
		if (photonView.isMine)
		{
			bool activeSelf = PlayerInput.gameObject.activeSelf;
			Team playerTeam = PlayerInput.PlayerTeam;
			int num = SaveLoadManager.GetPlayerSkinSelected(BodyParts.Head);
			int num2 = SaveLoadManager.GetPlayerSkinSelected(BodyParts.Body);
			int num3 = SaveLoadManager.GetPlayerSkinSelected(BodyParts.Legs);
			if (playerTeam == Team.Red && PlayerInput.Zombie)
			{
				num = 99;
				num2 = 99;
				num3 = 99;
			}
			byte b = (byte)(int)PlayerInput.Health;
			PlayerWeapons.PlayerWeaponData selectedWeaponData = PlayerInput.PlayerWeapon.GetSelectedWeaponData();
			int num4 = selectedWeaponData.ID;
			int num5 = selectedWeaponData.Skin;
			int num6 = selectedWeaponData.FireStat;
			byte[] array = Utils.SerializeWeaponStickers(num4, num5);
			pGroup = photonView.group;
			photonView.group = 0;
			photonView.RPC("PhotonConnected", playerConnect, activeSelf, b, (byte)num, (byte)num2, (byte)num3, (byte)playerTeam, (byte)num4, (byte)num5, num6, array);
			photonView.group = pGroup;
		}
	}

	[PunRPC]
	private void PhotonConnected(bool activePlayer, byte health, byte head, byte body, byte legs, byte team, byte weaponID, byte weaponSkin, int fireStat, byte[] stickers)
	{
		PlayerSkin.PlayerTeam = (Team)team;
		PlayerSkin.SetSkin(head, body, legs);
		PlayerSkin.Health = health;
		if (activePlayer)
		{
			PhotonActivePlayer(Vector3.zero, Vector3.zero);
			PlayerSkin.SetWeapon(WeaponManager.GetWeaponData(weaponID), weaponSkin, fireStat, stickers);
		}
		Invoke("UpdateAvatar", UnityEngine.Random.value * 2f);
	}

	private void PhotonConnected(object[] data)
	{
		PhotonConnected((bool)data[0], (byte)data[1], (byte)data[2], (byte)data[3], (byte)data[4], (byte)data[5], (byte)data[6], (byte)data[7], (int)data[8], (byte[])data[9]);
	}

	private void UpdateAvatar()
	{
		if (photonView != null)
		{
			AvatarManager.Load(photonView.owner.GetAvatarUrl());
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(PlayerTransform.position + PlayerPositionWidth);
			stream.SendNext(PlayerTransform.rotation);
			stream.SendNext((!(PlayerInput.MoveAxis.y < 0f)) ? PlayerInput.MoveAxis.magnitude : (0f - PlayerInput.MoveAxis.magnitude));
			stream.SendNext(PlayerInput.mCharacterController.isGrounded);
			stream.SendNext(PlayerInput.RotateCamera);
		}
		else
		{
			PlayerSkin.PhotonPosition = (Vector3)stream.ReceiveNext();
			PlayerSkin.PhotonRotation = (Quaternion)stream.ReceiveNext();
			PlayerSkin.SetMove((float)stream.ReceiveNext());
			PlayerSkin.SetGrounded((bool)stream.ReceiveNext());
			PlayerSkin.SetRotate((float)stream.ReceiveNext());
		}
	}

	public void SetWeapon(PlayerWeapons.PlayerWeaponData weaponData)
	{
		int num = weaponData.ID;
		int num2 = weaponData.Skin;
		int num3 = weaponData.FireStat;
		byte[] array = Utils.SerializeWeaponStickers(num, num2);
		pGroup = photonView.group;
		photonView.group = 0;
		photonView.RPC("PhotonSetWeapon", PhotonTargets.Others, (byte)num, (byte)num2, num3, array);
		photonView.group = pGroup;
	}

	[PunRPC]
	private void PhotonSetWeapon(byte weaponID, byte skinID, int firestat, byte[] stickers)
	{
		PlayerSkin.SetWeapon(WeaponManager.GetWeaponData(weaponID), skinID, firestat, stickers);
		if (SetWeaponEvent != null)
		{
			SetWeaponEvent(photonView.ownerId, weaponID, skinID);
		}
	}

	private void PhotonSetWeapon(object[] data)
	{
		PhotonSetWeapon((byte)data[0], (byte)data[1], (int)data[2], (byte[])data[3]);
	}

	public void FireWeapon(DecalInfo decalInfo)
	{
		photonView.RPC("PhotonFireWeapon", PhotonTargets.Others, decalInfo);
		DecalsManager.FireWeapon(decalInfo);
	}

	[PunRPC]
	private void PhotonFireWeapon(DecalInfo decalInfo)
	{
		PlayerSkin.Fire(decalInfo);
		if (SetFireEvent != null)
		{
			SetFireEvent(photonView.ownerId);
		}
	}

	private void PhotonFireWeapon(object[] data)
	{
		PhotonFireWeapon((DecalInfo)data[0]);
	}

	public void ReloadWeapon()
	{
		photonView.RPC("PhotonReloadWeapon", PhotonTargets.Others);
	}

	[PunRPC]
	public void PhotonReloadWeapon()
	{
		PlayerSkin.Reload();
		if (SetReloadEvent != null)
		{
			SetReloadEvent(photonView.ownerId);
		}
	}

	public void ActivePlayer(Vector3 pos, Vector3 rot)
	{
		if (photonView.isMine)
		{
			PlayerInput.FPController.Activate();
			PlayerInput.FPController.Stop();
			PlayerInput.FPController.SetPosition(pos);
			PlayerInput.FPCamera.SetRotation(rot);
			PhotonNetwork.player.SetDead(false);
		}
		pGroup = photonView.group;
		photonView.group = 0;
		photonView.RPC("PhotonActivePlayer", PhotonTargets.Others, pos, rot);
		photonView.group = pGroup;
	}

	[PunRPC]
	private void PhotonActivePlayer(Vector3 pos, Vector3 rot)
	{
		if (!photonView.isMine)
		{
			PlayerSkin.PlayerRagdoll.Deactive();
			PlayerSkin.SetPosition(pos);
			PlayerSkin.SetRotation(rot);
			PlayerSkin.Dead = false;
			PlayerSkin.PlayerRoot.SetActive(true);
			PlayerSkin.StartDamageTime();
			PlayerSkin.UpdateSkin();
			if (SetDeadEvent != null)
			{
				SetDeadEvent(photonView.ownerId, false);
			}
		}
	}

	private void PhotonActivePlayer(object[] data)
	{
		PhotonActivePlayer((Vector3)data[0], (Vector3)data[1]);
	}

	public void DeactivePlayer()
	{
		DeactivePlayer(Vector3.zero, false);
	}

	public void DeactivePlayer(Vector3 force)
	{
		DeactivePlayer(force, false);
	}

	public void DeactivePlayer(Vector3 force, bool headshot)
	{
		if (photonView.isMine)
		{
			PlayerInput.FPController.Deactivate();
			PhotonNetwork.player.SetDead(true);
		}
		pGroup = photonView.group;
		photonView.group = 0;
		photonView.RPC("PhotonDeactivePlayer", PhotonTargets.Others, force, headshot);
		photonView.group = pGroup;
	}

	[PunRPC]
	public void PhotonDeactivePlayer(Vector3 force, bool headshot)
	{
		if (!photonView.isMine)
		{
			PlayerSkin.ActiveRagdoll(force, headshot);
			PlayerSkin.Dead = true;
			if (SetDeadEvent != null)
			{
				SetDeadEvent(photonView.ownerId, true);
			}
		}
	}

	private void PhotonDeactivePlayer(object[] data)
	{
		PhotonDeactivePlayer((Vector3)data[0], (bool)data[1]);
	}

	public void SetPosition(Vector3 position)
	{
		photonView.RPC("PhotonSetPosition", PhotonTargets.All, position);
	}

	[PunRPC]
	private void PhotonSetPosition(Vector3 position)
	{
		if (photonView.isMine)
		{
			PlayerInput.FPController.Stop();
			PlayerInput.FPController.SetPosition(position);
		}
		else
		{
			PlayerSkin.SetPosition(position);
		}
	}

	private void PhotonSetPosition(object[] data)
	{
		PhotonSetPosition((Vector3)data[0]);
	}

	public void SetRotation(Vector3 rotation)
	{
		photonView.RPC("PhotonSetRotation", PhotonTargets.All, rotation);
	}

	[PunRPC]
	private void PhotonSetRotation(Vector3 rotation)
	{
		if (photonView.isMine)
		{
			PlayerInput.FPCamera.SetRotation(rotation);
		}
		else
		{
			PlayerSkin.SetRotation(rotation);
		}
	}

	private void PhotonSetRotation(object[] data)
	{
		PhotonSetRotation((Vector3)data[0]);
	}

	public void SpawnPlayer(Vector3 position, Vector3 rotation)
	{
		photonView.RPC("PhotonSpawnPlayer", PhotonTargets.All, position, rotation);
	}

	[PunRPC]
	private void PhotonSpawnPlayer(Vector3 position, Vector3 rotation)
	{
		if (photonView.isMine)
		{
			PlayerInput.FPController.Stop();
			PlayerInput.FPController.SetPosition(position);
			PlayerInput.FPCamera.SetRotation(rotation);
		}
		else
		{
			PlayerSkin.SetPosition(position);
			PlayerSkin.SetRotation(rotation);
		}
	}

	private void PhotonSpawnPlayer(object[] data)
	{
		PhotonSpawnPlayer((Vector3)data[0], (Vector3)data[1]);
	}

	public void Damage(DamageInfo damageInfo)
	{
		pGroup = photonView.group;
		photonView.group = 0;
		photonView.RPC("PhotonDamage", photonView.owner, damageInfo);
		photonView.group = pGroup;
	}

	[PunRPC]
	private void PhotonDamage(DamageInfo damageInfo, PhotonMessageInfo info)
	{
		if (info.timestamp + nValue.float05 > PhotonNetwork.time)
		{
			PlayerInput.Damage(damageInfo);
			if (falsePositives >= nValue.int0)
			{
				falsePositives--;
			}
		}
		else
		{
			falsePositives++;
			if (falsePositives >= nValue.int10)
			{
				PhotonNetwork.LeaveRoom();
			}
		}
	}

	private void PhotonDamage(object[] data, PhotonMessageInfo info)
	{
		PhotonDamage((DamageInfo)data[0], info);
	}

	public void SetHealth(byte health)
	{
		photonView.RPC("PhotonSetHealth", PhotonTargets.All, health);
	}

	[PunRPC]
	private void PhotonSetHealth(byte health)
	{
		if (!photonView.isMine)
		{
			PlayerSkin.Health = health;
			if (SetHealthEvent != null)
			{
				SetHealthEvent(photonView.ownerId, health);
			}
		}
	}

	private void PhotonSetHealth(object[] data)
	{
		PhotonSetHealth((byte)data[0]);
	}

	public void SetTeam(Team team)
	{
		photonView.owner.SetTeam(team);
		byte b = (byte)SaveLoadManager.GetPlayerSkinSelected(BodyParts.Head);
		byte b2 = (byte)SaveLoadManager.GetPlayerSkinSelected(BodyParts.Body);
		byte b3 = (byte)SaveLoadManager.GetPlayerSkinSelected(BodyParts.Legs);
		if (team == Team.Red && PlayerInput.Zombie)
		{
			b = 99;
			b2 = 99;
			b3 = 99;
		}
		pGroup = photonView.group;
		photonView.group = 0;
		photonView.RPC("PhotonSetTeam", PhotonTargets.All, (byte)team, b, b2, b3);
		photonView.group = pGroup;
	}

	[PunRPC]
	private void PhotonSetTeam(byte team, byte head, byte body, byte legs)
	{
		if (photonView.isMine)
		{
			PlayerInput.PlayerTeam = (Team)team;
			return;
		}
		PlayerSkin.PlayerTeam = (Team)team;
		PlayerSkin.SetSkin(head, body, legs);
		PlayerSkin.UpdateSkin();
		if (SetTeamEvent != null)
		{
			SetTeamEvent(photonView.ownerId, (Team)team);
		}
	}

	private void PhotonSetTeam(object[] data)
	{
		PhotonSetTeam((byte)data[0], (byte)data[1], (byte)data[2], (byte)data[3]);
	}

	public void UpdateFireStatValue(int weaponID)
	{
		photonView.RPC("PhotonUpdateFireStatValue", PhotonTargets.Others, (byte)weaponID);
	}

	[PunRPC]
	private void PhotonUpdateFireStatValue(byte weaponID)
	{
		if (PlayerSkin.SelectWeapon != null && PlayerSkin.SelectWeapon.WeaponID == weaponID)
		{
			PlayerSkin.SelectWeapon.UpdateFireStat1();
		}
	}

	private void PhotonUpdateFireStatValue(object[] data)
	{
		PhotonUpdateFireStatValue((byte)data[0]);
	}

	public static ControllerManager FindController(int id)
	{
		for (int i = 0; i < ControllerList.Count; i++)
		{
			if (ControllerList[i].photonView.ownerId == id)
			{
				return ControllerList[i];
			}
		}
		return null;
	}
}
