using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAIAction
	{
		public string Name
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public float Weight
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public UAIAction(string _name, float _weight)
		{
			this.Name = _name;
			this.Weight = _weight;
			this.considerations = new List<UAIConsiderationBase>();
			this.tasks = new List<UAITaskBase>();
		}

		public float GetScore(Context _context, object _target, float min = 0f)
		{
			float num = 1f;
			if (this.considerations.Count == 0)
			{
				return num * this.Weight;
			}
			if (this.tasks.Count == 0)
			{
				return 0f;
			}
			for (int i = 0; i < this.considerations.Count; i++)
			{
				if (0f > num || num < min)
				{
					return 0f;
				}
				num *= this.considerations[i].ComputeResponseCurve(this.considerations[i].GetScore(_context, _target));
			}
			return (num + (1f - num) * (float)(1 - 1 / this.considerations.Count) * num) * this.Weight;
		}

		public void AddConsideration(UAIConsiderationBase _c)
		{
			this.considerations.Add(_c);
		}

		public void AddTask(UAITaskBase _t)
		{
			this.tasks.Add(_t);
		}

		public List<UAIConsiderationBase> GetConsiderations()
		{
			return this.considerations;
		}

		public List<UAITaskBase> GetTasks()
		{
			return this.tasks;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<UAITaskBase> tasks;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<UAIConsiderationBase> considerations;
	}
}
