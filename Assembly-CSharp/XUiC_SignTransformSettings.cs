using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignTransformSettings : XUiC_SignLayerSettings
{
	public override void Init()
	{
		base.Init();
		this.txtPositionA = (XUiC_TextInput)base.GetChildById("txtPosition").GetChildById("valueA");
		this.txtPositionA.OnChangeHandler += this.OnChangeHandler;
		this.txtPositionB = (XUiC_TextInput)base.GetChildById("txtPosition").GetChildById("valueB");
		this.txtPositionB.OnChangeHandler += this.OnChangeHandler;
		this.txtRotation = (XUiC_TextInput)base.GetChildById("txtRotation").GetChildById("value");
		this.txtRotation.OnChangeHandler += this.OnChangeHandler;
		this.txtScaleA = (XUiC_TextInput)base.GetChildById("txtScale").GetChildById("valueA");
		this.txtScaleA.OnChangeHandler += this.OnChangeHandler;
		this.txtScaleB = (XUiC_TextInput)base.GetChildById("txtScale").GetChildById("valueB");
		this.txtScaleB.OnChangeHandler += this.OnChangeHandler;
		base.SetDefaultValue("txtPosition", new ValueTuple<float, float>(SignData.SignTransform.Defaults.position.x, SignData.SignTransform.Defaults.position.y));
		base.SetDefaultValue("txtRotation", SignData.SignTransform.Defaults.rotation);
		base.SetDefaultValue("txtScale", new ValueTuple<float, float>(SignData.SignTransform.Defaults.scale.x, SignData.SignTransform.Defaults.scale.y));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (this.currentLayer == null)
		{
			return;
		}
		if (_changeFromCode)
		{
			return;
		}
		float num;
		if (!float.TryParse(_text, out num))
		{
			Log.Error("Failed to parse value \"" + _text + "\" for transform adjustment.");
			return;
		}
		if (_sender == this.txtPositionA)
		{
			Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
			if (onPreLayerSettingsChanged != null)
			{
				onPreLayerSettingsChanged("Changed Transform Position", false);
			}
			this.currentLayer.transform.position.x = num;
		}
		else if (_sender == this.txtPositionB)
		{
			Action<string, bool> onPreLayerSettingsChanged2 = this.OnPreLayerSettingsChanged;
			if (onPreLayerSettingsChanged2 != null)
			{
				onPreLayerSettingsChanged2("Changed Transform Position", false);
			}
			this.currentLayer.transform.position.y = num;
		}
		else if (_sender == this.txtRotation)
		{
			Action<string, bool> onPreLayerSettingsChanged3 = this.OnPreLayerSettingsChanged;
			if (onPreLayerSettingsChanged3 != null)
			{
				onPreLayerSettingsChanged3("Changed Transform Rotation", false);
			}
			this.currentLayer.transform.rotation = num;
		}
		else if (_sender == this.txtScaleA)
		{
			Action<string, bool> onPreLayerSettingsChanged4 = this.OnPreLayerSettingsChanged;
			if (onPreLayerSettingsChanged4 != null)
			{
				onPreLayerSettingsChanged4("Changed Transform Scale", false);
			}
			this.currentLayer.transform.scale.x = num;
		}
		else if (_sender == this.txtScaleB)
		{
			Action<string, bool> onPreLayerSettingsChanged5 = this.OnPreLayerSettingsChanged;
			if (onPreLayerSettingsChanged5 != null)
			{
				onPreLayerSettingsChanged5("Changed Transform Scale", false);
			}
			this.currentLayer.transform.scale.y = num;
		}
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		this.currentLayer = layer;
		if (this.currentLayer == null)
		{
			this.txtPositionA.Text = string.Empty;
			this.txtPositionB.Text = string.Empty;
			this.txtRotation.Text = string.Empty;
			this.txtScaleA.Text = string.Empty;
			this.txtScaleB.Text = string.Empty;
			return;
		}
		this.txtPositionA.Text = layer.transform.position.x.ToString("F4");
		this.txtPositionB.Text = layer.transform.position.y.ToString("F4");
		this.txtRotation.Text = layer.transform.rotation.ToString("F4");
		this.txtScaleA.Text = layer.transform.scale.x.ToString("F4");
		this.txtScaleB.Text = layer.transform.scale.y.ToString("F4");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPositionA;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPositionB;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtScaleA;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtScaleB;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer currentLayer;
}
