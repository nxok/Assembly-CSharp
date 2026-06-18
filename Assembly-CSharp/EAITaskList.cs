using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAITaskList
{
	public EAITaskList(EAIManager _manager)
	{
		this.allTasks = new List<EAITaskEntry>();
		this.executingTasks = new List<EAITaskEntry>();
		this.executeDelayScale = 0.85f + _manager.random.RandomFloat * 0.25f;
	}

	public void AddTask(int _priority, EAIBase _eai)
	{
		this.allTasks.Add(new EAITaskEntry(_priority, _eai));
	}

	public void AddTaskList(EAITaskList _taskList)
	{
		for (int i = 0; i < _taskList.Tasks.Count; i++)
		{
			this.allTasks.Add(_taskList.Tasks[i]);
		}
	}

	public void RemoveTask(EAITaskEntry _entry)
	{
		this.allTasks.Remove(_entry);
		if (this.executingTasks.Contains(_entry))
		{
			this.executingTasks.Remove(_entry);
		}
	}

	public List<EAITaskEntry> Tasks
	{
		get
		{
			return this.allTasks;
		}
	}

	public List<EAITaskEntry> GetExecutingTasks()
	{
		return this.executingTasks;
	}

	public T GetTask<T>() where T : class
	{
		for (int i = 0; i < this.allTasks.Count; i++)
		{
			T t = this.allTasks[i].action as T;
			if (t != null)
			{
				return t;
			}
		}
		return default(T);
	}

	public void OnUpdateTasks()
	{
		this.startedTasks.Clear();
		int i = 0;
		while (i < this.allTasks.Count)
		{
			EAITaskEntry eaitaskEntry = this.allTasks[i];
			if (!eaitaskEntry.isExecuting)
			{
				goto IL_007A;
			}
			if (!this.isBestTask(eaitaskEntry) || !eaitaskEntry.action.Continue())
			{
				this.executingTasks.Remove(eaitaskEntry);
				eaitaskEntry.isExecuting = false;
				eaitaskEntry.executeTime = eaitaskEntry.action.executeDelay * this.executeDelayScale;
				eaitaskEntry.action.Reset();
				goto IL_007A;
			}
			IL_010D:
			i++;
			continue;
			IL_007A:
			eaitaskEntry.executeTime -= 0.05f;
			eaitaskEntry.action.executeWaitTime += 0.05f;
			if (eaitaskEntry.executeTime > 0f)
			{
				goto IL_010D;
			}
			eaitaskEntry.executeTime = eaitaskEntry.action.executeDelay * this.executeDelayScale;
			if (this.isBestTask(eaitaskEntry))
			{
				if (eaitaskEntry.action.CanExecute())
				{
					this.startedTasks.Add(eaitaskEntry);
					this.executingTasks.Add(eaitaskEntry);
					eaitaskEntry.isExecuting = true;
				}
				eaitaskEntry.action.executeWaitTime = 0f;
				goto IL_010D;
			}
			goto IL_010D;
		}
		for (int j = 0; j < this.startedTasks.Count; j++)
		{
			this.startedTasks[j].action.Start();
		}
		for (int k = 0; k < this.executingTasks.Count; k++)
		{
			this.executingTasks[k].action.Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBestTask(EAITaskEntry _task)
	{
		int i = 0;
		while (i < this.executingTasks.Count)
		{
			EAITaskEntry eaitaskEntry = this.executingTasks[i++];
			if (eaitaskEntry != _task)
			{
				if (eaitaskEntry.priority > _task.priority)
				{
					if (eaitaskEntry.action.IsContinuous())
					{
						continue;
					}
				}
				else if (this.areTasksCompatible(_task, eaitaskEntry))
				{
					continue;
				}
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool areTasksCompatible(EAITaskEntry _task, EAITaskEntry _other)
	{
		return (_task.action.MutexBits & _other.action.MutexBits) == 0;
	}

	public void StopAllTasks()
	{
		this.allTasks.Clear();
		this.executingTasks.Clear();
		this.startedTasks.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAITaskEntry> allTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAITaskEntry> executingTasks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAITaskEntry> startedTasks = new List<EAITaskEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float executeDelayScale;
}
