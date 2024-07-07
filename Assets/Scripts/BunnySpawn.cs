using UnityEngine;

public class BunnySpawn : MonoBehaviour
{
	public bool FinishSpawn;

	public CryptoInt XP;

	public CryptoInt Money;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player"))
		{
			return;
		}
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (component != null)
		{
			if (FinishSpawn)
			{
				BunnyHop.FinishMap(XP, Money);
				return;
			}
			GameManager.GetTeamSpawn().GetTransform().position = transform.position;
			GameManager.GetTeamSpawn().GetTransform().rotation = transform.rotation;
		}
	}
}
