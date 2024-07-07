using System.Collections.Generic;
using UnityEngine;

public class CheckManager : MonoBehaviour
{
	public List<CryptoString> AppList;

	public CryptoString[] Permissions;

	private bool isShow;

	private static bool isDetected;

	private static bool isQuit;

	private void Start()
    {
		DontDestroyOnLoad(gameObject);
	}

#if UNITY_EDITOR
	private void OnApplicationQuit()
    {
		GameSettings.instance.PhotonID = "";
    }
#endif

	public static void Detected()
	{
		Detected("Detected");
	}

	public static void Detected(string text)
	{
		Detected(text, string.Empty);
	}

	public static void Detected(string text, string log)
	{
		if (!isQuit && !isDetected)
		{
			Debug.Log(1);
			GameSettings.instance.PhotonID = string.Empty;
			isDetected = true;
			if (PhotonNetwork.inRoom)
			{
				PhotonNetwork.LeaveRoom();
			}
			AndroidNativeFunctions.ShowAlert(text, "Detected", "Ok", string.Empty, string.Empty, delegate
			{
				Application.Quit();
			});
		}
	}

	public static void Quit()
	{
		if (!isQuit)
		{
			isQuit = true;
			if (PhotonNetwork.inRoom)
			{
				PhotonNetwork.LeaveRoom();
			}
			TimerManager.In(nValue.int1, false, delegate
			{
				Application.Quit();
			});
		}
	}
}
