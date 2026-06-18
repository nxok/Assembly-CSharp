using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddBuff : MinEventActionBuffModifierBase
{
	public override void Execute(MinEventParams _params)
	{
		bool flag = !_params.Self.isEntityRemote | _params.IsLocal;
		int num = -1;
		if (_params.Buff != null)
		{
			num = _params.Buff.InstigatorId;
		}
		if (num == -1)
		{
			num = _params.Self.entityId;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			string[] array = this.buffNames;
			if (this.buffOneOnly && this.buffWeights != null)
			{
				float randomFloat = this.targets[i].rand.RandomFloat;
				float num2 = 0f;
				for (int j = 0; j < this.buffWeights.Length; j++)
				{
					num2 += this.buffWeights[j];
					if (num2 >= randomFloat)
					{
						array = new string[] { this.buffNames[j] };
						break;
					}
				}
			}
			else if (this.buffWeights != null)
			{
				List<string> list = new List<string>();
				for (int k = 0; k < this.buffWeights.Length; k++)
				{
					float randomFloat2 = this.targets[i].rand.RandomFloat;
					if (this.buffWeights[k] >= randomFloat2)
					{
						list.Add(this.buffNames[k]);
					}
				}
				array = list.ToArray();
			}
			foreach (string text in array)
			{
				BuffClass buff = BuffManager.GetBuff(text);
				if (buff != null)
				{
					if (this.durationAltered && this.cvarRef)
					{
						if (this.targets[i].Buffs.HasCustomVar(this.refCvarName))
						{
							this.duration = this.targets[i].Buffs.GetCustomVar(this.refCvarName);
						}
						else
						{
							this.duration = buff.InitialDurationMax;
						}
					}
					if (this.durationAltered)
					{
						this.targets[i].Buffs.AddBuff(text, num, flag, false, this.duration);
					}
					else
					{
						this.targets[i].Buffs.AddBuff(text, num, flag, false, -1f);
					}
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "duration")
		{
			if (_attribute.Value.StartsWith("@"))
			{
				this.cvarRef = true;
				this.refCvarName = _attribute.Value.Substring(1);
			}
			else
			{
				this.duration = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
			}
			this.durationAltered = true;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool durationAltered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName = string.Empty;
}
