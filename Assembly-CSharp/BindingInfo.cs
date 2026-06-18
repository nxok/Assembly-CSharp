using System;
using System.Collections.Generic;
using Unity.Profiling;

public class BindingInfo : IBindingInstance
{
	public BindingInfo(XUiView _view, string _attribute, string _sourceText)
	{
		this.View = _view;
		this.attributeName = _attribute;
		this.sourceText = _sourceText;
		this.pmRefreshValueCompleteSourceText = new ProfilerMarker(this.sourceText);
		int num2;
		for (int num = this.sourceText.IndexOf("{", StringComparison.Ordinal); num != -1; num = this.sourceText.IndexOf("{", num2, StringComparison.Ordinal))
		{
			num2 = this.sourceText.IndexOf("}", num, StringComparison.Ordinal);
			if (num2 == -1)
			{
				return;
			}
			string text = this.sourceText.Substring(num, num2 - num + 1);
			bool flag = false;
			for (int i = 0; i < this.bindingList.Count; i++)
			{
				if (this.bindingList[i].SourceText == text)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				BindingItem bindingItem;
				if (text.StartsWith("{cvar("))
				{
					bindingItem = new BindingItemCvar(this, text);
				}
				else if (text.StartsWith("{#"))
				{
					bindingItem = new BindingItemNcalc(this, text);
				}
				else
				{
					if (text.StartsWith("{%"))
					{
						throw new Exception(string.Concat(new string[]
						{
							"[XUi] Using new NCalc binding expression for partial attribute evaluation. Attribute: '",
							this.attributeName,
							"', full attribute value: '",
							this.sourceText,
							"', binding string: '",
							text,
							"', view hierarchy: ",
							this.View.GetXuiHierarchy(),
							":"
						}));
					}
					bindingItem = new BindingItemStandard(this, text);
				}
				this.bindingList.Add(bindingItem);
			}
		}
	}

	public void RefreshValue()
	{
		using (this.pmRefreshValueComplete.Auto())
		{
			using (this.pmRefreshValueCompleteSourceText.Auto())
			{
				bool flag = this.cachedResultValue == null;
				using (this.pmGetBindingValues.Auto())
				{
					for (int i = 0; i < this.bindingList.Count; i++)
					{
						string text = this.bindingList[i].GetValue() ?? "";
						if (i < this.cachedBindingValues.Count)
						{
							flag |= !string.Equals(this.cachedBindingValues[i], text, StringComparison.Ordinal);
							this.cachedBindingValues[i] = text;
						}
						else
						{
							flag = true;
							this.cachedBindingValues.Add(text);
						}
					}
				}
				using (this.pmBuildAttributeValue.Auto())
				{
					if (flag)
					{
						string text2 = this.sourceText;
						if (this.bindingList.Count == 1 && text2.Equals(this.bindingList[0].SourceText, StringComparison.Ordinal))
						{
							text2 = this.cachedBindingValues[0] ?? "";
						}
						else
						{
							for (int j = 0; j < this.bindingList.Count; j++)
							{
								BindingItem bindingItem = this.bindingList[j];
								text2 = text2.Replace(bindingItem.SourceText, this.cachedBindingValues[j]);
							}
						}
						this.cachedResultValue = text2;
					}
				}
				using (this.pmParseAttribute.Auto())
				{
					string text3 = this.cachedResultValue;
					if (text3.Contains("{cvar("))
					{
						text3 = BindingsManager.ReplaceCVars(text3);
					}
					try
					{
						if (!this.parsersDetected)
						{
							ParsingMethodCache.ParsingMethodData parsingMethodData;
							if (ParsingMethodCache.Instance.TryGetParsingDelegate(this.View, this.attributeName, out parsingMethodData) && parsingMethodData.TryGetDelegateForSourceType(typeof(string), out this.parsingDelegateView))
							{
								this.parsingDelegateView(this.View, text3);
							}
							if (this.parsingDelegateView == null && this.View.Controller != null && ParsingMethodCache.Instance.TryGetParsingDelegate(this.View.Controller, this.attributeName, out parsingMethodData) && parsingMethodData.TryGetDelegateForSourceType(typeof(string), out this.parsingDelegateController))
							{
								this.parsingDelegateController(this.View.Controller, text3);
							}
							if (this.parsingDelegateView == null && this.parsingDelegateController == null)
							{
								this.View.ParseAttributeViewAndController(this.attributeName, text3);
							}
							this.parsersDetected = true;
						}
						else if (this.parsingDelegateView != null)
						{
							this.parsingDelegateView(this.View, text3);
						}
						else if (this.parsingDelegateController != null)
						{
							this.parsingDelegateController(this.View.Controller, text3);
						}
						else
						{
							this.View.ParseAttributeViewAndController(this.attributeName, text3);
						}
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new string[]
						{
							"[XUi] Exception parsing result of binding. Attribute: '",
							this.attributeName,
							"', binding string: '",
							this.sourceText,
							"', binding result: '",
							text3,
							"', view hierarchy: ",
							this.View.GetXuiHierarchy(),
							":"
						}));
						Log.Exception(ex);
					}
				}
			}
		}
	}

	public readonly XUiView View;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string attributeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string sourceText;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool parsersDetected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XuiParsingDelegate parsingDelegateView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XuiParsingDelegate parsingDelegateController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BindingItem> bindingList = new List<BindingItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> cachedBindingValues = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResultValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmCtor = new ProfilerMarker("BindingInfo.ctor");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueComplete = new ProfilerMarker("BindingInfo.RefreshValue");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueCompleteSourceText;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmGetBindingValues = new ProfilerMarker("GetBindingValues");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmBuildAttributeValue = new ProfilerMarker("BuildAttributeValue");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmParseAttribute = new ProfilerMarker("ParseAttribute");
}
