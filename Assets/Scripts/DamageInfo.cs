using System.Collections.Generic;
using UnityEngine;

public class DamageInfo
{
	public int Damage;

	public Vector3 AttackPosition;

	public Team AttackerTeam;

	public int WeaponID;

	public int WeaponSkinID;

	public int PlayerID;

	public bool HeadShot;

	private static List<DamageInfo> PoolList = new List<DamageInfo>();

	public bool isPlayerID
	{
		get
		{
			return PlayerID != -1 && PlayerID != PhotonNetwork.player.ID;
		}
	}

	public static DamageInfo Get()
	{
		DamageInfo damageInfo = null;
		if (PoolList.Count != 0)
		{
			damageInfo = PoolList[0];
			damageInfo.HeadShot = false;
			PoolList.RemoveAt(0);
		}
		else
		{
			damageInfo = new DamageInfo();
		}
		return damageInfo;
	}

	public static DamageInfo Get(int damage, Vector3 attackPosition, Team attackerTeam, int weaponID, int weaponSkinID, int playerID, bool headshot)
	{
		DamageInfo damageInfo = null;
		if (PoolList.Count != 0)
		{
			damageInfo = PoolList[0];
			PoolList.RemoveAt(0);
		}
		else
		{
			damageInfo = new DamageInfo();
		}
		damageInfo.Damage = damage;
		damageInfo.AttackPosition = attackPosition;
		damageInfo.AttackerTeam = attackerTeam;
		damageInfo.WeaponID = weaponID;
		damageInfo.WeaponSkinID = weaponSkinID;
		damageInfo.PlayerID = playerID;
		damageInfo.HeadShot = headshot;
		return damageInfo;
	}

	public void Dispose()
	{
		PoolList.Add(this);
	}
}
