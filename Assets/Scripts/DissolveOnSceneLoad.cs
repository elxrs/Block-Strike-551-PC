using UnityEngine;

public class DissolveOnSceneLoad : MonoBehaviour
{
	public float dissolveSpeed = 0.1f;

	private float sliceAmount;

	private bool dissolve;

	private void Start()
	{
		dissolve = true;
	}

	private void Update()
	{
		if (dissolve)
		{
			sliceAmount -= Time.deltaTime * dissolveSpeed;
			transform.GetComponent<Renderer>().material.SetFloat("_DissolvePower", 0.65f + Mathf.Sin(0.9f) * sliceAmount);
			if (GetComponent<Renderer>().material.GetFloat("_DissolvePower") < -0.5f)
			{
				dissolve = false;
			}
		}
	}
}
