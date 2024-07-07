using UnityEngine;

public class mVersionManager : MonoBehaviour
{
	public UILabel VersionLabel;

	private static mVersionManager instance;

	private void Start()
	{
		instance = this;
		VersionLabel.text = VersionManager.bundleVersion;
		UpdateRegion();
	}

	private void OnLocalize()
	{
		UpdateRegion();
	}

	public static void UpdateRegion()
	{
		string region = mPhotonSettings.region;
		string bundleVersion = VersionManager.bundleVersion;
		switch (region)
		{
		case "ru":
			bundleVersion = bundleVersion + "\n" + Localization.Get("Russia");
			break;
		case "sa":
			bundleVersion = bundleVersion + "\n" + Localization.Get("Brazil");
			break;
		default:
			bundleVersion = bundleVersion + "\n" + Localization.Get("Russia");
			break;
		}
		instance.VersionLabel.text = bundleVersion;
	}
}
