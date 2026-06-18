using System;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxSettingsSaveAsPreset : XUiController
{
	[XuiXmlAttribute("sandboxcode", false)]
	[XuiXmlBinding("sandboxcode")]
	public string SandboxCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.sandboxCode ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (string.Equals(value, this.sandboxCode))
			{
				return;
			}
			this.sandboxCode = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("name_valid")]
	public bool NameValid
	{
		get
		{
			return this.nameValid;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.nameValid == value)
			{
				return;
			}
			this.nameValid = value;
			this.IsDirty = true;
		}
	}

	public string Description
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.description ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (string.Equals(value, this.description))
			{
				return;
			}
			this.description = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.txtPresetName.UIInput.onValidate = new UIInput.OnValidate(GameUtils.ValidateGameNameInput);
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.onConfirm = null;
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.NameValid)
		{
			return;
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		SandboxOptionPreset sandboxOptionPreset = new SandboxOptionPreset
		{
			Name = this.txtPresetName.Text,
			Description = this.txtDescription.Text,
			IsUserPreset = true,
			Group = "User",
			Icon = "Data/Sandbox/icons/user_custom"
		};
		this.sandboxManager.LoadOptionsFromCode(this.SandboxCode, sandboxOptionPreset);
		this.sandboxManager.SavePreset(sandboxOptionPreset, true, true);
		Action<string> action = this.onConfirm;
		if (action != null)
		{
			action(sandboxOptionPreset.Name);
		}
		this.onConfirm = null;
	}

	[XuiBindEvent("OnSubmitHandler", "txtPresetName")]
	[XuiBindEvent("OnSubmitHandler", "txtDescription")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnConfirmCreate_OnPressed(this, -1);
	}

	[XuiBindEvent("OnChangeHandler", "txtPresetName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		string text = _text.Trim();
		this.NameValid = text.Length > 0 && text.IndexOf('.') < 0 && this.sandboxManager.GetPreset(text) == null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				this.BtnCancelCreate_OnPressed(this, -1);
			}
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				this.BtnConfirmCreate_OnPressed(null, 0);
			}
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.NameValid = false;
		this.txtPresetName.Text = "";
		this.txtDescription.Text = this.Description ?? "";
		this.txtPresetName.SelectOrVirtualKeyboard(true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.Description = "";
	}

	public static void Open(XUi _xui, string _sandboxCode, string _description, Action<string> _onConfirm = null)
	{
		XUiC_SandboxSettingsSaveAsPreset childByType = _xui.GetChildByType<XUiC_SandboxSettingsSaveAsPreset>();
		childByType.SandboxCode = _sandboxCode;
		childByType.Description = _description;
		childByType.onConfirm = _onConfirm;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, false);
	}

	[XuiBindComponent("presetName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtPresetName;

	[XuiBindComponent("description", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtDescription;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandboxCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public string description;
}
