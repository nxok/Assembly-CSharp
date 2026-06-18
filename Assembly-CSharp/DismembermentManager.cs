using System;
using System.Collections.Generic;
using UnityEngine;

public class DismembermentManager
{
	public static string GetAssetBundlePath(string prefabPath)
	{
		return "@:Entities/Zombies/" + prefabPath + ".prefab";
	}

	public static bool IsDefaultGib(string matName)
	{
		for (int i = 0; i < DismembermentManager.DefaultBundleGibs.Length; i++)
		{
			if (DismembermentManager.DefaultBundleGibs[i] == matName)
			{
				return true;
			}
		}
		return false;
	}

	public Material GibCapsMaterial
	{
		get
		{
			if (!this.zombieGibCapsMaterial)
			{
				Material material = DataLoader.LoadAsset<Material>("@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps.mat", false);
				this.zombieGibCapsMaterial = Object.Instantiate<Material>(material);
				this.zombieGibCapsMaterial.name = material.name + "(global)";
				if (DismembermentManager.DebugLogEnabled)
				{
					Log.Out("{0} material: {1}", new object[]
					{
						this.zombieGibCapsMaterial ? "load" : "load failed",
						"@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps.mat"
					});
				}
			}
			return this.zombieGibCapsMaterial;
		}
	}

	public Material GibCapsRadMaterial
	{
		get
		{
			if (!this.zombieGibCapsMaterialRadiated)
			{
				Material material = DataLoader.LoadAsset<Material>("@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat", false);
				this.zombieGibCapsMaterialRadiated = Object.Instantiate<Material>(material);
				this.zombieGibCapsMaterialRadiated.name = material.name + "(global)";
				if (DismembermentManager.DebugLogEnabled)
				{
					Log.Out("{0} material: {1}", new object[]
					{
						this.zombieGibCapsMaterialRadiated ? "load" : "load failed",
						"@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat"
					});
				}
			}
			return this.zombieGibCapsMaterialRadiated;
		}
	}

	public static Texture GetShaderTexture(Material _mat)
	{
		if (_mat.HasTexture("_ZombieColor"))
		{
			return _mat.GetTexture("_ZombieColor");
		}
		if (_mat.HasTexture("_Albedo"))
		{
			return _mat.GetTexture("_Albedo");
		}
		return null;
	}

	public static void SetShaderTexture(Material _mat, Texture _altColor)
	{
		if (_mat.HasTexture("_ZombieColor"))
		{
			_mat.SetTexture("_ZombieColor", _altColor);
		}
		if (_mat.HasTexture("_Albedo"))
		{
			_mat.SetTexture("_Albedo", _altColor);
		}
	}

	public static DismembermentManager Instance
	{
		get
		{
			return DismembermentManager.instance;
		}
	}

	public static void Init()
	{
		DismembermentManager.instance = new DismembermentManager();
		if (DismembermentManager.DebugLogEnabled)
		{
			Log.Out("DismembermentManager Init");
		}
	}

