using System;
using UnityEngine.Scripting;

[Preserve]
public class BaseItemActionEntry
{
	public string ActionName { get; set; }

	public string IconName { get; set; }

	public bool Enabled { get; set; }

	public string SoundName { get; set; }

	public string DisabledSound { get; set; }

	public XUiController ItemController { get; set; }

	public XUiC_ItemActionEntry ParentItem { get; set; }

	public XUiC_ItemActionList ParentActionList { get; set; }

	public BaseItemActionEntry.GamepadShortCut ShortCut { get; set; }

	public BaseItemActionEntry(XUiController itemController, string actionName, string spriteName, BaseItemActionEntry.GamepadShortCut shortcut = BaseItemActionEntry.GamepadShortCut.None, string soundName = "crafting/craft_click_craft", string disabledSoundName = "ui/ui_denied")
	{
		this.ItemController = itemController;
		this.ActionName = Localization.Get(actionName, false, null);
		this.IconName = spriteName;
		this.SoundName = soundName;
		this.DisabledSound = disabledSoundName;
		this.Enabled = true;
		this.ShortCut = shortcut;
	}

	public virtual void RefreshEnabled()
	{
		if (this.ItemController is XUiC_ItemStack)
		{
			this.Enabled = !((XUiC_ItemStack)this.ItemController).StackLock;
		}
	}

	public virtual void OnActivated()
	{
	}

	public virtual void OnDisabledActivate()
	{
	}

	public virtual void OnTimerCompleted()
	{
	}

	public virtual void DisableEvents()
	{
	}

	public enum GamepadShortCut
	{
		DPadUp,
		DPadLeft,
		DPadRight,
		DPadDown,
		None,
		Max
	}
}
