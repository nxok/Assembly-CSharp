using System;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public struct ActionData
	{
		public UAITaskBase CurrentTask
		{
			get
			{
				if (this.Action == null || this.Action.GetTasks() == null || this.TaskIndex < 0 || this.TaskIndex >= this.Action.GetTasks().Count)
				{
					return null;
				}
				return this.Action.GetTasks()[this.TaskIndex];
			}
		}

		public void ClearData()
		{
			this.Data = null;
			this.TaskStartTimeStamp = 0UL;
			this.Initialized = false;
			this.Started = false;
			this.Executing = false;
			this.Failed = false;
			this.Finished = false;
		}

		public UAIAction Action;

		public object Target;

		public object Data;

		public int TaskIndex;

		public ulong TaskStartTimeStamp;

		public bool Initialized;

		public bool Started;

		public bool Executing;

		public bool Failed;

		public bool Finished;
	}
}
