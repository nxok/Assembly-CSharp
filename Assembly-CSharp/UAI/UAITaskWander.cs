using System;
using System.Globalization;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAITaskWander : UAITaskBase
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void initializeParameters()
		{
			base.initializeParameters();
			if (this.Parameters.ContainsKey("max_distance"))
			{
				this.maxWanderDistance = StringParsers.ParseFloat(this.Parameters["max_distance"], 0, -1, NumberStyles.Any);
			}
		}

		public override void Start(Context _context)
		{
			base.Start(_context);
			int num = 10;
			_context.Self.FindPath(RandomPositionGenerator.CalcAround(_context.Self, num, num), _context.Self.GetMoveSpeed(), false, null);
		}

		public override void Update(Context _context)
		{
			base.Update(_context);
			if (_context.Self.getNavigator().noPathAndNotPlanningOne())
			{
				this.Stop(_context);
			}
		}

		public float maxWanderDistance;
	}
}
