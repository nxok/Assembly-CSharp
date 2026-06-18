using System;
using System.Collections.Generic;
using System.Xml.Linq;

public abstract class StatCompareAbs : TargetedCompareRequirementBase
{
	public EntityStats Stats
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			StatCompareAbs.StatSample statSample = this.sample;
			if (statSample == StatCompareAbs.StatSample.Current)
			{
				return this.target.Stats;
			}
			if (statSample != StatCompareAbs.StatSample.StartOfFrame)
			{
				return this.target.Stats;
			}
			return this.target.StartOfFrameStats;
		}
	}

	public override bool IsValid(MinEventParams _params)
	{
		return base.IsValid(_params) && this.Compare(_params);
	}

	public abstract bool Compare(MinEventParams _params);

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}' {0} {2} {3}", new object[]
		{
			this.invert ? "NOT" : "",
			this.stat.ToStringCached<StatCompareAbs.StatTypes>(),
			this.operation.ToStringCached<RequirementBase.OperationTypes>(),
			this.value.ToCultureInvariantString()
		}));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "stat")
			{
				this.stat = EnumUtils.Parse<StatCompareAbs.StatTypes>(_attribute.Value, true);
				return true;
			}
			if (localName == "sample")
			{
				this.sample = EnumUtils.Parse<StatCompareAbs.StatSample>(_attribute.Value, true);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatCompareAbs()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatCompareAbs.StatTypes stat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatCompareAbs.StatSample sample;

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatTypes
	{
		None,
		Health,
		Stamina,
		Food,
		Water,
		Armor
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatSample
	{
		Current,
		StartOfFrame
	}
}
