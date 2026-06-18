using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBlockDowngrade : ActionBaseBlockAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
		{
			if (!blockValue.isair)
			{
				BlockValue blockValue2 = blockValue.Block.DowngradeBlock;
				blockValue2 = BlockPlaceholderMap.Instance.Replace(currentPos, blockValue2, GameManager.Instance.World.GetGameRandom(), false);
				blockValue2.rotation = blockValue.rotation;
				blockValue2.meta = blockValue.meta;
				if (!blockValue2.isair)
				{
					world.AddPendingDowngradeBlock(currentPos);
					return new BlockChangeInfo(currentPos, blockValue2);
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBlockDowngrade();
		}
	}
}
