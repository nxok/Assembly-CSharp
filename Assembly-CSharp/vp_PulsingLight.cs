using System;
using UnityEngine;

public class vp_PulsingLight : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.m_Light = base.GetComponent<Light>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.m_Light == null)
		{
			return;
		}
		this.m_Light.intensity = this.m_MinIntensity + Mathf.Abs(Mathf.Cos(Time.time * this.m_Rate) * (this.m_MaxIntensity - this.m_MinIntensity));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light m_Light;

	public float m_MinIntensity = 2f;

	public float m_MaxIntensity = 5f;

	public float m_Rate = 1f;
}
