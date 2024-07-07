using UnityEngine;
using UnityEngine.Events;

public class EventTrigger : MonoBehaviour
{
	public UnityEvent TriggerEnter;

	public UnityEvent TriggerExit;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerInput component = other.GetComponent<PlayerInput>();
			if (component != null)
			{
				TriggerEnter.Invoke();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerInput component = other.GetComponent<PlayerInput>();
			if (component != null)
			{
				TriggerExit.Invoke();
			}
		}
	}
}
