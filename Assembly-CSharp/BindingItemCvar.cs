using System;

public class BindingItemCvar : BindingItem
{
	public BindingItemCvar(BindingInfo _parent, string _sourceText)
		: base(_sourceText)
	{
		this.fieldName = this.fieldName.Replace("cvar(", "").Replace(")", "");
		if (this.fieldName.IndexOf(':') >= 0)
		{
			string[] array = this.fieldName.Split(':', StringSplitOptions.None);
			this.fieldName = array[0];
			this.format = array[1];
		}
		for (XUiController xuiController = _parent.View.Controller; xuiController != null; xuiController = xuiController.Parent)
		{
			if (xuiController.GetType() != typeof(XUiController))
			{
				xuiController.Bindings.AddBinding(_parent);
				return;
			}
		}
	}

	public override string GetValue()
	{
		return XUiM_Player.GetPlayer().GetCVar(this.fieldName).ToString(this.format);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string format;
}
