using System;
using UnityEngine;

public class AutoTurretController : MonoBehaviour, IPowerSystemCamera
{
	public int UserAccessingId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = -1;

	public bool IsTurning
	{
		get
		{
			return this.YawController.IsTurning || this.PitchController.IsTurning;
		}
	}

	public TileEntityPoweredRangedTrap TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.FireController.TileEntity = value;
		}
	}

	public void OnDestroy()
	{
		this.Cleanup();
		if (this.ConeMaterial != null)
		{
			Object.Destroy(this.ConeMaterial);
		}
	}

	public void Init(DynamicProperties _properties)
	{
		this.IsOn = false;
		this.FireController.Cone = this.Cone;
		this.FireController.Laser = this.Laser;
		this.FireController.Init(_properties, this);
		this.PitchController.Init(_properties);
		this.YawController.Init(_properties);
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
		}
		WireManager.Instance.AddPulseObject(this.Cone.gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.FireController.IsOn && !this.IsOn)
		{
			this.FireController.OnPoweredOff();
		}
		this.FireController.IsOn = this.IsOn;
		if (this.IsOn)
		{
			this.YawController.UpdateYaw();
			this.PitchController.UpdatePitch();
		}
	}

	public void SetConeVisible(bool visible)
	{
		if (this.Cone != null)
		{
			this.Cone.gameObject.SetActive(visible);
		}
	}

	public void SetLaserVisible(bool visible)
	{
		if (this.Laser != null)
		{
			this.Laser.gameObject.SetActive(visible);
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
		return this.Cone;
	}

	public void SetUserAccessing(int userAccessingId)
	{
		this.UserAccessingId = userAccessingId;
	}

	public void Cleanup()
	{
		if (this.Cone != null && WireManager.HasInstance)
		{
			WireManager.Instance.RemovePulseObject(this.Cone.gameObject);
		}
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
		return this.Laser != null;
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
		if (this.Laser != null)
		{
			this.Laser.gameObject.SetActive(_active);
		}
	}

	public bool GetLaserActive()
	{
		return this.Laser != null && this.Laser.gameObject.activeSelf;
	}

	public AutoTurretYawLerp YawController;

	public AutoTurretPitchLerp PitchController;

	public AutoTurretFireController FireController;

	public Transform Laser;

	public Transform Cone;

	public Material ConeMaterial;

	public Color ConeColor;

	public bool IsOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public TileEntityPoweredRangedTrap tileEntity;
}
