using System;
using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchHowToWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		base.GetChildById("leftButton").OnPress += this.Left_OnPress;
		base.GetChildById("rightButton").OnPress += this.Right_OnPress;
		this.lblTipHeader = Localization.Get("TwitchInfo_TipHeader", false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Left_OnPress(XUiController _sender, int _mouseButton)
	{
		this.tipIndex--;
		if (this.tipIndex == -1)
		{
			this.tipIndex = this.TipNames.Count - 1;
		}
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Right_OnPress(XUiController _sender, int _mouseButton)
	{
		this.tipIndex++;
		if (this.tipIndex == this.TipNames.Count)
		{
			this.tipIndex = 0;
		}
		base.RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.TipNames = TwitchManager.Current.tipTitleList;
		this.TipText = TwitchManager.Current.tipDescriptionList;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "tipheader")
		{
			value = this.lblTipHeader;
			return true;
		}
		if (bindingName == "tiptitle")
		{
			value = ((this.TipNames.Count > 0) ? this.TipNames[this.tipIndex] : "");
			return true;
		}
		if (!(bindingName == "tiptext"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = ((this.TipText.Count > 0) ? this.TipText[this.tipIndex] : "");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int tipIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> TipNames = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> TipText = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTipHeader;
}
