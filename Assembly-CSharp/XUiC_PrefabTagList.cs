using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabTagList : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return this.EditPrefab.Tags.Test_AllSet(FastTags<TagGroup.Poi>.GetTag(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		this.EditPrefab.Tags |= FastTags<TagGroup.Poi>.GetTag(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		FastTags<TagGroup.Poi> tag = FastTags<TagGroup.Poi>.GetTag(_featureName);
		if (this.EditPrefab.Tags.Test_AnySet(tag))
		{
			this.EditPrefab.Tags = this.EditPrefab.Tags.Remove(tag);
		}
		else
		{
			this.EditPrefab.Tags |= tag;
		}
		this.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllTags(this.groupsResult, this.EditPrefab);
	}
}
