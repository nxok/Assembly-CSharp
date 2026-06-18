using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ActiveBuffEntry : XUiC_SelectableEntry
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
			EntityUINotification entityUINotification = this.notification;
			this.buffName = ((((entityUINotification != null) ? entityUINotification.Buff : null) != null) ? this.notification.Buff.BuffClass.LocalizedName : "");
			base.ViewComponent.Enabled = value != null;
			if (((value != null) ? value.Buff : null) != null)
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

	public XUiC_BuffInfoWindow InfoWindow { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			this.InfoWindow.SetBuffInfo(this);
		}
		if (this.background != null)
		{
			this.background.Color = (isSelected ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			this.background.SpriteName = (isSelected ? "ui_game_select_row" : "menu_empty");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.notification != null;
		if (bindingName == "buffname")
		{
			value = (flag ? this.buffName : "");
			return true;
		}
		if (bindingName == "buffdisplayinfo")
		{
			value = (flag ? XUiM_PlayerBuffs.GetBuffDisplayInfo(this.notification, this.overridenBuff) : "");
			return true;
		}
		if (bindingName == "bufficon")
		{
			value = (flag ? this.notification.Icon : "");
			return true;
		}
		if (bindingName == "buffcolor")
		{
			Color32 color = (flag ? this.notification.GetColor() : Color.white);
			value = string.Format("{0},{1},{2},{3}", new object[] { color.r, color.g, color.b, color.a });
			return true;
		}
		if (!(bindingName == "fontcolor"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = ((flag && this.notification.Buff.Paused) ? "128,128,128,255" : "255,255,255,255");
		return true;
	}

	public override void Init()
	{
		base.Init();
		this.background = (XUiV_Sprite)base.GetChildById("background").ViewComponent;
		base.OnScroll += this.HandleOnScroll;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging pager = ((XUiC_ActiveBuffList)base.Parent).pager;
			if (pager == null)
			{
				return;
			}
			pager.PageDown();
			return;
		}
		else
		{
			XUiC_Paging pager2 = ((XUiC_ActiveBuffList)base.Parent).pager;
			if (pager2 == null)
			{
				return;
			}
			pager2.PageUp();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		if (this.background != null && !base.IsSelected)
		{
			if (_isOver && this.notification != null)
			{
				this.background.Color = new Color32(96, 96, 96, byte.MaxValue);
			}
			else
			{
				this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.OnHovered(_isOver);
	}

	public override void Update(float _dt)
	{
		base.RefreshBindings();
		this.IsDirty = false;
		base.Update(_dt);
	}

	public void Refresh()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityUINotification notification;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue overridenBuff;
}
