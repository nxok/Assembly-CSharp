using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementInPOI : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			bool flag = true;
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				if (entityPlayer.prefab != null)
				{
					Prefab prefab = entityPlayer.prefab.prefab;
					if (this.poiTags != "" && !prefab.Tags.Test_AnySet(FastTags<TagGroup.Poi>.Parse(this.poiTags)))
					{
						flag = false;
					}
					if (this.poiName != "" && !this.poiName.ContainsCaseInsensitive(prefab.PrefabName) && !this.poiName.ContainsCaseInsensitive(prefab.LocalizedName))
					{
						flag = false;
					}
					if (this.poiTier != -1 && (int)prefab.DifficultyTier != this.poiTier)
					{
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
			if (!this.Invert)
			{
				return flag;
			}
			return !flag;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseInt(RequirementInPOI.PropPOITier, ref this.poiTier);
			properties.ParseString(RequirementInPOI.PropPOITags, ref this.poiTags);
			properties.ParseString(RequirementInPOI.PropPOINames, ref this.poiName);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementInPOI
			{
				Invert = this.Invert,
				poiTier = this.poiTier,
				poiTags = this.poiTags,
				poiName = this.poiName
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string poiName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string poiTags = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public int poiTier = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPOITier = "tier";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPOITags = "tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPOINames = "name";
	}
}
