using System;
using CI.HttpClient;

public class mJoinServer
{
	public static RoomInfo room;

	public static Action onBack;

	public static void Join()
	{
		if (string.IsNullOrEmpty(room.GetPassword()))
		{
			if (room.PlayerCount == room.MaxPlayers)
			{
				OnLoadCustomMap(delegate
				{
					mPopUp.ShowPopup(Localization.Get("Do you want to queue up this server?"), Localization.Get("Queue"), Localization.Get("No"), delegate
					{
						onBack();
					}, Localization.Get("Yes"), delegate
					{
						mPhotonSettings.OnQueueServer(room);
					});
				});
			}
			else
			{
				OnLoadCustomMap(delegate
				{
					mPhotonSettings.OnJoinServer(room);
				});
			}
		}
		else
		{
			mPopUp.ShowInput(string.Empty, Localization.Get("Password"), 4, UIInput.KeyboardType.NumberPad, null, null, Localization.Get("Back"), delegate
			{
				onBack();
			}, "Ok", delegate
			{
				OnPassword();
			});
		}
	}

	private static void OnPassword()
	{
		if (room.GetPassword() == mPopUp.GetInputText())
		{
			if (room.PlayerCount == room.MaxPlayers)
			{
				OnLoadCustomMap(delegate
				{
					mPopUp.ShowPopup(Localization.Get("Do you want to queue up this server?"), Localization.Get("Queue"), Localization.Get("No"), onBack, Localization.Get("Yes"), delegate
					{
						mPhotonSettings.OnQueueServer(room);
					});
				});
			}
			else
			{
				OnLoadCustomMap(delegate
				{
					mPhotonSettings.OnJoinServer(room);
				});
			}
		}
		else
		{
			UIToast.Show(Localization.Get("Password is incorrect"));
#if UNITY_EDITOR
			UnityEngine.Debug.Log("Password: " + room.GetPassword());
#endif
		}
	}

	private static void OnLoadCustomMap(Action callback)
	{
		if (room.GetMapHash() == 0)
		{
			LevelManager.CustomMap = false;
			if (callback != null)
			{
				callback();
			}
			return;
		}
		int hash = CustomMapManager.GetMapHash(room.GetSceneName());
		if (hash != 0 && hash == room.GetMapHash())
		{
			CustomMapManager.Load(CustomMapManager.GetMapPath(room.GetSceneName()));
			LevelManager.CustomMap = true;
			if (callback != null)
			{
				callback();
			}
			return;
		}
		mPopUp.ShowPopup(Localization.Get("Do you really want to download a custom map?"), Localization.Get("Map"), Localization.Get("No"), delegate
		{
			onBack();
		}, Localization.Get("Yes"), delegate
		{
			HttpClient client = new HttpClient();
			Uri uri = new Uri("https://drive.google.com/uc?export=download&id=" + room.GetMapUrl());
			client.GetByteArray(uri, HttpCompletionOption.AllResponseContent, delegate(HttpResponseMessage<byte[]> r)
			{
				if (r.IsSuccessStatusCode)
				{
					string path = CustomMapManager.SaveMap(room.GetSceneName(), room.GetMapUrl(), r.Data);
					hash = CustomMapManager.GetMapHash(room.GetSceneName());
					if (hash != 0 && hash == room.GetMapHash())
					{
						CustomMapManager.Load(path);
						LevelManager.CustomMap = true;
						if (callback != null)
						{
							callback();
						}
					}
					else
					{
						LevelManager.CustomMap = false;
						UIToast.Show(Localization.Get("Error"));
						onBack();
					}
				}
				else
				{
					LevelManager.CustomMap = false;
					UIToast.Show(Localization.Get("Error"));
					onBack();
				}
			});
			mPopUp.ShowPopup(Localization.Get("Please wait") + "...", Localization.Get("Map"), Localization.Get("Exit"), delegate
			{
				client.Abort();
				LevelManager.CustomMap = false;
				onBack();
			});
		});
	}
}
