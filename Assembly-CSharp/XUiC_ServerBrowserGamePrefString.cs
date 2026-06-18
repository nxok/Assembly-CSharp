using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefString : XUiController, IServerBrowserFilterControl
{
	public GameInfoString GameInfoString
	{
		get
		{
			return this.gameInfoString;
		}
	}

	public override void Init()
	{
		base.Init();
		this.gameInfoString = EnumUtils.Parse<GameInfoString>(this.viewComponent.ID, false);
		this.value = base.GetChildById("value").GetChildByType<XUiC_TextInput>();
		this.value.OnChangeHandler += this.ControlText_OnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getGameInfoName()
	{
		return this.gameInfoString.ToStringCached<GameInfoString>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	public void Reset()
	{
		this.value.Text = "";
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	public void SetValue(string _value)
	{
		this.value.Text = _value;
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	public string GetValue()
	{
		return this.value.Text;
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string text = this.gameInfoString.ToStringCached<GameInfoString>();
		string input = this.value.Text.Trim();
		if (input.Length == 0)
		{
			return new XUiC_ServersList.UiServerFilter(text, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
		}
		Func<XUiC_ServersList.ListEntry, bool> func = (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(this.gameInfoString).ContainsCaseInsensitive(input);
		return new XUiC_ServersList.UiServerFilter(text, XUiC_ServersList.EnumServerLists.Regular, func, IServerListInterface.ServerFilter.EServerFilterType.StringContains, 0, 0, false, input);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput value;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoString gameInfoString;

	public Action<IServerBrowserFilterControl> OnValueChanged;
}
