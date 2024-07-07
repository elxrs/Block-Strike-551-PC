using UnityEngine;

public class RagdollObject : MonoBehaviour
{
	public float force;

	public Rigidbody[] playerLimbs;

	public MeshAtlas headAtlas;

	public MeshAtlas[] bodyAtlas;

	public MeshAtlas[] legsAtlas;

	public Transform playerWeaponRoot;

	public Transform player2WeaponRoot;

	[HideInInspector]
	public Vector3 defaultPosition;

	private Transform mTransform;

	private GameObject mGameObject;

	public Transform cachedTransform
	{
		get
		{
			if (mTransform == null)
			{
				mTransform = transform;
			}
			return mTransform;
		}
	}

	public GameObject cachedGameObject
	{
		get
		{
			if (mGameObject == null)
			{
				mGameObject = gameObject;
			}
			return mGameObject;
		}
	}

	public void SetSkin(UIAtlas atlas, string head, string body, string legs)
	{
		headAtlas.atlas = atlas;
		headAtlas.spriteName = "0-" + head;
		for (int i = 0; i < bodyAtlas.Length; i++)
		{
			bodyAtlas[i].atlas = atlas;
			bodyAtlas[i].spriteName = "1-" + body;
		}
		for (int j = 0; j < legsAtlas.Length; j++)
		{
			legsAtlas[j].atlas = atlas;
			legsAtlas[j].spriteName = "2-" + legs;
		}
	}

	public void Active(Vector3 f, Transform[] tr)
	{
		for (int i = 0; i < tr.Length; i++)
		{
			playerLimbs[i].position = tr[i].position;
			playerLimbs[i].rotation = tr[i].rotation;
			playerLimbs[i].isKinematic = false;
			playerLimbs[i].velocity = Vector3.zero;
			playerLimbs[i].AddForce(f * force);
			try
			{
				BoxCollider collider = FindObjectOfType<PlayerTriggerDetector>().GetComponent<BoxCollider>();
				if (playerLimbs[i].gameObject.GetComponent<BoxCollider>() != null)
				{
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<BoxCollider>(), PlayerInput.instance.mCharacterController);
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<BoxCollider>(), collider);
				}
				if (playerLimbs[i].gameObject.GetComponent<CapsuleCollider>() != null)
				{
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<CapsuleCollider>(), PlayerInput.instance.mCharacterController);
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<CapsuleCollider>(), collider);
				}
				if (playerLimbs[i].gameObject.GetComponent<SphereCollider>() != null)
				{
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<SphereCollider>(), PlayerInput.instance.mCharacterController);
					Physics.IgnoreCollision(playerLimbs[i].gameObject.GetComponent<SphereCollider>(), collider);
				}
			}
			catch
			{
			}
		}
	}

	public void Deactive()
	{
		for (int i = 0; i < playerLimbs.Length; i++)
		{
			playerLimbs[i].isKinematic = true;
		}
		playerLimbs[0].position = defaultPosition;
	}
}
