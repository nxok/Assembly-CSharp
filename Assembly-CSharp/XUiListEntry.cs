using System;

public abstract class XUiListEntry<T> : IComparable<T> where T : XUiListEntry<T>
{
	public abstract int CompareTo(T _otherEntry);

	public virtual bool MatchesSearch(string _searchString)
	{
		return true;
	}

	public virtual bool UiDirty { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiListEntry()
	{
	}
}
