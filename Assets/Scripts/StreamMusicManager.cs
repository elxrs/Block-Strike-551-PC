using System.Collections;
using FreeJSON;
using UnityEngine;

public class StreamMusicManager : MonoBehaviour
{
	public string MusicUrl;

	private WWW www;

	private AudioSource Source;

	private JsonArray MusicList;

	private static StreamMusicManager instance;

	private void Start()
	{
		instance = this;
		PhotonEvent.AddListener(PhotonEventTag.StreamMusic, PhotonStreamMusic);
		MusicUrl = PlayerPrefs.GetString("MusicUrl");
	}

	private void PhotonStreamMusic(PhotonEventData data)
	{
		switch ((byte)data.parameters[0])
		{
		case 1:
		{
			string url = (string)data.parameters[1];
			Play(url);
			break;
		}
		case 2:
			Pause();
			break;
		case 3:
			Resume();
			break;
		case 4:
			Stop();
			break;
		case 5:
			SetVolume((float)data.parameters[1]);
			break;
		}
	}

	public static void Play(string url)
	{
		if (instance.Source != null)
		{
			instance.Source.Stop();
			instance.Source.clip = null;
		}
		instance.www = null;
		instance.StopAllCoroutines();
		instance.StartCoroutine(instance.StartStream(url));
	}

	private IEnumerator StartStream(string url)
	{
		MusicUrl = url;
		PlayerPrefs.SetString("MusicUrl", url);
		www = new WWW(url);
		while (www.progress < 0.08f)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (Source == null)
		{
			Source = gameObject.AddComponent<AudioSource>();
		}
		Source.clip = www.GetAudioClip(false, true);
		Source.Play();
	}

	public static void Pause()
	{
		if (instance.Source != null && instance.Source.isPlaying)
		{
			instance.Source.Pause();
		}
	}

	public static void Resume()
	{
		if (instance.Source != null && !instance.Source.isPlaying)
		{
			instance.Source.Play();
		}
	}

	public static void Stop()
	{
		if (instance.Source != null)
		{
			instance.Source.Stop();
			instance.Source.clip = null;
		}
	}

	public static void SetVolume(float value)
	{
		if (instance.Source != null && instance.Source.isPlaying)
		{
			instance.Source.volume = value;
		}
	}
}
