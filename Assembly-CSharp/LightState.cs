using System;
using UnityEngine;

public abstract class LightState : MonoBehaviour
{
	public virtual float LODThreshold
	{
		get
		{
			return 1f;
		}
	}

	public virtual bool CanBeOn
	{
		get
		{
			return true;
		}
	}

	public virtual float Intensity
	{
		get
		{
			return 1f;
		}
	}

	public virtual float Emissive
	{
		get
		{
			return 1f;
		}
	}

	public virtual float AudioFrequency
	{
		get
		{
			if (!this.lightLOD)
			{
				return -1f;
			}
			return this.lightLOD.StateRate;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.lightLOD = base.gameObject.GetComponent<LightLOD>();
	}

	public virtual void Kill()
	{
		Object.Destroy(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public LightState()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public LightLOD lightLOD;
}
