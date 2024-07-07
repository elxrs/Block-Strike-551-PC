using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CI.HttpClient;
using UnityEngine;

public class Firebase
{
	public string Key;

	public string Auth;

	public string FullKey;

	public Firebase Parent;

	public string DataBase;

	private static bool isServerCertificate;

	public string FullURL
	{
		get
		{
			return "https://" + DataBase + FullKey + ".json";
		}
	}

	public Firebase()
	{
		Key = string.Empty;
		FullKey = string.Empty;
		Parent = null;
		DataBase = "";
	}

	public Firebase(string databaseURL)
	{
		DataBase = databaseURL;
	}

	public Firebase(string databaseURL, string auth)
	{
		DataBase = databaseURL;
		Auth = auth;
	}

	private Firebase(Firebase parent, string key, string auth)
	{
		Parent = parent;
		Key = key;
		Auth = auth;
		FullKey = parent.FullKey + "/" + key;
		DataBase = parent.DataBase;
	}

	public Firebase Child(string key)
	{
		return new Firebase(this, key, Auth);
	}

	public Firebase Copy()
	{
		Firebase firebase = new Firebase();
		firebase.Key = Key;
		firebase.Auth = Auth;
		firebase.FullKey = FullKey;
		firebase.Parent = Parent;
		firebase.DataBase = DataBase;
		return firebase;
	}

	public void SetTimeStamp(string key)
	{
		Child(key).SetValue(GetTimeStamp());
	}

	public static string GetTimeStamp()
	{
		return "{\".sv\": \"timestamp\"}";
	}

	public void GetValue()
	{
		GetValue(string.Empty, null, null);
	}

	public void GetValue(Action<string> success, Action<string> failed)
	{
		GetValue(string.Empty, success, failed);
	}

	public void GetValue(FirebaseParam param)
	{
		GetValue(param.ToString(), null, null);
	}

	public void GetValue(FirebaseParam param, Action<string> success, Action<string> failed)
	{
		GetValue(param.ToString(), success, failed);
	}

