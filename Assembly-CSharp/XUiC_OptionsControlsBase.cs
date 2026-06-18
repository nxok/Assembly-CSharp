using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_OptionsControlsBase : XUiC_OptionsDialogBase
{
	public static event Action OnSettingsChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doResetToDefaultsInternal()
	{
		XUiC_OptionsControlsBase.XUiC_BindingEntry[] childControllers = this.TabSelector.SelectedTab.GetChildControllers<XUiC_OptionsControlsBase.XUiC_BindingEntry>("", null);
		for (int i = 0; i < childControllers.Length; i++)
		{
			childControllers[i].ResetToDefault();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void createControlsEntries();

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		PlayerMoveController.UpdateControlsOptions();
		this.storeCurrentBindings();
		GameOptionsControls.Save();
		Action onSettingsChanged = XUiC_OptionsControlsBase.OnSettingsChanged;
		if (onSettingsChanged == null)
		{
			return;
		}
		onSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void storeCurrentBindings()
	{
		this.actionBindingsOnOpen.Clear();
		foreach (PlayerActionsBase playerActionsBase in PlatformManager.NativePlatform.Input.ActionSets)
		{
			this.actionBindingsOnOpen.Add(playerActionsBase.Save());
		}
	}

	public override void OnOpen()
	{
		if (!this.initialized)
		{
			this.createControlsEntries();
			this.initialized = true;
		}
		ThreadManager.StartCoroutine(this.storeBindingsLater());
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator storeBindingsLater()
	{
		yield return null;
		yield return null;
		this.storeCurrentBindings();
		yield break;
	}

	public override void OnClose()
	{
		base.OnClose();
		PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(this.actionBindingsOnOpen);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionsControlsBase()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<string> actionBindingsOnOpen = new List<string>();

	[Preserve]
	public class XUiC_BindingEntry : XUiController
	{
		public PlayerAction Action
		{
			get
			{
				return this.action;
			}
			set
			{
				if (this.action == value)
				{
					return;
				}
				if (this.action != null)
				{
					this.action.OnBindingsChanged -= this.actionBindingsChanged;
				}
				this.action = value;
				this.actionData = (PlayerActionData.ActionUserData)((value != null) ? value.UserData : null);
				if (this.action != null)
				{
					this.action.OnBindingsChanged += this.actionBindingsChanged;
				}
				this.IsDirty = true;
			}
		}

		[XuiXmlAttribute("for_controller", false)]
		public bool IsControllerBinding
		{
			get
			{
				return this.isControllerBinding;
			}
			set
			{
				if (value == this.isControllerBinding)
				{
					return;
				}
				this.isControllerBinding = value;
				this.IsDirty = true;
			}
		}

		[XuiXmlBinding("has_action")]
		public bool HasAction
		{
			get
			{
				return this.action != null;
			}
		}

		[XuiXmlBinding("allow_rebind")]
		public bool AllowRebind
		{
			get
			{
				PlayerActionData.ActionUserData actionUserData = this.actionData;
				return actionUserData != null && actionUserData.allowRebind;
			}
		}

		[XuiXmlBinding("action_label")]
		public string ActionLabel
		{
			get
			{
				PlayerActionData.ActionUserData actionUserData = this.actionData;
				return ((actionUserData != null) ? actionUserData.LocalizedName : null) ?? "";
			}
		}

		[XuiXmlBinding("action_description")]
		public string ActionDescription
		{
			get
			{
				PlayerActionData.ActionUserData actionUserData = this.actionData;
				return ((actionUserData != null) ? actionUserData.LocalizedDescription : null) ?? "";
			}
		}

		[XuiXmlBinding("action_binding")]
		public string ActionBinding
		{
			get
			{
				return this.bindingString ?? "";
			}
		}

		public override void Init()
		{
			base.Init();
			base.registerForInputStyleChanges();
		}

		public override void OnOpen()
		{
			base.OnOpen();
			this.frameOpened = Time.frameCount;
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			if (this.IsDirty)
			{
				this.IsDirty = false;
				this.bindingString = this.action.GetBindingString(this.isControllerBinding, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null);
				base.RefreshBindings();
			}
		}

		public override void Cleanup()
		{
			base.Cleanup();
			if (this.action != null)
			{
				this.action.OnBindingsChanged -= this.actionBindingsChanged;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
		{
			base.inputStyleChanged(_oldStyle, _newStyle);
			this.IsDirty = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void actionBindingsChanged()
		{
			if (Time.frameCount < this.frameOpened + 2)
			{
				return;
			}
			this.IsDirty = true;
			this.parentControls.SetChanged();
		}

		public void ResetToDefault()
		{
			PlayerAction playerAction = this.action;
			if (playerAction == null)
			{
				return;
			}
			playerAction.ResetBindings();
		}

		[XuiBindEvent("OnPress", "button")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void newBindingClick(XUiController _sender, int _mouseButton)
		{
			if (!this.HasAction || !this.actionData.allowRebind)
			{
				return;
			}
			XUiC_OptionsControlsNewBinding.GetNewBinding(this.xui, this.action, this.isControllerBinding);
		}

		[XuiBindEvent("OnPress", "unbind")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void unbindButtonClick(XUiController _sender, int _mouseButton)
		{
			if (!this.HasAction || !this.actionData.allowRebind)
			{
				return;
			}
			this.action.UnbindBindingsOfType(this.isControllerBinding);
		}

		[XuiBindParent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_OptionsDialogBase parentControls;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerAction action;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isControllerBinding;

		[PublicizedFrom(EAccessModifier.Private)]
		public int frameOpened;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerActionData.ActionUserData actionData;

		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingString;

		[XuiBindComponent("unbind", true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_Button unbind;

		[XuiBindComponent("background", true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_Button button;
	}
}
