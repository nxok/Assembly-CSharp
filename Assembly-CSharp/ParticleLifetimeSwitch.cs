using System;
using UnityEngine;

public class ParticleLifetimeSwitch : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		this.entityRoot = RootTransformRefEntity.FindEntityUpwards(base.transform);
		this.particles = base.transform.GetComponent<ParticleSystem>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (this.bFirstUpdate)
		{
			this.bFirstUpdate = false;
			this.entity = ((this.entityRoot != null) ? this.entityRoot.GetComponent<Entity>() : null);
			if (this.entity && this.entity.IsDead())
			{
				if (this.particles != null)
				{
					this.particles.emission.enabled = false;
					this.particles = null;
				}
				this.entity = null;
			}
		}
		if (this.particles && !this.entity)
		{
			this.delay -= Time.deltaTime;
			if (this.delay <= 0f)
			{
				this.particles.emission.enabled = false;
				this.particles = null;
			}
		}
		if (this.entity && this.entity.IsDead())
		{
			this.delay = this.TurnOffDelayAfterEntityDies;
			this.entity = null;
		}
	}

	public float TurnOffDelayAfterEntityDies = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform entityRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Entity entity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem particles;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float delay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bFirstUpdate = true;
}
