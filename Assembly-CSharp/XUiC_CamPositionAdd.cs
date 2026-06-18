using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CamPositionAdd : XUiController
{
	public override void Init()
	{
		base.Init();
		this.parentListWindow = base.GetParentByType<XUiC_CamPositionsList>();
		XUiController childById = base.GetChildById("txtName");
		this.txtName = ((childById != null) ? childById.GetChildByType<XUiC_TextInput>() : null);
		XUiController childById2 = base.GetChildById("txtComment");
		this.txtComment = ((childById2 != null) ? childById2.GetChildByType<XUiC_TextInput>() : null);
		if (this.txtName != null)
		{
			this.txtName.OnChangeHandler += this.Name_OnChangeHandler;
			this.txtName.OnSubmitHandler += this.Inputs_OnSubmitHandler;
			this.txtName.SelectOnTab = this.txtComment;
		}
		if (this.txtComment != null)
		{
			this.txtComment.OnChangeHandler += this.Comment_OnChangeHandler;
			this.txtComment.OnSubmitHandler += this.Inputs_OnSubmitHandler;
			this.txtComment.SelectOnTab = this.txtName;
		}
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnAdd") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += delegate(XUiController _, int _)
			{
				this.add();
			};
		}
		base.OnVisiblity += this.OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= this.OnVisibilityChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Name_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.name = _text;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Comment_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.comment = _text;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Inputs_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.add();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clear()
	{
		if (this.txtName != null)
		{
			this.txtName.Text = string.Empty;
		}
		if (this.txtComment != null)
		{
			this.txtComment.Text = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void add()
	{
		if (!this.validateInput())
		{
			return;
		}
		this.parentListWindow.Add(this.name, this.comment);
		this.parentListWindow.ShowAddCamPositionWindow = false;
		this.clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateInput()
	{
		return this.name.Trim().Length > 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "valid_input")
		{
			_value = this.validateInput().ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public void OnVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		GUIWindow modalWindow = this.xui.playerUI.windowManager.GetModalWindow();
		if (modalWindow == null)
		{
			return;
		}
		if (!_visibleInScene)
		{
			if (this.modalWindowGroupEscCloseableBefore != null)
			{
				modalWindow.isEscClosable = this.modalWindowGroupEscCloseableBefore.Value;
			}
			return;
		}
		this.modalWindowGroupEscCloseableBefore = new bool?(modalWindow.isEscClosable);
		modalWindow.isEscClosable = false;
		XUiC_TextInput xuiC_TextInput = this.txtName;
		if (xuiC_TextInput == null)
		{
			return;
		}
		xuiC_TextInput.SelectOrVirtualKeyboard(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.modalWindowGroupEscCloseableBefore = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.UpdateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInput()
	{
		if (!this.viewComponent.IsVisible)
		{
			return;
		}
		if (this.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || this.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			this.parentListWindow.ShowAddCamPositionWindow = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CamPositionsList parentListWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtComment;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool? modalWindowGroupEscCloseableBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string comment = string.Empty;
}
