using System;
using System.Collections;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionModifyStats : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (this.Delay > 0f)
		{
			GameManager.Instance.StartCoroutine(this.executeDelayed(this.Delay, _params));
			return;
		}
		this.execute(_params);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator executeDelayed(float delaySeconds, MinEventParams _params)
	{
		yield return new WaitForSeconds(delaySeconds);
		this.execute(_params);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.cvarRef)
			{
				this.value = this.targets[i].Buffs.GetCustomVar(this.refCvarName);
			}
			Stat stat = null;
			string text = this.statName;
			if (!(text == "health"))
			{
				if (!(text == "stamina"))
				{
					if (!(text == "food"))
					{
						if (text == "water")
						{
							stat = this.targets[i].Stats.Water;
						}
					}
					else
					{
						stat = this.targets[i].Stats.Food;
					}
				}
				else
				{
					stat = this.targets[i].Stats.Stamina;
				}
			}
			else
			{
				stat = this.targets[i].Stats.Health;
			}
			if (stat != null)
			{
				switch (this.operation)
				{
				case MinEventActionModifyStats.OperationTypes.set:
				case MinEventActionModifyStats.OperationTypes.setvalue:
					if (this.valueType == "max")
					{
						stat.BaseMax = this.value;
					}
					else if (this.valueType == "modifiedmax")
					{
						stat.MaxModifier = this.value - stat.BaseMax;
					}
					else
					{
						stat.Value = this.value;
					}
					break;
				case MinEventActionModifyStats.OperationTypes.add:
					if (this.valueType == "max")
					{
						stat.BaseMax += this.value;
					}
					else if (this.valueType == "modifiedmax")
					{
						stat.MaxModifier += this.value;
					}
					else
					{
						stat.Value += this.value;
					}
					break;
				case MinEventActionModifyStats.OperationTypes.subtract:
					if (this.valueType == "max")
					{
						stat.BaseMax -= this.value;
					}
					else if (this.valueType == "modifiedmax")
					{
						stat.MaxModifier -= this.value;
					}
					else
					{
						stat.Value -= this.value;
					}
					break;
				case MinEventActionModifyStats.OperationTypes.multiply:
					if (this.valueType == "max")
					{
						stat.BaseMax *= this.value;
					}
					else if (this.valueType == "modifiedmax")
					{
						stat.MaxModifier *= this.value;
					}
					else
					{
						stat.Value *= this.value;
					}
					break;
				case MinEventActionModifyStats.OperationTypes.divide:
					if (this.valueType == "max")
					{
						stat.BaseMax = stat.Value / ((this.value == 0f) ? 0.0001f : this.value);
					}
					else if (this.valueType == "modifiedmax")
					{
						stat.MaxModifier = stat.Value / ((this.value == 0f) ? 0.0001f : this.value);
					}
					else
					{
						stat.Value /= ((this.value == 0f) ? 0.0001f : this.value);
					}
					break;
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "stat")
			{
				this.statName = _attribute.Value.ToLower();
				return true;
			}
			if (localName == "operation")
			{
				this.operation = EnumUtils.Parse<MinEventActionModifyStats.OperationTypes>(_attribute.Value, true);
				return true;
			}
			if (localName == "value")
			{
				if (_attribute.Value.StartsWith("@"))
				{
					this.cvarRef = true;
					this.refCvarName = _attribute.Value.Substring(1);
				}
				else
				{
					this.value = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				}
				return true;
			}
			if (localName == "value_type")
			{
				this.valueType = _attribute.Value.ToLower();
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string statName;

	[PublicizedFrom(EAccessModifier.Private)]
	public MinEventActionModifyStats.OperationTypes operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum OperationTypes
	{
		set,
		setvalue,
		add,
		subtract,
		multiply,
		divide,
		randomfloat,
		randomint
	}
}
