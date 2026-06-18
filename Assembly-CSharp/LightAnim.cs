using System;
using UnityEngine;

public class LightAnim : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.Duration = Utils.FastMax(this.Duration, 0.001f);
		this.LightRef = base.GetComponent<Light>();
		this.intensityStart = this.LightRef.intensity;
		int length = this.IntensityCurve.length;
		if (length > 0)
		{
			this.timeEnd = this.IntensityCurve[length - 1].time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float num = Time.deltaTime / this.Duration;
		this.time += num;
		if (this.time < this.timeEnd)
		{
			this.LightRef.intensity = this.intensityStart * this.IntensityCurve.Evaluate(this.time);
			return;
		}
		if (this.DestroyAtEnd)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		base.gameObject.SetActive(false);
	}

	public float Duration = 0.5f;

	public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.1f, 1f),
		new Keyframe(1f, 0f)
	});

	public bool DestroyAtEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light LightRef;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float intensityStart;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float time;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeEnd;
}
