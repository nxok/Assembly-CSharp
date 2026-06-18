using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionProjectile : ItemActionAttack
{
	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		this.Explosion = new ExplosionData(this.Properties, this.item.Effects);
		this.Properties.ParseFloat("FlyTime", ref this.FlyTime);
		this.Properties.ParseFloat("LifeTime", ref this.LifeTime);
		this.Properties.ParseFloat("DeadTime", ref this.DeadTime);
		this.Properties.ParseFloat("Velocity", ref this.Velocity);
		this.Gravity = -9.81f;
		this.Properties.ParseFloat("Gravity", ref this.Gravity);
		this.Properties.ParseFloat("CollisionRadius", ref this.collisionRadius);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
	}

	public new ExplosionData Explosion;

	public new float Velocity;

	public new float FlyTime;

	public new float LifeTime;

	public float DeadTime;

	public float Gravity;

	public float collisionRadius;
}
