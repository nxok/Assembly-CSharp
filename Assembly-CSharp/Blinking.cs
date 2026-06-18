using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Blinking : LightState
{
	public override float LODThreshold
	{
		get
		{
			return 0.75f;
		}
	}

	public override bool CanBeOn
	{
		get
		{
			return this.switchedOn;
		}
	}

	public override float Emissive
	{
		get
		{
			if (!this.switchedOn)
			{
				return 0f;
			}
			return 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator blink()
	{
		for (;;)
		{
			this.switchedOn = !this.switchedOn;
			yield return new WaitForSeconds(1f / this.lightLOD.StateRate);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		base.StartCoroutine(this.blink());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		base.StopAllCoroutines();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool switchedOn = true;
}
