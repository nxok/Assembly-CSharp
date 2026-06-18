using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Progression
{
	public Progression()
	{
		this.ExpToNextLevel = this.getExpForLevel((float)(this.Level + 1));
	}

	public Progression(EntityAlive _parent)
	{
		this.parent = _parent;
		this.ExpToNextLevel = this.getExpForLevel((float)(this.Level + 1));
		this.SetupData();
	}

	public Dictionary<int, ProgressionValue> GetDict()
	{
		return this.ProgressionValues.Dict;
	}

	public static int CalcId(string _name)
	{
		return Progression.ProgressionNameIds.Add(_name);
	}

	public ProgressionValue GetProgressionValue(int _id)
	{
		return this.ProgressionValues.Get(_id);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getLevelFloat()
	{
		return (float)this.Level + (1f - (float)this.ExpToNextLevel / (float)this.GetExpForNextLevel());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int getExpForLevel(float _level)
	{
		return (int)Math.Min((float)Progression.BaseExpToLevel * Mathf.Pow(Progression.ExpMultiplier, _level), 2.1474836E+09f);
	}

	public int GetLevel()
	{
		return this.Level;
	}

	public int GetExpForNextLevel()
	{
		return this.getExpForLevel((float)Mathf.Clamp(this.Level + 1, 0, Progression.ClampExpCostAtLevel));
	}

	public float GetLevelProgressPercentage()
	{
		return this.getLevelFloat() - (float)this.Level;
	}

	public void ModifyValue(PassiveEffects _effect, ref float _baseVal, ref float _percVal, FastTags<TagGroup.Global> _tags)
	{
		if (_effect == PassiveEffects.AttributeLevel)
		{
			return;
		}
		if (_effect == PassiveEffects.SkillLevel)
		{
			return;
		}
		if (_effect == PassiveEffects.PerkLevel)
		{
			return;
		}
		List<ProgressionValue> list;
		if (this.passiveEffects.TryGetValue(_effect, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				ProgressionValue progressionValue = list[i];
				ProgressionClass progressionClass = progressionValue.ProgressionClass;
				if (progressionClass != null)
				{
					progressionClass.ModifyValue(this.parent, progressionValue, _effect, ref _baseVal, ref _percVal, _tags);
				}
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, PassiveEffects _effect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> _tags)
	{
		if (_effect == PassiveEffects.AttributeLevel)
		{
			return;
		}
		if (_effect == PassiveEffects.SkillLevel)
		{
			return;
		}
		if (_effect == PassiveEffects.PerkLevel)
		{
			return;
		}
		for (int i = 0; i < this.ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = this.ProgressionValueQuickList[i];
			if (progressionValue != null)
			{
				ProgressionClass progressionClass = progressionValue.ProgressionClass;
				if (progressionClass != null && progressionClass.Effects != null && progressionClass.Effects.PassivesIndex != null && progressionClass.Effects.PassivesIndex.Contains(_effect))
				{
					progressionClass.GetModifiedValueData(_modValueSources, _sourceType, this.parent, progressionValue, _effect, ref _base_val, ref _perc_val, _tags);
				}
			}
		}
	}

	public void Update()
	{
		if (this.timer <= 0f)
		{
			this.FireEvent(MinEventTypes.onSelfProgressionUpdate, this.parent.MinEventContext);
			this.timer = 1f;
		}
		else
		{
			this.timer -= Time.deltaTime;
		}
		this.parent.Buffs.SetCustomVar("_expdeficit", (float)this.ExpDeficit, true, CVarOperation.set);
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (this.eventList == null)
		{
			return;
		}
		for (int i = 0; i < this.eventList.Count; i++)
		{
			ProgressionValue progressionValue = this.eventList[i];
			ProgressionClass progressionClass = progressionValue.ProgressionClass;
			_params.ProgressionValue = progressionValue;
			progressionClass.FireEvent(_eventType, _params);
		}
	}

	public int AddLevelExp(int _exp, string _cvarXPName = "_xpOther", Progression.XPTypes _xpType = Progression.XPTypes.Other, bool useBonus = true, bool notifyUI = true, int _instigatorID = -1, ItemValue _itemValue = null)
	{
		if (this.parent as EntityPlayer == null)
		{
			return _exp;
		}
		this.parent.MinEventContext.Instigator = ((_instigatorID == -1) ? null : (this.parent.world.GetEntity(_instigatorID) as EntityAlive));
		float num = (float)_exp;
		if (useBonus)
		{
			if (this.xpFastTags == null)
			{
				this.xpFastTags = new FastTags<TagGroup.Global>[11];
				for (int i = 0; i < 11; i++)
				{
					this.xpFastTags[i] = FastTags<TagGroup.Global>.Parse(((Progression.XPTypes)i).ToStringCached<Progression.XPTypes>());
				}
			}
			num *= Progression.XPGain;
			PassiveEffects passiveEffects = PassiveEffects.PlayerExpGain;
			float num2 = num;
			EntityAlive entityAlive = this.parent;
			FastTags<TagGroup.Global> fastTags = this.xpFastTags[(int)_xpType];
			num = EffectManager.GetValue(passiveEffects, _itemValue, num2, entityAlive, null, fastTags, true, true, true, true, true, 1, true, false);
		}
		if (num > 214748370f)
		{
			num = 214748370f;
		}
		if (_xpType != Progression.XPTypes.Debug)
		{
			GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.XpEarnedBy, _xpType.ToStringCached<Progression.XPTypes>(), num, true, GameSparksCollector.GSDataCollection.SessionUpdates);
		}
		int level = this.Level;
		EntityPlayerLocal entityPlayerLocal = this.parent as EntityPlayerLocal;
		if (entityPlayerLocal && (Progression.ShowXPType == Progression.ShowXPTypes.All || Progression.ShowXPType == Progression.ShowXPTypes.NotificationsOnly))
		{
			entityPlayerLocal.PlayerUI.xui.CollectedItemList.AddIconNotification("ui_game_symbol_xp", (int)num, false);
		}
		this.AddLevelExpRecursive((int)num, _cvarXPName, notifyUI);
		if (this.Level != level)
		{
			Log.Out("{0} made level {1} (was {2}), exp for next level {3}", new object[]
			{
				this.parent.EntityName,
				this.Level,
				level,
				this.ExpToNextLevel
			});
		}
		return (int)num;
	}

	public void OnDeath()
	{
	}

	public void AddXPDeficit()
	{
		this.ExpDeficit += (int)((float)this.GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitPerDeathPercentage, null, 0.1f, this.parent, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		this.ExpDeficit = Mathf.Clamp(this.ExpDeficit, 0, (int)((float)this.GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitMaxPercentage, null, 0.5f, this.parent, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)));
		this.ExpDeficitGained = true;
	}

	public void OnRespawnFromDeath()
	{
		if (!this.ExpDeficitGained)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = this.parent as EntityPlayerLocal;
		if (this.ExpDeficit == (int)((float)this.GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitMaxPercentage, null, 0.5f, this.parent, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)))
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttResurrectMaxXPLost", false, null), false, false, 0f);
		}
		else
		{
			GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttResurrectXPLost", false, null), EffectManager.GetValue(PassiveEffects.ExpDeficitPerDeathPercentage, null, 0.1f, this.parent, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * 100f), false, false, 0f);
		}
		this.ExpDeficitGained = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddLevelExpRecursive(int exp, string _cvarXPName, bool notifyUI = true)
	{
		if (this.Level >= Progression.MaxLevel)
		{
			this.Level = Progression.MaxLevel;
			return;
		}
		this.parent.Buffs.IncrementCustomVar(_cvarXPName, (float)exp);
		int num;
		if (this.ExpDeficit > 0)
		{
			num = exp - this.ExpDeficit;
			this.ExpDeficit -= exp;
			this.ExpDeficit = Mathf.Clamp(this.ExpDeficit, 0, (int)((float)this.GetExpForNextLevel() * EffectManager.GetValue(PassiveEffects.ExpDeficitMaxPercentage, null, 0.5f, this.parent, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)));
		}
		else
		{
			num = exp - this.ExpToNextLevel;
			this.ExpToNextLevel -= exp;
		}
		EntityPlayerLocal entityPlayerLocal = this.parent as EntityPlayerLocal;
		if (this.ExpDeficit <= 0)
		{
			int level = this.Level;
			if (this.ExpToNextLevel <= 0)
			{
				this.Level++;
				if (this.Level % Progression.SkillPointsGainRate == 0)
				{
					if (Progression.SkillPointMultiplier == 0f)
					{
						this.SkillPoints += Progression.SkillPointsPerLevel;
					}
					else
					{
						this.SkillPoints += (int)Math.Min((float)Progression.SkillPointsPerLevel * Mathf.Pow(Progression.SkillPointMultiplier, (float)this.Level), 2.1474836E+09f);
					}
				}
				if (entityPlayerLocal)
				{
					GameSparksCollector.PlayerLevelUp(entityPlayerLocal, this.Level);
				}
				this.ExpToNextLevel = this.GetExpForNextLevel();
			}
			if ((this.ExpToNextLevel > num || this.Level == Progression.MaxLevel) && level != this.Level && entityPlayerLocal && notifyUI)
			{
				GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttLevelUp", false, null), this.Level.ToString(), this.SkillPoints), string.Empty, "levelupplayer", null, false, false, 0f);
			}
		}
		if (num > 0)
		{
			this.AddLevelExpRecursive(num, _cvarXPName, true);
		}
	}

	public void SpendSkillPoints(int _points, string _progressionName)
	{
		ProgressionValue progressionValue = this.GetProgressionValue(_progressionName);
		if (progressionValue != null && progressionValue.ProgressionClass.CurrencyType == ProgressionCurrencyType.SP)
		{
			this.addProgressionCurrency(_points, progressionValue);
		}
	}

	public ProgressionValue GetProgressionValue(string _progressionName)
	{
		return this.ProgressionValues.Get(_progressionName);
	}

	public void GetPerkList(List<ProgressionValue> perkList, string _skillName)
	{
		perkList.Clear();
		for (int i = 0; i < this.ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = this.ProgressionValueQuickList[i];
			if ((progressionValue.ProgressionClass.Type == ProgressionType.Perk || progressionValue.ProgressionClass.Type == ProgressionType.Book) && progressionValue.ProgressionClass.Parent.Name == _skillName)
			{
				perkList.Add(progressionValue);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addProgressionCurrency(int _currencyAmount, ProgressionValue _pv)
	{
		if (_pv == null)
		{
			return;
		}
		ProgressionClass progressionClass = _pv.ProgressionClass;
		if (_pv.Level >= progressionClass.MaxLevel)
		{
			if (_pv.Level > progressionClass.MaxLevel)
			{
				_pv.Level = progressionClass.MaxLevel;
			}
			return;
		}
		if (progressionClass.Type == ProgressionType.Skill)
		{
			_currencyAmount = (int)EffectManager.GetValue(PassiveEffects.SkillExpGain, null, (float)_currencyAmount, this.parent, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		int num = _currencyAmount - _pv.CostForNextLevel;
		_pv.CostForNextLevel -= _currencyAmount;
		if (_pv.CostForNextLevel <= 0)
		{
			int level = _pv.Level;
			_pv.Level = level + 1;
			_pv.CostForNextLevel = progressionClass.CalculatedCostForLevel(_pv.Level + 1);
		}
		if (num > 0)
		{
			this.addProgressionCurrency(num, _pv);
		}
	}

	public byte[] ToBytes(bool _IsNetwork = false)
	{
		byte[] array = null;
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
		{
			MemoryStream memoryStream = new MemoryStream();
			pooledBinaryWriter.SetBaseStream(memoryStream);
			this.Write(pooledBinaryWriter, _IsNetwork);
			array = memoryStream.ToArray();
			memoryStream.Dispose();
		}
		return array;
	}

	public void Write(BinaryWriter _bw, bool _IsNetwork = false)
	{
		_bw.Write(3);
		_bw.Write((ushort)this.Level);
		_bw.Write(this.ExpToNextLevel);
		_bw.Write((ushort)this.SkillPoints);
		int count = this.ProgressionValues.Count;
		_bw.Write(count);
		foreach (KeyValuePair<int, ProgressionValue> keyValuePair in this.ProgressionValues.Dict)
		{
			keyValuePair.Value.Write(_bw, _IsNetwork);
		}
		_bw.Write(this.ExpDeficit);
	}

	public static Progression FromBytes(byte[] data, EntityAlive _parent)
	{
		Progression progression = null;
		try
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				MemoryStream memoryStream = new MemoryStream(data);
				pooledBinaryReader.SetBaseStream(memoryStream);
				progression = Progression.Read(pooledBinaryReader, _parent);
				memoryStream.Dispose();
			}
		}
		catch
		{
			progression = null;
		}
		return progression;
	}

	public static Progression Read(BinaryReader _br, EntityAlive _parent)
	{
		Progression progression = _parent.Progression;
		if (progression == null)
		{
			Log.Warning("Progression Read {0}, new", new object[] { _parent });
			progression = new Progression(_parent);
			_parent.Progression = progression;
		}
		byte b = _br.ReadByte();
		progression.Level = (int)_br.ReadUInt16();
		progression.ExpToNextLevel = _br.ReadInt32();
		progression.SkillPoints = (int)_br.ReadUInt16();
		int num = _br.ReadInt32();
		ProgressionValue progressionValue = new ProgressionValue();
		for (int i = 0; i < num; i++)
		{
			progressionValue.Read(_br);
			if (Progression.ProgressionClasses.ContainsKey(progressionValue.Name))
			{
				ProgressionValue progressionValue2 = progression.ProgressionValues.Get(progressionValue.Name);
				if (progressionValue2 != null)
				{
					progressionValue2.CopyFrom(progressionValue);
				}
				else
				{
					Log.Error("ProgressionValues missing {0}", new object[] { progressionValue.Name });
					progressionValue2 = new ProgressionValue();
					progressionValue2.CopyFrom(progressionValue);
					progression.ProgressionValues.Add(progressionValue.Name, progressionValue2);
				}
			}
		}
		if (b > 2)
		{
			progression.ExpDeficit = _br.ReadInt32();
		}
		progression.SetupData();
		return progression;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupData()
	{
		foreach (KeyValuePair<string, ProgressionClass> keyValuePair in Progression.ProgressionClasses)
		{
			string name = keyValuePair.Value.Name;
			if (!this.ProgressionValues.Contains(name))
			{
				ProgressionValue progressionValue = new ProgressionValue(name)
				{
					Level = keyValuePair.Value.MinLevel,
					CostForNextLevel = keyValuePair.Value.CalculatedCostForLevel(this.Level + 1)
				};
				this.ProgressionValues.Add(name, progressionValue);
			}
		}
		this.ProgressionValueQuickList.Clear();
		foreach (KeyValuePair<int, ProgressionValue> keyValuePair2 in this.ProgressionValues.Dict)
		{
			this.ProgressionValueQuickList.Add(keyValuePair2.Value);
		}
		this.eventList.Clear();
		this.passiveEffects.Clear();
		for (int i = 0; i < this.ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue2 = this.ProgressionValueQuickList[i];
			ProgressionClass progressionClass = progressionValue2.ProgressionClass;
			if (progressionClass.HasEvents())
			{
				this.eventList.Add(progressionValue2);
			}
			MinEffectController effects = progressionClass.Effects;
			if (effects != null)
			{
				HashSet<PassiveEffects> passivesIndex = effects.PassivesIndex;
				if (passivesIndex != null)
				{
					foreach (PassiveEffects passiveEffects in passivesIndex)
					{
						List<ProgressionValue> list;
						if (!this.passiveEffects.TryGetValue(passiveEffects, out list))
						{
							list = new List<ProgressionValue>();
							this.passiveEffects.Add(passiveEffects, list);
						}
						list.Add(progressionValue2);
					}
				}
			}
		}
	}

	public void ClearProgressionClassLinks()
	{
		if (this.ProgressionValueQuickList == null)
		{
			return;
		}
		foreach (ProgressionValue progressionValue in this.ProgressionValueQuickList)
		{
			if (progressionValue != null)
			{
				progressionValue.ClearProgressionClassLink();
			}
		}
		this.SetupData();
	}

	public static void Cleanup()
	{
		if (Progression.ProgressionClasses != null)
		{
			Progression.ProgressionClasses.Clear();
		}
	}

	public void ResetProgression(bool _resetSkills = true, bool _resetBooks = false, bool _resetCrafting = false)
	{
		int num = 0;
		int i = 0;
		while (i < this.ProgressionValueQuickList.Count)
		{
			ProgressionValue progressionValue = this.ProgressionValueQuickList[i];
			ProgressionClass progressionClass = progressionValue.ProgressionClass;
			if (!progressionClass.IsBook)
			{
				goto IL_0034;
			}
			if (_resetBooks)
			{
				progressionValue.Level = 0;
				goto IL_0034;
			}
			IL_00C3:
			i++;
			continue;
			IL_0034:
			if (progressionClass.IsCrafting)
			{
				if (!_resetCrafting)
				{
					goto IL_00C3;
				}
				progressionValue.Level = 1;
			}
			if (!_resetSkills)
			{
				goto IL_00C3;
			}
			if (progressionClass.IsAttribute)
			{
				if (progressionValue.Level > 1)
				{
					for (int j = 2; j <= progressionValue.Level; j++)
					{
						num += progressionClass.CalculatedCostForLevel(j);
					}
					progressionValue.Level = 1;
					goto IL_00C3;
				}
				goto IL_00C3;
			}
			else
			{
				if (progressionClass.IsPerk && progressionValue.Level > 0)
				{
					for (int k = 1; k <= progressionValue.Level; k++)
					{
						num += progressionClass.CalculatedCostForLevel(k);
					}
					progressionValue.Level = 0;
					goto IL_00C3;
				}
				goto IL_00C3;
			}
		}
		EntityPlayerLocal entityPlayerLocal = this.parent as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			entityPlayerLocal.PlayerUI.xui.Recipes.RefreshTrackedRecipe();
		}
		this.SkillPoints += num;
	}

	public void RefreshPerks(string attribute)
	{
		for (int i = 0; i < this.ProgressionValueQuickList.Count; i++)
		{
			ProgressionValue progressionValue = this.ProgressionValueQuickList[i];
			ProgressionClass progressionClass = progressionValue.ProgressionClass;
			if (progressionClass.IsPerk && (attribute == "" || attribute.EqualsCaseInsensitive(progressionClass.ParentName)))
			{
				progressionValue.CalculatedLevel(this.parent);
			}
		}
	}

	public void UpdateForSandbox()
	{
		foreach (ProgressionClass progressionClass in Progression.ProgressionClasses.Values)
		{
			if (progressionClass.DisplayDataList != null)
			{
				progressionClass.UpdateForSandbox();
			}
		}
	}

	public const byte cVersion = 3;

	public static int BaseExpToLevel;

	public static int ClampExpCostAtLevel;

	public static float ExpMultiplier;

	public static int MaxLevel;

	public static int SkillPointsPerLevelXML;

	public static int SkillPointsGainRate;

	public static int SkillPointsPerLevel;

	public static float SkillPointMultiplier;

	public static Dictionary<string, ProgressionClass> ProgressionClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DictionaryNameIdMapping ProgressionNameIds = new DictionaryNameIdMapping();

	public bool bProgressionStatsChanged;

	public int ExpToNextLevel;

	public int ExpDeficit;

	public int Level = 1;

	public int SkillPoints;

	public static float XPGain = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ExpDeficitGained;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryNameId<ProgressionValue> ProgressionValues = new DictionaryNameId<ProgressionValue>(Progression.ProgressionNameIds);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ProgressionValue> ProgressionValueQuickList = new List<ProgressionValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ProgressionValue> eventList = new List<ProgressionValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<PassiveEffects, List<ProgressionValue>> passiveEffects = new Dictionary<PassiveEffects, List<ProgressionValue>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global>[] xpFastTags;

	public static Progression.ShowXPTypes ShowXPType = Progression.ShowXPTypes.All;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timer = 1f;

	public enum XPTypes
	{
		Kill,
		Harvesting,
		Upgrading,
		Crafting,
		Selling,
		Quest,
		Looting,
		Party,
		Other,
		Repairing,
		Debug,
		Max
	}

	public enum ShowXPTypes
	{
		All,
		BarOnly,
		NotificationsOnly,
		None
	}
}
