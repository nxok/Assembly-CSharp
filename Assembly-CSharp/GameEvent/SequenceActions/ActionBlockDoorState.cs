using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBlockDoorState : ActionBaseBlockAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
		{
			if (!blockValue.isair)
			{
				TileEntityComposite tileEntityComposite = world.GetTileEntity(currentPos) as TileEntityComposite;
				TEFeatureDoor tefeatureDoor;
				if (tileEntityComposite.TryGetSelfOrFeature(out tefeatureDoor))
				{
					tefeatureDoor.SetOpen(this.setOpen, false);
					TEFeatureLockable tefeatureLockable;
					if (this.handleLock && tileEntityComposite.TryGetSelfOrFeature(out tefeatureLockable))
					{
						tefeatureLockable.SetLocked(this.setLocked);
					}
					tefeatureDoor.HandleOpenCloseSound(currentPos);
					return new BlockChangeInfo(currentPos, blockValue);
				}
			}
			return null;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseBool(ActionBlockDoorState.PropSetOpenState, ref this.setOpen);
			if (properties.Contains(ActionBlockDoorState.PropSetLockState))
			{
				this.handleLock = true;
				properties.ParseBool(ActionBlockDoorState.PropSetLockState, ref this.setLocked);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBlockDoorState
			{
				setOpen = this.setOpen,
				setLocked = this.setLocked,
				handleLock = this.handleLock
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool setOpen = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool setLocked;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool handleLock;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSetOpenState = "set_open";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSetLockState = "set_lock";
	}
}
