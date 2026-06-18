using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionModifyCVar : MinEventActionTargetedBase
{
	public string cvarName
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (this.rollType == MinEventActionModifyCVar.RandomRollTypes.tierList)
		{
			if (_params.ParentType == MinEffectController.SourceParentType.ItemClass || _params.ParentType == MinEffectController.SourceParentType.ItemModifierClass)
			{
				if (!_params.ItemValue.IsEmpty())
				{
					int num = (int)(_params.ItemValue.Quality - 1);
					if (num >= 0)
					{
						this.value = this.valueList[num];
					}
				}
			}
			else if (_params.ParentType == MinEffectController.SourceParentType.ProgressionClass && _params.ProgressionValue != null)
			{
				int num2 = _params.ProgressionValue.CalculatedLevel(_params.Self);
				if (num2 >= 0)
				{
					this.value = this.valueList[num2];
				}
			}
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.cvarRef)
			{
				this.value = this.targets[i].Buffs.GetCustomVar(this.refCvarName);
			}
			else if (this.rollType == MinEventActionModifyCVar.RandomRollTypes.randomInt)
			{
				this.value = Mathf.Clamp((float)_params.Self.rand.RandomRange((int)this.minValue, (int)this.maxValue + 1), this.minValue, this.maxValue);
			}
			else if (this.rollType == MinEventActionModifyCVar.RandomRollTypes.randomFloat)
			{
				this.value = Mathf.Clamp(_params.Self.rand.RandomRange(this.minValue, this.maxValue + 1f), this.minValue, this.maxValue);
			}
			this.targets[i].Buffs.SetCustomVar(this.cvarName, this.value, (this.targets[i].isEntityRemote && !_params.Self.isEntityRemote) || _params.IsLocal, this.operation);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "cvar")
			{
				this.cvarName = _attribute.Value;
				return true;
			}
			if (localName == "operation")
			{
				this.operation = EnumUtils.Parse<CVarOperation>(_attribute.Value, true);
				return true;
			}
			if (localName == "value")
			{
				this.rollType = MinEventActionModifyCVar.RandomRollTypes.none;
				this.cvarRef = false;
				if (_attribute.Value.StartsWith("randomint", StringComparison.OrdinalIgnoreCase))
				{
					Vector2 vector = StringParsers.ParseVector2(_attribute.Value.Substring(_attribute.Value.IndexOf('(') + 1, _attribute.Value.IndexOf(')') - (_attribute.Value.IndexOf('(') + 1)));
					this.minValue = (float)((int)vector.x);
					this.maxValue = (float)((int)vector.y);
					this.rollType = MinEventActionModifyCVar.RandomRollTypes.randomInt;
				}
				else if (_attribute.Value.StartsWith("randomfloat", StringComparison.OrdinalIgnoreCase))
				{
					Vector2 vector2 = StringParsers.ParseVector2(_attribute.Value.Substring(_attribute.Value.IndexOf('(') + 1, _attribute.Value.IndexOf(')') - (_attribute.Value.IndexOf('(') + 1)));
					this.minValue = vector2.x;
					this.maxValue = vector2.y;
					this.rollType = MinEventActionModifyCVar.RandomRollTypes.randomFloat;
				}
				else if (_attribute.Value.StartsWith("@"))
				{
					this.cvarRef = true;
					this.refCvarName = _attribute.Value.Substring(1);
				}
				else if (_attribute.Value.Contains(','))
				{
					string[] array = _attribute.Value.Split(',', StringSplitOptions.None);
					this.valueList = new float[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						this.valueList[i] = StringParsers.ParseFloat(array[i], 0, -1, NumberStyles.Any);
					}
					this.rollType = MinEventActionModifyCVar.RandomRollTypes.tierList;
				}
				else
				{
					this.value = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				}
				return true;
			}
			if (localName == "seed_type")
			{
				this.seedType = EnumUtils.Parse<MinEventActionModifyCVar.SeedType>(_attribute.Value, true);
				return true;
			}
		}
		return flag;
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (this.cvarName != null && this.cvarName.StartsWith("_"))
		{
			Log.Out("CVar '{0}' is readonly", new object[] { this.cvarName });
			return false;
		}
		return base.CanExecute(_eventType, _params);
	}

	public float GetValueForDisplay()
	{
		if (this.operation == CVarOperation.add)
		{
			return this.value;
		}
		if (this.operation == CVarOperation.subtract)
		{
			return -this.value;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MinEventActionModifyCVar.SeedType seedType;

	[PublicizedFrom(EAccessModifier.Private)]
	public CVarOperation operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] valueList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public MinEventActionModifyCVar.RandomRollTypes rollType;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum SeedType
	{
		Item,
		Player,
		Random
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum RandomRollTypes : byte
	{
		none,
		randomInt,
		randomFloat,
		tierList
	}
}
