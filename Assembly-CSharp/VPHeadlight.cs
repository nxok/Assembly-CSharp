using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPHeadlight : VehiclePart
{
	public override void InitPrefabConnections()
	{
		this.headlightT = base.GetTransform();
		if (this.headlightT)
		{
			GameObject gameObject = this.headlightT.gameObject;
			this.lights = new List<Light>();
			gameObject.GetComponentsInChildren<Light>(true, this.lights);
			for (int i = this.lights.Count - 1; i >= 0; i--)
			{
				if (this.lights[i].type != null)
				{
					this.lights.RemoveAt(i);
				}
			}
		}
		Transform transform = base.GetTransform("matT");
		if (transform)
		{
			MeshRenderer componentInChildren = transform.GetComponentInChildren<MeshRenderer>();
			if (componentInChildren)
			{
				this.headlightMat = componentInChildren.material;
			}
		}
		this.modT = base.GetTransform("modT");
		if (this.modT)
		{
			List<MeshRenderer> list = new List<MeshRenderer>();
			this.modT.GetComponentsInChildren<MeshRenderer>(list);
			for (int j = 0; j < list.Count; j++)
			{
				MeshRenderer meshRenderer = list[j];
				if (meshRenderer.gameObject.CompareTag("LOD"))
				{
					if (!this.modMat)
					{
						this.modMat = meshRenderer.material;
					}
					else
					{
						meshRenderer.material = this.modMat;
					}
				}
			}
		}
		this.modOnT = base.GetTransform("modOnT");
		if (this.modOnT)
		{
			GameObject gameObject2 = this.modOnT.gameObject;
			this.modLights = new List<Light>();
			gameObject2.GetComponentsInChildren<Light>(true, this.modLights);
			for (int k = this.modLights.Count - 1; k >= 0; k--)
			{
				if (this.modLights[k].type != null)
				{
					this.modLights.RemoveAt(k);
				}
			}
		}
		this.curIntensity = -1f;
	}

	public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);
		StringParsers.TryParseFloat(base.GetProperty("bright"), out this.bright);
		this.properties.ParseColorHex("matEmissive", ref this.headLightEmissive);
		this.properties.ParseColorHex("modMatEmissive", ref this.modMatEmissive);
		this.properties.ParseColorHex("tailEmissive", ref this.tailLightEmissive);
	}

	public override void SetMods()
	{
		base.SetMods();
		this.UpdateOn();
	}

	public override void HandleEvent(VehiclePart.Event _event, VehiclePart _part, float _arg)
	{
		if (_event == VehiclePart.Event.LightsOn)
		{
			this.SetOn(_arg != 0f);
			this.PlaySound();
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsBroken())
		{
			this.SetOn(false);
			return;
		}
		if (this.lights != null)
		{
			float num = (this.vehicle.EffectLightIntensity - 1f) * 0.5f + 1f;
			num = Utils.FastClamp(num, 0f, 10f);
			if (num != this.curIntensity)
			{
				this.curIntensity = num;
				float num2 = this.bright * num;
				float num3 = 50f * num;
				if (this.modInstalled && this.modLights != null)
				{
					num2 *= 0.58f;
					for (int i = this.modLights.Count - 1; i >= 0; i--)
					{
						Light light = this.modLights[i];
						light.intensity = num2;
						light.range = num3;
					}
				}
				for (int j = this.lights.Count - 1; j >= 0; j--)
				{
					Light light2 = this.lights[j];
					light2.intensity = num2;
					light2.range = num3;
				}
			}
		}
		this.SetTailLights();
	}

	public void SetTailLights()
	{
		float num = 0f;
		if (this.isOn)
		{
			num = 0.5f;
		}
		if (this.vehicle.CurrentIsBreak)
		{
			num = 1f;
		}
		if (num == this.tailLightIntensity)
		{
			return;
		}
		this.tailLightIntensity = num;
		if (this.vehicle.mainEmissiveMat && this.tailLightEmissive.a > 0f)
		{
			Color color = this.tailLightEmissive;
			color.r *= num;
			color.g *= num;
			color.b *= num;
			this.vehicle.mainEmissiveMat.SetColor("_EmissionColor", color);
		}
	}

	public bool IsOn()
	{
		return this.isOn;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOn(bool _isOn)
	{
		if (_isOn == this.isOn)
		{
			return;
		}
		this.isOn = _isOn;
		this.UpdateOn();
		this.SetTailLights();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateOn()
	{
		this.curIntensity = -1f;
		if (this.headlightT)
		{
			this.headlightT.gameObject.SetActive(this.isOn);
		}
		if (this.headlightMat)
		{
			Color color = (this.isOn ? this.headLightEmissive : Color.black);
			this.headlightMat.SetColor("_EmissionColor", color);
		}
		if (this.modInstalled)
		{
			if (this.modOnT)
			{
				this.modOnT.gameObject.SetActive(this.isOn);
			}
			if (this.modMat)
			{
				Color color2 = (this.isOn ? this.modMatEmissive : Color.black);
				this.modMat.SetColor("_EmissionColor", color2);
			}
		}
	}

	public float GetLightLevel()
	{
		if (!this.isOn)
		{
			return 0f;
		}
		return this.bright * 3f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlaySound()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!primaryPlayer || !primaryPlayer.IsSpawned())
		{
			return;
		}
		if (this.vehicle.entity != null && !this.vehicle.entity.isEntityRemote)
		{
			this.vehicle.entity.PlayOneShot("UseActions/flashlight_toggle", false, false, false, null, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRange = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cModBrightPer = 0.58f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform headlightT;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Light> lights;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material headlightMat;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color headLightEmissive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform modT;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material modMat;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color modMatEmissive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform modOnT;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Light> modLights;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bright;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color tailLightEmissive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tailLightIntensity = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public float curIntensity;
}
