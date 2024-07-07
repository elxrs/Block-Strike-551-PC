using UnityEngine;

public class sTweenRotation : MonoBehaviour
{
	public Vector3 speed = Vector3.forward;

	private Transform mTransform;

	private Vector3 startEulerAngles;

	private void Start()
	{
		mTransform = transform;
		startEulerAngles = transform.localEulerAngles;
	}

	private void FixedUpdate()
	{
		mTransform.localEulerAngles = startEulerAngles + speed * Time.time * 10f;
	}
}
