using UnityEngine;

public class nSpriteCollider : nCollider
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	public Axis axis;

	private Vector3[] coordinatesX = new Vector3[4]
	{
		new Vector3(0.5f, -0.5f, 0f),
		new Vector3(-0.5f, -0.5f, 0f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(-0.5f, 0.5f, 0f)
	};

	private Vector3[] coordinatesY = new Vector3[4]
	{
		new Vector3(0.5f, 0f, -0.5f),
		new Vector3(-0.5f, 0f, -0.5f),
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(-0.5f, 0f, 0.5f)
	};

	private Vector3[] coordinatesZ = new Vector3[4]
	{
		new Vector3(0f, -0.5f, 0.5f),
		new Vector3(0f, -0.5f, -0.5f),
		new Vector3(0f, 0.5f, 0.5f),
		new Vector3(0f, 0.5f, -0.5f)
	};

	private Vector3[] box = new Vector3[4];

	private Vector3[] coordinates
	{
		get
		{
			if (axis == Axis.X)
			{
				return coordinatesX;
			}
			if (axis == Axis.Y)
			{
				return coordinatesY;
			}
			return coordinatesZ;
		}
	}

	public override bool Raycast(Ray ray, float _distance, bool update, bool checkVisible)
	{
		if (!distancesAwake)
		{
			distancesAwake = true;
			distances = new float[2]
			{
				float.MaxValue,
				float.MaxValue
			};
		}
		distances[0] = float.MaxValue;
		distances[1] = float.MaxValue;
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
			if (distances[1] == float.MaxValue)
			{
				return false;
			}
			distances[0] = distances[1];
			distances[1] = float.MaxValue;
		}
		if (distance > _distance)
		{
			return false;
		}
		return true;
	}

	public override void UpdateCoordinates()
	{
		for (int i = 0; i < 4; i++)
		{
			box[i].x = (coordinates[i].x + center.x) * size.x;
			box[i].y = (coordinates[i].y + center.y) * size.y;
			box[i].z = (coordinates[i].z + center.z) * size.z;
			box[i] = cachedTransform.TransformPoint(box[i]);
		}
	}
}
