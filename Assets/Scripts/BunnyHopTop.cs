using UnityEngine;

public class BunnyHopTop : MonoBehaviour
{
	public string LevelName;

	private int Timer;

	private static BunnyHopTop instance;

	private void Start()
	{
		instance = this;
		FirebaseManager.DebugAction = true;
		Firebase firebase = new Firebase();
		firebase.Child("GameModes").Child("BunnyHop").Child("Top")
			.Child(LevelName)
			.GetValue(FirebaseParam.Default.OrderByValue().StartAt(10).LimitToFirst(5));
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			StartTimer();
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			print(TimerManager.GetDuration(Timer));
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			StopTimer();
		}
	}

	public static void StartTimer()
	{
		TimerManager.Cancel(instance.Timer);
		instance.Timer = TimerManager.Start();
	}

	public static void StopTimer()
	{
		TimerManager.Cancel(instance.Timer);
	}

	public static void UpdateTime()
	{
	}
}
