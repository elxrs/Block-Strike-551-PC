using System.Collections.Generic;
using UnityEngine;

public class TimerBehaviour : MonoBehaviour
{
	private List<int> list = new List<int>();

	public int TimerID
	{
		set
		{
			list.Add(value);
		}
	}

	private void OnDestroy()
	{
		TimerManager.Cancel(list.ToArray());
	}
}
