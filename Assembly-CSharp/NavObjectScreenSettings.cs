using System;
using UnityEngine;

public class NavObjectScreenSettings : NavObjectSettings
{
	public override void Init()
	{
		base.Init();
		this.Properties.ParseEnum<NavObjectScreenSettings.ShowTextTypes>("text_type", ref this.ShowTextType);
		this.Properties.ParseFloat("fade_percent", ref this.FadePercent);
		this.Properties.ParseBool("show_offscreen", ref this.ShowOffScreen);
		this.Properties.ParseBool("use_head_offset", ref this.UseHeadOffset);
		this.Properties.ParseFloat("sprite_size", ref this.SpriteSize);
		this.Properties.ParseColor("sprite_fill_color", ref this.SpriteFillColor);
		this.Properties.ParseEnum<NavObjectScreenSettings.SpriteFillTypes>("sprite_fill_type", ref this.SpriteFillType);
		this.Properties.ParseString("sprite_fill_name", ref this.SpriteFillName);
		this.Properties.ParseString("sub_sprite_name", ref this.SubSpriteName);
		this.Properties.ParseVec("sub_sprite_offset", ref this.SubSpriteOffset);
		this.Properties.ParseFloat("sub_sprite_size", ref this.SubSpriteSize);
		this.Properties.ParseInt("font_size", ref this.FontSize);
		this.Properties.ParseColor("font_color", ref this.FontColor);
		this.FadeEndDistance = (this.MaxDistance - this.MinDistance) * this.FadePercent;
	}

	public NavObjectScreenSettings.ShowTextTypes ShowTextType;

	public int FontSize = 24;

	public Color FontColor = Color.white;

	public float SpriteSize = 32f;

	public float FadePercent = 0.9f;

	public float FadeEndDistance;

	public bool ShowOffScreen;

	public bool UseHeadOffset;

	public NavObjectScreenSettings.SpriteFillTypes SpriteFillType;

	public string SpriteFillName;

	public Color SpriteFillColor = Color.white;

	public string SubSpriteName;

	public Vector2 SubSpriteOffset;

	public float SubSpriteSize = 16f;

	public enum ShowTextTypes
	{
		None,
		Distance,
		Name,
		SpawnName
	}

	public enum SpriteFillTypes
	{
		None,
		Health
	}
}
