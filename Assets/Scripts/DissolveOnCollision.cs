using UnityEngine;

public class DissolveOnCollision : MonoBehaviour
{
	public Shader dissolveShader;

	public Texture2D dissolvePattern;

	public Color dissolveEmissionColor;

	public float dissolveSpeed = 0.1f;

	public bool checkForTag;

	public string objectTag;

	private Transform collidedObject;

	private float sliceAmount;

	private bool dissolve;

	private bool collided;

	private void Start()
	{
	}

	private void Update()
	{
		if (collided)
		{
			collidedObject.GetComponent<Renderer>().material.shader = dissolveShader;
			collidedObject.GetComponent<Renderer>().material.SetColor("_DissolveEmissionColor", dissolveEmissionColor);
			collidedObject.GetComponent<Renderer>().material.SetFloat("_DissolveEmissionThickness", -0.05f);
			collidedObject.GetComponent<Renderer>().material.SetTexture("_DissolveTex", dissolvePattern);
			dissolve = true;
		}
		if (dissolve)
		{
			sliceAmount -= Time.deltaTime * dissolveSpeed;
			collidedObject.GetComponent<Renderer>().material.SetFloat("_DissolvePower", 0.65f + Mathf.Sin(0.9f) * sliceAmount);
			if (collidedObject.GetComponent<Renderer>().material.GetFloat("_DissolvePower") < -0.1f)
			{
				collidedObject.GetComponent<Collider>().enabled = false;
				Destroy(gameObject);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (checkForTag)
		{
			if (other.transform.tag == objectTag)
			{
				collided = true;
				collidedObject = other.transform;
				transform.GetComponent<Renderer>().enabled = false;
				other.transform.tag = null;
				GetComponent<Collider>().enabled = false;
			}
		}
		else
		{
			collided = true;
			collidedObject = other.transform;
		}
	}
}
