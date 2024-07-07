using System;
using UnityEngine;

public class nSpriteCollider2 : nCollider
{
	private Vector3[] coordinates = new Vector3[8]
	{
		new Vector3(0.5f, -0.5f, 0f),
		new Vector3(-0.5f, -0.5f, 0f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(-0.5f, 0.5f, 0f),
		new Vector3(0f, -0.5f, 0.5f),
		new Vector3(0f, -0.5f, -0.5f),
		new Vector3(0f, 0.5f, 0.5f),
		new Vector3(0f, 0.5f, -0.5f)
	};

	private Vector3[] box = new Vector3[8];

	private void Start()
	{
		distances = new float[4]
		{
			float.MaxValue,
			float.MaxValue,
			float.MaxValue,
			float.MaxValue
		};
	}

	public override bool Raycast(Ray ray, float _distance, bool update, bool checkVisible)
	{
		for (int i = 0; i < 4; i++)
		{
			distances[i] = float.MaxValue;
		}
		if (!visible && checkVisible)
		{
			return false;
		}
		if (update)
		{
			UpdateCoordinates();
		}
		distances[0] = nRaycast.Intersect(box[0], box[3], box[1], ray);
		if (distances[0] == float.MaxValue)
		{
			distances[1] = nRaycast.Intersect(box[0], box[2], box[3], ray);
		}
		distances[2] = nRaycast.Intersect(box[4], box[7], box[5], ray);
		if (distances[2] == float.MaxValue)
		{
			distances[3] = nRaycast.Intersect(box[4], box[6], box[7], ray);
		}
		Array.Sort(distances, SortDistances);
		if (distance == float.MaxValue)
		{
			return false;
		}
		if (distance > _distance)
		{
			return false;
		}
		return true;
	}

	private static int SortDistances(float a, float b)
	{
		return a.CompareTo(b);
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
