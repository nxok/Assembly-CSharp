using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ActiveBuffList : XUiController, IEntityUINotificationChanged
{
	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			if (this.page != value)
			{
				this.page = value;
				this.IsDirty = true;
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging == null)
				{
					return;
				}
				xuiC_Paging.SetPage(this.page);
			}
		}
	}

	public XUiC_ActiveBuffEntry SelectedEntry
	{
		get
		{
			return this.selectedEntry;
		}
		set
		{
			if (this.selectedEntry != null)
			{
				this.selectedEntry.IsSelected = false;
			}
			this.selectedEntry = value;
			if (this.selectedEntry != null)
			{
				this.selectedEntry.IsSelected = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetFirstEntry()
	{
		XUiC_BuffInfoWindow childByType = base.WindowGroup.Controller.GetChildByType<XUiC_BuffInfoWindow>();
		this.SelectedEntry = ((this.entryList[0].Notification != null) ? this.entryList[0] : null);
		childByType.SetBuff(this.SelectedEntry);
	}

	public override void Init()
	{
		base.Init();
		XUiC_BuffInfoWindow childByType = base.WindowGroup.Controller.GetChildByType<XUiC_BuffInfoWindow>();
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_ActiveBuffEntry)
			{
				XUiC_ActiveBuffEntry xuiC_ActiveBuffEntry = (XUiC_ActiveBuffEntry)this.children[i];
				xuiC_ActiveBuffEntry.InfoWindow = childByType;
				this.entryList.Add(xuiC_ActiveBuffEntry);
				this.length++;
			}
		}
		this.pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate
			{
				this.Page = this.pager.CurrentPageNumber;
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressBuff(XUiController _sender, int _mouseButton)
	{
		XUiC_ActiveBuffEntry xuiC_ActiveBuffEntry = _sender as XUiC_ActiveBuffEntry;
		if (xuiC_ActiveBuffEntry != null)
		{
			this.SelectedEntry = xuiC_ActiveBuffEntry;
		}
	}

	public override void Update(float _dt)
	{
		EntityUINotification selectedNotification = this.xui.BuffPopoutList.SelectedNotification;
		if (selectedNotification != null)
		{
			this.xui.BuffPopoutList.SelectedNotification = null;
			for (int i = 0; i < this.buffNotificationList.Count; i++)
			{
				if (this.buffNotificationList[i] == selectedNotification)
				{
					this.Page = i / this.length;
				}
			}
		}
		if (this.IsDirty)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging != null)
			{
				xuiC_Paging.SetLastPageByElementsAndPageLength(this.buffNotificationList.Count, this.entryList.Count);
			}
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 != null)
			{
				xuiC_Paging2.SetPage(this.page);
			}
			for (int j = 0; j < this.length; j++)
			{
				int num = j + this.length * this.page;
				XUiC_ActiveBuffEntry xuiC_ActiveBuffEntry = this.entryList[j];
				if (xuiC_ActiveBuffEntry != null)
				{
					xuiC_ActiveBuffEntry.OnPress -= this.OnPressBuff;
					if (num < this.buffNotificationList.Count)
					{
						xuiC_ActiveBuffEntry.Notification = this.buffNotificationList[num];
						xuiC_ActiveBuffEntry.OnPress += this.OnPressBuff;
						xuiC_ActiveBuffEntry.ViewComponent.SoundPlayOnClick = true;
					}
					else
					{
						xuiC_ActiveBuffEntry.Notification = null;
						xuiC_ActiveBuffEntry.ViewComponent.SoundPlayOnClick = false;
					}
				}
			}
			if (this.setFirstEntry)
			{
				this.SetFirstEntry();
				this.setFirstEntry = false;
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
		if (selectedNotification != null)
		{
			for (int k = 0; k < this.entryList.Count; k++)
			{
				if (this.entryList[k].Notification == selectedNotification)
				{
					this.SelectedEntry = this.entryList[k];
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetNotificationIndex(EntityUINotification notification)
	{
		for (int i = 0; i < this.buffNotificationList.Count; i++)
		{
			if (this.buffNotificationList[i].Subject == notification.Subject)
			{
				if (notification.Subject != EnumEntityUINotificationSubject.Buff)
				{
					return i;
				}
				if (this.buffNotificationList[i].Buff.BuffClass.Name == notification.Buff.BuffClass.Name)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void EntityUINotificationAdded(EntityUINotification _notification)
	{
		if (_notification.Buff == null)
		{
			return;
		}
		int notificationIndex = this.GetNotificationIndex(_notification);
		if (notificationIndex == -1)
		{
			this.buffNotificationList.Add(_notification);
		}
		else
		{
			this.buffNotificationList[notificationIndex] = _notification;
		}
		this.IsDirty = true;
	}

	public void EntityUINotificationRemoved(EntityUINotification _notification)
	{
		if (_notification.Buff == null)
		{
			return;
		}
		this.buffNotificationList.Remove(_notification);
		if (this.SelectedEntry != null && this.SelectedEntry.Notification == _notification)
		{
			this.SelectedEntry.InfoWindow.SetBuffInfo(null);
			this.SelectedEntry = null;
		}
		this.IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		this.buffNotificationList.Clear();
		List<EntityUINotification> notifications = entityPlayer.PlayerStats.Notifications;
		for (int i = 0; i < notifications.Count; i++)
		{
			if (notifications[i].Buff != null)
			{
				this.buffNotificationList.Add(notifications[i]);
			}
		}
		entityPlayer.PlayerStats.AddUINotificationChangedDelegate(this);
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.entityPlayer.PlayerStats.RemoveUINotificationChangedDelegate(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_ActiveBuffEntry> entryList = new List<XUiC_ActiveBuffEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityUINotification> buffNotificationList = new List<EntityUINotification>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ActiveBuffEntry selectedEntry;

	public bool setFirstEntry;

	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;
}
