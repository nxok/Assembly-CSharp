using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapInvitesListEntry : XUiController
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
			if (this.m_bSelected && !value && this.Waypoint != null && this.Waypoint.navObject != null && this.Waypoint.navObject.NavObjectClass.NavObjectClassName == "waypoint_invite")
			{
				NavObjectManager.Instance.UnRegisterNavObject(this.Waypoint.navObject);
			}
			if (!this.m_bSelected && value)
			{
				this.Waypoint.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint_invite", this.Waypoint.pos.ToVector3(), this.Waypoint.icon, false, -1, null);
				this.Waypoint.navObject.IsActive = false;
				this.Waypoint.navObject.name = GeneratedTextManager.GetDisplayTextImmediately(this.Waypoint.name, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
				this.Waypoint.navObject.usingLocalizationId = this.Waypoint.bUsingLocalizationId;
			}
			this.m_bSelected = value;
			this.updateSelected(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int makeKey()
	{
		return int.MaxValue - this.Index;
	}

	public override void Init()
	{
		base.Init();
		this.waypointList = (XUiC_MapInvitesList)base.Parent.GetChildById("invitesList");
		this.Background = (XUiV_Sprite)base.GetChildById("Background").ViewComponent;
		this.Sprite = (XUiV_Sprite)base.GetChildById("Icon").ViewComponent;
		this.Name = (XUiV_Label)base.GetChildById("Name").ViewComponent;
		this.Distance = (XUiV_Label)base.GetChildById("Distance").ViewComponent;
		this.Background.Controller.OnHover += this.Controller_OnHover;
		this.Background.Controller.OnPress += this.Controller_OnPress;
		this.Background.IsNavigatable = (this.Background.IsSnappable = false);
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
			if (base.Parent.Children[i] is XUiC_MapInvitesListEntry)
			{
				((XUiC_MapInvitesListEntry)base.Parent.Children[i]).Selected = false;
			}
		}
		this.waypointList.SelectedInvite = this.Waypoint;
		this.waypointList.SelectedInviteEntry = this;
		this.Selected = true;
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
				return;
			}
			if (_bHover)
			{
				background.Color = new Color32(96, 96, 96, byte.MaxValue);
				background.SpriteName = "menu_empty";
				return;
			}
			background.Color = new Color32(64, 64, 64, byte.MaxValue);
			background.SpriteName = "menu_empty";
		}
	}

	public int Index;

	public XUiV_Sprite Background;

	public XUiV_Sprite Sprite;

	public XUiV_Label Name;

	public XUiV_Label Distance;

	public Action RefreshNameAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint _waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_bSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MapInvitesList waypointList;
}
