using UnityEngine;

public class UICrosshairSettings : MonoBehaviour
{
	public UISprite[] Crosshair;

	public UISprite Point;

	public UIToggle DynamicsToogle;

	public UIToggle PointToogle;

	public UIToggle CrosshairToogle;

	public UILabel SizeLabel;

	public UILabel ThicknessLabel;

	public UILabel GapLabel;

	public UILabel AlphaLabel;

	public UISlider SizeSlider;

	public UISlider ThicknessSlider;

	public UISlider GapSlider;

	public UISlider AlphaSlider;

	public UIColorPicker ColorPicker;

	private void Awake()
	{
		DynamicsToogle.value = PlayerPrefs.GetInt("CrosshairDynamics", 1) == 1;
		PointToogle.value = PlayerPrefs.GetInt("CrosshairPoint", 0) == 1;
		CrosshairToogle.value = PlayerPrefs.GetInt("CrosshairEnable", 1) == 1;
		SizeSlider.value = PlayerPrefs.GetFloat("CrosshairSize", 0.2f);
		ThicknessSlider.value = PlayerPrefs.GetFloat("CrosshairThickness", 0.1f);
		GapSlider.value = PlayerPrefs.GetFloat("CrosshairGap", 0f);
		AlphaSlider.value = PlayerPrefs.GetFloat("CrosshairAlpha", 1f);
		string[] array = PlayerPrefs.GetString("CrosshairColor", "1|1|1|1").Split("|"[0]);
		ColorPicker.value = new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		UpdateAll();
	}

	public void SetSize()
	{
		int num = Mathf.FloorToInt(SizeSlider.value * 40f + 4f);
		SizeLabel.text = Localization.Get("Size") + ": " + num;
		Crosshair[0].width = num;
		Crosshair[1].width = num;
		Crosshair[2].height = num;
		Crosshair[3].height = num;
		PlayerPrefs.SetFloat("CrosshairSize", SizeSlider.value);
	}

	public void SetThickness()
	{
		int num = Mathf.FloorToInt(ThicknessSlider.value * 20f + 2f);
		ThicknessLabel.text = Localization.Get("Thickness") + ": " + num;
		Crosshair[0].height = num;
		Crosshair[1].height = num;
		Crosshair[2].width = num;
		Crosshair[3].width = num;
		Point.width = num;
		Point.height = num;
		PlayerPrefs.SetFloat("CrosshairThickness", ThicknessSlider.value);
	}

	public void SetGap()
	{
		int num = Mathf.FloorToInt(GapSlider.value * 20f) + 10;
		GapLabel.text = Localization.Get("Gap") + ": " + (num - 10);
		Crosshair[0].cachedTransform.localPosition = Vector3.left * num;
		Crosshair[1].cachedTransform.localPosition = Vector3.right * num;
		Crosshair[2].cachedTransform.localPosition = Vector3.up * num;
		Crosshair[3].cachedTransform.localPosition = Vector3.down * num;
		PlayerPrefs.SetFloat("CrosshairGap", GapSlider.value);
	}

	public void SetAlpha()
	{
		float alpha = Mathf.Clamp(AlphaSlider.value, 0.01f, 1f);
		AlphaLabel.text = Localization.Get("Alpha") + ": " + alpha.ToString("f2");
		Crosshair[0].alpha = alpha;
		Crosshair[1].alpha = alpha;
		Crosshair[2].alpha = alpha;
		Crosshair[3].alpha = alpha;
		Point.alpha = alpha;
		PlayerPrefs.SetFloat("CrosshairAlpha", AlphaSlider.value);
	}

	public void SetColor()
	{
		Color value = ColorPicker.value;
		Crosshair[0].color = value;
		Crosshair[1].color = value;
		Crosshair[2].color = value;
		Crosshair[3].color = value;
		Point.color = value;
		PlayerPrefs.SetString("CrosshairColor", value.r + "|" + value.g + "|" + value.b + "|" + value.a);
	}

	public void SetPoint()
	{
		Point.cachedGameObject.SetActive(PointToogle.value);
		PlayerPrefs.SetInt("CrosshairPoint", PointToogle.value ? 1 : 0);
	}

	public void SetDynamics()
	{
		PlayerPrefs.SetInt("CrosshairDynamics", DynamicsToogle.value ? 1 : 0);
	}

	public void SetCrosshair()
	{
		Crosshair[0].cachedGameObject.SetActive(CrosshairToogle.value);
		Crosshair[1].cachedGameObject.SetActive(CrosshairToogle.value);
		Crosshair[2].cachedGameObject.SetActive(CrosshairToogle.value);
		Crosshair[3].cachedGameObject.SetActive(CrosshairToogle.value);
		PlayerPrefs.SetInt("CrosshairEnable", CrosshairToogle.value ? 1 : 0);
	}

	public void SetDefault()
	{
		DynamicsToogle.value = true;
		PointToogle.value = false;
		CrosshairToogle.value = true;
		SizeSlider.value = 0.2f;
		ThicknessSlider.value = 0.1f;
		GapSlider.value = 0f;
		AlphaSlider.value = 1f;
		ColorPicker.Select(Color.white);
		PlayerPrefs.SetInt("CrosshairDynamics", 1);
		PlayerPrefs.SetInt("CrosshairPoint", 0);
		PlayerPrefs.SetInt("CrosshairEnable", 1);
		PlayerPrefs.SetFloat("CrosshairSize", 0.2f);
		PlayerPrefs.SetFloat("CrosshairThickness", 0.1f);
		PlayerPrefs.SetFloat("CrosshairGap", 0f);
		PlayerPrefs.SetFloat("CrosshairAlpha", 1f);
		PlayerPrefs.SetString("CrosshairColor", "1|1|1|1");
		UpdateAll();
	}

	private void UpdateAll()
	{
		SetSize();
		SetThickness();
		SetGap();
		SetAlpha();
		SetColor();
		SetPoint();
		SetDynamics();
		SetCrosshair();
	}
}
