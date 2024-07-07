using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class AntiSpeedHack2 : MonoBehaviour
{
	public static OnDetectListener detectListener;

	public int startDifference = 7;

	private byte falses;

	private void Start()
	{
		CheckStart();
		InvokeRepeating("Check", nValue.int1, nValue.int1);
	}

	private void Check()
	{
		string text = File.ReadAllLines("proc/driver/rtc")[0];
		text = text.Replace(" ", string.Empty);
		string[] array = text.Split(":"[0]);
		int num = int.Parse(array[1]) * 3600 + int.Parse(array[2]) * 60 + int.Parse(array[3]);
		DateTime utcNow = DateTime.UtcNow;
		int num2 = utcNow.Hour * 3600 + utcNow.Minute * 60 + utcNow.Second;
		int num3 = num - num2;
		if (num3 > 3 || num3 < -3)
		{
			falses++;
			if (falses >= 3 && detectListener != null)
			{
				detectListener();
			}
		}
		else if (falses != 0)
		{
			falses--;
		}
	}

	private void CheckStart()
	{
		string text = File.ReadAllLines("proc/driver/rtc")[0];
		text = text.Replace(" ", string.Empty);
		string[] array = text.Split(":"[0]);
		int num = int.Parse(array[1]) * 3600 + int.Parse(array[2]) * 60 + int.Parse(array[3]);
		DateTime dateTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();
		int num2 = dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second;
		if (num - num2 > startDifference)
		{
			Application.Quit();
		}
	}
}
