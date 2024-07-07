using System;
using System.Collections.Generic;
using System.Threading;

namespace U3D.Threading.Tasks
{
	public class Task
	{
		protected enum TState
		{
			Created,
			Running,
			Successful,
			Aborted,
			Faulted
		}

		private sealed class WhenAllPromise : Task
		{
			private Task[] m_tasks;

			public WhenAllPromise(Task[] tasks)
			{
				m_tasks = tasks;
				foreach (Task task in tasks)
				{
					if (task.IsCompleted)
					{
						Invoke(task);
					}
					else
					{
						task.AddCompletionAction(Invoke);
					}
				}
			}

			public override void AbortThread()
			{
				Task[] tasks = m_tasks;
				foreach (Task task in tasks)
				{
					if (task.m_state == TState.Running)
					{
						task.AbortThread();
					}
				}
			}

			private void Invoke(Task task)
			{
				if (m_state == TState.Created)
				{
					m_state = TState.Running;
				}
				TState tState = TState.Successful;
				AggregateException ex = null;
				Task[] tasks = m_tasks;
				foreach (Task task2 in tasks)
				{
					switch (task2.m_state)
					{
					case TState.Created:
					case TState.Running:
						tState = TState.Running;
						break;
					case TState.Faulted:
						if (ex == null)
						{
							ex = new AggregateException();
						}
						ex.AddException(task2.Exception);
						break;
					default:
						throw new InvalidOperationException("Unknown task state: " + task2.m_state);
					case TState.Successful:
					case TState.Aborted:
						break;
					}
				}
				if (ex != null && tState == TState.Successful)
				{
					tState = TState.Faulted;
				}
				Exception = ex;
				m_state = tState;
			}
		}

		private sealed class WhenAnyPromise : Task
		{
			private Task[] m_tasks;

			public WhenAnyPromise(Task[] tasks)
			{
				m_tasks = tasks;
				foreach (Task task in tasks)
				{
					if (task.IsCompleted)
					{
						Invoke(task);
						break;
					}
					task.AddCompletionAction(Invoke);
				}
			}

			public override void AbortThread()
			{
				Task[] tasks = m_tasks;
				foreach (Task task in tasks)
				{
					if (task.m_state == TState.Running)
					{
						task.AbortThread();
					}
				}
			}

			private void Invoke(Task task)
			{
				if (m_state == TState.Created)
				{
					m_state = TState.Running;
				}
				TState tState = TState.Running;
				AggregateException ex = null;
				Task[] tasks = m_tasks;
				foreach (Task task2 in tasks)
				{
					switch (task2.m_state)
					{
					case TState.Successful:
					case TState.Aborted:
						tState = TState.Successful;
						break;
					case TState.Faulted:
						if (ex == null)
						{
							ex = new AggregateException();
						}
						ex.AddException(task2.Exception);
						break;
					default:
						throw new InvalidOperationException("Unknown task state: " + task2.m_state);
					case TState.Created:
					case TState.Running:
						break;
					}
				}
				if (ex != null && tState == TState.Successful)
				{
					tState = TState.Faulted;
				}
				Exception = ex;
				m_state = tState;
			}
		}

		private TState __state;

		private ManualResetEvent _syncEvent = new ManualResetEvent(false);

		private object _lockObject = new object();

		protected Action m_action;

		private Thread m_runThread;

		private Queue<Action<Task>> m_whenDone = new Queue<Action<Task>>();

		private object m_whenDoneSync = new object();

		private static Task s_completedTask;

		protected TState m_state
		{
			get
			{
				return __state;
			}
			set
			{
				if (value == __state)
				{
					return;
				}
				lock (_lockObject)
				{
					switch (value)
					{
					case TState.Created:
					case TState.Running:
						if (__state != 0)
						{
							throw new InvalidOperationException(string.Format("Invalid state transition; {0} -> {1}", __state, value));
						}
						break;
					case TState.Successful:
					case TState.Aborted:
					case TState.Faulted:
						if (__state != TState.Running)
						{
							throw new InvalidOperationException(string.Format("Invalid state transition; {0} -> {1}", __state, value));
						}
						__state = value;
						_syncEvent.Set();
						break;
					default:
						throw new InvalidOperationException(string.Format("Unexpected state; {0}", value));
					}
					__state = value;
				}
				if (m_state != TState.Successful && m_state != TState.Aborted && m_state != TState.Faulted)
				{
					return;
				}
				while (m_whenDone.Count > 0)
				{
					lock (m_whenDoneSync)
					{
						m_whenDone.Dequeue()(this);
					}
				}
			}
		}

		public AggregateException Exception { get; protected set; }

		public bool IsCompleted
		{
			get
			{
				return m_state == TState.Successful || m_state == TState.Aborted || m_state == TState.Faulted;
			}
		}

		public bool IsFaulted
		{
			get
			{
				return m_state == TState.Faulted;
			}
		}

		public bool IsAborted
		{
			get
			{
				return m_state == TState.Aborted;
			}
		}

		internal static Task CompletedTask
		{
			get
			{
				if (s_completedTask == null)
				{
					s_completedTask = new Task(delegate
					{
					});
					s_completedTask.RunAsync();
				}
				return s_completedTask;
			}
		}

		protected Task()
		{
			Dispatcher.Initialize();
		}

		protected Task(Action action)
			: this()
		{
			m_action = action;
		}

