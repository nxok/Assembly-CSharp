using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CaptionedTextBox : XUiController
{
	public string Text
	{
		get
		{
			return this.textInput.Text;
		}
		set
		{
			this.textInput.Text = value;
		}
	}

	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public override void Init()
	{
		base.Init();
		this.CustomAttributes.EntryModified += this.OnCustomAttributeModified;
		this.textInput.UIInputController.OnHover += this.Event_OnHover;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCustomAttributeModified(object _sender, DictionaryChangedEventArgs<string, object> _e)
	{
		this.IsDirty = true;
	}

	[XuiBindEvent("OnInteraction", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAnyEvent(XUiController _sender, EXUiControllerInteractionType _type)
	{
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnHover(XUiController _sender, bool _isOver)
	{
		this.IsDirty = true;
	}

	[XuiBindEvent("OnInputSelectedHandler", "textInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnInputSelected(XUiController _sender, bool _selected)
	{
		if (!_selected)
		{
			this.switchToButtonIfEmpty();
		}
		this.IsDirty = true;
	}

	[XuiBindEvent("OnChangeHandler", "textInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiEvent_InputOnChangedEventHandler onChangeHandler = this.OnChangeHandler;
		if (onChangeHandler != null)
		{
			onChangeHandler(_sender, _text, _changeFromCode);
		}
		if (!this.textInput.IsSelected)
		{
			this.switchToButtonIfEmpty();
		}
	}

	[XuiBindEvent("OnSubmitHandler", "textInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Event_OnSubmitChanged(XUiController _sender, string _text)
	{
		XUiEvent_InputOnSubmitEventHandler onSubmitHandler = this.OnSubmitHandler;
		if (onSubmitHandler == null)
		{
			return;
		}
		onSubmitHandler(_sender, _text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void switchToButtonIfEmpty()
	{
		if (!string.IsNullOrWhiteSpace(this.textInput.Text))
		{
			return;
		}
		this.textInput.Text = "";
		ThreadManager.RunTaskAfterFrames(delegate
		{
			this.IsDirty = true;
		}, 1);
	}

	[XuiXmlBinding("box_enabled")]
	public bool BindingEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiView viewComponent = this.viewComponent;
			return viewComponent == null || viewComponent.Enabled;
		}
	}

	[XuiXmlBinding("box_hovered")]
	public bool BindingHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_TextInput xuiC_TextInput = this.textInput;
			return xuiC_TextInput != null && xuiC_TextInput.UIInputController.ViewComponent.IsHovered;
		}
	}

	[XuiXmlBinding("box_empty")]
	public bool BindingEmpty
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_TextInput xuiC_TextInput = this.textInput;
			return string.IsNullOrEmpty((xuiC_TextInput != null) ? xuiC_TextInput.Text : null);
		}
	}

	[XuiXmlBinding("box_selected")]
	public bool BindingSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_TextInput xuiC_TextInput = this.textInput;
			return xuiC_TextInput != null && xuiC_TextInput.IsSelected;
		}
	}

	[XuiXmlBinding("box_attributes")]
	public ObservableDictionary<string, object> BindingCustomAttributes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CustomAttributes;
		}
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput textInput;
}
