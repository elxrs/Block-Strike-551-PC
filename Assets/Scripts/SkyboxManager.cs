using UnityEngine;

[ExecuteInEditMode]
public class SkyboxManager : MonoBehaviour
{
	[Range(0f, 1f)]
	public float TimeDay;

	public bool Moon;

	public bool Stars;

	public bool Sun;

	public bool Clouds;

	public Material SkyboxMaterial;

	public Transform SkyboxCamera;

	public Transform SkyboxCameraParent;

	public GameObject MoonObject;

	public GameObject StarsObject;

	public GameObject SunObject;

	public GameObject CloudsObject;

	private static SkyboxManager instance;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			SkyboxMaterial.mainTextureOffset = new Vector2(TimeDay, 0f);
			MoonObject.SetActive(Moon);
			StarsObject.SetActive(Stars);
			SunObject.SetActive(Sun);
			CloudsObject.SetActive(Clouds);
			if (LevelManager.GetSceneName() == "Shooting Range")
			{
				nConsole.AddCommand("timeday", nValueType.Int, SetTimeDay);
			}
		}
	}

	private void Reset()
	{
		SkyboxMaterial.mainTextureOffset = new Vector2(TimeDay, 0f);
	}

	private void OnDisable()
	{
		if (LevelManager.GetSceneName() == "Shooting Range")
		{
			nConsole.Remove("timeday");
		}
	}

	public static Transform GetCamera()
	{
		return instance.SkyboxCamera;
	}

	public static Transform GetCameraParent()
	{
		return instance.SkyboxCameraParent;
	}

	private void SetTimeDay(string command, object value)
	{
		TimeDay = 100f / Mathf.Clamp((int)value, 0, 100);
		SkyboxMaterial.mainTextureOffset = new Vector2(TimeDay, 0f);
	}
}
