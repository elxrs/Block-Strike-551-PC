using UnityEngine;

public class mAvatarManager : MonoBehaviour
{
	public UITexture sprite;

	public Color32 selectColor;

	public int width = 32;

	public int height = 32;

	private Texture2D customTexture;

	private Vector3 lastPosition;

	private void Start()
	{
		customTexture = GenerateTexture(width, height);
		sprite.mainTexture = customTexture;
	}

	private void Update()
	{
		if (Input.GetMouseButton(0) && UICamera.lastWorldPosition != Vector3.zero && UICamera.lastWorldPosition != lastPosition)
		{
			Vector3 vector = sprite.cachedTransform.InverseTransformPoint(UICamera.lastWorldPosition);
			vector.x += sprite.width / 2f;
			vector.y += sprite.height / 2f;
			vector.x /= sprite.width;
			vector.y /= sprite.height;
			SetPixel((int)(width * vector.x), (int)(height * vector.y));
			lastPosition = UICamera.lastWorldPosition;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			byte[] array = customTexture.EncodeToPNG();
			print(array.Length);
		}
	}

	private void SetPixel(int x, int y)
	{
		customTexture.SetPixel(x, y, selectColor);
		customTexture.Apply(false);
	}

	private Texture2D GenerateTexture(int x, int y)
	{
		Texture2D texture2D = new Texture2D(x, y);
		for (int i = 0; i < x; i++)
		{
			for (int j = 0; j < y; j++)
			{
				texture2D.SetPixel(i, j, Color.white);
			}
		}
		texture2D.filterMode = FilterMode.Point;
		texture2D.Apply();
		return texture2D;
	}
}
