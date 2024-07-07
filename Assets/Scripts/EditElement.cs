using UnityEngine;

public class EditElement : MonoBehaviour
{
	[SelectedWeapon]
	public int ID;

	public Transform RootMesh;

	public GameObject[] Meshes;

	public Material DefaultMaterial;

	public Vector3 DefaultPosition;

	public Vector3 DefaultRotation;

	public Vector3 MaxScale;

	public void SetDefaultPosition(bool rotation)
	{
		RootMesh.localPosition = DefaultPosition;
		if (rotation)
		{
			RootMesh.localEulerAngles = DefaultRotation;
		}
	}
}
