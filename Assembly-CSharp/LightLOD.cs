using System;
using Audio;
using UnityEngine;

public class LightLOD : MonoBehaviour
{
	public Light GetLight()
	{
		if (!this.myLight)
		{
			if (!this.otherLight)
			{
				this.myLight = base.GetComponent<Light>();
			}
			else
			{
				this.myLight = this.otherLight;
			}
			if (this.myLight)
			{
				this.shadowStateMaster = this.myLight.shadows;
				this.shadowStrengthMaster = this.myLight.shadowStrength;
			}
		}
		return this.myLight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		if (this.hasInitialized)
		{
			return;
		}
		this.hasInitialized = true;
		this.selfT = base.transform;
		Light light = this.GetLight();
		if (light)
		{
			if (GameManager.IsDedicatedServer && this.registeredRange == 0f)
			{
				this.SetRange(light.range);
			}
			this.lightIntensityMaster = light.intensity;
			this.lightRangeMaster = light.range;
			this.CalcViewDistance();
			if (this.lightStateType != LightStateType.Static)
			{
				Type type = Type.GetType(this.lightStateType.ToStringCached<LightStateType>());
				this.lightState = base.gameObject.GetComponent(type) as LightState;
				if (!this.lightState)
				{
					this.lightState = (LightState)base.gameObject.AddComponent(type);
				}
			}
		}
		if (this.RefFlare)
		{
			this.lensFlare = this.RefFlare.GetComponent<LensFlare>();
		}
		else
		{
			this.lensFlare = base.GetComponent<LensFlare>();
		}
		if (this.RefIlluminatedMaterials != null)
		{
			this.maxLightPowerValues = new float[this.RefIlluminatedMaterials.Length];
			this.cachedMaterials = new Material[this.RefIlluminatedMaterials.Length][];
			for (int i = 0; i < this.RefIlluminatedMaterials.Length; i++)
			{
				Transform transform = this.RefIlluminatedMaterials[i];
				if (transform)
				{
					Renderer component = transform.GetComponent<Renderer>();
					if (component != null)
					{
						Material material = component.material;
						if (material != null)
						{
							if (material.HasProperty("_LightPower"))
							{
								this.maxLightPowerValues[i] = Utils.FastMax(material.GetFloat("_LightPower"), 1f);
							}
							else
							{
								this.maxLightPowerValues[i] = 1f;
							}
						}
						this.cachedMaterials[i] = component.materials;
					}
				}
			}
		}
		if (this.lightStateStart != LightStateType.Static)
		{
			this.LightStateType = this.lightStateStart;
		}
		if (!GameManager.IsDedicatedServer)
		{
			this.audioPlayer = base.GetComponent<AudioPlayer>();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		this.Init();
		if (GameManager.Instance.World == null)
		{
			return;
		}
		this.lightRange = this.lightRangeMaster;
		this.lightIntensity = this.lightIntensityMaster;
		if (this.myLight)
		{
			this.myLight.enabled = false;
		}
		this.isInitialBlockDone = false;
		this.parentT = this.selfT.parent;
		LightManager.LightChanged(this.selfT.position + Origin.position);
		if (GameLightManager.Instance != null)
		{
			GameLightManager.Instance.AddLight(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckInitialBlock()
	{
		if (this.isInitialBlockDone || !this.parentT || !this.selfT)
		{
			return;
		}
		if (this.bed == null)
		{
			this.isInitialBlockDone = true;
			if (!this.isHeld && this.registeredRange == 0f && this.myLight)
			{
				this.SetRange(this.lightRange);
			}
			return;
		}
		World world = GameManager.Instance.World;
		BlockValue blockValue = world.GetBlock(this.bed.pos);
		Block block = blockValue.Block;
		if (block.isMultiBlock && blockValue.ischild)
		{
			Vector3i parentPos = block.multiBlockPos.GetParentPos(this.bed.pos, blockValue);
			blockValue = world.GetBlock(parentPos);
		}
		if (this.bToggleable)
		{
			this.SwitchOnOff((blockValue.meta & 2) > 0, false);
		}
		if (this.registeredRange == 0f && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.myLight)
		{
			this.SetRange(this.lightRange);
		}
		this.isInitialBlockDone = true;
	}

	public LightStateType LightStateType
	{
		get
		{
			return this.lightStateType;
		}
		set
		{
			if (this.lightStateType == value)
			{
				return;
			}
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			if (this.lightState)
			{
				Object.Destroy(this.lightState);
			}
			this.lightStateType = value;
			if (this.lightStateType != LightStateType.Static)
			{
				Type type = Type.GetType(this.lightStateType.ToStringCached<LightStateType>());
				this.lightState = (LightState)base.gameObject.AddComponent(type);
			}
		}
	}

	public float MaxIntensity
	{
		get
		{
			return this.lightIntensity;
		}
		set
		{
			this.lightIntensity = value;
			this.lightIntensityMaster = value;
			LightLOD.MaxIntensityEvent maxIntensityChanged = this.MaxIntensityChanged;
			if (maxIntensityChanged == null)
			{
				return;
			}
			maxIntensityChanged();
		}
	}

	public float LightAngle
	{
		set
		{
			if (this.myLight.type == null)
			{
				if (value > 160f)
				{
					value = 160f;
				}
				this.myLight.spotAngle = value;
			}
		}
	}

	public void SetEmissiveColor(bool _on)
	{
		this.SetEmissiveColor(_on ? 1f : 0f);
	}

	public void SetEmissiveColor(float _newV)
	{
		if (_newV <= 0f)
		{
			this.SetEmissiveColor(LightLOD.EmissiveColorOff);
			return;
		}
		Color color;
		if (!this.EmissiveFromLightColorOn)
		{
			color = this.EmissiveColor;
		}
		else
		{
			Light light = this.GetLight();
			if (!light)
			{
				return;
			}
			color = light.color;
		}
		if (_newV >= 1f)
		{
			this.SetEmissiveColor(color);
		}
		float num;
		float num2;
		float num3;
		Color.RGBToHSV(color, ref num, ref num2, ref num3);
		Color color2 = Color.HSVToRGB(num, num2, _newV);
		this.SetEmissiveColor(color2);
	}

	public void SetEmissiveColor(Color _color)
	{
		if (this.RefIlluminatedMaterials == null)
		{
			return;
		}
		if (this.lastEmissiveColor != null && _color == this.lastEmissiveColor.Value)
		{
			return;
		}
		this.lastEmissiveColor = new Color?(_color);
		bool flag = _color != LightLOD.EmissiveColorOff;
		for (int i = 0; i < this.cachedMaterials.Length; i++)
		{
			Material[] array = this.cachedMaterials[i];
			if (array != null)
			{
				foreach (Material material in array)
				{
					if (material)
					{
						material.SetColor(LightLOD.EmissionColorId, _color);
						if (flag)
						{
							material.EnableKeyword("_EMISSION");
						}
						else
						{
							material.DisableKeyword("_EMISSION");
						}
					}
				}
			}
		}
	}

	public void SetRange(float _range)
	{
		Light light = this.GetLight();
		this.lightRange = _range;
		this.lightRangeMaster = _range;
		light.range = _range;
		this.CalcViewDistance();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && base.enabled && light.enabled)
		{
			this.UnregisterFromLightManager();
			this.registeredPos = light.transform.position + Origin.position;
			this.registeredRange = LightManager.RegisterLight(light);
		}
	}

	public void TestRegistration()
	{
		if (this.myLight)
		{
			this.SetRange(this.lightRange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcViewDistance()
	{
		this.lightViewDistance = Utils.FastMax(this.MaxDistance, this.lightRangeMaster * 1.5f);
	}

	public void SetBlockEntityData(BlockEntityData _bed)
	{
		this.bed = _bed;
	}

	public void SwitchOnOff(bool _isOn, bool _ignoreToggle = false)
	{
		if ((_ignoreToggle || this.bToggleable) && _isOn != this.bSwitchedOn)
		{
			this.bSwitchedOn = _isOn;
		}
		if (this.bSwitchedOn)
		{
			base.enabled = true;
		}
	}

	public void SetCulled(bool _culled)
	{
		this.isCulled = _culled;
		if (this.myLight)
		{
			this.myLight.enabled = !_culled;
		}
	}

	public void OnDisable()
	{
		if (!string.IsNullOrEmpty(this.cyclingAudioClipPath) && this.cyclingAudioHandle != null)
		{
			this.cyclingAudioHandle.SetVolume(0f);
		}
		if (GameManager.Instance.World != null)
		{
			LightManager.LightChanged(this.selfT.position + Origin.position);
			if (GameLightManager.Instance != null)
			{
				GameLightManager.Instance.RemoveLight(this);
			}
		}
		this.UnregisterFromLightManager();
		this.parentT = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnregisterFromLightManager()
	{
		if (this.registeredRange > 0f)
		{
			LightManager.UnRegisterLight(this.registeredPos, this.registeredRange);
			this.registeredRange = 0f;
		}
	}

	public void FrameUpdate(Vector3 _cameraPos)
	{
		bool flag = this.lightStateType > LightStateType.Static;
		this.priority = (float)(flag ? 1 : 0);
		if (this.bRenderingOff || !this.myLight)
		{
			return;
		}
		this.CheckInitialBlock();
		bool flag2 = this.bSwitchedOn && (!flag || this.lightState.CanBeOn);
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		if (!this.isCulled)
		{
			num = (this.selfT.position - _cameraPos).sqrMagnitude * this.DistanceScale;
			num2 = Mathf.Sqrt(num) - this.lightRange;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
		}
		float num3 = this.lightViewDistance;
		if (this.bPlayerPlacedLight)
		{
			num3 *= 1.2f;
		}
		if (LightLOD.DebugViewDistance > 0f)
		{
			num3 = Utils.FastMax(LightLOD.DebugViewDistance, this.lightRange + 0.01f);
		}
		float num4 = num3 * num3;
		float num5 = num / num4;
		float num6 = num3 - this.lightRange;
		if (flag2)
		{
			flag2 &= num2 < num6;
			if (this.lightStateType != LightStateType.Blinking)
			{
				flag2 &= num5 < (flag ? this.lightState.LODThreshold : 1f);
			}
		}
		if (flag2 && !this.bWorksUnderwater)
		{
			bool flag3 = false;
			if (this.waterLevelRecheckTime != 3.4028235E+38f)
			{
				if (Time.time >= this.waterLevelRecheckTime)
				{
					this.waterLevelRecheckTime = float.MaxValue;
					if (this.WaterLevelDirty)
					{
						this.waterLevelRecheckTime = Time.time + 0.5f;
					}
					this.WaterLevelDirty = false;
					flag3 = this.bed != null;
				}
			}
			else if (this.WaterLevelDirty)
			{
				this.WaterLevelDirty = false;
				flag3 = this.bed != null;
				if (flag3)
				{
					this.waterLevelRecheckTime = Time.time + 0.5f;
				}
			}
			if (flag3)
			{
				World world = GameManager.Instance.World;
				bool flag4 = world.IsWater(this.bed.pos.x, this.bed.pos.y + 1, this.bed.pos.z);
				if (!flag4 && world.IsWater(this.bed.pos))
				{
					flag4 = 0.6f + (float)this.bed.pos.y > base.transform.position.y;
				}
				if (this.isUnderwater != flag4)
				{
					this.isUnderwater = flag4;
				}
			}
			flag2 &= !this.isUnderwater;
		}
		if (flag2)
		{
			if (this.bPlayerPlacedLight)
			{
				if (num5 >= 0.64000005f)
				{
					this.myLight.shadows = 0;
				}
				else if (num5 >= 0.0625f)
				{
					if (this.shadowStateMaster == 2)
					{
						this.myLight.shadows = 1;
					}
					this.myLight.shadowStrength = (1f - Utils.FastClamp01((num5 - 0.36f) / 0.28000003f)) * this.shadowStrengthMaster;
				}
				else
				{
					this.myLight.shadows = this.shadowStateMaster;
					this.myLight.shadowStrength = this.shadowStrengthMaster;
				}
			}
			float num7 = num2 / num6;
			float num8 = 1f - num7 * num7;
			this.myLight.intensity = this.lightIntensity * num8 * (flag ? this.lightState.Intensity : 1f);
			this.myLight.range = this.lightRange * 0.5f + this.lightRange * 0.5f * num8;
			this.SetEmissiveColor(flag ? this.lightState.Emissive : 1f);
			if (num8 < 0.9f && this.myLight.enabled)
			{
				this.priority = 1f;
			}
			if (flag && this.lightState.AudioFrequency > 0f && !string.IsNullOrEmpty(this.cyclingAudioClipPath))
			{
				float num9 = this.cyclingAudioPlayEveryNCycles * (1f / this.lightState.AudioFrequency);
				float num10 = (Time.time + this.cyclingAudioOffset) % num9;
				if (num10 < this.previousCycleTime)
				{
					this.cyclingAudioHandle = Manager.Play(this.selfT.position + Origin.position, this.cyclingAudioClipPath, -1, true, 1f);
				}
				this.previousCycleTime = num10;
			}
			if (this.bed != null)
			{
				Block block = this.bed.blockValue.Block;
				if (block.RadiusEffects != null)
				{
					GameManager.Instance.World.GetPrimaryPlayer().BlockRadiusEffectsApply(block, this.selfT.position + Origin.position);
				}
			}
		}
		else
		{
			this.SetEmissiveColor(false);
		}
		this.myLight.enabled = flag2;
		if (this.LitRootObject)
		{
			this.LitRootObject.SetActive(flag2);
		}
		if (this.audioPlayer)
		{
			this.audioPlayer.enabled = flag2;
		}
		if (this.lensFlare)
		{
			if (flag2 && num < 10f * num4)
			{
				float num11 = (1f - num / (num4 * 10f)) * this.lightIntensity * 0.33f * this.FlareBrightnessFactor;
				if (num11 > 1f)
				{
					num11 = 1f;
				}
				if (this.lightRange < 4f)
				{
					num11 *= this.lightRange * 0.25f;
				}
				this.lensFlare.brightness = num11;
				this.lensFlare.color = this.myLight.color;
				this.lensFlare.enabled = true;
			}
			else
			{
				this.lensFlare.enabled = false;
			}
		}
		base.enabled = this.bSwitchedOn;
	}

	public void SetRenderingOn()
	{
		this.bRenderingOff = false;
	}

	public void SetRenderingOff()
	{
		this.bRenderingOff = true;
	}

	public GameObject LitRootObject;

	public Transform[] RefIlluminatedMaterials;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material[][] cachedMaterials;

	public Transform RefFlare;

	public float MaxDistance = 30f;

	public float DistanceScale = 1f;

	public float FlareBrightnessFactor = 1f;

	public bool bPlayerPlacedLight;

	public bool bSwitchedOn;

	public bool bToggleable = true;

	public bool bWorksUnderwater = true;

	public bool isHeld;

	public Light otherLight;

	public LightStateType lightStateStart;

	public string cyclingAudioClipPath;

	public float cyclingAudioOffset;

	public float cyclingAudioPlayEveryNCycles = 1f;

	public bool EmissiveFromLightColorOn;

	public Color EmissiveColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly Color EmissiveColorOff = Color.black;

	public float StateRate = 1f;

	public float FluxDelay = 1f;

	public static float DebugViewDistance;

	public LightLOD.MaxIntensityEvent MaxIntensityChanged;

	public bool WaterLevelDirty;

	public float priority;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCulled;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform selfT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform parentT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BlockEntityData bed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInitialBlockDone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light myLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightIntensityMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightRangeMaster;

	[NonSerialized]
	public float lightRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightViewDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bRenderingOff;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LensFlare lensFlare;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] maxLightPowerValues;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 registeredPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float registeredRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LightStateType lightStateType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LightState lightState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LightShadows shadowStateMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float shadowStrengthMaster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioPlayer audioPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Handle cyclingAudioHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isUnderwater;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float waterLevelRecheckTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color? lastEmissiveColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float previousCycleTime;

	public delegate void MaxIntensityEvent();
}
