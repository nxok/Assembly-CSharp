using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class ConsiderationData
	{
		public ConsiderationData()
		{
			this.EntityTargets = new List<Entity>();
			this.WaypointTargets = new List<Vector3>();
		}

		public List<Entity> EntityTargets;

		public List<Vector3> WaypointTargets;
	}
}
