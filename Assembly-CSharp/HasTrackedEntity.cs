using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class HasTrackedEntity : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		EntityPlayerLocal entityPlayerLocal = this.target as EntityPlayerLocal;
		if (entityPlayerLocal == null)
		{
			return false;
		}
		bool flag = false;
		float value = EffectManager.GetValue(PassiveEffects.TrackDistance, null, 0f, entityPlayerLocal, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (value >= 0f)
		{
			List<Entity> entitiesInBounds = entityPlayerLocal.world.GetEntitiesInBounds(entityPlayerLocal, new Bounds(entityPlayerLocal.position, Vector3.one * (2f * value)));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				if (this.hasAllTags)
				{
					if (entitiesInBounds[i].HasAllTags(this.trackerTags))
					{
						flag = true;
						break;
					}
				}
				else if (entitiesInBounds[i].HasAnyTags(this.trackerTags))
				{
					flag = true;
					break;
				}
			}
		}
		return flag == !this.invert;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} Tracking Entity", this.invert ? "NOT " : ""));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				this.trackerTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
				return true;
			}
			if (localName == "has_all_tags")
			{
				this.hasAllTags = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> trackerTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;
}
