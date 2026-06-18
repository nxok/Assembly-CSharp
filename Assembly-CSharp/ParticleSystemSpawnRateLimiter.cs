using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemSpawnRateLimiter : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		num = Utils.FastMax(0.15f, num);
		foreach (ParticleSystem particleSystem in base.GetComponentsInChildren<ParticleSystem>())
		{
			ParticleSystem.EmissionModule emission = particleSystem.emission;
			ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
			ParticleSystemSpawnRateLimiter.EmissionRateData emissionRateData = new ParticleSystemSpawnRateLimiter.EmissionRateData
			{
				mode = rateOverTime.mode,
				curveMultiplier = rateOverTime.curveMultiplier,
				originalBursts = new ParticleSystem.Burst[emission.burstCount]
			};
			emission.GetBursts(emissionRateData.originalBursts);
			switch (rateOverTime.mode)
			{
			case 0:
				emissionRateData.constant = rateOverTime.constant;
				break;
			case 1:
				emissionRateData.curve = new AnimationCurve(rateOverTime.curve.keys);
				break;
			case 2:
				emissionRateData.curveMin = new AnimationCurve(rateOverTime.curveMin.keys);
				emissionRateData.curveMax = new AnimationCurve(rateOverTime.curveMax.keys);
				break;
			case 3:
				emissionRateData.constantMin = rateOverTime.constantMin;
				emissionRateData.constantMax = rateOverTime.constantMax;
				break;
			}
			this.originalRates[particleSystem] = emissionRateData;
			this.ScaleEmissionRate(particleSystem, num);
		}
		GameOptionsManager.OnGameOptionsApplied += this.OnGameOptionsApplied;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.OnGameOptionsApplied -= this.OnGameOptionsApplied;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameOptionsApplied()
	{
		float num = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		num = Utils.FastMax(0.15f, num);
		foreach (ParticleSystem particleSystem in base.GetComponentsInChildren<ParticleSystem>())
		{
			if (this.originalRates.ContainsKey(particleSystem))
			{
				this.ScaleEmissionRate(particleSystem, num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ScaleEmissionRate(ParticleSystem ps, float scale)
	{
		if (!this.originalRates.ContainsKey(ps))
		{
			return;
		}
		ParticleSystem.EmissionModule emission = ps.emission;
		ParticleSystemSpawnRateLimiter.EmissionRateData emissionRateData = this.originalRates[ps];
		ParticleSystem.MinMaxCurve minMaxCurve = default(ParticleSystem.MinMaxCurve);
		switch (emissionRateData.mode)
		{
		case 0:
			minMaxCurve..ctor(emissionRateData.constant * scale);
			break;
		case 1:
			minMaxCurve..ctor(emissionRateData.curveMultiplier * scale, emissionRateData.curve);
			break;
		case 2:
			minMaxCurve..ctor(emissionRateData.curveMultiplier * scale, emissionRateData.curveMin, emissionRateData.curveMax);
			break;
		case 3:
			minMaxCurve..ctor(emissionRateData.constantMin * scale, emissionRateData.constantMax * scale);
			break;
		}
		emission.rateOverTime = minMaxCurve;
		if (emissionRateData.originalBursts != null && emissionRateData.originalBursts.Length != 0)
		{
			ParticleSystem.Burst[] array = new ParticleSystem.Burst[emissionRateData.originalBursts.Length];
			for (int i = 0; i < emissionRateData.originalBursts.Length; i++)
			{
				ParticleSystem.Burst burst = emissionRateData.originalBursts[i];
				array[i] = new ParticleSystem.Burst(burst.time, ParticleSystemSpawnRateLimiter.ScaleBurstCurve(burst.count, scale), burst.cycleCount, burst.repeatInterval);
			}
			emission.SetBursts(array);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ParticleSystem.MinMaxCurve ScaleBurstCurve(ParticleSystem.MinMaxCurve original, float scale)
	{
		ParticleSystem.MinMaxCurve minMaxCurve = default(ParticleSystem.MinMaxCurve);
		minMaxCurve.mode = original.mode;
		switch (original.mode)
		{
		case 0:
			minMaxCurve.constant = ParticleSystemSpawnRateLimiter.ScaleBurstConstant(original.constant, scale);
			break;
		case 1:
			minMaxCurve.curveMultiplier = original.curveMultiplier * scale;
			minMaxCurve.curve = original.curve;
			break;
		case 2:
			minMaxCurve.curveMultiplier = original.curveMultiplier * scale;
			minMaxCurve.curveMin = original.curveMin;
			minMaxCurve.curveMax = original.curveMax;
			break;
		case 3:
			minMaxCurve.constantMin = ParticleSystemSpawnRateLimiter.ScaleBurstConstant(original.constantMin, scale);
			minMaxCurve.constantMax = ParticleSystemSpawnRateLimiter.ScaleBurstConstant(original.constantMax, scale);
			break;
		}
		return minMaxCurve;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ScaleBurstConstant(float constant, float scale)
	{
		if (Utils.FastAbs(constant) <= 1E-45f)
		{
			return 0f;
		}
		return Utils.FastMax(Mathf.Round(constant * scale), 1f) + float.Epsilon;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cScaleMin = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<ParticleSystem, ParticleSystemSpawnRateLimiter.EmissionRateData> originalRates = new Dictionary<ParticleSystem, ParticleSystemSpawnRateLimiter.EmissionRateData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class EmissionRateData
	{
		public ParticleSystemCurveMode mode;

		public float constant;

		public AnimationCurve curve;

		public float curveMultiplier;

		public float constantMin;

		public float constantMax;

		public AnimationCurve curveMin;

		public AnimationCurve curveMax;

		public ParticleSystem.Burst[] originalBursts;
	}
}
