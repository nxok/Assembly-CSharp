using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogResponseEntry : XUiController
{
	public DialogResponse CurrentResponse
	{
		get
		{
			return this.currentResponse;
		}
		set
		{
			this.currentResponse = value;
			this.HasRequirement = true;
			base.ViewComponent.Enabled = value != null;
			if (this.currentResponse != null && this.currentResponse.RequirementList.Count > 0)
			{
				int i = 0;
				while (i < this.currentResponse.RequirementList.Count)
				{
					if (!this.currentResponse.RequirementList[i].CheckRequirement(this.xui.playerUI.entityPlayer, this.xui.Dialog.Respondent))
					{
						this.HasRequirement = false;
						if (this.currentResponse.RequirementList[i].RequirementVisibilityType == BaseDialogRequirement.RequirementVisibilityTypes.Hide)
						{
							this.currentResponse = null;
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
			}
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.currentResponse != null;
		if (bindingName == "response")
		{
			value = "";
			if (flag)
			{
				value = (this.HasRequirement ? this.currentResponse.Text : this.currentResponse.GetRequiredDescription(this.xui.playerUI.entityPlayer));
			}
			return true;
		}
		if (bindingName == "textstatecolor")
		{
			value = "255,255,255,255";
			if (flag)
			{
				value = (this.HasRequirement ? XUiC_DialogResponseEntry.enabledColor : XUiC_DialogResponseEntry.disabledColor);
			}
			return true;
		}
		if (bindingName == "rowstatecolor")
		{
			value = "255,255,255,255";
			if (flag)
			{
				if (this.HasRequirement)
				{
					value = (this.Selected ? "255,255,255,255" : (this.IsHovered ? this.hoverColor : XUiC_DialogResponseEntry.enabledColor));
				}
				else
				{
					value = XUiC_DialogResponseEntry.disabledColor;
				}
			}
			return true;
		}
		if (bindingName == "rowstatesprite")
		{
			value = (this.Selected ? "ui_game_select_row" : "menu_empty");
			return true;
		}
		if (!(bindingName == "showresponse"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = flag.ToString();
		return true;
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
		if (this.currentResponse == null)
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
			XUiC_DialogResponseEntry.enabledColor = value;
			return true;
		}
		if (name == "disabled_color")
		{
			XUiC_DialogResponseEntry.disabledColor = value;
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
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	public new bool Selected;

	public bool IsHovered;

	public bool HasRequirement = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string enabledColor = "255,255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string disabledColor = "200,200,200,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public DialogResponse currentResponse;
}
