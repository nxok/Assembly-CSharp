using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillSkillMilestone : XUiController
{
	public ProgressionValue CurrentSkill
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.xui.SelectedSkill == null || !this.xui.SelectedSkill.ProgressionClass.IsSkill)
			{
				return null;
			}
			return this.xui.SelectedSkill;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSkill()
	{
		this.effectLines.Clear();
		if (this.CurrentSkill != null)
		{
			int levelGoal = this.LevelGoal;
			foreach (MinEffectGroup minEffectGroup in this.CurrentSkill.ProgressionClass.Effects.EffectGroups)
			{
				if (minEffectGroup.EffectDescriptions != null)
				{
					for (int i = 0; i < minEffectGroup.EffectDescriptions.Count; i++)
					{
						if (levelGoal >= minEffectGroup.EffectDescriptions[i].MinLevel && levelGoal <= minEffectGroup.EffectDescriptions[i].MaxLevel)
						{
							this.effectLines.Add(minEffectGroup.EffectDescriptions[i].Description);
							return;
						}
					}
				}
				foreach (PassiveEffect passiveEffect in minEffectGroup.PassiveEffects)
				{
					float num = 0f;
					float num2 = 1f;
					int entityClass = this.xui.playerUI.entityPlayer.entityClass;
					if (EntityClass.list.ContainsKey(entityClass) && EntityClass.list[entityClass].Effects != null)
					{
						EntityClass.list[entityClass].Effects.ModifyValue(this.xui.playerUI.entityPlayer, passiveEffect.Type, ref num, ref num2, 0f, EntityClass.list[entityClass].Tags, 1);
					}
					float num3 = num;
					passiveEffect.ModifyValue(this.xui.playerUI.entityPlayer, (float)levelGoal, ref num, ref num2, passiveEffect.Tags, 1);
					if (num != num3 || num2 != 1f)
					{
						if (num == num3)
						{
							this.effectLines.Add(this.attributeSubtractionFormatter.Format(passiveEffect.Type.ToStringCached<PassiveEffects>(), 100f * num2, true));
						}
						else
						{
							this.effectLines.Add(this.attributeSetValueFormatter.Format(passiveEffect.Type.ToStringCached<PassiveEffects>(), num2 * num));
						}
					}
				}
			}
		}
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "color_lbl_available")
		{
			this.color_lbl_available = _value;
			return true;
		}
		if (!(_name == "color_lbl_locked"))
		{
			return base.ParseAttribute(_name, _value);
		}
		this.color_lbl_locked = _value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		bool flag = this.CurrentSkill != null && this.CurrentSkill.ProgressionClass.MaxLevel >= this.LevelGoal;
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1566407741U)
		{
			if (num <= 1240702784U)
			{
				if (num != 12870938U)
				{
					if (num == 1240702784U)
					{
						if (_bindingName == "effectsCol1")
						{
							int num2 = this.effectLines.Count;
							if (this.effectLines.Count > 3)
							{
								num2 = this.effectLines.Count / 2 + this.effectLines.Count % 2;
							}
							_value = "";
							for (int i = 0; i < num2; i++)
							{
								if (_value.Length > 0)
								{
									_value += "\n";
								}
								_value += this.effectLines[i];
							}
							return true;
						}
					}
				}
				else if (_bindingName == "color_fg")
				{
					if (flag && this.CurrentSkill.CalculatedLevel(entityPlayer) >= this.LevelGoal && !this.CurrentSkill.IsLocked(entityPlayer))
					{
						_value = this.color_lbl_available;
					}
					else
					{
						_value = this.color_lbl_locked;
					}
					return true;
				}
			}
			else if (num != 1291035641U)
			{
				if (num == 1566407741U)
				{
					if (_bindingName == "hasentry")
					{
						_value = flag.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "effectsCol2")
			{
				int num2 = this.effectLines.Count;
				if (this.effectLines.Count > 3)
				{
					num2 = this.effectLines.Count / 2 + this.effectLines.Count % 2;
				}
				_value = "";
				for (int j = num2; j < this.effectLines.Count; j++)
				{
					if (_value.Length > 0)
					{
						_value += "\n";
					}
					_value += this.effectLines[j];
				}
				return true;
			}
		}
		else if (num <= 3406319174U)
		{
			if (num != 2610554845U)
			{
				if (num == 3406319174U)
				{
					if (_bindingName == "progress")
					{
						if (flag)
						{
							if (this.CurrentSkill.CalculatedLevel(entityPlayer) < this.LevelStart)
							{
								_value = "0.0";
							}
							else if (this.CurrentSkill.CalculatedLevel(entityPlayer) >= this.LevelGoal)
							{
								_value = "1.0";
							}
							else
							{
								float num3 = (float)this.CurrentSkill.CalculatedLevel(entityPlayer) + this.CurrentSkill.PercToNextLevel;
								_value = ((num3 - (float)this.LevelStart) / (float)(this.LevelGoal - this.LevelStart)).ToCultureInvariantString();
							}
						}
						else
						{
							_value = "0.0";
						}
						return true;
					}
				}
			}
			else if (_bindingName == "level")
			{
				_value = this.LevelGoal.ToString();
				return true;
			}
		}
		else if (num != 3499124306U)
		{
			if (num != 3862959600U)
			{
				if (num == 4105239451U)
				{
					if (_bindingName == "icontooltip")
					{
						if (flag && this.CurrentSkill.CalculatedMaxLevel(entityPlayer) < this.LevelGoal)
						{
							ProgressionValue progressionValue = entityPlayer.Progression.GetProgressionValue(this.CurrentSkill.ProgressionClass.ParentName);
							_value = string.Format(Localization.Get("xuiSkillRequirement", false, null), Localization.Get(progressionValue.ProgressionClass.NameKey, false, null), Mathf.CeilToInt((float)this.LevelGoal / this.CurrentSkill.ProgressionClass.ParentMaxLevelRatio));
						}
						else
						{
							_value = "";
						}
						return true;
					}
				}
			}
			else if (_bindingName == "icon")
			{
				if (flag && this.CurrentSkill.CalculatedLevel(entityPlayer) >= this.LevelGoal)
				{
					_value = "ui_game_symbol_check";
				}
				else
				{
					_value = "ui_game_symbol_lock";
				}
				return true;
			}
		}
		else if (_bindingName == "iconvisible")
		{
			_value = (flag && (this.CurrentSkill.CalculatedLevel(entityPlayer) >= this.LevelGoal || this.CurrentSkill.CalculatedMaxLevel(entityPlayer) < this.LevelGoal)).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			this.IsDirty = false;
			this.UpdateSkill();
			base.RefreshBindings();
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> effectLines = new List<string>();

	public int LevelStart;

	public int LevelGoal;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeMultiplicationFormatter = new CachedStringFormatter<string, float>((string _s, float _f) => string.Format("{0}: {1}%", _s, (_f < 0f) ? _f.ToCultureInvariantString("0.#") : ("+" + _f.ToCultureInvariantString("0.#"))));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeDivisionFormatter = new CachedStringFormatter<string, float>((string _s, float _f) => string.Format("{0}: {1}%", _s, _f.ToCultureInvariantString("0.#")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeAdditionFormatter = new CachedStringFormatter<string, float, bool>((string _s, float _f, bool _b) => string.Format("{0}: +{1}", _s, _f.ToCultureInvariantString("0.#") + (_b ? "%" : "")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeSubtractionFormatter = new CachedStringFormatter<string, float, bool>((string _s, float _f, bool _b) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#") + (_b ? "%" : "")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeSetValueFormatter = new CachedStringFormatter<string, float>((string _s, float _f) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string> attributeLockedFormatter = new CachedStringFormatter<string, string>((string _s1, string _s2) => string.Format("{0}: {1}", _s1, _s2));
}
