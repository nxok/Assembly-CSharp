using System;
using System.Globalization;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAITaskFleeFromTarget : UAITaskBase
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void initializeParameters()
		{
			base.initializeParameters();
			if (this.Parameters.ContainsKey("max_distance"))
			{
				this.maxFleeDistance = StringParsers.ParseFloat(this.Parameters["max_distance"], 0, -1, NumberStyles.Any);
			}
		}

		public override void Start(Context _context)
		{
			base.Start(_context);
			EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
			if (entityAlive != null)
			{
				_context.Self.detachHome();
				_context.Self.FindPath(RandomPositionGenerator.CalcAway(_context.Self, 0, (int)this.maxFleeDistance, (int)this.maxFleeDistance, entityAlive.position), _context.Self.GetMoveSpeedPanic(), false, null);
				return;
			}
			_context.ActionData.Failed = true;
		}

		public override void Update(Context _context)
		{
			base.Update(_context);
			if (_context.Self.getNavigator().noPathAndNotPlanningOne())
			{
				_context.Self.setHomeArea(new Vector3i(_context.Self.position), 10);
				this.Stop(_context);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float maxFleeDistance;
	}
}
