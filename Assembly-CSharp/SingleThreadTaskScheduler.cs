using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine.Profiling;

public sealed class SingleThreadTaskScheduler : TaskScheduler, IDisposable
{
	public SingleThreadTaskScheduler(string threadGroupName, string threadName)
	{
		this.m_threadGroupName = threadGroupName;
		this.m_threadName = threadName;
		this.m_taskThread = new Thread(new ThreadStart(this.TaskThread))
		{
			Name = this.m_threadName,
			IsBackground = true
		};
		this.m_taskCancellationSource = new CancellationTokenSource();
		this.m_taskFactory = new TaskFactory(this.m_taskCancellationSource.Token, TaskCreationOptions.None, TaskContinuationOptions.None, this);
		this.m_running = true;
		this.m_taskThread.Start();
	}

	public void Dispose()
	{
		this.m_taskFactory = null;
		CancellationTokenSource taskCancellationSource = this.m_taskCancellationSource;
		if (taskCancellationSource != null)
		{
			taskCancellationSource.Cancel();
		}
		this.m_taskCancellationSource = null;
		this.m_running = false;
		AutoResetEvent waitHandle = this.m_waitHandle;
		if (waitHandle != null)
		{
			waitHandle.Set();
		}
		Thread taskThread = this.m_taskThread;
		if (taskThread != null)
		{
			taskThread.Interrupt();
		}
		Thread taskThread2 = this.m_taskThread;
		if (taskThread2 != null)
		{
			taskThread2.Join();
		}
		this.m_taskThread = null;
		AutoResetEvent waitHandle2 = this.m_waitHandle;
		if (waitHandle2 != null)
		{
			waitHandle2.Close();
		}
		this.m_waitHandle = null;
	}

	public Thread Thread
	{
		get
		{
			return this.m_taskThread;
		}
	}

	public TaskFactory Factory
	{
		get
		{
			return this.m_taskFactory;
		}
	}

	public bool IsCurrentThread
	{
		get
		{
			return Thread.CurrentThread == this.m_taskThread;
		}
	}

	public Task ExecuteNoWait(Action task)
	{
		if (!this.IsCurrentThread)
		{
			return this.m_taskFactory.StartNew(task);
		}
		task();
		return Task.CompletedTask;
	}

	public Task<T> ExecuteNoWait<T>(Func<T> task)
	{
		if (!this.IsCurrentThread)
		{
			return this.m_taskFactory.StartNew<T>(task);
		}
		return Task.FromResult<T>(task());
	}

	public void ExecuteAndWait(Action task)
	{
		if (this.IsCurrentThread)
		{
			task();
			return;
		}
		this.m_taskFactory.StartNew(task).Wait();
	}

	public T ExecuteAndWait<T>(Func<T> task)
	{
		if (!this.IsCurrentThread)
		{
			return this.m_taskFactory.StartNew<T>(task).Result;
		}
		return task();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TaskThread()
	{
		Log.Out("[" + this.m_threadGroupName + "] Started SingleThreadTaskScheduler Thread: " + this.m_threadName);
		try
		{
			while (this.m_running)
			{
				try
				{
					this.<TaskThread>g__ProcessTasks|24_0();
				}
				finally
				{
				}
				try
				{
					this.m_waitHandle.WaitOne();
				}
				finally
				{
				}
			}
		}
		catch (ThreadInterruptedException)
		{
			Log.Out("[" + this.m_threadGroupName + "] Interrupted SingleThreadTaskScheduler Thread: " + this.m_threadName);
		}
		finally
		{
			Profiler.EndThreadProfiling();
			Log.Out("[" + this.m_threadGroupName + "] Stopped SingleThreadTaskScheduler Thread: " + this.m_threadName);
		}
	}

	public override int MaximumConcurrencyLevel
	{
		get
		{
			return 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		return Thread.CurrentThread == this.m_taskThread && (!taskWasPreviouslyQueued || this.TryDequeue(task)) && base.TryExecuteTask(task);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void QueueTask(Task task)
	{
		LinkedList<Task> tasks = this.m_tasks;
		lock (tasks)
		{
			this.m_tasks.AddLast(task);
			this.m_waitHandle.Set();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryDequeue(Task task)
	{
		LinkedList<Task> tasks = this.m_tasks;
		bool flag2;
		lock (tasks)
		{
			flag2 = this.m_tasks.Remove(task);
		}
		return flag2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerable<Task> GetScheduledTasks()
	{
		bool flag = false;
		IEnumerable<Task> tasks;
		try
		{
			Monitor.TryEnter(this.m_tasks, ref flag);
			if (!flag)
			{
				throw new NotSupportedException();
			}
			tasks = this.m_tasks;
		}
		finally
		{
			if (flag)
			{
				Monitor.Exit(this.m_tasks);
			}
		}
		return tasks;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <TaskThread>g__ProcessTasks|24_0()
	{
		for (;;)
		{
			Task value;
			try
			{
				LinkedList<Task> tasks = this.m_tasks;
				lock (tasks)
				{
					if (this.m_tasks.Count == 0)
					{
						break;
					}
					value = this.m_tasks.First.Value;
					this.m_tasks.RemoveFirst();
				}
			}
			finally
			{
			}
			try
			{
				base.TryExecuteTask(value);
				continue;
			}
			finally
			{
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ProcessTasksMarker = new ProfilerMarker("SingleThreadTaskScheduler.ProcessTasks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_WaitForTasksMarker = new ProfilerMarker("SingleThreadTaskScheduler.WaitForTasks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_TryGetTaskMarker = new ProfilerMarker("SingleThreadTaskScheduler.TryGetTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ExecuteTaskMarker = new ProfilerMarker("SingleThreadTaskScheduler.ExecuteTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_threadGroupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_threadName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<Task> m_tasks = new LinkedList<Task>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_running;

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread m_taskThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent m_waitHandle = new AutoResetEvent(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource m_taskCancellationSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public TaskFactory m_taskFactory;
}