	public static void Cleanup()
	{
		if (DismembermentManager.instance != null)
		{
			List<DismemberedPart> list = DismembermentManager.instance.parts;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].CleanupDetached();
			}
			list.Clear();
		}
	}

	public void AddPart(DismemberedPart part)
	{
		this.parts.Add(part);
	}

	public void Update()
	{
		for (int i = 0; i < this.parts.Count; i++)
		{
			DismemberedPart dismemberedPart = this.parts[i];
			dismemberedPart.Update();
			if (dismemberedPart.ReadyForCleanup)
			{
				dismemberedPart.CleanupDetached();
				this.parts.RemoveAt(i);
				i--;
			}
		}
		if (this.parts.Count > 25)
		{
			int num = this.parts.Count - 25;
			for (int j = 0; j < num; j++)
			{
				this.parts[j].ReadyForCleanup = true;
			}
		}
	}

	public static float GetImpactForce(ItemClass ic, float strength)
	{
		if (ic != null)
		{
			if (ic.HasAnyTags(DismembermentManager.shotgunTags))
			{
				return 1.5f;
			}
			if (ic.HasAnyTags(DismembermentManager.sledgeTags))
			{
				return Mathf.Clamp(1f + Mathf.Abs(strength), 1f, 1.5f);
			}
			if (ic.HasAnyTags(DismembermentManager.knifeTags))
			{
				return Mathf.Abs(1f * strength) * 0.67f;
			}
		}
		return 1f;
	}

	public static EnumBodyPartHit GetBodyPartHit(uint bodyDamageFlag)
	{
		if (bodyDamageFlag == 1U)
		{
			return EnumBodyPartHit.Head;
		}
		if (bodyDamageFlag == 2U)
		{
			return EnumBodyPartHit.LeftUpperArm;
		}
		if (bodyDamageFlag == 4U)
		{
			return EnumBodyPartHit.LeftLowerArm;
		}
		if (bodyDamageFlag == 8U)
		{
			return EnumBodyPartHit.RightUpperArm;
		}
		if (bodyDamageFlag == 16U)
		{
			return EnumBodyPartHit.RightLowerArm;
		}
		if (bodyDamageFlag == 32U)
		{
			return EnumBodyPartHit.LeftUpperLeg;
		}
		if (bodyDamageFlag == 64U)
		{
			return EnumBodyPartHit.LeftLowerLeg;
		}
		if (bodyDamageFlag == 128U)
		{
			return EnumBodyPartHit.RightUpperLeg;
		}
		if (bodyDamageFlag == 256U)
		{
			return EnumBodyPartHit.RightLowerLeg;
		}
		return EnumBodyPartHit.None;
	}

	public static EnumBodyPartHit GetBodyPartHit(string _propKey)
	{
		if (_propKey.ContainsCaseInsensitive("L_HeadGore"))
		{
			return EnumBodyPartHit.Head;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftUpperArmGore"))
		{
			return EnumBodyPartHit.LeftUpperArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftLowerArmGore"))
		{
			return EnumBodyPartHit.LeftLowerArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightUpperArmGore"))
		{
			return EnumBodyPartHit.RightUpperArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightLowerArmGore"))
		{
			return EnumBodyPartHit.RightLowerArm;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftUpperLegGore"))
		{
			return EnumBodyPartHit.LeftUpperLeg;
		}
		if (_propKey.ContainsCaseInsensitive("L_LeftLowerLegGore"))
		{
			return EnumBodyPartHit.LeftLowerLeg;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightUpperLegGore"))
		{
			return EnumBodyPartHit.RightUpperLeg;
		}
		if (_propKey.ContainsCaseInsensitive("L_RightLowerLegGore"))
		{
			return EnumBodyPartHit.RightLowerLeg;
		}
		return EnumBodyPartHit.None;
	}

	public static string GetDamageTag(EnumDamageTypes _damageType, bool lastHitRanged)
	{
		if (_damageType == EnumDamageTypes.Piercing && lastHitRanged)
		{
			return "blunt";
		}
		switch (_damageType)
		{
		case EnumDamageTypes.Piercing:
		case EnumDamageTypes.Slashing:
			return "blade";
		case EnumDamageTypes.Bashing:
		case EnumDamageTypes.Crushing:
			return "blunt";
		case EnumDamageTypes.Heat:
			return "blunt";
		}
		return null;
	}

	public static DismemberedPartData DismemberPart(uint bodyDamageFlag, EnumDamageTypes damageType, EntityAlive _entity, bool isBiped, bool useLegacy = false)
	{
		return DismembermentManager.dismemberPart(DismembermentManager.GetBodyPartHit(bodyDamageFlag), damageType, _entity, isBiped, useLegacy);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismemberedPartData dismemberPart(EnumBodyPartHit partHit, EnumDamageTypes damageType, EntityAlive _entity, bool isBiped, bool useLegacy = false)
	{
		if (!DismembermentManager.hasDismemberedPart(partHit, isBiped))
		{
			return null;
		}
		DismemberedPartData dismemberedPartData = new DismemberedPartData();
		string[] dismemberedPart = DismembermentManager.getDismemberedPart(partHit, isBiped);
		dismemberedPartData.propertyKey = dismemberedPart[0];
		dismemberedPartData.prefabPath = dismemberedPart[1];
		dismemberedPartData.damageTypeKey = DismembermentManager.GetDamageTag(damageType, _entity.lastHitRanged);
		DynamicProperties properties = _entity.EntityClass.Properties;
		if (useLegacy || !properties.Contains(dismemberedPartData.propertyKey) || string.IsNullOrEmpty(properties.GetValue(dismemberedPartData.propertyKey)))
		{
			return dismemberedPartData;
		}
		if (properties.Data.ContainsKey(dismemberedPartData.propertyKey))
		{
			string[] array = properties.Values[dismemberedPartData.propertyKey].Split(';', StringSplitOptions.None);
			string[] array2 = properties.Data[dismemberedPartData.propertyKey].Split(';', StringSplitOptions.None);
			if (array[0].ContainsCaseInsensitive("linked"))
			{
				string text = array2[0].Replace("target=", "");
				array = properties.Values[text].Split(';', StringSplitOptions.None);
				array2 = properties.Data[text].Split(';', StringSplitOptions.None);
				dismemberedPartData.isLinked = true;
			}
			DismemberedPartData dismemberedPartData2 = DismembermentManager.readRandomPart(array, array2, dismemberedPartData.damageTypeKey);
			if (dismemberedPartData2 == null && dismemberedPartData.damageTypeKey == "blunt" && !dismemberedPartData.prefabPath.ContainsCaseInsensitive("blunt"))
			{
				DismemberedPartData dismemberedPartData3 = DismembermentManager.readRandomPart(array, array2, "blade");
				if (dismemberedPartData3 != null && (dismemberedPartData3.useMask || dismemberedPartData3.scaleOutLimb))
				{
					dismemberedPartData2 = dismemberedPartData3;
				}
			}
			if (dismemberedPartData2 != null)
			{
				if (!dismemberedPartData2.isLinked && dismemberedPartData2.Invalid)
				{
					return dismemberedPartData;
				}
				if (!string.IsNullOrEmpty(dismemberedPartData2.prefabPath))
				{
					dismemberedPartData.prefabPath = dismemberedPartData2.prefabPath;
				}
				dismemberedPartData.scale = dismemberedPartData2.scale;
				if (dismemberedPartData2.hasRotOffset)
				{
					dismemberedPartData.SetRot(dismemberedPartData2.rot);
				}
				dismemberedPartData.targetBone = dismemberedPartData2.targetBone;
				dismemberedPartData.attachToParent = dismemberedPartData2.attachToParent;
				dismemberedPartData.particlePaths = dismemberedPartData2.particlePaths;
				dismemberedPartData.isDetachable = dismemberedPartData2.isDetachable;
				dismemberedPartData.offset = dismemberedPartData2.offset;
				dismemberedPartData.useMask = dismemberedPartData2.useMask;
				dismemberedPartData.scaleOutLimb = dismemberedPartData2.scaleOutLimb;
				dismemberedPartData.solTarget = dismemberedPartData2.solTarget;
				dismemberedPartData.solScale = dismemberedPartData2.solScale;
				dismemberedPartData.hasSolScale = dismemberedPartData2.hasSolScale;
				dismemberedPartData.childTargetObj = dismemberedPartData2.childTargetObj;
				dismemberedPartData.insertBoneObj = dismemberedPartData2.insertBoneObj;
				dismemberedPartData.addScalePoint = dismemberedPartData2.addScalePoint;
				dismemberedPartData.maskScaleBlend = dismemberedPartData2.maskScaleBlend;
				dismemberedPartData.setFixedValues = dismemberedPartData2.setFixedValues;
				if (properties.Contains("DismemberMaterial"))
				{
					dismemberedPartData.dismemberMatPath = properties.GetValue("DismemberMaterial");
				}
			}
		}
		if (DismembermentManager.DebugLogEnabled)
		{
			Log.Out("[{0}.DismemberPart] - entityClass: {1}{2}", new object[]
			{
				"DismembermentManager",
				EntityClass.list[_entity.entityClass].entityClassName,
				dismemberedPartData.Log()
			});
		}
		return dismemberedPartData;
	}

	public static void ActivateDetachable(Transform rootT, string targetPart)
	{
		Transform transform = rootT;
		Transform transform2 = transform.Find("Physics");
		if (transform2)
		{
			transform = transform2;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			bool flag = child.name.ContainsCaseInsensitive(targetPart);
			child.gameObject.SetActive(flag);
		}
	}

	public static DismemberedPartData GetPartData(EntityAlive _entity)
	{
		string[] dismemberedPart = DismembermentManager.getDismemberedPart(_entity.bodyDamage.bodyPartHit, true);
		if (dismemberedPart != null)
		{
			string text = dismemberedPart[0];
			DynamicProperties properties = _entity.EntityClass.Properties;
			if (properties.Data.ContainsKey(text))
			{
				string[] array = properties.Values[text].Split(';', StringSplitOptions.None);
				string text2 = array[0];
				string[] array2 = properties.Data[text].Split(';', StringSplitOptions.None);
				string text3 = array2[0].Replace("target=", "");
				bool flag = text2.ContainsCaseInsensitive("linked");
				if (flag)
				{
					array = properties.Values[text3].Split(';', StringSplitOptions.None);
					array2 = properties.Data[text3].Split(';', StringSplitOptions.None);
				}
				DismemberedPartData dismemberedPartData = DismembermentManager.readPart(array2);
				if (dismemberedPartData != null)
				{
					dismemberedPartData.propertyKey = text3.Trim();
					dismemberedPartData.prefabPath = array[0].Trim();
					dismemberedPartData.isLinked = flag;
					return dismemberedPartData;
				}
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool hasDismemberedPart(EnumBodyPartHit part, bool isBiped = true)
	{
		if (isBiped)
		{
			return DismembermentManager.BipedDismemberments.ContainsKey(part);
		}
		return DismembermentManager.QuadrupedDismemberments.ContainsKey(part);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] getDismemberedPart(EnumBodyPartHit part, bool isBiped = true)
	{
		string[] array = null;
		if (isBiped)
		{
			DismembermentManager.BipedDismemberments.TryGetValue(part, out array);
		}
		else
		{
			DismembermentManager.QuadrupedDismemberments.TryGetValue(part, out array);
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void readString(string rawString, DismemberedPartData data)
	{
		string[] array = rawString.Split('=', StringSplitOptions.None);
		string text = array[0].Trim();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 1412654217U)
		{
			if (num <= 845187144U)
			{
				if (num <= 660706664U)
				{
					if (num != 91623701U)
					{
						if (num == 660706664U)
						{
							if (text == "atp")
							{
								bool.TryParse(array[1], out data.attachToParent);
								return;
							}
						}
					}
					else if (text == "solscale")
					{
						string[] array2 = array[1].Split(',', StringSplitOptions.None);
						float.TryParse(array2[0], out data.solScale.x);
						float.TryParse(array2[1], out data.solScale.y);
						float.TryParse(array2[2], out data.solScale.z);
						data.hasSolScale = true;
						return;
					}
				}
				else if (num != 829615687U)
				{
					if (num == 845187144U)
					{
						if (text == "target")
						{
							data.targetBone = array[1].Trim();
							return;
						}
					}
				}
				else if (text == "asp")
				{
					data.addScalePoint = array[1].Trim();
					return;
				}
			}
			else if (num <= 1213057714U)
			{
				if (num != 1158553358U)
				{
					if (num == 1213057714U)
					{
						if (text == "detach")
						{
							bool.TryParse(array[1], out data.isDetachable);
							return;
						}
					}
				}
				else if (text == "rot")
				{
					string[] array3 = array[1].Split(',', StringSplitOptions.None);
					if (array3.Length != 3)
					{
						return;
					}
					Vector3 zero = Vector3.zero;
					float.TryParse(array3[0], out zero.x);
					float.TryParse(array3[1], out zero.y);
					float.TryParse(array3[2], out zero.z);
					if (zero != Vector3.zero)
					{
						data.SetRot(zero);
						return;
					}
					return;
				}
			}
			else if (num != 1325231910U)
			{
				if (num != 1361572173U)
				{
					if (num == 1412654217U)
					{
						if (text == "pos")
						{
							string[] array4 = array[1].Split(',', StringSplitOptions.None);
							float.TryParse(array4[0], out data.pos.x);
							float.TryParse(array4[1], out data.pos.y);
							float.TryParse(array4[2], out data.pos.z);
							return;
						}
					}
				}
				else if (text == "type")
				{
					string text2 = array[1].Trim();
					if (text2 == "blunt" || text2 == "blade" || text2 == "bullet" || text2 == "explosive")
					{
						data.damageTypeKey = array[1].Trim();
						return;
					}
					return;
				}
			}
			else if (text == "oset")
			{
				string[] array5 = array[1].Split(',', StringSplitOptions.None);
				float.TryParse(array5[0], out data.offset.x);
				float.TryParse(array5[1], out data.offset.y);
				float.TryParse(array5[2], out data.offset.z);
				return;
			}
		}
		else if (num <= 2631859207U)
		{
			if (num <= 2190941297U)
			{
				if (num != 2095122494U)
				{
					if (num == 2190941297U)
					{
						if (text == "scale")
						{
							string[] array6 = array[1].Split(',', StringSplitOptions.None);
							float.TryParse(array6[0], out data.scale.x);
							float.TryParse(array6[1], out data.scale.y);
							float.TryParse(array6[2], out data.scale.z);
							return;
						}
					}
				}
				else if (text == "ico")
				{
					data.childTargetObj = array[1].Trim();
					return;
				}
			}
			else if (num != 2531611380U)
			{
				if (num == 2631859207U)
				{
					if (text == "ibo")
					{
						data.insertBoneObj = array[1].Trim();
						return;
					}
				}
			}
			else if (text == "soltarget")
			{
				data.solTarget = array[1].Trim();
				return;
			}
		}
		else if (num <= 3716176457U)
		{
			if (num != 3592343918U)
			{
				if (num == 3716176457U)
				{
					if (text == "msb")
					{
						data.maskScaleBlend = array[1].Trim();
						return;
					}
				}
			}
			else if (text == "sfv")
			{
				data.setFixedValues = array[1].Trim();
				return;
			}
		}
		else if (num != 3740252708U)
		{
			if (num != 3795205537U)
			{
				if (num == 3883353449U)
				{
					if (text == "mask")
					{
						bool.TryParse(array[1], out data.useMask);
						return;
					}
				}
			}
			else if (text == "sol")
			{
				bool.TryParse(array[1], out data.scaleOutLimb);
				return;
			}
		}
		else if (text == "particles")
		{
			data.particlePaths = array[1].Split(',', StringSplitOptions.None);
			return;
		}
		data.Invalid = true;
		if (DismembermentManager.DebugLogEnabled)
		{
			Log.Warning("[{0}.readString] entityclasses.xml unknown key:{1} in raw:{2}", new object[] { "DismembermentManager", text, rawString });
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismemberedPartData readRandomPart(string[] prefabs, string[] data, string tag)
	{
		List<DismemberedPartData> list = new List<DismemberedPartData>();
		for (int i = 0; i < data.Length; i++)
		{
			DismemberedPartData dismemberedPartData = new DismemberedPartData();
			if (i < prefabs.Length)
			{
				string text = prefabs[i].Trim();
				dismemberedPartData.prefabPath = text;
			}
			if (data[i].Contains('+'.ToString()))
			{
				string[] array = data[i].Split('+', StringSplitOptions.None);
				for (int j = 0; j < array.Length; j++)
				{
					DismembermentManager.readString(array[j], dismemberedPartData);
				}
				if (!string.IsNullOrEmpty(dismemberedPartData.damageTypeKey) && dismemberedPartData.damageTypeKey == tag)
				{
					list.Add(dismemberedPartData);
				}
			}
			else
			{
				DismembermentManager.readString(data[i], dismemberedPartData);
				if (!string.IsNullOrEmpty(dismemberedPartData.damageTypeKey) && dismemberedPartData.damageTypeKey == tag)
				{
					list.Add(dismemberedPartData);
				}
			}
		}
		if (list.Count > 0)
		{
			int num = Random.Range(0, list.Count);
			return list[num];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismemberedPartData readPart(string[] data)
	{
		List<DismemberedPartData> list = new List<DismemberedPartData>();
		for (int i = 0; i < data.Length; i++)
		{
			DismemberedPartData dismemberedPartData = new DismemberedPartData();
			string text = data[i];
			if (text.Contains('+'.ToString()))
			{
				string[] array = text.Split('+', StringSplitOptions.None);
				for (int j = 0; j < array.Length; j++)
				{
					DismembermentManager.readString(array[j], dismemberedPartData);
				}
				list.Add(dismemberedPartData);
			}
			else
			{
				DismembermentManager.readString(text, dismemberedPartData);
				list.Add(dismemberedPartData);
			}
		}
		if (list.Count > 0)
		{
			return list[0];
		}
		return null;
	}

	public static void SpawnParticleEffect(ParticleEffect _pe, int _entityId = -1)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!GameManager.IsDedicatedServer)
			{
				GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId, false, true);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, false), false, -1, _entityId, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, false), false);
	}

	public static void AddDebugArmObjects(Transform partT, Transform parentT)
	{
		if (partT && parentT && partT.name.ContainsCaseInsensitive("arm"))
		{
			GameObject gameObject = DataLoader.LoadAsset<GameObject>("@:Entities/Zombies/Gibs/Debug/debugAxisObj.prefab", false);
			if (gameObject)
			{
				GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
				gameObject2.transform.SetParent(parentT);
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				if (partT.name.ContainsCaseInsensitive("right"))
				{
					gameObject2.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
				}
				gameObject2.transform.localScale = Vector3.one * 0.5f;
				Transform transform = parentT.FindRecursive("rot");
				if (transform)
				{
					GameObject gameObject3 = Object.Instantiate<GameObject>(gameObject);
					gameObject3.transform.SetParent(transform);
					gameObject3.transform.localPosition = Vector3.zero;
					gameObject3.transform.localRotation = Quaternion.identity;
					gameObject3.transform.localScale = Vector3.one * 0.33f;
					gameObject3.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
				}
			}
		}
	}

	public const string cClassName = "DismembermentManager";

	public const string cManagedLimbsParentName = "DismemberedLimbs";

	public static bool DebugLogEnabled;

	public static bool DebugShowArmRotations;

	public static bool DebugDismemberExplosions;

	public static bool DebugBulletTime;

	public static bool DebugBloodParticles;

	public static bool DebugDontCreateParts;

	public static EnumBodyPartHit DebugBodyPartHit;

	public static bool DebugUseLegacy;

	public static bool DebugExplosiveCleanup;

	public const string DestroyedCapRoot = "pos";

	public const string PhysicsRootName = "Physics";

	public const string DetachableRootName = "Detachable";

	public const string cSubTagHeadAccessories = "HeadAccessories";

	public const string DetachableArmName = "HalfArm";

	public const string DetachableLegName = "HalfLeg";

	public const string ZombieSkinPrefix = "HD_";

	public const string MatPropRadiated = "_IsRadiated";

	public const string MatPropIrradiated = "_Irradiated";

	public const string MatPropFade = "_Fade";

	public static readonly FastTags<TagGroup.Global> radiatedTag = FastTags<TagGroup.Global>.Parse("radiated");

	public static readonly FastTags<TagGroup.Global> radOrChargedTag = FastTags<TagGroup.Global>.Parse("radiated,charged");

	public const string cCensorGoreSearch = "_CGore";

	public static readonly List<string> BluntCensors = new List<string> { "zombieLab", "zombieUtilityWorker" };

	public const string cAssetBundleZombies = "@:Entities/Zombies/";

	public const string cAssetBundleSearchName = "Dismemberment";

	public const string cAssetBundleFolder = "@:Entities/Zombies/";

	public const string cPrefabExt = ".prefab";

	public const string cLOD0 = "LOD0";

	public const string cLOD1 = "LOD1";

	public const string cLOD2 = "LOD2";

	public const string MatPropLeftLowerLeg = "_LeftLowerLeg";

	public const string MatPropRightLowerLeg = "_RightLowerLeg";

	public const string MatPropLeftUpperLeg = "_LeftUpperLeg";

	public const string MatPropRightUpperLeg = "_RightUpperLeg";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cAssetBundleGibMats = "Common/Gibs/Materials";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string CAssetBundleDefaultGib = "gib_dismemberment";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string CAssetBundleDefaultGibBlood = "gib_bloodcap";

	public const string CAssetBundleDefaultGibChunk = "ZombieGibs_caps";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDismemberMatXmlProp = "DismemberMaterial";

	public static string[] DefaultBundleGibs = new string[] { "gib_dismemberment", "gib_bloodcap", "ZombieGibs_caps" };

	public const string cGlobalMatName = "(global)";

	public const string cLocalMatName = "(local)";

	public const string cInstanceMatName = "(Instance)";

	public const string cMatExt = ".mat";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cGibCapsMatPath = "@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps.mat";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cGibCapsMatRadPath = "@:Entities/Zombies/Common/Gibs/Materials/ZombieGibs_caps_IsRadiated.mat";

	[PublicizedFrom(EAccessModifier.Private)]
	public Material zombieGibCapsMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material zombieGibCapsMaterialRadiated;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDefaultMatBaseTex = "_ZombieColor";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDefaultMatBaseTexAlt = "_Albedo";

	public const float cDefaultDetachLimbLifeTime = 10f;

	public const int cDefaultDetachLimbMax = 25;

	public const int cDefaultDetachLimbCleanupCount = 5;

	public const int cMaxLimbsFromExplosiveDeath = 3;

	public List<DismemberedPart> parts = new List<DismemberedPart>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static DismembermentManager instance;

	public const string cDynamicGore = "DynamicGore";

	public static FastTags<TagGroup.Global> rangedTags = FastTags<TagGroup.Global>.Parse("ranged");

	public static FastTags<TagGroup.Global> launcherTags = FastTags<TagGroup.Global>.Parse("launcher");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> shotgunTags = FastTags<TagGroup.Global>.Parse("shotgun");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> sledgeTags = FastTags<TagGroup.Global>.Parse("sledge");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> knifeTags = FastTags<TagGroup.Global>.Parse("knife");

	[PublicizedFrom(EAccessModifier.Private)]
	public const float MaxForce = 1.5f;

	public const string cXmlTag = "DismemberTag_";

	public const char cParamSplit = ';';

	public const char cRawSplit = '+';

	public const char cDataSplit = '=';

	public const char cCommaDel = ',';

	public static readonly Dictionary<EnumBodyPartHit, string[]> BipedDismemberments = new Dictionary<EnumBodyPartHit, string[]>
	{
		{
			EnumBodyPartHit.Head,
			new string[] { "DismemberTag_L_HeadGore", "Common/Dismemberment/HeadGore" }
		},
		{
			EnumBodyPartHit.LeftUpperLeg,
			new string[] { "DismemberTag_L_LeftUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.LeftLowerLeg,
			new string[] { "DismemberTag_L_LeftLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.RightUpperLeg,
			new string[] { "DismemberTag_L_RightUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.RightLowerLeg,
			new string[] { "DismemberTag_L_RightLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.LeftUpperArm,
			new string[] { "DismemberTag_L_LeftUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.LeftLowerArm,
			new string[] { "DismemberTag_L_LeftLowerArmGore", "Common/Dismemberment/LowerArmGore" }
		},
		{
			EnumBodyPartHit.RightUpperArm,
			new string[] { "DismemberTag_L_RightUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.RightLowerArm,
			new string[] { "DismemberTag_L_RightLowerArmGore", "Common/Dismemberment/LowerArmGore" }
		}
	};

	public static readonly Dictionary<EnumBodyPartHit, string[]> QuadrupedDismemberments = new Dictionary<EnumBodyPartHit, string[]>
	{
		{
			EnumBodyPartHit.Head,
			new string[] { "DismemberTag_L_HeadGore", "Common/Dismemberment/HeadGore" }
		},
		{
			EnumBodyPartHit.LeftUpperLeg,
			new string[] { "DismemberTag_L_LeftUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.LeftLowerLeg,
			new string[] { "DismemberTag_L_LeftLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.RightUpperLeg,
			new string[] { "DismemberTag_L_RightUpperLegGore", "Common/Dismemberment/UpperLegGore" }
		},
		{
			EnumBodyPartHit.RightLowerLeg,
			new string[] { "DismemberTag_L_RightLowerLegGore", "Common/Dismemberment/LowerLegGore" }
		},
		{
			EnumBodyPartHit.LeftUpperArm,
			new string[] { "DismemberTag_L_LeftUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.LeftLowerArm,
			new string[] { "DismemberTag_L_LeftLowerArmGore", "Common/Dismemberment/LowerArmGore" }
		},
		{
			EnumBodyPartHit.RightUpperArm,
			new string[] { "DismemberTag_L_RightUpperArmGore", "Common/Dismemberment/UpperArmGore" }
		},
		{
			EnumBodyPartHit.RightLowerArm,
			new string[] { "DismemberTag_L_RightLowerArmGore", "Common/Dismemberment/LowerArmGore" }
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDebugAxisPath = "@:Entities/Zombies/Gibs/Debug/debugAxisObj.prefab";

	public static class DamageKeys
	{
		public const string blade = "blade";

		public const string blunt = "blunt";

		public const string bullet = "bullet";

		public const string exlosive = "explosive";
	}

	public enum DamageTags
	{
		none,
		blade,
		blunt,
		any
	}

	public static class ParseKeys
	{
		public const string cType = "type";

		public const string cTarget = "target";

		public const string cAttachToParent = "atp";

		public const string cDetach = "detach";

		public const string cMask = "mask";

		public const string cScaleOutLimb = "sol";

		public const string cSolTarget = "soltarget";

		public const string cSolScale = "solscale";

		public const string cInsertChildObj = "ico";

		public const string cInsertBoneObj = "ibo";

		public const string cAddScalePoint = "asp";

		public const string cMaskScaleBlend = "msb";

		public const string cSetFixedValues = "sfv";
	}
}
