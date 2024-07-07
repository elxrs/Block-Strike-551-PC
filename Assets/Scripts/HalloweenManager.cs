using UnityEngine;

public class HalloweenManager : MonoBehaviour
{
	public GameObject[] heads;

	public void Init(int seed)
	{
		System.Random random = new System.Random(seed);
		heads[random.Next(0, heads.Length)].SetActive(true);
	}
}
