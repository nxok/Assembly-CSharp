using System;
using UnityEngine;

public class AudioSourceLifetimeSwitch : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		this.entityRoot = RootTransformRefEntity.FindEntityUpwards(base.transform);
		this.audio = base.transform.GetComponent<AudioSource>();
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
				if (this.audio)
				{
					this.audio.enabled = false;
					this.audio = null;
				}
				this.entity = null;
			}
		}
		if (this.audio && !this.entity)
		{
			this.delay -= Time.deltaTime;
			if (this.delay <= 0f)
			{
				this.audio.enabled = false;
				this.audio = null;
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
	public AudioSource audio;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float delay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bFirstUpdate = true;
}
