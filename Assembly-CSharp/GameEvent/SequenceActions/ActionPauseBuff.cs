using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionPauseBuff : ActionBaseClientAction
	{
		public override bool CanPerform(Entity target)
		{
			if (!this.checkAlreadyExists)
			{
				return true;
			}
			EntityAlive entityAlive = target as EntityAlive;
			return entityAlive == null || entityAlive.Buffs.HasBuffByTag(this.buffTags);
		}

		public override void OnClientPerform(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				for (int i = 0; i < entityAlive.Buffs.ActiveBuffs.Count; i++)
				{
					BuffValue buffValue = entityAlive.Buffs.ActiveBuffs[i];
					if (buffValue.BuffClass.Tags.Test_AnySet(this.buffTags))
					{
						buffValue.Paused = this.pauseState;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnServerPerform(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				for (int i = 0; i < entityAlive.Buffs.ActiveBuffs.Count; i++)
				{
					BuffValue buffValue = entityAlive.Buffs.ActiveBuffs[i];
					if (buffValue.BuffClass.Tags.Test_AnySet(this.buffTags))
					{
						buffValue.Paused = this.pauseState;
					}
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			string text = "";
			properties.ParseString(ActionPauseBuff.PropBuffTags, ref text);
			if (text != "")
			{
				this.buffTags = FastTags<TagGroup.Global>.Parse(text);
			}
			properties.ParseBool(ActionPauseBuff.PropPauseState, ref this.pauseState);
			properties.ParseBool(ActionPauseBuff.PropCheckAlreadyExists, ref this.checkAlreadyExists);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionPauseBuff
			{
				buffTags = this.buffTags,
				pauseState = this.pauseState,
				targetGroup = this.targetGroup,
				checkAlreadyExists = this.checkAlreadyExists
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public FastTags<TagGroup.Global> buffTags = FastTags<TagGroup.Global>.none;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool checkAlreadyExists = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool pauseState = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffTags = "buff_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPauseState = "state";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropCheckAlreadyExists = "check_already_exists";
	}
}
