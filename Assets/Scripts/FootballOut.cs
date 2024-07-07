using UnityEngine;

public class FootballOut : MonoBehaviour
{
	public FootballMode Target;

	private void OnTriggerEnter(Collider other)
	{
		if (PhotonNetwork.isMasterClient && other.CompareTag("RigidbodyObject"))
		{
			FootballManager.StartRound();
		}
		if (other.CompareTag("Player"))
		{
			if (GameManager.GetRoundState() == RoundState.PlayRound)
			{
				PlayerInput.instance.SetMove(false, nValue.int5);
			}
			Target.OnCreatePlayer();
		}
	}
}
