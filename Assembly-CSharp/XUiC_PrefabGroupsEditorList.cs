using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabGroupsEditorList : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return this.EditPrefab.editorGroups.Contains(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		this.EditPrefab.editorGroups.Add(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		if (this.EditPrefab.editorGroups.Contains(_featureName))
		{
			this.EditPrefab.editorGroups.Remove(_featureName);
			return;
		}
		this.EditPrefab.editorGroups.Add(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllGroups(this.groupsResult, this.EditPrefab);
	}
}
