using UnityEngine;

public class DissolveTextureChange : MonoBehaviour
{
	public Texture2D mainTexNormal;

	public Texture2D secondTexNormal;

	private Color dissolveEmissionColor;

	private float dissolveEmissionThickness = -0.02f;

	private float dissolvePower = 0.6f;

	private bool mainNormal;

	private bool secNormal;

	private void Start()
	{
	}

	private void Update()
	{
		GetComponent<Renderer>().material.SetColor("_DissolveEmissionColor", dissolveEmissionColor);
		GetComponent<Renderer>().material.SetFloat("_DissolveEmissionThickness", dissolveEmissionThickness);
		GetComponent<Renderer>().material.SetFloat("_DissolvePower", dissolvePower);
		if (mainNormal)
		{
			GetComponent<Renderer>().material.SetTexture("_MainTexNormal", mainTexNormal);
		}
		else
		{
			GetComponent<Renderer>().material.SetTexture("_MainTexNormal", null);
		}
		if (secNormal)
		{
			GetComponent<Renderer>().material.SetTexture("_SecondTexNormal", secondTexNormal);
		}
		else
		{
			GetComponent<Renderer>().material.SetTexture("_SecondTexNormal", null);
		}
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 40f, 200f, 20f), "Dissolve Power");
		dissolvePower = GUI.HorizontalSlider(new Rect(10f, 60f, 200f, 20f), dissolvePower, 0.6f, -0.2f);
		GUI.Label(new Rect(10f, 80f, 200f, 20f), "Dissolve Emission Thickness");
		dissolveEmissionThickness = GUI.HorizontalSlider(new Rect(10f, 100f, 200f, 20f), dissolveEmissionThickness, -0.01f, -0.026f);
		GUI.Label(new Rect(10f, 120f, 200f, 20f), "Dissolve Emission Color");
		dissolveEmissionColor.r = GUI.HorizontalSlider(new Rect(10f, 140f, 200f, 20f), dissolveEmissionColor.r, 0f, 1f);
		dissolveEmissionColor.g = GUI.HorizontalSlider(new Rect(10f, 160f, 200f, 20f), dissolveEmissionColor.g, 0f, 1f);
		dissolveEmissionColor.b = GUI.HorizontalSlider(new Rect(10f, 180f, 200f, 20f), dissolveEmissionColor.b, 0f, 1f);
		mainNormal = GUI.Toggle(new Rect(10f, 200f, 200f, 20f), mainNormal, "Main texture normal map");
		secNormal = GUI.Toggle(new Rect(10f, 220f, 200f, 20f), secNormal, "Second texture normal map");
	}
}
