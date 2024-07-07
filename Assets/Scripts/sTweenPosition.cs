using UnityEngine;

public class sTweenPosition : MonoBehaviour
{
	public float speed = 1f;

	public Vector3 vector = Vector3.one;

	public float delay;

	public bool debug;

	private Transform mTransform;

	private Vector3 startPosition;

	private void Start()
	{
		mTransform = transform;
		startPosition = transform.localPosition;
	}

	private void FixedUpdate()
	{
		mTransform.localPosition = startPosition + vector * Mathf.PingPong((sTweenTimer.time + delay) * speed, 1f);
	}
}
