using System.Collections.Generic;
using UnityEngine;

public static class nRaycast
{
	public static Transform hitTransform;

	public static float hitDistance;

	public static Vector3 hitPoint;

	private static RaycastHit hitInfo;

	public static List<nCollider> boxList = new List<nCollider>();

	private static List<nCollider> contactList = new List<nCollider>();

	private static List<nCollider> contactList2 = new List<nCollider>();

	private static Vector3 e1;

	private static Vector3 e2;

	private static Vector3 p;

	private static Vector3 q;

	private static Vector3 t;

	private static float det;

	private static float invDet;

	private static float u;

	private static float v;

	private static float result;

	public static bool Raycast(Ray ray, float distance)
	{
		return Raycast(ray, distance, string.Empty, -5);
	}

	public static bool Raycast(Ray ray, float distance, string layer)
	{
		return Raycast(ray, distance, layer, -5);
	}

	public static bool Raycast(Ray ray, float distance, int layerMask)
	{
		return Raycast(ray, distance, string.Empty, layerMask);
	}

	public static bool Raycast(Ray ray, float distance, string layer, int layerMask)
	{
		contactList.Clear();
		if (boxList.Count == 0)
		{
			return false;
		}
		if (string.IsNullOrEmpty(layer))
		{
			for (int i = 0; i < boxList.Count; i++)
			{
				if (boxList[i] != null && boxList[i].Raycast(ray, distance, true))
				{
					contactList.Add(boxList[i]);
				}
			}
		}
		else
		{
			for (int j = 0; j < boxList.Count; j++)
			{
				if (boxList[j] != null && boxList[j].layer == layer && boxList[j].Raycast(ray, distance, true))
				{
					contactList.Add(boxList[j]);
				}
			}
		}
		if (contactList.Count == 0)
		{
			return false;
		}
		if (contactList.Count > 1)
		{
			contactList.Sort(SortBox);
		}
		if (Physics.Raycast(ray, out hitInfo, distance, layerMask) && hitInfo.distance < contactList[0].distance)
		{
			return false;
		}
		hitTransform = contactList[0].cachedTransform;
		hitDistance = contactList[0].distance;
		hitPoint = ray.GetPoint(hitDistance);
		return true;
	}

	public static bool RaycastFire(Ray ray, float distance, string layer, string layer2, string layer3, int layerMask)
	{
		contactList.Clear();
		if (boxList.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < boxList.Count; i++)
		{
			if (boxList[i].layer == layer && boxList[i].visible && boxList[i].Raycast(ray, distance, true))
			{
				contactList.Add(boxList[i]);
			}
		}
		if (contactList.Count == 0)
		{
			for (int j = 0; j < boxList.Count; j++)
			{
				if (boxList[j].layer == layer2 && boxList[j].visible && boxList[j].Raycast(ray, distance, true))
				{
					contactList.Add(boxList[j]);
				}
			}
		}
		if (contactList.Count == 0)
		{
			return false;
		}
		if (contactList.Count > 1)
		{
			contactList.Sort(SortBox);
		}
		for (int k = 0; k < contactList.Count; k++)
		{
			contactList2.Clear();
			string name = contactList[k].cachedTransform.root.name;
			for (int l = 0; l < boxList.Count; l++)
			{
				if (boxList[l].layer == layer3 && name == boxList[l].cachedTransform.root.name && boxList[l].Raycast(ray, distance, true))
				{
					contactList2.Add(boxList[l]);
				}
			}
			if (contactList2.Count != 0)
			{
				break;
			}
		}
		if (contactList2.Count == 0)
		{
			return false;
		}
		if (contactList.Count > 1)
		{
			contactList.Sort(SortBox);
		}
		if (Physics.Raycast(ray, out hitInfo, distance, layerMask) && hitInfo.distance < contactList2[0].distance)
		{
			return false;
		}
		hitTransform = contactList2[0].cachedTransform;
		hitDistance = contactList2[0].distance;
		hitPoint = ray.GetPoint(hitDistance);
		return true;
	}

	public static bool RaycastCheck(Ray ray, Transform cam, float distance, string layer, int layerMask)
	{
		contactList.Clear();
		if (boxList.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < boxList.Count; i++)
		{
			if (boxList[i].layer == layer && boxList[i].Raycast(ray, distance, true))
			{
				contactList.Add(boxList[i]);
			}
		}
		if (contactList.Count == 0)
		{
			return false;
		}
		if (contactList.Count > 1)
		{
			contactList.Sort(SortBox);
		}
		if (Physics.Raycast(ray, out hitInfo, distance, layerMask) && hitInfo.distance < contactList[0].distance)
		{
			return false;
		}
		if (cam == contactList[0].cachedTransform)
		{
			return true;
		}
		return false;
	}

	private static int SortBox(nCollider a, nCollider b)
	{
		return a.distance.CompareTo(b.distance);
	}

	public static float Intersect(Vector3 p1, Vector3 p2, Vector3 p3, Ray ray)
	{
		Vector3 vector = p2 - p1;
		Vector3 vector2 = p3 - p1;
		Vector3 rhs = Vector3.Cross(ray.direction, vector2);
		float num = Vector3.Dot(vector, rhs);
		if (num > -1E-45f && num < float.Epsilon)
		{
			return float.MaxValue;
		}
		float num2 = 1f / num;
		Vector3 lhs = ray.origin - p1;
		float num3 = Vector3.Dot(lhs, rhs) * num2;
		if (num3 < 0f || num3 > 1f)
		{
			return float.MaxValue;
		}
		Vector3 rhs2 = Vector3.Cross(lhs, vector);
		float num4 = Vector3.Dot(ray.direction, rhs2) * num2;
		if (num4 < 0f || num3 + num4 > 1f)
		{
			return float.MaxValue;
		}
		float num5 = Vector3.Dot(vector2, rhs2) * num2;
		if (num5 > float.Epsilon)
		{
			return num5;
		}
		return float.MaxValue;
	}
}
