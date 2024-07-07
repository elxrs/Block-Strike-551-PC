using UnityEngine;

public class TriggerTeleport : MonoBehaviour
{
	public Transform Target;

	public Vector3 Position;

	public DrawElements TargetElement;

	private void Start()
	{
		if (LevelManager.CustomMap)
		{
			Target = transform.Find("Finish");
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PlayerInput component = other.GetComponent<PlayerInput>();
		if (!(component == null))
		{
			if (Position != Vector3.zero)
			{
				component.Controller.SetPosition(Position);
			}
			else if (Target != null)
			{
				component.Controller.SetPosition(Target.position);
				component.FPCamera.SetRotation(Target.eulerAngles);
			}
			else
			{
				component.Controller.SetPosition(TargetElement.GetSpawnPosition());
			}
		}
	}
}
