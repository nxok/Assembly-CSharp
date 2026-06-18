using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillSkillInfoWindow : XUiC_InfoWindow
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

	public override void Init()
	{
		base.Init();
		base.GetChildrenByType<XUiC_SkillSkillMilestone>(this.levelEntries);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSkill()
	{
		this.effectLines.Clear();
		if (this.CurrentSkill != null)
		{
			int level = this.CurrentSkill.Level;
			foreach (MinEffectGroup minEffectGroup in this.CurrentSkill.ProgressionClass.Effects.EffectGroups)
			{
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
					passiveEffect.ModifyValue(this.xui.playerUI.entityPlayer, (float)level, ref num, ref num2, passiveEffect.Tags, 1);
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
		if (this.CurrentSkill != null)
		{
			float num4 = 0f;
			float num5;
			if (this.CurrentSkill.ProgressionClass.MaxLevel - this.CurrentSkill.ProgressionClass.MinLevel <= 5)
			{
				num5 = 1f;
			}
			else
			{
				num5 = (float)this.CurrentSkill.ProgressionClass.MaxLevel / 5f;
			}
			using (List<XUiC_SkillSkillMilestone>.Enumerator enumerator3 = this.levelEntries.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					XUiC_SkillSkillMilestone xuiC_SkillSkillMilestone = enumerator3.Current;
					if (num5 < 2f && num4 < (float)this.CurrentSkill.ProgressionClass.MinLevel)
					{
						num4 = (float)this.CurrentSkill.ProgressionClass.MinLevel;
					}
					xuiC_SkillSkillMilestone.LevelStart = Mathf.RoundToInt(num4);
					if (xuiC_SkillSkillMilestone.LevelStart < this.CurrentSkill.ProgressionClass.MinLevel)
					{
						xuiC_SkillSkillMilestone.LevelStart = this.CurrentSkill.ProgressionClass.MinLevel;
					}
					float num6 = num4 + num5;
					if (Mathf.RoundToInt(num6) == Mathf.RoundToInt(num4))
					{
						num6 += 1f;
					}
					xuiC_SkillSkillMilestone.LevelGoal = Mathf.RoundToInt(num6);
					xuiC_SkillSkillMilestone.IsDirty = true;
					num4 = num6;
				}
				return;
			}
		}
		foreach (XUiC_SkillSkillMilestone xuiC_SkillSkillMilestone2 in this.levelEntries)
		{
			xuiC_SkillSkillMilestone2.LevelStart = 0;
			xuiC_SkillSkillMilestone2.LevelGoal = 1;
			xuiC_SkillSkillMilestone2.IsDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1283949528U)
		{
			if (num <= 831192208U)
			{
				if (num != 443815844U)
				{
					if (num != 660229954U)
					{
						if (num == 831192208U)
						{
							if (_bindingName == "nextSkillLevelRequirement")
							{
								if (this.CurrentSkill != null)
								{
									ProgressionValue progressionValue = entityPlayer.Progression.GetProgressionValue(this.CurrentSkill.ProgressionClass.ParentName);
									_value = string.Format(Localization.Get("xuiSkillRequirement", false, null), Localization.Get(progressionValue.ProgressionClass.NameKey, false, null), Mathf.CeilToInt((float)this.CurrentSkill.Level / this.CurrentSkill.ProgressionClass.ParentMaxLevelRatio));
								}
								else
								{
									_value = "false";
								}
								return true;
							}
						}
					}
					else if (_bindingName == "notmaxlevel")
					{
						_value = ((this.CurrentSkill != null) ? (this.CurrentSkill.CalculatedLevel(entityPlayer) < this.CurrentSkill.ProgressionClass.MaxLevel).ToString() : "true");
						return true;
					}
				}
				else if (_bindingName == "skillLevel")
				{
					_value = ((this.CurrentSkill != null) ? this.skillLevelFormatter.Format(this.CurrentSkill.GetCalculatedLevel(entityPlayer)) : "0");
					return true;
				}
			}
			else if (num != 1240702784U)
			{
				if (num != 1275709072U)
				{
					if (num == 1283949528U)
					{
						if (_bindingName == "currentlevel")
						{
							_value = Localization.Get("xuiSkillLevel", false, null);
							return true;
						}
					}
				}
				else if (_bindingName == "maxSkillLevel")
				{
					_value = ((this.CurrentSkill != null) ? this.maxSkillLevelFormatter.Format((float)this.CurrentSkill.ProgressionClass.MaxLevel) : "0");
					return true;
				}
			}
			else if (_bindingName == "effectsCol1")
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
		else if (num <= 2063064015U)
		{
			if (num != 1291035641U)
			{
				if (num != 1572078155U)
				{
					if (num == 2063064015U)
					{
						if (_bindingName == "skillpercentthislevel")
						{
							_value = ((this.CurrentSkill != null && this.CurrentSkill.CalculatedLevel(entityPlayer) < this.CurrentSkill.ProgressionClass.MaxLevel) ? this.skillPercentThisLevelFormatter.Format(this.CurrentSkill.PercToNextLevel) : "1");
							return true;
						}
					}
				}
				else if (_bindingName == "nextSkillLevelLocked")
				{
					_value = ((this.CurrentSkill != null) ? (this.CurrentSkill.CalculatedLevel(entityPlayer) < this.CurrentSkill.ProgressionClass.MaxLevel && this.CurrentSkill.CalculatedMaxLevel(entityPlayer) < this.CurrentSkill.CalculatedLevel(entityPlayer) + 1).ToString() : "false");
					return true;
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
		else if (num <= 2606420134U)
		{
			if (num != 2521042125U)
			{
				if (num == 2606420134U)
				{
					if (_bindingName == "groupdescription")
					{
						_value = ((this.CurrentSkill != null) ? Localization.Get(this.CurrentSkill.ProgressionClass.DescKey, false, null) : "");
						return true;
					}
				}
			}
			else if (_bindingName == "nextSkillLevel")
			{
				_value = ((this.CurrentSkill != null) ? this.skillLevelFormatter.Format(this.CurrentSkill.GetCalculatedLevel(entityPlayer) + 1f) : "0");
				return true;
			}
		}
		else if (num != 3504806855U)
		{
			if (num == 4010384093U)
			{
				if (_bindingName == "groupicon")
				{
					_value = ((this.CurrentSkill != null) ? this.CurrentSkill.ProgressionClass.Icon : "ui_game_symbol_skills");
					return true;
				}
			}
		}
		else if (_bindingName == "groupname")
		{
			_value = ((this.CurrentSkill != null) ? Localization.Get(this.CurrentSkill.ProgressionClass.NameKey, false, null) : "Skill Info");
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_SkillSkillMilestone> levelEntries = new List<XUiC_SkillSkillMilestone>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> effectLines = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeSubtractionFormatter = new CachedStringFormatter<string, float, bool>((string _s, float _f, bool _b) => _s + ": " + _f.ToCultureInvariantString("0.#") + (_b ? "%" : ""));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeSetValueFormatter = new CachedStringFormatter<string, float>((string _s, float _f) => _s + ": " + _f.ToCultureInvariantString("0.#"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float, float> groupLevelFormatter = new CachedStringFormatter<float, float>((float _i1, float _i2) => _i1.ToCultureInvariantString() + "/" + _i2.ToCultureInvariantString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> groupPointCostFormatter = new CachedStringFormatter<int>((int _i) => string.Format("{0} {1}", _i, (_i != 1) ? Localization.Get("xuiSkillPoints", false, null) : Localization.Get("xuiSkillPoint", false, null)));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillPercentThisLevelFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillLevelFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat maxSkillLevelFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> buyCostFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString() + " " + Localization.Get("xuiSkillPoints", false, null));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> expCostFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString() + " " + Localization.Get("RewardExp_keyword", false, null));
}
