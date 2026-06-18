using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_List<T> : XUiController where T : XUiListEntry<T>
{
	public event XUiEvent_ListSelectionChangedEventHandler<T> SelectionChanged;

	public event XUiEvent_ListPageNumberChangedEventHandler<T> PageNumberChanged;

	public event XUiEvent_ListEntryClickedEventHandler<T> ListEntryClicked;

	public event XUiEvent_ListEntryDoubleClickedEventHandler<T> ListEntryDoubleClicked;

	public event XUiEvent_ListEntryHoveredEventHandler<T> ListEntryHovered;

	public event XUiEvent_PageContentsChangedEventHandler<T> PageContentsChanged;

	public int PageLength
	{
		get
		{
			if (this.PageSizeOverride > 0)
			{
				return this.PageSizeOverride;
			}
			XUiC_List<T>.XUiC_ListEntry[] array = this.listEntryControllers;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			int num = Mathf.Clamp(value, 0, this.LastPage);
			if (num == this.page)
			{
				return;
			}
			this.page = num;
			this.updatePageLabel();
			this.IsDirty = true;
			this.ClearSelection();
			this.CurrentBaseIndex = this.page * this.PageLength;
			XUiEvent_ListPageNumberChangedEventHandler<T> pageNumberChanged = this.PageNumberChanged;
			if (pageNumberChanged == null)
			{
				return;
			}
			pageNumberChanged(this, this.page);
		}
	}

	public int LastPage
	{
		get
		{
			return Math.Max(0, Mathf.CeilToInt((float)this.filteredEntries.Count / (float)this.PageLength) - 1);
		}
	}

	public XUiC_List<T>.XUiC_ListEntry SelectedEntry
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.selectedEntry;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value == this.selectedEntry)
			{
				return;
			}
			T t = this.currentSelectedEntry;
			XUiC_List<T>.XUiC_ListEntry xuiC_ListEntry = this.selectedEntry;
			this.selectedEntry = null;
			if (xuiC_ListEntry != null)
			{
				xuiC_ListEntry.Selected = false;
			}
			this.selectedEntry = value;
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = true;
				this.currentSelectedEntry = this.selectedEntry.GetEntry();
				for (int i = 0; i < this.listEntryControllers.Length; i++)
				{
					if (this.selectedEntry == this.listEntryControllers[i])
					{
						this.selectedEntryIndex = this.CurrentBaseIndex + i;
						break;
					}
				}
			}
			else
			{
				this.currentSelectedEntry = default(T);
				this.selectedEntryIndex = -1;
			}
			if (t != this.currentSelectedEntry)
			{
				this.OnSelectionChanged(xuiC_ListEntry, this.selectedEntry);
			}
			this.IsDirty = true;
		}
	}

	public T SelectedEntryData
	{
		get
		{
			XUiC_List<T>.XUiC_ListEntry xuiC_ListEntry = this.SelectedEntry;
			if (xuiC_ListEntry == null)
			{
				return default(T);
			}
			return xuiC_ListEntry.GetEntry();
		}
	}

	public int SelectedEntryIndex
	{
		get
		{
			return this.selectedEntryIndex;
		}
		set
		{
			if (value < 0 || value >= this.EntryCount)
			{
				return;
			}
			if (this.selectedEntryIndex == value)
			{
				return;
			}
			this.forceEntryVisible(value);
			this.selectedEntryIndex = value;
			this.updateSelectedItemByIndex = true;
			this.updateCurrentPageContents();
			this.updateSelectedItemByIndex = false;
			this.SelectedEntry = this.listEntryControllers[value - this.CurrentBaseIndex];
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("page_size_override", false)]
	public int PageSizeOverride
	{
		get
		{
			return this.pageSizeOverride;
		}
		set
		{
			if (value == this.pageSizeOverride)
			{
				return;
			}
			this.pageSizeOverride = value;
			if (this.PagingStepSize == XUiC_List<T>.EPagingStepSize.Page)
			{
				this.Page = 0;
			}
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("paging_step_size", false)]
	public XUiC_List<T>.EPagingStepSize PagingStepSize
	{
		get
		{
			return this.pagingStepSize;
		}
		set
		{
			if (value == this.pagingStepSize)
			{
				return;
			}
			this.pagingStepSize = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("paging_overscroll", false)]
	public Vector2i PagingOverscroll
	{
		get
		{
			return this.pagingOverscroll;
		}
		set
		{
			if (value == this.pagingOverscroll)
			{
				return;
			}
			this.pagingOverscroll = value;
			this.IsDirty = true;
		}
	}

	public int CurrentBaseIndex
	{
		get
		{
			return this.minIndex;
		}
		set
		{
			if (this.minIndex == value)
			{
				return;
			}
			if (value < 0)
			{
				value = Mathf.Max(value, this.PagingOverscroll.x);
			}
			if (value > this.EntryCount - this.PageLength)
			{
				XUiC_List<T>.EPagingStepSize epagingStepSize = this.PagingStepSize;
				if (epagingStepSize != XUiC_List<T>.EPagingStepSize.Page)
				{
					if (epagingStepSize != XUiC_List<T>.EPagingStepSize.SingleEntry)
					{
						throw new ArgumentOutOfRangeException();
					}
					value = Mathf.Min(value, this.EntryCount - this.PageLength + this.PagingOverscroll.y);
				}
				else
				{
					value = Mathf.Min(value, this.EntryCount - 1);
				}
			}
			this.minIndex = value;
			this.ClearSelection();
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("current_min_index")]
	public int CurrentPageMinIndex
	{
		get
		{
			return Mathf.Max(this.minIndex, 0);
		}
	}

	[XuiXmlBinding("current_max_index")]
	public int CurrentPageMaxIndex
	{
		get
		{
			return Mathf.Min(this.minIndex + this.PageLength, this.EntryCount) - 1;
		}
	}

	[XuiXmlBinding("hasselection")]
	public bool HasSelection
	{
		get
		{
			return this.SelectedEntryData != null;
		}
	}

	[XuiXmlBinding("entrycountfiltered")]
	public int EntryCount
	{
		get
		{
			return this.filteredEntries.Count;
		}
	}

	public T GetEntry(int _index)
	{
		return this.filteredEntries[_index];
	}

	[XuiXmlBinding("entrycounttotal")]
	public int UnfilteredEntryCount
	{
		get
		{
			return this.allEntries.Count;
		}
	}

	[XuiXmlAttribute("clear_selection_on_open", false)]
	public bool ClearSelectionOnOpenClose { get; set; } = true;

	[XuiXmlAttribute("clear_searchtext_on_open", false)]
	public bool ClearSearchTextOnOpenClose { get; set; }

	[XuiXmlAttribute("selectable", false)]
	public bool SelectableEntries { get; set; } = true;

	[XuiXmlAttribute("cursor_controllable", false)]
	public bool CursorControllable { get; set; }

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("list");
		if (childById == null)
		{
			Log.Warning("[XUi] List controller without a 'list' child element! (window group '" + base.WindowGroup.Id + "')");
		}
		else if (this.CursorControllable)
		{
			XUiV_Grid xuiV_Grid = childById.ViewComponent as XUiV_Grid;
			if (xuiV_Grid != null)
			{
				if (xuiV_Grid.Arrangement == null)
				{
					this.columns = xuiV_Grid.Columns;
					this.rows = this.listEntryControllers.Length / this.columns;
				}
				else
				{
					this.rows = xuiV_Grid.Rows;
					this.columns = this.listEntryControllers.Length / this.rows;
				}
			}
			XUiV_Table xuiV_Table = childById.ViewComponent as XUiV_Table;
			if (xuiV_Table != null)
			{
				this.columns = xuiV_Table.Columns;
				this.rows = this.listEntryControllers.Length / this.columns;
			}
		}
		this.RebuildList(true);
	}

	public virtual void RebuildList(bool _resetFilter = false)
	{
		this.ClearSelection();
		List<T> list = this.filteredEntries;
		if (list != null)
		{
			list.Clear();
		}
		this.RefreshView(_resetFilter, true);
	}

	public virtual void RefreshView(bool _resetFilter = false, bool _resetPage = true)
	{
		if (_resetFilter)
		{
			if (this.searchBox != null)
			{
				this.searchBox.Text = "";
			}
			if (this.searchBox2 != null)
			{
				this.searchBox2.Text = "";
			}
		}
		string text;
		if (this.searchBox == null)
		{
			XUiC_CaptionedTextBox xuiC_CaptionedTextBox = this.searchBox2;
			text = ((xuiC_CaptionedTextBox != null) ? xuiC_CaptionedTextBox.Text : null);
		}
		else
		{
			text = this.searchBox.Text;
		}
		this.OnSearchInputChanged(this, text, true);
		if (!_resetPage)
		{
			return;
		}
		XUiC_List<T>.EPagingStepSize epagingStepSize = this.PagingStepSize;
		if (epagingStepSize == XUiC_List<T>.EPagingStepSize.Page)
		{
			this.Page = 0;
			return;
		}
		if (epagingStepSize != XUiC_List<T>.EPagingStepSize.SingleEntry)
		{
			throw new ArgumentOutOfRangeException();
		}
		this.CurrentBaseIndex = this.PagingOverscroll.x;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_List<T>.XUiC_ListEntry IsVisible(T _value)
	{
		foreach (XUiC_List<T>.XUiC_ListEntry xuiC_ListEntry in this.listEntryControllers)
		{
			T entry = xuiC_ListEntry.GetEntry();
			if (entry != null && entry == _value)
			{
				return xuiC_ListEntry;
			}
		}
		return null;
	}

	[XuiBindEvent("OnPageChanged", "pager")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnPageChanged()
	{
		this.Page = this.pager.CurrentPageNumber;
	}

	[XuiBindEvent("OnScroll", null)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		this.ChangePage((_delta > 0f) ? (-1) : 1);
	}

	public void ChangePage(int _offset)
	{
		XUiC_List<T>.EPagingStepSize epagingStepSize = this.PagingStepSize;
		if (epagingStepSize == XUiC_List<T>.EPagingStepSize.Page)
		{
			this.Page += _offset;
			return;
		}
		if (epagingStepSize != XUiC_List<T>.EPagingStepSize.SingleEntry)
		{
			throw new ArgumentOutOfRangeException();
		}
		this.CurrentBaseIndex += _offset;
	}

	[XuiBindEvent("OnSubmitHandler", "searchBox")]
	[XuiBindEvent("OnSubmitHandler", "searchBox2")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSearchInputSubmit(XUiController _sender, string _text)
	{
		this.OnSearchInputChanged(_sender, _text, false);
	}

	[XuiBindEvent("OnChangeHandler", "searchBox")]
	[XuiBindEvent("OnChangeHandler", "searchBox2")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.FilterResults(_text);
		this.IsDirty = true;
	}

	public IReadOnlyList<T> AllEntries()
	{
		return this.allEntries;
	}

	public T GetEntryByIndex(int _index)
	{
		if (_index < 0 || _index >= this.filteredEntries.Count)
		{
			return default(T);
		}
		return this.filteredEntries[_index];
	}

	public void GetCurrentPageEntries(List<T> _entries)
	{
		if (_entries == null)
		{
			throw new ArgumentNullException("_entries");
		}
		_entries.Clear();
		for (int i = 0; i < this.PageLength; i++)
		{
			int num = i + this.CurrentBaseIndex;
			if (num < this.filteredEntries.Count)
			{
				_entries.Add(this.filteredEntries[num]);
			}
		}
	}

	public void SetFilter(string _filter)
	{
		if (this.searchBox != null)
		{
			this.searchBox.Text = _filter;
			this.IsDirty = true;
			return;
		}
		if (this.searchBox2 != null)
		{
			this.searchBox2.Text = _filter;
			this.IsDirty = true;
			return;
		}
		Log.Error("Can not set filter without search box");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FilterResults(string _textMatch)
	{
		if (_textMatch == null)
		{
			this.filteredEntries.Clear();
			this.filteredEntries.AddRange(this.allEntries);
			return;
		}
		if (_textMatch == this.previousMatch && this.filteredEntries.Count == this.allEntries.Count)
		{
			return;
		}
		this.previousMatch = _textMatch;
		this.filteredEntries.Clear();
		if (_textMatch.Length > 0)
		{
			using (List<T>.Enumerator enumerator = this.allEntries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					T t = enumerator.Current;
					if (t.MatchesSearch(_textMatch))
					{
						this.filteredEntries.Add(t);
					}
				}
				return;
			}
		}
		this.filteredEntries.AddRange(this.allEntries);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSelectionChanged(XUiC_List<T>.XUiC_ListEntry _previousEntry, XUiC_List<T>.XUiC_ListEntry _newEntry)
	{
		if (this.ignoreSelectionChange)
		{
			return;
		}
		XUiEvent_ListSelectionChangedEventHandler<T> selectionChanged = this.SelectionChanged;
		if (selectionChanged != null)
		{
			selectionChanged(this, (_previousEntry != null) ? _previousEntry.GetEntry() : default(T), (_newEntry != null) ? _newEntry.GetEntry() : default(T));
		}
		if (this.SelectableEntries)
		{
			return;
		}
		this.ignoreSelectionChange = true;
		this.ClearSelection();
		this.ignoreSelectionChange = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void goToPageOfCurrentlySelectedEntry()
	{
		if (this.currentSelectedEntry == null)
		{
			return;
		}
		T t = this.currentSelectedEntry;
		bool flag = false;
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i] == this.currentSelectedEntry)
			{
				this.forceEntryVisible(i);
				flag = true;
				break;
			}
		}
		this.currentSelectedEntry = (flag ? t : default(T));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void forceEntryVisible(int _index)
	{
		if (_index < 0 || _index >= this.filteredEntries.Count)
		{
			return;
		}
		if (_index >= this.CurrentBaseIndex && _index <= this.CurrentPageMaxIndex)
		{
			return;
		}
		XUiC_List<T>.EPagingStepSize epagingStepSize = this.PagingStepSize;
		if (epagingStepSize == XUiC_List<T>.EPagingStepSize.Page)
		{
			this.Page = _index / this.PageLength;
			return;
		}
		if (epagingStepSize != XUiC_List<T>.EPagingStepSize.SingleEntry)
		{
			throw new ArgumentOutOfRangeException();
		}
		this.CurrentBaseIndex = _index;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentPageContents()
	{
		if (this.filteredEntries == null)
		{
			Log.Error("filteredEntries is null!");
			return;
		}
		for (int i = 0; i < this.listEntryControllers.Length; i++)
		{
			int num = i + this.CurrentBaseIndex;
			XUiC_List<T>.XUiC_ListEntry xuiC_ListEntry = this.listEntryControllers[i];
			if (xuiC_ListEntry == null)
			{
				Log.Error("listEntry is null! {0} items in listEntryControllers", new object[] { this.listEntryControllers.Length });
				return;
			}
			if (num >= 0 && num < this.filteredEntries.Count && i < this.PageLength)
			{
				xuiC_ListEntry.SetEntry(this.filteredEntries[num]);
			}
			else
			{
				xuiC_ListEntry.SetEntry(default(T));
				if (xuiC_ListEntry.Selected)
				{
					xuiC_ListEntry.Selected = false;
				}
			}
			if (!this.updateSelectedItemByIndex && this.currentSelectedEntry != null && this.currentSelectedEntry == xuiC_ListEntry.GetEntry())
			{
				this.SelectedEntry = xuiC_ListEntry;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePageLabel()
	{
		if (this.pager == null)
		{
			return;
		}
		this.pager.LastPageNumber = this.LastPage;
		this.pager.CurrentPageNumber = this.page;
	}

	public override void Update(float _dt)
	{
		if (this.SelectableEntries && this.CursorControllable && this.columns > 0 && this.rows > 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !this.xui.playerUI.windowManager.IsInputActive())
		{
			PlayerActionsGUI guiactions = this.windowGroup.playerUI.playerInput.GUIActions;
			if (guiactions.Left.WasPressed)
			{
				this.SelectedEntryIndex = Math.Max(0, this.SelectedEntryIndex - this.rows);
			}
			if (guiactions.Right.WasPressed)
			{
				this.SelectedEntryIndex = Math.Min(this.EntryCount - 1, this.SelectedEntryIndex + this.rows);
			}
			if (guiactions.Up.WasPressed)
			{
				this.SelectedEntryIndex = Math.Max(0, this.SelectedEntryIndex - 1);
			}
			if (guiactions.Down.WasPressed)
			{
				this.SelectedEntryIndex = Math.Min(this.EntryCount - 1, this.SelectedEntryIndex + 1);
			}
		}
		if (this.IsDirty)
		{
			this.goToPageOfCurrentlySelectedEntry();
			if (this.page > this.LastPage)
			{
				this.Page = this.LastPage;
			}
			this.updateCurrentPageContents();
			if (this.SelectedEntryData != this.currentSelectedEntry)
			{
				this.ClearSelection();
			}
			this.updatePageLabel();
			XUiEvent_PageContentsChangedEventHandler<T> pageContentsChanged = this.PageContentsChanged;
			if (pageContentsChanged != null)
			{
				pageContentsChanged(this);
			}
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.ClearSelectionOnOpenClose)
		{
			this.ClearSelection();
		}
		if (this.ClearSearchTextOnOpenClose)
		{
			if (this.searchBox != null)
			{
				this.searchBox.Text = string.Empty;
				this.OnSearchInputChanged(this, string.Empty, true);
			}
			if (this.searchBox2 != null)
			{
				this.searchBox2.Text = string.Empty;
				this.OnSearchInputChanged(this, string.Empty, true);
			}
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.ClearSelectionOnOpenClose)
		{
			this.ClearSelection();
		}
		if (this.ClearSearchTextOnOpenClose)
		{
			if (this.searchBox != null)
			{
				this.searchBox.Text = "";
			}
			if (this.searchBox2 != null)
			{
				this.searchBox2.Text = "";
			}
		}
	}

	public void ClearSelection()
	{
		this.SelectedEntry = null;
	}

	public void SelectCursorElementForSelectedEntry()
	{
		XUiC_List<T>.XUiC_ListEntry xuiC_ListEntry = this.SelectedEntry;
		if (xuiC_ListEntry == null)
		{
			return;
		}
		xuiC_ListEntry.SelectCursorElement(true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEntryClicked(T _entry)
	{
		XUiEvent_ListEntryClickedEventHandler<T> listEntryClicked = this.ListEntryClicked;
		if (listEntryClicked == null)
		{
			return;
		}
		listEntryClicked(this, _entry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEntryDoubleClicked(T _entry)
	{
		XUiEvent_ListEntryDoubleClickedEventHandler<T> listEntryDoubleClicked = this.ListEntryDoubleClicked;
		if (listEntryDoubleClicked == null)
		{
			return;
		}
		listEntryDoubleClicked(this, _entry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEntryHovered(T _entry, bool _isOver)
	{
		XUiEvent_ListEntryHoveredEventHandler<T> listEntryHovered = this.ListEntryHovered;
		if (listEntryHovered == null)
		{
			return;
		}
		listEntryHovered(this, _entry, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_List()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<T> filteredEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public T currentSelectedEntry;

	[XuiBindComponent("list.", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_List<T>.XUiC_ListEntry[] listEntryControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_List<T>.XUiC_ListEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedEntryIndex = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool updateSelectedItemByIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pageSizeOverride = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_List<T>.EPagingStepSize pagingStepSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i pagingOverscroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[XuiBindComponent("searchInput", false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_TextInput searchBox;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_CaptionedTextBox searchBox2;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<T> allEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string previousMatch = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreSelectionChange;

	public enum EPagingStepSize
	{
		Page,
		SingleEntry
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class XUiC_ListEntry : XUiController
	{
		[XuiXmlBinding("hasentry")]
		public bool HasEntry
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.entryData != null;
			}
		}

		[XuiXmlBinding("hovered")]
		public bool IsHovered
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.isHovered || this.forceHovered;
			}
		}

		[XuiXmlBinding("isselected")]
		public new bool Selected
		{
			get
			{
				return this.selected;
			}
			set
			{
				if (value)
				{
					if (this.list.SelectedEntry != null)
					{
						this.list.SelectedEntry.selected = false;
					}
				}
				else if (this.list.SelectedEntry == this)
				{
					this.selected = false;
					this.list.ClearSelection();
				}
				this.selected = value;
				if (this.selected)
				{
					this.list.SelectedEntry = this;
				}
				this.IsDirty = true;
			}
		}

		public bool ForceHovered
		{
			get
			{
				return this.forceHovered;
			}
			set
			{
				if (value == this.forceHovered)
				{
					return;
				}
				this.forceHovered = value;
				this.IsDirty = true;
			}
		}

		public override void Init()
		{
			base.Init();
			base.ViewComponent.Enabled = this.HasEntry;
			this.IsDirty = true;
		}

		[XuiBindEvent("OnScroll", null)]
		[PublicizedFrom(EAccessModifier.Private)]
		public void Event_OnScroll(XUiController _sender, float _delta)
		{
			this.list.HandleOnScroll(this, _delta);
		}

		[XuiBindEvent("OnPress", null)]
		public void Event_OnPress(XUiController _sender, int _mouseButton)
		{
			if (!this.Selected)
			{
				this.Selected = true;
			}
			this.list.OnListEntryClicked(this.entryData);
		}

		[XuiBindEvent("OnDoubleClick", null)]
		public void Event_OnDoubleClick(XUiController _sender, int _mouseButton)
		{
			this.list.OnListEntryDoubleClicked(this.entryData);
		}

		[XuiBindEvent("OnHover", null)]
		public void Event_OnHover(XUiController _sender, bool _isOver)
		{
			this.list.OnListEntryHovered(this.entryData, _isOver);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnHovered(bool _isOver)
		{
			this.isHovered = _isOver;
			this.IsDirty = true;
			base.OnHovered(_isOver);
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			T t = this.entryData;
			if (t != null && t.UiDirty)
			{
				this.IsDirty = true;
			}
			if (base.handleDirtyUpdateDefault() && this.entryData != null)
			{
				this.entryData.UiDirty = false;
			}
		}

		public virtual void SetEntry(T _data)
		{
			if (_data != this.entryData)
			{
				this.entryData = _data;
				base.ViewComponent.Enabled = this.HasEntry;
			}
			this.IsDirty = true;
		}

		public T GetEntry()
		{
			return this.entryData;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isHovered;

		[XuiBindParent(true)]
		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly XUiC_List<T> list;

		[PublicizedFrom(EAccessModifier.Protected)]
		public T entryData;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool selected;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool forceHovered;
	}
}
