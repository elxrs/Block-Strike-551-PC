using UnityEngine;

public class ShootingRangeTargetDamage : MonoBehaviour
{
	public PlayerSkinMember Member;

	public ShootingRangeTarget Target;

	private void Damage(DamageInfo damageInfo)
	{
		if (Member == PlayerSkinMember.Face)
		{
			damageInfo.HeadShot = true;
		}
		damageInfo.Damage = WeaponManager.GetMemberDamage(Member, damageInfo.WeaponID);
		if (Target.GetActive())
		{
			UICrosshair.Hit();
		}
		Target.Damage(damageInfo);
		if (ShootingRangeManager.ShowDamage)
		{
			UIToast.Show(Localization.Get("Damage") + ": " + damageInfo.Damage, 2f);
		}
	}
}
