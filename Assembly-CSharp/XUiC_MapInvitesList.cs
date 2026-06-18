using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapInvitesList : XUiController
{
	public Waypoint SelectedInvite
	{
		get
		{
			return this.selectedInvite;
		}
		set
		{
			this.selectedInvite = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.waypointSetBtn = base.Parent.Parent.GetChildById("waypointSetBtn");
		this.waypointSetBtn.OnPress += this.onInviteAddToWaypoints;
		this.waypointShowOnMapBtn = base.Parent.Parent.GetChildById("showOnMapBtn");
		this.waypointShowOnMapBtn.OnPress += this.onInviteShowOnMapPressed;
		this.waypointRemoveBtn = base.Parent.Parent.GetChildById("waypointRemoveBtn");
		this.waypointRemoveBtn.OnPress += this.onInviteRemovePressed;
		this.waypointReportBtn = base.Parent.Parent.GetChildById("waypointReportBtn");
		this.waypointReportBtn.OnPress += this.onReportWaypointPressed;
		this.list = (XUiV_Grid)base.GetChildById("invitesList").ViewComponent;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.UpdateInvitesList();
	}

	public void UpdateInvitesList()
	{
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		List<Waypoint> waypointInvites = entityPlayer.WaypointInvites;
		int num = 0;
		foreach (XUiController xuiController in this.children)
		{
			XUiC_MapInvitesListEntry uiEntry = xuiController as XUiC_MapInvitesListEntry;
			if (uiEntry != null)
			{
				if (num >= waypointInvites.Count)
				{
					uiEntry.Index = num;
					uiEntry.Sprite.SpriteName = string.Empty;
					uiEntry.Waypoint = null;
					uiEntry.Name.Text = string.Empty;
					uiEntry.Selected = false;
					uiEntry.Background.SoundPlayOnClick = false;
					uiEntry.Distance.Text = string.Empty;
				}
				else
				{
					Waypoint waypoint = waypointInvites[num];
					uiEntry.Index = num;
					uiEntry.Sprite.SpriteName = waypoint.icon;
					uiEntry.Waypoint = waypoint;
					if (waypoint.bIsAutoWaypoint)
					{
						uiEntry.Name.Text = Localization.Get(waypoint.name.Text, false, null);
					}
					else
					{
						GeneratedTextManager.GetDisplayText(waypoint.name, delegate(string _filtered)
						{
							uiEntry.Name.Text = _filtered;
						}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
					}
					uiEntry.Selected = false;
					uiEntry.Background.SoundPlayOnClick = true;
					Vector3 vector = waypoint.pos.ToVector3();
					Vector3 position = entityPlayer.GetPosition();
					vector.y = 0f;
					position.y = 0f;
					float magnitude = (vector - position).magnitude;
					uiEntry.Distance.Text = ValueDisplayFormatters.Distance(magnitude);
				}
				num++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteAddToWaypoints(XUiController _sender, int _mouseButton)
	{
		if (this.SelectedInvite != null)
		{
			EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
			entityPlayer.WaypointInvites.Remove(this.SelectedInvite);
			entityPlayer.Waypoints.Collection.Add(this.SelectedInvite);
			this.SelectedInviteEntry.Selected = false;
			this.SelectedInvite.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", this.SelectedInvite.pos.ToVector3(), this.SelectedInvite.icon, false, -1, null);
			this.SelectedInvite.navObject.IsActive = false;
			this.SelectedInvite.navObject.usingLocalizationId = this.SelectedInvite.bUsingLocalizationId;
			GeneratedTextManager.GetDisplayText(this.SelectedInvite.name, delegate(string _filtered)
			{
				this.SelectedInvite.navObject.name = _filtered;
			}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			XUiV_Window window = this.xui.GetWindow("mapTracking");
			if (window != null && window.IsVisible)
			{
				((XUiC_MapWaypointList)window.Controller.GetChildById("waypointList")).UpdateWaypointsList(null);
			}
			this.SelectedInvite = null;
			this.SelectedInviteEntry = null;
			this.UpdateInvitesList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteShowOnMapPressed(XUiController _sender, int _mouseButton)
	{
		if (this.SelectedInvite != null)
		{
			((XUiC_MapArea)this.xui.GetWindow("mapArea").Controller).PositionMapAt(this.SelectedInvite.pos.ToVector3());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInviteRemovePressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (this.SelectedInvite != null)
		{
			entityPlayer.WaypointInvites.Remove(this.SelectedInvite);
			this.SelectedInviteEntry.Selected = false;
			this.SelectedInvite = null;
			this.SelectedInviteEntry = null;
			this.UpdateInvitesList();
			Manager.PlayInsidePlayerHead("ui_waypoint_delete", -1, 0f, false, false);
			return;
		}
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onReportWaypointPressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (this.SelectedInvite != null && !this.SelectedInvite.bIsAutoWaypoint && PlatformManager.MultiPlatform.PlayerReporting != null)
		{
			PersistentPlayerData ppData = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.SelectedInvite.InviterEntityId);
			if (ppData != null)
			{
				GeneratedTextManager.GetDisplayText(this.SelectedInvite.name, delegate(string _filtered)
				{
					ThreadManager.AddSingleTaskMainThread("OpenReportWindow", delegate(object _)
					{
						XUiC_ReportPlayer.Open(ppData.PlayerData, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage", false, null), _filtered));
					}, null);
				}, true, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			}
			return;
		}
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Waypoint selectedInvite;

	public XUiC_MapInvitesListEntry SelectedInviteEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid list;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointSetBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointShowOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController waypointReportBtn;
}
