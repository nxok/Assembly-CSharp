using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapWaypointListEntry : XUiController
{
	public Waypoint Waypoint
	{
		get
		{
			return this._waypoint;
		}
		set
		{
			this._waypoint = value;
			this.Background.IsNavigatable = (this.Background.IsSnappable = this._waypoint != null);
		}
	}

	public new bool Selected
	{
		get
		{
			return this.m_bSelected;
		}
		set
		{
			this.m_bSelected = value;
			this.updateSelected(false);
		}
	}

	public override void Init()
	{
		base.Init();
		this.waypointList = (XUiC_MapWaypointList)base.Parent.GetChildById("waypointList");
		this.Background = (XUiV_Sprite)base.GetChildById("Background").ViewComponent;
		this.Sprite = (XUiV_Sprite)base.GetChildById("Icon").ViewComponent;
		this.Tracking = (XUiV_Sprite)base.GetChildById("Tracking").ViewComponent;
		this.Name = (XUiV_Label)base.GetChildById("Name").ViewComponent;
		this.Distance = (XUiV_Label)base.GetChildById("Distance").ViewComponent;
		this.Background.Controller.OnHover += this.Controller_OnHover;
		this.Background.Controller.OnPress += this.Controller_OnPress;
		this.Background.Controller.OnScroll += this.Controller_OnScroll;
		this.Background.IsNavigatable = (this.Background.IsSnappable = false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Controller_OnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging pager = this.waypointList.pager;
			if (pager == null)
			{
				return;
			}
			pager.PageDown();
			return;
		}
		else
		{
			XUiC_Paging pager2 = this.waypointList.pager;
			if (pager2 == null)
			{
				return;
			}
			pager2.PageUp();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Controller_OnHover(XUiController _sender, bool _isOver)
	{
		this.updateSelected(this.Waypoint != null && _isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.Waypoint == null)
		{
			this.Selected = false;
			return;
		}
		for (int i = 0; i < base.Parent.Children.Count; i++)
		{
			if (base.Parent.Children[i] is XUiC_MapWaypointListEntry)
			{
				((XUiC_MapWaypointListEntry)base.Parent.Children[i]).Selected = false;
			}
		}
		this.waypointList.SelectedWaypoint = this.Waypoint;
		this.waypointList.SelectedWaypointEntry = this;
		this.Selected = true;
		if (InputUtils.ShiftKeyPressed && this.Waypoint != null)
		{
			this.waypointList.TrackedWaypoint = this.Waypoint;
			this.Waypoint.hiddenOnCompass = false;
			this.Waypoint.navObject.hiddenOnCompass = false;
			this.waypointList.UpdateWaypointsList(this.waypointList.SelectedWaypointEntry.Waypoint);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.Tracking.IsVisible = this.Waypoint != null && this.Waypoint.bTracked;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSelected(bool _bHover)
	{
		XUiV_Sprite background = this.Background;
		if (background != null)
		{
			if (this.m_bSelected)
			{
				background.Color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				background.SpriteName = "ui_game_select_row";
			}
			else if (_bHover)
			{
				background.Color = new Color32(96, 96, 96, byte.MaxValue);
				background.SpriteName = "menu_empty";
			}
			else
			{
				background.Color = new Color32(64, 64, 64, byte.MaxValue);
				background.SpriteName = "menu_empty";
			}
		}
		this.Tracking.IsVisible = this.Waypoint != null && this.Waypoint.bTracked;
	}

	public int Index;

	public XUiV_Sprite Background;

	public XUiV_Sprite Sprite;

	public XUiV_Label Name;

	public XUiV_Label Distance;

	public XUiV_Sprite Tracking;

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint _waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_bSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MapWaypointList waypointList;
}
