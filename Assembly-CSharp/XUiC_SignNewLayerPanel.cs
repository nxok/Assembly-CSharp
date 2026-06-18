using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignNewLayerPanel : XUiController
{
	public override void Init()
	{
		base.Init();
		this.layerTypeText = base.GetChildById("layerTypeText").GetChildByType<XUiC_SignLayerType>();
		this.layerTypePolygon = base.GetChildById("layerTypePolygon").GetChildByType<XUiC_SignLayerType>();
		this.layerTypeNoise = base.GetChildById("layerTypeNoise").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xuiC_SignLayerType = this.layerTypeText;
		xuiC_SignLayerType.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType.OnBecameSelected, new Action<XUiC_SignGridEntry>(this.LayerTypeText_OnBecameSelected));
		XUiC_SignLayerType xuiC_SignLayerType2 = this.layerTypePolygon;
		xuiC_SignLayerType2.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType2.OnBecameSelected, new Action<XUiC_SignGridEntry>(this.LayerTypePolygon_OnBecameSelected));
		XUiC_SignLayerType xuiC_SignLayerType3 = this.layerTypeNoise;
		xuiC_SignLayerType3.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xuiC_SignLayerType3.OnBecameSelected, new Action<XUiC_SignGridEntry>(this.LayerTypeNoise_OnBecameSelected));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LayerTypeText_OnBecameSelected(XUiC_SignGridEntry gridEntry)
	{
		SignData.SignLayer signLayer = new SignData.TextSignLayer();
		Action<SignData.SignLayer> onLayerTypeSelected = this.OnLayerTypeSelected;
		if (onLayerTypeSelected == null)
		{
			return;
		}
		onLayerTypeSelected(signLayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LayerTypePolygon_OnBecameSelected(XUiC_SignGridEntry gridEntry)
	{
		SignData.SignLayer signLayer = new SignData.PolygonSignLayer();
		Action<SignData.SignLayer> onLayerTypeSelected = this.OnLayerTypeSelected;
		if (onLayerTypeSelected == null)
		{
			return;
		}
		onLayerTypeSelected(signLayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LayerTypeNoise_OnBecameSelected(XUiC_SignGridEntry gridEntry)
	{
		SignData.SignLayer signLayer = new SignData.NoiseSignLayer();
		Action<SignData.SignLayer> onLayerTypeSelected = this.OnLayerTypeSelected;
		if (onLayerTypeSelected == null)
		{
			return;
		}
		onLayerTypeSelected(signLayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType layerTypeText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType layerTypePolygon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType layerTypeNoise;

	public Action<SignData.SignLayer> OnLayerTypeSelected;
}
