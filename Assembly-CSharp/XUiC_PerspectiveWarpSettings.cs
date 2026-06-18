using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PerspectiveWarpSettings : XUiC_SignWarpSettings
{
	public override SignData.SignWarp CurrentWarp
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.currentWarp;
		}
	}

	public override void Init()
	{
		base.Init();
		this.cbxRotationX = (XUiC_ComboBoxFloat)base.GetChildById("cbxRotationX").GetChildById("value");
		this.cbxRotationX.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Perspective Rotation X");
		};
		this.cbxRotationY = (XUiC_ComboBoxFloat)base.GetChildById("cbxRotationY").GetChildById("value");
		this.cbxRotationY.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Perspective Rotation Y");
		};
		this.cbxRotationZ = (XUiC_ComboBoxFloat)base.GetChildById("cbxRotationZ").GetChildById("value");
		this.cbxRotationZ.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Perspective Rotation Z");
		};
		this.cbxStrength = (XUiC_ComboBoxFloat)base.GetChildById("cbxStrength").GetChildById("value");
		this.cbxStrength.OnValueChangedGeneric += delegate(XUiController _)
		{
			this.OnValueChangedGeneric("Changed Perspective Strength");
		};
		SignData.PerspectiveWarp perspectiveWarp = new SignData.PerspectiveWarp();
		base.SetDefaultValue("cbxRotationX", perspectiveWarp.rotation.x);
		base.SetDefaultValue("cbxRotationY", perspectiveWarp.rotation.y);
		base.SetDefaultValue("cbxRotationZ", perspectiveWarp.rotation.z);
		base.SetDefaultValue("cbxStrength", perspectiveWarp.strength);
	}

	public override void SetWarp(SignData.SignWarp warp)
	{
		this.currentWarp = warp as SignData.PerspectiveWarp;
		if (this.currentWarp == null)
		{
			this.cbxRotationX.Value = 0.0;
			this.cbxRotationY.Value = 0.0;
			this.cbxRotationZ.Value = 0.0;
			this.cbxStrength.Value = 0.0;
			return;
		}
		this.cbxRotationX.Value = (double)this.currentWarp.rotation.x;
		this.cbxRotationY.Value = (double)this.currentWarp.rotation.y;
		this.cbxRotationZ.Value = (double)this.currentWarp.rotation.z;
		this.cbxStrength.Value = (double)this.currentWarp.strength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnValueChangedGeneric(string _changeDescription)
	{
		if (this.currentWarp == null)
		{
			return;
		}
		Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
		if (onPreLayerSettingsChanged != null)
		{
			onPreLayerSettingsChanged(_changeDescription, false);
		}
		this.currentWarp.rotation = new Vector3((float)this.cbxRotationX.Value, (float)this.cbxRotationY.Value, (float)this.cbxRotationZ.Value);
		this.currentWarp.strength = (float)this.cbxStrength.Value;
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged == null)
		{
			return;
		}
		onLayerSettingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.PerspectiveWarp currentWarp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotationX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotationY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRotationZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxStrength;
}
