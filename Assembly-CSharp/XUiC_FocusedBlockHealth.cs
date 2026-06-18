using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_FocusedBlockHealth : XUiController
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
				this.text = value ?? "";
				this.IsDirty = true;
			}
		}
	}

	public float Fill
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.fill;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != this.fill)
			{
				this.fill = value;
				this.IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_FocusedBlockHealth.ID = base.WindowGroup.Id;
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
		if (!(_bindingName == "fill"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = this.fill.ToCultureInvariantString();
		return true;
	}

	public static void SetData(LocalPlayerUI _playerUi, string _text, float _fill)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiController xuiController = _playerUi.xui.FindWindowGroupByName(XUiC_FocusedBlockHealth.ID);
			XUiC_FocusedBlockHealth xuiC_FocusedBlockHealth = ((xuiController != null) ? xuiController.GetChildByType<XUiC_FocusedBlockHealth>() : null);
			if (xuiC_FocusedBlockHealth == null)
			{
				return;
			}
			xuiC_FocusedBlockHealth.Text = _text;
			xuiC_FocusedBlockHealth.Fill = _fill;
			if (_text == null)
			{
				_playerUi.windowManager.Close(XUiC_FocusedBlockHealth.ID);
				return;
			}
			_playerUi.windowManager.Open(XUiC_FocusedBlockHealth.ID, false);
		}
	}

	public static bool IsWindowOpen(LocalPlayerUI _playerUi)
	{
		return _playerUi.windowManager.IsWindowOpen(XUiC_FocusedBlockHealth.ID);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string text = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float fill;
}
