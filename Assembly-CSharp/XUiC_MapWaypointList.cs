using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapWaypointList : XUiController
{
	public Waypoint TrackedWaypoint
	{
		get
		{
			return this.trackedWaypoint;
		}
		set
		{
			if (this.trackedWaypoint != null)
			{
				this.trackedWaypoint.bTracked = false;
				this.trackedWaypoint.navObject.IsActive = this.trackedWaypoint.bTracked;
			}
			this.trackedWaypoint = value;
			if (this.trackedWaypoint != null)
			{
				this.trackedWaypoint.bTracked = true;
				this.trackedWaypoint.navObject.IsActive = this.trackedWaypoint.bTracked;
			}
		}
	}

	public Waypoint SelectedWaypoint
	{
		get
		{
			return this.selectedWaypoint;
		}
		set
		{
			this.selectedWaypoint = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.list = (XUiV_Grid)base.GetChildById("waypointList").ViewComponent;
		this.cCountWaypointsPerPage = this.list.Columns * this.list.Rows;
		this.pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate
			{
				this.currentPage = this.pager.CurrentPageNumber;
				this.UpdateWaypointsList(null);
				if (this.SelectedWaypointEntry != null)
				{
					this.SelectedWaypointEntry.Selected = false;
				}
			};
		}
		this.trackBtn = base.Parent.GetChildById("trackBtn");
		this.trackBtn.OnPress += this.onTrackWaypointPressed;
		this.showOnMapBtn = base.Parent.GetChildById("showOnMapBtn");
		this.showOnMapBtn.OnPress += this.onShowOnMapPressed;
		this.waypointRemoveBtn = base.Parent.GetChildById("waypointRemoveBtn");
		this.waypointRemoveBtn.OnPress += this.onWaypointRemovePressed;
		this.inviteBtn = base.Parent.GetChildById("inviteBtn");
		this.inviteBtn.OnPress += this.onInvitePressed;
		this.txtInputFilter = (XUiC_TextInput)base.Parent.GetChildById("searchInput");
		this.txtInputFilter.Text = string.Empty;
		this.txtInputFilter.OnChangeHandler += this.waypointFilterOnChangeHandler;
		this.txtInputFilter.OnSubmitHandler += this.waypointFilerOnSubmitHandler;
		this.xui.GetWindow("mapTrackingPopup").Controller.GetChildById("inviteFriends").OnPress += this.onInviteFriendsPressed;
		this.xui.GetWindow("mapTrackingPopup").Controller.GetChildById("inviteEveryone").OnPress += this.onInviteEveryonePressed;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.SelectedWaypointEntry != null)
		{
			this.SelectedWaypoint = null;
			this.SelectedWaypointEntry.Selected = false;
		}
		this.currentPage = 0;
		this.filterString = this.txtInputFilter.Text;
		this.GetTrackedWaypoint();
		this.UpdateWaypointsList(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetTrackedWaypoint()
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		bool flag = false;
		for (int i = 0; i < entityPlayer.Waypoints.Collection.list.Count; i++)
		{
			Waypoint waypoint = entityPlayer.Waypoints.Collection.list[i];
			if (waypoint.bTracked)
			{
				if (!flag)
				{
					this.TrackedWaypoint = waypoint;
					flag = true;
				}
				else
				{
					waypoint.bTracked = false;
					waypoint.navObject.IsActive = false;
				}
			}
		}
	}

	public void UpdateWaypointsList(Waypoint _selectThisWaypoint = null)
	{
		if (this.pager == null)
		{
			this.updateWaypointsNextUpdate = true;
			return;
		}
		for (int i = 0; i < this.cCountWaypointsPerPage; i++)
		{
			XUiC_MapWaypointListEntry xuiC_MapWaypointListEntry = (XUiC_MapWaypointListEntry)this.children[i];
			if (xuiC_MapWaypointListEntry != null)
			{
				xuiC_MapWaypointListEntry.Index = i;
				xuiC_MapWaypointListEntry.Sprite.SpriteName = string.Empty;
				xuiC_MapWaypointListEntry.Name.Text = string.Empty;
				xuiC_MapWaypointListEntry.Distance.Text = string.Empty;
				xuiC_MapWaypointListEntry.Waypoint = null;
				xuiC_MapWaypointListEntry.Selected = false;
				xuiC_MapWaypointListEntry.Background.Enabled = false;
			}
		}
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		List<Waypoint> list = new List<Waypoint>();
		foreach (Waypoint waypoint in entityPlayer.Waypoints.Collection.list)
		{
			if (!waypoint.HiddenOnMap)
			{
				list.Add(waypoint);
			}
		}
		list.Sort(new XUiC_MapWaypointList.WaypointSorter(entityPlayer));
		if (this.txtInputFilter.Text != null && this.txtInputFilter.Text != string.Empty)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (!list[j].name.Text.ContainsCaseInsensitive(this.txtInputFilter.Text))
				{
					list.RemoveAt(j);
					j--;
					if (j < 0)
					{
						j = 0;
					}
				}
			}
			if (this.filterString != this.txtInputFilter.Text)
			{
				this.currentPage = 0;
				this.filterString = this.txtInputFilter.Text;
			}
		}
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging != null)
		{
			xuiC_Paging.SetLastPageByElementsAndPageLength(list.Count, this.cCountWaypointsPerPage);
		}
		XUiC_Paging xuiC_Paging2 = this.pager;
		if (xuiC_Paging2 != null)
		{
			xuiC_Paging2.SetPage(this.currentPage);
		}
		int num = 0;
		for (int k = 0; k < this.cCountWaypointsPerPage; k++)
		{
			int num2 = k + this.cCountWaypointsPerPage * this.currentPage;
			if (num2 >= list.Count)
			{
				break;
			}
			XUiC_MapWaypointListEntry waypointEntry = (XUiC_MapWaypointListEntry)this.children[num];
			if (waypointEntry != null && (this.txtInputFilter.Text == null || !(this.txtInputFilter.Text != string.Empty) || list[num2].name.Text.ContainsCaseInsensitive(this.txtInputFilter.Text)))
			{
				waypointEntry.Background.Enabled = true;
				waypointEntry.Index = k;
				waypointEntry.Sprite.SpriteName = list[num2].icon;
				waypointEntry.Waypoint = list[num2];
				if (waypointEntry.Waypoint.bIsAutoWaypoint || waypointEntry.Waypoint.bUsingLocalizationId)
				{
					waypointEntry.Name.Text = Localization.Get(waypointEntry.Waypoint.name.Text, false, null);
				}
				else
				{
					GeneratedTextManager.GetDisplayText(waypointEntry.Waypoint.name, delegate(string _filtered)
					{
						waypointEntry.Name.Text = _filtered;
					}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
				}
				waypointEntry.Selected = _selectThisWaypoint != null && _selectThisWaypoint.Equals(list[num2]);
				Vector3 vector = list[num2].pos.ToVector3();
				Vector3 position = entityPlayer.GetPosition();
				vector.y = 0f;
				position.y = 0f;
				float num3 = (vector - position).magnitude;
				string text = "m";
				if (num3 >= 1000f)
				{
					num3 /= 1000f;
					text = "km";
				}
				waypointEntry.Distance.Text = string.Format("{0} {1}", num3.ToCultureInvariantString("0.0"), text);
				if (_selectThisWaypoint != null && _selectThisWaypoint.Equals(list[num2]))
				{
					this.SelectedWaypointEntry = waypointEntry;
				}
				num++;
			}
		}
	}

	public void SelectWaypoint(Waypoint _w)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		List<Waypoint> list = new List<Waypoint>(entityPlayer.Waypoints.Collection.list);
		list.Sort(new XUiC_MapWaypointList.WaypointSorter(entityPlayer));
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Equals(_w))
			{
				this.currentPage = i / this.cCountWaypointsPerPage;
				this.UpdateWaypointsList(_w);
				this.SelectedWaypoint = _w;
				return;
			}
		}
	}

	public void SelectWaypoint(NavObject _nav)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		List<Waypoint> list = new List<Waypoint>(entityPlayer.Waypoints.Collection.list);
		list.Sort(new XUiC_MapWaypointList.WaypointSorter(entityPlayer));
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].navObject.Equals(_nav))
			{
				this.currentPage = i / this.cCountWaypointsPerPage;
				this.UpdateWaypointsList(list[i]);
				this.SelectedWaypoint = list[i];
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onTrackWaypointPressed(XUiController _sender, int _mouseButton)
	{
		Waypoint waypoint = this.GetSelectedWaypoint();
		if (waypoint != null && this.SelectedWaypointEntry != null)
		{
			if (this.TrackedWaypoint == waypoint)
			{
				this.TrackedWaypoint = null;
			}
			else
			{
				this.TrackedWaypoint = waypoint;
				this.trackedWaypoint.hiddenOnCompass = false;
				this.trackedWaypoint.navObject.hiddenOnCompass = false;
			}
			this.UpdateWaypointsList(this.SelectedWaypointEntry.Waypoint);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInvitePressed(XUiController _sender, int _mouseButton)
	{
		if (this.selectedWaypoint != null)
		{
			this.xui.GetWindow("mapTrackingPopup").IsVisible = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onShowOnMapPressed(XUiController _sender, int _mouseButton)
	{
		Waypoint waypoint = this.GetSelectedWaypoint();
		if (waypoint != null)
		{
			((XUiC_MapArea)this.xui.GetWindow("mapArea").Controller).PositionMapAt(waypoint.pos.ToVector3());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onWaypointRemovePressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		Waypoint waypoint = this.GetSelectedWaypoint();
		if (waypoint != null && (waypoint.lastKnownPositionEntityId == -1 || waypoint.bIsAutoWaypoint))
		{
			entityPlayer.Waypoints.Collection.Remove(waypoint);
			NavObjectManager.Instance.UnRegisterNavObject(waypoint.navObject);
			this.UpdateWaypointsList(null);
			this.SelectedWaypoint = null;
			if (this.SelectedWaypointEntry != null)
			{
				this.SelectedWaypointEntry.Selected = false;
			}
			Manager.PlayInsidePlayerHead("ui_waypoint_delete", -1, 0f, false, false);
			return;
		}
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	public Waypoint GetSelectedWaypoint()
	{
		return this.SelectedWaypoint;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteFriendsPressed(XUiController _sender, int _mouseButton)
	{
		if (this.SelectedWaypoint != null)
		{
			EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
			GameManager.Instance.WaypointInviteServer(this.SelectedWaypoint, EnumWaypointInviteMode.Friends, entityPlayer.entityId);
			this.xui.GetWindow("mapTrackingPopup").IsVisible = false;
			GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("tooltipInviteFriends", false, null), this.SelectedWaypoint.navObject.DisplayName), false, false, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteEveryonePressed(XUiController _sender, int _mouseButton)
	{
		if (this.SelectedWaypoint != null)
		{
			EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
			GameManager.Instance.WaypointInviteServer(this.SelectedWaypoint, EnumWaypointInviteMode.Everyone, entityPlayer.entityId);
			this.xui.GetWindow("mapTrackingPopup").IsVisible = false;
			GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("tooltipInviteEveryone", false, null), this.SelectedWaypoint.navObject.DisplayName), false, false, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointFilerOnSubmitHandler(XUiController _sender, string _text)
	{
		this.UpdateWaypointsList(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waypointFilterOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.UpdateWaypointsList(null);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.updateWaypointsNextUpdate)
		{
			this.updateWaypointsNextUpdate = false;
			this.UpdateWaypointsList(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint trackedWaypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint selectedWaypoint;

	public XUiC_MapWaypointListEntry SelectedWaypointEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cCountWaypointsPerPage = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid list;

	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInputFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController trackBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController showOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController inviteBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateWaypointsNextUpdate;

	public class WaypointSorter : IComparer<Waypoint>
	{
		public WaypointSorter(EntityPlayerLocal _localPlayer)
		{
			this.localPlayerPos = _localPlayer.GetPosition();
		}

		public int Compare(Waypoint _w1, Waypoint _w2)
		{
			float sqrMagnitude = (_w1.pos.ToVector3() - this.localPlayerPos).sqrMagnitude;
			float sqrMagnitude2 = (_w2.pos.ToVector3() - this.localPlayerPos).sqrMagnitude;
			if (sqrMagnitude < sqrMagnitude2)
			{
				return -1;
			}
			if (sqrMagnitude > sqrMagnitude2)
			{
				return 1;
			}
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 localPlayerPos;
	}
}
