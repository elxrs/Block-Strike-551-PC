using UnityEngine;

public class InAppManager : MonoBehaviour
{
	public static string GetPrice()
	{
		return Localization.Get("Free");
	}
}
