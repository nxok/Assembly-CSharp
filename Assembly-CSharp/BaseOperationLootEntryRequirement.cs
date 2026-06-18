using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class BaseOperationLootEntryRequirement : BaseLootEntryRequirement
{
	public override void Init(XElement e)
	{
		base.Init(e);
		string text = "";
		if (e.ParseAttribute("operation", ref text))
		{
			this.operation = Enum.Parse<BaseOperationLootEntryRequirement.OperationTypes>(text);
		}
	}

	public override bool CheckRequirement(EntityPlayer player)
	{
		float num = this.LeftSide(player);
		float num2 = this.RightSide(player);
		switch (this.operation)
		{
		case BaseOperationLootEntryRequirement.OperationTypes.Equals:
		case BaseOperationLootEntryRequirement.OperationTypes.EQ:
		case BaseOperationLootEntryRequirement.OperationTypes.E:
			return num == num2;
		case BaseOperationLootEntryRequirement.OperationTypes.NotEquals:
		case BaseOperationLootEntryRequirement.OperationTypes.NEQ:
		case BaseOperationLootEntryRequirement.OperationTypes.NE:
			return num != num2;
		case BaseOperationLootEntryRequirement.OperationTypes.Less:
		case BaseOperationLootEntryRequirement.OperationTypes.LessThan:
		case BaseOperationLootEntryRequirement.OperationTypes.LT:
			return num < num2;
		case BaseOperationLootEntryRequirement.OperationTypes.Greater:
		case BaseOperationLootEntryRequirement.OperationTypes.GreaterThan:
		case BaseOperationLootEntryRequirement.OperationTypes.GT:
			return num > num2;
		case BaseOperationLootEntryRequirement.OperationTypes.LessOrEqual:
		case BaseOperationLootEntryRequirement.OperationTypes.LessThanOrEqualTo:
		case BaseOperationLootEntryRequirement.OperationTypes.LTE:
			return num <= num2;
		case BaseOperationLootEntryRequirement.OperationTypes.GreaterOrEqual:
		case BaseOperationLootEntryRequirement.OperationTypes.GreaterThanOrEqualTo:
		case BaseOperationLootEntryRequirement.OperationTypes.GTE:
			return num >= num2;
		default:
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float LeftSide(EntityPlayer player)
	{
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float RightSide(EntityPlayer player)
	{
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BaseOperationLootEntryRequirement.OperationTypes operation = BaseOperationLootEntryRequirement.OperationTypes.Equals;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOperation = "operation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum OperationTypes
	{
		None,
		Equals,
		EQ,
		E,
		NotEquals,
		NEQ,
		NE,
		Less,
		LessThan,
		LT,
		Greater,
		GreaterThan,
		GT,
		LessOrEqual,
		LessThanOrEqualTo,
		LTE,
		GreaterOrEqual,
		GreaterThanOrEqualTo,
		GTE
	}
}
