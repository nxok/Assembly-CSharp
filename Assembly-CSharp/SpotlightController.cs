using System;
using UnityEngine;

public class SpotlightController : MonoBehaviour, IPowerSystemCamera
{
	public int UserAccessingId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = -1;

	public void Init(DynamicProperties _properties)
	{
		if (this.initialized)
		{
			return;
		}
		this.initialized = true;
		if (this.Cone != null)
		{
			MeshRenderer component = this.Cone.GetComponent<MeshRenderer>();
			if (component != null)
			{
				if (component.material != null)
				{
					this.ConeMaterial = component.material;
					this.ConeColor = this.ConeMaterial.GetColor("_Color");
				}
				else if (component.sharedMaterial != null)
				{
					this.ConeMaterial = component.sharedMaterial;
					this.ConeColor = this.ConeMaterial.GetColor("_Color");
				}
			}
			this.Cone.gameObject.SetActive(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.TileEntity == null)
		{
			return;
		}
		if (this.TileEntity.IsPowered && this.UserAccessingId == -1)
		{
			if (this.TileEntity.IsPowered)
			{
				if (this.YawController.Yaw != this.TileEntity.CenteredYaw)
				{
					this.YawController.Yaw = Mathf.Lerp(this.YawController.Yaw, this.TileEntity.CenteredYaw, Time.deltaTime * this.degreesPerSecond);
					this.YawController.UpdateYaw();
				}
				if (this.PitchController.Pitch != this.TileEntity.CenteredPitch)
				{
					this.PitchController.Pitch = Mathf.Lerp(this.PitchController.Pitch, this.TileEntity.CenteredPitch, Time.deltaTime * this.degreesPerSecond);
					this.PitchController.UpdatePitch();
				}
			}
			this.IsOn &= this.TileEntity.IsPowered;
			if (this.LightScript.bSwitchedOn != this.IsOn)
			{
				this.UpdateEmissionColor(this.IsOn);
				this.LightScript.bSwitchedOn = this.IsOn;
			}
			return;
		}
		if (this.UserAccessingId != -1)
		{
			this.YawController.Yaw = this.TileEntity.CenteredYaw;
			this.YawController.UpdateYaw();
			this.PitchController.Pitch = this.TileEntity.CenteredPitch;
			this.PitchController.UpdatePitch();
			return;
		}
		if (!this.TileEntity.IsPowered)
		{
			if (this.YawController.Yaw != this.TileEntity.CenteredYaw)
			{
				this.YawController.Yaw = this.TileEntity.CenteredYaw;
				this.YawController.SetYaw();
			}
			if (this.PitchController.Pitch != this.TileEntity.CenteredPitch)
			{
				this.PitchController.Pitch = this.TileEntity.CenteredPitch;
				this.PitchController.SetPitch();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEmissionColor(bool isPowered)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		if (componentsInChildren != null)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].material != componentsInChildren[i].sharedMaterial)
				{
					componentsInChildren[i].material = new Material(componentsInChildren[i].sharedMaterial);
				}
				if (isPowered)
				{
					componentsInChildren[i].material.SetColor("_EmissionColor", Color.white);
				}
				else
				{
					componentsInChildren[i].material.SetColor("_EmissionColor", Color.black);
				}
				componentsInChildren[i].sharedMaterial = componentsInChildren[i].material;
			}
		}
	}

	public void SetPitch(float pitch)
	{
		this.TileEntity.CenteredPitch = pitch;
	}

	public void SetYaw(float yaw)
	{
		this.TileEntity.CenteredYaw = yaw;
	}

	public float GetPitch()
	{
		return this.TileEntity.CenteredPitch;
	}

	public float GetYaw()
	{
		return this.TileEntity.CenteredYaw;
	}

	public Transform GetCameraTransform()
	{
		return null;
	}

	public void SetUserAccessing(int userAccessingId)
	{
		this.UserAccessingId = userAccessingId;
	}

	public void SetConeColor(Color _color)
	{
		if (this.ConeMaterial != null)
		{
			this.ConeMaterial.SetColor("_Color", _color);
		}
	}

	public Color GetOriginalConeColor()
	{
		return this.ConeColor;
	}

	public void SetConeActive(bool _active)
	{
		if (this.Cone != null)
		{
			this.Cone.gameObject.SetActive(_active);
		}
	}

	public bool GetConeActive()
	{
		return this.Cone != null && this.Cone.gameObject.activeSelf;
	}

	public bool HasCone()
	{
		return this.Cone != null;
	}

	public bool HasLaser()
	{
		return false;
	}

	public void SetLaserColor(Color _color)
	{
	}

	public Color GetOriginalLaserColor()
	{
		return Color.black;
	}

	public void SetLaserActive(bool _active)
	{
	}

	public bool GetLaserActive()
	{
		return false;
	}

	public AutoTurretYawLerp YawController;

	public AutoTurretPitchLerp PitchController;

	public LightLOD LightScript;

	public Transform Cone;

	public Material ConeMaterial;

	public Color ConeColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float degreesPerSecond = 11.25f;

	public bool IsOn;

	public TileEntityPowered TileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool initialized;
}
