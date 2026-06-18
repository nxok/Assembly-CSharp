using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryShowChallenge : BaseItemActionEntry
{
	public ItemActionEntryShowChallenge(XUiController controller, RecipeUnlockData unlockData)
		: base(controller, "Challenge", "ui_game_symbol_challenge", BaseItemActionEntry.GamepadShortCut.DPadUp, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.UnlockData = unlockData;
		base.ActionName = Localization.Get("challenge", false, null);
		base.IconName = "ui_game_symbol_challenge";
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		xui.playerUI.windowManager.Close(XUiC_LootWindowGroup.ID);
		XUiC_WindowSelector.OpenSelectorAndWindow(xui.playerUI.entityPlayer, "challenges");
		XUiC_ChallengeEntryListWindow childByType = xui.GetChildByType<XUiC_ChallengeEntryListWindow>();
		if (childByType != null)
		{
			childByType.SetSelectedByUnlockData(this.UnlockData);
		}
	}

	public RecipeUnlockData UnlockData;
}
