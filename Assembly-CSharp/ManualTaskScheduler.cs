using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling;

public sealed class ManualTaskScheduler : TaskScheduler, IDisposable
{
	public ManualTaskScheduler()
	{
		this.m_taskCancellationSource = new CancellationTokenSource();
		this.m_taskFactory = new TaskFactory(this.m_taskCancellationSource.Token, TaskCreationOptions.None, TaskContinuationOptions.None, this);
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
	}

	public void ProcessTasks()
	{
		try
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
				}
				finally
				{
				}
			}
		}
		finally
		{
		}
	}

	public override int MaximumConcurrencyLevel
	{
		get
		{
			return 1;
		}
	}

	public TaskFactory Factory
	{
		get
		{
			return this.m_taskFactory;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
	{
		return (!taskWasPreviouslyQueued || this.TryDequeue(task)) && base.TryExecuteTask(task);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void QueueTask(Task task)
	{
		LinkedList<Task> tasks = this.m_tasks;
		lock (tasks)
		{
			this.m_tasks.AddLast(task);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ProcessTasksMarker = new ProfilerMarker("ManualTaskScheduler.ProcessTasks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_TryGetTaskMarker = new ProfilerMarker("ManualTaskScheduler.TryGetTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ExecuteTaskMarker = new ProfilerMarker("ManualTaskScheduler.ExecuteTask");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<Task> m_tasks = new LinkedList<Task>();

	[PublicizedFrom(EAccessModifier.Private)]
	public CancellationTokenSource m_taskCancellationSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public TaskFactory m_taskFactory;
}
