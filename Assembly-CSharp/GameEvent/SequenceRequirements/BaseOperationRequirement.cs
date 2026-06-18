using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class BaseOperationRequirement : BaseRequirement
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
			case BaseOperationRequirement.OperationTypes.Equals:
			case BaseOperationRequirement.OperationTypes.EQ:
			case BaseOperationRequirement.OperationTypes.E:
				return valueA == valueB;
			case BaseOperationRequirement.OperationTypes.NotEquals:
			case BaseOperationRequirement.OperationTypes.NEQ:
			case BaseOperationRequirement.OperationTypes.NE:
				return valueA != valueB;
			case BaseOperationRequirement.OperationTypes.Less:
			case BaseOperationRequirement.OperationTypes.LessThan:
			case BaseOperationRequirement.OperationTypes.LT:
				return valueA < valueB;
			case BaseOperationRequirement.OperationTypes.Greater:
			case BaseOperationRequirement.OperationTypes.GreaterThan:
			case BaseOperationRequirement.OperationTypes.GT:
				return valueA > valueB;
			case BaseOperationRequirement.OperationTypes.LessOrEqual:
			case BaseOperationRequirement.OperationTypes.LessThanOrEqualTo:
			case BaseOperationRequirement.OperationTypes.LTE:
				return valueA <= valueB;
			case BaseOperationRequirement.OperationTypes.GreaterOrEqual:
			case BaseOperationRequirement.OperationTypes.GreaterThanOrEqualTo:
			case BaseOperationRequirement.OperationTypes.GTE:
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
			case BaseOperationRequirement.OperationTypes.Equals:
			case BaseOperationRequirement.OperationTypes.EQ:
			case BaseOperationRequirement.OperationTypes.E:
				return num == 0;
			case BaseOperationRequirement.OperationTypes.NotEquals:
			case BaseOperationRequirement.OperationTypes.NEQ:
			case BaseOperationRequirement.OperationTypes.NE:
				return num != 0;
			case BaseOperationRequirement.OperationTypes.Less:
			case BaseOperationRequirement.OperationTypes.LessThan:
			case BaseOperationRequirement.OperationTypes.LT:
				return num < 0;
			case BaseOperationRequirement.OperationTypes.Greater:
			case BaseOperationRequirement.OperationTypes.GreaterThan:
			case BaseOperationRequirement.OperationTypes.GT:
				return num > 0;
			case BaseOperationRequirement.OperationTypes.LessOrEqual:
			case BaseOperationRequirement.OperationTypes.LessThanOrEqualTo:
			case BaseOperationRequirement.OperationTypes.LTE:
				return num <= 0;
			case BaseOperationRequirement.OperationTypes.GreaterOrEqual:
			case BaseOperationRequirement.OperationTypes.GreaterThanOrEqualTo:
			case BaseOperationRequirement.OperationTypes.GTE:
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
			properties.ParseEnum<BaseOperationRequirement.OperationTypes>(BaseOperationRequirement.PropOperation, ref this.operation);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new BaseOperationRequirement
			{
				Invert = this.Invert,
				operation = this.operation
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public BaseOperationRequirement.OperationTypes operation = BaseOperationRequirement.OperationTypes.Equals;

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
