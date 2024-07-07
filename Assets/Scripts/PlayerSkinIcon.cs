using UnityEngine;

public class PlayerSkinIcon : MonoBehaviour
{
	public MeshEditor meshEditor;

	public void SetIcon(Material mat)
	{
		meshEditor.cachedGameObject.SetActive(true);
		meshEditor.meshRenderer.material = mat;
	}

	public void Deactive()
	{
		meshEditor.cachedGameObject.SetActive(false);
	}
}
