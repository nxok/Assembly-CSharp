using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_SignWarpSettings : XUiC_SignLayerSettings
{
	public abstract SignData.SignWarp CurrentWarp
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public override void Init()
	{
		base.Init();
		this.btnRemove = (XUiC_SimpleButton)base.GetChildById("btnRemove");
		this.btnRemove.OnPressed += this.BtnRemove_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRemove_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.currentLayer == null)
		{
			Log.Error("Failed to remove warp. Layer reference is null.");
			return;
		}
		if (this.CurrentWarp == null)
		{
			Log.Error("Failed to remove warp. Warp reference is null.");
			return;
		}
		string name = this.CurrentWarp.GetType().Name;
		if (!this.currentLayer.warps.Contains(this.CurrentWarp))
		{
			Log.Error("Failed to remove " + name + ". Layer warp list does not contain referenced warp.");
			return;
		}
		Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
		if (onPreLayerSettingsChanged != null)
		{
			onPreLayerSettingsChanged("Remove " + name, true);
		}
		this.currentLayer.warps.Remove(this.CurrentWarp);
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged != null)
		{
			onLayerSettingsChanged();
		}
		Action onWarpRemoved = this.OnWarpRemoved;
		if (onWarpRemoved == null)
		{
			return;
		}
		onWarpRemoved();
	}

	public abstract void SetWarp(SignData.SignWarp warp);

	public override void SetLayer(SignData.SignLayer layer)
	{
		this.currentLayer = layer;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SignWarpSettings()
	{
	}

	public Action OnWarpRemoved;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SignData.SignLayer currentLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SimpleButton btnRemove;
}
