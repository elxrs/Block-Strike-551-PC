using UnityEngine;

public class AmbientManager : MonoBehaviour
{
	public AudioSource Source;

	private AudioClip SelectedAmbient;

	private float Volume;

	public AudioClip[] Ambients;

	private void Start()
	{
		EventManager.AddListener("UpdateSettings", UpdateSettings);
		SelectAmbient();
		Play();
	}

	private void SelectAmbient()
	{
		string sceneName = LevelManager.GetSceneName();
		int num = 0;
		for (int i = 0; i < GameSettings.instance.Ambients.Count; i++)
		{
			if (GameSettings.instance.Ambients[i].Map == sceneName)
			{
				num = GameSettings.instance.Ambients[i].Ambient;
				SelectedAmbient = Ambients[GameSettings.instance.Ambients[i].Ambient];
				Volume = GameSettings.instance.Ambients[i].Volume;
				break;
			}
		}
		for (int j = 0; j < Ambients.Length; j++)
		{
			if (num != j)
			{
				Resources.UnloadAsset(Ambients[j]);
			}
		}
		Ambients = null;
	}

	private void Play()
	{
		if (Settings.AmbientSound && Settings.Sound && !Source.isPlaying && !(SelectedAmbient == null))
		{
			Source.clip = SelectedAmbient;
			Source.volume = Volume;
			Source.Play();
		}
	}

	private void UpdateSettings()
	{
		if (Settings.AmbientSound && Settings.Sound)
		{
			Play();
			return;
		}
		Source.Stop();
		Source.clip = null;
	}
}
