using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntitySwarm : EntityVulture
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		base.Init();
		this.targetAttackHealthPercent = 1f;
		this.ignoreTargetAttached = true;
		this.wanderHeightRange.x = 1f;
		this.wanderHeightRange.y = 8f;
		this.dissipateDelay = 24f;
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (!this.IsDead())
		{
			this.dissipateDelay -= 0.05f;
			if (this.dissipateDelay <= 2f && this.state != EntityVulture.State.Home)
			{
				base.StartHome(this.position + new Vector3(0f, 50f, 0f));
			}
			if (this.dissipateDelay <= 0f)
			{
				this.Kill(DamageResponse.New(true));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float dissipateDelay;
}
