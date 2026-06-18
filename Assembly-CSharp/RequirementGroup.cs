using System;
using System.Collections.Generic;

public sealed class RequirementGroup
{
	public RequirementGroup(RequirementGroup.Op _op, List<IRequirement> _reqs, List<RequirementGroup> _groups)
	{
		this.op = _op;
		this.reqs = _reqs;
		this.groups = _groups;
	}

	public bool IsValid(MinEventParams _params)
	{
		RequirementGroup.Op op = this.op;
		bool flag;
		if (op != RequirementGroup.Op.And)
		{
			if (op != RequirementGroup.Op.Or)
			{
				flag = this.EvalAnd(_params);
			}
			else
			{
				flag = this.EvalOr(_params);
			}
		}
		else
		{
			flag = this.EvalAnd(_params);
		}
		return flag;
	}

	public void GetInfoStrings(ref List<string> list)
	{
		if (this.reqs != null)
		{
			for (int i = 0; i < this.reqs.Count; i++)
			{
				string description = this.reqs[i].GetDescription();
				if (string.IsNullOrEmpty(description))
				{
					this.reqs[i].GetInfoStrings(ref list);
				}
				else
				{
					list.Add(description);
				}
			}
		}
		if (this.groups != null)
		{
			for (int j = 0; j < this.groups.Count; j++)
			{
				this.groups[j].GetInfoStrings(ref list);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EvalAnd(MinEventParams _params)
	{
		if ((this.reqs == null || this.reqs.Count == 0) && (this.groups == null || this.groups.Count == 0))
		{
			return true;
		}
		if (this.reqs != null)
		{
			for (int i = 0; i < this.reqs.Count; i++)
			{
				if (!this.reqs[i].IsValid(_params))
				{
					return false;
				}
			}
		}
		if (this.groups != null)
		{
			for (int j = 0; j < this.groups.Count; j++)
			{
				if (!this.groups[j].IsValid(_params))
				{
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EvalOr(MinEventParams _params)
	{
		bool flag = false;
		if (this.reqs != null)
		{
			flag |= this.reqs.Count > 0;
			for (int i = 0; i < this.reqs.Count; i++)
			{
				if (this.reqs[i].IsValid(_params))
				{
					return true;
				}
			}
		}
		if (this.groups != null)
		{
			flag |= this.groups.Count > 0;
			for (int j = 0; j < this.groups.Count; j++)
			{
				if (this.groups[j].IsValid(_params))
				{
					return true;
				}
			}
		}
		return !flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RequirementGroup.Op op;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IRequirement> reqs;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<RequirementGroup> groups;

	public enum Op
	{
		And,
		Or
	}
}