		public static Task Run(Action action)
		{
			Task task = new Task(action);
			task.RunAsync();
			return task;
		}

		public static Task RunInMainThread(Action action)
		{
			Dispatcher.Initialize();
			return Dispatcher.instance.TaskToMainThread(action);
		}

		protected Task RunAsync()
		{
			Exception = null;
			m_state = TState.Running;
			m_runThread = new Thread((ThreadStart)delegate
			{
				try
				{
					m_action();
					m_state = TState.Successful;
				}
#if !UNITY_WSA && !UNITY_EDITOR
                catch (System.Threading.ThreadAbortException)
                {
                    m_state = TState.Aborted;
                }
#endif
				catch (Exception ex2)
				{
					Exception = new AggregateException(ex2);
					m_state = TState.Faulted;
				}
				finally
				{
					m_runThread = null;
				}
			});
			m_runThread.Start();
			return this;
		}

		public virtual void AbortThread()
		{
			m_runThread.Abort();
		}

		public void Wait()
		{
			if (!IsCompleted)
			{
				_syncEvent.WaitOne();
			}
			if (!IsCompleted)
			{
				Wait();
			}
		}

		public Task ContinueWith(Action<Task> continuationAction)
		{
			return Run(delegate
			{
				Wait();
				continuationAction(this);
			});
		}

		public Task ContinueInMainThreadWith(Action<Task> continuationAction)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Run(delegate
			{
				Wait();
				Dispatcher.instance.ToMainThread(delegate
				{
					try
					{
						continuationAction(this);
						tcs.SetResult(true);
					}
					catch (Exception error)
					{
						tcs.SetError(error);
					}
				});
			});
			return tcs.Task;
		}

		internal void AddCompletionAction(Action<Task> continuation)
		{
			lock (m_whenDoneSync)
			{
				m_whenDone.Enqueue(continuation);
			}
		}

		public static Task WhenAll(params Task[] tasks)
		{
			if (tasks == null)
			{
				throw new ArgumentNullException("Tasks cannot be null");
			}
			int num = tasks.Length;
			if (num == 0)
			{
				return InternalWhenAll(tasks);
			}
			Task[] array = new Task[num];
			for (int i = 0; i < num; i++)
			{
				Task task = tasks[i];
				if (task == null)
				{
					throw new ArgumentException("A task cannot be null");
				}
				array[i] = task;
			}
			return InternalWhenAll(array);
		}

		public static Task WhenAll(List<Task> tasks)
		{
			return WhenAll(tasks.ToArray());
		}

		private static Task InternalWhenAll(Task[] tasks)
		{
			return (tasks.Length != 0) ? new WhenAllPromise(tasks) : CompletedTask;
		}

		public static Task WhenAny(params Task[] tasks)
		{
			if (tasks == null)
			{
				throw new ArgumentNullException("Tasks cannot be null");
			}
			int num = tasks.Length;
			if (num == 0)
			{
				return InternalWhenAny(tasks);
			}
			Task[] array = new Task[num];
			for (int i = 0; i < num; i++)
			{
				Task task = tasks[i];
				if (task == null)
				{
					throw new ArgumentException("A task cannot be null");
				}
				array[i] = task;
			}
			return InternalWhenAny(array);
		}

		public static Task WhenAny(List<Task> tasks)
		{
			return WhenAny(tasks.ToArray());
		}

		private static Task InternalWhenAny(Task[] tasks)
		{
			return (tasks.Length != 0) ? new WhenAnyPromise(tasks) : CompletedTask;
		}
	}
	public class Task<TResult> : Task
	{
		public TResult Result { get; private set; }

		public Task()
		{
			Result = default(TResult);
		}

		protected Task(Func<TResult> f)
		{
			Task<TResult> task = this;
			Result = default(TResult);
			m_action = delegate
			{
				task.Result = f();
			};
		}

		public bool TrySetResult(TResult result)
		{
			if (IsCompleted)
			{
				return false;
			}
			if (m_state == TState.Created)
			{
				m_state = TState.Running;
			}
			Result = result;
			m_state = TState.Successful;
			return true;
		}

		public bool TrySetError(Exception e)
		{
			if (IsCompleted)
			{
				return false;
			}
			if (m_state == TState.Created)
			{
				m_state = TState.Running;
			}
			Exception = new AggregateException(e);
			m_state = TState.Faulted;
			return true;
		}

		public static Task<TResult> Run(Func<TResult> action)
		{
			Task<TResult> task = new Task<TResult>(action);
			task.RunAsync();
			return task;
		}

		public static Task<TResult> RunInMainThread(Func<TResult> action)
		{
			Dispatcher.Initialize();
			return Dispatcher.instance.TaskToMainThread(action);
		}

		public Task ContinueWith(Action<Task<TResult>> continuationAction)
		{
			return Run(delegate
			{
				Wait();
				continuationAction(this);
			});
		}

		public Task ContinueInMainThreadWith(Action<Task<TResult>> continuationAction)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			Run(delegate
			{
				Wait();
				Dispatcher.instance.ToMainThread(delegate
				{
					try
					{
						continuationAction(this);
						tcs.SetResult(true);
					}
					catch (Exception error)
					{
						tcs.SetError(error);
					}
				});
			});
			return tcs.Task;
		}

		internal void SetIsRunning()
		{
			m_state = TState.Running;
		}
	}
}
