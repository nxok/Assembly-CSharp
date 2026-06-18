using System;

namespace Quests.Requirements
{
	public abstract class BaseRequirement
	{
		public string ID { get; set; }

		public string Value { get; set; }

		public bool Complete
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public Quest OwnerQuest { get; set; }

		public QuestClass Owner { get; set; }

		public string Description
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public string StatusText
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public int Phase { get; set; }

		public virtual void HandleVariables()
		{
			this.ID = this.OwnerQuest.ParseVariable(this.ID);
			this.Value = this.OwnerQuest.ParseVariable(this.Value);
		}

		public virtual void SetupRequirement()
		{
		}

		public virtual bool CheckRequirement()
		{
			return false;
		}

		public virtual BaseRequirement Clone()
		{
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public BaseRequirement()
		{
		}

		public DynamicProperties Properties;

		public enum RequirementTypes
		{
			Buff,
			Holding,
			Level,
			Wearing
		}
	}
}
