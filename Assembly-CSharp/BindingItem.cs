using System;

public abstract class BindingItem
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public BindingItem(string _sourceText)
	{
		this.SourceText = _sourceText;
		this.fieldName = _sourceText.Substring(1, _sourceText.Length - 2);
	}

	public abstract string GetValue();

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fieldName;

	public readonly string SourceText;
}
