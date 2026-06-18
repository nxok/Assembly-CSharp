using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_DamageInfo
{
	public vp_DamageInfo(float damage, Transform source)
	{
		this.Damage = damage;
		this.Source = source;
		this.OriginalSource = source;
	}

	public vp_DamageInfo(float damage, Transform source, Transform originalSource)
	{
		this.Damage = damage;
		this.Source = source;
		this.OriginalSource = originalSource;
	}

	public float Damage;

	public Transform Source;

	public Transform OriginalSource;
}
