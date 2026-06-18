using System;

namespace Twitch
{
	public class BaseTwitchVoteOperationRequirement : BaseTwitchVoteRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public override bool CanPerform(EntityPlayer player)
		{
			float num = this.LeftSide(player);
			float num2 = this.RightSide(player);
			switch (this.operation)
			{
			case BaseTwitchVoteOperationRequirement.OperationTypes.Equals:
			case BaseTwitchVoteOperationRequirement.OperationTypes.EQ:
			case BaseTwitchVoteOperationRequirement.OperationTypes.E:
				return num == num2;
			case BaseTwitchVoteOperationRequirement.OperationTypes.NotEquals:
			case BaseTwitchVoteOperationRequirement.OperationTypes.NEQ:
			case BaseTwitchVoteOperationRequirement.OperationTypes.NE:
				return num != num2;
			case BaseTwitchVoteOperationRequirement.OperationTypes.Less:
			case BaseTwitchVoteOperationRequirement.OperationTypes.LessThan:
			case BaseTwitchVoteOperationRequirement.OperationTypes.LT:
				return num < num2;
			case BaseTwitchVoteOperationRequirement.OperationTypes.Greater:
			case BaseTwitchVoteOperationRequirement.OperationTypes.GreaterThan:
			case BaseTwitchVoteOperationRequirement.OperationTypes.GT:
				return num > num2;
			case BaseTwitchVoteOperationRequirement.OperationTypes.LessOrEqual:
			case BaseTwitchVoteOperationRequirement.OperationTypes.LessThanOrEqualTo:
			case BaseTwitchVoteOperationRequirement.OperationTypes.LTE:
				return num <= num2;
			case BaseTwitchVoteOperationRequirement.OperationTypes.GreaterOrEqual:
			case BaseTwitchVoteOperationRequirement.OperationTypes.GreaterThanOrEqualTo:
			case BaseTwitchVoteOperationRequirement.OperationTypes.GTE:
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

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<BaseTwitchVoteOperationRequirement.OperationTypes>(BaseTwitchVoteOperationRequirement.PropOperation, ref this.operation);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public BaseTwitchVoteOperationRequirement.OperationTypes operation;

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
}
