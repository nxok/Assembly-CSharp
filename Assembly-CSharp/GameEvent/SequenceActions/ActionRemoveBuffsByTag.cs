using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRemoveBuffsByTag : ActionBaseTargetAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			this.tags = FastTags<TagGroup.Global>.Parse(this.buffTag);
		}

		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.Buffs.RemoveBuffsByTag(this.tags);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionRemoveBuffsByTag.PropBuffTags))
			{
				this.buffTag = properties.Values[ActionRemoveBuffsByTag.PropBuffTags];
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRemoveBuffsByTag
			{
				buffTag = this.buffTag,
				tags = this.tags,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string buffTag = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public FastTags<TagGroup.Global> tags;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffTags = "buff_tag";
	}
}
