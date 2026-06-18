using System;
using System.Globalization;
using Audio;
using UnityEngine;

public class SpinningBladeTrapController : MonoBehaviour
{
	public float HealthRatio
	{
		get
		{
			return this.healthRatio;
		}
		set
		{
			this.lastHealthRatio = this.healthRatio;
			this.healthRatio = value;
			if (this.healthRatio != this.lastHealthRatio)
			{
				this.CheckHealthChanged();
			}
		}
	}

	public float CurrentSpeedRatio
	{
		get
		{
			return this.windUpDownTime / this.windUpTimeMax * (this.windUpDownTime / this.windUpTimeMax);
		}
	}

	public bool IsOn
	{
		get
		{
			return this.isOn;
		}
		set
		{
			this.lastIsOn = this.isOn;
			this.isOn = value;
			this.BladeController.IsOn = value;
		}
	}

	public SpinningBladeTrapController.BladeTrapStates CurrentState
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.currentState;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.EnterState(this.currentState, value);
			this.currentState = value;
		}
	}

	public void Init(DynamicProperties _properties, Block _block)
	{
		if (this.initialized)
		{
			return;
		}
		this.initialized = true;
		this.breakingPercentage = 0.5f;
		if (_properties.Values.ContainsKey("BreakingPercentage"))
		{
			this.breakingPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BreakingPercentage"], 0, -1, NumberStyles.Any));
		}
		this.brokenPercentage = 0.25f;
		if (_properties.Values.ContainsKey("BrokenPercentage"))
		{
			this.brokenPercentage = Mathf.Clamp01(StringParsers.ParseFloat(_properties.Values["BrokenPercentage"], 0, -1, NumberStyles.Any));
		}
		if (_properties.Values.ContainsKey("StartSound"))
		{
			this.startSound = _properties.Values["StartSound"];
		}
		if (_properties.Values.ContainsKey("StopSound"))
		{
			this.stopSound = _properties.Values["StopSound"];
		}
		if (_properties.Values.ContainsKey("RunningSound"))
		{
			this.runningSound = _properties.Values["RunningSound"];
		}
		if (_properties.Values.ContainsKey("RunningSoundBreaking"))
		{
			this.runningSoundPartlyBroken = _properties.Values["RunningSoundBreaking"];
		}
		if (_properties.Values.ContainsKey("RunningSoundBroken"))
		{
			this.runningSoundBroken = _properties.Values["RunningSoundBroken"];
		}
		if (this.BladeController != null)
		{
			this.BladeController.Init(_properties, _block);
		}
		this.randomStartDelayMax = GameManager.Instance.World.GetGameRandom().RandomFloat * this.randomStartDelayMax;
	}

	public void DamageSelf(float damage)
	{
		this.totalDamage += damage;
		if (this.totalDamage < 1f)
		{
			return;
		}
		damage = (float)((int)this.totalDamage);
		this.totalDamage = 0f;
		if (this.chunk == null)
		{
			this.chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(this.BlockPosition);
		}
		BlockValue block = GameManager.Instance.World.GetBlock(this.BlockPosition);
		this.HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		block.damage = Mathf.Clamp(block.damage + (int)damage, 0, block.Block.MaxDamage);
		GameManager.Instance.World.SetBlock(this.BlockPosition, block, false, false);
	}

	public void StopAllSounds()
	{
		Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSound);
		Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSoundPartlyBroken);
		Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSoundBroken);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnterState(SpinningBladeTrapController.BladeTrapStates oldState, SpinningBladeTrapController.BladeTrapStates newState)
	{
		switch (newState)
		{
		case SpinningBladeTrapController.BladeTrapStates.IsOff:
			this.StopAllSounds();
			break;
		case SpinningBladeTrapController.BladeTrapStates.RandomWaitToStart:
			this.randomStartDelay = 0f;
			return;
		case SpinningBladeTrapController.BladeTrapStates.IsStarting:
			this.StopAllSounds();
			Manager.BroadcastPlay(this.BlockPosition.ToVector3(), this.startSound, 0f);
			return;
		case SpinningBladeTrapController.BladeTrapStates.IsOn:
			Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSoundPartlyBroken);
			Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSoundBroken);
			Manager.BroadcastPlay(this.BlockPosition.ToVector3(), this.runningSound, 0f);
			this.degreesPerSecond = this.degreesPerSecondMax;
			return;
		case SpinningBladeTrapController.BladeTrapStates.IsOnPartlyBroken:
			Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSound);
			Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSoundBroken);
			Manager.BroadcastPlay(this.BlockPosition.ToVector3(), this.runningSoundPartlyBroken, 0f);
			this.degreesPerSecond = this.degreesPerSecondMax;
			return;
		case SpinningBladeTrapController.BladeTrapStates.IsOnBroken:
			Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSound);
			Manager.BroadcastStop(this.BlockPosition.ToVector3(), this.runningSoundPartlyBroken);
			Manager.BroadcastPlay(this.BlockPosition.ToVector3(), this.runningSoundBroken, 0f);
			this.degreesPerSecond = this.degreesPerSecondMax;
			return;
		case SpinningBladeTrapController.BladeTrapStates.IsStopping:
			this.StopAllSounds();
			Manager.BroadcastPlay(this.BlockPosition.ToVector3(), this.stopSound, 0f);
			this.windUpDownTime = this.windDownTimeMax;
			if (this.HealthRatio <= this.brokenPercentage)
			{
				this.HandleParticlesForBroken();
				return;
			}
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		BlockValue block = GameManager.Instance.World.GetBlock(this.BlockPosition);
		if (block.isair)
		{
			return;
		}
		this.HealthRatio = 1f - (float)block.damage / (float)block.Block.MaxDamage;
		if (this.lastIsOn)
		{
			bool flag = !this.isOn;
		}
		if (this.isOn)
		{
			bool flag2 = !this.lastIsOn;
		}
		SpinningBladeTrapController.BladeTrapStates initialState = this.GetInitialState();
		if (initialState != this.currentState)
		{
			this.CurrentState = initialState;
			return;
		}
		switch (this.currentState)
		{
		case SpinningBladeTrapController.BladeTrapStates.IsOff:
			if (this.IsOn && this.HealthRatio >= this.brokenPercentage)
			{
				this.CurrentState = SpinningBladeTrapController.BladeTrapStates.RandomWaitToStart;
			}
			break;
		case SpinningBladeTrapController.BladeTrapStates.RandomWaitToStart:
			if (this.randomStartDelay < this.randomStartDelayMax)
			{
				this.randomStartDelay += Time.deltaTime;
			}
			else
			{
				this.windUpDownTime = 0f;
				this.CurrentState = SpinningBladeTrapController.BladeTrapStates.IsStarting;
			}
			break;
		case SpinningBladeTrapController.BladeTrapStates.IsStarting:
			if (this.degreesPerSecond < this.degreesPerSecondMax)
			{
				if (this.HealthRatio > this.breakingPercentage)
				{
					this.degreesPerSecond = Mathf.Lerp(0f, this.degreesPerSecondMax, this.CurrentSpeedRatio);
				}
				else
				{
					this.degreesPerSecond = Mathf.Lerp(0f, this.degreesPerSecondMax * (Mathf.Clamp(this.HealthRatio, 0f, this.breakingPercentage) * 2f), this.CurrentSpeedRatio);
					if (this.HealthRatio <= this.brokenPercentage)
					{
						this.degreesPerSecond = 0f;
					}
				}
			}
			this.windUpDownTime += Time.deltaTime;
			this.windUpDownTime = Mathf.Clamp(this.windUpDownTime, 0f, this.windUpTimeMax);
			if (this.degreesPerSecond == this.degreesPerSecondMax)
			{
				this.CheckHealthChanged();
			}
			if (!this.isOn)
			{
				this.CurrentState = SpinningBladeTrapController.BladeTrapStates.IsStopping;
			}
			break;
		case SpinningBladeTrapController.BladeTrapStates.IsOn:
		case SpinningBladeTrapController.BladeTrapStates.IsOnPartlyBroken:
		case SpinningBladeTrapController.BladeTrapStates.IsOnBroken:
			if (this.degreesPerSecond < this.degreesPerSecondMax)
			{
				if (this.HealthRatio > this.breakingPercentage)
				{
					this.degreesPerSecond = Mathf.Lerp(0f, this.degreesPerSecondMax, this.CurrentSpeedRatio);
				}
				else
				{
					this.degreesPerSecond = Mathf.Lerp(0f, this.degreesPerSecondMax * (Mathf.Clamp(this.HealthRatio, 0f, this.breakingPercentage) * 2f), this.CurrentSpeedRatio);
					if (this.HealthRatio <= this.brokenPercentage)
					{
						this.degreesPerSecond = 0f;
					}
				}
			}
			if (!this.isOn)
			{
				this.CurrentState = SpinningBladeTrapController.BladeTrapStates.IsStopping;
			}
			break;
		case SpinningBladeTrapController.BladeTrapStates.IsStopping:
			if (this.degreesPerSecond > 0f)
			{
				this.degreesPerSecond = Mathf.Lerp(0f, this.degreesPerSecond, this.CurrentSpeedRatio);
			}
			this.windUpDownTime -= Time.deltaTime;
			this.windUpDownTime = Mathf.Clamp(this.windUpDownTime, 0f, this.windDownTimeMax);
			this.degreesPerSecond = Mathf.Lerp(0f, this.degreesPerSecond, this.CurrentSpeedRatio);
			if (this.windUpDownTime <= 0f)
			{
				this.CurrentState = SpinningBladeTrapController.BladeTrapStates.IsOff;
			}
			if (this.IsOn && this.HealthRatio > this.brokenPercentage)
			{
				this.CurrentState = SpinningBladeTrapController.BladeTrapStates.IsStarting;
			}
			break;
		}
		float num = this.BladeControllerTransform.localRotation.eulerAngles.y - this.degreesPerSecond * Time.deltaTime;
		float num2 = Utils.FastLerp(-15f, 0f, Utils.FastClamp(this.HealthRatio, 0f, this.breakingPercentage) * 2f);
		this.BladeControllerTransform.localRotation = Quaternion.Euler(num2, num, 0f);
		this.BladeBottomTransform.localRotation = Quaternion.Euler(0f, num, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckHealthChanged()
	{
		if (this.CurrentState == SpinningBladeTrapController.BladeTrapStates.IsStarting || this.CurrentState == SpinningBladeTrapController.BladeTrapStates.IsOn || this.currentState == SpinningBladeTrapController.BladeTrapStates.IsOnPartlyBroken || this.currentState == SpinningBladeTrapController.BladeTrapStates.IsOnBroken)
		{
			SpinningBladeTrapController.BladeTrapStates stateByHealthRange = this.GetStateByHealthRange();
			if (stateByHealthRange != this.currentState)
			{
				this.CurrentState = stateByHealthRange;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SpinningBladeTrapController.BladeTrapStates GetInitialState()
	{
		if (this.isOn)
		{
			if (this.HealthRatio >= 0.75f)
			{
				return SpinningBladeTrapController.BladeTrapStates.IsOn;
			}
			if (this.HealthRatio >= this.breakingPercentage)
			{
				return SpinningBladeTrapController.BladeTrapStates.IsOnPartlyBroken;
			}
			if (this.HealthRatio > this.brokenPercentage)
			{
				return SpinningBladeTrapController.BladeTrapStates.IsOnBroken;
			}
		}
		return this.currentState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SpinningBladeTrapController.BladeTrapStates GetStateByHealthRange()
	{
		if (!this.isOn)
		{
			return this.currentState;
		}
		if (this.HealthRatio >= 0.75f)
		{
			return SpinningBladeTrapController.BladeTrapStates.IsOn;
		}
		if (this.HealthRatio >= this.breakingPercentage)
		{
			return SpinningBladeTrapController.BladeTrapStates.IsOnPartlyBroken;
		}
		if (this.HealthRatio > this.brokenPercentage)
		{
			return SpinningBladeTrapController.BladeTrapStates.IsOnBroken;
		}
		if (this.currentState == SpinningBladeTrapController.BladeTrapStates.IsOff)
		{
			return SpinningBladeTrapController.BladeTrapStates.IsOff;
		}
		return SpinningBladeTrapController.BladeTrapStates.IsStopping;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParticlesForBroken()
	{
		float num = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.BlockPosition.ToVector3())) / 2f;
		ParticleEffect particleEffect = new ParticleEffect("big_smoke", new Vector3(0f, 0.25f, 0f), num, new Color(1f, 1f, 1f, 0.3f), null, base.transform, false, 1f, "");
		GameManager.Instance.SpawnParticleEffectServer(particleEffect, -1, false, false);
		ParticleEffect particleEffect2 = new ParticleEffect("electric_fence_sparks", new Vector3(0f, 0.25f, 0f), num, new Color(1f, 1f, 1f, 0.3f), "electric_fence_impact", base.transform, false, 1f, "");
		GameManager.Instance.SpawnParticleEffectServer(particleEffect2, -1, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.Cleanup();
	}

	public void Cleanup()
	{
		this.StopAllSounds();
		this.IsOn = false;
		this.lastIsOn = false;
		this.currentState = SpinningBladeTrapController.BladeTrapStates.IsOff;
		this.degreesPerSecond = 0f;
		this.windUpDownTime = 0f;
		this.initialized = false;
	}

	public Transform BladeControllerTransform;

	public Transform BladeBottomTransform;

	public SpinningBladeTrapBladeController BladeController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastHealthRatio;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float healthRatio = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lastIsOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float degreesPerSecondMax = 720f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float degreesPerSecond;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windUpTimeMax = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windDownTimeMax = 7.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windUpDownTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float randomStartDelayMax = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float randomStartDelay;

	public Vector3i BlockPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string startSound = "Electricity/BladeTrap/bladetrap_startup";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string stopSound = "Electricity/BladeTrap/bladetrap_stop";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string runningSound = "Electricity/BladeTrap/bladetrap_fire_lp";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string runningSoundPartlyBroken = "Electricity/BladeTrap/bladetrap_dm1_lp";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string runningSoundBroken = "Electricity/BladeTrap/bladetrap_dm2_lp";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float brokenPercentage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float breakingPercentage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SpinningBladeTrapController.BladeTrapStates currentState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float totalDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string currentRunningSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum BladeTrapStates
	{
		IsOff,
		RandomWaitToStart,
		IsStarting,
		IsOn,
		IsOnPartlyBroken,
		IsOnBroken,
		IsStopping
	}
}
