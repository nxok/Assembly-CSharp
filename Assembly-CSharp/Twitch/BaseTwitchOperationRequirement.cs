using System;
using UnityEngine.Scripting;

namespace Twitch
{
	[Preserve]
	public class BaseTwitchOperationRequirement : BaseTwitchRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public bool StringCompareCaseSensitive
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.stringComparison == StringComparison.CurrentCulture;
			}
			[PublicizedFrom(EAccessModifier.Protected)]
			set
			{
				this.stringComparison = (value ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool compare(float valueA, float valueB)
		{
			switch (this.operation)
			{
			case BaseTwitchOperationRequirement.OperationTypes.Equals:
			case BaseTwitchOperationRequirement.OperationTypes.EQ:
			case BaseTwitchOperationRequirement.OperationTypes.E:
				return valueA == valueB;
			case BaseTwitchOperationRequirement.OperationTypes.NotEquals:
			case BaseTwitchOperationRequirement.OperationTypes.NEQ:
			case BaseTwitchOperationRequirement.OperationTypes.NE:
				return valueA != valueB;
			case BaseTwitchOperationRequirement.OperationTypes.Less:
			case BaseTwitchOperationRequirement.OperationTypes.LessThan:
			case BaseTwitchOperationRequirement.OperationTypes.LT:
				return valueA < valueB;
			case BaseTwitchOperationRequirement.OperationTypes.Greater:
			case BaseTwitchOperationRequirement.OperationTypes.GreaterThan:
			case BaseTwitchOperationRequirement.OperationTypes.GT:
				return valueA > valueB;
			case BaseTwitchOperationRequirement.OperationTypes.LessOrEqual:
			case BaseTwitchOperationRequirement.OperationTypes.LessThanOrEqualTo:
			case BaseTwitchOperationRequirement.OperationTypes.LTE:
				return valueA <= valueB;
			case BaseTwitchOperationRequirement.OperationTypes.GreaterOrEqual:
			case BaseTwitchOperationRequirement.OperationTypes.GreaterThanOrEqualTo:
			case BaseTwitchOperationRequirement.OperationTypes.GTE:
				return valueA >= valueB;
			default:
				return true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool compare(string valueA, string valueB)
		{
			int num = string.Compare(valueA, valueB, this.stringComparison);
			switch (this.operation)
			{
			case BaseTwitchOperationRequirement.OperationTypes.Equals:
			case BaseTwitchOperationRequirement.OperationTypes.EQ:
			case BaseTwitchOperationRequirement.OperationTypes.E:
				return num == 0;
			case BaseTwitchOperationRequirement.OperationTypes.NotEquals:
			case BaseTwitchOperationRequirement.OperationTypes.NEQ:
			case BaseTwitchOperationRequirement.OperationTypes.NE:
				return num != 0;
			case BaseTwitchOperationRequirement.OperationTypes.Less:
			case BaseTwitchOperationRequirement.OperationTypes.LessThan:
			case BaseTwitchOperationRequirement.OperationTypes.LT:
				return num < 0;
			case BaseTwitchOperationRequirement.OperationTypes.Greater:
			case BaseTwitchOperationRequirement.OperationTypes.GreaterThan:
			case BaseTwitchOperationRequirement.OperationTypes.GT:
				return num > 0;
			case BaseTwitchOperationRequirement.OperationTypes.LessOrEqual:
			case BaseTwitchOperationRequirement.OperationTypes.LessThanOrEqualTo:
			case BaseTwitchOperationRequirement.OperationTypes.LTE:
				return num <= 0;
			case BaseTwitchOperationRequirement.OperationTypes.GreaterOrEqual:
			case BaseTwitchOperationRequirement.OperationTypes.GreaterThanOrEqualTo:
			case BaseTwitchOperationRequirement.OperationTypes.GTE:
				return num <= 0;
			default:
				return true;
			}
		}

		public override bool CanPerform(Entity target)
		{
			object obj = this.LeftSide(target);
			object obj2 = this.RightSide(target);
			if (obj is string)
			{
				return this.compare((string)obj, (string)obj2);
			}
			return this.compare(Convert.ToSingle(obj), Convert.ToSingle(obj2));
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual object LeftSide(Entity target)
		{
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual object RightSide(Entity target)
		{
			return 0;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<BaseTwitchOperationRequirement.OperationTypes>(BaseTwitchOperationRequirement.PropOperation, ref this.operation);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public BaseTwitchOperationRequirement.OperationTypes operation = BaseTwitchOperationRequirement.OperationTypes.Equals;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropOperation = "operation";

		[PublicizedFrom(EAccessModifier.Private)]
		public StringComparison stringComparison;

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
}
