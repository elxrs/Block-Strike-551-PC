using UnityEngine;

public class WaterSystem : MonoBehaviour
{
	public bool freeGravity;

	private void OnTriggerEnter(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (!(component == null))
		{
			component.SetWater(true, freeGravity);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (!(component == null))
		{
			component.SetWater(false);
		}
	}
}
