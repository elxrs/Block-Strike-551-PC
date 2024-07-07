using UnityEngine;

public class SurfFinish : MonoBehaviour
{
	public CryptoInt XP;

	public CryptoInt Money;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerInput component = other.GetComponent<PlayerInput>();
			if (component != null)
			{
				SurfMode.FinishMap(XP, Money);
			}
		}
	}
}
