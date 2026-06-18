using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VehiclePart
{
	public virtual void InitPrefabConnections()
	{
	}

	public void InitIKTarget(AvatarIKGoal ikGoal, Transform parentT)
	{
		string text = IKController.IKNames[ikGoal];
		string property = this.GetProperty(text + "Position");
		if (string.IsNullOrEmpty(property))
		{
			return;
		}
		Vector3 vector = StringParsers.ParseVector3(property, 0, -1);
		Vector3 vector2 = StringParsers.ParseVector3(this.GetProperty(text + "Rotation"), 0, -1);
		IKController.Target target;
		target.avatarGoal = ikGoal;
		target.transform = null;
		if (parentT)
		{
			Transform transform = new GameObject(text).transform;
			target.transform = transform;
			transform.SetParent(parentT, false);
			transform.localPosition = vector;
			transform.localEulerAngles = vector2;
			target.position = Vector3.zero;
			target.rotation = Vector3.zero;
		}
		else
		{
			target.position = vector;
			target.rotation = vector2;
		}
		if (this.ikTargets == null)
		{
			this.ikTargets = new List<IKController.Target>();
		}
		this.ikTargets.Add(target);
	}

	public virtual void SetProperties(DynamicProperties _properties)
	{
		if (_properties == null)
		{
			Log.Warning("VehiclePart SetProperties null");
		}
		this.properties = _properties;
	}

	public string GetProperty(string _key)
	{
		if (this.properties == null)
		{
			Log.Warning("VehiclePart GetProperty null");
			return string.Empty;
		}
		return this.properties.GetString(_key);
	}

	public virtual void SetMods()
	{
		this.modInstalled = false;
		string property = this.GetProperty("mod");
		if (property.Length > 0)
		{
			int bit = FastTags<TagGroup.Global>.GetBit(property);
			this.modInstalled = this.vehicle.ModTags.Test_Bit(bit);
			Transform transform = this.GetTransform("modT");
			if (transform)
			{
				string property2 = this.GetProperty("modRot");
				if (property2.Length > 0)
				{
					Vector3 vector = Vector3.zero;
					if (this.modInstalled)
					{
						vector = StringParsers.ParseVector3(property2, 0, -1);
					}
					transform.localEulerAngles = vector;
				}
				else
				{
					transform.gameObject.SetActive(this.modInstalled);
				}
			}
			this.SetTransformActive("modHideT", !this.modInstalled);
			this.SetPhysicsTransformActive("modRBT", this.modInstalled);
		}
	}

	public void SetVehicle(Vehicle _v)
	{
		this.vehicle = _v;
	}

	public void SetTag(string _tag)
	{
		this.tag = _tag;
	}

	public Transform GetTransform()
	{
		return this.GetTransform("transform");
	}

	public Transform GetTransform(string _property)
	{
		Transform meshTransform = this.vehicle.GetMeshTransform();
		if (meshTransform)
		{
			string property = this.GetProperty(_property);
			if (property.Length > 0)
			{
				return meshTransform.Find(property);
			}
		}
		return null;
	}

	public void SetTransformActive(string _property, bool _active)
	{
		Transform transform = this.vehicle.GetMeshTransform();
		if (transform)
		{
			string property = this.GetProperty(_property);
			if (property.Length > 0)
			{
				transform = transform.Find(property);
				if (transform)
				{
					transform.gameObject.SetActive(_active);
					return;
				}
				Log.Warning("Vehicle SetTransformActive missing {0}", new object[] { property });
			}
		}
	}

	public void SetPhysicsTransformActive(string _property, bool _active)
	{
		Transform transform = this.vehicle.entity.PhysicsTransform;
		if (transform)
		{
			string property = this.GetProperty(_property);
			if (property.Length > 0)
			{
				transform = transform.Find(property);
				if (transform)
				{
					transform.gameObject.SetActive(_active);
					return;
				}
				Log.Warning("Vehicle SetPhysicsTransformActive missing {0}", new object[] { property });
			}
		}
	}

	public virtual bool IsBroken()
	{
		return this.vehicle.GetHealth() <= 0;
	}

	public float GetHealthPercentage()
	{
		return this.vehicle.GetHealthPercent();
	}

	public bool IsRequired()
	{
		return false;
	}

	public void SetColors(Color _color)
	{
		Transform transform = this.GetTransform("paint");
		if (transform)
		{
			transform.GetComponentsInChildren<Renderer>(true, VehiclePart.renderers);
			if (VehiclePart.renderers.Count > 0)
			{
				Material material = VehiclePart.renderers[0].material;
				this.vehicle.mainEmissiveMat = material;
				material.color = _color;
				for (int i = 1; i < VehiclePart.renderers.Count; i++)
				{
					Renderer renderer = VehiclePart.renderers[i];
					if (renderer.CompareTag("LOD"))
					{
						renderer.GetSharedMaterials(VehiclePart.materials);
						if (VehiclePart.materials.Count == 1)
						{
							renderer.material = material;
						}
						VehiclePart.materials.Clear();
					}
				}
				VehiclePart.renderers.Clear();
			}
		}
	}

	public virtual void Update(float _dt)
	{
	}

	public virtual void HandleEvent(Vehicle.Event _event, float _arg)
	{
	}

	public virtual void HandleEvent(VehiclePart.Event _event, VehiclePart _fromPart, float arg)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicProperties properties;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vehicle vehicle;

	public string tag;

	public bool modInstalled;

	public List<IKController.Target> ikTargets;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Renderer> renderers = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Material> materials = new List<Material>();

	public enum Event
	{
		Broken,
		LightsOn,
		FuelEmpty,
		FuelRemove
	}
}
