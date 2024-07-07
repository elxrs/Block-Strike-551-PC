using System;
using System.Collections;
using System.Collections.Generic;
using U3D.Threading.Tasks;
using UnityEngine;

namespace U3D.Threading
{
	public class Dispatcher : MonoBehaviour
	{
		private static Dispatcher _instance;

		private static bool _initialized;

		private Queue<Action> m_q = new Queue<Action>();

		public static Dispatcher instance
		{
			get
			{
				if (!_initialized)
				{
					Debug.LogError("You need to call Initialize on the Main thread before using the Dispatcher");
					throw new InvalidOperationException("You need to call Initialize on the Main thread before using the Dispatcher");
				}
				return _instance;
			}
		}

		public static void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
				GameObject gameObject = GameObject.Find(typeof(Dispatcher).Name);
				if (gameObject == null)
				{
					gameObject = new GameObject(typeof(Dispatcher).Name);
				}
				_instance = gameObject.GetComponent<Dispatcher>();
				if (_instance == null)
				{
					_instance = gameObject.AddComponent<Dispatcher>();
				}
				DontDestroyOnLoad(_instance.gameObject);
				_instance.gameObject.SendMessage("InitializeInstance", SendMessageOptions.DontRequireReceiver);
			}
		}

		private void Update()
		{
			if (m_q.Count > 0)
			{
				Action action = m_q.Dequeue();
				if (action != null)
				{
					action();
				}
			}
		}

		public void ToMainThread(Action a)
		{
			m_q.Enqueue(a);
		}

		public Task TaskToMainThread(Action a)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			m_q.Enqueue(delegate
			{
				a();
				tcs.SetResult(true);
			});
			return tcs.Task;
		}

		public Task<TResult> TaskToMainThread<TResult>(Func<TResult> f)
		{
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			m_q.Enqueue(delegate
			{
				tcs.SetResult(f());
			});
			return tcs.Task;
		}

		public void ToMainThreadAfterDelay(double seconds, Action a)
		{
			instance.ToMainThread(delegate
			{
				instance.LaunchCoroutine(instance.ExecuteDelayed(seconds, a));
			});
		}

		private IEnumerator ExecuteDelayed(double seconds, Action a)
		{
			yield return new WaitForSeconds((float)seconds);
			m_q.Enqueue(a);
		}

		public void LaunchCoroutine(IEnumerator firstIterationResult)
		{
			instance.StartCoroutine(firstIterationResult);
		}
	}
}
