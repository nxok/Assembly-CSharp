using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchLeaderboardEntry : XUiController
{
	public TwitchLeaderboardEntry LeaderboardEntry
	{
		get
		{
			return this.leaderboardEntry;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			this.leaderboardEntry = value;
			this.IsDirty = true;
		}
	}

	public XUiC_TwitchInfoWindowGroup TwitchInfoUIHandler { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.leaderboardEntry != null;
		if (bindingName == "username")
		{
			value = (flag ? string.Format("[{0}]{1}[-]", this.leaderboardEntry.UserColor, this.leaderboardEntry.UserName) : "");
			return true;
		}
		if (!(bindingName == "kills"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = (flag ? this.leaderboardEntry.Kills.ToString() : "");
		return true;
	}

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.RefreshBindings();
		this.IsDirty = false;
		base.Update(_dt);
	}

	public override void OnCursorSelected(bool _isActualElement)
	{
		base.OnCursorSelected(_isActualElement);
		base.GetParentByType<XUiC_TwitchLeaderboardEntryList>().SelectedEntry = this;
	}

	public void Refresh()
	{
		this.IsDirty = true;
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "enabled_color")
		{
			this.enabledColor = value;
			return true;
		}
		if (name == "disabled_color")
		{
			this.disabledColor = value;
			return true;
		}
		if (name == "positive_color")
		{
			this.positiveColor = value;
			return true;
		}
		if (name == "negative_color")
		{
			this.negativeColor = value;
			return true;
		}
		if (name == "row_color")
		{
			this.rowColor = value;
			return true;
		}
		if (!(name == "hover_color"))
		{
			return base.ParseAttribute(name, value);
		}
		this.hoverColor = value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string positiveColor = "0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string negativeColor = "255,0,0";

	public new bool Selected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchLeaderboardEntry leaderboardEntry;

	public XUiC_TwitchLeaderboardEntryList Owner;
}
