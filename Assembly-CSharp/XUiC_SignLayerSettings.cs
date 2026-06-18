using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_SignLayerSettings : XUiController
{
	public abstract void SetLayer(SignData.SignLayer layer);

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetDefaultValue(string id, object value)
	{
		XUiC_SignEditorControl xuiC_SignEditorControl = base.GetChildById(id) as XUiC_SignEditorControl;
		if (xuiC_SignEditorControl != null)
		{
			xuiC_SignEditorControl.defaultValue = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SignLayerSettings()
	{
	}

	public Action<string, bool> OnPreLayerSettingsChanged;

	public Action OnLayerSettingsChanged;
}
