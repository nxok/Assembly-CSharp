using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBlockTriggerMines : ActionBaseBlockAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
		{
			if (!blockValue.isair)
			{
				return new BlockChangeInfo(currentPos, blockValue, true);
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ProcessChanges(World world, List<BlockChangeInfo> blockChanges)
		{
			for (int i = 0; i < blockChanges.Count; i++)
			{
				BlockMine blockMine = blockChanges[i].blockValue.Block as BlockMine;
				if (blockMine != null)
				{
					blockMine.TriggerMine(base.Owner.Target, world, blockChanges[i].blockValueRef, this.useTrigger);
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			this.Properties.ParseBool(ActionBlockTriggerMines.PropUseTrigger, ref this.useTrigger);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBlockTriggerMines
			{
				useTrigger = this.useTrigger
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool useTrigger;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropUseTrigger = "use_trigger";
	}
}
