using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SizeBar : XUiController
{
	public override void Init()
	{
		base.Init();
		this.border = (XUiV_Sprite)base.GetChildById("border").ViewComponent;
		this.background = (XUiV_Sprite)base.GetChildById("background").ViewComponent;
		this.bar_used = (XUiV_Sprite)base.GetChildById("bar_used").ViewComponent;
		this.bar_excess = (XUiV_Sprite)base.GetChildById("bar_excess").ViewComponent;
		this.bar_selected_fill = (XUiV_Sprite)base.GetChildById("bar_selected_fill").ViewComponent;
		this.bar_selected_outline = (XUiV_Sprite)base.GetChildById("bar_selected_outline").ViewComponent;
		this.bar_hovered_outline = (XUiV_Sprite)base.GetChildById("bar_hovered_outline").ViewComponent;
		this.normalBorderColor = this.border.Color;
		this.selectionFillColor = this.bar_selected_fill.Color;
		this.fullWidth = this.background.Size.x;
		this.fullHeight = this.background.Size.y;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	public void SetSelectedRegion(XUiC_SizeBar.BarRegionFloat primaryRegion)
	{
		if (!this.selectedRegion.Equals(primaryRegion))
		{
			this.selectedRegion = primaryRegion;
			this.IsDirty = true;
		}
	}

	public void SetHoveredRegion(XUiC_SizeBar.BarRegionFloat region)
	{
		if (!this.hoveredRegion.Equals(region))
		{
			this.hoveredRegion = region;
			this.IsDirty = true;
		}
	}

	public void SetUsed(float used)
	{
		if (this.used == used)
		{
			return;
		}
		this.used = used;
		this.IsDirty = true;
	}

	public void SetAllowance(float allowance)
	{
		if (this.allowance == allowance)
		{
			return;
		}
		this.allowance = allowance;
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Refresh()
	{
		this.bar_used.IsVisible = false;
		this.bar_excess.IsVisible = false;
		this.bar_selected_fill.IsVisible = false;
		this.bar_selected_outline.IsVisible = false;
		this.bar_hovered_outline.IsVisible = false;
		this.RefreshSelectionMode();
		base.RefreshBindings();
		this.IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshSelectionMode()
	{
		XUiC_SizeBar.<>c__DisplayClass27_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		if (this.allowance <= 0f || this.used <= 0f)
		{
			return;
		}
		float num = Mathf.Max(this.allowance, this.used);
		CS$<>8__locals1.pixelScaleFactor = ((num > 0f) ? ((float)this.fullWidth / num) : 0f);
		int num2 = Mathf.CeilToInt(CS$<>8__locals1.pixelScaleFactor * this.used);
		this.bar_used.IsVisible = true;
		this.bar_used.Size = new Vector2i(Math.Max(num2, this.minBarWidth), this.fullHeight);
		CS$<>8__locals1.maxPosition = this.fullWidth;
		if (this.used > this.allowance)
		{
			this.bar_excess.IsVisible = true;
			int num3 = Mathf.Min(Mathf.CeilToInt(CS$<>8__locals1.pixelScaleFactor * this.allowance), CS$<>8__locals1.maxPosition - this.minBarWidth);
			int num4 = Math.Max(Mathf.FloorToInt(CS$<>8__locals1.pixelScaleFactor * (this.used - this.allowance)), this.minBarWidth);
			this.bar_excess.Position = new Vector2i(num3, 0);
			this.bar_excess.Size = new Vector2i(num4, this.fullHeight);
		}
		this.<RefreshSelectionMode>g__UpdateRegion|27_1(this.selectedRegion, this.bar_selected_fill, this.bar_selected_outline, ref CS$<>8__locals1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "showwarningoverlay")
		{
			_value = (this.used > this.allowance).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <RefreshSelectionMode>g__GetPixelValues|27_0(XUiC_SizeBar.BarRegionFloat region, int maxPosition, out int position, out int width, ref XUiC_SizeBar.<>c__DisplayClass27_0 A_5)
	{
		position = Mathf.Min(Mathf.FloorToInt(A_5.pixelScaleFactor * region.Start), maxPosition - this.minBarWidth);
		width = Mathf.Max(Mathf.CeilToInt(A_5.pixelScaleFactor * region.Size), this.minBarWidth);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <RefreshSelectionMode>g__UpdateRegion|27_1(XUiC_SizeBar.BarRegionFloat region, XUiV_Sprite fillSprite, XUiV_Sprite outlineSprite, ref XUiC_SizeBar.<>c__DisplayClass27_0 A_4)
	{
		if (this.hoveredRegion.Size > 0f)
		{
			int num;
			int num2;
			this.<RefreshSelectionMode>g__GetPixelValues|27_0(this.hoveredRegion, A_4.maxPosition, out num, out num2, ref A_4);
			this.bar_hovered_outline.IsVisible = true;
			this.bar_hovered_outline.Position = new Vector2i(num - 1, 0);
			this.bar_hovered_outline.Size = new Vector2i(num2 + 2, this.fullHeight);
		}
		if (region.Size <= 0f)
		{
			return;
		}
		int num3;
		int num4;
		this.<RefreshSelectionMode>g__GetPixelValues|27_0(region, A_4.maxPosition, out num3, out num4, ref A_4);
		fillSprite.IsVisible = true;
		fillSprite.Position = new Vector2i(num3, 0);
		fillSprite.Size = new Vector2i(num4, this.fullHeight);
		fillSprite.Color = this.selectionFillColor;
		outlineSprite.IsVisible = true;
		outlineSprite.Position = fillSprite.Position;
		outlineSprite.Size = fillSprite.Size;
		outlineSprite.Color = XUiC_SizeBar.selectionOutlineColor;
		A_4.maxPosition = num3 + num4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int minBarWidth = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_used;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_excess;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_fill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_selected_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bar_hovered_outline;

	[PublicizedFrom(EAccessModifier.Private)]
	public float used;

	[PublicizedFrom(EAccessModifier.Private)]
	public float allowance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SizeBar.BarRegionFloat selectedRegion = XUiC_SizeBar.BarRegionFloat.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SizeBar.BarRegionFloat hoveredRegion = XUiC_SizeBar.BarRegionFloat.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fullHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color32 selectionOutlineColor = new Color32(250, byte.MaxValue, 163, 193);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectionFillColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 normalBorderColor;

	public struct BarRegionFloat : IEquatable<XUiC_SizeBar.BarRegionFloat>
	{
		public BarRegionFloat(float offset, float size)
		{
			this.Start = offset;
			this.Size = size;
			this.End = this.Start + this.Size;
		}

		public bool Equals(XUiC_SizeBar.BarRegionFloat other)
		{
			return this.Start == other.Start && this.Size == other.Size;
		}

		public readonly float Start;

		public readonly float Size;

		public readonly float End;

		public static readonly XUiC_SizeBar.BarRegionFloat None = new XUiC_SizeBar.BarRegionFloat(0f, 0f);
	}
}
