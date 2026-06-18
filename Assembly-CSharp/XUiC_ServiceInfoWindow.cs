using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServiceInfoWindow : XUiC_InfoWindow
{
	public override void Init()
	{
		base.Init();
		this.servicePreview = base.GetChildById("servicePreview");
		this.windowName = base.GetChildById("windowName");
		this.windowIcon = base.GetChildById("windowIcon");
		this.description = base.GetChildById("descriptionText");
		this.stats = base.GetChildById("statText");
		this.mainActionItemList = (XUiC_ItemActionList)base.GetChildById("itemActions");
		base.OnVisiblity += base.OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= base.OnVisibilityChanged;
	}

	public override void Deselect()
	{
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty && base.ViewComponent.IsVisible)
		{
			if (this.emptyInfoWindow == null)
			{
				this.emptyInfoWindow = (XUiC_InfoWindow)this.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2353034265U)
		{
			if (num <= 1380912366U)
			{
				if (num != 165025526U)
				{
					if (num == 1380912366U)
					{
						if (bindingName == "servicedescription")
						{
							value = ((this.service != null) ? this.service.Description : "");
							return true;
						}
					}
				}
				else if (bindingName == "servicegroupicon")
				{
					value = ((this.service != null) ? this.service.Icon : "");
					return true;
				}
			}
			else if (num != 2341642767U)
			{
				if (num == 2353034265U)
				{
					if (bindingName == "servicecost")
					{
						value = ((this.service != null) ? this.servicecostFormatter.Format(this.service.Price) : "");
						return true;
					}
				}
			}
			else if (bindingName == "servicestats")
			{
				value = ((this.service != null) ? this.stat1 : "");
				return true;
			}
		}
		else if (num <= 2418997840U)
		{
			if (num != 2390918988U)
			{
				if (num == 2418997840U)
				{
					if (bindingName == "pricelabel")
					{
						value = Localization.Get("xuiCost", false, null);
						return true;
					}
				}
			}
			else if (bindingName == "serviceicontint")
			{
				Color32 color = Color.white;
				value = this.serviceicontintcolorFormatter.Format(color);
				return true;
			}
		}
		else if (num != 3116710815U)
		{
			if (num == 3397569669U)
			{
				if (bindingName == "serviceicon")
				{
					value = ((this.service != null) ? this.service.Icon : "");
					return true;
				}
			}
		}
		else if (bindingName == "servicename")
		{
			value = ((this.service != null) ? this.service.Name : "");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public void SetInfo(InGameService _service, XUiController controller)
	{
		this.service = _service;
		if (this.service == null)
		{
			if (this.emptyInfoWindow == null)
			{
				this.emptyInfoWindow = (XUiC_InfoWindow)this.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			this.emptyInfoWindow.ViewComponent.IsVisible = true;
			return;
		}
		base.ViewComponent.IsVisible = true;
		if (this.servicePreview == null)
		{
			return;
		}
		string text = Utils.ColorToHex(this.valueColor);
		this.stat1 = XUiM_InGameService.GetServiceStats(this.xui, this.service).Replace("REPLACE_COLOR", text);
		this.mainActionItemList.SetServiceActionList(this.service, controller);
		base.RefreshBindings();
	}

	public void OnVisibilityChanged(XUiController _sender, bool _isVisible)
	{
		if (this.service != null)
		{
			this.service.VisibleChangedHandler(_isVisible);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.service == null)
		{
			if (this.emptyInfoWindow == null)
			{
				this.emptyInfoWindow = (XUiC_InfoWindow)this.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			this.emptyInfoWindow.ViewComponent.IsVisible = true;
			return;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.service = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public InGameService service;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController servicePreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController description;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList mainActionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 valueColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_InfoWindow emptyInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public string stat1 = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt servicecostFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor serviceicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();
}
