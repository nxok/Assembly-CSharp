using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.DuckType.Jiggle
{
	public static class JiggleScheduler
	{
		public static void Register(Jiggle jiggleBone)
		{
			JiggleScheduler.s_Records[jiggleBone] = JiggleScheduler.GetHierarchyDepth(jiggleBone.transform);
			JiggleScheduler.isDirty = true;
		}

		public static void Deregister(Jiggle jiggleBone)
		{
			JiggleScheduler.s_Records.Remove(jiggleBone);
			JiggleScheduler.isDirty = true;
		}

		public static void Update(Jiggle jiggle)
		{
			if (JiggleScheduler.isDirty)
			{
				JiggleScheduler.isDirty = false;
				JiggleScheduler.UpdateOrderedRecords();
			}
			if (jiggle == JiggleScheduler.m_UpdateTriggerJiggle)
			{
				foreach (Jiggle jiggle2 in JiggleScheduler.s_OrderedRecords)
				{
					if (jiggle2.enabled && !jiggle2.UpdateWithPhysics)
					{
						jiggle2.ScheduledUpdate(Time.deltaTime);
					}
				}
			}
		}

		public static void FixedUpdate(Jiggle jiggle)
		{
			if (jiggle == JiggleScheduler.m_UpdateTriggerJiggle)
			{
				foreach (Jiggle jiggle2 in JiggleScheduler.s_OrderedRecords)
				{
					if (jiggle2.enabled && jiggle2.UpdateWithPhysics)
					{
						jiggle2.ScheduledUpdate(Time.fixedDeltaTime);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void UpdateOrderedRecords()
		{
			JiggleScheduler.s_OrderedRecords = (from x in JiggleScheduler.s_Records
				orderby x.Value
				select x.Key).ToList<Jiggle>();
			JiggleScheduler.m_UpdateTriggerJiggle = JiggleScheduler.s_OrderedRecords.FirstOrDefault<Jiggle>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int GetHierarchyDepth(Transform t)
		{
			if (!(t == null))
			{
				return JiggleScheduler.GetHierarchyDepth(t.parent) + 1;
			}
			return -1;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<Jiggle, int> s_Records = new Dictionary<Jiggle, int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<Jiggle> s_OrderedRecords = new List<Jiggle>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static Jiggle m_UpdateTriggerJiggle = null;

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool isDirty;
	}
}
