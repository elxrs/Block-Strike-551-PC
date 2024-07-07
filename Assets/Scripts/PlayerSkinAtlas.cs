using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[ExecuteInEditMode]
public class PlayerSkinAtlas : MonoBehaviour
{
	public enum SpriteList
	{
		Head,
		Body,
		LeftHand,
		RightHand,
		Legs
	}

	[Serializable]
	public class SpriteData
	{
		public SpriteList spriteElement;

		public string sprite;

		public UISpriteData spriteData;

		public bool spriteSet;
	}

	public static UIAtlas lastUsedAtlas;

	public Mesh originalMesh;

	private SkinnedMeshRenderer _mf;

	private Mesh mesh;

	public Material originalMaterial;

	public Material customMaterial;

	public UIAtlas mAtlas;

	public List<SpriteData> spriteData = new List<SpriteData>();

	public SkinnedMeshRenderer mf
	{
		get
		{
			if (_mf == null)
			{
				_mf = gameObject.GetComponent<SkinnedMeshRenderer>();
			}
			return _mf;
		}
	}

	[SerializeField]
	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if (!(mAtlas != value))
			{
				return;
			}
			mAtlas = value;
			for (int i = 0; i < spriteData.Count; i++)
			{
				SpriteData spriteData = this.spriteData[i];
				spriteData.spriteData = null;
				if (!(mAtlas != null))
				{
					continue;
				}
				lastUsedAtlas = value;
				customMaterial = mAtlas.spriteMaterial;
				if (originalMaterial != null && !string.IsNullOrEmpty(originalMaterial.name))
				{
					for (int j = 0; j < mAtlas.spriteList.Count; j++)
					{
						if (mAtlas.spriteList[j].name == originalMaterial.name)
						{
							SetAtlasSprite(spriteData.spriteElement, mAtlas.spriteList[j]);
							spriteData.sprite = spriteData.spriteData.name;
							break;
						}
					}
				}
				if (string.IsNullOrEmpty(spriteData.sprite) && mAtlas != null && mAtlas.spriteList.Count > 0)
				{
					SetAtlasSprite(spriteData.spriteElement, mAtlas.spriteList[0]);
					spriteData.sprite = spriteData.spriteData.name;
				}
				if (!string.IsNullOrEmpty(spriteData.sprite))
				{
					string sprite = spriteData.sprite;
					spriteData.sprite = string.Empty;
					SetSprite(spriteData.spriteElement, sprite);
				}
			}
		}
	}

	private void OnEnable()
	{
		spriteData.Clear();
		for (int i = 0; i < Enum.GetValues(typeof(SpriteList)).Length; i++)
		{
			SpriteData spriteData = new SpriteData();
			spriteData.spriteElement = (SpriteList)i;
			this.spriteData.Add(spriteData);
		}
		UpdateMesh();
	}

	private void Reset()
	{
		DisableMesh();
		if (Application.isEditor)
		{
			UpdateMesh();
		}
	}

	public void UpdateMesh()
	{
		if (originalMesh == null)
		{
			originalMesh = gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
			originalMaterial = mf.GetComponent<Renderer>().sharedMaterial;
			if (atlas == null && lastUsedAtlas != null)
			{
				atlas = lastUsedAtlas;
			}
		}
		if (enabled)
		{
			EnableMesh();
		}
		else
		{
			DisableMesh();
		}
	}

	public void EnableMesh()
	{
		if (mesh == null)
		{
			mesh = Instantiate(originalMesh);
			mesh.name = originalMesh.name + "_Atlas";
			UpdateUVs();
			if (customMaterial != null)
			{
				mf.GetComponent<Renderer>().sharedMaterial = customMaterial;
			}
			mf.sharedMesh = mesh;
		}
	}

	public void DisableMesh()
	{
		if (mesh != null)
		{
			customMaterial = mf.GetComponent<Renderer>().sharedMaterial;
			DestroyImmediate(mesh);
			mesh = null;
		}
		if (originalMesh != null)
		{
			mf.sharedMesh = originalMesh;
		}
		if (originalMaterial != null)
		{
			mf.GetComponent<Renderer>().sharedMaterial = originalMaterial;
		}
	}

	public void UpdateUVs()
	{
		if (mesh == null || atlas == null)
		{
			return;
		}
		Vector2[] array = originalMesh.uv;
		for (int i = 0; i < spriteData.Count; i++)
		{
			SpriteData spriteData = this.spriteData[i];
			if (string.IsNullOrEmpty(GetSprite(spriteData.spriteElement)))
			{
				return;
			}
			UISpriteData sprite = atlas.GetSprite(GetSprite(spriteData.spriteElement));
			if (sprite == null || atlas.texture == null)
			{
				return;
			}
			spriteData.spriteData = sprite;
			Rect rect = new Rect(spriteData.spriteData.x, spriteData.spriteData.y, spriteData.spriteData.width, spriteData.spriteData.height);
			Rect rect2 = NGUIMath.ConvertToTexCoords(rect, atlas.texture.width, atlas.texture.height);
			array = UpdatePlayerUV(spriteData.spriteElement, rect2, array);
		}
		mesh.uv = array;
	}

	public Vector2[] UpdatePlayerUV(SpriteList element, Rect rect, Vector2[] uvs)
	{
		for (int i = 0; i < uvs.Length; i++)
		{
			switch (element)
			{
			case SpriteList.Head:
				if (i >= 432 && i <= 455)
				{
					uvs[i].x = uvs[i].x * rect.width + rect.x;
					uvs[i].y = uvs[i].y * rect.height + rect.y;
				}
				break;
			case SpriteList.Body:
				if (i >= 208 && i <= 231)
				{
					uvs[i].x = uvs[i].x * rect.width + rect.x;
					uvs[i].y = uvs[i].y * rect.height + rect.y;
				}
				break;
			case SpriteList.LeftHand:
				if (i >= 232 && i <= 331)
				{
					uvs[i].x = uvs[i].x * rect.width + rect.x;
					uvs[i].y = uvs[i].y * rect.height + rect.y;
				}
				break;
			case SpriteList.RightHand:
				if (i >= 333 && i <= 431)
				{
					uvs[i].x = uvs[i].x * rect.width + rect.x;
					uvs[i].y = uvs[i].y * rect.height + rect.y;
				}
				break;
			case SpriteList.Legs:
				if ((i >= 0 && i <= 207) || (i >= 456 && i <= 537))
				{
					uvs[i].x = uvs[i].x * rect.width + rect.x;
					uvs[i].y = uvs[i].y * rect.height + rect.y;
				}
				break;
			}
		}
		return uvs;
	}

	public string GetSprite(SpriteList element)
	{
		return GetSpriteData(element).sprite;
	}

	public void SetSprite(SpriteList element, string value)
	{
		SpriteData spriteData = GetSpriteData(element);
		if (string.IsNullOrEmpty(value))
		{
			if (!string.IsNullOrEmpty(spriteData.sprite))
			{
				spriteData.sprite = string.Empty;
				spriteData.spriteData = null;
			}
		}
		else if (spriteData.sprite != value)
		{
			spriteData.sprite = value;
			spriteData.spriteData = null;
			UpdateUVs();
		}
	}

	public SpriteData GetSpriteData(SpriteList element)
	{
		for (int i = 0; i < spriteData.Count; i++)
		{
			if (spriteData[i].spriteElement == element)
			{
				return spriteData[i];
			}
		}
		return null;
	}

	public bool isValid(SpriteList element)
	{
		return GetAtlasSprite(element) != null;
	}

	public UISpriteData GetAtlasSprite(SpriteList element)
	{
		SpriteData spriteData = GetSpriteData(element);
		if (!spriteData.spriteSet)
		{
			spriteData.spriteData = null;
		}
		if (spriteData.spriteData == null && mAtlas != null)
		{
			if (!string.IsNullOrEmpty(spriteData.sprite))
			{
				UISpriteData sprite = mAtlas.GetSprite(spriteData.sprite);
				if (sprite == null)
				{
					return null;
				}
				SetAtlasSprite(element, sprite);
			}
			if (spriteData.spriteData == null && mAtlas.spriteList.Count > 0)
			{
				UISpriteData uISpriteData = mAtlas.spriteList[0];
				if (uISpriteData == null)
				{
					return null;
				}
				SetAtlasSprite(element, uISpriteData);
				if (spriteData.spriteData == null)
				{
					Debug.LogError(mAtlas.name + " seems to have a null sprite!");
					return null;
				}
				spriteData.sprite = spriteData.spriteData.name;
			}
		}
		return spriteData.spriteData;
	}

	private void SetAtlasSprite(SpriteList element, UISpriteData sp)
	{
		SpriteData spriteData = GetSpriteData(element);
		spriteData.spriteSet = true;
		if (sp != null)
		{
			spriteData.spriteData = sp;
			spriteData.sprite = spriteData.spriteData.name;
		}
		else
		{
			spriteData.sprite = ((spriteData.spriteData == null) ? string.Empty : spriteData.spriteData.name);
			spriteData.spriteData = sp;
		}
	}

	public void UpdateAllMeshes()
	{
		MeshAtlas[] array = NGUITools.FindActive<MeshAtlas>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			MeshAtlas meshAtlas = array[i];
			if (meshAtlas.enabled && originalMesh == meshAtlas.originalMesh)
			{
				meshAtlas.UpdateMesh();
			}
		}
	}

	public void UpdateMeshTextures()
	{
		MeshAtlas[] array = NGUITools.FindActive<MeshAtlas>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			MeshAtlas meshAtlas = array[i];
			if (UIAtlas.CheckIfRelated(atlas, meshAtlas.atlas))
			{
				meshAtlas.UpdateMesh();
			}
		}
	}
}
