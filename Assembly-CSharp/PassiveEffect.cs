using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

public class PassiveEffect
{
	public void ModifyValue(EntityAlive _ea, float _level, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>), int _stackEffectMultiplier = 1)
	{
		if (this.CVarValues != null)
		{
			for (int i = 0; i < this.CVarValues.Length; i++)
			{
				string text = this.CVarValues[i];
				if (text != null)
				{
					if (_ea.Buffs.HasCustomVar(text))
					{
						this.Values[i] = _ea.Buffs.GetCustomVar(text);
					}
					else
					{
						_ea.Buffs.AddCustomVar(text, 0f);
						this.Values[i] = 0f;
					}
				}
			}
		}
		PassiveEffect.ModValue(this.Modifier, _level, ref _base_value, ref _perc_value, this.Levels, this.Values, (float)_stackEffectMultiplier, 0);
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSource, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, MinEffectController.SourceParentType _parentType, EntityAlive _ea, float _level, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>), int _stackEffectMultiplier = 1, object _parentPointer = null)
	{
		float num = 0f;
		float num2 = 1f;
		if (this.CVarValues != null)
		{
			for (int i = 0; i < this.CVarValues.Length; i++)
			{
				string text = this.CVarValues[i];
				if (text != null)
				{
					if (_ea.Buffs.HasCustomVar(text))
					{
						this.Values[i] = _ea.Buffs.GetCustomVar(text);
					}
					else
					{
						Log.Out("PassiveEffects: CVar '{0}' was not found in custom variable dictionary for entity '{1}'", new object[] { text, _ea.EntityName });
					}
				}
			}
		}
		PassiveEffect.ModValue(this.Modifier, _level, ref num, ref num2, this.Levels, this.Values, (float)_stackEffectMultiplier, 0);
		if (num == 0f && num2 == 1f)
		{
			return;
		}
		EffectManager.ModifierValuesAndSources modifierValuesAndSources = new EffectManager.ModifierValuesAndSources
		{
			ValueSource = _sourceType,
			ParentType = _parentType,
			Source = _parentPointer,
			ModifierType = this.Modifier,
			Tags = this.Tags
		};
		if (this.Modifier.ToStringCached<PassiveEffect.ValueModifierTypes>().Contains("base"))
		{
			modifierValuesAndSources.Value = num;
		}
		else
		{
			modifierValuesAndSources.Value = num2;
		}
		_modValueSource.Add(modifierValuesAndSources);
	}

	public bool RequirementsMet(MinEventParams _params)
	{
		return this.hasMatchingTag(_params.Tags) && (this.Requirements == null || this.Requirements.IsValid(_params));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasMatchingTag(FastTags<TagGroup.Global> _tags)
	{
		if (_tags.IsEmpty && !this.Tags.IsEmpty)
		{
			return false;
		}
		if (this.MatchAnyTags)
		{
			if (this.Tags.IsEmpty)
			{
				return !this.InvertTagCheck;
			}
			if (!this.InvertTagCheck)
			{
				return _tags.Test_AnySet(this.Tags);
			}
			return !_tags.Test_AnySet(this.Tags);
		}
		else
		{
			if (!this.InvertTagCheck)
			{
				return _tags.Test_AllSet(this.Tags);
			}
			return !_tags.Test_AllSet(this.Tags);
		}
	}

	public static PassiveEffect ParsePassiveEffect(XElement _element)
	{
		string attribute = _element.GetAttribute("name");
		if (attribute.Length == 0)
		{
			return null;
		}
		string text = _element.GetAttribute("modifier");
		if (text.Length == 0)
		{
			text = _element.GetAttribute("operation");
			if (text.Length == 0)
			{
				return null;
			}
		}
		string text2 = _element.GetAttribute("value");
		if (text2.Length == 0)
		{
			return null;
		}
		if (text2[0] == '^')
		{
			text2 = EntityClassesFromXml.sReplacePassiveEffects[text2];
		}
		PassiveEffect passiveEffect = new PassiveEffect();
		passiveEffect.Type = EnumUtils.Parse<PassiveEffects>(attribute, true);
		if (passiveEffect.Type == PassiveEffects.None)
		{
			return null;
		}
		if (text2.Contains(","))
		{
			string[] array = text2.Split(',', StringSplitOptions.None);
			passiveEffect.Values = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				float num;
				if (array[i].StartsWith("@"))
				{
					if (passiveEffect.CVarValues == null)
					{
						passiveEffect.CVarValues = new string[array.Length];
					}
					passiveEffect.CVarValues[i] = array[i].Trim().Remove(0, 1);
				}
				else if (StringParsers.TryParseFloat(array[i], out num))
				{
					passiveEffect.Values[i] = num;
				}
			}
		}
		else if (text2.StartsWith("@"))
		{
			passiveEffect.CVarValues = new string[] { text2.Trim().Remove(0, 1) };
			passiveEffect.Values = new float[1];
		}
		else
		{
			passiveEffect.Values = new float[] { StringParsers.ParseFloat(text2, 0, -1, NumberStyles.Any) };
		}
		if (passiveEffect.CVarValues != null)
		{
			for (int j = 0; j < passiveEffect.CVarValues.Length; j++)
			{
				string text3 = passiveEffect.CVarValues[j];
				if (text3 != null && text3.Contains("@"))
				{
					Log.Error("CVar reference contains an '@' symbol! This will break calls to it.");
				}
			}
		}
		string text4 = _element.GetAttribute("level");
		if (text4.Length > 0)
		{
			if (text4.Contains(","))
			{
				string[] array2 = text4.Split(',', StringSplitOptions.None);
				passiveEffect.Levels = new float[array2.Length];
				for (int k = 0; k < array2.Length; k++)
				{
					passiveEffect.Levels[k] = StringParsers.ParseFloat(array2[k], 0, -1, NumberStyles.Any);
				}
			}
			else
			{
				passiveEffect.Levels = new float[] { StringParsers.ParseFloat(text4, 0, -1, NumberStyles.Any) };
			}
		}
		else if ((text4 = _element.GetAttribute("tier")).Length > 0)
		{
			if (text4.Contains(","))
			{
				string[] array3 = text4.Split(',', StringSplitOptions.None);
				passiveEffect.Levels = new float[array3.Length];
				for (int l = 0; l < array3.Length; l++)
				{
					passiveEffect.Levels[l] = StringParsers.ParseFloat(array3[l], 0, -1, NumberStyles.Any);
				}
			}
			else
			{
				passiveEffect.Levels = new float[] { StringParsers.ParseFloat(text4, 0, -1, NumberStyles.Any) };
			}
		}
		else if ((text4 = _element.GetAttribute("duration")).Length > 0)
		{
			if (text4.Contains(","))
			{
				string[] array4 = text4.Split(',', StringSplitOptions.None);
				passiveEffect.Levels = new float[array4.Length];
				for (int m = 0; m < array4.Length; m++)
				{
					passiveEffect.Levels[m] = StringParsers.ParseFloat(array4[m], 0, -1, NumberStyles.Any);
				}
			}
			else
			{
				passiveEffect.Levels = new float[] { StringParsers.ParseFloat(text4, 0, -1, NumberStyles.Any) };
			}
		}
		string text5 = _element.GetAttribute("tags");
		if (text5.Length > 0)
		{
			passiveEffect.Tags = FastTags<TagGroup.Global>.Parse(text5);
		}
		else
		{
			text5 = _element.GetAttribute("tag");
			if (text5.Length > 0)
			{
				passiveEffect.Tags = FastTags<TagGroup.Global>.Parse(text5);
			}
		}
		if (_element.HasAttribute("match_all_tags"))
		{
			passiveEffect.MatchAnyTags = false;
		}
		if (_element.HasAttribute("invert_tag_check"))
		{
			passiveEffect.InvertTagCheck = true;
		}
		passiveEffect.Modifier = EnumUtils.Parse<PassiveEffect.ValueModifierTypes>(text, false);
		passiveEffect.Requirements = RequirementBase.ParseRequirementGroup(_element);
		return passiveEffect;
	}

	public static PassiveEffect CreateEmptyPassiveEffect(PassiveEffects type)
	{
		return new PassiveEffect
		{
			Type = type,
			Modifier = PassiveEffect.ValueModifierTypes.perc_add,
			Values = new float[1]
		};
	}

	public void AddColoredInfoStrings(ref List<string> _infoList, float _level = -1f)
	{
		if (this.Levels == null)
		{
			_infoList.Add(this.GetDisplayValue(0f, 0f, 1f, 1f));
			return;
		}
		if (_level == -1f)
		{
			for (int i = 0; i < this.Levels.Length; i++)
			{
				_infoList.Add(this.GetDisplayValue(this.Levels[i], 0f, 1f, 1f));
			}
			return;
		}
		_infoList.Add(this.GetDisplayValue(_level, 0f, 1f, 1f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ModValue(PassiveEffect.ValueModifierTypes _modifier, float _level, ref float _base_value, ref float _perc_value, float[] _levels, float[] _values, float _multiplier = 1f, int _seed = 0)
	{
		if (_levels != null)
		{
			if (_values != null)
			{
				if (_values.Length == _levels.Length)
				{
					if (_levels.Length >= 2)
					{
						int i = _levels.Length - 1;
						while (i > 0)
						{
							if (PassiveEffect.InLevelRange(_level, _levels[i - 1], _levels[i]))
							{
								switch (_modifier)
								{
								case PassiveEffect.ValueModifierTypes.base_set:
									_base_value = Mathf.Lerp(_values[i - 1], _values[i], (_level - _levels[i - 1]) / (_levels[i] - _levels[i - 1]));
									return;
								case PassiveEffect.ValueModifierTypes.base_add:
									_base_value += Mathf.Lerp(_values[i - 1], _values[i], (_level - _levels[i - 1]) / (_levels[i] - _levels[i - 1]));
									return;
								case PassiveEffect.ValueModifierTypes.base_subtract:
									_base_value -= Mathf.Lerp(_values[i - 1], _values[i], (_level - _levels[i - 1]) / (_levels[i] - _levels[i - 1]));
									return;
								case PassiveEffect.ValueModifierTypes.perc_set:
									_perc_value = Mathf.Lerp(_values[i - 1], _values[i], (_level - _levels[i - 1]) / (_levels[i] - _levels[i - 1]));
									return;
								case PassiveEffect.ValueModifierTypes.perc_add:
									_perc_value += Mathf.Lerp(_values[i - 1], _values[i], (_level - _levels[i - 1]) / (_levels[i] - _levels[i - 1]));
									return;
								case PassiveEffect.ValueModifierTypes.perc_subtract:
									_perc_value -= Mathf.Lerp(_values[i - 1], _values[i], (_level - _levels[i - 1]) / (_levels[i] - _levels[i - 1]));
									return;
								default:
									return;
								}
							}
							else
							{
								i--;
							}
						}
						return;
					}
					if (_levels.Length >= 1 && Mathf.FloorToInt(_level) == Mathf.FloorToInt(_levels[0]))
					{
						switch (_modifier)
						{
						case PassiveEffect.ValueModifierTypes.base_set:
							_base_value = _values[0];
							return;
						case PassiveEffect.ValueModifierTypes.base_add:
							_base_value += _values[0];
							return;
						case PassiveEffect.ValueModifierTypes.base_subtract:
							_base_value -= _values[0];
							return;
						case PassiveEffect.ValueModifierTypes.perc_set:
							_perc_value = _values[0];
							return;
						case PassiveEffect.ValueModifierTypes.perc_add:
							_perc_value += _values[0];
							return;
						case PassiveEffect.ValueModifierTypes.perc_subtract:
							_perc_value -= _values[0];
							return;
						default:
							return;
						}
					}
				}
				else if (_levels.Length == 1 && _values.Length == 2 && Mathf.FloorToInt(_level) == Mathf.FloorToInt(_levels[0]))
				{
					if (MinEventParams.CachedEventParam.Seed == 0)
					{
						switch (_modifier)
						{
						case PassiveEffect.ValueModifierTypes.base_set:
							_base_value = (_values[0] + _values[1]) * 0.5f;
							return;
						case PassiveEffect.ValueModifierTypes.base_add:
							_base_value += (_values[0] + _values[1]) * 0.5f;
							return;
						case PassiveEffect.ValueModifierTypes.base_subtract:
							_base_value -= (_values[0] + _values[1]) * 0.5f;
							return;
						case PassiveEffect.ValueModifierTypes.perc_set:
							_perc_value = (_values[0] + _values[1]) * 0.5f;
							return;
						case PassiveEffect.ValueModifierTypes.perc_add:
							_perc_value += (_values[0] + _values[1]) * 0.5f;
							return;
						case PassiveEffect.ValueModifierTypes.perc_subtract:
							_perc_value -= (_values[0] + _values[1]) * 0.5f;
							return;
						default:
							return;
						}
					}
					else
					{
						GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
						switch (_modifier)
						{
						case PassiveEffect.ValueModifierTypes.base_set:
							_base_value = tempGameRandom.RandomRange(_values[0], _values[1]);
							return;
						case PassiveEffect.ValueModifierTypes.base_add:
							_base_value += tempGameRandom.RandomRange(_values[0], _values[1]);
							return;
						case PassiveEffect.ValueModifierTypes.base_subtract:
							_base_value -= tempGameRandom.RandomRange(_values[0], _values[1]);
							return;
						case PassiveEffect.ValueModifierTypes.perc_set:
							_perc_value = tempGameRandom.RandomRange(_values[0], _values[1]);
							return;
						case PassiveEffect.ValueModifierTypes.perc_add:
							_perc_value += tempGameRandom.RandomRange(_values[0], _values[1]);
							return;
						case PassiveEffect.ValueModifierTypes.perc_subtract:
							_perc_value -= tempGameRandom.RandomRange(_values[0], _values[1]);
							return;
						default:
							return;
						}
					}
				}
				else if (_values.Length == 1 && _levels.Length == 2 && PassiveEffect.InLevelRange(_level, _levels[0], _levels[1]))
				{
					switch (_modifier)
					{
					case PassiveEffect.ValueModifierTypes.base_set:
						_base_value = _values[0];
						return;
					case PassiveEffect.ValueModifierTypes.base_add:
						_base_value += _values[0];
						return;
					case PassiveEffect.ValueModifierTypes.base_subtract:
						_base_value -= _values[0];
						return;
					case PassiveEffect.ValueModifierTypes.perc_set:
						_perc_value = _values[0];
						return;
					case PassiveEffect.ValueModifierTypes.perc_add:
						_perc_value += _values[0];
						return;
					case PassiveEffect.ValueModifierTypes.perc_subtract:
						_perc_value -= _values[0];
						return;
					default:
						return;
					}
				}
			}
		}
		else if (_values != null)
		{
			if (_values.Length == 1)
			{
				switch (_modifier)
				{
				case PassiveEffect.ValueModifierTypes.base_set:
					_base_value = _values[0];
					return;
				case PassiveEffect.ValueModifierTypes.base_add:
					_base_value += _values[0];
					return;
				case PassiveEffect.ValueModifierTypes.base_subtract:
					_base_value -= _values[0];
					return;
				case PassiveEffect.ValueModifierTypes.perc_set:
					_perc_value = _values[0];
					return;
				case PassiveEffect.ValueModifierTypes.perc_add:
					_perc_value += _values[0];
					return;
				case PassiveEffect.ValueModifierTypes.perc_subtract:
					_perc_value -= _values[0];
					return;
				default:
					return;
				}
			}
			else if (_values.Length == 2)
			{
				if (MinEventParams.CachedEventParam.Seed == 0)
				{
					switch (_modifier)
					{
					case PassiveEffect.ValueModifierTypes.base_set:
						_base_value = (_values[0] + _values[1]) * 0.5f;
						return;
					case PassiveEffect.ValueModifierTypes.base_add:
						_base_value += (_values[0] + _values[1]) * 0.5f;
						return;
					case PassiveEffect.ValueModifierTypes.base_subtract:
						_base_value -= (_values[0] + _values[1]) * 0.5f;
						return;
					case PassiveEffect.ValueModifierTypes.perc_set:
						_perc_value = (_values[0] + _values[1]) * 0.5f;
						return;
					case PassiveEffect.ValueModifierTypes.perc_add:
						_perc_value += (_values[0] + _values[1]) * 0.5f;
						return;
					case PassiveEffect.ValueModifierTypes.perc_subtract:
						_perc_value -= (_values[0] + _values[1]) * 0.5f;
						return;
					default:
						return;
					}
				}
				else
				{
					GameRandom tempGameRandom2 = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
					switch (_modifier)
					{
					case PassiveEffect.ValueModifierTypes.base_set:
						_base_value = tempGameRandom2.RandomRange(_values[0], _values[1]);
						return;
					case PassiveEffect.ValueModifierTypes.base_add:
						_base_value += tempGameRandom2.RandomRange(_values[0], _values[1]);
						return;
					case PassiveEffect.ValueModifierTypes.base_subtract:
						_base_value -= tempGameRandom2.RandomRange(_values[0], _values[1]);
						return;
					case PassiveEffect.ValueModifierTypes.perc_set:
						_perc_value = tempGameRandom2.RandomRange(_values[0], _values[1]);
						return;
					case PassiveEffect.ValueModifierTypes.perc_add:
						_perc_value += tempGameRandom2.RandomRange(_values[0], _values[1]);
						return;
					case PassiveEffect.ValueModifierTypes.perc_subtract:
						_perc_value -= tempGameRandom2.RandomRange(_values[0], _values[1]);
						break;
					default:
						return;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool InLevelRange(float _level, float _min, float _max)
	{
		return _level >= _min && _level <= _max;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetDisplayValue(float _level, float _base_value = 0f, float _perc_value = 1f, float _multiplier = 1f)
	{
		if (this.Levels != null)
		{
			if (this.Values != null)
			{
				if (this.Values.Length == this.Levels.Length)
				{
					if (this.Levels.Length >= 2)
					{
						for (int i = 0; i < this.Levels.Length - 1; i += 2)
						{
							if (PassiveEffect.InLevelRange(_level, this.Levels[i], this.Levels[i + 1]))
							{
								PassiveEffect.ValueModifierTypes valueModifierTypes = this.Modifier;
								if (valueModifierTypes <= PassiveEffect.ValueModifierTypes.base_subtract)
								{
									_base_value = Mathf.Lerp(this.Values[i], this.Values[i + 1], (_level - this.Levels[i]) / (this.Levels[i + 1] - this.Levels[i]));
									return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_base_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
								}
								if (valueModifierTypes - PassiveEffect.ValueModifierTypes.perc_set <= 2)
								{
									_perc_value = Mathf.Lerp(this.Values[i], this.Values[i + 1], (_level - this.Levels[i]) / (this.Levels[i + 1] - this.Levels[i]));
									return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_perc_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
								}
							}
						}
					}
					else if (this.Levels.Length >= 1 && _level == this.Levels[0])
					{
						PassiveEffect.ValueModifierTypes valueModifierTypes = this.Modifier;
						if (valueModifierTypes <= PassiveEffect.ValueModifierTypes.base_subtract)
						{
							_base_value = this.Values[0];
							return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_base_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
						}
						if (valueModifierTypes - PassiveEffect.ValueModifierTypes.perc_set <= 2)
						{
							_perc_value = this.Values[0];
							return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_perc_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
						}
					}
				}
				else if (this.Values.Length == 2 && this.Levels.Length == 1)
				{
					GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
					PassiveEffect.ValueModifierTypes valueModifierTypes = this.Modifier;
					if (valueModifierTypes <= PassiveEffect.ValueModifierTypes.base_subtract)
					{
						_base_value = ((MinEventParams.CachedEventParam.Seed != 0) ? tempGameRandom.RandomRange(this.Values[0], this.Values[1]) : ((this.Values[0] + this.Values[1]) * 0.5f));
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_base_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
					}
					if (valueModifierTypes - PassiveEffect.ValueModifierTypes.perc_set <= 2)
					{
						_perc_value = ((MinEventParams.CachedEventParam.Seed != 0) ? tempGameRandom.RandomRange(this.Values[0], this.Values[1]) : ((this.Values[0] + this.Values[1]) * 0.5f));
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_perc_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
					}
				}
				else if (this.Values.Length == 1 && this.Levels.Length == 2 && PassiveEffect.InLevelRange(_level, this.Levels[0], this.Levels[1]))
				{
					PassiveEffect.ValueModifierTypes valueModifierTypes = this.Modifier;
					if (valueModifierTypes <= PassiveEffect.ValueModifierTypes.base_subtract)
					{
						_base_value = this.Values[0];
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_base_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
					}
					if (valueModifierTypes - PassiveEffect.ValueModifierTypes.perc_set <= 2)
					{
						_perc_value = this.Values[0];
						return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_perc_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
					}
				}
			}
		}
		else if (this.Values != null)
		{
			if (this.Values.Length == 1)
			{
				PassiveEffect.ValueModifierTypes valueModifierTypes = this.Modifier;
				if (valueModifierTypes <= PassiveEffect.ValueModifierTypes.base_subtract)
				{
					_base_value = this.Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_base_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
				}
				if (valueModifierTypes - PassiveEffect.ValueModifierTypes.perc_set <= 2)
				{
					_perc_value = this.Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_perc_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
				}
			}
			else if (this.Values.Length == 2)
			{
				GameRandom tempGameRandom2 = GameRandomManager.Instance.GetTempGameRandom(MinEventParams.CachedEventParam.Seed);
				PassiveEffect.ValueModifierTypes valueModifierTypes = this.Modifier;
				if (valueModifierTypes <= PassiveEffect.ValueModifierTypes.base_subtract)
				{
					_base_value = tempGameRandom2.RandomRange(this.Values[0], this.Values[1]);
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_base_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
				}
				if (valueModifierTypes - PassiveEffect.ValueModifierTypes.perc_set <= 2)
				{
					_perc_value = tempGameRandom2.RandomRange(this.Values[0], this.Values[1]);
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_perc_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
				}
			}
			else
			{
				PassiveEffect.ValueModifierTypes valueModifierTypes = this.Modifier;
				if (valueModifierTypes <= PassiveEffect.ValueModifierTypes.base_subtract)
				{
					_base_value = this.Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_base_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.base_add) ? "+" : "", _base_value.ToCultureInvariantString("0.0"));
				}
				if (valueModifierTypes - PassiveEffect.ValueModifierTypes.perc_set <= 2)
				{
					_perc_value = this.Values[0];
					return string.Format("{0}: [REPLACE_COLOR]{1}{2}%[-]\n", this.Type.ToStringCached<PassiveEffects>(), (_perc_value > 0f && this.Modifier == PassiveEffect.ValueModifierTypes.perc_add) ? "+" : "", (_perc_value * 100f).ToCultureInvariantString("0.0"));
				}
			}
		}
		return null;
	}

	public PassiveEffects Type;

	public PassiveEffect.ValueModifierTypes Modifier;

	public string[] CVarValues;

	public float[] Values;

	public float[] Levels;

	public RequirementGroup Requirements;

	public FastTags<TagGroup.Global> Tags;

	public bool MatchAnyTags = true;

	public bool InvertTagCheck;

	public enum ValueModifierTypes
	{
		base_set,
		base_add,
		base_subtract,
		perc_set,
		perc_add,
		perc_subtract,
		COUNT
	}
}
