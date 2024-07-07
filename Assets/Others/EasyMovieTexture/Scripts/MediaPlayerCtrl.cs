using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MediaPlayerCtrl : MonoBehaviour
{
	public enum MEDIAPLAYER_ERROR
	{
		MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK = 200,
		MEDIA_ERROR_IO = -1004,
		MEDIA_ERROR_MALFORMED = -1007,
		MEDIA_ERROR_TIMED_OUT = -110,
		MEDIA_ERROR_UNSUPPORTED = -1010,
		MEDIA_ERROR_SERVER_DIED = 100,
		MEDIA_ERROR_UNKNOWN = 1
	}

	public enum MEDIAPLAYER_STATE
	{
		NOT_READY,
		READY,
		END,
		PLAYING,
		PAUSED,
		STOPPED,
		ERROR
	}

	public enum MEDIA_SCALE
	{
		SCALE_X_TO_Y,
		SCALE_X_TO_Z,
		SCALE_Y_TO_X,
		SCALE_Y_TO_Z,
		SCALE_Z_TO_X,
		SCALE_Z_TO_Y,
		SCALE_X_TO_Y_2
	}

	public delegate void VideoEnd();

	public delegate void VideoReady();

	public delegate void VideoError(MEDIAPLAYER_ERROR errorCode, MEDIAPLAYER_ERROR errorCodeExtra);

	public delegate void VideoFirstFrameReady();

	public delegate void VideoResize();

	public string m_strFileName;

	public GameObject[] m_TargetMaterial;

	private Texture2D m_VideoTexture;

	private Texture2D m_VideoTextureDummy;

	private MEDIAPLAYER_STATE m_CurrentState;

	private int m_iCurrentSeekPosition;

	private float m_fVolume = 1f;

	private int m_iWidth;

	private int m_iHeight;

	private float m_fSpeed = 1f;

	public bool m_bFullScreen;

	public bool m_bSupportRockchip = true;

	public VideoResize OnResize;

	public VideoReady OnReady;

	public VideoEnd OnEnd;

	public VideoError OnVideoError;

	public VideoFirstFrameReady OnVideoFirstFrameReady;

	private IntPtr m_texPtr;

	private int m_iAndroidMgrID;

	private bool m_bIsFirstFrameReady;

	private bool m_bFirst;

	public bool m_Scale = true;

	public MEDIA_SCALE m_ScaleValue;

	public GameObject[] m_objResize;

	public bool m_bLoop;

	public bool m_bAutoPlay = true;

	private bool m_bStop;

	private bool m_bInit;

	private bool m_bCheckFBO;

	private bool m_bPause;

	private bool m_bReadyPlay;

	private AndroidJavaObject javaObj;

	private List<Action> unityMainThreadActionList = new List<Action>();

	private bool checkNewActions;

	private object thisLock = new object();

	static MediaPlayerCtrl()
	{
	}

	private void Awake()
	{
		if (SystemInfo.deviceModel.Contains("rockchip"))
		{
			m_bSupportRockchip = true;
		}
		else
		{
			m_bSupportRockchip = false;
		}
		m_iAndroidMgrID = Call_InitNDK();
		Call_SetUnityActivity();
	}

	private void Start()
	{
		if (Application.dataPath.Contains(".obb"))
		{
			Call_SetSplitOBB(true, Application.dataPath);
		}
		else
		{
			Call_SetSplitOBB(false, null);
		}
		m_bInit = true;
	}

	private void OnApplicationQuit()
	{
	}

	private void OnDisable()
	{
		if (GetCurrentState() == MEDIAPLAYER_STATE.PLAYING)
		{
			Pause();
		}
	}

	private void OnEnable()
	{
		if (GetCurrentState() == MEDIAPLAYER_STATE.PAUSED)
		{
			Play();
		}
	}

	private void Update()
	{
		if (string.IsNullOrEmpty(m_strFileName))
		{
			return;
		}
		if (checkNewActions)
		{
			checkNewActions = false;
			CheckThreading();
		}
		if (!m_bFirst)
		{
			string text = m_strFileName.Trim();
			if (m_bSupportRockchip)
			{
				Call_SetRockchip(m_bSupportRockchip);
				if (text.Contains("://"))
				{
					Call_Load(text, 0);
				}
				else
				{
					StartCoroutine(CopyStreamingAssetVideoAndLoad(text));
				}
			}
			else
			{
				Call_Load(text, 0);
			}
			Call_SetLooping(m_bLoop);
			m_bFirst = true;
		}
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED)
		{
			if (!m_bCheckFBO)
			{
				if (Call_GetVideoWidth() <= 0 || Call_GetVideoHeight() <= 0)
				{
					return;
				}
				m_iWidth = Call_GetVideoWidth();
				m_iHeight = Call_GetVideoHeight();
				Resize();
				if (m_VideoTexture != null)
				{
					if (m_VideoTextureDummy != null)
					{
						Destroy(m_VideoTextureDummy);
						m_VideoTextureDummy = null;
					}
					m_VideoTextureDummy = m_VideoTexture;
					m_VideoTexture = null;
				}
				if (m_bSupportRockchip)
				{
					m_VideoTexture = new Texture2D(Call_GetVideoWidth(), Call_GetVideoHeight(), TextureFormat.RGB565, false);
				}
				else
				{
					m_VideoTexture = new Texture2D(Call_GetVideoWidth(), Call_GetVideoHeight(), TextureFormat.RGBA32, false);
				}
				m_VideoTexture.filterMode = FilterMode.Bilinear;
				m_VideoTexture.wrapMode = TextureWrapMode.Clamp;
				m_texPtr = m_VideoTexture.GetNativeTexturePtr();
				Call_SetUnityTexture(m_VideoTexture.GetNativeTextureID());
				Call_SetWindowSize();
				m_bCheckFBO = true;
				if (OnResize != null)
				{
					OnResize();
				}
			}
			else if (Call_GetVideoWidth() != m_iWidth || Call_GetVideoHeight() != m_iHeight)
			{
				m_iWidth = Call_GetVideoWidth();
				m_iHeight = Call_GetVideoHeight();
				if (OnResize != null)
				{
					OnResize();
				}
				ResizeTexture();
			}
			Call_UpdateVideoTexture();
			m_iCurrentSeekPosition = Call_GetSeekPosition();
		}
		if (m_CurrentState != Call_GetStatus())
		{
			m_CurrentState = Call_GetStatus();
			if (m_CurrentState == MEDIAPLAYER_STATE.READY)
			{
				if (OnReady != null)
				{
					OnReady();
				}
				if (m_bAutoPlay)
				{
					Call_Play(0);
				}
				if (m_bReadyPlay)
				{
					Call_Play(0);
					m_bReadyPlay = false;
				}
				SetVolume(m_fVolume);
			}
			else if (m_CurrentState == MEDIAPLAYER_STATE.END)
			{
				if (OnEnd != null)
				{
					OnEnd();
				}
				if (m_bLoop)
				{
					Call_Play(0);
				}
			}
			else if (m_CurrentState == MEDIAPLAYER_STATE.ERROR)
			{
				OnError((MEDIAPLAYER_ERROR)Call_GetError(), (MEDIAPLAYER_ERROR)Call_GetErrorExtra());
			}
		}
		GL.InvalidateState();
	}

	public void DeleteVideoTexture()
	{
		if (m_VideoTextureDummy != null)
		{
			Destroy(m_VideoTextureDummy);
			m_VideoTextureDummy = null;
		}
		if (m_VideoTexture != null)
		{
			Destroy(m_VideoTexture);
			m_VideoTexture = null;
		}
	}

	public void ResizeTexture()
	{
		Debug.Log("ResizeTexture " + m_iWidth + " " + m_iHeight);
		if (m_iWidth == 0 || m_iHeight == 0)
		{
			return;
		}
		if (m_VideoTexture != null)
		{
			if (m_VideoTextureDummy != null)
			{
				Destroy(m_VideoTextureDummy);
				m_VideoTextureDummy = null;
			}
			m_VideoTextureDummy = m_VideoTexture;
			m_VideoTexture = null;
		}
		if (m_bSupportRockchip)
		{
			m_VideoTexture = new Texture2D(Call_GetVideoWidth(), Call_GetVideoHeight(), TextureFormat.RGB565, false);
		}
		else
		{
			m_VideoTexture = new Texture2D(Call_GetVideoWidth(), Call_GetVideoHeight(), TextureFormat.RGBA32, false);
		}
		m_VideoTexture.filterMode = FilterMode.Bilinear;
		m_VideoTexture.wrapMode = TextureWrapMode.Clamp;
		m_texPtr = m_VideoTexture.GetNativeTexturePtr();
		Call_SetUnityTexture(m_VideoTexture.GetNativeTextureID());
		Call_SetWindowSize();
	}

	public void Resize()
	{
		if (m_CurrentState != MEDIAPLAYER_STATE.PLAYING || Call_GetVideoWidth() <= 0 || Call_GetVideoHeight() <= 0 || m_objResize == null)
		{
			return;
		}
		int width = Screen.width;
		int height = Screen.height;
		float num = height / width;
		int num2 = Call_GetVideoWidth();
		int num3 = Call_GetVideoHeight();
		float num4 = num3 / num2;
		float num5 = num / num4;
		for (int i = 0; i < m_objResize.Length; i++)
		{
			if (m_objResize[i] == null)
			{
				continue;
			}
			if (m_bFullScreen)
			{
				m_objResize[i].transform.localScale = new Vector3(20f / num, 20f / num, 1f);
				if (num4 < 1f)
				{
					if (num < 1f && num4 > num)
					{
						m_objResize[i].transform.localScale *= num5;
					}
					m_ScaleValue = MEDIA_SCALE.SCALE_X_TO_Y;
				}
				else
				{
					if (num > 1f)
					{
						if (num4 >= num)
						{
							m_objResize[i].transform.localScale *= num5;
						}
					}
					else
					{
						m_objResize[i].transform.localScale *= num5;
					}
					m_ScaleValue = MEDIA_SCALE.SCALE_X_TO_Y;
				}
			}
			if (m_Scale)
			{
				if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Y)
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.x, m_objResize[i].transform.localScale.x * num4, m_objResize[i].transform.localScale.z);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Y_2)
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.x, m_objResize[i].transform.localScale.x * num4 / 2f, m_objResize[i].transform.localScale.z);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Z)
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.x, m_objResize[i].transform.localScale.y, m_objResize[i].transform.localScale.x * num4);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Y_TO_X)
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.y / num4, m_objResize[i].transform.localScale.y, m_objResize[i].transform.localScale.z);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Y_TO_Z)
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.x, m_objResize[i].transform.localScale.y, m_objResize[i].transform.localScale.y / num4);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Z_TO_X)
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.z * num4, m_objResize[i].transform.localScale.y, m_objResize[i].transform.localScale.z);
				}
				else if (m_ScaleValue == MEDIA_SCALE.SCALE_Z_TO_Y)
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.x, m_objResize[i].transform.localScale.z * num4, m_objResize[i].transform.localScale.z);
				}
				else
				{
					m_objResize[i].transform.localScale = new Vector3(m_objResize[i].transform.localScale.x, m_objResize[i].transform.localScale.y, m_objResize[i].transform.localScale.z);
				}
			}
		}
	}

	private void OnError(MEDIAPLAYER_ERROR iCode, MEDIAPLAYER_ERROR iCodeExtra)
	{
		string empty = string.Empty;
		switch (iCode)
		{
		case MEDIAPLAYER_ERROR.MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK:
			empty = "MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK";
			break;
		case MEDIAPLAYER_ERROR.MEDIA_ERROR_SERVER_DIED:
			empty = "MEDIA_ERROR_SERVER_DIED";
			break;
		case MEDIAPLAYER_ERROR.MEDIA_ERROR_UNKNOWN:
			empty = "MEDIA_ERROR_UNKNOWN";
			break;
		default:
			empty = "Unknown error " + iCode;
			break;
		}
		empty += " ";
		switch (iCodeExtra)
		{
		case MEDIAPLAYER_ERROR.MEDIA_ERROR_IO:
			empty += "MEDIA_ERROR_IO";
			break;
		case MEDIAPLAYER_ERROR.MEDIA_ERROR_MALFORMED:
			empty += "MEDIA_ERROR_MALFORMED";
			break;
		case MEDIAPLAYER_ERROR.MEDIA_ERROR_TIMED_OUT:
			empty += "MEDIA_ERROR_TIMED_OUT";
			break;
		case MEDIAPLAYER_ERROR.MEDIA_ERROR_UNSUPPORTED:
			empty += "MEDIA_ERROR_UNSUPPORTED";
			break;
		default:
			empty = "Unknown error " + iCode;
			break;
		}
		Debug.LogError(empty);
		if (OnVideoError != null)
		{
			OnVideoError(iCode, iCodeExtra);
		}
	}

	private void OnDestroy()
	{
		Call_UnLoad();
		if (m_VideoTextureDummy != null)
		{
			Destroy(m_VideoTextureDummy);
			m_VideoTextureDummy = null;
		}
		if (m_VideoTexture != null)
		{
			Destroy(m_VideoTexture);
		}
		Call_Destroy();
	}

	private void OnApplicationPause(bool bPause)
	{
		Debug.Log("ApplicationPause : " + bPause);
		if (bPause)
		{
			if (m_CurrentState == MEDIAPLAYER_STATE.PAUSED)
			{
				m_bPause = true;
			}
			Call_Pause();
			return;
		}
		Call_RePlay();
		if (m_bPause)
		{
			Call_Pause();
			m_bPause = false;
		}
	}

	public MEDIAPLAYER_STATE GetCurrentState()
	{
		return m_CurrentState;
	}

	public Texture2D GetVideoTexture()
	{
		return m_VideoTexture;
	}

	public void Play()
	{
		if (m_bStop)
		{
			SeekTo(0);
			Call_Play(0);
			m_bStop = false;
		}
		if (m_CurrentState == MEDIAPLAYER_STATE.PAUSED)
		{
			Call_RePlay();
		}
		else if (m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED || m_CurrentState == MEDIAPLAYER_STATE.END)
		{
			Call_Play(0);
		}
		else if (m_CurrentState == MEDIAPLAYER_STATE.NOT_READY)
		{
			m_bReadyPlay = true;
		}
	}

	public void Stop()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING)
		{
			Call_Pause();
		}
		m_bStop = true;
		m_CurrentState = MEDIAPLAYER_STATE.STOPPED;
		m_iCurrentSeekPosition = 0;
	}

	public void Pause()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING)
		{
			Call_Pause();
		}
		m_CurrentState = MEDIAPLAYER_STATE.PAUSED;
	}

	public void Load(string strFileName)
	{
		if (GetCurrentState() != 0)
		{
			UnLoad();
		}
		m_bReadyPlay = false;
		m_bIsFirstFrameReady = false;
		m_bFirst = false;
		m_bCheckFBO = false;
		m_strFileName = strFileName;
		if (m_bInit)
		{
			m_CurrentState = MEDIAPLAYER_STATE.NOT_READY;
		}
	}

	public void SetVolume(float fVolume)
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			m_fVolume = fVolume;
			Call_SetVolume(fVolume);
		}
	}

	public int GetSeekPosition()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END)
		{
			return m_iCurrentSeekPosition;
		}
		return 0;
	}

	public void SeekTo(int iSeek)
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			Call_SetSeekPosition(iSeek);
		}
	}

	public void SetSpeed(float fSpeed)
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			m_fSpeed = fSpeed;
			Call_SetSpeed(fSpeed);
		}
	}

	public int GetDuration()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			return Call_GetDuration();
		}
		return 0;
	}

	public float GetSeekBarValue()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED)
		{
			if (GetDuration() == 0)
			{
				return 0f;
			}
			return GetSeekPosition() / GetDuration();
		}
		return 0f;
	}

	public void SetSeekBarValue(float fValue)
	{
		if ((m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY || m_CurrentState == MEDIAPLAYER_STATE.STOPPED) && GetDuration() != 0)
		{
			SeekTo((int)(GetDuration() * fValue));
		}
	}

	public int GetCurrentSeekPercent()
	{
		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED || m_CurrentState == MEDIAPLAYER_STATE.END || m_CurrentState == MEDIAPLAYER_STATE.READY)
		{
			return Call_GetCurrentSeekPercent();
		}
		return 0;
	}

	public int GetVideoWidth()
	{
		return Call_GetVideoWidth();
	}

	public int GetVideoHeight()
	{
		return Call_GetVideoHeight();
	}

	public void UnLoad()
	{
		m_bCheckFBO = false;
		Call_UnLoad();
		m_CurrentState = MEDIAPLAYER_STATE.NOT_READY;
	}

	private AndroidJavaObject GetJavaObject()
	{
		if (javaObj == null)
		{
			javaObj = new AndroidJavaObject("com.EasyMovieTexture.EasyMovieTexture");
		}
		return javaObj;
	}

	private void Call_Destroy()
	{
		GetJavaObject().Call("Destroy");
	}

	private void Call_UnLoad()
	{
		GetJavaObject().Call("UnLoad");
	}

	private bool Call_Load(string strFileName, int iSeek)
	{
		GetJavaObject().Call("NDK_SetFileName", strFileName);
		if (GetJavaObject().Call<bool>("Load", new object[0]))
		{
			return true;
		}
		OnError(MEDIAPLAYER_ERROR.MEDIA_ERROR_UNKNOWN, MEDIAPLAYER_ERROR.MEDIA_ERROR_UNKNOWN);
		return false;
	}

	private void Call_UpdateVideoTexture()
	{
		if (!Call_IsUpdateFrame())
		{
			return;
		}
		if (m_VideoTextureDummy != null)
		{
			Destroy(m_VideoTextureDummy);
			m_VideoTextureDummy = null;
		}
		for (int i = 0; i < m_TargetMaterial.Length; i++)
		{
			if ((bool)m_TargetMaterial[i])
			{
				if (m_TargetMaterial[i].GetComponent<MeshRenderer>() != null && m_TargetMaterial[i].GetComponent<MeshRenderer>().material.mainTexture != m_VideoTexture)
				{
					m_TargetMaterial[i].GetComponent<MeshRenderer>().material.mainTexture = m_VideoTexture;
				}
				if (m_TargetMaterial[i].GetComponent<RawImage>() != null && m_TargetMaterial[i].GetComponent<RawImage>().texture != m_VideoTexture)
				{
					m_TargetMaterial[i].GetComponent<RawImage>().texture = m_VideoTexture;
				}
			}
		}
		GetJavaObject().Call("UpdateVideoTexture");
		if (!m_bIsFirstFrameReady)
		{
			m_bIsFirstFrameReady = true;
			if (OnVideoFirstFrameReady != null)
			{
				OnVideoFirstFrameReady();
			}
		}
	}

	private void Call_SetVolume(float fVolume)
	{
		GetJavaObject().Call("SetVolume", fVolume);
	}

	private void Call_SetSeekPosition(int iSeek)
	{
		GetJavaObject().Call("SetSeekPosition", iSeek);
	}

	private int Call_GetSeekPosition()
	{
		return GetJavaObject().Call<int>("GetSeekPosition", new object[0]);
	}

	private void Call_Play(int iSeek)
	{
		GetJavaObject().Call("Play", iSeek);
	}

	private void Call_Reset()
	{
		GetJavaObject().Call("Reset");
	}

	private void Call_Stop()
	{
		GetJavaObject().Call("Stop");
	}

	private void Call_RePlay()
	{
		GetJavaObject().Call("RePlay");
	}

	private void Call_Pause()
	{
		GetJavaObject().Call("Pause");
	}

	private int Call_InitNDK()
	{
		return GetJavaObject().Call<int>("InitNative", new object[1] { GetJavaObject() });
	}

	private int Call_GetVideoWidth()
	{
		return GetJavaObject().Call<int>("GetVideoWidth", new object[0]);
	}

	private int Call_GetVideoHeight()
	{
		return GetJavaObject().Call<int>("GetVideoHeight", new object[0]);
	}

	private bool Call_IsUpdateFrame()
	{
		return GetJavaObject().Call<bool>("IsUpdateFrame", new object[0]);
	}

	private void Call_SetUnityTexture(int iTextureID)
	{
		GetJavaObject().Call("SetUnityTexture", iTextureID);
	}

	private void Call_SetWindowSize()
	{
		GetJavaObject().Call("SetWindowSize");
	}

	private void Call_SetLooping(bool bLoop)
	{
		GetJavaObject().Call("SetLooping", bLoop);
	}

	private void Call_SetRockchip(bool bValue)
	{
		GetJavaObject().Call("SetRockchip", bValue);
	}

	private int Call_GetDuration()
	{
		return GetJavaObject().Call<int>("GetDuration", new object[0]);
	}

	private int Call_GetCurrentSeekPercent()
	{
		return GetJavaObject().Call<int>("GetCurrentSeekPercent", new object[0]);
	}

	private int Call_GetError()
	{
		return GetJavaObject().Call<int>("GetError", new object[0]);
	}

	private void Call_SetSplitOBB(bool bValue, string strOBBName)
	{
		GetJavaObject().Call("SetSplitOBB", bValue, strOBBName);
	}

	private int Call_GetErrorExtra()
	{
		return GetJavaObject().Call<int>("GetErrorExtra", new object[0]);
	}

	private void Call_SetUnityActivity()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		GetJavaObject().Call("SetUnityActivity", @static);
		Call_InitJniManager();
	}

	private void Call_SetNotReady()
	{
		GetJavaObject().Call("SetNotReady");
	}

	private void Call_InitJniManager()
	{
		GetJavaObject().Call("InitJniManager");
	}

	private void Call_SetSpeed(float fSpeed)
	{
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build$VERSION"))
		{
			int @static = androidJavaClass.GetStatic<int>("SDK_INT");
			if (@static >= 23)
			{
				GetJavaObject().Call("SetSpeed", fSpeed);
			}
		}
	}

	private MEDIAPLAYER_STATE Call_GetStatus()
	{
		return (MEDIAPLAYER_STATE)GetJavaObject().Call<int>("GetStatus", new object[0]);
	}

	public IEnumerator DownloadStreamingVideoAndLoad(string strURL)
	{
		strURL = strURL.Trim();
		Debug.Log("DownloadStreamingVideo : " + strURL);
		WWW www2 = new WWW(strURL);
		yield return www2;
		if (string.IsNullOrEmpty(www2.error))
		{
			if (!Directory.Exists(Application.persistentDataPath + "/Data"))
			{
				Directory.CreateDirectory(Application.persistentDataPath + "/Data");
			}
			string write_path = Application.persistentDataPath + "/Data" + strURL.Substring(strURL.LastIndexOf("/"));
			File.WriteAllBytes(write_path, www2.bytes);
			Load("file://" + write_path);
		}
		else
		{
			Debug.Log(www2.error);
		}
		www2.Dispose();
		www2 = null;
		Resources.UnloadUnusedAssets();
	}

	public IEnumerator DownloadStreamingVideoAndLoad2(string strURL)
	{
		strURL = strURL.Trim();
		string write_path = Application.persistentDataPath + "/Data" + strURL.Substring(strURL.LastIndexOf("/"));
		if (File.Exists(write_path))
		{
			Load("file://" + write_path);
			yield break;
		}
		WWW www2 = new WWW(strURL);
		yield return www2;
		if (string.IsNullOrEmpty(www2.error))
		{
			if (!Directory.Exists(Application.persistentDataPath + "/Data"))
			{
				Directory.CreateDirectory(Application.persistentDataPath + "/Data");
			}
			File.WriteAllBytes(write_path, www2.bytes);
			Load("file://" + write_path);
		}
		else
		{
			Debug.Log(www2.error);
		}
		www2.Dispose();
		www2 = null;
		Resources.UnloadUnusedAssets();
	}

	private IEnumerator CopyStreamingAssetVideoAndLoad(string strURL)
	{
		strURL = strURL.Trim();
		string write_path = Application.persistentDataPath + "/" + strURL;
		if (!File.Exists(write_path))
		{
			Debug.Log("CopyStreamingAssetVideoAndLoad : " + strURL);
			WWW www2 = new WWW(Application.streamingAssetsPath + "/" + strURL);
			yield return www2;
			if (string.IsNullOrEmpty(www2.error))
			{
				Debug.Log(write_path);
				File.WriteAllBytes(write_path, www2.bytes);
				Load("file://" + write_path);
			}
			else
			{
				Debug.Log(www2.error);
			}
			www2.Dispose();
			www2 = null;
		}
		else
		{
			Load("file://" + write_path);
		}
	}

	private void CheckThreading()
	{
		lock (thisLock)
		{
			if (unityMainThreadActionList.Count <= 0)
			{
				return;
			}
			foreach (Action unityMainThreadAction in unityMainThreadActionList)
			{
				unityMainThreadAction();
			}
			unityMainThreadActionList.Clear();
		}
	}

	private void AddActionForUnityMainThread(Action a)
	{
		lock (thisLock)
		{
			unityMainThreadActionList.Add(a);
		}
		checkNewActions = true;
	}
}
