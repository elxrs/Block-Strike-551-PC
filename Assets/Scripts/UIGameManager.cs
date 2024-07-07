using System;
using DG.Tweening;
using UnityEngine;

public class UIGameManager : MonoBehaviour
{
	[Header("Health")]
	public UILabel HealthLabel;

	public UISprite HealthPanel;

	private Color HealthNormalColor = new Color(0f, 0f, 0f, 0.705f);

	private Color HealthCriticalColor = new Color(1f, 0f, 0f, 0.705f);

	[Header("Ammo")]
	public UILabel AmmoLabel;

	public GameObject AmmoPanel;

	[Header("Score")]
	public GameObject Score;

	public UILabel MaxScoreLabel;

	public UILabel BlueScoreLabel;

	public UILabel RedScoreLabel;

	public bool isScoreTimer;

	[HideInInspector]
	public float ScoreTimer;

	private bool ScoreTimerShow = true;

	private Action ScoreTimerAction;

	[Header("FPS Meter")]
	public UILabel FPSMeterLabel;

	private bool isFPSMeter;

	private float FPSMeterAccum;

	private float FPSMeterFrames;

	private int FPSMeterTimer;

	[Header("Duration")]
	public UISprite DurationSprite;

	public UILabel DurationLabel;

	private Tweener DurationTween;

	[Header("Pause")]
	public GameObject PauseWeapons;

	public static UIGameManager instance;

	public float Timer
	{
		get
		{
			if (ScoreTimer == 0f)
			{
				return 0f;
			}
			return ScoreTimer - Time.time;
		}
	}

