using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CI.HttpClient;
using FreeJSON;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
	public AccountData Data = new AccountData();

	public CryptoString[] Links = new CryptoString[0];

	public static bool isConnect;

	public static CryptoString AccountID;

	public static AccountManager instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public static void Init()
	{
		GameObject gameObject = new GameObject("AccountManager");
		gameObject.AddComponent<AccountManager>();
	}

	public static void Login(Action<bool> complete, Action<string> failed)
	{
		Login(AccountID, complete, failed);
	}

	public static void Login(string id, Action<bool> complete, Action<string> failed)
	{
		AccountID = id;
		ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object o, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => true));
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("i", AndroidNativeFunctions.GetAndroidID2());
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("v", VersionManager.bundleVersion);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[0], 1, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "register":
					complete(false);
					isConnect = false;
					break;
				case "ban":
					failed("Account Ban");
					TimerManager.In(10f, false, delegate
					{
						Application.Quit();
					});
					break;
				case "deviceban":
					failed("Device Ban");
					TimerManager.In(10f, false, delegate
					{
						Application.Quit();
					});
					break;
				case "oldversion":
					failed(Localization.Get("Old Version"));
					isConnect = false;
					break;
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					instance.Data = AccountConvert.Deserialize(data);
					if (!CheckVersion())
					{
						complete(true);
						isConnect = true;
						CheckAndroidEmulator();
						SetAvatar(AndroidGoogleSignIn.Account.PhotoUrl);
						EventManager.Dispatch("AccountConnected");
					}
					else
					{
						failed(Localization.Get("Old Version"));
						isConnect = false;
					}
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void SetAvatar(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return;
		}
		if (CacheManager.Exists(url, "Avatars", true))
		{
			instance.Data.Avatar = new Texture2D(96, 96);
			instance.Data.Avatar.LoadImage(CacheManager.Load<byte[]>(url, "Avatars", true));
			instance.Data.Avatar.Apply();
			instance.Data.AvatarUrl = url;
			EventManager.Dispatch("AvatarUpdate");
			return;
		}
		HttpClient httpClient = new HttpClient();
		httpClient.GetByteArray(new Uri(url), HttpCompletionOption.AllResponseContent, delegate(HttpResponseMessage<byte[]> r)
		{
			if (r.IsSuccessStatusCode)
			{
				instance.Data.Avatar = new Texture2D(96, 96);
				instance.Data.Avatar.LoadImage(r.Data);
				instance.Data.Avatar.Apply();
				instance.Data.AvatarUrl = url;
				EventManager.Dispatch("AvatarUpdate");
				CacheManager.Save(url, "Avatars", r.Data, true);
			}
		});
	}

	public static bool CheckVersion()
	{
		return Utils.CompareVersion(VersionManager.bundleVersion, instance.Data.GameVersion);
	}

	public static bool CheckAndroidEmulator()
	{
		if (AndroidEmulatorDetector.isEmulator() && !CryptoPrefs.GetBool("AndroidEmulator", false))
		{
			TimerManager.In(0.2f, false, delegate
			{
				Application.Quit();
			});
			AndroidNativeFunctions.ShowToast("Android Emulator Detected");
			return true;
		}
		return false;
	}

	public static void Register(string playerName, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("n", playerName);
		jsonObject.Add("e", AccountID);
		jsonObject.Add("i", AndroidNativeFunctions.GetAndroidID2());
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("v", VersionManager.bundleVersion);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[0], 2, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "busy":
					failed(Localization.Get("Name already taken"));
					break;
				case "deviceban":
					failed("Device Ban");
					TimerManager.In(5f, false, delegate
					{
						Application.Quit();
					});
					break;
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					instance.Data = AccountConvert.Deserialize(data);
					complete();
					isConnect = true;
					CheckAndroidEmulator();
					EventManager.Dispatch("AccountConnected");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
				isConnect = false;
			}
		});
	}

	public static void UpdateName(string newName, Action<string> complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("n", newName);
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[0], 3, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "busy":
					failed(Localization.Get("Name already taken"));
					break;
				case "nomoney":
					failed(Localization.Get("Not enough money"));
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
				{
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					AccountData data2 = instance.Data;
					data2.Gold = (int)data2.Gold - nValue.int100;
					complete(data);
					CheckAndroidEmulator();
					break;
				}
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void UpdateData(Action failed)
	{
		UpdateData(failed, null);
	}

	public static void UpdateRound(JsonObject data, Action<string> complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("d", data);
		jsonObject.Add("s", instance.Data.Session);
		int key = UnityEngine.Random.Range(0, 6);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[0], 4, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data2 = r.Data;
				switch (data2)
				{
				case "error":
					failed("Unknown error");
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					data2 = Utils.XOR(data2, GameSettings.instance.Keys[key], false);
					instance.Data.UpdateData(data2);
					CheckAndroidEmulator();
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void BuyPlayerSkin(int id, BodyParts part, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		switch (part)
		{
		case BodyParts.Head:
			jsonObject.Add("h", id);
			break;
		case BodyParts.Body:
			jsonObject.Add("b", id);
			break;
		case BodyParts.Legs:
			jsonObject.Add("l", id);
			break;
		}
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 1, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "nomoney":
					failed(Localization.Get("Not enough money"));
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					instance.Data.UpdateData(data);
					CheckAndroidEmulator();
					complete();
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void OpenSkinCase(int id, Action<string> complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("i", id);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 4, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "nomoney":
					failed(Localization.Get("Not enough money"));
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				case "gold":
				{
					complete(data);
					AccountData data2 = instance.Data;
					data2.Gold = (int)data2.Gold + 2;
					EventManager.Dispatch("AccountUpdate");
					break;
				}
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					instance.Data.UpdateData(data);
					complete(data);
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void OpenStickerCase(int id, Action<string> complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("i", id);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 5, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "nomoney":
					failed(Localization.Get("Not enough money"));
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					data = data.Replace("\"", string.Empty);
					SetStickers(int.Parse(data));
					complete(data);
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void InAppPurchase(JsonObject purchase, Action<string> complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("d", purchase);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 6, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					instance.Data.UpdateData(data);
					CheckAndroidEmulator();
					complete(data);
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void Rewarded(GameCurrency currency, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("c", (int)currency);
		int num = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[num], true));
		string uriString = string.Format(instance.Links[1], 7, arg, num);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				switch (r.Data)
				{
				case "error":
					failed("Unknown error");
					break;
				default:
					if (currency == GameCurrency.Gold)
					{
						AccountData data = instance.Data;
						data.Gold = (int)data.Gold + nValue.int1;
					}
					else
					{
						AccountData data2 = instance.Data;
						data2.Money = (int)data2.Money + nValue.int50;
					}
					complete();
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void UpdateData(Action failed, Action complete)
	{
		JsonObject jsonObject = AccountConvert.CompareDefaultValue(instance.Data);
		JsonObject jsonObject2 = AccountConvert.CompareWeaponValue(instance.Data);
		if (jsonObject.Length == 0 && jsonObject2.Length == 0)
		{
			return;
		}
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject3 = new JsonObject();
		jsonObject3.Add("s", instance.Data.Session);
		jsonObject3.Add("e", AccountID);
		jsonObject3.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject3.Add("m", jsonObject);
		jsonObject3.Add("w", jsonObject2);
		int num = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject3.ToString(), GameSettings.instance.Keys[num], true));
		string uriString = string.Format(instance.Links[0], 5, arg, num);
		mPopUp.SetActiveWait(true, Localization.Get("Synchronize data") + "...");
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			mPopUp.SetActiveWait(false);
			if (r.IsSuccessStatusCode)
			{
				switch (r.Data)
				{
				case "error":
					failed();
					UIToast.Show(Localization.Get("Error"));
					break;
				case "session":
					failed();
					UIToast.Show(Localization.Get("Error"));
					break;
				default:
					if (complete != null)
					{
						complete();
					}
					break;
				}
			}
			else
			{
				if (failed != null)
				{
					failed();
				}
				UIToast.Show(Localization.Get("Error"));
			}
		});
	}

	public static void SetSticker(int weapon, int skin, int sticker, int pos, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("w", weapon.ToString("D2"));
		jsonObject.Add("k", skin.ToString("D2"));
		jsonObject.Add("t", sticker.ToString("D2"));
		jsonObject.Add("o", pos.ToString("D2"));
		int num = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[num], true));
		string uriString = string.Format(instance.Links[1], 2, arg, num);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				switch (r.Data)
				{
				case "error":
					failed("Unknown error");
					break;
				default:
					DeleteSticker(sticker);
					SetWeaponSticker(weapon, skin, pos, sticker);
					complete();
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void DeleteSticker(int weapon, int skin, int pos, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("w", weapon.ToString("D2"));
		jsonObject.Add("k", skin.ToString("D2"));
		jsonObject.Add("o", pos.ToString("D2"));
		int num = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[num], true));
		string uriString = string.Format(instance.Links[1], 3, arg, num);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				switch (r.Data)
				{
				case "error":
					failed("Unknown error");
					break;
				default:
					DeleteWeaponSticker(weapon, skin, pos);
					complete();
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void BuyWeapon(int id, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("w", id);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 8, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "nomoney":
					failed(Localization.Get("Not enough money"));
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					instance.Data.UpdateData(data);
					CheckAndroidEmulator();
					complete();
					EventManager.Dispatch("AccountUpdate");
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void AddFriend(int id, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("f", 1);
		jsonObject.Add("d", id);
		int num = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[num], true));
		string uriString = string.Format(instance.Links[1], 9, arg, num);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					instance.Data.Friends.Add(int.Parse(data));
					complete();
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void DeleteFriend(int id, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("f", 2);
		JsonArray jsonArray = new JsonArray();
		jsonArray.Add(id.ToString());
		jsonArray.Add(instance.Data.ID.ToString());
		jsonObject.Add("d", jsonArray);
		int num = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[num], true));
		string uriString = string.Format(instance.Links[1], 9, arg, num);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				switch (r.Data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					complete();
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void GetFriendsName(int[] ids, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject json = new JsonObject();
		json.Add("e", AccountID);
		json.Add("p", AndroidGoogleSignIn.Account.Id);
		json.Add("s", instance.Data.Session);
		json.Add("f", 3);
		JsonArray jsonArray = new JsonArray();
		for (int i = 0; i < ids.Length; i++)
		{
			jsonArray.Add(ids[i].ToString());
		}
		json.Add("d", jsonArray);
		int num = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(json.ToString(), GameSettings.instance.Keys[num], true));
		string uriString = string.Format(instance.Links[1], 9, arg, num);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
				{
					json = JsonObject.Parse(data);
					for (int j = 0; j < json.Length; j++)
					{
						CryptoPrefs.SetString("Friend_#" + json.GetKey(j), json.Get<string>(json.GetKey(j)));
					}
					complete();
					break;
				}
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void GetFriendsInfo(int id, Action<string> complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("f", 4);
		jsonObject.Add("d", id);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 9, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					complete(data);
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void CreateClan(string tag, string name, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("f", 1);
		JsonObject jsonObject2 = new JsonObject();
		jsonObject2.Add("i", instance.Data.ID.ToString());
		jsonObject2.Add("n", name);
		jsonObject2.Add("t", tag);
		jsonObject.Add("d", jsonObject2);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 10, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "busy":
					failed(Localization.Get("Name already taken"));
					break;
				case "nomoney":
					failed(Localization.Get("Not enough money"));
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					print(data);
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					print(data);
					complete();
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static void AddPlayerClan(int id, Action complete, Action<string> failed)
	{
		HttpClient httpClient = new HttpClient();
		JsonObject jsonObject = new JsonObject();
		jsonObject.Add("e", AccountID);
		jsonObject.Add("p", AndroidGoogleSignIn.Account.Id);
		jsonObject.Add("s", instance.Data.Session);
		jsonObject.Add("f", 2);
		JsonObject jsonObject2 = new JsonObject();
		jsonObject2.Add("i", id);
		jsonObject2.Add("t", instance.Data.Clan);
		jsonObject.Add("d", jsonObject2);
		int key = UnityEngine.Random.Range(0, 9);
		string arg = WWW.EscapeURL(Utils.XOR(jsonObject.ToString(), GameSettings.instance.Keys[key], true));
		string uriString = string.Format(instance.Links[1], 10, arg, key);
		httpClient.GetString(new Uri(uriString), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				string data = r.Data;
				switch (data)
				{
				case "error":
					failed("Unknown error");
					break;
				case "busy":
					failed(Localization.Get("Name already taken"));
					break;
				case "nomoney":
					failed(Localization.Get("Not enough money"));
					break;
				case "session":
					failed(Localization.Get("Session error"));
					break;
				default:
					print(data);
					data = Utils.XOR(data, GameSettings.instance.Keys[key], false);
					print(data);
					complete();
					break;
				}
			}
			else
			{
				failed(r.Exception.Message);
			}
		});
	}

	public static int GetMoney()
	{
		return instance.Data.Money;
	}

	public static int GetGold()
	{
		return instance.Data.Gold;
	}

	public static int GetXP()
	{
		if (GetLevel() == 250)
		{
			return GetMaxXP();
		}
		return instance.Data.XP;
	}

	public static int GetMaxXP()
	{
		return 150 + 150 * GetLevel();
	}

	public static int GetLevel()
	{
		return instance.Data.Level;
	}

	public static List<string> GetInAppPurchase()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < instance.Data.InAppPurchase.Count; i++)
		{
			list.Add(instance.Data.InAppPurchase[i]);
		}
		return list;
	}

	public static int GetDeaths()
	{
		return instance.Data.Deaths;
	}

	public static int GetKills()
	{
		return instance.Data.Kills;
	}

	public static int GetHeadshot()
	{
		return instance.Data.Headshot;
	}

	public static void SetWeaponSelected(WeaponType weaponType, int weaponID)
	{
		switch (weaponType)
		{
		case WeaponType.Knife:
			instance.Data.SelectedKnife = weaponID;
			break;
		case WeaponType.Pistol:
			instance.Data.SelectedPistol = weaponID;
			break;
		case WeaponType.Rifle:
			instance.Data.SelectedRifle = weaponID;
			break;
		}
		instance.Data.UpdateSelectedWeapon = true;
	}

	public static int GetWeaponSelected(WeaponType weaponType)
	{
		switch (weaponType)
		{
		case WeaponType.Knife:
			return instance.Data.SelectedKnife;
		case WeaponType.Pistol:
			return instance.Data.SelectedPistol;
		case WeaponType.Rifle:
			return instance.Data.SelectedRifle;
		default:
			return 0;
		}
	}

	public static void SetPlayerSkinSelected(int id, BodyParts part)
	{
		switch (part)
		{
		case BodyParts.Head:
			instance.Data.PlayerSkin.Select[0] = id;
			break;
		case BodyParts.Body:
			instance.Data.PlayerSkin.Select[1] = id;
			break;
		case BodyParts.Legs:
			instance.Data.PlayerSkin.Select[2] = id;
			break;
		}
		instance.Data.UpdateSelectedPlayerSkin = true;
	}

	public static int GetPlayerSkinSelected(BodyParts part)
	{
		switch (part)
		{
		case BodyParts.Head:
			return instance.Data.PlayerSkin.Select[0];
		case BodyParts.Body:
			return instance.Data.PlayerSkin.Select[1];
		case BodyParts.Legs:
			return instance.Data.PlayerSkin.Select[2];
		default:
			return -1;
		}
	}

	public static bool GetPlayerSkin(int id, BodyParts part)
	{
		if (id == 0)
		{
			return true;
		}
		switch (part)
		{
		case BodyParts.Head:
			return instance.Data.PlayerSkin.Head.Contains(id);
		case BodyParts.Body:
			return instance.Data.PlayerSkin.Body.Contains(id);
		case BodyParts.Legs:
			return instance.Data.PlayerSkin.Legs.Contains(id);
		default:
			return false;
		}
	}

	public static bool GetWeapon(int id)
	{
		if (id == nValue.int12 || id == nValue.int3 || id == nValue.int4)
		{
			return true;
		}
		for (int i = 0; i < GameSettings.instance.Weapons.Count; i++)
		{
			if ((int)GameSettings.instance.Weapons[i].ID != id)
			{
				continue;
			}
			if ((bool)GameSettings.instance.Weapons[i].Secret)
			{
				for (int j = 0; j < instance.Data.Weapons.Count; j++)
				{
					if ((int)instance.Data.Weapons[j].ID == id)
					{
						if (instance.Data.Weapons[j].Skins != null && instance.Data.Weapons[j].Skins.Count != 0)
						{
							return true;
						}
						return false;
					}
				}
				continue;
			}
			for (int k = 0; k < instance.Data.Weapons.Count; k++)
			{
				if ((int)instance.Data.Weapons[k].ID == id)
				{
					return instance.Data.Weapons[k].Buy;
				}
			}
		}
		return false;
	}

	public static void SetWeaponSkin(int id, int skin)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID == id)
			{
				if (!instance.Data.Weapons[i].Skins.Contains(skin))
				{
					instance.Data.Weapons[i].Skins.Add(skin);
				}
				break;
			}
		}
	}

	public static bool GetWeaponSkin(int id, int skin)
	{
		if (skin == 0)
		{
			return true;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID == id)
			{
				return instance.Data.Weapons[i].Skins.Contains(skin);
			}
		}
		return false;
	}

	public static void SetWeaponSkinSelected(int id, int skin)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID == id)
			{
				instance.Data.Weapons[i].LastSkin = instance.Data.Weapons[i].Skin;
				instance.Data.Weapons[i].Skin = skin;
				break;
			}
		}
	}

	public static int GetWeaponSkinSelected(int id)
	{
		if (id == 0)
		{
			return 0;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID == id)
			{
				return instance.Data.Weapons[i].Skin;
			}
		}
		return 0;
	}

	public static void SetFireStat(int id, int skin)
	{
		if (skin == 0)
		{
			return;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID != id)
			{
				continue;
			}
			if (instance.Data.Weapons[i].FireStats.Count > skin)
			{
				if ((int)instance.Data.Weapons[i].FireStats[skin] < 0)
				{
					instance.Data.Weapons[i].FireStats[skin] = 0;
				}
				break;
			}
			for (int j = instance.Data.Weapons[i].FireStats.Count - 1; j < skin; j++)
			{
				instance.Data.Weapons[i].FireStats.Add(-1);
			}
			instance.Data.Weapons[i].FireStats[instance.Data.Weapons[i].FireStats.Count - 1] = 0;
			break;
		}
	}

	public static bool GetFireStat(int id, int skin)
	{
		if (skin == 0)
		{
			return false;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID == id)
			{
				if (instance.Data.Weapons[i].FireStats.Count > skin && (int)instance.Data.Weapons[i].FireStats[skin] != -1)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public static void SetFireStatCounter(int id, int skin, int value)
	{
		if (skin == 0)
		{
			return;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID != id)
			{
				continue;
			}
			if (instance.Data.Weapons[i].FireStats.Count > skin)
			{
				instance.Data.Weapons[i].FireStats[skin] = value;
				break;
			}
			for (int j = instance.Data.Weapons[i].FireStats.Count - 1; j < skin; j++)
			{
				instance.Data.Weapons[i].FireStats.Add(-1);
			}
			instance.Data.Weapons[i].FireStats[instance.Data.Weapons[i].FireStats.Count - 1] = value;
			break;
		}
	}

	public static int GetFireStatCounter(int id, int skin)
	{
		if (skin == 0)
		{
			return -1;
		}
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID == id)
			{
				if (instance.Data.Weapons[i].FireStats.Count > skin)
				{
					return instance.Data.Weapons[i].FireStats[skin];
				}
				return -1;
			}
		}
		return -1;
	}

	public static void SetClan(string clan)
	{
		instance.Data.Clan = clan;
		instance.Data.UpdateClan = true;
	}

	public static string GetClan()
	{
		return instance.Data.Clan;
	}

	public static void SetStickers(int id)
	{
		if (GetStickers(id))
		{
			for (int i = 0; i < instance.Data.Stickers.Count; i++)
			{
				if ((int)instance.Data.Stickers[i].ID == id)
				{
					++instance.Data.Stickers[i].Count;
					break;
				}
			}
		}
		else
		{
			AccountSticker accountSticker = new AccountSticker();
			accountSticker.ID = id;
			accountSticker.Count = 1;
			instance.Data.Stickers.Add(accountSticker);
			instance.Data.SortStickers();
		}
	}

	private static void DeleteSticker(int id)
	{
		if (!GetStickers(id))
		{
			return;
		}
		for (int i = 0; i < instance.Data.Stickers.Count; i++)
		{
			if ((int)instance.Data.Stickers[i].ID == id)
			{
				--instance.Data.Stickers[i].Count;
				if ((int)instance.Data.Stickers[i].Count == 0)
				{
					instance.Data.Stickers.RemoveAt(i);
					instance.Data.SortStickers();
				}
				break;
			}
		}
	}

	public static bool GetStickers(int id)
	{
		for (int i = 0; i < instance.Data.Stickers.Count; i++)
		{
			if ((int)instance.Data.Stickers[i].ID == id)
			{
				return true;
			}
		}
		return false;
	}

	public static int[] GetStickers()
	{
		int[] array = new int[instance.Data.Stickers.Count];
		for (int i = 0; i < instance.Data.Stickers.Count; i++)
		{
			array[i] = instance.Data.Stickers[i].ID;
		}
		return array;
	}

	public static int GetStickerCount(int id)
	{
		for (int i = 0; i < instance.Data.Stickers.Count; i++)
		{
			if ((int)instance.Data.Stickers[i].ID == id)
			{
				return instance.Data.Stickers[i].Count;
			}
		}
		return 0;
	}

	private static void SetWeaponSticker(int weapon, int skin, int pos, int sticker)
	{
		AccountWeaponStickers weaponStickers = GetWeaponStickers(weapon, skin);
		if (HasWeaponSticker(weapon, skin, pos))
		{
			for (int i = 0; i < weaponStickers.StickerData.Count; i++)
			{
				if ((int)weaponStickers.StickerData[i].Index == pos)
				{
					weaponStickers.StickerData[i].StickerID = sticker;
					break;
				}
			}
		}
		else
		{
			AccountWeaponStickerData accountWeaponStickerData = new AccountWeaponStickerData();
			accountWeaponStickerData.Index = pos;
			accountWeaponStickerData.StickerID = sticker;
			weaponStickers.StickerData.Add(accountWeaponStickerData);
			weaponStickers.SortWeaponStickerData();
		}
	}

	public static void DeleteWeaponSticker(int weapon, int skin, int pos)
	{
		AccountWeaponStickers weaponStickers = GetWeaponStickers(weapon, skin);
		if (weaponStickers == null)
		{
			return;
		}
		for (int i = 0; i < weaponStickers.StickerData.Count; i++)
		{
			if ((int)weaponStickers.StickerData[i].Index == pos)
			{
				weaponStickers.StickerData.RemoveAt(i);
				weaponStickers.SortWeaponStickerData();
				break;
			}
		}
	}

	public static bool HasWeaponSticker(int weapon, int skin, int pos)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID != weapon)
			{
				continue;
			}
			for (int j = 0; j < instance.Data.Weapons[i].Stickers.Count; j++)
			{
				if ((int)instance.Data.Weapons[i].Stickers[j].SkinID != skin)
				{
					continue;
				}
				for (int k = 0; k < instance.Data.Weapons[i].Stickers[j].StickerData.Count; k++)
				{
					if ((int)instance.Data.Weapons[i].Stickers[j].StickerData[k].Index == pos)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static int GetWeaponSticker(int weapon, int skin, int pos)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID != weapon)
			{
				continue;
			}
			for (int j = 0; j < instance.Data.Weapons[i].Stickers.Count; j++)
			{
				if ((int)instance.Data.Weapons[i].Stickers[j].SkinID != skin)
				{
					continue;
				}
				for (int k = 0; k < instance.Data.Weapons[i].Stickers[j].StickerData.Count; k++)
				{
					if ((int)instance.Data.Weapons[i].Stickers[j].StickerData[k].Index == pos)
					{
						return instance.Data.Weapons[i].Stickers[j].StickerData[k].StickerID;
					}
				}
			}
		}
		return -1;
	}

	public static AccountWeaponStickers GetWeaponStickers(int weapon, int skin)
	{
		for (int i = 0; i < instance.Data.Weapons.Count; i++)
		{
			if ((int)instance.Data.Weapons[i].ID != weapon)
			{
				continue;
			}
			for (int j = 0; j < instance.Data.Weapons[i].Stickers.Count; j++)
			{
				if ((int)instance.Data.Weapons[i].Stickers[j].SkinID == skin)
				{
					return instance.Data.Weapons[i].Stickers[j];
				}
			}
			AccountWeaponStickers accountWeaponStickers = new AccountWeaponStickers();
			accountWeaponStickers.SkinID = skin;
			instance.Data.Weapons[i].Stickers.Add(accountWeaponStickers);
			return instance.Data.Weapons[i].Stickers[instance.Data.Weapons[i].Stickers.Count - 1];
		}
		return new AccountWeaponStickers();
	}
}
