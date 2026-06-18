using System;

public class DisplayInfoEntry
{
	public FastTags<TagGroup.Global> Tags
	{
		get
		{
			return this.tags;
		}
		set
		{
			this.tags = value;
			this.TagsSet = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.none;

	public bool TagsSet;

	public PassiveEffects StatType;

	public string CustomName = "";

	public string TitleOverride;

	public DisplayInfoEntry.DisplayTypes DisplayType;

	public bool ShowInverted;

	public bool NegativePreferred;

	public bool DisplayLeadingPlus;

	public enum DisplayTypes
	{
		Integer,
		Decimal1,
		Decimal2,
		Bool,
		Percent,
		Time
	}
}
