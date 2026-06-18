using System;
using System.IO;
using UnityEngine;

public class ProgressionValue
{
	public float GetCalculatedLevel(EntityAlive _ea)
	{
		if (this.calculatedFrame == Time.frameCount)
		{
			return this.calculatedLevel;
		}
		ProgressionClass progressionClass = this.ProgressionClass;
		if (progressionClass == null)
		{
			return 0f;
		}
		float num = (float)this.Level;
		PassiveEffects passiveEffects;
		switch (progressionClass.Type)
		{
		case ProgressionType.Attribute:
			passiveEffects = PassiveEffects.AttributeLevel;
			break;
		case ProgressionType.Skill:
			passiveEffects = PassiveEffects.SkillLevel;
			break;
		case ProgressionType.Perk:
			passiveEffects = PassiveEffects.PerkLevel;
			break;
		default:
			passiveEffects = PassiveEffects.None;
			break;
		}
		PassiveEffects passiveEffects2 = passiveEffects;
		if (passiveEffects2 != PassiveEffects.None)
		{
			num = EffectManager.GetValue(passiveEffects2, null, num, _ea, null, progressionClass.NameTag, true, true, true, true, true, 1, true, false);
		}
		num = Mathf.Min(num, (float)ProgressionClass.GetCalculatedMaxLevel(_ea, this));
		num = Mathf.Max(num, (float)progressionClass.MinLevel);
		this.calculatedFrame = Time.frameCount;
		this.calculatedLevel = num;
		return num;
	}

	public ProgressionValue()
	{
	}

	public ProgressionValue(string _name)
	{
		this.name = _name;
	}

	public string Name
	{
		get
		{
			return this.name;
		}
	}

	public ProgressionClass ProgressionClass
	{
		get
		{
			if (this.cachedProgressionClass == null && !Progression.ProgressionClasses.TryGetValue(this.name, out this.cachedProgressionClass))
			{
				Log.Error("ProgressionValue ProgressionClasses missing {0}", new object[] { this.name });
			}
			return this.cachedProgressionClass;
		}
	}

	public int CostForNextLevel
	{
		get
		{
			if (this.ProgressionClass.CurrencyType == ProgressionCurrencyType.SP)
			{
				return this.ProgressionClass.CalculatedCostForLevel(this.Level + 1);
			}
			return this.costForNextLevel;
		}
		set
		{
			if (this.ProgressionClass.CurrencyType != ProgressionCurrencyType.SP)
			{
				this.costForNextLevel = value;
			}
		}
	}

	public int Level
	{
		get
		{
			ProgressionClass progressionClass = this.ProgressionClass;
			if (progressionClass == null)
			{
				return this.level;
			}
			if (progressionClass.IsSkill)
			{
				return progressionClass.MaxLevel;
			}
			return this.level;
		}
		set
		{
			this.calculatedFrame = -1;
			if (this.ProgressionClass == null)
			{
				this.level = value;
				return;
			}
			if (this.ProgressionClass.IsSkill)
			{
				this.level = this.ProgressionClass.MaxLevel;
				return;
			}
			this.level = value;
		}
	}

	public int CalculatedLevel(EntityAlive _ea)
	{
		return (int)this.GetCalculatedLevel(_ea);
	}

	public int CalculatedMaxLevel(EntityAlive _ea)
	{
		return ProgressionClass.GetCalculatedMaxLevel(_ea, this);
	}

	public bool IsLocked(EntityAlive _ea)
	{
		return ProgressionClass.GetCalculatedMaxLevel(_ea, this) == 0;
	}

	public float PercToNextLevel
	{
		get
		{
			return 1f - (float)this.CostForNextLevel / (float)this.ProgressionClass.CalculatedCostForLevel(this.level + 1);
		}
	}

	public void ClearProgressionClassLink()
	{
		this.cachedProgressionClass = null;
	}

	public bool CanPurchase(EntityAlive _ea, int _level)
	{
		return _level <= this.ProgressionClass.MaxLevel;
	}

	public void CopyFrom(ProgressionValue _pv)
	{
		this.name = _pv.name;
		this.level = _pv.level;
		this.costForNextLevel = _pv.costForNextLevel;
	}

	public void Read(BinaryReader _reader)
	{
		_reader.ReadByte();
		this.name = _reader.ReadString();
		this.level = (int)_reader.ReadByte();
		this.costForNextLevel = _reader.ReadInt32();
	}

	public void Write(BinaryWriter _writer, bool _IsNetwork)
	{
		_writer.Write(1);
		_writer.Write(this.name);
		_writer.Write((byte)this.level);
		_writer.Write(this.costForNextLevel);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte Version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public int level;

	[PublicizedFrom(EAccessModifier.Private)]
	public int costForNextLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int calculatedFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public float calculatedLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass cachedProgressionClass;
}
