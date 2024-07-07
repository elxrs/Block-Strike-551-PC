using System;
using System.Collections.Generic;
using CI.HttpClient;
using UnityEngine;

public class AvatarManager
{
	private static Dictionary<string, Texture2D> avatars = new Dictionary<string, Texture2D>();

	public static bool Contains(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return false;
		}
		return CacheManager.Exists(url, "Avatars", true);
	}

	public static void Get(string url, Action<Texture2D> callback)
	{
		if (string.IsNullOrEmpty(url))
		{
			return;
		}
		if (Contains(url))
		{
			if (avatars.ContainsKey(url))
			{
				callback(avatars[url]);
				return;
			}
			Texture2D texture2D = new Texture2D(96, 96);
			try
			{
				texture2D.LoadImage(CacheManager.Load<byte[]>(url, "Avatars", true));
				texture2D.Apply();
				avatars.Add(url, texture2D);
				callback(texture2D);
				return;
			}
			catch
			{
				callback(null);
				return;
			}
		}
		HttpClient httpClient = new HttpClient();
		httpClient.GetByteArray(new Uri(url), HttpCompletionOption.AllResponseContent, delegate(HttpResponseMessage<byte[]> r)
		{
			if (r.IsSuccessStatusCode)
			{
				Texture2D texture2D2 = new Texture2D(96, 96);
				try
				{
					texture2D2.LoadImage(r.Data);
					texture2D2.Apply();
					avatars.Add(url, texture2D2);
					CacheManager.Save(url, "Avatars", r.Data, true);
					callback(texture2D2);
				}
				catch
				{
					callback(null);
				}
			}
		});
	}

	public static Texture2D Get(string url)
	{
		if (avatars.ContainsKey(url))
		{
			return avatars[url];
		}
		if (string.IsNullOrEmpty(url))
		{
			return GameSettings.instance.NoAvatarTexture;
		}
		if (Contains(url))
		{
			try
			{
				Texture2D texture2D = new Texture2D(96, 96);
				texture2D.LoadImage(CacheManager.Load<byte[]>(url, "Avatars", true));
				texture2D.Apply();
				avatars.Add(url, texture2D);
				return texture2D;
			}
			catch
			{
			}
		}
		return GameSettings.instance.NoAvatarTexture;
	}

	public static void Load(string url)
	{
		if (string.IsNullOrEmpty(url) || Contains(url))
		{
			return;
		}
		HttpClient httpClient = new HttpClient();
		httpClient.GetByteArray(new Uri(url), HttpCompletionOption.AllResponseContent, delegate(HttpResponseMessage<byte[]> r)
		{
			if (r.IsSuccessStatusCode)
			{
				CacheManager.Save(url, "Avatars", r.Data, true);
			}
		});
	}
}
