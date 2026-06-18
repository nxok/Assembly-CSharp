using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BuffInfoWindow : XUiC_InfoWindow
{
	public EntityUINotification Notification
	{
		get
		{
			return this.notification;
		}
		set
		{
			this.overridenBuff = null;
			this.notification = value;
			this.IsDirty = true;
			this.buffName = ((this.notification != null && this.notification.Buff != null) ? Localization.Get(this.notification.Buff.BuffClass.Name, false, null) : "");
			if (value != null && value.Buff != null)
			{
				EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
				for (int i = 0; i < entityPlayer.Buffs.ActiveBuffs.Count; i++)
				{
					if (!entityPlayer.Buffs.ActiveBuffs[i].BuffClass.Hidden && !entityPlayer.Buffs.ActiveBuffs[i].Paused)
					{
						this.overridenBuff = entityPlayer.Buffs.ActiveBuffs[i];
						return;
					}
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.itemPreview = base.GetChildById("itemPreview");
		this.windowName = base.GetChildById("windowName");
		this.windowIcon = base.GetChildById("windowIcon");
		this.description = base.GetChildById("descriptionText");
		this.stats = base.GetChildById("statText");
		this.actionItemList = (XUiC_ItemActionList)base.GetChildById("itemActions");
		this.statButton = base.GetChildById("statButton");
		if (this.statButton != null)
		{
			this.statButton.OnPress += this.StatButton_OnPress;
		}
		this.descriptionButton = base.GetChildById("descriptionButton");
		if (this.descriptionButton != null)
		{
			this.descriptionButton.OnPress += this.DescriptionButton_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DescriptionButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)this.statButton.ViewComponent).Selected = false;
		((XUiV_Button)this.descriptionButton.ViewComponent).Selected = true;
		this.showStats = false;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StatButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)this.statButton.ViewComponent).Selected = true;
		((XUiV_Button)this.descriptionButton.ViewComponent).Selected = false;
		this.showStats = true;
		this.IsDirty = true;
	}

	public override void Deselect()
	{
		if (this.selectedEntry != null)
		{
			this.selectedEntry.IsSelected = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.notification != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2465532423U)
		{
			if (num <= 1988587286U)
			{
				if (num != 1569673958U)
				{
					if (num == 1988587286U)
					{
						if (bindingName == "buffstatus")
						{
							value = ((flag && this.notification.Buff.Paused) ? Localization.Get("TwitchCooldownStatus_Paused", false, null) : "");
							return true;
						}
					}
				}
				else if (bindingName == "buffdescription")
				{
					value = (flag ? this.notification.Buff.BuffClass.Description : "");
					return true;
				}
			}
			else if (num != 2030550547U)
			{
				if (num == 2465532423U)
				{
					if (bindingName == "buffname")
					{
						value = (flag ? Localization.Get(this.notification.Buff.BuffClass.LocalizedName, false, null) : "");
						return true;
					}
				}
			}
			else if (bindingName == "buffcolor")
			{
				Color32 color = (flag ? this.notification.GetColor() : Color.white);
				value = string.Format("{0},{1},{2},{3}", new object[] { color.r, color.g, color.b, color.a });
				return true;
			}
		}
		else if (num <= 3154262838U)
		{
			if (num != 2929532957U)
			{
				if (num == 3154262838U)
				{
					if (bindingName == "showdescription")
					{
						value = (!this.showStats).ToString();
						return true;
					}
				}
			}
			else if (bindingName == "bufficon")
			{
				value = (flag ? this.notification.Icon : "");
				return true;
			}
		}
		else if (num != 3257770903U)
		{
			if (num == 4276755783U)
			{
				if (bindingName == "buffstats")
				{
					value = (flag ? XUiM_PlayerBuffs.GetInfoFromBuff(this.xui.playerUI.entityPlayer, this.notification, this.overridenBuff) : "");
					return true;
				}
			}
		}
		else if (bindingName == "showstats")
		{
			value = this.showStats.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public void SetBuff(XUiC_ActiveBuffEntry buffEntry)
	{
		this.ensureReferencesSet();
		if (this.emptyInfoWindow != null && this.itemInfoWindow != null && buffEntry == null)
		{
			if (!this.itemInfoWindow.ViewComponent.IsVisible)
			{
				this.emptyInfoWindow.ViewComponent.IsVisible = true;
			}
			return;
		}
		this.selectedEntry = buffEntry;
		this.Notification = buffEntry.Notification;
		EntityUINotification entityUINotification = this.notification;
		this.actionItemList.SetCraftingActionList(XUiC_ItemActionList.ItemActionListTypes.Buff, buffEntry);
		if (this.selectedEntry != null)
		{
			base.RefreshBindings();
		}
		this.IsDirty = true;
	}

	public void SetBuffInfo(XUiC_ActiveBuffEntry buff)
	{
		if (buff != null)
		{
			base.ViewComponent.IsVisible = true;
		}
		this.SetBuff(buff);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ensureReferencesSet()
	{
		if (this.emptyInfoWindow == null)
		{
			this.emptyInfoWindow = (XUiC_InfoWindow)this.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
		}
		if (this.itemInfoWindow == null)
		{
			this.itemInfoWindow = (XUiC_ItemInfoWindow)this.xui.FindWindowGroupByName("backpack").GetChildById("itemInfoPanel");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityUINotification notification;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue overridenBuff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ActiveBuffEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController description;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController craftingTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList actionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_InfoWindow emptyInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemInfoWindow itemInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController statButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController descriptionButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 valueColor = new Color32(222, 206, 163, byte.MaxValue);
}
