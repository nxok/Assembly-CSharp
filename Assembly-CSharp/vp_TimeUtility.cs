using System;
using UnityEngine;

public static class vp_TimeUtility
{
	public static float TimeScale
	{
		get
		{
			return Time.timeScale;
		}
		set
		{
			value = vp_TimeUtility.ClampTimeScale(value);
			Time.timeScale = value;
			Time.fixedDeltaTime = vp_TimeUtility.InitialFixedTimeStep * Time.timeScale;
		}
	}

	public static float AdjustedTimeScale
	{
		get
		{
			return 1f / (Time.timeScale * (0.02f / Time.fixedDeltaTime));
		}
	}

	public static void FadeTimeScale(float targetTimeScale, float fadeSpeed)
	{
		if (vp_TimeUtility.TimeScale == targetTimeScale)
		{
			return;
		}
		targetTimeScale = vp_TimeUtility.ClampTimeScale(targetTimeScale);
		vp_TimeUtility.TimeScale = Mathf.Lerp(vp_TimeUtility.TimeScale, targetTimeScale, Time.deltaTime * 60f * fadeSpeed);
		if (Mathf.Abs(vp_TimeUtility.TimeScale - targetTimeScale) < 0.01f)
		{
			vp_TimeUtility.TimeScale = targetTimeScale;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ClampTimeScale(float t)
	{
		if (t < vp_TimeUtility.m_MinTimeScale || t > vp_TimeUtility.m_MaxTimeScale)
		{
			t = Mathf.Clamp(t, vp_TimeUtility.m_MinTimeScale, vp_TimeUtility.m_MaxTimeScale);
			Debug.LogWarning(string.Concat(new string[]
			{
				"Warning: (vp_TimeUtility) TimeScale was clamped to within the supported range (",
				vp_TimeUtility.m_MinTimeScale.ToCultureInvariantString(),
				" - ",
				vp_TimeUtility.m_MaxTimeScale.ToCultureInvariantString(),
				")."
			}));
		}
		return t;
	}

	public static bool Paused
	{
		get
		{
			return vp_TimeUtility.m_Paused;
		}
		set
		{
			if (value)
			{
				if (vp_TimeUtility.m_Paused)
				{
					return;
				}
				vp_TimeUtility.m_Paused = true;
				vp_TimeUtility.m_TimeScaleOnPause = Time.timeScale;
				Time.timeScale = 0f;
				return;
			}
			else
			{
				if (!vp_TimeUtility.m_Paused)
				{
					return;
				}
				vp_TimeUtility.m_Paused = false;
				Time.timeScale = vp_TimeUtility.m_TimeScaleOnPause;
				vp_TimeUtility.m_TimeScaleOnPause = 1f;
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_MinTimeScale = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_MaxTimeScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool m_Paused = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float m_TimeScaleOnPause = 1f;

	public static float InitialFixedTimeStep = Time.fixedDeltaTime;
}
