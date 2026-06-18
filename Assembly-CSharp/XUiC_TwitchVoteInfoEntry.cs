using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchVoteInfoEntry : XUiController
{
	public TwitchVote Vote
	{
		get
		{
			return this.vote;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			this.vote = value;
			this.IsDirty = true;
		}
	}

	public XUiC_TwitchInfoWindowGroup TwitchInfoUIHandler { get; set; }

	public bool Tracked { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.vote != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1656712805U)
		{
			if (num != 765459171U)
			{
				if (num != 1129104269U)
				{
					if (num == 1656712805U)
					{
						if (bindingName == "rowstatesprite")
						{
							value = (this.Selected ? "ui_game_select_row" : "menu_empty");
							return true;
						}
					}
				}
				else if (bindingName == "showicon")
				{
					value = ((this.Owner != null) ? (this.Owner.TwitchEntryListWindow.VoteCategory == "").ToString() : "true");
					return true;
				}
			}
			else if (bindingName == "rowstatecolor")
			{
				value = (this.Selected ? "255,255,255,255" : (this.IsHovered ? this.hoverColor : this.rowColor));
				return true;
			}
		}
		else if (num <= 2801367993U)
		{
			if (num != 2104701544U)
			{
				if (num == 2801367993U)
				{
					if (bindingName == "votetitle")
					{
						value = (flag ? this.vote.VoteDescription : "");
						return true;
					}
				}
			}
			else if (bindingName == "votecolor")
			{
				if (flag)
				{
					if (this.vote.Enabled)
					{
						value = ((this.vote.TitleColor == "") ? this.enabledColor : this.vote.TitleColor);
					}
					else
					{
						value = this.disabledColor;
					}
				}
				return true;
			}
		}
		else if (num != 3106195591U)
		{
			if (num == 4217867520U)
			{
				if (bindingName == "voteicon")
				{
					value = "";
					if (flag && this.vote.MainVoteType != null)
					{
						value = this.vote.MainVoteType.Icon;
					}
					return true;
				}
			}
		}
		else if (bindingName == "iconcolor")
		{
			value = "255,255,255,255";
			if (flag)
			{
				value = (this.vote.Enabled ? this.enabledColor : this.disabledColor);
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (this.Vote == null)
		{
			this.IsHovered = false;
			return;
		}
		if (this.IsHovered != _isOver)
		{
			this.IsHovered = _isOver;
			base.RefreshBindings();
		}
	}

	public override void OnCursorSelected(bool _isActualElement)
	{
		base.OnCursorSelected(_isActualElement);
		base.GetParentByType<XUiC_TwitchVoteInfoEntryList>().SelectedEntry = this;
		this.TwitchInfoUIHandler.SetEntry(this);
	}

	public override void Update(float _dt)
	{
		base.RefreshBindings();
		this.IsDirty = false;
		base.Update(_dt);
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
	public TwitchVote vote;

	public XUiC_TwitchVoteInfoEntryList Owner;
}
