using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityFlying : EntityEnemy
{
	public override bool IsAirBorne()
	{
		return true;
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		if (this.AttachedToEntity != null)
		{
			return;
		}
		if (this.IsDead())
		{
			this.entityCollision(this.motion);
			this.motion.y = this.motion.y - 0.08f;
			this.motion.y = this.motion.y * 0.98f;
			this.motion.x = this.motion.x * 0.91f;
			this.motion.z = this.motion.z * 0.91f;
			return;
		}
		if (base.IsInWater())
		{
			this.Move(_direction, _isDirAbsolute, 0.02f, 1f);
			this.entityCollision(this.motion);
			this.motion *= 0.8f;
			return;
		}
		float num = 0.91f;
		if (this.onGround)
		{
			num = 0.55f;
			BlockValue block = this.world.GetBlock(Utils.Fastfloor(this.position.x), Utils.Fastfloor(this.boundingBox.min.y) - 1, Utils.Fastfloor(this.position.z));
			if (!block.isair)
			{
				num = Mathf.Clamp(block.Block.blockMaterial.Friction, 0.01f, 1f);
			}
		}
		float num2 = 0.163f / (num * num * num);
		this.Move(_direction, _isDirAbsolute, this.onGround ? (0.1f * num2) : 0.02f, 1f);
		this.entityCollision(this.motion);
		this.motion *= num;
	}
}
