using System;
using UnityEngine;

public class VideoTextureManager : MonoBehaviour
{
	public Transform Video;

	public Material VideoMaterial;

	public MediaPlayerCtrl MediaPlayer;

	public TextMesh TimeMesh;

	public string VideoUrl;

	public Transform CheckVideo;

	public float distance = 2f;

	public float size = 0.25f;

	public float pushDistance = 0.009f;

	public static VideoTextureManager instance;

	private void Start()
	{
		instance = this;
		PhotonEvent.AddListener(PhotonEventTag.CreateVideo, PhotonCreateVideo);
		VideoUrl = PlayerPrefs.GetString("VideoUrl");
	}

	private void UpdateTime()
	{
		if (MediaPlayer.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
		{
			TimeMesh.text = ConvertTime(MediaPlayer.GetSeekPosition()) + "/" + ConvertTime(MediaPlayer.GetDuration());
		}
	}

	private string ConvertTime(int seek)
	{
		int num = seek / 1000 / 60;
		int num2 = (seek - num * 1000 * 60) / 1000;
		return num + ":" + num2.ToString("D2");
	}

	private bool CheckPosition(RaycastHit hit)
	{
		CheckVideo.transform.position = hit.point + hit.normal * pushDistance;
		CheckVideo.transform.forward = -hit.normal;
		if (Physics.Raycast(CheckVideo.position, CheckVideo.forward, out hit, distance))
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 origin = Vector3.zero;
			Vector3 direction = Vector3.zero;
			for (int i = 1; i < 5; i++)
			{
				switch (i)
				{
				case 1:
					origin = CheckVideo.position - CheckVideo.right * size / 2f;
					direction = CheckVideo.forward;
					break;
				case 2:
					origin = CheckVideo.position + CheckVideo.right * size / 2f;
					direction = CheckVideo.forward;
					break;
				case 3:
					origin = CheckVideo.position - CheckVideo.up * size / 2f;
					direction = CheckVideo.forward;
					break;
				case 4:
					origin = CheckVideo.position + CheckVideo.up * size / 2f;
					direction = CheckVideo.forward;
					break;
				case 5:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = Video.right;
					break;
				case 6:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = -Video.right;
					break;
				case 7:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = Video.up;
					break;
				case 8:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = -Video.up;
					break;
				}
				if (Physics.Raycast(origin, direction, out hitInfo, distance))
				{
					if (!(hit.transform == hitInfo.transform) || Math.Round(hit.distance, 1) != Math.Round(hitInfo.distance, 1))
					{
						return false;
					}
					continue;
				}
				return false;
			}
			for (int j = 1; j < 5; j++)
			{
				switch (j)
				{
				case 1:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = CheckVideo.right;
					break;
				case 2:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = -CheckVideo.right;
					break;
				case 3:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = CheckVideo.up;
					break;
				case 4:
					origin = CheckVideo.position - CheckVideo.forward * 0.1f;
					direction = -CheckVideo.up;
					break;
				}
				if (Physics.Raycast(origin, direction, out hitInfo, size / 2f))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void CreateVideo(RaycastHit hit, float size)
	{
	}

	private void PhotonCreateVideo(PhotonEventData data)
	{
		switch ((byte)data.parameters[0])
		{
		case 0:
		{
			Vector3 vector = (Vector3)data.parameters[1];
			Vector3 vector2 = (Vector3)data.parameters[2];
			VideoUrl = (string)data.parameters[3];
			size = (float)data.parameters[4];
			Video.gameObject.SetActive(true);
			Video.position = vector2 + vector * pushDistance;
			Video.forward = -vector;
			Video.localScale = Vector3.one * size;
			if (MediaPlayer.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
			{
				MediaPlayer.Stop();
			}
			MediaPlayer.Load(VideoUrl);
			MediaPlayer.Play();
			InvokeRepeating("UpdateTime", 0.5f, 0.5f);
			break;
		}
		case 1:
			if (MediaPlayer.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PAUSED)
			{
				MediaPlayer.Play();
			}
			break;
		case 2:
			if (MediaPlayer.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
			{
				MediaPlayer.Pause();
			}
			break;
		case 3:
			CancelInvoke("UpdateTime");
			Video.gameObject.SetActive(false);
			MediaPlayer.Stop();
			break;
		case 4:
			MediaPlayer.SetSpeed((float)data.parameters[1]);
			break;
		case 5:
			MediaPlayer.SetVolume((float)data.parameters[1]);
			break;
		}
	}
}
