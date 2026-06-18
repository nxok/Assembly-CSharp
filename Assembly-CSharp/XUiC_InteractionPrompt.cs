using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InteractionPrompt : XUiController
{
	public string Text
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.text;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != this.text)
			{
				this.text = value;
				this.IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_InteractionPrompt.ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "text")
		{
			_value = this.text;
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static void SetText(LocalPlayerUI _playerUi, string _text)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiController xuiController = _playerUi.xui.FindWindowGroupByName(XUiC_InteractionPrompt.ID);
			XUiC_InteractionPrompt xuiC_InteractionPrompt = ((xuiController != null) ? xuiController.GetChildByType<XUiC_InteractionPrompt>() : null);
			if (xuiC_InteractionPrompt == null)
			{
				return;
			}
			xuiC_InteractionPrompt.Text = _text;
			if (string.IsNullOrEmpty(_text))
			{
				_playerUi.windowManager.Close(XUiC_InteractionPrompt.ID);
				return;
			}
			_playerUi.windowManager.Open(XUiC_InteractionPrompt.ID, false);
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string text;
}
