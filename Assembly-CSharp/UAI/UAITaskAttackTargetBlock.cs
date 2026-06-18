using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAITaskAttackTargetBlock : UAITaskBase
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void initializeParameters()
		{
			base.initializeParameters();
		}

		public override void Start(Context _context)
		{
			base.Start(_context);
			if (_context.ActionData.Target.GetType() == typeof(Vector3))
			{
				this.attackTimeout = _context.Self.GetAttackTimeoutTicks();
				Vector3 vector = (Vector3)_context.ActionData.Target;
				_context.Self.SetLookPosition(_context.Self.CanSee(vector) ? vector : Vector3.zero);
				if (_context.Self.bodyDamage.HasLimbs)
				{
					_context.Self.RotateTo(vector.x, vector.y, vector.z, 30f, 30f);
					return;
				}
			}
			else
			{
				this.Stop(_context);
			}
		}

		public override void Update(Context _context)
		{
			base.Update(_context);
			if (_context.ActionData.Target.GetType() == typeof(Vector3))
			{
				Vector3 vector = (Vector3)_context.ActionData.Target;
				this.attackTimeout = Utils.FastMax(this.attackTimeout - 1, 0);
				if (this.attackTimeout > 0)
				{
					return;
				}
				_context.Self.SetLookPosition(vector);
				if (_context.Self.bodyDamage.HasLimbs)
				{
					_context.Self.RotateTo(vector.x, vector.y, vector.z, 30f, 30f);
				}
				if (_context.Self.Attack(false))
				{
					this.attackTimeout = _context.Self.GetAttackTimeoutTicks();
					_context.Self.Attack(true);
					this.Stop(_context);
					return;
				}
			}
			else
			{
				this.Stop(_context);
			}
		}

		public int attackTimeout;
	}
}
