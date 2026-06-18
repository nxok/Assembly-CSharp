using System;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DataManagementBar : XUiController
{
	[XuiXmlBinding("isroamingoptional")]
	public bool IsRoamingOptional
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
		}
	}

	[XuiXmlBinding("warningtext")]
	public string WarningText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			long num = this.allowanceBytes - this.usedBytes;
			if (this.displayMode == XUiC_DataManagementBar.DisplayMode.Selection || num >= this.pendingBytes)
			{
				return string.Empty;
			}
			long num2 = this.pendingBytes - num;
			return string.Format(Localization.Get("xuiDmBarRequiredSpaceWarning", false, null), ValueDisplayFormatters.MemoryMiB(num2));
		}
	}

	public override void Init()
	{
		base.Init();
		this.selectionFillColor = this.barSelectedPrimaryFill.Color;
		this.fullWidth = this.background.Size.x;
		this.fullHeight = this.background.Size.y;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public void SetDisplayMode(XUiC_DataManagementBar.DisplayMode _displayMode)
	{
		if (this.displayMode == _displayMode)
		{
			return;
		}
		this.displayMode = _displayMode;
		this.IsDirty = true;
	}

	public void SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion _primaryRegion)
	{
		this.SetSelectedByteRegion(_primaryRegion, XUiC_DataManagementBar.BarRegion.None, XUiC_DataManagementBar.BarRegion.None);
	}

	public void SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion _primaryRegion, XUiC_DataManagementBar.BarRegion _secondaryRegion)
	{
		this.SetSelectedByteRegion(_primaryRegion, _secondaryRegion, XUiC_DataManagementBar.BarRegion.None);
	}

	public void SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion _primaryRegion, XUiC_DataManagementBar.BarRegion _secondaryRegion, XUiC_DataManagementBar.BarRegion _tertiaryRegion)
	{
		if (!this.primaryByteRegion.Equals(_primaryRegion))
		{
			this.primaryByteRegion = _primaryRegion;
			this.IsDirty = true;
		}
		if (!this.secondaryByteRegion.Equals(_secondaryRegion))
		{
			this.secondaryByteRegion = _secondaryRegion;
			this.IsDirty = true;
		}
		if (!this.tertiaryByteRegion.Equals(_tertiaryRegion))
		{
			this.tertiaryByteRegion = _tertiaryRegion;
			this.IsDirty = true;
		}
	}

	public void SetHoveredByteRegion(XUiC_DataManagementBar.BarRegion _region)
	{
		if (!this.hoveredByteRegion.Equals(_region))
		{
			this.hoveredByteRegion = _region;
			this.IsDirty = true;
		}
	}

	public void SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion _region)
	{
		if (!this.archivePreviewByteRegion.Equals(_region))
		{
			this.archivePreviewByteRegion = _region;
			this.IsDirty = true;
		}
	}

	public void SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth _selectionDepth)
	{
		if (this.focusedSelectionDepth != _selectionDepth)
		{
			this.focusedSelectionDepth = _selectionDepth;
			this.IsDirty = true;
		}
	}

	public void SetDeleteHovered(bool _hovered)
	{
		if (this.deleteHovered != _hovered)
		{
			this.deleteHovered = _hovered;
			this.IsDirty = true;
		}
	}

	public void SetDeleteWindowDisplayed(bool _displayed)
	{
		if (this.deleteWindowDisplayed != _displayed)
		{
			this.deleteWindowDisplayed = _displayed;
			this.IsDirty = true;
		}
	}

	public void SetUsedBytes(long _usedBytes)
	{
		if (this.usedBytes == _usedBytes)
		{
			return;
		}
		this.usedBytes = _usedBytes;
		this.IsDirty = true;
	}

	public void SetAllowanceBytes(long _allowanceBytes)
	{
		if (this.allowanceBytes == _allowanceBytes)
		{
			return;
		}
		this.allowanceBytes = _allowanceBytes;
		this.bytesToPixels = (((float)_allowanceBytes > 0f) ? ((float)this.fullWidth / (float)_allowanceBytes) : 0f);
		this.IsDirty = true;
	}

	public void SetPendingBytes(long _pendingBytes)
	{
		if (this.pendingBytes == _pendingBytes)
		{
			return;
		}
		this.pendingBytes = _pendingBytes;
		this.IsDirty = true;
	}

	public long GetPendingBytes()
	{
		return this.pendingBytes;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refresh()
	{
		this.barUsed.IsVisible = false;
		this.barSelectedPrimaryFill.IsVisible = false;
		this.barSelectedSecondaryFill.IsVisible = false;
		this.barSelectedTertiaryFill.IsVisible = false;
		this.barSelectedPrimaryOutline.IsVisible = false;
		this.barSelectedSecondaryOutline.IsVisible = false;
		this.barSelectedTertiaryOutline.IsVisible = false;
		this.barHoveredOutline.IsVisible = false;
		this.barRequired.IsVisible = false;
		this.barPending.IsVisible = false;
		XUiC_DataManagementBar.DisplayMode displayMode = this.displayMode;
		if (displayMode != XUiC_DataManagementBar.DisplayMode.Selection && displayMode == XUiC_DataManagementBar.DisplayMode.Preview)
		{
			this.refreshPreviewMode();
		}
		else
		{
			this.refreshSelectionMode();
		}
		base.RefreshBindings();
		this.IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshPreviewMode()
	{
		if (this.allowanceBytes <= 0L || (this.usedBytes <= 0L && this.pendingBytes <= 0L))
		{
			return;
		}
		int num = Mathf.CeilToInt(this.bytesToPixels * (float)this.usedBytes);
		int num2 = 0;
		int num3 = 0;
		int num6;
		if (this.pendingBytes > 0L)
		{
			long num4 = this.allowanceBytes - this.usedBytes;
			if (num4 < this.pendingBytes)
			{
				num3 = this.fullWidth - num;
				long num5 = this.pendingBytes - num4;
				num2 = Math.Clamp(Mathf.CeilToInt(this.bytesToPixels * (float)num5), 3, num);
				num6 = num - num2;
			}
			else
			{
				num3 = Math.Clamp(Mathf.CeilToInt(this.bytesToPixels * (float)this.pendingBytes), 3, this.fullWidth - 3);
				num6 = Math.Min(num, this.fullWidth - num3);
			}
		}
		else
		{
			num6 = num;
		}
		if (num6 > 0)
		{
			this.barUsed.IsVisible = true;
			this.barUsed.Size = new Vector2i(num6, this.fullHeight);
		}
		if (num2 > 0)
		{
			this.barRequired.IsVisible = true;
			this.barRequired.Position = new Vector2i(num6, 0);
			this.barRequired.Size = new Vector2i(num2, this.fullHeight);
		}
		if (num3 > 0)
		{
			this.barPending.IsVisible = true;
			this.barPending.Position = new Vector2i(num6 + num2, 0);
			this.barPending.Size = new Vector2i(num3, this.fullHeight);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshSelectionMode()
	{
		XUiC_DataManagementBar.<>c__DisplayClass55_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		if (this.allowanceBytes <= 0L || this.usedBytes <= 0L)
		{
			return;
		}
		int num = Mathf.CeilToInt(this.bytesToPixels * (float)this.usedBytes);
		this.barUsed.IsVisible = true;
		this.barUsed.Size = new Vector2i(Math.Max(num, 8), this.fullHeight);
		CS$<>8__locals1.maxPosition = this.fullWidth;
		this.<refreshSelectionMode>g__UpdateRegion|55_1(this.primaryByteRegion, this.barSelectedPrimaryFill, this.barSelectedPrimaryOutline, XUiC_DataManagementBar.SelectionDepth.Primary, ref CS$<>8__locals1);
		this.<refreshSelectionMode>g__UpdateRegion|55_1(this.secondaryByteRegion, this.barSelectedSecondaryFill, this.barSelectedSecondaryOutline, XUiC_DataManagementBar.SelectionDepth.Secondary, ref CS$<>8__locals1);
		this.<refreshSelectionMode>g__UpdateRegion|55_1(this.tertiaryByteRegion, this.barSelectedTertiaryFill, this.barSelectedTertiaryOutline, XUiC_DataManagementBar.SelectionDepth.Tertiary, ref CS$<>8__locals1);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <refreshSelectionMode>g__GetPixelValues|55_0(XUiC_DataManagementBar.BarRegion _byteRegion, int _maxPosition, out int _position, out int _width, ref XUiC_DataManagementBar.<>c__DisplayClass55_0 A_5)
	{
		_position = Mathf.Min(Mathf.FloorToInt(this.bytesToPixels * (float)_byteRegion.Start), _maxPosition - 8);
		_width = Mathf.Max(Mathf.CeilToInt(this.bytesToPixels * (float)_byteRegion.Size), 8);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <refreshSelectionMode>g__UpdateRegion|55_1(XUiC_DataManagementBar.BarRegion _byteRegion, XUiV_Sprite _fillSprite, XUiV_Sprite _outlineSprite, XUiC_DataManagementBar.SelectionDepth _depth, ref XUiC_DataManagementBar.<>c__DisplayClass55_0 A_5)
	{
		if (this.focusedSelectionDepth == _depth && this.hoveredByteRegion.Size > 0L)
		{
			int num;
			int num2;
			this.<refreshSelectionMode>g__GetPixelValues|55_0(this.hoveredByteRegion, A_5.maxPosition, out num, out num2, ref A_5);
			this.barHoveredOutline.IsVisible = true;
			this.barHoveredOutline.Position = new Vector2i(num, 0);
			this.barHoveredOutline.Size = new Vector2i(num2, this.fullHeight);
		}
		if (_byteRegion.Size <= 0L)
		{
			return;
		}
		int num3;
		int num4;
		this.<refreshSelectionMode>g__GetPixelValues|55_0(_byteRegion, A_5.maxPosition, out num3, out num4, ref A_5);
		_fillSprite.IsVisible = true;
		_fillSprite.Position = new Vector2i(num3, 0);
		_fillSprite.Size = new Vector2i(num4, this.fullHeight);
		float num5 = ((this.focusedSelectionDepth == XUiC_DataManagementBar.SelectionDepth.Secondary) ? 1f : 0.5f) * (float)(this.focusedSelectionDepth - _depth);
		Color color = this.selectionFillColor;
		color = Color.Lerp(color, Color.white, 0.5f * Mathf.Abs(num5));
		bool flag = this.deleteHovered || this.deleteWindowDisplayed;
		if (num5 > 0f)
		{
			color.a = Mathf.Lerp(color.a, 0.5f, num5);
		}
		else if (flag)
		{
			color = Color.Lerp(color, XUiC_DataManagementBar.selectionOutlineColorDelete, 0.2f);
		}
		_fillSprite.Color = color;
		if (!flag || num5 >= 0f)
		{
			_outlineSprite.IsVisible = true;
			_outlineSprite.Position = _fillSprite.Position;
			_outlineSprite.Size = _fillSprite.Size;
			Color color2;
			if (this.focusedSelectionDepth == _depth)
			{
				color2 = (flag ? XUiC_DataManagementBar.selectionOutlineColorDelete : XUiC_DataManagementBar.selectionOutlineColor);
			}
			else
			{
				color2 = XUiC_DataManagementBar.selectionOutlineColorFade;
			}
			_outlineSprite.Color = color2;
		}
		if (this.focusedSelectionDepth == _depth && this.archivePreviewByteRegion.Size > 0L)
		{
			int num6;
			int num7;
			this.<refreshSelectionMode>g__GetPixelValues|55_0(this.archivePreviewByteRegion, A_5.maxPosition, out num6, out num7, ref A_5);
			this.barPending.IsVisible = true;
			this.barPending.Position = new Vector2i(num6, 0);
			this.barPending.Size = new Vector2i(num7, this.fullHeight);
		}
		A_5.maxPosition = num3 + num4;
	}

	[XuiBindComponent("background", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite background;

	[XuiBindComponent("bar_used", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barUsed;

	[XuiBindComponent("bar_required", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barRequired;

	[XuiBindComponent("bar_pending", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barPending;

	[XuiBindComponent("bar_selected_primary_fill", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedPrimaryFill;

	[XuiBindComponent("bar_selected_secondary_fill", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedSecondaryFill;

	[XuiBindComponent("bar_selected_tertiary_fill", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedTertiaryFill;

	[XuiBindComponent("bar_selected_primary_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedPrimaryOutline;

	[XuiBindComponent("bar_selected_secondary_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedSecondaryOutline;

	[XuiBindComponent("bar_selected_tertiary_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barSelectedTertiaryOutline;

	[XuiBindComponent("bar_hovered_outline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite barHoveredOutline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.DisplayMode displayMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public long usedBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long allowanceBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion primaryByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion secondaryByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion tertiaryByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion hoveredByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.BarRegion archivePreviewByteRegion = XUiC_DataManagementBar.BarRegion.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar.SelectionDepth focusedSelectionDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deleteHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool deleteWindowDisplayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bytesToPixels;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColor = new Color32(250, byte.MaxValue, 163, 193);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColorFade = new Color32(250, byte.MaxValue, 163, 86);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColorDelete = new Color32(234, 67, 53, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectionFillColor;

	public enum SelectionDepth
	{
		Primary,
		Secondary,
		Tertiary
	}

	public readonly struct BarRegion : IEquatable<XUiC_DataManagementBar.BarRegion>
	{
		public BarRegion(long _offset, long _size)
		{
			this.Start = _offset;
			this.Size = _size;
			this.End = this.Start + this.Size;
		}

		public bool Equals(XUiC_DataManagementBar.BarRegion _other)
		{
			return this.Start == _other.Start && this.Size == _other.Size;
		}

		public readonly long Start;

		public readonly long Size;

		public readonly long End;

		public static readonly XUiC_DataManagementBar.BarRegion None = new XUiC_DataManagementBar.BarRegion(0L, 0L);
	}

	public enum DisplayMode
	{
		Selection,
		Preview
	}
}
