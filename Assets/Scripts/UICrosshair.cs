using DG.Tweening;
using UnityEngine;

public class UICrosshair : MonoBehaviour
{
	public GameObject Crosshair;

	public UISprite LeftSprite;

	public UISprite RightSprite;

	public UISprite TopSprite;

	public UISprite BottomSprite;

	public UISprite PointSprite;

	public CryptoFloat MaxAccuracy;

	public CryptoFloat Accuracy;

	private Vector2 FireAccuracy;

	public CryptoInt AccuracyWidth = 1600;

	public CryptoInt AccuracyHeight = 960;

	private Tweener Tween;

	[Header("Hit Settings")]
	public UIWidget HitSprite;

	public float HitDuration;

	private Tweener HitTween;

	private bool HitMarker = true;

	[Header("Scope")]
	public GameObject RifleScope;

	private bool isDynamic = true;

	public int Gap;

	private bool isActive;

	private static UICrosshair instance;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		UpdateSettings();
		Tween = DOTween.To(() => Accuracy, delegate(float x)
		{
			Accuracy = x;
		}, nValue.int5, nValue.int1).SetAutoKill(false);
		Tween.OnUpdate(delegate
		{
			if (isDynamic)
			{
				LeftSprite.cachedTransform.localPosition = Vector3.left * (Accuracy + Gap);
				RightSprite.cachedTransform.localPosition = Vector3.right * (Accuracy + Gap);
				TopSprite.cachedTransform.localPosition = Vector3.up * (Accuracy + Gap);
				BottomSprite.cachedTransform.localPosition = Vector3.down * (Accuracy + Gap);
			}
		});
		HitTween = DOTween.To(() => HitSprite.alpha, delegate(float x)
		{
			HitSprite.alpha = x;
		}, nValue.int0, HitDuration).SetAutoKill(false);
	}

	private void OnEnable()
	{
		isActive = true;
	}

	private void OnDisable()
	{
		isActive = false;
	}

	public static void SetAccuracy(float accuracy)
	{
		if (!(instance == null) && instance.isActive)
		{
			float num = accuracy * nValue.float15;
			if (instance.Tween != null)
			{
				instance.Tween.ChangeEndValue(num, true);
			}
			instance.Accuracy = num;
			instance.UpdateCrosshair();
			if (!instance.isDynamic)
			{
				instance.LeftSprite.cachedTransform.localPosition = Vector3.left * (num + instance.Gap);
				instance.RightSprite.cachedTransform.localPosition = Vector3.right * (num + instance.Gap);
				instance.TopSprite.cachedTransform.localPosition = Vector3.up * (num + instance.Gap);
				instance.BottomSprite.cachedTransform.localPosition = Vector3.down * (num + instance.Gap);
			}
		}
	}

	public static Vector2 Fire(float accuracy)
	{
		if (instance == null || !instance.isActive)
		{
			return new Vector2(Random.Range(0f - accuracy, accuracy), Random.Range(0f - accuracy, accuracy));
		}
		instance.FireAccuracy = Vector3.zero;
		if (accuracy != nValue.int0)
		{
			instance.FireAccuracy = new Vector2(instance.Accuracy / instance.AccuracyWidth, instance.Accuracy / instance.AccuracyHeight);
			UICrosshair uICrosshair = instance;
			uICrosshair.Accuracy = uICrosshair.Accuracy + accuracy * nValue.float15;
			instance.Accuracy = Mathf.Min(instance.Accuracy, instance.MaxAccuracy);
			instance.UpdateCrosshair();
		}
		return instance.FireAccuracy;
	}

	public static void SetMove(Vector2 move)
	{
		if (move.sqrMagnitude != nValue.int0)
		{
			UICrosshair uICrosshair = instance;
			uICrosshair.Accuracy = uICrosshair.Accuracy + move.sqrMagnitude;
			instance.Accuracy = Mathf.Min(instance.Accuracy, instance.MaxAccuracy);
			instance.UpdateCrosshair();
		}
	}

	private void UpdateCrosshair()
	{
		Tween.ChangeStartValue((float)Accuracy).Restart();
	}

	public static void Hit()
	{
		if (instance.HitMarker)
		{
			instance.HitSprite.alpha = nValue.int1;
			instance.HitTween.ChangeStartValue(instance.HitSprite.alpha).Restart();
		}
	}

	public static void SetActiveScope(bool active)
	{
		instance.RifleScope.SetActive(active);
		SetActiveCrosshair(!active);
	}

	public static void SetActiveCrosshair(bool active)
	{
		try
		{
			instance.Crosshair.SetActive(active);
		}
		catch
		{
		}
	}

	private void UpdateSettings()
	{
		HitMarker = Settings.HitMarker;
		int num = Mathf.FloorToInt(PlayerPrefs.GetFloat("CrosshairSize", 0.2f) * 40f + 4f);
		LeftSprite.width = num;
		RightSprite.width = num;
		TopSprite.height = num;
		BottomSprite.height = num;
		int num2 = Mathf.FloorToInt(PlayerPrefs.GetFloat("CrosshairThickness", 0.1f) * 20f + 2f);
		LeftSprite.height = num2;
		RightSprite.height = num2;
		TopSprite.width = num2;
		BottomSprite.width = num2;
		PointSprite.width = num2;
		PointSprite.height = num2;
		Gap = Mathf.FloorToInt(PlayerPrefs.GetFloat("CrosshairGap", 0f) * 20f);
		LeftSprite.cachedTransform.localPosition = Vector3.left * (Accuracy + Gap);
		RightSprite.cachedTransform.localPosition = Vector3.right * (Accuracy + Gap);
		TopSprite.cachedTransform.localPosition = Vector3.up * (Accuracy + Gap);
		BottomSprite.cachedTransform.localPosition = Vector3.down * (Accuracy + Gap);
		string[] array = PlayerPrefs.GetString("CrosshairColor", "1|1|1|1").Split("|"[0]);
		Color color = new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		LeftSprite.color = color;
		RightSprite.color = color;
		TopSprite.color = color;
		BottomSprite.color = color;
		PointSprite.color = color;
		float @float = PlayerPrefs.GetFloat("CrosshairAlpha", 1f);
		LeftSprite.alpha = @float;
		RightSprite.alpha = @float;
		TopSprite.alpha = @float;
		BottomSprite.alpha = @float;
		PointSprite.alpha = @float;
		PointSprite.cachedGameObject.SetActive(PlayerPrefs.GetInt("CrosshairPoint", 0) == 1);
		bool flag = PlayerPrefs.GetInt("CrosshairEnable", 1) == 1;
		LeftSprite.cachedGameObject.SetActive(flag);
		RightSprite.cachedGameObject.SetActive(flag);
		TopSprite.cachedGameObject.SetActive(flag);
		BottomSprite.cachedGameObject.SetActive(flag);
		isDynamic = PlayerPrefs.GetInt("CrosshairDynamics", 1) == 1;
	}
}
