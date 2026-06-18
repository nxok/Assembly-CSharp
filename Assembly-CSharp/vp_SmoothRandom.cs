using System;
using UnityEngine;

public class vp_SmoothRandom
{
	public static Vector3 GetVector3(float speed)
	{
		float num = Time.time * 0.01f * speed;
		return new Vector3(vp_SmoothRandom.Get().HybridMultifractal(num, 15.73f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num, 63.94f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num, 0.2f, 0.58f));
	}

	public static Vector3 GetVector3Centered(float speed)
	{
		float num = Time.time * 0.01f * speed;
		float num2 = (Time.time - 1f) * 0.01f * speed;
		Vector3 vector = new Vector3(vp_SmoothRandom.Get().HybridMultifractal(num, 15.73f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num, 63.94f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num, 0.2f, 0.58f));
		Vector3 vector2;
		vector2..ctor(vp_SmoothRandom.Get().HybridMultifractal(num2, 15.73f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num2, 63.94f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num2, 0.2f, 0.58f));
		return vector - vector2;
	}

	public static Vector3 GetVector3Centered(float time, float speed)
	{
		float num = time * 0.01f * speed;
		float num2 = (time - 1f) * 0.01f * speed;
		Vector3 vector = new Vector3(vp_SmoothRandom.Get().HybridMultifractal(num, 15.73f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num, 63.94f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num, 0.2f, 0.58f));
		Vector3 vector2;
		vector2..ctor(vp_SmoothRandom.Get().HybridMultifractal(num2, 15.73f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num2, 63.94f, 0.58f), vp_SmoothRandom.Get().HybridMultifractal(num2, 0.2f, 0.58f));
		return vector - vector2;
	}

	public static float Get(float speed)
	{
		float num = Time.time * 0.01f * speed;
		return vp_SmoothRandom.Get().HybridMultifractal(num * 0.01f, 15.7f, 0.65f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static vp_FractalNoise Get()
	{
		if (vp_SmoothRandom.s_Noise == null)
		{
			vp_SmoothRandom.s_Noise = new vp_FractalNoise(1.27f, 2.04f, 8.36f);
		}
		return vp_SmoothRandom.s_Noise;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static vp_FractalNoise s_Noise;
}
