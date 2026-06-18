using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BagStorageWindowGroup : XUiController
{
	public Bag Bag
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		this.containerWindow = base.GetChildByType<XUiC_BagContainer>();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		this.timerWindow = this.xui.GetChildByType<XUiC_Timer>();
	}

	public static void Open(XUi _xui, Entity _entity, Bag _bag, LootContainer _lootContainer, string _containerName = null, Action _onModified = null, Action _onClose = null, Func<bool> _interactionCheck = null, bool _runOpenTimer = false)
	{
		if (_containerName == null)
		{
			_containerName = Localization.Get("xuiLoot", false, null);
		}
		XUiC_BagStorageWindowGroup ctrl = _xui.FindWindowGroupByName(XUiC_BagStorageWindowGroup.ID).GetChildByType<XUiC_BagStorageWindowGroup>();
		ctrl.Entity = _entity;
		ctrl.Bag = _bag;
		ctrl.lootContainer = _lootContainer;
		ctrl.onCloseCallback = _onClose;
		ctrl.checkInteraction = _interactionCheck;
		ctrl.playOnOpenSound = !_runOpenTimer;
		ctrl.containerWindow.SetBag(_bag, _lootContainer, _containerName, _onModified);
		if (!_runOpenTimer || _lootContainer == null)
		{
			ctrl.OpenStorageWindow();
			return;
		}
		EntityPlayer entityPlayer = _xui.playerUI.entityPlayer;
		float num = EffectManager.GetValue(PassiveEffects.ScavengingTime, null, (entityPlayer != null && entityPlayer.IsCrouching) ? (_lootContainer.openTime * 1.5f) : _lootContainer.openTime, entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * LootContainer.LootTimerModifier;
		if (num == 0f)
		{
			num = 0.01f;
		}
		if (num <= 0f)
		{
			ctrl.OpenStorageWindow();
			return;
		}
		_xui.playerUI.windowManager.Close("backpack");
		_xui.playerUI.windowManager.Open("CalloutGroup", false);
		TimerEventData timerEventData = new TimerEventData
		{
			CloseOnHit = true
		};
		timerEventData.FullTimeFinishEvent += delegate(TimerEventData _)
		{
			ctrl.OpenStorageWindow();
		};
		timerEventData.CloseEvent += delegate(TimerEventData _)
		{
			ctrl.CancelOpen();
		};
		XUiC_Timer.OpenTimer(_xui, num, timerEventData, -1f, Localization.Get("xuiOpeningLoot", false, null), false);
		if (!string.IsNullOrEmpty(ctrl.lootContainer.soundOpen))
		{
			Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, ctrl.lootContainer.soundOpen);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenStorageWindow()
	{
		this.xui.playerUI.windowManager.Open(XUiC_BagStorageWindowGroup.ID, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CancelOpen()
	{
		Action action = this.onCloseCallback;
		if (action != null)
		{
			action();
		}
		this.ClearPendingState();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.checkInteraction != null && !this.checkInteraction())
		{
			this.xui.playerUI.windowManager.Close(XUiC_BagStorageWindowGroup.ID);
			return;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.ignoreCloseSound = false;
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		if (EffectManager.GetValue(PassiveEffects.DisableLoot, null, 0f, entityPlayer, null, this.Entity.EntityClass.Tags, true, true, true, true, true, 1, true, false) > 0f)
		{
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			GUIWindowManager windowManager = this.xui.playerUI.windowManager;
			this.ignoreCloseSound = true;
			windowManager.Close("timer");
			windowManager.Close(XUiC_BagStorageWindowGroup.ID);
			return;
		}
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(Localization.Get("xuiStorage", false, null));
		}
		if (this.lootContainer == null)
		{
			return;
		}
		this.xui.playerUI.windowManager.Open("backpack", false);
		XUiC_BagContainer xuiC_BagContainer = this.containerWindow;
		if (((xuiC_BagContainer != null) ? xuiC_BagContainer.ViewComponent : null) != null)
		{
			this.containerWindow.ViewComponent.UiTransform.gameObject.SetActive(true);
			this.containerWindow.ViewComponent.IsVisible = true;
		}
		this.xui.RecenterWindowGroup(this.windowGroup, false);
		if (this.playOnOpenSound && !string.IsNullOrEmpty(this.lootContainer.soundOpen))
		{
			Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, this.lootContainer.soundOpen);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close("backpack");
		if (!this.ignoreCloseSound && this.lootContainer != null && !string.IsNullOrEmpty(this.lootContainer.soundOpen))
		{
			Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, this.lootContainer.soundOpen);
		}
		Action action = this.onCloseCallback;
		if (action != null)
		{
			action();
		}
		this.ClearPendingState();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearPendingState()
	{
		this.ignoreCloseSound = false;
		this.playOnOpenSound = false;
		XUiC_BagContainer xuiC_BagContainer = this.containerWindow;
		if (((xuiC_BagContainer != null) ? xuiC_BagContainer.ViewComponent : null) != null)
		{
			this.containerWindow.ViewComponent.UiTransform.gameObject.SetActive(true);
			this.containerWindow.ViewComponent.IsVisible = true;
		}
		this.Bag = null;
		this.lootContainer = null;
		this.onCloseCallback = null;
		this.checkInteraction = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BagContainer containerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Timer timerWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playOnOpenSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreCloseSound;

	public static string ID = "bagStorage";

	public Entity Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public LootContainer lootContainer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> checkInteraction;
}
