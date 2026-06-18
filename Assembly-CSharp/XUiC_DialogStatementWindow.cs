using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogStatementWindow : XUiController
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

	public override void Init()
	{
		base.Init();
		XUiController xuiController = base.GetChildById("statementLabel");
		if (xuiController != null)
		{
			this.label = (XUiV_Label)xuiController.ViewComponent;
		}
		xuiController = base.GetChildById("backgroundSprite");
		if (xuiController != null)
		{
			this.backgroundSprite = (XUiV_Sprite)xuiController.ViewComponent;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.currentDialog = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "statement")
		{
			value = ((this.currentDialog != null && this.currentDialog.CurrentStatement != null) ? this.currentDialog.CurrentStatement.Text : "");
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
	public XUiV_Sprite backgroundSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog currentDialog;
}
