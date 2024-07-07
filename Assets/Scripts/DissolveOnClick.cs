using UnityEngine;

public class DissolveOnClick : MonoBehaviour
{
	public Shader dissolveShader;

	public Texture2D dissolvePattern;

	public Color dissolveEmissionColor;

	public float dissolveSpeed = 0.1f;

	private float sliceAmount;

	private bool dissolve;

	private bool mouseOver;

	private void Update()
	{
		if (mouseOver && Input.GetMouseButtonUp(0))
		{
			transform.GetComponent<Renderer>().material.shader = dissolveShader;
			transform.GetComponent<Renderer>().material.SetColor("_DissolveEmissionColor", dissolveEmissionColor);
			transform.GetComponent<Renderer>().material.SetFloat("_DissolveEmissionThickness", -0.05f);
			transform.GetComponent<Renderer>().material.SetTexture("_DissolveTex", dissolvePattern);
			transform.GetComponent<Renderer>().material.SetTextureOffset("_DissolveTex", new Vector2(Random.Range(1f, 10f), Random.Range(1f, 10f)));
			dissolve = true;
		}
		if (dissolve)
		{
			sliceAmount -= Time.deltaTime * dissolveSpeed;
			transform.GetComponent<Renderer>().material.SetFloat("_DissolvePower", 0.65f + Mathf.Sin(0.9f) * sliceAmount);
			if (GetComponent<Renderer>().material.GetFloat("_DissolvePower") < 0.2f)
			{
				dissolve = false;
			}
		}
	}

	private void OnMouseEnter()
	{
		mouseOver = true;
	}

	private void OnMouseExit()
	{
		mouseOver = false;
	}
}
