using System;
using UnityEngine;

[ExecuteInEditMode]
public class nBoxCollider : nCollider
{
	public bool fastHit;

	private Vector3[] coordinates = new Vector3[8]
	{
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f)
	};

	private Vector3[] box = new Vector3[8];

	private void Awake()
	{
		distances = new float[12]
		{
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue
		};
	}

	public override bool Raycast(Ray ray, float _distance, bool update)
	{
		for (int i = 0; i < 12; i++)
		{
			distances[i] = float.MaxValue;
		}
		if (!visible)
		{
			return false;
		}
		if (update)
		{
			UpdateCoordinates();
		}
		distances[0] = nRaycast.Intersect(box[0], box[3], box[1], ray);
		if (fastHit && distances[0] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[1] = nRaycast.Intersect(box[0], box[2], box[3], ray);
		if (fastHit && distances[1] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[2] = nRaycast.Intersect(box[2], box[5], box[3], ray);
		if (fastHit && distances[2] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[3] = nRaycast.Intersect(box[2], box[4], box[5], ray);
		if (fastHit && distances[3] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[4] = nRaycast.Intersect(box[4], box[7], box[5], ray);
		if (fastHit && distances[4] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[5] = nRaycast.Intersect(box[4], box[6], box[7], ray);
		if (fastHit && distances[5] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[6] = nRaycast.Intersect(box[6], box[1], box[7], ray);
		if (fastHit && distances[6] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[7] = nRaycast.Intersect(box[6], box[0], box[1], ray);
		if (fastHit && distances[7] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[8] = nRaycast.Intersect(box[1], box[5], box[7], ray);
		if (fastHit && distances[8] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[9] = nRaycast.Intersect(box[1], box[3], box[5], ray);
		if (fastHit && distances[9] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[10] = nRaycast.Intersect(box[6], box[2], box[0], ray);
		if (fastHit && distances[10] <= _distance)
		{
			return Sorting(_distance);
		}
		distances[11] = nRaycast.Intersect(box[6], box[4], box[2], ray);
		return Sorting(_distance);
	}

	private bool Sorting(float _distance)
	{
		Array.Sort(distances, (float x, float y) => x.CompareTo(y));
		if (distance == float.MaxValue)
		{
			return false;
		}
		if (distance >= _distance)
		{
			return false;
		}
		return true;
	}

	public override void UpdateCoordinates()
	{
		for (int i = 0; i < 8; i++)
		{
			box[i].x = (coordinates[i].x + center.x) * size.x;
			box[i].y = (coordinates[i].y + center.y) * size.y;
			box[i].z = (coordinates[i].z + center.z) * size.z;
			box[i] = cachedTransform.TransformPoint(box[i]);
		}
	}
}
