using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class Vehicle
{
	public Vector2 CameraDistance
	{
		get
		{
			return this.cameraDistance;
		}
	}

	public Vector2 CameraTurnRate
	{
		get
		{
			return this.cameraTurnRate;
		}
	}

	public float BrakeTorque
	{
		get
		{
			return this.brakeTorque;
		}
	}

	public float SteerAngleMax
	{
		get
		{
			return this.steerAngleMax;
		}
	}

	public float SteerRate
	{
		get
		{
			return this.steerRate;
		}
	}

	public float SteerCenteringRate
	{
		get
		{
			return this.steerCenteringRate;
		}
	}

	public float TiltAngleMax
	{
		get
		{
			return this.tiltAngleMax;
		}
	}

	public float TiltThreshold
	{
		get
		{
			return this.tiltThreshold;
		}
	}

	public float TiltDampening
	{
		get
		{
			return this.tiltDampening;
		}
	}

	public float TiltDampenThreshold
	{
		get
		{
			return this.tiltDampenThreshold;
		}
	}

	public float TiltUpForce
	{
		get
		{
			return this.tiltUpForce;
		}
	}

	public Vector2 HopForce
	{
		get
		{
			return this.hopForce;
		}
	}

	public float UpAngleMax
	{
		get
		{
			return this.upAngleMax;
		}
	}

	public float UpForce
	{
		get
		{
			return this.upForce;
		}
	}

	public float UnstickForce
	{
		get
		{
			return this.unstickForce;
		}
	}

	public float MaxPossibleSpeed
	{
		get
		{
			return this.VelocityMaxTurboForward;
		}
	}

	public PlatformUserIdentifierAbs OwnerId
	{
		get
		{
			return this.m_ownerId;
		}
		set
		{
			this.m_ownerId = value;
			int num = -1;
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(this.m_ownerId);
			if (playerData != null)
			{
				num = playerData.EntityId;
			}
			this.entity.belongsPlayerId = num;
		}
	}

	public Vehicle(string _vehicleName, EntityVehicle _entity)
	{
		this.vehicleName = _vehicleName.ToLower();
		this.entity = _entity;
		this.SetupProperties();
		this.meshT = this.entity.ModelTransform.Find("Mesh");
		if (!this.meshT)
		{
			this.meshT = this.entity.ModelTransform;
		}
		this.vehicleParts = new List<VehiclePart>();
		this.OwnerId = null;
		this.AllowedUsers = new List<PlatformUserIdentifierAbs>();
		this.PasswordHash = string.Empty;
		this.MakeItemValue();
		this.CreateParts();
	}

	public void MakeItemValue()
	{
		string name = this.GetName();
		int num = 0;
		ItemClass itemClass = ItemClass.GetItemClass(name + "Placeable", true);
		if (itemClass != null)
		{
			num = itemClass.Id;
		}
		this.itemValue = new ItemValue(num, 1, 6, false, null, 1f);
		this.SetItemValue(this.itemValue);
	}

	public void SetItemValue(ItemValue _itemValue)
	{
		this.itemValue = _itemValue;
		if (this.itemValue.CosmeticMods.Length == 0)
		{
			this.itemValue.CosmeticMods = new ItemValue[1];
		}
		int num = this.itemValue.MaxUseTimes;
		if (this.itemValue.type == 0)
		{
			num = 5555;
		}
		int num2 = num - (int)this.itemValue.UseTimes;
		this.entity.Stats.Health.BaseMax = (float)num;
		this.entity.Stats.Health.OriginalMax = (float)num;
		this.entity.Health = num2;
		this.CalcEffects();
		this.SetFuelLevel((float)this.itemValue.Meta / 50f);
		this.CalcMods();
		this.SetColors();
		this.SetSeats();
	}

	public void SetItemValueMods(ItemValue _itemValue)
	{
		ItemValue itemValue = _itemValue.Clone();
		this.itemValue.Modifications = itemValue.Modifications;
		this.itemValue.CosmeticMods = itemValue.CosmeticMods;
		this.CalcEffects();
		this.CalcMods();
		this.SetColors();
		this.SetSeats();
	}

	public void SetColors()
	{
		Color white = Color.white;
		Vector3 vector = Block.StringToVector3(this.itemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255"));
		white.r = vector.x;
		white.g = vector.y;
		white.b = vector.z;
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			this.vehicleParts[i].SetColors(white);
		}
	}

	public void SetSeats()
	{
		int num = (int)EffectManager.GetValue(PassiveEffects.VehicleSeats, this.itemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		int num2 = 0;
		if (this.Properties != null)
		{
			int num3 = 0;
			int num4 = 0;
			DynamicProperties dynamicProperties;
			while (num4 < 99 && this.Properties.Classes.TryGetValue("seat" + num4.ToString(), out dynamicProperties))
			{
				if (dynamicProperties.GetString("mod").Length > 0)
				{
					if (num3 >= num)
					{
						break;
					}
					num3++;
				}
				num2++;
				num4++;
			}
		}
		this.entity.SetAttachMaxCount(num2);
	}

	public int GetSeatPose(int _seatIndex)
	{
		DynamicProperties dynamicProperties;
		if (this.Properties != null && this.Properties.Classes.TryGetValue("seat" + _seatIndex.ToString(), out dynamicProperties))
		{
			string @string = dynamicProperties.GetString("pose");
			if (@string.Length > 0)
			{
				return int.Parse(@string);
			}
		}
		return 0;
	}

	public ItemValue GetUpdatedItemValue()
	{
		this.itemValue.UseTimes = (float)((int)this.entity.Stats.Health.BaseMax - this.entity.Health);
		this.itemValue.Meta = (int)(this.GetFuelLevel() * 50f);
		return this.itemValue;
	}

	public void LoadItems(ItemStack[] _items)
	{
		this.SetItemValue(_items[0].itemValue);
	}

	public ItemStack[] GetItems()
	{
		return new ItemStack[]
		{
			new ItemStack(this.GetUpdatedItemValue(), 1)
		};
	}

	public void Update(float _deltaTime)
	{
		this.UpdateEffects(_deltaTime);
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			this.vehicleParts[i].Update(_deltaTime);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !this.HasStorage())
		{
			this.entity.DropBagServer();
		}
	}

	public void UpdateSimulation()
	{
		this.FireEvent(Vehicle.Event.SimulationUpdate);
	}

	public void FireEvent(Vehicle.Event _event)
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			this.vehicleParts[i].HandleEvent(_event, 0f);
		}
	}

	public void FireEvent(VehiclePart.Event _event, VehiclePart _fromPart, float _arg)
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			this.vehicleParts[i].HandleEvent(_event, _fromPart, _arg);
		}
	}

	public void SetupProperties()
	{
		if (!Vehicle.PropertyMap.TryGetValue(this.vehicleName, out this.Properties))
		{
			Log.Error("Vehicle properties for '{0}' not found!", new object[] { this.vehicleName });
		}
	}

	public DynamicProperties GetPropertiesForClass(string className)
	{
		if (this.Properties == null)
		{
			return null;
		}
		DynamicProperties dynamicProperties;
		this.Properties.Classes.TryGetValue(className, out dynamicProperties);
		return dynamicProperties;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseGeneralProperties(DynamicProperties properties)
	{
		properties.ParseVec("cameraDistance", ref this.cameraDistance);
		properties.ParseVec("cameraTurnRate", ref this.cameraTurnRate);
		properties.ParseFloat("steerAngleMax", ref this.steerAngleMax);
		properties.ParseFloat("steerRate", ref this.steerRate);
		properties.ParseFloat("steerCenteringRate", ref this.steerCenteringRate);
		properties.ParseFloat("tiltAngleMax", ref this.tiltAngleMax);
		properties.ParseFloat("tiltThreshold", ref this.tiltThreshold);
		properties.ParseFloat("tiltDampening", ref this.tiltDampening);
		properties.ParseFloat("tiltDampenThreshold", ref this.tiltDampenThreshold);
		properties.ParseFloat("tiltUpForce", ref this.tiltUpForce);
		properties.ParseFloat("upAngleMax", ref this.upAngleMax);
		properties.ParseFloat("upForce", ref this.upForce);
		properties.ParseVec("motorTorque_turbo", ref this.MotorTorqueForward, ref this.MotorTorqueBackward, ref this.MotorTorqueTurboForward, ref this.MotorTorqueTurboBackward);
		properties.ParseVec("velocityMax_turbo", ref this.VelocityMaxForward, ref this.VelocityMaxBackward, ref this.VelocityMaxTurboForward, ref this.VelocityMaxTurboBackward);
		properties.ParseFloat("brakeTorque", ref this.brakeTorque);
		properties.ParseVec("hopForce", ref this.hopForce);
		properties.ParseFloat("unstickForce", ref this.unstickForce);
		properties.ParseVec("airDrag_velScale_angVelScale", ref this.AirDragVelScale, ref this.AirDragAngVelScale);
		properties.ParseVec("waterDrag_y_velScale_velMaxScale", ref this.WaterDragY, ref this.WaterDragVelScale, ref this.WaterDragVelMaxScale);
		properties.ParseVec("waterLift_y_depth_force", ref this.WaterLiftY, ref this.WaterLiftDepth, ref this.WaterLiftForce);
		properties.ParseFloat("wheelPtlScale", ref this.WheelPtlScale);
		properties.ParseString("recipeName", ref this.RecipeName);
	}

	public void OnXMLChanged()
	{
		this.SetupProperties();
		DynamicProperties properties = this.Properties;
		if (properties == null)
		{
			return;
		}
		this.ParseGeneralProperties(properties);
		foreach (KeyValuePair<string, DynamicProperties> keyValuePair in properties.Classes)
		{
			VehiclePart vehiclePart = this.FindPart(keyValuePair.Key);
			if (vehiclePart != null)
			{
				DynamicProperties value = keyValuePair.Value;
				vehiclePart.SetProperties(value);
			}
		}
	}

	public void TriggerUpdateEffects()
	{
		this.effectUpdateDelay = 0f;
	}

	public void UpdateEffects(float _deltaTime)
	{
		this.effectUpdateDelay -= _deltaTime;
		if (this.effectUpdateDelay > 0f)
		{
			return;
		}
		this.effectUpdateDelay = 2f;
		this.GetUpdatedItemValue();
		this.CalcEffects();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcEffects()
	{
		EntityAlive entityAlive = this.entity.AttachedMainEntity as EntityAlive;
		FastTags<TagGroup.Global> entityTags = this.entity.EntityTags;
		this.EffectEntityDamagePer = EffectManager.GetValue(PassiveEffects.VehicleEntityDamage, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false) * EntityVehicle.VehicleEntityDamageModifier;
		this.EffectBlockDamagePer = EffectManager.GetValue(PassiveEffects.VehicleBlockDamage, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false) * EntityVehicle.VehicleBlockDamageModifier;
		this.EffectSelfDamagePer = EffectManager.GetValue(PassiveEffects.VehicleSelfDamage, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false) * EntityVehicle.VehicleSelfDamageModifier;
		this.EffectStrongSelfDamagePer = EffectManager.GetValue(PassiveEffects.VehicleStrongSelfDamage, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false) * EntityVehicle.VehicleSelfDamageModifier;
		this.EffectLightIntensity = EffectManager.GetValue(PassiveEffects.LightIntensity, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false);
		this.EffectFuelMaxPer = EffectManager.GetValue(PassiveEffects.VehicleFuelMaxPer, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false);
		this.EffectFuelUsePer = EffectManager.GetValue(PassiveEffects.VehicleFuelUsePer, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false) * EntityVehicle.VehicleFuelUsageModifier;
		this.EffectMotorTorquePer = EffectManager.GetValue(PassiveEffects.VehicleMotorTorquePer, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false);
		this.EffectVelocityMaxPer = EffectManager.GetValue(PassiveEffects.VehicleVelocityMaxPer, this.itemValue, 1f, entityAlive, null, entityTags, true, true, true, true, true, 1, true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcMods()
	{
		int num = 0;
		this.ModTags = FastTags<TagGroup.Global>.none;
		ItemValue[] modifications = this.itemValue.Modifications;
		if (modifications != null)
		{
			foreach (ItemValue itemValue in modifications)
			{
				if (itemValue != null)
				{
					ItemClassModifier itemClassModifier = itemValue.ItemClass as ItemClassModifier;
					if (itemClassModifier != null)
					{
						this.ModTags |= itemClassModifier.ModifierTags;
						if (itemClassModifier.ModifierTags.Test_AnySet(EntityVehicle.StorageModifierTags))
						{
							num++;
						}
					}
				}
			}
		}
		for (int j = 0; j < this.vehicleParts.Count; j++)
		{
			this.vehicleParts[j].SetMods();
		}
		this.entity.UpdateStorageModCount(num);
		this.entity.UpdateContainerSize(false);
	}

	public void CreateParts()
	{
		DynamicProperties properties = this.Properties;
		if (properties == null)
		{
			return;
		}
		this.ParseGeneralProperties(properties);
		foreach (KeyValuePair<string, DynamicProperties> keyValuePair in properties.Classes)
		{
			DynamicProperties value = keyValuePair.Value;
			string @string = value.GetString("class");
			if (@string.Length > 0)
			{
				try
				{
					VehiclePart vehiclePart = (VehiclePart)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("VP", @string));
					vehiclePart.SetVehicle(this);
					vehiclePart.SetTag(keyValuePair.Key);
					vehiclePart.SetProperties(value);
					this.vehicleParts.Add(vehiclePart);
				}
				catch (Exception ex)
				{
					Log.Out(ex.Message);
					Log.Out(ex.StackTrace);
					throw new Exception("No vehicle part class 'VP" + @string + "' found!");
				}
			}
		}
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			this.vehicleParts[i].InitPrefabConnections();
		}
	}

	public VehiclePart FindPart(string _tag)
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			if (this.vehicleParts[i].tag == _tag)
			{
				return this.vehicleParts[i];
			}
		}
		return null;
	}

	public string GetPartProperty(string _tag, string _propertyName)
	{
		VehiclePart vehiclePart = this.FindPart(_tag);
		if (vehiclePart == null)
		{
			return string.Empty;
		}
		return vehiclePart.GetProperty(_propertyName);
	}

	public List<VehiclePart> GetParts()
	{
		return this.vehicleParts;
	}

	public static void SetupPreview(Transform rootT)
	{
		Transform transform = rootT.Find("Physics");
		if (transform)
		{
			Object.Destroy(transform.gameObject);
		}
		ParticleSystem[] componentsInChildren = rootT.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(false);
		}
	}

	public Transform GetMeshTransform()
	{
		return this.meshT;
	}

	public string GetName()
	{
		return this.vehicleName;
	}

	public static void Cleanup()
	{
		Vehicle.PropertyMap = new Dictionary<string, DynamicProperties>();
	}

	public string GetFuelItem()
	{
		if (this.HasEnginePart())
		{
			return "ammoGasCan";
		}
		return "";
	}

	public float GetFuelPercent()
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			if (this.vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				float num = ((VPFuelTank)this.vehicleParts[i]).GetFuelLevelPercent();
				if (num > 0.993f)
				{
					num = 1f;
				}
				return num;
			}
		}
		return 0f;
	}

	public float GetFuelLevel()
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			if (this.vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				return ((VPFuelTank)this.vehicleParts[i]).GetFuelLevel();
			}
		}
		return 0f;
	}

	public float GetMaxFuelLevel()
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			if (this.vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				return ((VPFuelTank)this.vehicleParts[i]).GetMaxFuelLevel();
			}
		}
		return 0f;
	}

	public void SetFuelLevel(float _fuelLevel)
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			if (this.vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				((VPFuelTank)this.vehicleParts[i]).SetFuelLevel(_fuelLevel);
				return;
			}
		}
	}

	public float GetBatteryLevel()
	{
		return 0f;
	}

	public void SetBatteryLevel(float _amount)
	{
	}

	public void AddFuel(float _fuelLevel)
	{
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			if (this.vehicleParts[i].GetType() == typeof(VPFuelTank))
			{
				((VPFuelTank)this.vehicleParts[i]).AddFuel(_fuelLevel);
				return;
			}
		}
	}

	public int GetRepairAmountNeeded()
	{
		return this.GetMaxHealth() - this.entity.Health;
	}

	public void RepairParts(int _add, float _percent)
	{
		int num = _add + (int)((float)this.GetMaxHealth() * _percent);
		num = Utils.FastMin(num, this.GetRepairAmountNeeded());
		this.entity.Health += num;
	}

	public bool IsDriveable()
	{
		return this.HasSteering();
	}

	public bool HasEnginePart()
	{
		return this.FindPart("engine") != null;
	}

	public float GetEngineQualityPercent()
	{
		return 0f;
	}

	public bool HasStorage()
	{
		return this.FindPart("storage") != null;
	}

	public bool HasSteering()
	{
		return true;
	}

	public bool IsSteeringBroken()
	{
		return !this.HasSteering() || this.FindPart("handlebars").IsBroken();
	}

	public bool HasLock()
	{
		return this.FindPart("lock") != null;
	}

	public bool IsLockBroken()
	{
		return this.FindPart("lock").GetHealthPercentage() == 0f;
	}

	public string GetHornSoundName()
	{
		return this.Properties.GetString("hornSound");
	}

	public bool HasHorn()
	{
		return this.GetHornSoundName().Length > 0;
	}

	public List<IKController.Target> GetIKTargets(int slot)
	{
		List<IKController.Target> list = new List<IKController.Target>();
		if (slot == 0)
		{
			VehiclePart vehiclePart = this.FindPart("handlebars");
			if (vehiclePart != null)
			{
				list.AddRange(vehiclePart.ikTargets);
			}
			VehiclePart vehiclePart2 = this.FindPart("pedals");
			if (vehiclePart2 != null)
			{
				list.AddRange(vehiclePart2.ikTargets);
			}
		}
		VehiclePart vehiclePart3 = this.FindPart("seat" + slot.ToString());
		if (vehiclePart3 != null && vehiclePart3.ikTargets != null)
		{
			list.AddRange(vehiclePart3.ikTargets);
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list;
	}

	public List<string> GetParticleTransformPaths()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < this.vehicleParts.Count; i++)
		{
			string property = this.vehicleParts[i].GetProperty("particle_transform");
			if (property != string.Empty)
			{
				list.Add(property);
			}
		}
		return list;
	}

	public int GetVehicleQuality()
	{
		return (int)this.itemValue.Quality;
	}

	public int GetHealth()
	{
		int health = this.entity.Health;
		if (health <= 1)
		{
			return 0;
		}
		return health;
	}

	public int GetMaxHealth()
	{
		return this.entity.GetMaxHealth();
	}

	public float GetHealthPercent()
	{
		return (float)this.GetHealth() / (float)this.entity.GetMaxHealth();
	}

	public float GetPlayerDamagePercent()
	{
		return 0.1f;
	}

	public float GetNoise()
	{
		return 0.5f;
	}

	public void SetLocked(bool isLocked, EntityPlayerLocal player)
	{
		if (player == null)
		{
			return;
		}
		if (isLocked)
		{
			this.entity.SetOwner(PlatformManager.InternalLocalUserIdentifier);
			this.entity.isLocked = true;
			this.PasswordHash = string.Empty;
			return;
		}
		this.entity.isLocked = false;
		this.PasswordHash = string.Empty;
	}

	public static Dictionary<string, DynamicProperties> PropertyMap;

	public DynamicProperties Properties;

	public ItemValue itemValue;

	public List<PlatformUserIdentifierAbs> AllowedUsers;

	public string PasswordHash = string.Empty;

	public EntityVehicle entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_ownerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string vehicleName;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<VehiclePart> vehicleParts;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform meshT;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 cameraDistance = new Vector2(1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 cameraTurnRate = new Vector2(1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public float upAngleMax = 70f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float upForce = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerAngleMax = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerRate = 130f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerCenteringRate = 90f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltAngleMax = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltThreshold = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltDampening = 0.22f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltDampenThreshold = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tiltUpForce = 5f;

	public float MotorTorqueForward = 300f;

	public float MotorTorqueBackward = 300f;

	public float MotorTorqueTurboForward;

	public float MotorTorqueTurboBackward;

	public float VelocityMaxForward = 10f;

	public float VelocityMaxBackward = 10f;

	public float VelocityMaxTurboForward = 10f;

	public float VelocityMaxTurboBackward = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float brakeTorque = 4000f;

	public bool CanTurbo = true;

	public bool IsTurbo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 hopForce;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unstickForce = 1f;

	public float AirDragVelScale = 0.997f;

	public float AirDragAngVelScale = 1f;

	public float WaterDragY;

	public float WaterDragVelScale = 1f;

	public float WaterDragVelMaxScale = 1f;

	public float WaterLiftY;

	public float WaterLiftDepth;

	public float WaterLiftForce;

	public float WheelPtlScale;

	public float CurrentForwardVelocity;

	public bool CurrentIsAccel;

	public bool CurrentIsBreak;

	public float CurrentMotorTorquePercent;

	public float CurrentSteeringPercent;

	public Vector3 CurrentVelocity;

	public string RecipeName;

	public Material mainEmissiveMat;

	public float EffectEntityDamagePer;

	public float EffectBlockDamagePer;

	public float EffectSelfDamagePer;

	public float EffectStrongSelfDamagePer;

	public float EffectLightIntensity;

	public float EffectFuelMaxPer = 1f;

	public float EffectFuelUsePer;

	public float EffectMotorTorquePer;

	public float EffectVelocityMaxPer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float effectUpdateDelay;

	public FastTags<TagGroup.Global> ModTags;

	public enum Event
	{
		Start,
		Started,
		Stop,
		Stopped,
		SimulationUpdate,
		HealthChanged
	}
}
