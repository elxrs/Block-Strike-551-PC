using System.Collections.Generic;
using UnityEngine;

public class DecalInfo
{
	public byte BloodDecal = 200;

	public bool isKnife;

	public List<Vector3> Points = new List<Vector3>();

	public List<Vector3> Normals = new List<Vector3>();

	private static List<DecalInfo> PoolList = new List<DecalInfo>();

	public static DecalInfo Get()
	{
		if (PoolList.Count != 0)
		{
			DecalInfo result = PoolList[0];
			PoolList.RemoveAt(0);
			return result;
		}
		return new DecalInfo();
	}

	public void Dispose()
	{
		BloodDecal = 200;
		isKnife = false;
		Points = new List<Vector3>();
		Normals = new List<Vector3>();
		PoolList.Add(this);
	}
}
