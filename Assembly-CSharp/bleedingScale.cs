using System;
using UnityEngine;

public class bleedingScale : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		float x = this.parentObject.transform.lossyScale.x;
		ParticleSystem.MainModule main = base.gameObject.GetComponent<ParticleSystem>().main;
		ParticleSystem.MinMaxCurve minMaxCurve;
		minMaxCurve..ctor(this.minParticleScale * x, this.maxParticleScale * x);
		minMaxCurve.mode = 3;
		main.startSize = minMaxCurve;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	public GameObject parentObject;

	public float minParticleScale;

	public float maxParticleScale;
}
