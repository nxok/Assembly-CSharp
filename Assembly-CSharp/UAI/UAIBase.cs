using System;
using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
	public static class UAIBase
	{
		public static void Update(Context _context)
		{
			if (_context.updateTimer <= 0f)
			{
				_context.updateTimer = UAIBase.ActionChoiceDelay;
				UAIBase.chooseAction(_context);
			}
			UAIBase.updateAction(_context);
			_context.updateTimer -= Time.deltaTime;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void updateAction(Context _context)
		{
			if (_context.ActionData.CurrentTask == null)
			{
				return;
			}
			if (!_context.ActionData.Initialized)
			{
				_context.ActionData.CurrentTask.Init(_context);
			}
			if (!_context.ActionData.Started)
			{
				_context.ActionData.CurrentTask.Start(_context);
			}
			if (_context.ActionData.Executing)
			{
				_context.ActionData.CurrentTask.Update(_context);
				return;
			}
			_context.ActionData.CurrentTask.Reset(_context);
			if (_context.ActionData.TaskIndex + 1 < _context.ActionData.Action.GetTasks().Count)
			{
				_context.ActionData.TaskIndex = _context.ActionData.TaskIndex + 1;
				return;
			}
			_context.ActionData.Action = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void chooseAction(Context _context)
		{
			float num = 0f;
			_context.ConsiderationData.EntityTargets.Clear();
			_context.ConsiderationData.WaypointTargets.Clear();
			UAIBase.addEntityTargetsToConsider(_context);
			UAIBase.addWaypointTargetsToConsider(_context);
			for (int i = 0; i < _context.AIPackages.Count; i++)
			{
				UAIAction uaiaction;
				object obj;
				if (UAIBase.AIPackages.ContainsKey(_context.AIPackages[i]) && UAIBase.AIPackages[_context.AIPackages[i]].DecideAction(_context, out uaiaction, out obj) * UAIBase.AIPackages[_context.AIPackages[i]].Weight > num && _context.ActionData.Action != uaiaction)
				{
					if (_context.ActionData.Action != null && _context.ActionData.CurrentTask != null)
					{
						if (_context.ActionData.Started)
						{
							_context.ActionData.CurrentTask.Stop(_context);
						}
						if (_context.ActionData.Initialized)
						{
							_context.ActionData.CurrentTask.Reset(_context);
						}
					}
					_context.ActionData.Action = uaiaction;
					_context.ActionData.Target = obj;
					_context.ActionData.TaskIndex = 0;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void addWaypointTargetsToConsider(Context _context)
		{
			if (_context.ConsiderationData.WaypointTargets == null)
			{
				_context.ConsiderationData.WaypointTargets = new List<Vector3>();
			}
			if (_context.ConsiderationData.WaypointTargets.Count > 1)
			{
				_context.ConsiderationData.WaypointTargets.Sort(new UAIUtils.NearestWaypointSorter(_context.Self));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void addEntityTargetsToConsider(Context _context)
		{
			if (_context.ConsiderationData.EntityTargets == null)
			{
				_context.ConsiderationData.EntityTargets = new List<Entity>();
			}
			if (_context.Self.GetRevengeTarget() != null)
			{
				_context.ConsiderationData.EntityTargets.Add(_context.Self.GetRevengeTarget());
			}
			_context.ConsiderationData.EntityTargets.AddRange(_context.Self.world.GetEntitiesInBounds(_context.Self, BoundsUtils.ExpandBounds(_context.Self.boundingBox, _context.Self.GetSeeDistance(), _context.Self.GetSeeDistance(), _context.Self.GetSeeDistance())));
			if (_context.ConsiderationData.EntityTargets.Count > 1)
			{
				_context.ConsiderationData.EntityTargets.Sort(new UAIUtils.NearestEntitySorter(_context.Self));
			}
		}

		public static void Cleanup()
		{
			if (UAIBase.AIPackages != null)
			{
				UAIBase.AIPackages.Clear();
			}
		}

		public static void Reload()
		{
			UAIBase.AIPackages.Clear();
			WorldStaticData.Reset("utilityai");
		}

		public static Dictionary<string, UAIPackage> AIPackages = new Dictionary<string, UAIPackage>();

		public static int MaxEntitiesToConsider = 5;

		public static int MaxWaypointsToConsider = 5;

		public static float ActionChoiceDelay = 0.2f;
	}
}
