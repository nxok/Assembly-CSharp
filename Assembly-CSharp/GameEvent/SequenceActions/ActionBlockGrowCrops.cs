using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBlockGrowCrops : ActionBaseBlockAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
		{
			if (!blockValue.isair)
			{
				BlockPlantGrowing blockPlantGrowing = blockValue.Block as BlockPlantGrowing;
				if (blockPlantGrowing != null)
				{
					blockValue = blockPlantGrowing.ForceNextGrowStage(world, currentPos, blockValue);
					return new BlockChangeInfo(currentPos, blockValue);
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBlockGrowCrops();
		}
	}
}
