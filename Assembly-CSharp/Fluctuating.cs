using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class Fluctuating : LightState
{
	public override float LODThreshold
	{
		get
		{
			return 0.2f;
		}
	}

	public override float Intensity
	{
		get
		{
			return this.currentIntensity;
		}
	}

	public override float Emissive
	{
		get
		{
			return this.currentEmissive;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.startIntensity = 1f;
		this.fixedFrameRate = 1f / Time.fixedDeltaTime;
		this.currentIntensity = this.startIntensity;
		this.currentEmissive = this.startIntensity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		if (this.canSwitchProcess)
		{
			this.ChangeProcess();
			this.currentFrame = 0;
			int num = (int)(this.lightLOD.FluxDelay * this.fixedFrameRate);
			if (this.process == 0)
			{
				this.numOfFrames = Random.Range(num / 2, num);
				this.t = 0f;
				this.preSlideIntenisty = this.currentIntensity;
				this.up = Random.Range(0f, 1f) > this.slideProbability(this.preSlideIntenisty);
				if (this.up)
				{
					this.slideTo = Random.Range(this.preSlideIntenisty, 1f);
				}
				else
				{
					this.slideTo = Random.Range(0.2f, this.preSlideIntenisty);
				}
				this.increment = (this.slideTo - this.preSlideIntenisty) / (float)this.numOfFrames;
			}
			else if (this.process == 1)
			{
				this.numOfFrames = Random.Range(90, 181);
			}
			else
			{
				this.numOfFrames = Random.Range(num / 2, num);
			}
		}
		if (this.process == 0)
		{
			this.Slide();
		}
		else if (this.process == 1)
		{
			this.Flutter();
		}
		else if (this.canSwitchProcess)
		{
			this.currentIntensity = this.startIntensity;
			this.currentEmissive = this.startIntensity / 1f;
		}
		int num2 = this.currentFrame + 1;
		this.currentFrame = num2;
		this.canSwitchProcess = num2 >= this.numOfFrames;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Slide()
	{
		if (this.up)
		{
			this.currentIntensity = Mathf.Lerp(this.preSlideIntenisty, this.slideTo, this.t);
			this.currentEmissive = this.currentIntensity / 1f;
			this.t += this.increment;
			return;
		}
		this.currentIntensity = Mathf.Lerp(this.slideTo, this.preSlideIntenisty, this.t);
		this.currentEmissive = this.currentIntensity / 1f;
		this.t -= this.increment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Flutter()
	{
		int num = Random.Range(0, 3);
		if (num == 0)
		{
			this.currentIntensity = Mathf.Clamp(this.currentIntensity + 0.0625f, 0.2f, 1f);
			this.currentEmissive = this.currentIntensity / 1f;
			return;
		}
		if (num != 1)
		{
			return;
		}
		this.currentIntensity = Mathf.Clamp(this.currentIntensity - 0.0625f, 0.2f, 1f);
		this.currentEmissive = this.currentIntensity / 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float slideProbability(float intensity)
	{
		return (intensity - 0.2f) / 0.8f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChangeProcess()
	{
		int num = Random.Range(1, 3);
		if (num != this.process)
		{
			this.process = num;
			return;
		}
		if (this.process > 0)
		{
			this.process = (this.process + 1) % 3;
			return;
		}
		this.process++;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float unityLightIntensityMax = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float hiRange = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float loRange = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float flutterVariance = 0.0625f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float increment;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float startIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fixedFrameRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentEmissive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canSwitchProcess = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int process;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int numOfFrames;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float t;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float preSlideIntenisty;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float slideTo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool up;
}
