using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.dewCatcherWindow = base.GetChildByType<XUiC_DewCollectorWindow>();
		this.dewCollectorModGrid = base.GetChildByType<XUiC_DewCollectorModGrid>();
		this.collectorStacks = base.GetChildrenByType<XUiC_DewCollectorStack>(null);
		this.fuelWindow = base.GetChildById("windowCollectorFuel");
		this.fuelWindowTitle = this.fuelWindow.GetChildById("windowCollectorFuelTitle").ViewComponent as XUiV_Label;
		this.fuelWindowSprite = this.fuelWindow.GetChildById("windowCollectorFuelSprite").ViewComponent as XUiV_Sprite;
		this.collectorFuelGrid = this.fuelWindow.GetChildByType<XUiC_CollectorFuelGrid>();
		this.catalystWindow = base.GetChildById("windowCollectorCatalyst");
		this.catalystWindowTitle = this.catalystWindow.GetChildById("windowCollectorFuelTitle").ViewComponent as XUiV_Label;
		this.catalystWindowSprite = this.catalystWindow.GetChildById("windowCollectorFuelSprite").ViewComponent as XUiV_Sprite;
		this.collectorCatalyst = this.catalystWindow.GetChildByType<XUiC_CollectorFuelGrid>();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public void SetTileEntity(TileEntityCollector _te)
	{
		this.te = _te;
		BlockCollector blockCollector = this.te.blockValue.Block as BlockCollector;
		this.fuelWindowTitle.Text = Localization.Get(blockCollector.FuelTitleText, false, null);
		this.fuelWindowSprite.UIAtlas = blockCollector.FuelTitleSpriteAtlas;
		this.fuelWindowSprite.SpriteName = blockCollector.FuelTitleSprite;
		this.catalystWindowTitle.Text = Localization.Get(blockCollector.CatalystTitleText, false, null);
		this.catalystWindowSprite.UIAtlas = blockCollector.CatalystTitleSpriteAtlas;
		this.catalystWindowSprite.SpriteName = blockCollector.CatalystTitleSprite;
		this.dewCatcherWindow.SetTileEntity(_te);
		this.dewCollectorModGrid.SetTileEntity(_te);
		this.collectorFuelGrid.SetTileEntity(_te);
		this.collectorCatalyst.SetTileEntity(_te);
		int num = this.collectorStacks.Length;
		for (int i = 0; i < num; i++)
		{
			XUiC_DewCollectorStack xuiC_DewCollectorStack = this.collectorStacks[i];
			xuiC_DewCollectorStack.SetStandardFillColor(blockCollector.ItemBackgroundColor);
			xuiC_DewCollectorStack.SetSprite(blockCollector.ItemIconBackdrop[i % blockCollector.ItemIconBackdrop.Length]);
		}
		this.lootingHeader = Localization.Get(blockCollector.LootLabelKey, false, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OpenContainer()
	{
		base.OnOpen();
		this.xui.playerUI.windowManager.Open("backpack", false);
		this.dewCatcherWindow.ViewComponent.IsVisible = true;
		this.dewCatcherWindow.OpenContainer();
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(this.lootingHeader);
		}
		this.dewCatcherWindow.ViewComponent.IsVisible = true;
	}

	public override void OnOpen()
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader("LOOTING");
		}
		this.OpenContainer();
		BlockCollector blockCollector = this.te.blockValue.Block as BlockCollector;
		if (blockCollector != null)
		{
			this.fuelWindow.ViewComponent.IsVisible = blockCollector.UsesFuel();
			this.catalystWindow.ViewComponent.IsVisible = blockCollector.UsesCatalyst();
			string openSound = blockCollector.OpenSound;
			Manager.BroadcastPlayByLocalPlayer(this.te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, openSound);
			this.doEvent(blockCollector.ActivationEvent);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close("backpack");
		this.te.ToWorldPos();
		this.te.SetUserAccessing(false);
		this.te.SetModified();
		LockManager.Instance.UnlockRequestLocal();
		if (this.te.blockValue.Block != null)
		{
			BlockCollector blockCollector = this.te.blockValue.Block as BlockCollector;
			if (blockCollector != null)
			{
				string closeSound = blockCollector.CloseSound;
				Manager.BroadcastPlayByLocalPlayer(this.te.ToWorldPos().ToVector3() + Vector3.one * 0.5f, closeSound);
				string runningSound = blockCollector.RunningSound;
				this.doEvent(blockCollector.CloseEvent);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doEvent(string activationEvent)
	{
		if (!string.IsNullOrEmpty(activationEvent))
		{
			List<Tuple<string, string>> list = new List<Tuple<string, string>>();
			int num = 1;
			foreach (ItemStack itemStack in this.te.ModSlots)
			{
				string text = "";
				if (!itemStack.IsEmpty())
				{
					text = itemStack.itemValue.ItemClass.Name;
				}
				list.Add(new Tuple<string, string>(string.Format("_Mod{0}Name", num++), text));
			}
			GameEventManager.Current.HandleAction(activationEvent, null, null, false, this.te.ToWorldCenterPos(), "", "", false, true, "", null, list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_DewCollectorWindowGroup GetInstance(XUi _xuiInstance = null)
	{
		if (_xuiInstance == null)
		{
			_xuiInstance = LocalPlayerUI.GetUIForPrimaryPlayer().xui;
		}
		return (XUiC_DewCollectorWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_DewCollectorWindowGroup.ID);
	}

	public static Vector3i GetTeBlockPos(XUi _xuiInstance = null)
	{
		TileEntityCollector tileEntityCollector = XUiC_DewCollectorWindowGroup.GetInstance(_xuiInstance).te;
		if (tileEntityCollector == null)
		{
			return Vector3i.zero;
		}
		return tileEntityCollector.ToWorldPos();
	}

	public static void CloseIfOpenAtPos(Vector3i _blockPos, XUi _xuiInstance = null)
	{
		GUIWindowManager windowManager = XUiC_DewCollectorWindowGroup.GetInstance(_xuiInstance).xui.playerUI.windowManager;
		if (windowManager.IsWindowOpen(XUiC_DewCollectorWindowGroup.ID) && XUiC_DewCollectorWindowGroup.GetTeBlockPos(null) == _blockPos)
		{
			windowManager.Close(XUiC_DewCollectorWindowGroup.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorWindow dewCatcherWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorModGrid dewCollectorModGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label headerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DewCollectorStack[] collectorStacks;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector te;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController fuelWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label fuelWindowTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite fuelWindowSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CollectorFuelGrid collectorFuelGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController catalystWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label catalystWindowTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite catalystWindowSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CollectorFuelGrid collectorCatalyst;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootingHeader;

	public static string ID = "dewcollector";

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalOpenTime;
}
