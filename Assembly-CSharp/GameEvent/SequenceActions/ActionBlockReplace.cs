using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBlockReplace : ActionBaseBlockAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool CheckValid(World world, Vector3i currentPos)
		{
			BlockValue block = world.GetBlock(currentPos + Vector3i.down);
			return !block.isair && !block.Block.IsTerrainDecoration;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
		{
			if (this.blockTo == null)
			{
				return null;
			}
			if (this.emptyOnly && !blockValue.isair)
			{
				return null;
			}
			if (!blockValue.Block.blockMaterial.CanDestroy)
			{
				return null;
			}
			BlockValue blockValue2 = Block.GetBlockValue(this.blockTo[this.random.RandomRange(0, this.blockTo.Length)], false);
			if (blockValue.type != blockValue2.type)
			{
				return new BlockChangeInfo(currentPos, blockValue2, true);
			}
			return null;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			string text = "";
			this.Properties.ParseString(ActionBlockReplace.PropBlockTo, ref text);
			if (text != "")
			{
				this.blockTo = text.Split(',', StringSplitOptions.None);
			}
			properties.ParseBool(ActionBlockReplace.PropEmptyOnly, ref this.emptyOnly);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBlockReplace
			{
				blockTo = this.blockTo,
				emptyOnly = this.emptyOnly
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] blockTo;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool emptyOnly;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBlockTo = "block_to";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropEmptyOnly = "empty_only";
	}
}
