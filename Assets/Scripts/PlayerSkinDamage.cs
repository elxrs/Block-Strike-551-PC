using UnityEngine;

public class PlayerSkinDamage : MonoBehaviour
{
	public PlayerSkinMember Member;

	public PlayerSkin m_PlayerSkin;

	private void Damage(DamageInfo damageInfo)
	{
		if (Member == PlayerSkinMember.Face)
		{
			damageInfo.HeadShot = true;
		}
		damageInfo.Damage = WeaponManager.GetMemberDamage(Member, damageInfo.WeaponID);
		m_PlayerSkin.Damage(damageInfo);
	}
}
