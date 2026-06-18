using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddBuff : ActionBaseTargetAction
	{
		public override bool CanPerform(Entity target)
		{
			if (!this.checkAlreadyExists)
			{
				return true;
			}
			EntityAlive entityAlive = target as EntityAlive;
			return entityAlive != null && !entityAlive.Buffs.HasBuff(this.buffName) && (!(this.altVisionBuffName != "") || !entityAlive.Buffs.HasBuff(this.altVisionBuffName));
		}

		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			if (this.removesBuffs == null)
			{
				this.removesBuffs = this.removesBuff.Split(',', StringSplitOptions.None);
			}
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				bool flag = false;
				for (int i = 0; i < this.removesBuffs.Length; i++)
				{
					if (entityAlive.Buffs.HasBuff(this.removesBuffs[i]))
					{
						entityAlive.Buffs.RemoveBuff(this.removesBuffs[i], -1, true);
						flag = true;
					}
				}
				if (!flag)
				{
					if (this.altVisionBuffName != "" && entityAlive is EntityPlayer && (entityAlive as EntityPlayer).TwitchVisionDisabled)
					{
						entityAlive.Buffs.AddBuff(this.altVisionBuffName, -1, true, false, -1f);
						return BaseAction.ActionCompleteStates.Complete;
					}
					entityAlive.Buffs.AddBuff(this.buffName, -1, true, false, this.duration);
					if (this.sequenceLink != "" && entityAlive.Buffs.GetBuff(this.buffName) != null)
					{
						GameEventManager.Current.RegisterLink(entityAlive as EntityPlayer, base.Owner, this.sequenceLink);
					}
				}
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddBuff.PropBuffName, ref this.buffName);
			properties.ParseString(ActionAddBuff.PropRemovesBuff, ref this.removesBuff);
			properties.ParseString(ActionAddBuff.PropAltVisionBuffName, ref this.altVisionBuffName);
			properties.ParseBool(ActionAddBuff.PropCheckAlreadyExists, ref this.checkAlreadyExists);
			properties.ParseString(ActionAddBuff.PropSequenceLink, ref this.sequenceLink);
			this.Properties.ParseFloat(ActionAddBuff.PropDuration, ref this.duration);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddBuff
			{
				buffName = this.buffName,
				removesBuff = this.removesBuff,
				targetGroup = this.targetGroup,
				altVisionBuffName = this.altVisionBuffName,
				checkAlreadyExists = this.checkAlreadyExists,
				sequenceLink = this.sequenceLink,
				duration = this.duration
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string buffName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string removesBuff = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] removesBuffs;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string altVisionBuffName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool checkAlreadyExists = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string sequenceLink = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public float duration = -1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffName = "buff_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRemovesBuff = "removes_buff";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAltVisionBuffName = "alt_vision_buff_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropCheckAlreadyExists = "check_already_exists";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSequenceLink = "sequence_link";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropDuration = "duration";
	}
}
