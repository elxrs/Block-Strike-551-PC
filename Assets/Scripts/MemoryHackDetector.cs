using System.Diagnostics;
using System.IO;
using System.Threading;
using U3D.Threading.Tasks;
using UnityEngine;

public class MemoryHackDetector : MonoBehaviour
{
	public int interval;

	private bool detect;

	private Task task;

	private string pid;

	private void Awake()
	{
		pid = Process.GetCurrentProcess().Id.ToString();
		TimerManager.In(2f, delegate
		{
			task = Task.Run(Check01);
		});
		TimerManager.In(0.2f, false, -1, 0.2f, CheckTasks);
	}

	private void CheckTasks()
	{
		if ((task.IsAborted || task.IsCompleted || task.IsFaulted) && !detect)
		{
			task = Task.Run(Check01);
		}
	}

	private void Check01()
	{
		string text = string.Empty;
		string value = string.Empty;
		while (!detect)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
			{
				string[] directories = Directory.GetDirectories("proc/" + pid + "/task");
				for (int i = 0; i < directories.Length; i++)
				{
					string text2 = File.ReadAllText(directories[i] + "/status");
					if (!text2.Contains("FinalizerWatchd"))
					{
						continue;
					}
					string[] array = File.ReadAllLines(directories[i] + "/status");
					text = directories[i] + "/status";
					int num = 0;
					while (i < array.Length)
					{
						if (array[num].Contains("TracerPid"))
						{
							value = array[num];
							break;
						}
						num++;
					}
					break;
				}
			}
			else if (File.Exists(text))
			{
				string text3 = File.ReadAllText(text);
				if (text3.Contains("TracerPid"))
				{
					if (!text3.Contains(value))
					{
						detect = true;
						Task.RunInMainThread(delegate
						{
							OnHackDetected("Memory Error", string.Empty);
						});
					}
				}
				else
				{
					text = string.Empty;
				}
			}
			else
			{
				text = string.Empty;
			}
			Thread.Sleep(interval);
		}
	}

	private void OnHackDetected(string text, string log)
	{
		CheckManager.Detected(text, log);
	}

	public static int toInt(string text)
	{
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			if (char.IsDigit(text[i]))
			{
				text2 += text[i];
			}
		}
		return int.Parse(text2);
	}
}
