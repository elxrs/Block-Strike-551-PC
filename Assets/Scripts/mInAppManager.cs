using UnityEngine;

public class mInAppManager : MonoBehaviour
{
	private bool isRewardedVideo;

	public static mInAppManager instance;

	public void OnRewardedVideo(int currency)
	{
		if (!AccountManager.isConnect)
		{
			UIToast.Show(Localization.Get("Connection account"));
		}
		else
		{
			if (isRewardedVideo)
			{
				return;
			}
			UIToast.Show(Localization.Get("Please wait") + "...");
			isRewardedVideo = true;
			TimerManager.In(0.5f, delegate
			{
				RewardedVideoComplete(currency);
			});
		}
	}

	private void RewardedVideoComplete(int currency)
	{
		TimerManager.In(1f, delegate
		{
			switch (currency)
            {
				case 1:
					SaveLoadManager.SetGold1(1);
					UIToast.Show("+1 BS Coins");
					break;
				case 2:
					SaveLoadManager.SetGold1(100);
					UIToast.Show("+100 BS Coins");
					break;
				case 3:
					SaveLoadManager.SetGold1(250);
					UIToast.Show("+250 BS Coins");
					break;
				case 4:
					SaveLoadManager.SetGold1(600);
					UIToast.Show("+600 BS Coins");
					break;
				case 5:
					SaveLoadManager.SetGold1(1000);
					UIToast.Show("+1000 BS Coins");
					break;
				case 6:
					SaveLoadManager.SetMoney1(50);
					UIToast.Show("+50 BS Silver");
					break;
				case 7:
					SaveLoadManager.SetMoney1(5000);
					UIToast.Show("+5000 BS Silver");
					break;
				case 8:
					SaveLoadManager.SetMoney1(10000);
					UIToast.Show("+10000 BS Silver");
					break;
				case 9:
					SaveLoadManager.SetMoney1(20000);
					UIToast.Show("+20000 BS Silver");
					break;
				case 10:
					SaveLoadManager.SetMoney1(30000);
					UIToast.Show("+30000 BS Silver");
					break;
			}
			EventManager.Dispatch("AccountUpdate");
			isRewardedVideo = false;
		});
	}
}
