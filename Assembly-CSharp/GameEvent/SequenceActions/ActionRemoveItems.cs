using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRemoveItems : ActionBaseItemAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
		{
			if (stack.IsEmpty() || (!(this.itemTags == "") && !stack.itemValue.ItemClass.HasAnyTags(this.fastItemTags)))
			{
				return false;
			}
			if (this.count != -1)
			{
				if (this.countType == ActionBaseItemAction.CountTypes.Items)
				{
					if (stack.count >= this.count)
					{
						stack.count -= this.count;
						this.count = 0;
						this.isFinished = true;
						if (stack.count == 0)
						{
							stack = ItemStack.Empty;
						}
					}
					else
					{
						this.count -= stack.count;
						stack = ItemStack.Empty;
					}
				}
				else
				{
					stack = ItemStack.Empty;
					this.count--;
					if (this.count == 0)
					{
						this.isFinished = true;
					}
				}
				return true;
			}
			stack = ItemStack.Empty;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool HandleItemValueChange(ref ItemValue itemValue, EntityPlayer player)
		{
			if (!itemValue.IsEmpty() && (this.itemTags == "" || itemValue.ItemClass.HasAnyTags(this.fastItemTags)))
			{
				itemValue = ItemValue.None;
				if (this.count != -1)
				{
					this.count--;
					if (this.count == 0)
					{
						this.isFinished = true;
					}
				}
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRemoveItems();
		}
	}
}
