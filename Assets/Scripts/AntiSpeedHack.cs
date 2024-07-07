using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class AntiSpeedHack : MonoBehaviour
{
	public static OnDetectListener detectListener;

	private FileSystemWatcher watcher = new FileSystemWatcher();

	[DllImport("ash")]
	private static extern int doNothing(string path);

	private void Awake()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		string path = Application.persistentDataPath + "/ash";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		CreateFileWatcher(path);
		try
		{
			doNothing(path);
		}
		catch
		{
			AndroidNativeFunctions.ShowToast("Error: 457");
			TimerManager.In(2f, delegate
			{
				Application.Quit();
			});
		}
	}

	private void CreateFileWatcher(string path)
	{
		watcher.Path = path;
		watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite;
		watcher.Created += OnDetected;
		watcher.EnableRaisingEvents = true;
	}

	private void OnDetected(object source, FileSystemEventArgs e)
	{
		watcher.EnableRaisingEvents = false;
		File.Delete(e.FullPath);
		if (detectListener != null)
		{
			detectListener();
		}
	}
}
