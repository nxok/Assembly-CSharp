using System;
using UnityEngine.Scripting;

[Preserve]
public class CreativeActionEntryFavorite : BaseItemActionEntry
{
	public CreativeActionEntryFavorite(XUiController _controller, int _stackID)
		: base(_controller, "lblContextActionFavorite", "server_favorite", BaseItemActionEntry.GamepadShortCut.DPadRight, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.stackID = (ushort)_stackID;
	}

	public override void OnActivated()
	{
		EntityPlayer entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if (entityPlayer.favoriteCreativeStacks.Contains(this.stackID))
		{
			entityPlayer.favoriteCreativeStacks.Remove(this.stackID);
		}
		else
		{
			entityPlayer.favoriteCreativeStacks.Add(this.stackID);
		}
		XUiC_Creative2Window childByType = base.ItemController.WindowGroup.Controller.GetChildByType<XUiC_Creative2Window>();
		if (childByType == null)
		{
			return;
		}
		childByType.RefreshView();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ushort stackID;
}
