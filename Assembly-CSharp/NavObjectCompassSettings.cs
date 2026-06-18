using System;
using System.Globalization;
using UnityEngine;

public class NavObjectCompassSettings : NavObjectSettings
{
	public bool ShowVerticalCompassIcons
	{
		get
		{
			return this.UpSpriteName != "" || this.DownSpriteName != "";
		}
	}

	public override void Init()
	{
		base.Init();
		this.Properties.ParseBool("icon_clamped", ref this.IconClamped);
		this.Properties.ParseFloat("min_icon_scale", ref this.MinCompassIconScale);
		this.Properties.ParseFloat("max_icon_scale", ref this.MaxCompassIconScale);
		this.Properties.ParseFloat("min_fade_percent", ref this.MinFadePercent);
		this.MaxScaleDistance = this.MaxDistance;
		this.Properties.ParseFloat("max_scale_distance", ref this.MaxScaleDistance);
		this.Properties.ParseString("up_sprite_name", ref this.UpSpriteName);
		this.Properties.ParseString("down_sprite_name", ref this.DownSpriteName);
		this.Properties.ParseFloat("show_up_offset", ref this.ShowUpOffset);
		this.Properties.ParseFloat("show_down_offset", ref this.ShowDownOffset);
		if (this.Properties.Values.ContainsKey("hot_zone_type"))
		{
			NavObjectCompassSettings.HotZoneSettings.HotZoneTypes hotZoneTypes = NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.None;
			if (!Enum.TryParse<NavObjectCompassSettings.HotZoneSettings.HotZoneTypes>(this.Properties.Values["hot_zone_type"], out hotZoneTypes))
			{
				hotZoneTypes = NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.None;
			}
			if (hotZoneTypes != NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.None)
			{
				this.HotZone = new NavObjectCompassSettings.HotZoneSettings();
				this.HotZone.HotZoneType = hotZoneTypes;
				this.Properties.ParseString("hot_zone_sprite", ref this.HotZone.SpriteName);
				if (this.Properties.Values.ContainsKey("hot_zone_sprite"))
				{
					this.HotZone.SpriteName = this.Properties.Values["hot_zone_sprite"];
				}
				if (this.Properties.Values.ContainsKey("hot_zone_color"))
				{
					this.HotZone.Color = StringParsers.ParseColor32(this.Properties.Values["hot_zone_color"]);
				}
				if (this.Properties.Values.ContainsKey("hot_zone_distance"))
				{
					this.HotZone.CustomDistance = StringParsers.ParseFloat(this.Properties.Values["hot_zone_distance"], 0, -1, NumberStyles.Any);
				}
			}
		}
		if (this.Properties.Values.ContainsKey("depth_offset"))
		{
			this.DepthOffset = StringParsers.ParseSInt32(this.Properties.Values["depth_offset"], 0, -1, NumberStyles.Integer);
		}
	}

	public bool IconClamped;

	public float MinCompassIconScale = 0.5f;

	public float MaxCompassIconScale = 1.25f;

	public float MaxScaleDistance = -1f;

	public float MinFadePercent = -1f;

	public string UpSpriteName = "";

	public string DownSpriteName = "";

	public float ShowUpOffset = 3f;

	public float ShowDownOffset = -2f;

	public int DepthOffset;

	public NavObjectCompassSettings.HotZoneSettings HotZone;

	public class HotZoneSettings
	{
		public NavObjectCompassSettings.HotZoneSettings.HotZoneTypes HotZoneType;

		public string SpriteName = "";

		public Color Color = Color.white;

		public float CustomDistance = -1f;

		public enum HotZoneTypes
		{
			None,
			Treasure,
			Custom
		}
	}
}
