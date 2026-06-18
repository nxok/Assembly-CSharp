using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabQuestTags : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return this.EditPrefab.GetQuestTag(FastTags<TagGroup.Global>.Parse(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		this.EditPrefab.ToggleQuestTag(FastTags<TagGroup.Global>.Parse(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		this.EditPrefab.ToggleQuestTag(FastTags<TagGroup.Global>.GetTag(_featureName));
		this.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllQuestTags(this.groupsResult, this.EditPrefab);
	}
}
