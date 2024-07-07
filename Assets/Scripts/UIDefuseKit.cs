public class UIDefuseKit : NGUIBehaviour
{
	private static bool DefuseKit;

	public UISprite DefuseKitButton;

	private static UISprite DefuseKitIcon;

	private void Start()
	{
		NGUIEvents.Add(gameObject, this);
	}

	private void OnEnable()
	{
		DefuseKitButton.cachedGameObject.SetActive(PlayerInput.instance.PlayerTeam == Team.Blue);
		DefuseKitButton.alpha = ((!DefuseKit) ? 1f : 0.5f);
	}

	private void OnDestroy()
	{
		DefuseKit = false;
	}

	public override void OnClick()
	{
		if (!DefuseKit)
		{
			if (UIBuyWeapon.Money < 400)
			{
				UIToast.Show(Localization.Get("Not enough money"));
				return;
			}
			UIBuyWeapon.Money -= 400;
			SetDefuseKit(true);
			DefuseKitButton.alpha = 0.5f;
		}
	}

	public static void SetDefuseKit(bool active)
	{
		DefuseKit = active;
	}

	public static bool GetDefuseKit()
	{
		return DefuseKit;
	}
}
