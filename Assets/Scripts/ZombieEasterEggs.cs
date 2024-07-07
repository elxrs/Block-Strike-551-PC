using UnityEngine;

public class ZombieEasterEggs : MonoBehaviour
{
	[Disabled]
	public bool Complete;

	public Transform TeleportPosition;

	public ZombieEasterEggs[] OtherEggs;

	private bool isTrigger;

	private void Start()
	{
		if (TeleportPosition != null)
		{
			EventManager.AddListener("StartRound", StartRound);
		}
	}

	private void StartRound()
	{
		Complete = false;
		for (int i = 0; i < OtherEggs.Length; i++)
		{
			OtherEggs[i].Complete = false;
		}
	}

	private void OnEnable()
	{
		InputManager.GetButtonDownEvent += GetButtonDown;
	}

	private void OnDisable()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
	}

	private void GetButtonDown(string name)
	{
		if (!(name == "Fire") || !isTrigger)
		{
			return;
		}
		Complete = true;
		if (OtherEggs != null)
		{
			for (int i = 0; i < OtherEggs.Length; i++)
			{
				if (!OtherEggs[i].Complete)
				{
					return;
				}
			}
		}
		if (TeleportPosition != null)
		{
			PlayerInput.instance.Controller.SetPosition(TeleportPosition.position);
			PlayerInput.instance.FPCamera.SetRotation(TeleportPosition.eulerAngles);
			StartRound();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (!(component == null))
		{
			isTrigger = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (component != null)
		{
			isTrigger = false;
		}
	}
}
