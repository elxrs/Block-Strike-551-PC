using UnityEngine;

public class BunnyAutoJump : MonoBehaviour
{
	private PlayerInput Player;

	public CryptoInt jumpTime = 2;

	private int TimerID;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player"))
		{
			return;
		}
		Player = other.GetComponent<PlayerInput>();
		if (Player != null)
		{
			Player.SetBunnyHopAutoJump(true);
			TimerID = TimerManager.In(jumpTime, delegate
			{
				BunnyHop.SpawnDead();
			});
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") && Player != null)
		{
			Player.SetBunnyHopAutoJump(false);
			Player = null;
			TimerManager.Cancel(TimerID);
		}
	}
}
