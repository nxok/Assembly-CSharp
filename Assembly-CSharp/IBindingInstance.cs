using System;

public interface IBindingInstance
{
	void RefreshValue();

	public const char CvarFormatSplitChar = ':';
}
