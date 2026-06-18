using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		if (base.GetChildByType<XUiC_WindowNonPagingHeader>() != null)
		{
			this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		}
		if (base.GetChildByType<XUiC_CharacterCosmeticWindow>() != null)
		{
			this.characterCosmeticWindow = base.GetChildByType<XUiC_CharacterCosmeticWindow>();
		}
		if (base.GetChildByType<XUiC_CharacterCosmeticsListWindow>() != null)
		{
			this.characterCosmeticListWindow = base.GetChildByType<XUiC_CharacterCosmeticsListWindow>();
		}
	}

	public static void Open(XUi xui, EquipmentSlots slot)
	{
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		windowManager.Open("cosmetics", true);
		((XUiC_CharacterCosmeticWindowGroup)((XUiWindowGroup)windowManager.GetWindow("cosmetics")).Controller).SetEquipSlot(slot);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(Localization.Get("xuiCosmetics", false, null));
		}
		if (this.xui.PlayerEquipment != null)
		{
			this.xui.PlayerEquipment.IsOpen = true;
		}
	}

	public void SetEquipSlot(EquipmentSlots equipSlot)
	{
		if (equipSlot != EquipmentSlots.Count)
		{
			this.characterCosmeticListWindow.SetCategory(equipSlot);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.xui.PlayerEquipment != null)
		{
			this.xui.PlayerEquipment.IsOpen = false;
		}
	}

	public void ResetPreview()
	{
		this.characterCosmeticWindow.MakePreview();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticWindow characterCosmeticWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticsListWindow characterCosmeticListWindow;
}
