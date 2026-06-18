using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsControlsNewBinding : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_OptionsControlsNewBinding.id = base.WindowGroup.Id;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnNewBinding")).OnPressed += this.BtnNewBinding_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnGrabBinding")).OnPressed += this.BtnGrabBinding_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNewBinding_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.startGetBinding();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnGrabBinding_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.conflictingAction.ClearInputState();
		this.conflictingAction.RemoveBinding(this.binding);
		this.action.UnbindBindingsOfType(this.forController);
		this.action.AddBinding(this.binding);
		ThreadManager.StartCoroutine(this.closeNextFrame());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startGetBinding()
	{
		this.xui.playerUI.windowManager.IsInputLocked = true;
		InputUtils.EnableAllPlayerActions(false);
		GameManager.Instance.SetCursorEnabledOverride(true, false);
		this.rectInUse.IsVisible = false;
		this.conflictingAction = null;
		this.binding = null;
		this.action.Owner.ListenOptions.IncludeUnknownControllers = false;
		this.action.Owner.ListenOptions.IncludeMouseButtons = !this.forController;
		this.action.Owner.ListenOptions.IncludeMouseScrollWheel = !this.forController;
		this.action.Owner.ListenOptions.IncludeKeys = true;
		this.action.Owner.ListenOptions.OnBindingFound = new Func<PlayerAction, BindingSource, bool>(this.onBindingReceived);
		this.action.ListenForBinding();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stoppedGetBinding(bool _alsoStopListening = true)
	{
		if (_alsoStopListening)
		{
			this.action.StopListeningForBinding();
		}
		InputUtils.EnableAllPlayerActions(true);
		this.xui.playerUI.windowManager.IsInputLocked = false;
		GameManager.Instance.SetCursorEnabledOverride(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onBindingReceived(PlayerAction _action, BindingSource _binding)
	{
		if (this.bindingAbortActions.Contains(_binding))
		{
			Log.Out("Abort action pressed, aborting listening for new binding for {0}", new object[] { _action.Name });
			this.stoppedGetBinding(true);
			ThreadManager.StartCoroutine(this.closeNextFrame());
			return false;
		}
		if (this.forController && (_binding is KeyBindingSource || _binding is MouseBindingSource))
		{
			Log.Out("Cannot accept key or mouse for controller binding");
			return false;
		}
		if (!this.forController && _binding is DeviceBindingSource)
		{
			Log.Out("Cannot accept device binding source for keyboard/mouse binding");
			return false;
		}
		foreach (BindingSource bindingSource in this.bindingForbidden)
		{
			if (_binding == bindingSource)
			{
				Log.Out("Binding {0} not allowed", new object[] { _binding.Name });
				return false;
			}
		}
		if (this.forController != (_binding.BindingSourceType == 1))
		{
			Log.Out("New binding ({0}) doesn't match expected input device type ({1})", new object[]
			{
				_binding.BindingSourceType.ToStringCached<BindingSourceType>(),
				this.forController ? 1.ToStringCached<BindingSourceType>() : 2.ToStringCached<BindingSourceType>()
			});
			return false;
		}
		if (_action.HasBinding(_binding))
		{
			Log.Out("Binding {0} already bound to the current action {1}", new object[] { _binding.Name, _action.Name });
			this.stoppedGetBinding(true);
			ThreadManager.StartCoroutine(this.closeNextFrame());
			return false;
		}
		if (this.alreadyBound(_binding, _action))
		{
			Log.Out("Binding {0} already bound to the action {1}", new object[]
			{
				_binding.Name,
				this.conflictingAction.Name
			});
			this.stoppedGetBinding(true);
			return false;
		}
		_action.UnbindBindingsOfType(this.forController);
		this.stoppedGetBinding(false);
		ThreadManager.StartCoroutine(this.closeNextFrame());
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alreadyBound(BindingSource _binding, PlayerAction _selfAction)
	{
		PlayerAction playerAction = _selfAction.Owner.BindingUsed(_binding);
		if (playerAction == null && _selfAction.Owner.UserData != null)
		{
			PlayerActionsBase[] bindingsConflictWithSet = ((PlayerActionData.ActionSetUserData)_selfAction.Owner.UserData).bindingsConflictWithSet;
			for (int i = 0; i < bindingsConflictWithSet.Length; i++)
			{
				playerAction = bindingsConflictWithSet[i].BindingUsed(_binding);
				if (playerAction != null)
				{
					break;
				}
			}
		}
		if (playerAction == null)
		{
			return false;
		}
		PlayerActionData.ActionUserData actionUserData = (PlayerActionData.ActionUserData)_selfAction.UserData;
		PlayerActionData.ActionUserData actionUserData2 = (PlayerActionData.ActionUserData)playerAction.UserData;
		if (actionUserData.allowMultipleBindings || actionUserData2.allowMultipleBindings)
		{
			PlayerActionSet playerActionSet = this.xui.playerUI.playerInput;
			if (!this.xui.playerUI.playerInput.Actions.Contains(_selfAction) || !this.xui.playerUI.playerInput.Actions.Contains(playerAction))
			{
				if (this.xui.playerUI.playerInput.GUIActions.Actions.Contains(_selfAction) && this.xui.playerUI.playerInput.GUIActions.Actions.Contains(playerAction))
				{
					playerActionSet = this.xui.playerUI.playerInput.GUIActions;
				}
				else if (this.xui.playerUI.playerInput.VehicleActions.Actions.Contains(_selfAction) && this.xui.playerUI.playerInput.VehicleActions.Actions.Contains(playerAction))
				{
					playerActionSet = this.xui.playerUI.playerInput.VehicleActions;
				}
				else
				{
					if (!this.xui.playerUI.playerInput.PermanentActions.Actions.Contains(_selfAction) || !this.xui.playerUI.playerInput.PermanentActions.Actions.Contains(playerAction))
					{
						return false;
					}
					playerActionSet = this.xui.playerUI.playerInput.PermanentActions;
				}
			}
			bool flag = false;
			foreach (PlayerAction playerAction2 in playerActionSet.Actions)
			{
				if (playerAction2 != playerAction && playerAction2 != _selfAction && playerAction2.Bindings.Contains(_binding))
				{
					flag = true;
					playerAction = playerAction2;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		this.rectInUse.IsVisible = true;
		this.binding = playerAction.GetBindingOfType(this.forController);
		this.conflictingAction = playerAction;
		if (this.forController)
		{
			this.lblInUseBy.Text = string.Format(Localization.Get("xuiNewBindingConflictingAction_Controller", false, null), playerAction.GetBindingString(true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null), ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedName);
		}
		else
		{
			this.lblInUseBy.Text = string.Format(Localization.Get("xuiNewBindingConflictingAction", false, null), playerAction.GetBindingString(this.forController, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null), ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedName);
		}
		base.GetChildById("btnNewBinding").SelectCursorElement(true, false);
		this.lblInUseBy.ToolTip = ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedDescription;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeNextFrame()
	{
		yield return null;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startNextFrame()
	{
		yield return null;
		this.startGetBinding();
		yield break;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.forController)
		{
			this.lblAction.Text = string.Format(Localization.Get("xuiNewBindingCurrent_Controller", false, null), ((PlayerActionData.ActionUserData)this.action.UserData).LocalizedName, this.action.GetBindingString(true, PlayerInputManager.InputStyleFromSelectedIconStyle(), XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
		}
		else
		{
			this.lblAction.Text = string.Format(Localization.Get("xuiNewBindingCurrent", false, null), ((PlayerActionData.ActionUserData)this.action.UserData).LocalizedName, this.action.GetBindingString(this.forController, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
		}
		PlayerInputManager.InputStyle inputStyle = PlayerInputManager.InputStyleFromSelectedIconStyle();
		string text2;
		if (this.forController)
		{
			string text = ((inputStyle == PlayerInputManager.InputStyle.PS4) ? "[sp=PS5_Button_Options] / ESC" : "[sp=XB_Button_Back] / ESC");
			text2 = string.Format(Localization.Get("xuiNewBindingAbort_Controller", false, null), text);
		}
		else
		{
			string text3 = ((inputStyle == PlayerInputManager.InputStyle.PS4) ? "ESC / [sp=PS5_Button_Options]" : "ESC / [sp=XB_Button_Back]");
			text2 = string.Format(Localization.Get("xuiNewBindingAbort_Controller", false, null), text3);
		}
		this.lblAbort.Text = text2;
		ThreadManager.StartCoroutine(this.startNextFrame());
	}

	public static void GetNewBinding(XUi _xuiInstance, PlayerAction _action, bool _forController = false)
	{
		XUiC_OptionsControlsNewBinding childByType = _xuiInstance.FindWindowGroupByName(XUiC_OptionsControlsNewBinding.id).GetChildByType<XUiC_OptionsControlsNewBinding>();
		childByType.action = _action;
		childByType.forController = _forController;
		_xuiInstance.playerUI.windowManager.Open(childByType.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id;

	[XuiBindComponent("forAction", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblAction;

	[XuiBindComponent("newBindingAbort", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblAbort;

	[XuiBindComponent("inUse", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Rect rectInUse;

	[XuiBindComponent("inUseBy", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblInUseBy;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction action;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction conflictingAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public BindingSource binding;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BindingSource> bindingAbortActions = new List<BindingSource>
	{
		new DeviceBindingSource(100),
		new DeviceBindingSource(104),
		new DeviceBindingSource(109),
		new DeviceBindingSource(114),
		new KeyBindingSource(new Key[] { 13 })
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BindingSource[] bindingForbidden = new BindingSource[]
	{
		new KeyBindingSource(new Key[] { 14 }),
		new KeyBindingSource(new Key[] { 15 }),
		new KeyBindingSource(new Key[] { 16 }),
		new KeyBindingSource(new Key[] { 17 }),
		new KeyBindingSource(new Key[] { 18 }),
		new KeyBindingSource(new Key[] { 19 }),
		new KeyBindingSource(new Key[] { 20 }),
		new KeyBindingSource(new Key[] { 21 }),
		new KeyBindingSource(new Key[] { 22 }),
		new KeyBindingSource(new Key[] { 23 }),
		new KeyBindingSource(new Key[] { 24 }),
		new KeyBindingSource(new Key[] { 25 }),
		new DeviceBindingSource(101),
		new DeviceBindingSource(100),
		new DeviceBindingSource(1),
		new DeviceBindingSource(2),
		new DeviceBindingSource(3),
		new DeviceBindingSource(4),
		new DeviceBindingSource(6),
		new DeviceBindingSource(7),
		new DeviceBindingSource(8),
		new DeviceBindingSource(9),
		new DeviceBindingSource(107),
		new DeviceBindingSource(106),
		new DeviceBindingSource(109),
		new DeviceBindingSource(104),
		new DeviceBindingSource(113),
		new DeviceBindingSource(114),
		new DeviceBindingSource(258),
		new DeviceBindingSource(102),
		new DeviceBindingSource(302),
		new DeviceBindingSource(301),
		new DeviceBindingSource(304),
		new DeviceBindingSource(303),
		new DeviceBindingSource(115),
		new DeviceBindingSource(117),
		new DeviceBindingSource(108),
		new DeviceBindingSource(116),
		new DeviceBindingSource(111)
	};
}
