using UnityEngine;

public class TNTRunBlock : MonoBehaviour
{
	public int id;

	public BoxCollider Trigger;

	public MeshRenderer Render;

	private bool Active = true;

	private void OnTriggerEnter(Collider other)
	{
		if (GameManager.GetRoundState() == RoundState.PlayRound && Active && other.CompareTag("Player"))
		{
			TimerManager.In(0.5f, delegate
			{
				TNTRunManager.DeleteBlock(id);
				SetActive(false);
			});
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		print("OnCollisionEnter");
	}

	public void SetActive(bool active)
	{
		Trigger.isTrigger = !active;
		Render.enabled = active;
		Active = active;
	}
}
