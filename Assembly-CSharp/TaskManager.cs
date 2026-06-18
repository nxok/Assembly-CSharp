using System;
using System.Threading;

public class TaskManager
{
	public static bool Pending
	{
		get
		{
			return TaskManager.rootGroup.Pending;
		}
	}

	public static void Init()
	{
		TaskManager.rootGroup = new TaskManager.TaskGroup(null);
		TaskManager.tasks = new WorkBatch<TaskManager.Task>();
	}

	public static void Destroy()
	{
		TaskManager.WaitOnGroup(TaskManager.rootGroup);
	}

	public static void Update()
	{
		TaskManager.tasks.DoWork(new Action<TaskManager.Task>(TaskManager.CompleteTask));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CompleteTask(TaskManager.Task _task)
	{
		if (_task.Complete != null)
		{
			_task.Complete();
			TaskManager.OnTaskCompleted(_task);
		}
	}

	public static TaskManager.TaskGroup CreateGroup()
	{
		return new TaskManager.TaskGroup(TaskManager.rootGroup);
	}

	public static TaskManager.TaskGroup CreateGroup(TaskManager.TaskGroup _parent)
	{
		return new TaskManager.TaskGroup(_parent);
	}

	public static void Schedule(Action _execute, Action _complete)
	{
		TaskManager.Task task = new TaskManager.Task(TaskManager.rootGroup, _execute, _complete);
		TaskManager.OnTaskCreated(task);
		ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(TaskManager.Execute), task, null, false);
	}

	public static void Schedule(TaskManager.TaskGroup _group, Action _execute, Action _complete)
	{
		TaskManager.Task task = new TaskManager.Task(_group, _execute, _complete);
		TaskManager.OnTaskCreated(task);
		ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(TaskManager.Execute), task, null, false);
	}

	public static void WaitOnGroup(TaskManager.TaskGroup _group)
	{
		if (!ThreadManager.MainThreadRef.Equals(Thread.CurrentThread))
		{
			throw new Exception("TaskManager.WaitOnGroup should only be called from the main thread.");
		}
		TaskManager.Update();
		while (_group.Pending)
		{
			Thread.Sleep(1);
			TaskManager.Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Execute(ThreadManager.TaskInfo _info)
	{
		TaskManager.Task task = _info.parameter as TaskManager.Task;
		task.Execute();
		if (task.Complete != null)
		{
			TaskManager.tasks.Add(task);
			return;
		}
		TaskManager.OnTaskCompleted(task);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnTaskCreated(TaskManager.Task task)
	{
		for (TaskManager.TaskGroup taskGroup = task.Group; taskGroup != null; taskGroup = taskGroup.parent)
		{
			Interlocked.Increment(ref taskGroup.pending);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnTaskCompleted(TaskManager.Task task)
	{
		for (TaskManager.TaskGroup taskGroup = task.Group; taskGroup != null; taskGroup = taskGroup.parent)
		{
			Interlocked.Decrement(ref taskGroup.pending);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TaskManager.TaskGroup rootGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorkBatch<TaskManager.Task> tasks;

	public class TaskGroup
	{
		public bool Pending
		{
			get
			{
				return Interlocked.CompareExchange(ref this.pending, 0, 0) != 0;
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public TaskGroup(TaskManager.TaskGroup _parent)
		{
			this.parent = _parent;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public TaskManager.TaskGroup parent;

		[PublicizedFrom(EAccessModifier.Internal)]
		public int pending;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public class Task
	{
		[PublicizedFrom(EAccessModifier.Internal)]
		public Task(TaskManager.TaskGroup _group, Action _execute, Action _complete)
		{
			this.Group = _group;
			this.Execute = _execute;
			this.Complete = _complete;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public TaskManager.TaskGroup Group;

		[PublicizedFrom(EAccessModifier.Internal)]
		public Action Execute;

		[PublicizedFrom(EAccessModifier.Internal)]
		public Action Complete;
	}
}
