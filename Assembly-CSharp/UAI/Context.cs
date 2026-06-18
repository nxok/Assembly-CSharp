using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class Context
	{
		public List<string> AIPackages
		{
			get
			{
				return this.Self.AIPackages;
			}
		}

		public Context(EntityAlive _self)
		{
			this.Self = _self;
			this.World = GameManager.Instance.World;
			this.ConsiderationData = new ConsiderationData();
			this.ActionData = default(ActionData);
		}

		public EntityAlive Self;

		public World World;

		public ConsiderationData ConsiderationData;

		public ActionData ActionData;

		public float updateTimer;
	}
}
