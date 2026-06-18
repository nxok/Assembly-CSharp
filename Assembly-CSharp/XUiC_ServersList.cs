using System;
using System.Collections.Generic;
using System.Linq;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServersList : XUiC_List<XUiC_ServersList.ListEntry>
{
	public event Action<int> OnFilterResultsChanged;

	public event Action CountsChanged;

	public XUiC_ServersList.EnumServerLists CurrentServerTypeList
	{
		get
		{
			return this.currentServerTypeList;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController xuiController;
		if (base.TryGetChildController<XUiController>("serverlistheader", out xuiController))
		{
			for (int i = 0; i < 5; i++)
			{
				if (xuiController.TryGetChildController<XUiC_Button>(((XUiC_ServersList.EnumColumns)i).ToStringCached<XUiC_ServersList.EnumColumns>(), out this.sortButtons[i]))
				{
					this.sortButtons[i].OnPress += this.SortButton_OnPress;
				}
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
		this.stopListUpdateThread = false;
		ThreadManager.StartThread("ServerBrowserListUpdater", null, new ThreadManager.ThreadFunctionLoopDelegate(this.currentListUpdateThread), null, null, null, false, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.stopListUpdateThread = true;
	}

	public override void Update(float _dt)
	{
		bool flag = false;
		if (this.pageUpdateRequired && (double)(Time.realtimeSinceStartup - this.lastPageUpdate) > 0.1)
		{
			flag = true;
			this.pageUpdateRequired = false;
			this.lastPageUpdate = Time.realtimeSinceStartup;
			this.IsDirty = true;
		}
		base.Update(_dt);
		if (flag)
		{
			Action<int> onFilterResultsChanged = this.OnFilterResultsChanged;
			if (onFilterResultsChanged == null)
			{
				return;
			}
			onFilterResultsChanged(base.EntryCount);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		Dictionary<XUiC_ServersList.EnumServerLists, int> dictionary = this.serverCounts;
		lock (dictionary)
		{
			this.serverCounts.Clear();
		}
		base.RebuildList(_resetFilter);
	}

	public void RefreshBindingListEntry(XUiC_ServersList.ListEntry _entry)
	{
		foreach (XUiC_List<XUiC_ServersList.ListEntry>.XUiC_ListEntry xuiC_ListEntry in this.listEntryControllers)
		{
			if (xuiC_ListEntry.GetEntry() == _entry)
			{
				xuiC_ListEntry.RefreshBindings();
				return;
			}
		}
	}

	public override void RefreshView(bool _resetFilter = false, bool _resetPage = true)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!string.IsNullOrEmpty(_text))
		{
			this.SetFilter(new XUiC_ServersList.UiServerFilter("servername", XUiC_ServersList.EnumServerLists.All, (XUiC_ServersList.ListEntry _entry) => _entry.MatchesSearch(_text), IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null));
		}
		else
		{
			this.SetFilter(new XUiC_ServersList.UiServerFilter("servername", XUiC_ServersList.EnumServerLists.All, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null));
		}
		this.updateCurrentList = true;
		this.IsDirty = true;
	}

	public void SetServerTypeFilter(XUiC_ServersList.EnumServerLists _typelist)
	{
		Func<XUiC_ServersList.ListEntry, bool> func;
		switch (_typelist)
		{
		case XUiC_ServersList.EnumServerLists.Dedicated:
			func = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.IsDedicated;
			goto IL_00D8;
		case XUiC_ServersList.EnumServerLists.Peer:
			func = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.IsPeerToPeer;
			goto IL_00D8;
		case XUiC_ServersList.EnumServerLists.Regular:
			break;
		case XUiC_ServersList.EnumServerLists.Friends:
			func = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.IsFriends;
			goto IL_00D8;
		default:
			if (_typelist == XUiC_ServersList.EnumServerLists.History)
			{
				func = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.IsFavoriteHistory;
				goto IL_00D8;
			}
			if (_typelist == XUiC_ServersList.EnumServerLists.LAN)
			{
				func = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.IsLAN;
				goto IL_00D8;
			}
			break;
		}
		func = null;
		IL_00D8:
		Func<XUiC_ServersList.ListEntry, bool> func2 = func;
		this.currentServerTypeList = _typelist;
		this.SetFilter(new XUiC_ServersList.UiServerFilter("servertype", XUiC_ServersList.EnumServerLists.All, func2, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null));
		base.ClearSelection();
	}

	public void SetFilter(XUiC_ServersList.UiServerFilter _filter)
	{
		Dictionary<string, XUiC_ServersList.UiServerFilter> dictionary = this.currentFilters;
		lock (dictionary)
		{
			if (_filter.Func == null)
			{
				this.currentFilters.Remove(_filter.Name);
			}
			else
			{
				this.currentFilters[_filter.Name] = _filter;
			}
		}
		this.updateCurrentList = true;
	}

	public int GetActiveFilterCount()
	{
		Dictionary<string, XUiC_ServersList.UiServerFilter> dictionary = this.currentFilters;
		int num;
		lock (dictionary)
		{
			num = (this.currentFilters.ContainsKey("servertype") ? (this.currentFilters.Count - 1) : this.currentFilters.Count);
		}
		return num;
	}

	public int GetServerCount(XUiC_ServersList.EnumServerLists _list)
	{
		Dictionary<XUiC_ServersList.EnumServerLists, int> dictionary = this.serverCounts;
		int num;
		lock (dictionary)
		{
			this.serverCounts.TryGetValue(_list, out num);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateListFiltering(ref IEnumerable<XUiC_ServersList.ListEntry> _list)
	{
		Dictionary<string, XUiC_ServersList.UiServerFilter> dictionary = this.currentFilters;
		lock (dictionary)
		{
			foreach (KeyValuePair<string, XUiC_ServersList.UiServerFilter> keyValuePair in this.currentFilters)
			{
				if ((this.currentServerTypeList & keyValuePair.Value.ApplyingLists) != XUiC_ServersList.EnumServerLists.None)
				{
					_list = _list.Where(keyValuePair.Value.Func);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateListSorting(ref IEnumerable<XUiC_ServersList.ListEntry> _list)
	{
		if (this.currentServerTypeList == XUiC_ServersList.EnumServerLists.History && this.sortFuncInt == null && this.sortFuncString == null)
		{
			_list = _list.OrderByDescending(this.sortDefaultFavHistory);
		}
		if (this.sortFuncInt != null)
		{
			_list = (this.sortAscending ? _list.OrderBy(this.sortFuncInt) : _list.OrderByDescending(this.sortFuncInt));
		}
		if (this.sortFuncString != null)
		{
			_list = (this.sortAscending ? _list.OrderBy(this.sortFuncString) : _list.OrderByDescending(this.sortFuncString));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int indexOfGameServerInfo(GameServerInfo _gameServerInfo)
	{
		if (_gameServerInfo == null)
		{
			return -1;
		}
		int hashCode = this.uniqueIdComparer.GetHashCode(_gameServerInfo);
		int count = this.allEntries.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.uniqueIdComparer.GetHashCode(this.allEntries[i].GameServerInfo) == hashCode && this.uniqueIdComparer.Equals(_gameServerInfo, this.allEntries[i].GameServerInfo))
			{
				return i;
			}
		}
		return -1;
	}

	public bool AddGameServer(GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		int num = this.indexOfGameServerInfo(_gameServerInfo);
		bool flag;
		if (num >= 0)
		{
			GameServerInfo gameServerInfo = this.allEntries[num].GameServerInfo;
			this.addRemoveServerCount(gameServerInfo, false);
			gameServerInfo.Merge(_gameServerInfo, _source);
			this.addRemoveServerCount(gameServerInfo, true);
			XUiC_List<XUiC_ServersList.ListEntry>.XUiC_ListEntry xuiC_ListEntry = base.IsVisible(this.allEntries[num]);
			if (xuiC_ListEntry != null)
			{
				xuiC_ListEntry.IsDirty = true;
			}
			flag = false;
		}
		else
		{
			List<XUiC_ServersList.ListEntry> allEntries = this.allEntries;
			lock (allEntries)
			{
				this.allEntries.Add(new XUiC_ServersList.ListEntry(_gameServerInfo, this));
			}
			this.addRemoveServerCount(_gameServerInfo, true);
			flag = true;
		}
		this.updateCurrentList = true;
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addRemoveServerCount(GameServerInfo _gsi, bool _add)
	{
		if (_gsi.IsDedicated)
		{
			this.addRemoveCountSingleListType(_add, XUiC_ServersList.EnumServerLists.Dedicated);
		}
		if (_gsi.IsPeerToPeer)
		{
			this.addRemoveCountSingleListType(_add, XUiC_ServersList.EnumServerLists.Peer);
		}
		if (_gsi.IsFriends)
		{
			this.addRemoveCountSingleListType(_add, XUiC_ServersList.EnumServerLists.Friends);
		}
		if (_gsi.IsFavoriteHistory)
		{
			this.addRemoveCountSingleListType(_add, XUiC_ServersList.EnumServerLists.History);
		}
		if (_gsi.IsLAN)
		{
			this.addRemoveCountSingleListType(_add, XUiC_ServersList.EnumServerLists.LAN);
		}
		Action countsChanged = this.CountsChanged;
		if (countsChanged == null)
		{
			return;
		}
		countsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addRemoveCountSingleListType(bool _add, XUiC_ServersList.EnumServerLists _list)
	{
		Dictionary<XUiC_ServersList.EnumServerLists, int> dictionary = this.serverCounts;
		lock (dictionary)
		{
			int num;
			if (this.serverCounts.TryGetValue(_list, out num))
			{
				this.serverCounts[_list] = num + (_add ? 1 : (-1));
			}
			else if (_add)
			{
				this.serverCounts[_list] = 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentListUpdateThread(ThreadManager.ThreadInfo _tInfo)
	{
		if (this.stopListUpdateThread || _tInfo.TerminationRequested())
		{
			return -1;
		}
		if (!this.updateCurrentList)
		{
			return 50;
		}
		this.updateCurrentList = false;
		List<XUiC_ServersList.ListEntry> allEntries = this.allEntries;
		lock (allEntries)
		{
			this.filteredEntriesTmp.AddRange(this.allEntries);
		}
		IEnumerable<XUiC_ServersList.ListEntry> enumerable = this.filteredEntriesTmp;
		this.updateListFiltering(ref enumerable);
		this.updateListSorting(ref enumerable);
		this.filteredEntriesTmp = enumerable.ToList<XUiC_ServersList.ListEntry>();
		lock (this)
		{
			allEntries = this.filteredEntriesTmp;
			List<XUiC_ServersList.ListEntry> filteredEntries = this.filteredEntries;
			this.filteredEntries = allEntries;
			this.filteredEntriesTmp = filteredEntries;
		}
		this.pageUpdateRequired = true;
		this.filteredEntriesTmp.Clear();
		return 50;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortButton_OnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < this.sortButtons.Length; i++)
		{
			if (_sender == this.sortButtons[i])
			{
				this.updateSortType((XUiC_ServersList.EnumColumns)i);
				break;
			}
		}
		this.IsDirty = true;
	}

	[XuiXmlBinding("sortcolumn")]
	public XUiC_ServersList.EnumColumns SortColumn
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.sortColumn;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == this.sortColumn)
			{
				return;
			}
			this.sortColumn = value;
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSortType(XUiC_ServersList.EnumColumns _sortType)
	{
		switch (_sortType)
		{
		case XUiC_ServersList.EnumColumns.ServerName:
			this.sortFuncString = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoString.GameHost);
			this.sortFuncInt = null;
			break;
		case XUiC_ServersList.EnumColumns.World:
			this.sortFuncString = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoString.LevelName);
			this.sortFuncInt = null;
			break;
		case XUiC_ServersList.EnumColumns.Difficulty:
			this.sortFuncString = null;
			this.sortFuncInt = null;
			break;
		case XUiC_ServersList.EnumColumns.Players:
			this.sortFuncString = null;
			this.sortFuncInt = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoInt.CurrentPlayers);
			break;
		case XUiC_ServersList.EnumColumns.Ping:
			this.sortFuncString = null;
			this.sortFuncInt = (XUiC_ServersList.ListEntry _line) => _line.GameServerInfo.GetValue(GameInfoInt.Ping);
			break;
		default:
			this.sortFuncString = null;
			this.sortFuncInt = null;
			break;
		}
		if (this.sortColumn == _sortType)
		{
			if (!this.sortAscending)
			{
				this.sortColumn = XUiC_ServersList.EnumColumns.Count;
				this.sortFuncString = null;
				this.sortFuncInt = null;
				return;
			}
			this.sortAscending = false;
		}
		else
		{
			this.sortAscending = true;
		}
		this.sortColumn = _sortType;
		this.updateCurrentList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string ServerTypeFilterName = "servertype";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string ServerNameFilterName = "servername";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopListUpdateThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateCurrentList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageUpdateRequired;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastPageUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ServersList.ListEntry> filteredEntriesTmp = new List<XUiC_ServersList.ListEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button[] sortButtons = new XUiC_Button[5];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortAscending = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<XUiC_ServersList.ListEntry, string> sortFuncString;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<XUiC_ServersList.ListEntry, int> sortFuncInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<XUiC_ServersList.ListEntry, int> sortDefaultFavHistory = delegate(XUiC_ServersList.ListEntry _line)
	{
		if (!_line.GameServerInfo.IsFavorite)
		{
			return _line.GameServerInfo.LastPlayedLinux;
		}
		return int.MaxValue;
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.EnumServerLists currentServerTypeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, XUiC_ServersList.UiServerFilter> currentFilters = new Dictionary<string, XUiC_ServersList.UiServerFilter>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly GameServerInfo.UniqueIdEqualityComparer uniqueIdComparer = GameServerInfo.UniqueIdEqualityComparer.Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<XUiC_ServersList.EnumServerLists, int> serverCounts = new EnumDictionary<XUiC_ServersList.EnumServerLists, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.EnumColumns sortColumn = XUiC_ServersList.EnumColumns.Count;

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_ServersList.ListEntry>
	{
		public ListEntry(GameServerInfo _serverInfo, XUiC_ServersList _serversList)
		{
			this.GameServerInfo = _serverInfo;
		}

		public override int CompareTo(XUiC_ServersList.ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return this.GameServerInfo.LastPlayedLinux.CompareTo(_otherEntry.GameServerInfo.LastPlayedLinux);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.GameServerInfo.GetValue(GameInfoString.GameHost).ContainsCaseInsensitive(_searchString);
		}

		public readonly GameServerInfo GameServerInfo;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_ServersList.ListEntry>.XUiC_ListEntry
	{
		public override void Init()
		{
			base.Init();
			this.parentList = (XUiC_ServersList)this.list;
			base.GetChildById("favorite").OnPress += delegate(XUiController _, int _)
			{
				GameServerInfo gameServerInfo = this.entryData.GameServerInfo;
				((XUiC_ServersList)this.list).addRemoveServerCount(gameServerInfo, false);
				ServerInfoCache.Instance.ToggleFavorite(gameServerInfo);
				((XUiC_ServersList)this.list).addRemoveServerCount(gameServerInfo, true);
				this.IsDirty = true;
			};
		}

		public override void Update(float _dt)
		{
			this.IsDirty = true;
			base.Update(_dt);
		}

		[XuiXmlBinding("servername")]
		public string ServerName
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.entryData == null)
				{
					return "";
				}
				string displayTextImmediately = GeneratedTextManager.GetDisplayTextImmediately(this.entryData.GameServerInfo.ServerDisplayName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (!GeneratedTextManager.IsFiltering(this.entryData.GameServerInfo.ServerDisplayName) && !GeneratedTextManager.IsFiltered(this.entryData.GameServerInfo.ServerDisplayName))
				{
					GeneratedTextManager.GetDisplayText(this.entryData.GameServerInfo.ServerDisplayName, delegate(string _)
					{
						this.parentList.RefreshBindingListEntry(this.entryData);
					}, false, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				}
				return displayTextImmediately;
			}
		}

		[XuiXmlBinding("servericon")]
		public string ServerIcon
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.entryData != null)
				{
					return PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(this.entryData.GameServerInfo.PlayGroup, true, EPlatformIdentifier.None);
				}
				return "";
			}
		}

		[XuiXmlBinding("servericonatlas")]
		public string ServerIconAtlas
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return "SymbolAtlas";
			}
		}

		[XuiXmlBinding("playersonline")]
		public int PlayersOnline
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				XUiC_ServersList.ListEntry entryData = this.entryData;
				if (entryData == null)
				{
					return 0;
				}
				return entryData.GameServerInfo.GetValue(GameInfoInt.CurrentPlayers);
			}
		}

		[XuiXmlBinding("playersmax")]
		public int PlayersMax
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				XUiC_ServersList.ListEntry entryData = this.entryData;
				if (entryData == null)
				{
					return 0;
				}
				return entryData.GameServerInfo.GetValue(GameInfoInt.MaxPlayers);
			}
		}

		[XuiXmlBinding("ping")]
		public string Ping
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.entryData == null)
				{
					return "";
				}
				int value = this.entryData.GameServerInfo.GetValue(GameInfoInt.Ping);
				if (value < 0)
				{
					return "N/A";
				}
				return value.ToString();
			}
		}

		[XuiXmlBinding("world")]
		public string World
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.entryData == null)
				{
					return "";
				}
				string displayTextImmediately = GeneratedTextManager.GetDisplayTextImmediately(this.entryData.GameServerInfo.ServerWorldName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				if (!GeneratedTextManager.IsFiltering(this.entryData.GameServerInfo.ServerWorldName) && !GeneratedTextManager.IsFiltered(this.entryData.GameServerInfo.ServerWorldName))
				{
					GeneratedTextManager.GetDisplayText(this.entryData.GameServerInfo.ServerWorldName, delegate(string _)
					{
						this.parentList.RefreshBindingListEntry(this.entryData);
					}, false, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
				}
				return displayTextImmediately;
			}
		}

		[XuiXmlBinding("anticheatprotected")]
		public bool AntiCheatProtected
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				XUiC_ServersList.ListEntry entryData = this.entryData;
				return entryData != null && entryData.GameServerInfo.EACEnabled;
			}
		}

		[XuiXmlBinding("passwordprotected")]
		public bool PasswordColorProtected
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				XUiC_ServersList.ListEntry entryData = this.entryData;
				return entryData != null && entryData.GameServerInfo.GetValue(GameInfoBool.IsPasswordProtected);
			}
		}

		[XuiXmlBinding("isfavorite")]
		public bool IsFavorite
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				XUiC_ServersList.ListEntry entryData = this.entryData;
				return entryData != null && entryData.GameServerInfo.IsFavorite;
			}
		}

		[XuiXmlBinding("isdedicated")]
		public bool IsDedicated
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				XUiC_ServersList.ListEntry entryData = this.entryData;
				return entryData != null && entryData.GameServerInfo.IsDedicated;
			}
		}

		[XuiXmlBinding("hasping")]
		public bool HasPing
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.entryData != null && this.entryData.GameServerInfo.IsCompatibleVersion && this.entryData.GameServerInfo.GetValue(GameInfoInt.Ping) >= 0;
			}
		}

		[XuiXmlBinding("sameversion")]
		public bool SameVersion
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				XUiC_ServersList.ListEntry entryData = this.entryData;
				return entryData == null || entryData.GameServerInfo.IsCompatibleVersion;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_ServersList parentList;
	}

	public class UiServerFilter : IServerListInterface.ServerFilter
	{
		public UiServerFilter(string _name, XUiC_ServersList.EnumServerLists _applyingTo, Func<XUiC_ServersList.ListEntry, bool> _func = null, IServerListInterface.ServerFilter.EServerFilterType _type = IServerListInterface.ServerFilter.EServerFilterType.Any, int _intMinValue = 0, int _intMaxValue = 0, bool _boolValue = false, string _stringNeedle = null)
			: base(_name, _type, _intMinValue, _intMaxValue, _boolValue, _stringNeedle)
		{
			this.Func = _func;
			this.ApplyingLists = _applyingTo;
		}

		public readonly Func<XUiC_ServersList.ListEntry, bool> Func;

		public readonly XUiC_ServersList.EnumServerLists ApplyingLists;
	}

	[Flags]
	public enum EnumServerLists
	{
		None = 0,
		Dedicated = 1,
		Peer = 2,
		Friends = 4,
		History = 8,
		LAN = 16,
		Regular = 3,
		Special = 28,
		All = 31
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EnumColumns
	{
		ServerName,
		World,
		Difficulty,
		Players,
		Ping,
		Count
	}
}
