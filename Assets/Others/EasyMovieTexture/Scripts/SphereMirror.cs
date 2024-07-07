using UnityEngine;

public class SphereMirror : MonoBehaviour
{
	private void Start()
	{
		Vector2[] uv = transform.GetComponent<MeshFilter>().mesh.uv;
		for (int i = 0; i < uv.Length; i++)
		{
			uv[i] = new Vector2(1f - uv[i].x, uv[i].y);
		}
		transform.GetComponent<MeshFilter>().mesh.uv = uv;
	}

	private void Update()
	{
	}
}
