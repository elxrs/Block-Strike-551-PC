using FreeJSON;
using UnityEngine;

public class CustomMapElements : MonoBehaviour
{
	public string element;

	public string data;

	private void Start()
	{
		switch (element)
		{
		case "DeadTrigger":
			gameObject.AddComponent<DeadTrigger>();
			break;
		case "TriggerTeleport":
		case "Teleport":
			gameObject.AddComponent<TriggerTeleport>();
			break;
		case "ClimbSystem":
		case "Climb":
			gameObject.AddComponent<ClimbSystem>();
			break;
		case "WaterSystem":
		case "Water":
			gameObject.AddComponent<WaterSystem>();
			break;
		case "BunnySpawn":
		{
			BunnySpawn bunnySpawn = gameObject.AddComponent<BunnySpawn>();
			bunnySpawn.FinishSpawn = JsonObject.Parse(data).ContainsKey("Finish");
			break;
		}
		case "BunnyAutoJump":
			gameObject.AddComponent<BunnyAutoJump>();
			break;
		}
	}
}
