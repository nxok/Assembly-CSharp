using System;

public class BindingItemStandard : BindingItem
{
	public BindingItemStandard(BindingInfo _parent, string _sourceText)
		: base(_sourceText)
	{
		for (XUiController xuiController = _parent.View.Controller; xuiController != null; xuiController = xuiController.Parent)
		{
			if (xuiController.GetType() != typeof(XUiController))
			{
				this.dataContext = xuiController;
				string text = "";
				if (this.dataContext.GetBindingValue(ref text, this.fieldName))
				{
					this.dataContext.Bindings.AddBinding(_parent);
					return;
				}
				if (BindingMethodCache.Instance.TryGetBindingDelegate(xuiController, this.fieldName, out this.bindingDelegate))
				{
					this.dataContext.Bindings.AddBinding(_parent);
					return;
				}
			}
		}
		if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
		{
			Log.Warning("[XUi] Binding name '" + this.fieldName + "' not found! Hierarchy: " + _parent.View.GetXuiHierarchy());
		}
	}

	public override string GetValue()
	{
		if (this.bindingDelegate != null)
		{
			object obj = this.bindingDelegate(this.dataContext);
			if (obj == null)
			{
				Log.Warning("[XUi] Binding '" + this.SourceText + "' returned null, should always return appropriate non-null value of same data type. Hierarchy: " + this.dataContext.GetXuiHierarchy());
				obj = "";
			}
			return obj.ToString();
		}
		string text = "";
		this.dataContext.GetBindingValue(ref text, this.fieldName);
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController dataContext;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XuiBindingDelegate bindingDelegate;
}
