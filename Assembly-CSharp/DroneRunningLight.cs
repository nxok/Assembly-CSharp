using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneRunningLight : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.runningLight = base.GetComponent<Light>();
		this.particles = base.transform.GetComponentInChildren<ParticleSystem>();
		this._initLights();
		this.setLightsActive(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _initLights()
	{
		this.lightBlinkTimer = this.LightBlinkInterval;
		this.startIntensity = this.MinLightIntensity;
		this.runningLight.intensity = this.startIntensity;
		this.setLightColor(this.LightColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLightsActive(bool value)
	{
		this.runningLight.enabled = value;
		if (this.connectedLights != null)
		{
			for (int i = 0; i < this.connectedLights.Count; i++)
			{
				this.connectedLights[i].enabled = value;
			}
		}
		if (!this.dayTimeVisibility)
		{
			this.particles.gameObject.SetActive(value);
		}
		this.lightsActive = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLightColor(Color color)
	{
		this.runningLight.color = color;
		this.particles.main.startColor = color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		float num = (float)GameUtils.WorldTimeToHours(world.worldTime);
		if (num > 4f && num < 22f)
		{
			if (this.runningLight.intensity > this.startIntensity)
			{
				this._initLights();
			}
			if (this.lightsActive)
			{
				this.setLightsActive(!this.lightsActive);
			}
			return;
		}
		if (!this.lightsActive)
		{
			this.setLightsActive(!this.lightsActive);
		}
		if (this.runningLight.color != this.LightColor || this.particles.main.startColor.color != this.LightColor)
		{
			this.setLightColor(this.LightColor);
		}
		if (this.startIntensity != this.MinLightIntensity)
		{
			this.startIntensity = this.MinLightIntensity;
		}
		if (this.lightBlinkTimer > 0f)
		{
			this.lightBlinkTimer -= Time.deltaTime;
			if (this.lightBlinkTimer < 0.2f && this.lightBlinkTimer > 0.15f && this.particles.gameObject.activeSelf)
			{
				this.particles.gameObject.SetActive(false);
			}
			if (this.lightBlinkTimer < 0.15f && !this.particles.gameObject.activeSelf)
			{
				this.particles.gameObject.SetActive(true);
			}
			if (this.lightBlinkTimer <= 0f)
			{
				base.StartCoroutine(this.blink());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator blink()
	{
		this.transitionTimer = this.transitionTime;
		while (this.runningLight.intensity < this.MaxLightIntensity)
		{
			this.transitionTimer += Time.deltaTime;
			this.runningLight.intensity = Mathf.Lerp(this.startIntensity, this.MaxLightIntensity, this.transitionTimer / this.transitionTime);
			yield return null;
		}
		this.transitionTimer = this.transitionTime;
		while (this.runningLight.intensity > this.startIntensity)
		{
			this.transitionTimer += Time.deltaTime;
			this.runningLight.intensity = Mathf.Lerp(this.MaxLightIntensity, this.startIntensity, this.transitionTimer / this.transitionTime);
			yield return null;
		}
		this.runningLight.intensity = this.startIntensity;
		this.lightBlinkTimer = this.LightBlinkInterval;
		yield return null;
		yield break;
	}

	public float MinLightIntensity;

	public float MaxLightIntensity;

	public float LightBlinkInterval;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light runningLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float startIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem particles;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightBlinkTimer;

	public Color LightColor;

	public List<Light> connectedLights;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lightsActive;

	public bool dayTimeVisibility;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float transitionTime = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float transitionTimer;
}
