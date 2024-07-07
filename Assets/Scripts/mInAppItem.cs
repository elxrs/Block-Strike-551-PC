public class mInAppItem : NGUIBehaviour
{
	public enum ItemList
	{
		Purchase,
		Consume
	}

	public ItemList Item;

	public string Sku;

	public UILabel PriceLabel;

	private void Start()
	{
		PriceLabel.text = InAppManager.GetPrice();
		NGUIEvents.Add(gameObject, this);
	}

	public override void OnClick()
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
		}
	}
}
