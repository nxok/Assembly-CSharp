using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabThemeTagList : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return this.EditPrefab.ThemeTags.Test_AllSet(FastTags<TagGroup.Poi>.GetTag(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		this.EditPrefab.ThemeTags |= FastTags<TagGroup.Poi>.GetTag(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		FastTags<TagGroup.Poi> tag = FastTags<TagGroup.Poi>.GetTag(_featureName);
		if (this.EditPrefab.ThemeTags.Test_AnySet(tag))
		{
			this.EditPrefab.ThemeTags = this.EditPrefab.ThemeTags.Remove(tag);
		}
		else
		{
			this.EditPrefab.ThemeTags |= tag;
		}
		this.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllThemeTags(this.groupsResult, this.EditPrefab);
	}
}
