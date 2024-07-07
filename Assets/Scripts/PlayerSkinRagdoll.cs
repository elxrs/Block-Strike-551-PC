using UnityEngine;

public class PlayerSkinRagdoll : TimerBehaviour
{
	public bool tweenEffect;

	public PlayerSkin Player;

	public float Force = 150f;

	public Collider[] Colliders;

	public Rigidbody[] Rigidbodies;

	public Transform[] Transforms;

	public Vector3[] Positions;

	public Quaternion[] Rotations;

	private bool Actived;

	private void Start()
	{
		for (int i = 0; i < Rigidbodies.Length; i++)
		{
			Rigidbodies[i].detectCollisions = false;
		}
	}
}
