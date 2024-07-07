using System.Collections.Generic;
using UnityEngine;

public class nTimer : MonoBehaviour
{
	private class TimerData
	{
		public int ID;

		public Callback Function;

		public float FinishTime;

		public bool Infinity;

		public float Delay;

		public bool Update(float time)
		{
			if (time >= FinishTime)
			{
				Function();
				if (Infinity)
				{
					FinishTime = Time.time + Delay;
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public delegate void Callback();

	private List<TimerData> Active = new List<TimerData>();

	private List<TimerData> Pool = new List<TimerData>();

	private float time;

	private float error;

	private int LastID;

	private void Start()
	{
		error = Random.Range(0.0001f, 0.01f);
	}

	public int In(float delay, Callback callback)
	{
		return In(delay, false, callback);
	}

	public int In(float delay, bool infinity, Callback callback)
	{
		TimerData timerData = null;
		if (Pool.Count > 0)
		{
			timerData = Pool[0];
			Pool.RemoveAt(0);
		}
		else
		{
			timerData = new TimerData();
		}
		LastID++;
		timerData.ID = LastID;
		timerData.FinishTime = Time.time + delay + error;
		timerData.Function = callback;
		timerData.Infinity = infinity;
		if (infinity)
		{
			timerData.Delay = delay;
		}
		Active.Add(timerData);
		return LastID;
	}

	public void Cancel(int id)
	{
		for (int i = 0; i < Active.Count; i++)
		{
			if (Active[i].ID == id)
			{
				Pool.Add(Active[i]);
				Active.RemoveAt(i);
			}
		}
	}

	public bool isActive(int id)
	{
		for (int i = 0; i < Active.Count; i++)
		{
			if (Active[i].ID == id)
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		time = Time.time;
		for (int i = 0; i < Active.Count; i++)
		{
			if (Active[i].Update(time))
			{
				Pool.Add(Active[i]);
				Active.RemoveAt(i);
			}
		}
	}
}
