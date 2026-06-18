using System;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		base.GetComponent<Light>().intensity = 0f;
		this.targetIntensity = this.highIntensity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (this.alarmOn)
		{
			base.GetComponent<Light>().intensity = Mathf.Lerp(base.GetComponent<Light>().intensity, this.targetIntensity, this.fadeSpeed * Time.deltaTime);
			this.CheckTargetIntensity();
			return;
		}
		base.GetComponent<Light>().intensity = Mathf.Lerp(base.GetComponent<Light>().intensity, 0f, this.fadeSpeed * Time.deltaTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckTargetIntensity()
	{
		if (Mathf.Abs(this.targetIntensity - base.GetComponent<Light>().intensity) < this.changeMargin)
		{
			if (this.targetIntensity == this.highIntensity)
			{
				this.targetIntensity = this.lowIntensity;
				return;
			}
			this.targetIntensity = this.highIntensity;
		}
	}

	public float fadeSpeed = 2f;

	public float highIntensity = 2f;

	public float lowIntensity = 0.5f;

	public float changeMargin = 0.2f;

	public bool alarmOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetIntensity;
}