	public static event Action PauseEvent;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
	}

	private void OnEnable()
	{
		InputManager.GetButtonDownEvent += GetButtonDown;
	}

	private void OnDisable()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
	}

	private void GetButtonDown(string name)
	{
		if (name == "Pause")
		{
			UpdatePause();
		}
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape) && LevelManager.GetSceneName() != "MainTutorial" && !PlayerInput.instance.PlayerWeapon.isScope)
		{
			UpdatePause();
		}
		UpdateFPSMeter();
		UpdateScore();
	}

	private void UpdatePause()
	{
		InputManager.instance.isCursor = true;
		UIPanelManager.ShowPanel("Pause");
		if (PauseEvent != null)
		{
			PauseEvent();
		}
		if (!GameManager.GetChangeWeapons())
		{
			PauseWeapons.SetActive(false);
		}
	}

	private void UpdateSettings()
	{
		isFPSMeter = Settings.FPSMeter;
		TimerManager.Cancel(FPSMeterTimer);
		if (isFPSMeter)
		{
			FPSMeterTimer = TimerManager.In(0.8f, -1, 0.8f, UpdateFPSMeterLabel);
		}
		FPSMeterLabel.gameObject.SetActive(isFPSMeter);
		UIElements.Get<UIPanel>("DisplayPanel").alpha = ((!Settings.HUD) ? 0.002f : 1f);
	}

	private void UpdateFPSMeter()
	{
		if (isFPSMeter)
		{
			FPSMeterAccum += Time.timeScale / Time.deltaTime;
			FPSMeterFrames += 1f;
		}
	}

	private void UpdateFPSMeterLabel()
	{
		float num = FPSMeterAccum / FPSMeterFrames;
		string text = string.Format("{0:F2} FPS", num);
		FPSMeterAccum = 0f;
		FPSMeterFrames = 0f;
		FPSMeterLabel.text = text;
	}

	public static UISprite GetDurationSprite()
	{
		return instance.DurationSprite;
	}

	public static void StartDuration(float duration)
	{
		StartDuration(duration, false, null);
	}

	public static void StartDuration(float duration, bool time)
	{
		StartDuration(duration, time, null);
	}

	public static void StartDuration(float duration, bool time, TweenCallback callback)
	{
		StopDuration();
		instance.DurationSprite.alpha = 1f;
		if (callback != null)
		{
			instance.DurationTween = DOTween.To(() => instance.DurationSprite.width, delegate(int x)
			{
				instance.DurationSprite.width = x;
			}, 155, duration).SetEase(Ease.Linear).OnComplete(callback);
			if (time)
			{
				instance.DurationTween.OnUpdate(instance.UpdateDuration);
			}
		}
		else
		{
			instance.DurationTween = DOTween.To(() => instance.DurationSprite.width, delegate(int x)
			{
				instance.DurationSprite.width = x;
			}, 155, duration).SetEase(Ease.Linear);
			if (time)
			{
				instance.DurationTween.OnUpdate(instance.UpdateDuration);
			}
		}
	}

	private void UpdateDuration()
	{
		DurationLabel.text = (DurationTween.Duration() - DurationTween.fullPosition).ToString("00:00");
	}

	public static void StopDuration()
	{
		if (instance.DurationTween != null && instance.DurationTween.IsActive())
		{
			instance.DurationTween.Kill();
		}
		instance.DurationSprite.alpha = 0f;
		instance.DurationSprite.width = 0;
		instance.DurationLabel.text = string.Empty;
	}

	public static void SetHealthLabel(int health)
	{
		if (health == 0)
		{
			instance.HealthLabel.text = string.Empty;
			instance.HealthPanel.cachedGameObject.SetActive(false);
			instance.AmmoLabel.text = string.Empty;
			instance.AmmoPanel.SetActive(false);
			return;
		}
		instance.HealthPanel.cachedGameObject.SetActive(true);
		instance.HealthLabel.text = "+" + health;
		if (health <= 25)
		{
			instance.HealthPanel.color = instance.HealthCriticalColor;
		}
		else
		{
			instance.HealthPanel.color = instance.HealthNormalColor;
		}
	}

	public static void SetAmmoLabel(int ammo, int maxAmmo)
	{
		SetAmmoLabel(ammo, maxAmmo, false);
	}

	public static void SetAmmoLabel(int ammo, int maxAmmo, bool infinity)
	{
		if (maxAmmo == -1)
		{
			instance.AmmoLabel.text = string.Empty;
			instance.AmmoPanel.SetActive(false);
			return;
		}
		if (!instance.AmmoPanel.activeSelf)
		{
			instance.AmmoPanel.SetActive(true);
		}
		if (infinity)
		{
			instance.AmmoLabel.text = ammo + "/∞";
		}
		else
		{
			instance.AmmoLabel.text = ammo + "/" + maxAmmo;
		}
	}

	private void UpdateScore()
	{
		if (!isScoreTimer)
		{
			return;
		}
		float num = ScoreTimer - Time.time;
		int num2 = (int)num / nValue.int60;
		int num3 = (int)num - num2 * nValue.int60;
		if (ScoreTimerShow)
		{
			MaxScoreLabel.text = string.Format("{0:0}:{1:00}", num2, num3);
		}
		if (ScoreTimer <= Time.time)
		{
			isScoreTimer = false;
			if (ScoreTimerAction != null)
			{
				ScoreTimerAction();
			}
		}
	}

	public static void SetActiveScore(bool active)
	{
		instance.Score.SetActive(active);
	}

	public static void SetActiveScore(bool active, int maxScore)
	{
		instance.Score.SetActive(active);
		if (maxScore <= nValue.int0)
		{
			instance.MaxScoreLabel.text = "-";
		}
		else
		{
			instance.MaxScoreLabel.text = maxScore.ToString();
		}
	}

	public static void UpdateScoreLabel()
	{
		if (GameManager.MaxScore <= nValue.int0)
		{
			instance.MaxScoreLabel.text = "-";
		}
		else
		{
			instance.MaxScoreLabel.text = GameManager.MaxScore.ToString();
		}
		instance.BlueScoreLabel.text = GameManager.BlueScore.ToString();
		instance.RedScoreLabel.text = GameManager.RedScore.ToString();
	}

	public static void UpdateScoreLabel(int maxScore, int blueScore, int redScore)
	{
		if (maxScore <= nValue.int0)
		{
			instance.MaxScoreLabel.text = "-";
		}
		else
		{
			instance.MaxScoreLabel.text = maxScore.ToString();
		}
		instance.BlueScoreLabel.text = blueScore.ToString();
		instance.RedScoreLabel.text = redScore.ToString();
	}

	public static void StartScoreTimer(float time, Action finishAction)
	{
		StartScoreTimer(time, true, finishAction);
	}

	public static void StartScoreTimer(float time, bool show, Action finishAction)
	{
		instance.isScoreTimer = true;
		instance.ScoreTimerShow = show;
		instance.ScoreTimer = time + Time.time;
		instance.ScoreTimerAction = finishAction;
	}

	public void OnExitServer()
	{
		PhotonNetwork.LeaveRoom();
	}
}