	public void GetValue(string param, Action<string> success, Action<string> failed)
	{
		if (!string.IsNullOrEmpty(Auth))
		{
			param = new FirebaseParam(param).Auth(Auth).ToString();
		}
		string text = FullURL;
		param = WWW.EscapeURL(param);
		if (!string.IsNullOrEmpty(param))
		{
			text = text + "?" + param;
		}
		HttpClient httpClient = new HttpClient();
		httpClient.GetString(new Uri(text), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				if (success != null)
				{
					success(r.Data);
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnGetSuccess");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + r.Data);
				}
			}
			else
			{
				if (failed != null)
				{
					failed(r.Exception.Message);
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnGetFailed");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + r.Exception.Message);
				}
			}
		});
	}

	private IEnumerator GetValueCoroutine(string url, Action<string> success, Action<string> failed)
	{
		WWW www = new WWW(url);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			if (success != null)
			{
				success(www.text);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnGetSuccess");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.text);
			}
		}
		else
		{
			if (failed != null)
			{
				failed(www.error);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnGetFailed");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.error);
			}
		}
	}

	public void SetValue(string json)
	{
		SetValue(json, string.Empty, null, null);
	}

	public void SetValue(string json, Action<string> success, Action<string, string> failed)
	{
		SetValue(json, string.Empty, success, failed);
	}

	public void SetValue(string json, FirebaseParam param)
	{
		SetValue(json, param.ToString(), null, null);
	}

	public void SetValue(string json, FirebaseParam param, Action<string> success, Action<string, string> failed)
	{
		SetValue(json, param.ToString(), success, failed);
	}

	public void SetValue(string json, string param, Action<string> success, Action<string, string> failed)
	{
		if (!string.IsNullOrEmpty(Auth))
		{
			param = new FirebaseParam(param).Auth(Auth).ToString();
		}
		string text = FullURL;
		param = WWW.EscapeURL(param);
		if (!string.IsNullOrEmpty(param))
		{
			text = text + "?" + param;
		}
		HttpClient httpClient = new HttpClient();
		StringContent content = new StringContent(json.ToString());
		httpClient.Put(new Uri(text), content, HttpCompletionOption.AllResponseContent, delegate(HttpResponseMessage<byte[]> r)
		{
			if (r.IsSuccessStatusCode)
			{
				if (success != null)
				{
					success(Encoding.UTF8.GetString(r.Data));
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnSetSuccess");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + Encoding.UTF8.GetString(r.Data));
				}
			}
			else
			{
				if (failed != null)
				{
					failed(r.Exception.Message, json);
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnSetFailed");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + r.Exception.Message);
				}
			}
		});
	}

	private IEnumerator SetValueCoroutine(string url, string json, Action<string> success, Action<string, string> failed)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "Content-Type", "application/json" },
			{ "X-HTTP-Method-Override", "PUT" }
		};
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		WWW www = new WWW(url, bytes, dictionary);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			if (success != null)
			{
				success(www.text);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnSetSuccess");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.text);
			}
		}
		else
		{
			if (failed != null)
			{
				failed(www.error, json);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnSetFailed");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.error);
			}
		}
	}

	public void UpdateValue(string json)
	{
		UpdateValue(json, string.Empty, null, null);
	}

	public void UpdateValue(string json, Action<string> success, Action<string, string> failed)
	{
		UpdateValue(json, string.Empty, success, failed);
	}

	public void UpdateValue(string json, FirebaseParam param)
	{
		UpdateValue(json, param.ToString(), null, null);
	}

	public void UpdateValue(string json, FirebaseParam param, Action<string> success, Action<string, string> failed)
	{
		UpdateValue(json, param.ToString(), success, failed);
	}

	public void UpdateValue(string json, string param, Action<string> success, Action<string, string> failed)
	{
		if (!string.IsNullOrEmpty(Auth))
		{
			param = new FirebaseParam(param).Auth(Auth).ToString();
		}
		string text = FullURL;
		param = WWW.EscapeURL(param);
		if (!string.IsNullOrEmpty(param))
		{
			text = text + "?" + param;
		}
		HttpClient httpClient = new HttpClient();
		StringContent content = new StringContent(json.ToString());
		httpClient.Patch(new Uri(text), content, HttpCompletionOption.AllResponseContent, delegate(HttpResponseMessage<byte[]> r)
		{
			if (r.IsSuccessStatusCode)
			{
				if (success != null)
				{
					success(Encoding.UTF8.GetString(r.Data));
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnUpdateSuccess");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + Encoding.UTF8.GetString(r.Data));
				}
			}
			else
			{
				if (failed != null)
				{
					failed(r.Exception.Message, json);
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnUpdateFailed");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + r.Exception.Message);
				}
			}
		});
	}

	private IEnumerator UpdateValueCoroutine(string url, string json, Action<string> success, Action<string, string> failed)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "Content-Type", "application/json" },
			{ "X-HTTP-Method-Override", "PATCH" }
		};
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		WWW www = new WWW(url, bytes, dictionary);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			if (success != null)
			{
				success(www.text);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnUpdateSuccess");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.text);
			}
		}
		else
		{
			if (failed != null)
			{
				failed(www.error, json);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnUpdateFailed");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.error);
			}
		}
	}

	public void Push(string json)
	{
		Push(json, string.Empty, null, null);
	}

	public void Push(string json, Action<string> success, Action<string> failed)
	{
		Push(json, string.Empty, success, failed);
	}

	public void Push(string json, FirebaseParam param)
	{
		Push(json, param.ToString(), null, null);
	}

	public void Push(string json, FirebaseParam param, Action<string> success, Action<string> failed)
	{
		Push(json, param.ToString(), success, failed);
	}

	public void Push(string json, string param, Action<string> success, Action<string> failed)
	{
		if (!string.IsNullOrEmpty(Auth))
		{
			param = new FirebaseParam(param).Auth(Auth).ToString();
		}
		string text = FullURL;
		param = WWW.EscapeURL(param);
		if (!string.IsNullOrEmpty(param))
		{
			text = text + "?" + param;
		}
		HttpClient httpClient = new HttpClient();
		StringContent content = new StringContent(json.ToString());
		httpClient.Put(new Uri(text), content, HttpCompletionOption.AllResponseContent, delegate(HttpResponseMessage<byte[]> r)
		{
			if (r.IsSuccessStatusCode)
			{
				if (success != null)
				{
					success(Encoding.UTF8.GetString(r.Data));
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnSetSuccess");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + Encoding.UTF8.GetString(r.Data));
				}
			}
			else
			{
				if (failed != null)
				{
					failed(r.Exception.Message);
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnSetFailed");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + r.Exception.Message);
				}
			}
		});
	}

	private IEnumerator PushCoroutine(string url, string json, Action<string> success, Action<string> failed)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		WWW www = new WWW(url, bytes);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			if (success != null)
			{
				success(www.text);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnPushSuccess");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.text);
			}
		}
		else
		{
			if (failed != null)
			{
				failed(www.error);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnPushFailed");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.error);
			}
		}
	}

	public void Delete()
	{
		Delete(string.Empty, null, null);
	}

	public void Delete(Action<string> success, Action<string> failed)
	{
		Delete(string.Empty, success, failed);
	}

	public void Delete(FirebaseParam param)
	{
		Delete(param.ToString(), null, null);
	}

	public void Delete(FirebaseParam param, Action<string> success, Action<string> failed)
	{
		Delete(param.ToString(), success, failed);
	}

	public void Delete(string param, Action<string> success, Action<string> failed)
	{
		if (!string.IsNullOrEmpty(Auth))
		{
			param = new FirebaseParam(param).Auth(Auth).ToString();
		}
		string text = FullURL;
		param = WWW.EscapeURL(param);
		if (!string.IsNullOrEmpty(param))
		{
			text = text + "?" + param;
		}
		HttpClient httpClient = new HttpClient();
		httpClient.Delete(new Uri(text), delegate(HttpResponseMessage<string> r)
		{
			if (r.IsSuccessStatusCode)
			{
				if (success != null)
				{
					success(r.Data);
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnSetSuccess");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + r.Data);
				}
			}
			else
			{
				if (failed != null)
				{
					failed(r.Exception.Message);
				}
				if (FirebaseManager.DebugAction)
				{
					Debug.Log("OnSetFailed");
					Debug.Log("Firebase: " + FullURL);
					Debug.Log("Json: " + r.Exception.Message);
				}
			}
		});
	}

	private IEnumerator DeleteCoroutine(string url, Action<string> success, Action<string> failed)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>
		{
			{ "Content-Type", "application/json" },
			{ "X-HTTP-Method-Override", "DELETE" }
		};
		byte[] bytes = Encoding.UTF8.GetBytes("{ \"dummy\" : \"dummies\"}");
		WWW www = new WWW(url, bytes, dictionary);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			if (success != null)
			{
				success(www.text);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnDeleteSuccess");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.text);
			}
		}
		else
		{
			if (failed != null)
			{
				failed(www.error);
			}
			if (FirebaseManager.DebugAction)
			{
				Debug.Log("OnDeleteFailed");
				Debug.Log("Firebase: " + FullURL);
				Debug.Log("Json: " + www.error);
			}
		}
	}
}
