using UnityEngine;

public class PlayerAIDamage : MonoBehaviour
{
	public PlayerSkinMember Member;

	public PlayerAI playerAI;

	private void Damage(DamageInfo damageInfo)
	{
		if (Member == PlayerSkinMember.Face)
		{
			damageInfo.HeadShot = true;
		}
		damageInfo.Damage = WeaponManager.GetMemberDamage(Member, damageInfo.WeaponID);
		playerAI.Damage(damageInfo);
	}
}
