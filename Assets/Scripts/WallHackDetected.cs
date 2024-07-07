using UnityEngine;

public class WallHackDetected : MonoBehaviour
{
	private CharacterController controller;

	private Transform controllerTransform;

	private bool controllerVelocity;

	private byte controllerDetect;

	private byte controllerDetect2;

	private Transform boxCollider;

	private void Start()
	{
		GameObject gameObject = new GameObject("CharacterController");
		controller = gameObject.AddComponent<CharacterController>();
		controllerTransform = gameObject.transform;
		controllerTransform.SetParent(transform);
		controllerTransform.localPosition = Vector3.down * nValue.int1000;
		gameObject = new GameObject("BoxCollider");
		BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
		this.boxCollider = gameObject.transform;
		boxCollider.size = new Vector3(nValue.int3, nValue.int3, nValue.float05);
		this.boxCollider.SetParent(transform);
		this.boxCollider.localPosition = Vector3.down * nValue.int1000;
		TimerManager.In(nValue.int1, false, -nValue.int1, nValue.int5, WallHackStart);
	}

	private void WallHackStart()
	{
		controllerTransform.localPosition = new Vector3(nValue.int0, -nValue.int1000, -nValue.int1);
		controllerVelocity = true;
		TimerManager.In(nValue.int4, WallHackDistance);
	}

	private void WallHackDistance()
	{
		if (Vector3.Distance(controllerTransform.localPosition, boxCollider.localPosition) > 1f)
		{
			controllerDetect2++;
			if (controllerDetect2 >= nValue.int3)
			{
				CheckManager.Detected("Wall Hack Detected 2");
			}
		}
	}

	private void LateUpdate()
	{
		if (!controllerVelocity)
		{
			return;
		}
		controller.Move(new Vector3(Random.Range(-0.002f, 0.002f), nValue.int0, Random.Range(nValue.float001, nValue.float008)));
		if (controllerTransform.localPosition.z > nValue.int0)
		{
			controllerDetect++;
			controllerVelocity = false;
			if (controllerDetect >= nValue.int3)
			{
				CheckManager.Detected("Wall Hack Detected");
			}
		}
	}
}
