using FreeJSON;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomMapSettings : MonoBehaviour
{
	public string data;

	public static CustomMapSettings instance;

	private void Awake()
	{
		instance = this;
		CheckTextures();
	}

	private void CheckTextures()
	{
		try
		{
			if (JsonObject.Parse(data).ContainsKey(Utils.XOR(LevelManager.GetSceneName(), true)))
			{
				Finish();
				return;
			}
		}
		catch
		{
		}
		MeshRenderer[] array = FindObjectsOfType<MeshRenderer>();
		for (int i = 0; i < array.Length; i++)
		{
			CustomMapTexture component = array[i].GetComponent<CustomMapTexture>();
			if (component != null)
			{
				Material[] materials = array[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					if (component.id.Count - 1 >= j)
					{
						materials[j] = GameSettings.instance.CustomMaterials[component.id[j]];
					}
					else
					{
						materials[j] = GameSettings.instance.CustomMaterialError;
					}
				}
				array[i].materials = materials;
			}
			else
			{
				Material[] materials = array[i].materials;
				for (int k = 0; k < materials.Length; k++)
				{
					materials[k] = GameSettings.instance.CustomMaterialError;
				}
				array[i].materials = materials;
			}
		}
		Canvas[] array2 = FindObjectsOfType<Canvas>();
		for (int l = 0; l < array2.Length; l++)
		{
			array2[l].gameObject.SetActive(false);
		}
		EventSystem[] array3 = FindObjectsOfType<EventSystem>();
		for (int m = 0; m < array3.Length; m++)
		{
			array3[m].gameObject.SetActive(false);
		}
		AudioSource[] array4 = FindObjectsOfType<AudioSource>();
		for (int n = 0; n < array4.Length; n++)
		{
			array4[n].gameObject.SetActive(false);
		}
		AudioListener[] array5 = FindObjectsOfType<AudioListener>();
		for (int num = 0; num < array5.Length; num++)
		{
			array5[num].enabled = false;
		}
#if UNITY_5 || UNITY_2017_1
		GUIText[] array6 = FindObjectsOfType<GUIText>();
		for (int num2 = 0; num2 < array6.Length; num2++)
		{
			array6[num2].enabled = false;
		}
		GUITexture[] array7 = FindObjectsOfType<GUITexture>();
		for (int num3 = 0; num3 < array7.Length; num3++)
		{
			array7[num3].enabled = false;
		}
#endif
		Camera[] array8 = FindObjectsOfType<Camera>();
		for (int num4 = 0; num4 < array8.Length; num4++)
		{
			array8[num4].enabled = false;
		}
		ParticleSystem[] array9 = FindObjectsOfType<ParticleSystem>();
		for (int num5 = 0; num5 < array9.Length; num5++)
		{
			array9[num5].gameObject.SetActive(false);
		}
		TrailRenderer[] array10 = FindObjectsOfType<TrailRenderer>();
		for (int num6 = 0; num6 < array10.Length; num6++)
		{
			array10[num6].enabled = false;
		}
		LineRenderer[] array11 = FindObjectsOfType<LineRenderer>();
		for (int num7 = 0; num7 < array11.Length; num7++)
		{
			array11[num7].enabled = false;
		}
		LensFlare[] array12 = FindObjectsOfType<LensFlare>();
		for (int num8 = 0; num8 < array12.Length; num8++)
		{
			array12[num8].enabled = false;
		}
		Projector[] array13 = FindObjectsOfType<Projector>();
		for (int num9 = 0; num9 < array13.Length; num9++)
		{
			array13[num9].enabled = false;
		}
		ParticleSystem[] array14 = FindObjectsOfType<ParticleSystem>();
		for (int num10 = 0; num10 < array14.Length; num10++)
		{
			array14[num10].gameObject.SetActive(false);
		}
		SpriteRenderer[] array15 = FindObjectsOfType<SpriteRenderer>();
		for (int num11 = 0; num11 < array15.Length; num11++)
		{
			array15[num11].enabled = false;
		}
		Finish();
	}

	private void Finish()
	{
		Instantiate(GameSettings.instance.UIRoot).name = "UIRoot";
		if (PhotonNetwork.isMasterClient)
		{
			GameObject gameObject = PhotonNetwork.InstantiateSceneObject("RoundManager", Vector3.zero, Quaternion.identity, 0, null);
			gameObject.name = "RoundManager";
		}
	}
}
