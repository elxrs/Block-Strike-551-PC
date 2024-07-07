using System;
using System.Collections.Generic;
using UnityEngine;

public class CreativeManager : MonoBehaviour
{
	public CreativeObject block;

	public bool isDelete;

	public byte selectBlockID;

	public List<CreativeObject> blockList = new List<CreativeObject>();

	private Transform mTransform;

	private static CreativeManager instance;

	private void Awake()
	{
		instance = this;
		mTransform = transform;
	}

	private void Start()
	{
		EventManager.AddListener<RaycastHit>("Creative", CreateBlock);
		PhotonEvent.AddListener(1, PhotonLoad);
	}

	private void OnEnable()
	{
		InputManager.GetButtonDownEvent += GetButtonDown;
	}

	private void OnDisable()
	{
		InputManager.GetButtonDownEvent -= GetButtonDown;
	}

	private void GetButtonDown(string key)
	{
		if (key == "Reload")
		{
			isDelete = !isDelete;
		}
	}

	[ContextMenu("Send Data")]
	private void SendData()
	{
		byte[] data = GetData();
		PhotonEvent.RPC(1, PhotonTargets.All, data);
	}

	[ContextMenu("Save Map")]
	private void SaveMap()
	{
		byte[] data = GetData();
		string value = Convert.ToBase64String(data);
		PlayerPrefs.SetString("CreativeMap", value);
	}

	[ContextMenu("Load Map")]
	private void LoadMap()
	{
		string @string = PlayerPrefs.GetString("CreativeMap");
		byte[] bytes = Convert.FromBase64String(@string);
		LoadData(bytes);
	}

	[ContextMenu("Clear Map")]
	private void ClearMap()
	{
		Clear();
	}

	private void CreateBlock(RaycastHit hit)
	{
		if (isDelete)
		{
			if (!(hit.transform.name != "Plane"))
			{
				return;
			}
			int id = hit.transform.parent.GetComponent<CreativeObject>().id;
			for (int i = 0; i < blockList.Count; i++)
			{
				if (blockList[i].id == id)
				{
					blockList[i].Delete();
					blockList.RemoveAt(i);
					break;
				}
			}
		}
		else if (hit.distance > 1.5f)
		{
			if (hit.transform.name == "Plane")
			{
				Vector3 zero = Vector3.zero;
				zero.x = Mathf.Round(hit.point.x);
				zero.y = 0f;
				zero.z = Mathf.Round(hit.point.z);
				Create(zero, selectBlockID);
			}
			else
			{
				Create(hit.transform.parent.position + hit.normal, selectBlockID);
			}
		}
	}

	public static void Create(Vector3 pos, byte id)
	{
		GameObject gameObject = PoolManager.Spawn("Block", instance.block.cachedGameObject, pos, Vector3.zero, instance.mTransform);
		CreativeObject component = gameObject.GetComponent<CreativeObject>();
		component.UpdateFaces(id);
		instance.blockList.Add(component);
	}

	public static void Clear()
	{
		for (int i = 0; i < instance.blockList.Count; i++)
		{
			instance.blockList[i].Delete(false);
		}
		instance.blockList.Clear();
	}

	private void PhotonLoad(PhotonEventData data)
	{
		LoadData((byte[])data.parameters[0]);
	}

	private void LoadData(byte[] bytes)
	{
		Clear();
		int num = bytes.Length / 10;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = PoolManager.Spawn("Block", instance.block.cachedGameObject, new Vector3(bytes[num2 + 7], bytes[num2 + 8], bytes[num2 + 9]), Vector3.zero, instance.mTransform);
			CreativeObject component = gameObject.GetComponent<CreativeObject>();
			component.UpdateFaces(bytes[num2], false);
			blockList.Add(component);
			component.meshAtlases[0].meshRenderer.enabled = bytes[num2 + 1] == 1;
			component.meshAtlases[1].meshRenderer.enabled = bytes[num2 + 2] == 1;
			component.meshAtlases[2].meshRenderer.enabled = bytes[num2 + 3] == 1;
			component.meshAtlases[3].meshRenderer.enabled = bytes[num2 + 4] == 1;
			component.meshAtlases[4].meshRenderer.enabled = bytes[num2 + 5] == 1;
			component.meshAtlases[5].meshRenderer.enabled = bytes[num2 + 6] == 1;
			num2 += 10;
		}
	}

	private byte[] GetData()
	{
		byte[] array = new byte[blockList.Count * 10];
		int num = 0;
		for (int i = 0; i < blockList.Count; i++)
		{
			array[num] = blockList[i].id;
			array[num + 1] = (byte)(blockList[i].meshAtlases[0].meshRenderer.enabled ? 1u : 0u);
			array[num + 2] = (byte)(blockList[i].meshAtlases[1].meshRenderer.enabled ? 1u : 0u);
			array[num + 3] = (byte)(blockList[i].meshAtlases[2].meshRenderer.enabled ? 1u : 0u);
			array[num + 4] = (byte)(blockList[i].meshAtlases[3].meshRenderer.enabled ? 1u : 0u);
			array[num + 5] = (byte)(blockList[i].meshAtlases[4].meshRenderer.enabled ? 1u : 0u);
			array[num + 6] = (byte)(blockList[i].meshAtlases[5].meshRenderer.enabled ? 1u : 0u);
			array[num + 7] = (byte)blockList[i].cachedTransform.localPosition.x;
			array[num + 8] = (byte)blockList[i].cachedTransform.localPosition.y;
			array[num + 9] = (byte)blockList[i].cachedTransform.localPosition.z;
			num += 10;
		}
		return array;
	}
}
