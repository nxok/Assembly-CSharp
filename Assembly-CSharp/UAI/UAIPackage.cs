using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAIPackage
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

		public UAIPackage(string _name = "", float _weight = 1f)
		{
			this.Name = _name;
			this.Weight = _weight;
			this.actionList = new List<UAIAction>();
		}

		public float DecideAction(Context _context, out UAIAction _chosenAction, out object _chosenTarget)
		{
			float num = 0f;
			_chosenAction = null;
			_chosenTarget = null;
			for (int i = 0; i < this.actionList.Count; i++)
			{
				int num2 = 0;
				int num3 = 0;
				while (num3 < _context.ConsiderationData.EntityTargets.Count && num2 <= UAIBase.MaxEntitiesToConsider)
				{
					float score = this.actionList[i].GetScore(_context, _context.ConsiderationData.EntityTargets[num3], 0f);
					if (score > num)
					{
						num = score;
						_chosenAction = this.actionList[i];
						_chosenTarget = _context.ConsiderationData.EntityTargets[num3];
					}
					num2++;
					num3++;
				}
				int num4 = 0;
				while (num4 < _context.ConsiderationData.WaypointTargets.Count && num4 <= UAIBase.MaxWaypointsToConsider)
				{
					float score2 = this.actionList[i].GetScore(_context, _context.ConsiderationData.WaypointTargets[num4], 0f);
					if (score2 > num)
					{
						num = score2;
						_chosenAction = this.actionList[i];
						_chosenTarget = _context.ConsiderationData.WaypointTargets[num4];
					}
					num4++;
				}
			}
			return num;
		}

		public List<UAIAction> GetActions()
		{
			return this.actionList;
		}

		public void AddAction(UAIAction _action)
		{
			this.actionList.Add(_action);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<UAIAction> actionList;
	}
}
