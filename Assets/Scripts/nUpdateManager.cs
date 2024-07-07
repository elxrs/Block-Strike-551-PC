using UnityEngine;

public class nUpdateManager : MonoBehaviour
{
	public delegate void Callback();

	public Callback[] updateArray = new Callback[0];

	public int updateCount;

	private static nUpdateManager instance;

	private void Awake()
	{
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public static void AddUpdate(Callback callback)
	{
		instance.updateArray = instance.AddItem(instance.updateArray, callback);
		instance.updateCount++;
	}

	public static void RemoveUpdate(Callback callback)
	{
		instance.updateArray = instance.RemoveItem(instance.updateArray, callback);
		instance.updateCount = instance.updateArray.Length;
	}

	private Callback[] AddItem(Callback[] original, Callback itemToAdd)
	{
		int num = original.Length;
		Callback[] array = new Callback[num + 1];
		for (int i = 0; i < num; i++)
		{
			array[i] = original[i];
		}
		array[array.Length - 1] = itemToAdd;
		return array;
	}

	private Callback[] RemoveItem(Callback[] original, Callback itemToRemove)
	{
		int num = original.Length;
		Callback[] array = new Callback[num - 1];
		for (int i = 0; i < num; i++)
		{
			if (!(original[i] == itemToRemove))
			{
				array[i] = original[i];
			}
		}
		return array;
	}

	private void Update()
	{
		for (int i = 0; i < updateCount; i++)
		{
			updateArray[i]();
		}
	}
}
