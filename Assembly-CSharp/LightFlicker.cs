using System;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	public void Start()
	{
		this.Init();
	}

	public void Update()
	{
		this.m_time += Time.deltaTime;
		LightFlicker.Interval interval;
		for (;;)
		{
			interval = this.m_steps[this.m_intervalIdx];
			if (this.m_time < interval.Time)
			{
				break;
			}
			this.m_time -= interval.Time;
			this.m_baseLight = interval.Value;
			this.m_intervalIdx++;
			if (this.m_intervalIdx >= this.m_steps.Count)
			{
				this.m_intervalIdx = 0;
			}
		}
		base.GetComponent<Light>().intensity = Mathf.Lerp(this.m_baseLight, interval.Value, this.m_time / interval.Time);
	}

	public void Reset()
	{
		this.MinLight = 0.1f;
		this.MaxLight = 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		this.m_intervalIdx = 0;
		this.m_time = 0f;
		this.m_steps = new List<LightFlicker.Interval>();
		this.m_baseLight = this.MinLight;
		base.GetComponent<Light>().intensity = this.m_baseLight;
		float num = 0f;
		do
		{
			LightFlicker.Interval interval = new LightFlicker.Interval();
			interval.Time = Random.Range(Mathf.Max(0.001f, this.IntervalMin), Mathf.Max(0.001f, this.IntervalMax));
			interval.Value = Random.Range(this.MinLight, this.MaxLight);
			this.m_steps.Add(interval);
			num += interval.Time;
		}
		while (num < 5f);
	}

	public float MinLight;

	public float MaxLight;

	public float IntervalMin;

	public float IntervalMax;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int m_intervalIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float m_time;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float m_baseLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<LightFlicker.Interval> m_steps;

	[PublicizedFrom(EAccessModifier.Private)]
	public class Interval
	{
		public float Time;

		public float Value;
	}
}
