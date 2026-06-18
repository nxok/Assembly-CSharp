using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogRespondentName : XUiController
{
	public Dialog CurrentDialog
	{
		get
		{
			return this.currentDialog;
		}
		set
		{
			this.currentDialog = value;
			base.RefreshBindings();
			this.IsDirty = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.currentDialog = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "respondentname")
		{
			value = ((this.xui.Dialog.Respondent != null) ? Localization.Get(this.xui.Dialog.Respondent.EntityName, false, null) : "");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public void Refresh()
	{
		base.RefreshBindings();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog currentDialog;
}
