using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAITaskMoveToTarget : UAITaskBase
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void initializeParameters()
		{
			base.initializeParameters();
			if (this.Parameters.ContainsKey("distance"))
			{
				this.distance = StringParsers.ParseFloat(this.Parameters["distance"], 0, -1, NumberStyles.Any);
			}
			if (this.Parameters.ContainsKey("run"))
			{
				this.run = StringParsers.ParseBool(this.Parameters["run"], 0, -1, true);
			}
			if (this.Parameters.ContainsKey("break_walls"))
			{
				this.shouldBreakWalls = StringParsers.ParseBool(this.Parameters["break_walls"], 0, -1, true);
			}
		}

		public override void Start(Context _context)
		{
			base.Start(_context);
			EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
			if (entityAlive != null)
			{
				_context.Self.FindPath(RandomPositionGenerator.CalcNear(_context.Self, entityAlive.position, (int)this.distance, (int)this.distance), this.run ? _context.Self.GetMoveSpeedPanic() : (_context.Self.IsAlert ? _context.Self.GetMoveSpeedAggro() : _context.Self.GetMoveSpeed()), this.shouldBreakWalls, null);
				return;
			}
			if (_context.ActionData.Target.GetType() == typeof(Vector3))
			{
				_context.Self.FindPath(RandomPositionGenerator.CalcNear(_context.Self, (Vector3)_context.ActionData.Target, (int)this.distance, (int)this.distance), this.run ? _context.Self.GetMoveSpeedPanic() : _context.Self.GetMoveSpeed(), this.shouldBreakWalls, null);
				return;
			}
			this.Stop(_context);
		}

		public override void Update(Context _context)
		{
			base.Update(_context);
			if (_context.Self.getNavigator().noPathAndNotPlanningOne())
			{
				this.Stop(_context);
			}
		}

		public float distance;

		public bool run;

		public bool shouldBreakWalls;
	}
}
