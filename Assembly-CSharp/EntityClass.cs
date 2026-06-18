using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityClass
{
	public static void Add(string _entityClassname, EntityClass _entityClass)
	{
		_entityClass.entityClassName = _entityClassname;
		EntityClass.list[_entityClassname.GetHashCode()] = _entityClass;
	}

	public static EntityClass GetEntityClass(int entityClass)
	{
		EntityClass entityClass2;
		EntityClass.list.TryGetValue(entityClass, out entityClass2);
		return entityClass2;
	}

	public static string GetEntityClassName(int entityClass)
	{
		EntityClass entityClass2;
		if (EntityClass.list.TryGetValue(entityClass, out entityClass2))
		{
			return entityClass2.entityClassName;
		}
		return "null";
	}

	public static int GetId(string _name)
	{
		foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
		{
			if (keyValuePair.Value.entityClassName == _name)
			{
				return keyValuePair.Key;
			}
		}
		return -1;
	}

	public static int FromString(string _s)
	{
		return _s.GetHashCode();
	}

	public EntityClass Init()
	{
		this.censorType = 1;
		string text = "";
		if (this.Properties.Contains(EntityClass.PropCensor))
		{
			text = this.Properties.GetString(EntityClass.PropCensor);
		}
		if (!string.IsNullOrEmpty(text) && text.Contains(","))
		{
			string[] array = text.Split(",", StringSplitOptions.None);
			if (array.Length > 1)
			{
				StringParsers.TryParseSInt32(array[0], out this.censorMode);
				StringParsers.TryParseSInt32(array[1], out this.censorType);
			}
		}
		else
		{
			this.Properties.ParseInt(EntityClass.PropCensor, ref this.censorMode);
		}
		if (!this.Properties.Values.TryGetValue(EntityClass.PropPrefab, out this.prefabPath) || this.prefabPath.Length == 0)
		{
			throw new Exception("Mandatory property 'prefab' missing in entity_class '" + this.entityClassName + "'");
		}
		string text2;
		bool flag;
		if (this.Properties.Values.TryGetValue(EntityClass.PropPrefabCombined, out text2) && bool.TryParse(text2, out flag) && flag)
		{
			this.IsPrefabCombined = true;
		}
		else if (this.prefabPath[0] == '/')
		{
			this.prefabPath = this.prefabPath.Substring(1);
			this.IsPrefabCombined = true;
		}
		else if (DataLoader.IsInResources(this.prefabPath))
		{
			this.prefabPath = "Prefabs/prefabEntity" + this.prefabPath;
		}
		string text3;
		if (this.Properties.Values.TryGetValue(EntityClass.PropMesh, out text3) && text3.Length > 0)
		{
			if (this.censorMode != 0 && (this.censorType == 1 || this.censorType == 3) && GameManager.Instance && GameManager.Instance.IsGoreCensored())
			{
				text3 = text3.Replace(".", "_CGore.");
			}
			if (DataLoader.IsInResources(text3))
			{
				text3 = "Entities/" + text3;
			}
			this.meshPath = text3;
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropMeshFP))
		{
			string text4 = this.Properties.Values[EntityClass.PropMeshFP];
			if (DataLoader.IsInResources(text4))
			{
				text4 = "Entities/" + text4;
			}
			this.meshFP = DataLoader.LoadAsset<Transform>(text4, false);
			if (this.meshFP == null)
			{
				Log.Error(string.Concat(new string[] { "Could not load file '", text4, "' for entity_class '", this.entityClassName, "'" }));
			}
		}
		this.entityFlags = EntityFlags.None;
		EntityClass.ParseEntityFlags(this.Properties.GetString(EntityClass.PropEntityFlags), ref this.entityFlags);
		if (this.Properties.Values.ContainsKey(EntityClass.PropClass))
		{
			this.classname = Type.GetType(this.Properties.Values[EntityClass.PropClass]);
			if (this.classname == null)
			{
				Log.Error(string.Concat(new string[]
				{
					"Could not instantiate class",
					this.Properties.Values[EntityClass.PropClass],
					"' for entity_class '",
					this.entityClassName,
					"'"
				}));
			}
		}
		this.modelType = typeof(EModelCustom);
		string @string = this.Properties.GetString(EntityClass.PropModelType);
		if (@string.Length > 0)
		{
			this.modelType = ReflectionHelpers.GetTypeWithPrefix("EModel", @string);
			if (this.modelType == null)
			{
				throw new Exception("Model class '" + @string + "' not found!");
			}
		}
		string string2 = this.Properties.GetString(EntityClass.PropAltMats);
		if (string2.Length > 0)
		{
			this.AltMatNames = string2.Split(',', StringSplitOptions.None);
		}
		string string3 = this.Properties.GetString(EntityClass.PropSwapMats);
		if (string3.Length > 0)
		{
			this.MatSwap = string3.Split(",", StringSplitOptions.None);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropParticleOnSpawn))
		{
			this.particleOnSpawn.fileName = this.Properties.Values[EntityClass.PropParticleOnSpawn];
			this.particleOnSpawn.shapeMesh = this.Properties.Params1[EntityClass.PropParticleOnSpawn];
			DataLoader.PreloadBundle(this.particleOnSpawn.fileName);
		}
		this.RagdollOnDeathChance = 0.5f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropRagdollOnDeathChance))
		{
			this.RagdollOnDeathChance = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropRagdollOnDeathChance], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropHasRagdoll))
		{
			this.HasRagdoll = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropHasRagdoll], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropColliders))
		{
			this.CollidersRagdollAsset = this.Properties.Values[EntityClass.PropColliders];
			DataLoader.PreloadBundle(this.CollidersRagdollAsset);
		}
		this.Properties.ParseFloat(EntityClass.PropLookAtAngle, ref this.LookAtAngle);
		if (this.Properties.Values.ContainsKey(EntityClass.PropCrouchYOffsetFP))
		{
			this.crouchYOffsetFP = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropCrouchYOffsetFP], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropParent))
		{
			this.parentGameObjectName = this.Properties.Values[EntityClass.PropParent];
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSkinTexture))
		{
			this.skinTexture = this.Properties.Values[EntityClass.PropSkinTexture];
			DataLoader.PreloadBundle(this.skinTexture);
		}
		this.bIsEnemyEntity = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsEnemyEntity))
		{
			this.bIsEnemyEntity = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsEnemyEntity], 0, -1, true);
		}
		this.bIsAnimalEntity = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsAnimalEntity))
		{
			this.bIsAnimalEntity = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsAnimalEntity], 0, -1, true);
		}
		this.RootMotion = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropRootMotion))
		{
			this.RootMotion = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropRootMotion], 0, -1, true);
		}
		this.HasDeathAnim = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropHasDeathAnim))
		{
			this.HasDeathAnim = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropHasDeathAnim], 0, -1, true);
		}
		this.ExperienceValue = 100;
		if (this.Properties.Values.ContainsKey(EntityClass.PropExperienceGain))
		{
			this.ExperienceValue = (int)StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropExperienceGain], 0, -1, NumberStyles.Any);
		}
		string string4 = this.Properties.GetString(EntityClass.PropLootDropEntityClass);
		if (string4.Length > 0)
		{
			this.lootDrops = new List<EntityClass.LootDrop>();
			EntityClass.LootDrop lootDrop = default(EntityClass.LootDrop);
			if (!string4.Contains(","))
			{
				lootDrop.entityClass = EntityClass.FromString(string4);
				lootDrop.weight = 1f;
				this.lootDrops.Add(lootDrop);
			}
			else
			{
				string[] array2 = string4.Split(EntityClass.commaSeparator);
				int num = array2.Length / 2;
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 2;
					lootDrop.entityClass = EntityClass.FromString(array2[num2].Trim());
					lootDrop.weight = float.Parse(array2[num2 + 1]);
					this.lootDrops.Add(lootDrop);
				}
				float num3 = 0f;
				for (int j = 0; j < num; j++)
				{
					num3 += this.lootDrops[j].weight;
				}
				for (int k = 0; k < num; k++)
				{
					lootDrop = this.lootDrops[k];
					lootDrop.weight /= num3;
					this.lootDrops[k] = lootDrop;
				}
			}
		}
		this.bIsMale = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsMale))
		{
			this.bIsMale = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsMale], 0, -1, true);
		}
		this.bIsChunkObserver = false;
		if (this.Properties.Values.ContainsKey(EntityClass.PropIsChunkObserver))
		{
			this.bIsChunkObserver = StringParsers.ParseBool(this.Properties.Values[EntityClass.PropIsChunkObserver], 0, -1, true);
		}
		this.SightRange = Constants.cDefaultMonsterSeeDistance;
		if (this.Properties.Values.ContainsKey(EntityClass.PropSightRange))
		{
			this.SightRange = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropSightRange], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropSightLightThreshold))
		{
			this.sightLightThreshold = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropSightLightThreshold]);
		}
		else
		{
			this.sightLightThreshold = new Vector2(30f, 100f);
		}
		this.SleeperNoiseToSense = new Vector2(15f, 15f);
		this.Properties.ParseVec(EntityClass.PropSleeperNoiseToSense, ref this.SleeperNoiseToSense);
		this.SleeperNoiseToSenseSoundChance = 1f;
		this.Properties.ParseFloat(EntityClass.PropSleeperNoiseToSenseSoundChance, ref this.SleeperNoiseToSenseSoundChance);
		this.SleeperNoiseToWake = new Vector2(15f, 15f);
		this.Properties.ParseVec(EntityClass.PropSleeperNoiseToWake, ref this.SleeperNoiseToWake);
		this.SleeperSightToSenseMin = new Vector2(25f, 25f);
		this.Properties.ParseVec(EntityClass.PropSleeperSightToSenseMin, ref this.SleeperSightToSenseMin);
		this.SleeperSightToSenseMax = new Vector2(200f, 200f);
		this.Properties.ParseVec(EntityClass.PropSleeperSightToSenseMax, ref this.SleeperSightToSenseMax);
		this.SleeperSightToWakeMin = new Vector2(15f, 15f);
		this.Properties.ParseVec(EntityClass.PropSleeperSightToWakeMin, ref this.SleeperSightToWakeMin);
		this.SleeperSightToWakeMax = new Vector2(200f, 200f);
		this.Properties.ParseVec(EntityClass.PropSleeperSightToWakeMax, ref this.SleeperSightToWakeMax);
		this.MassKg = 10f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropMass))
		{
			this.MassKg = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropMass], 0, -1, NumberStyles.Any);
		}
		this.MassKg *= 0.454f;
		this.SizeScale = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropSizeScale))
		{
			this.SizeScale = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropSizeScale], 0, -1, NumberStyles.Any);
		}
		string string5 = this.Properties.GetString(EntityClass.PropPhysicsBody);
		if (string5.Length > 0)
		{
			this.PhysicsBody = PhysicsBodyLayout.Find(string5);
		}
		if (this.Properties.Values.ContainsKey("DeadBodyHitPoints"))
		{
			this.DeadBodyHitPoints = int.Parse(this.Properties.Values["DeadBodyHitPoints"]);
		}
		this.Properties.ParseFloat(EntityClass.PropLegCrippleScale, ref this.LegCrippleScale);
		this.Properties.ParseFloat(EntityClass.PropLegCrawlerThreshold, ref this.LegCrawlerThreshold);
		this.DismemberMultiplierHead = 1f;
		this.Properties.ParseFloat(EntityClass.PropDismemberMultiplierHead, ref this.DismemberMultiplierHead);
		this.DismemberMultiplierArms = 1f;
		this.Properties.ParseFloat(EntityClass.PropDismemberMultiplierArms, ref this.DismemberMultiplierArms);
		this.DismemberMultiplierLegs = 1f;
		this.Properties.ParseFloat(EntityClass.PropDismemberMultiplierLegs, ref this.DismemberMultiplierLegs);
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownKneelDamageThreshold))
		{
			this.KnockdownKneelDamageThreshold = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropKnockdownKneelDamageThreshold], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownKneelStunDuration))
		{
			this.KnockdownKneelStunDuration = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownKneelStunDuration]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownProneDamageThreshold))
		{
			this.KnockdownProneDamageThreshold = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropKnockdownProneDamageThreshold], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownProneStunDuration))
		{
			this.KnockdownProneStunDuration = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownProneStunDuration]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownKneelRefillRate))
		{
			this.KnockdownKneelRefillRate = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownKneelRefillRate]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropKnockdownProneRefillRate))
		{
			this.KnockdownProneRefillRate = StringParsers.ParseMinMaxCount(this.Properties.Values[EntityClass.PropKnockdownProneRefillRate]);
		}
		this.LegsExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropLegsExplosionDamageMultiplier))
		{
			this.LegsExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropLegsExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		this.ArmsExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropArmsExplosionDamageMultiplier))
		{
			this.ArmsExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropArmsExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		this.HeadExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropHeadExplosionDamageMultiplier))
		{
			this.HeadExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropHeadExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		this.ChestExplosionDamageMultiplier = 1f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropChestExplosionDamageMultiplier))
		{
			this.ChestExplosionDamageMultiplier = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropChestExplosionDamageMultiplier], 0, -1, NumberStyles.Any);
		}
		Vector3 zero = Vector3.zero;
		this.Properties.ParseVec(EntityClass.PropPainResistPerHit, ref zero, 0f);
		this.PainResistPerHit = zero.x;
		this.PainResistPerHitLowHealth = zero.y;
		this.PainResistPerHitLowHealthPercent = zero.z;
		if (this.Properties.Values.ContainsKey(EntityClass.PropArchetype))
		{
			this.ArchetypeName = this.Properties.Values[EntityClass.PropArchetype];
		}
		this.SwimOffset = 0.9f;
		if (this.Properties.Values.ContainsKey(EntityClass.PropSwimOffset))
		{
			this.SwimOffset = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropSwimOffset], 0, -1, NumberStyles.Any);
		}
		this.SearchRadius = 6f;
		this.Properties.ParseFloat(EntityClass.PropSearchRadius, ref this.SearchRadius);
		if (this.Properties.Values.ContainsKey(EntityClass.PropUMARace))
		{
			this.UMARace = this.Properties.Values[EntityClass.PropUMARace];
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropUMAGeneratedModelName))
		{
			this.UMAGeneratedModelName = this.Properties.Values[EntityClass.PropUMAGeneratedModelName];
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropModelTransformAdjust))
		{
			this.ModelTransformAdjust = StringParsers.ParseVector3(this.Properties.Values[EntityClass.PropModelTransformAdjust], 0, -1);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropAIPackages))
		{
			this.AIPackages = this.Properties.Values[EntityClass.PropAIPackages].Split(',', StringSplitOptions.None);
			for (int l = 0; l < this.AIPackages.Length; l++)
			{
				this.AIPackages[l] = this.AIPackages[l].Trim();
			}
			this.UseAIPackages = true;
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropBuffs))
		{
			string[] array3 = this.Properties.Values[EntityClass.PropBuffs].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (array3.Length != 0)
			{
				this.Buffs = new List<string>(array3);
			}
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropMaxTurnSpeed))
		{
			this.MaxTurnSpeed = StringParsers.ParseFloat(this.Properties.Values[EntityClass.PropMaxTurnSpeed], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropTags))
		{
			this.Tags = FastTags<TagGroup.Global>.Parse(this.Properties.Values[EntityClass.PropTags]);
		}
		if (this.Properties.Values.ContainsKey(EntityClass.PropNavObject))
		{
			this.NavObject = this.Properties.Values[EntityClass.PropNavObject];
		}
		this.Properties.ParseVec(EntityClass.PropNavObjectHeadOffset, ref this.NavObjectHeadOffset);
		this.explosionData = new ExplosionData(this.Properties, this.Effects);
		bool flag2 = false;
		this.Properties.ParseBool(EntityClass.PropHideInSpawnMenu, ref flag2);
		if (flag2)
		{
			this.userSpawnType = EntityClass.UserSpawnType.Console;
		}
		this.Properties.ParseEnum<EntityClass.UserSpawnType>(EntityClass.PropUserSpawnType, ref this.userSpawnType);
		this.Properties.ParseBool(EntityClass.PropCanBigHead, ref this.CanBigHead);
		this.Properties.ParseInt(EntityClass.PropDanceType, ref this.DanceTypeID);
		this.Properties.ParseString(EntityClass.PropOnActivateEvent, ref this.onActivateEvent);
		this.Properties.ParseString(EntityClass.PropPreviousTierZombie, ref this.PreviousTierZombieName);
		List<Dictionary<string, string>> list;
		if (this.Properties.Array.TryGetValue(EntityClass.PropTokenManager, out list))
		{
			this.TokenManagerConfig = new Dictionary<AITokenType, AITokenConfig>();
			for (int m = 0; m < list.Count; m++)
			{
				string text5;
				AITokenType aitokenType;
				if (list[m].TryGetValue("type", out text5) && Enum.TryParse<AITokenType>(text5, true, out aitokenType))
				{
					AITokenConfig aitokenConfig = default(AITokenConfig);
					string text6;
					if (list[m].TryGetValue("max", out text6))
					{
						int.TryParse(text6, out aitokenConfig.MaxClaims);
					}
					this.TokenManagerConfig[aitokenType] = aitokenConfig;
				}
			}
		}
		this.CalculateEntityTier();
		return this;
	}

	public void CopyFrom(EntityClass _other, HashSet<string> _exclude = null)
	{
		foreach (KeyValuePair<string, string> keyValuePair in _other.Properties.Values)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair.Key))
			{
				this.Properties.Values[keyValuePair.Key] = _other.Properties.Values[keyValuePair.Key];
			}
		}
		foreach (KeyValuePair<string, string> keyValuePair2 in _other.Properties.Params1)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair2.Key))
			{
				this.Properties.Params1[keyValuePair2.Key] = keyValuePair2.Value;
			}
		}
		foreach (KeyValuePair<string, string> keyValuePair3 in _other.Properties.Params2)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair3.Key))
			{
				this.Properties.Params2[keyValuePair3.Key] = keyValuePair3.Value;
			}
		}
		foreach (KeyValuePair<string, string> keyValuePair4 in _other.Properties.Data)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair4.Key))
			{
				this.Properties.Data[keyValuePair4.Key] = keyValuePair4.Value;
			}
		}
		foreach (KeyValuePair<string, DynamicProperties> keyValuePair5 in _other.Properties.Classes)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair5.Key))
			{
				DynamicProperties dynamicProperties = new DynamicProperties();
				dynamicProperties.CopyFrom(keyValuePair5.Value, null);
				this.Properties.Classes[keyValuePair5.Key] = dynamicProperties;
			}
		}
	}

	public static void ParseEntityFlags(string _names, ref EntityFlags optionalValue)
	{
		if (_names.Length > 0)
		{
			if (_names.IndexOf(',') >= 0)
			{
				string[] array = _names.Split(EntityClass.commaSeparator, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					EntityFlags entityFlags;
					if (EnumUtils.TryParse<EntityFlags>(array[i], out entityFlags, true))
					{
						optionalValue |= entityFlags;
					}
				}
				return;
			}
			EntityFlags entityFlags2;
			if (EnumUtils.TryParse<EntityFlags>(_names, out entityFlags2, true))
			{
				optionalValue = entityFlags2;
			}
		}
	}

	public static void Cleanup()
	{
		EntityClass.list.Clear();
	}

	public void AddDroppedId(EnumDropEvent _eEvent, string _name, int _minCount, int _maxCount, float _prob, float _stickChance, string _toolCategory, string _tag)
	{
		List<Block.SItemDropProb> list = (this.itemsToDrop.ContainsKey(_eEvent) ? this.itemsToDrop[_eEvent] : null);
		if (list == null)
		{
			list = new List<Block.SItemDropProb>();
			this.itemsToDrop[_eEvent] = list;
		}
		list.Add(new Block.SItemDropProb(_name, _minCount, _maxCount, _prob, 1f, _stickChance, _toolCategory, _tag));
	}

	public int LootDropPick(GameRandom _rand)
	{
		int num = 0;
		if (this.lootDrops.Count >= 2)
		{
			float num2 = 0f;
			float randomFloat = _rand.RandomFloat;
			for (int i = 0; i < this.lootDrops.Count; i++)
			{
				num2 += this.lootDrops[i].weight;
				if (randomFloat <= num2)
				{
					num = i;
					break;
				}
			}
		}
		return this.lootDrops[num].entityClass;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateEntityTier()
	{
		if (this.Tags.Test_AnySet(EntityClass.eliteTag))
		{
			this.EntityTier = EntityClass.EntityTierTypes.Elite;
			return;
		}
		if (this.Tags.Test_AnySet(EntityClass.radiatedTag))
		{
			this.EntityTier = EntityClass.EntityTierTypes.Radiated;
			return;
		}
		if (this.Tags.Test_AnySet(EntityClass.feralTag))
		{
			this.EntityTier = EntityClass.EntityTierTypes.Feral;
			return;
		}
		if (this.Tags.Test_AnySet(EntityClass.specialTag))
		{
			this.EntityTier = EntityClass.EntityTierTypes.Special;
			return;
		}
		if (this.Tags.Test_AnySet(EntityClass.strongTag))
		{
			this.EntityTier = EntityClass.EntityTierTypes.Strong;
			return;
		}
		this.EntityTier = EntityClass.EntityTierTypes.Normal;
	}

	public EntityClass GetPreviousTierEntity()
	{
		if (this.previousTierEntities == null && this.PreviousTierZombieName != "")
		{
			string[] array = this.PreviousTierZombieName.Split(',', StringSplitOptions.None);
			this.previousTierEntities = new EntityClass[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				this.previousTierEntities[i] = EntityClass.GetEntityClass(EntityClass.GetId(array[i]));
			}
		}
		if (this.previousTierEntities == null || this.previousTierEntities.Length == 0)
		{
			return null;
		}
		if (this.previousTierEntities.Length > 1)
		{
			return this.previousTierEntities[GameManager.Instance.World.GetGameRandom().RandomRange(this.previousTierEntities.Length)];
		}
		return this.previousTierEntities[0];
	}

	public static string PropEntityFlags = "EntityFlags";

	public static string PropEntityType = "EntityType";

	public static string PropClass = "Class";

	public static string PropCensor = "Censor";

	public static string PropMesh = "Mesh";

	public static string PropMeshFP = "MeshFP";

	public static string PropPrefab = "Prefab";

	public static string PropPrefabCombined = "PrefabCombined";

	public static string PropParent = "Parent";

	public static string PropAvatarController = "AvatarController";

	public static string PropLocalAvatarController = "LocalAvatarController";

	public static string PropSkinTexture = "SkinTexture";

	public static string PropAltMats = "AltMats";

	public static string PropSwapMats = "SwapMats";

	public static string PropMatColor = "MatColor";

	public static string PropRightHandJointName = "RightHandJointName";

	public static string PropHandItem = "HandItem";

	public static string PropHandItemCrawler = "HandItemCrawler";

	public static string PropMaxHealth = "MaxHealth";

	public static string PropMaxStamina = "MaxStamina";

	public static string PropSickness = "Sickness";

	public static string PropGassiness = "Gassiness";

	public static string PropWellness = "Wellness";

	public static string PropFood = "Food";

	public static string PropWater = "Water";

	public static string PropMaxViewAngle = "MaxViewAngle";

	public static string PropWeight = "Weight";

	public static string PropPushFactor = "PushFactor";

	public static string PropTimeStayAfterDeath = "TimeStayAfterDeath";

	public static string PropImmunity = "Immunity";

	public static string PropIsMale = "IsMale";

	public static string PropIsChunkObserver = "IsChunkObserver";

	public static string PropAIFeralSense = "AIFeralSense";

	public static string PropAIGroupCircle = "AIGroupCircle";

	public static string PropAINoiseSeekDist = "AINoiseSeekDist";

	public static string PropAISeeOffset = "AISeeOffset";

	public static string PropAIPathCostScale = "AIPathCostScale";

	public static string PropAITask = "AITask-";

	public static string PropAITargetTask = "AITarget-";

	public static string PropMoveSpeed = "MoveSpeed";

	public static string PropMoveSpeedNight = "MoveSpeedNight";

	public static string PropMoveSpeedAggro = "MoveSpeedAggro";

	public static string PropMoveSpeedRand = "MoveSpeedRand";

	public static string PropMoveSpeedPanic = "MoveSpeedPanic";

	public static string PropMoveSpeedPattern = "MoveSpeedPattern";

	public static string PropSwimSpeed = "SwimSpeed";

	public static string PropSwimStrokeRate = "SwimStrokeRate";

	public static string PropCrouchType = "CrouchType";

	public static string PropDanceType = "DanceType";

	public static string PropWalkType = "WalkType";

	public static string PropCanClimbVertical = "CanClimbVertical";

	public static string PropCanClimbLadders = "CanClimbLadders";

	public static string PropJumpDelay = "JumpDelay";

	public static string PropJumpMaxDistance = "JumpMaxDistance";

	public static string PropIsEnemyEntity = "IsEnemyEntity";

	public static string PropIsAnimalEntity = "IsAnimalEntity";

	public static string PropSoundRandomTime = "SoundRandomTime";

	public static string PropSoundAlertTime = "SoundAlertTime";

	public static string PropSoundRandom = "SoundRandom";

	public static string PropSoundHurt = "SoundHurt";

	public static string PropSoundJump = "SoundJump";

	public static string PropSoundPlayerLandThump = "SoundPlayerLandThump";

	public static string PropSoundHurtSmall = "SoundHurtSmall";

	public static string PropSoundDrownPain = "SoundDrownPain";

	public static string PropSoundDrownDeath = "SoundDrownDeath";

	public static string PropSoundWaterSurface = "SoundWaterSurface";

	public static string PropSoundDeath = "SoundDeath";

	public static string PropSoundAttack = "SoundAttack";

	public static string PropSoundAlert = "SoundAlert";

	public static string PropSoundSense = "SoundSense";

	public static string PropSoundStamina = "SoundStamina";

	public static string PropSoundLiving = "SoundLiving";

	public static string PropSoundSpawn = "SoundSpawn";

	public static string PropSoundLand = "SoundLanding";

	public static string PropSoundStepType = "SoundStepType";

	public static string PropSoundGiveUp = "SoundGiveUp";

	public static string PropSoundExplodeWarn = "SoundExplodeWarn";

	public static string PropSoundTick = "SoundTick";

	public static string PropExplodeDelay = "ExplodeDelay";

	public static string PropExplodeHealthThreshold = "ExplodeHealthThreshold";

	public static string PropLootList = "LootList";

	public static string PropLootDropProb = "LootDropProb";

	public static string PropLootDropEntityClass = "LootDropEntityClass";

	public static string PropAttackTimeoutDay = "AttackTimeoutDay";

	public static string PropAttackTimeoutNight = "AttackTimeoutNight";

	public static string PropMapIcon = "MapIcon";

	public static string PropCompassIcon = "CompassIcon";

	public static string PropTrackerIcon = "TrackerIcon";

	public static string PropCompassUpIcon = "CompassUpIcon";

	public static string PropCompassDownIcon = "CompassDownIcon";

	public static string PropParticleOnSpawn = "ParticleOnSpawn";

	public static string PropParticleOnDeath = "ParticleOnDeath";

	public static string PropParticleOnDestroy = "ParticleOnDestroy";

	public static string PropItemsOnEnterGame = "ItemsOnEnterGame";

	public static string PropFallLandBehavior = "FallLandBehavior";

	public static string PropDestroyBlockBehavior = "DestroyBlockBehavior";

	public static string PropDropInventoryBlock = "DropInventoryBlock";

	public static string PropModelType = "ModelType";

	public static string PropRagdollOnDeathChance = "RagdollOnDeathChance";

	public static string PropHasRagdoll = "HasRagdoll";

	public static string PropMass = "Mass";

	public static string PropSizeScale = "SizeScale";

	public static string PropPhysicsBody = "PhysicsBody";

	public static string PropColliders = "Colliders";

	public static string PropLookAtAngle = "LookAtAngle";

	public static string PropCrouchYOffsetFP = "CrouchYOffsetFP";

	public static string PropRotateToGround = "RotateToGround";

	public static string PropRootMotion = "RootMotion";

	public static string PropExperienceGain = "ExperienceGain";

	public static string PropHasDeathAnim = "HasDeathAnim";

	public static string PropLegCrippleScale = "LegCrippleScale";

	public static string PropLegCrawlerThreshold = "LegCrawlerThreshold";

	public static string PropDismemberMultiplierHead = "DismemberMultiplierHead";

	public static string PropDismemberMultiplierArms = "DismemberMultiplierArms";

	public static string PropDismemberMultiplierLegs = "DismemberMultiplierLegs";

	public static string PropKnockdownKneelDamageThreshold = "KnockdownKneelDamageThreshold";

	public static string PropKnockdownKneelStunDuration = "KnockdownKneelStunDuration";

	public static string PropKnockdownProneDamageThreshold = "KnockdownProneDamageThreshold";

	public static string PropKnockdownProneStunDuration = "KnockdownProneStunDuration";

	public static string PropKnockdownProneRefillRate = "KnockdownProneRefillRate";

	public static string PropKnockdownKneelRefillRate = "KnockdownKneelRefillRate";

	public static string PropArmsExplosionDamageMultiplier = "ArmsExplosionDamageMultiplier";

	public static string PropLegsExplosionDamageMultiplier = "LegsExplosionDamageMultiplier";

	public static string PropChestExplosionDamageMultiplier = "ChestExplosionDamageMultiplier";

	public static string PropHeadExplosionDamageMultiplier = "HeadExplosionDamageMultiplier";

	public static string PropPainResistPerHit = "PainResistPerHit";

	public static string PropArchetype = "Archetype";

	public static string PropSwimOffset = "SwimOffset";

	public static string PropUMARace = "UMARace";

	public static string PropUMAGeneratedModelName = "UMAGeneratedModelName";

	public static string PropNPCID = "NPCID";

	public static string PropModelTransformAdjust = "ModelTransformAdjust";

	public static string PropAIPackages = "AIPackages";

	public static string PropBuffs = "Buffs";

	public static string PropStealthSoundDecayRate = "StealthSoundDecayRate";

	public static string PropSightRange = "SightRange";

	public static string PropSightLightThreshold = "SightLightThreshold";

	public static string PropSleeperSightToSenseMin = "SleeperSightToSenseMin";

	public static string PropSleeperSightToSenseMax = "SleeperSightToSenseMax";

	public static string PropSleeperSightToWakeMin = "SleeperSightToWakeMin";

	public static string PropSleeperSightToWakeMax = "SleeperSightToWakeMax";

	public static string PropSleeperNoiseToSense = "SleeperNoiseToSense";

	public static string PropSleeperNoiseToSenseSoundChance = "SleeperNoiseToSenseSoundChance";

	public static string PropSleeperNoiseToWake = "SleeperNoiseToWake";

	public static string PropSoundSleeperSense = "SoundSleeperSense";

	public static string PropSoundSleeperSnore = "SoundSleeperBackToSleep";

	public static string PropMaxTurnSpeed = "MaxTurnSpeed";

	public static string PropSearchRadius = "SearchRadius";

	public static string PropTags = "Tags";

	public static string PropNavObject = "NavObject";

	public static string PropNavObjectHeadOffset = "NavObjectHeadOffset";

	public static string PropStompsSpikes = "StompsSpikes";

	public static string PropUserSpawnType = "UserSpawnType";

	public static string PropHideInSpawnMenu = "HideInSpawnMenu";

	public static string PropCanBigHead = "CanBigHead";

	public static string PropOnActivateEvent = "ActivateEvent";

	public static string PropCustomCommandName = "CustomCommandName";

	public static string PropCustomCommandIcon = "CustomCommandIcon";

	public static string PropCustomCommandIconColor = "CustomCommandIconColor";

	public static string PropCustomCommandEvent = "CustomCommandEvent";

	public static string PropCustomCommandActivateTime = "CustomCommandActivateTime";

	public static string PropPreviousTierZombie = "PreviousTier";

	public static string PropTokenManager = "TokenManager";

	public static readonly int itemClass = EntityClass.FromString("item");

	public static readonly int fallingBlockClass = EntityClass.FromString("fallingBlock");

	public static readonly int fallingBlocksClass = EntityClass.FromString("fallingBlocks");

	public static readonly int fallingTreeClass = EntityClass.FromString("fallingTree");

	public static readonly int playerMaleClass = EntityClass.FromString("playerMale");

	public static readonly int playerFemaleClass = EntityClass.FromString("playerFemale");

	public static readonly int playerNewMaleClass = EntityClass.FromString("playerNewMale");

	public static readonly int junkDroneClass = EntityClass.FromString("entityJunkDrone");

	public static FastTags<TagGroup.Global> strongTag = FastTags<TagGroup.Global>.Parse("strong");

	public static FastTags<TagGroup.Global> specialTag = FastTags<TagGroup.Global>.Parse("special");

	public static FastTags<TagGroup.Global> feralTag = FastTags<TagGroup.Global>.Parse("feral,animalFeral");

	public static FastTags<TagGroup.Global> radiatedTag = FastTags<TagGroup.Global>.Parse("radiated");

	public static FastTags<TagGroup.Global> eliteTag = FastTags<TagGroup.Global>.Parse("charged,infernal");

	public static Dictionary<string, Color> sColors = new Dictionary<string, Color>();

	public static DictionarySave<int, EntityClass> list = new DictionarySave<int, EntityClass>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] commaSeparator = new char[] { ',' };

	public DynamicProperties Properties = new DynamicProperties();

	public Type classname;

	public int censorMode;

	public EntityFlags entityFlags;

	public int censorType;

	public string prefabPath;

	public bool IsPrefabCombined;

	public string meshPath;

	public Transform meshFP;

	public string skinTexture;

	public string parentGameObjectName;

	public string entityClassName;

	public EntityClass.UserSpawnType userSpawnType = EntityClass.UserSpawnType.Menu;

	public bool bIsEnemyEntity;

	public bool bIsAnimalEntity;

	public ExplosionData explosionData;

	public Type modelType;

	public float MassKg;

	public float SizeScale;

	public float RagdollOnDeathChance;

	public bool HasRagdoll;

	public string CollidersRagdollAsset;

	public float LookAtAngle;

	public float crouchYOffsetFP;

	public float MaxTurnSpeed;

	public bool RootMotion;

	public bool HasDeathAnim;

	public bool bIsMale;

	public bool bIsChunkObserver;

	public int ExperienceValue;

	public PhysicsBodyLayout PhysicsBody;

	public int DeadBodyHitPoints;

	public List<EntityClass.LootDrop> lootDrops;

	public float LegCrippleScale;

	public float LegCrawlerThreshold;

	public float DismemberMultiplierHead;

	public float DismemberMultiplierArms;

	public float DismemberMultiplierLegs;

	public float LowerLegDismemberThreshold;

	public float LowerLegDismemberBonusChance;

	public float LowerLegDismemberBaseChance;

	public float UpperLegDismemberThreshold;

	public float UpperLegDismemberBonusChance;

	public float UpperLegDismemberBaseChance;

	public float LowerArmDismemberThreshold;

	public float LowerArmDismemberBonusChance;

	public float LowerArmDismemberBaseChance;

	public float UpperArmDismemberThreshold;

	public float UpperArmDismemberBonusChance;

	public float UpperArmDismemberBaseChance;

	public float KnockdownKneelDamageThreshold;

	public float LegsExplosionDamageMultiplier;

	public float ArmsExplosionDamageMultiplier;

	public float ChestExplosionDamageMultiplier;

	public float HeadExplosionDamageMultiplier;

	public float PainResistPerHit;

	public float PainResistPerHitLowHealth;

	public float PainResistPerHitLowHealthPercent;

	public float SearchRadius;

	public float SwimOffset;

	public float SightRange;

	public Vector2 SleeperSightToSenseMin;

	public Vector2 SleeperSightToSenseMax;

	public Vector2 SleeperSightToWakeMin;

	public Vector2 SleeperSightToWakeMax;

	public Vector2 sightLightThreshold;

	public Vector2 NoiseAlert;

	public Vector2 SleeperNoiseToSense;

	public float SleeperNoiseToSenseSoundChance;

	public Vector2 SleeperNoiseToWake;

	public string UMARace;

	public string UMAGeneratedModelName;

	public string[] AltMatNames;

	public string[] MatSwap;

	public EntityClass.ParticleData particleOnSpawn;

	public Vector2 KnockdownKneelStunDuration;

	public float KnockdownProneDamageThreshold;

	public Vector2 KnockdownProneStunDuration;

	public Vector2 KnockdownProneRefillRate;

	public Vector2 KnockdownKneelRefillRate;

	public Vector3 ModelTransformAdjust;

	public string ArchetypeName;

	public string[] AIPackages;

	public bool UseAIPackages;

	public Dictionary<EnumDropEvent, List<Block.SItemDropProb>> itemsToDrop = new EnumDictionary<EnumDropEvent, List<Block.SItemDropProb>>();

	public List<string> Buffs;

	public FastTags<TagGroup.Global> Tags;

	public string NavObject = "";

	public Vector3 NavObjectHeadOffset = Vector3.zero;

	public bool CanBigHead = true;

	public int DanceTypeID;

	public MinEffectController Effects;

	public string PreviousTierZombieName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityClass[] previousTierEntities;

	public EntityClass.EntityTierTypes EntityTier;

	public string onActivateEvent = "";

	public Dictionary<AITokenType, AITokenConfig> TokenManagerConfig;

	public enum CensorModeType
	{
		None,
		ZPrefab,
		Dismemberment,
		ZPrefabAndDismemberment
	}

	public enum UserSpawnType
	{
		None,
		Console,
		Menu
	}

	public struct LootDrop
	{
		public int entityClass;

		public float weight;
	}

	public struct ParticleData
	{
		public string fileName;

		public string shapeMesh;
	}

	public enum EntityTierTypes
	{
		Normal,
		Strong,
		Special,
		Feral,
		Radiated,
		Elite
	}
}
