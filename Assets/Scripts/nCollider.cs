using UnityEngine;

public class nCollider : MonoBehaviour
{
	public string layer;

	public Vector3 center = Vector3.zero;

	public Vector3 size = Vector3.one;

	[HideInInspector]
	public float[] distances;

	public bool distancesAwake;

	private bool mVisible = true;

	private Transform mTransform;

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

	public float distance
	{
		get
		{
			return distances[0];
		}
	}

	public bool visible
	{
		get
		{
			return mVisible;
		}
	}

	private void OnEnable()
	{
		nRaycast.boxList.Add(this);
	}

	private void OnDisable()
	{
		nRaycast.boxList.Remove(this);
	}

	private void OnBecameVisible()
	{
		mVisible = true;
	}

	private void OnBecameInvisible()
	{
		mVisible = false;
	}

	public virtual bool Raycast(Ray ray, float _distance, bool update)
	{
		return Raycast(ray, _distance, update, true);
	}

	public virtual bool Raycast(Ray ray, float _distance, bool update, bool checkVisible)
	{
		return false;
	}

	public virtual void UpdateCoordinates()
	{
	}
}
