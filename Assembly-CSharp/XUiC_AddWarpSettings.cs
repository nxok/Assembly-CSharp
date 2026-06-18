using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AddWarpSettings : XUiC_SignLayerSettings
{
	public override void Init()
	{
		base.Init();
		this.warpTypeSkew = base.GetChildById("warpTypeSkew").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType = this.warpTypeSkew;
		xuiC_SignLayerType.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.SkewWarp>(new SignData.SkewWarp());
		}));
		this.warpTypeBulge = base.GetChildById("warpTypeBulge").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType2 = this.warpTypeBulge;
		xuiC_SignLayerType2.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType2.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.BulgeWarp>(new SignData.BulgeWarp());
		}));
		this.warpTypeTwirl = base.GetChildById("warpTypeTwirl").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType3 = this.warpTypeTwirl;
		xuiC_SignLayerType3.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType3.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.TwirlWarp>(new SignData.TwirlWarp());
		}));
		this.warpTypeKaleido = base.GetChildById("warpTypeKaleido").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType4 = this.warpTypeKaleido;
		xuiC_SignLayerType4.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType4.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.KaleidoWarp>(new SignData.KaleidoWarp());
		}));
		this.warpTypePerspective = base.GetChildById("warpTypePerspective").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType5 = this.warpTypePerspective;
		xuiC_SignLayerType5.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType5.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.PerspectiveWarp>(new SignData.PerspectiveWarp());
		}));
		this.warpTypeArc = base.GetChildById("warpTypeArc").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType6 = this.warpTypeArc;
		xuiC_SignLayerType6.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType6.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.ArcWarp>(new SignData.ArcWarp());
		}));
		this.warpTypeStretch = base.GetChildById("warpTypeStretch").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType7 = this.warpTypeStretch;
		xuiC_SignLayerType7.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType7.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.StretchWarp>(new SignData.StretchWarp());
		}));
		this.warpTypeGrid = base.GetChildById("warpTypeGrid").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType8 = this.warpTypeGrid;
		xuiC_SignLayerType8.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType8.OnBecameSelected, new Action<XUiC_SignGridEntry>(delegate(XUiC_SignGridEntry _)
		{
			this.AddWarp<SignData.GridWarp>(new SignData.GridWarp());
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddWarp<T>(T warp) where T : SignData.SignWarp
	{
		if (this.currentLayer == null)
		{
			Log.Error("Failed to add warp: layer reference is null.");
			return;
		}
		if (this.currentLayer.warps == null)
		{
			Log.Error("Failed to add warp: layer warps list is null.");
			return;
		}
		string name = typeof(T).Name;
		Action<string, bool> onPreLayerSettingsChanged = this.OnPreLayerSettingsChanged;
		if (onPreLayerSettingsChanged != null)
		{
			onPreLayerSettingsChanged("Add " + name, true);
		}
		this.currentLayer.warps.Add(warp);
		Action onLayerSettingsChanged = this.OnLayerSettingsChanged;
		if (onLayerSettingsChanged != null)
		{
			onLayerSettingsChanged();
		}
		Action<string> onWarpAdded = this.OnWarpAdded;
		if (onWarpAdded == null)
		{
			return;
		}
		onWarpAdded(name);
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		this.currentLayer = layer;
	}

	public Action<string> OnWarpAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer currentLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeSkew;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeBulge;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeTwirl;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeKaleido;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypePerspective;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeArc;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeStretch;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeGrid;
}
