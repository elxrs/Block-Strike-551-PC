using System;
using System.Threading;

namespace U3D.Threading.Tasks
{
	public class TaskCompletionSource<TResult>
	{
		private readonly Task<TResult> m_task;

		public Task<TResult> Task
		{
			get
			{
				return m_task;
			}
		}

		public TaskCompletionSource()
		{
			m_task = new Task<TResult>();
		}

		private void SpinUntilCompleted()
		{
			while (!m_task.IsCompleted)
			{
				Thread.Sleep(10);
			}
		}

		public bool TrySetResult(TResult result)
		{
			bool flag = m_task.TrySetResult(result);
			if (!flag && !m_task.IsCompleted)
			{
				m_task.Wait();
			}
			return flag;
		}

		public bool TrySetError(Exception error)
		{
			bool flag = m_task.TrySetError(error);
			if (!flag && !m_task.IsCompleted)
			{
				m_task.Wait();
			}
			return flag;
		}

		public void SetResult(TResult result)
		{
			if (!TrySetResult(result))
			{
				throw new InvalidOperationException("Set Result: TaskT_TransitionToFinal_AlreadyCompletd\n" + result);
			}
		}

		public void SetError(Exception error)
		{
			if (!TrySetError(error))
			{
				throw new InvalidOperationException("Set ERROR: TaskT_TransitionToFinal_AlreadyCompleted\n" + error);
			}
		}

		internal void SetIsRunning()
		{
			m_task.SetIsRunning();
		}
	}
}
