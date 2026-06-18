using System;

public class TagGroup
{
	public abstract class TagsGroupAbs
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public TagsGroupAbs()
		{
		}
	}

	public class Global : TagGroup.TagsGroupAbs
	{
	}

	public class Poi : TagGroup.TagsGroupAbs
	{
	}
}
