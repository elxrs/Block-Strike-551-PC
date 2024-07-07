using UnityEngine;

public class DrawElements : MonoBehaviour
{
	public Color m_Color = Color.red;

	public Vector3 Size = Vector3.one;

	private Transform m_Transform;

	private void Start()
	{
		m_Transform = transform;
	}

	public Transform GetTransform()
	{
		if (m_Transform == null)
		{
			m_Transform = transform;
		}
		return m_Transform;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.color = m_Color;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
		Gizmos.DrawCube(transform.position, Size);
	}
#endif

	public Vector3 GetSpawnPosition()
	{
		Vector3 position = GetTransform().position;
		position.x += Random.Range((0f - Size.x) / 2f, Size.x / 2f);
		position.z += Random.Range((0f - Size.z) / 2f, Size.z / 2f);
		return position;
	}

	public Vector3 GetSpawnRotation()
	{
		return GetTransform().eulerAngles;
	}
}
