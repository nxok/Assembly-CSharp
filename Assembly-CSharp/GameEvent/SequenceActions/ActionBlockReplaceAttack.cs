using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBlockReplaceAttack : ActionBlockReplace
	{
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
				if (!blockValue2.isair)
				{
					if (this.blocksAdded == null)
					{
						this.blocksAdded = new List<Vector3i>();
					}
					this.blocksAdded.Add(currentPos);
				}
				return new BlockChangeInfo(currentPos, blockValue2, true);
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ChangesComplete()
		{
			base.ChangesComplete();
			if (this.blocksAdded != null)
			{
				GameEventManager.SpawnedBlocksEntry spawnedBlocksEntry = GameEventManager.Current.RegisterSpawnedBlocks(this.blocksAdded, base.Owner.Target, base.Owner.Requester, base.Owner, this.timeAlive, this.removeSound, (base.Owner.Target != null) ? base.Owner.Target.position : base.Owner.TargetPosition, this.refundOnRemove);
				if (base.Owner.Requester != null)
				{
					if (base.Owner.Requester is EntityPlayerLocal)
					{
						GameEventManager.Current.HandleGameBlocksAdded(base.Owner.Name, spawnedBlocksEntry.BlockGroupID, this.blocksAdded, base.Owner.Tag);
						return;
					}
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlocksAdded, base.Owner.Name, spawnedBlocksEntry.BlockGroupID, this.blocksAdded, base.Owner.Tag, false), false, base.Owner.Requester.entityId, -1, -1, null, 192, false);
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseFloat(ActionBlockReplaceAttack.PropTimeAlive, ref this.timeAlive);
			properties.ParseString(ActionBlockReplaceAttack.PropRemoveSound, ref this.removeSound);
			properties.ParseBool(ActionBlockReplaceAttack.PropRefundOnRemove, ref this.refundOnRemove);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBlockReplaceAttack
			{
				blockTo = this.blockTo,
				emptyOnly = this.emptyOnly,
				timeAlive = this.timeAlive,
				removeSound = this.removeSound,
				refundOnRemove = this.refundOnRemove
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<Vector3i> blocksAdded = new List<Vector3i>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public float timeAlive = -1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string removeSound = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool refundOnRemove;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTimeAlive = "time_alive";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRemoveSound = "remove_sound";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRefundOnRemove = "refund_on_remove";
	}
}
