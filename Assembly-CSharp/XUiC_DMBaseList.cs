using System;

public abstract class XUiC_DMBaseList<T> : XUiC_List<T> where T : XUiListEntry<T>
{
	public event XUiC_DMBaseList<T>.ListEntryHoveredDelegate OnEntryHovered;

	public override void Init()
	{
		base.Init();
		XUiC_List<T>.XUiC_ListEntry[] listEntryControllers = this.listEntryControllers;
		for (int i = 0; i < listEntryControllers.Length; i++)
		{
			listEntryControllers[i].OnHover += this.childElementHovered;
		}
		foreach (XUiController xuiController in this.pager.Children)
		{
			xuiController.OnHover += this.childElementHovered;
		}
		base.PageContentsChanged += this.pageContentsChangedHandler;
		this.searchBox.OnHover += this.childElementHovered;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void pageContentsChangedHandler(XUiC_List<T> _list)
	{
		if (this.hoveredElement == null || this.OnEntryHovered == null)
		{
			return;
		}
		T t = default(T);
		XUiC_List<T>.XUiC_ListEntry xuiC_ListEntry = this.hoveredElement as XUiC_List<T>.XUiC_ListEntry;
		if (xuiC_ListEntry != null)
		{
			t = xuiC_ListEntry.GetEntry();
		}
		this.OnEntryHovered(t, false);
		this.OnEntryHovered(t, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void childElementHovered(XUiController _sender, bool _isOver)
	{
		this.hoveredElement = (_isOver ? _sender : null);
		XUiC_List<T>.XUiC_ListEntry xuiC_ListEntry = _sender as XUiC_List<T>.XUiC_ListEntry;
		XUiC_DMBaseList<T>.ListEntryHoveredDelegate onEntryHovered = this.OnEntryHovered;
		if (onEntryHovered == null)
		{
			return;
		}
		onEntryHovered((xuiC_ListEntry != null) ? xuiC_ListEntry.GetEntry() : default(T), _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_DMBaseList()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController hoveredElement;

	public delegate void ListEntryHoveredDelegate(T _entry, bool _isOver);
}
