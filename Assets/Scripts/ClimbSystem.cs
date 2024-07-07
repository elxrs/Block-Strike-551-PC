using UnityEngine;

public class ClimbSystem : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (!(component == null))
		{
			component.SetClimb(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (!(component == null))
		{
			component.SetClimb(false);
		}
	}
}
