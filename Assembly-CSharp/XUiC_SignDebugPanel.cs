using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignDebugPanel : XUiController
{
	public override void Init()
	{
		base.Init();
		this.lblGroupDepth = (XUiV_Label)base.GetChildById("lblGroupDepth").GetChildById("value").ViewComponent;
		this.lblComplexity = (XUiV_Label)base.GetChildById("lblComplexity").GetChildById("value").ViewComponent;
		this.lblDescCount = (XUiV_Label)base.GetChildById("lblDescCount").GetChildById("value").ViewComponent;
		this.lblWarpCount = (XUiV_Label)base.GetChildById("lblWarpCount").GetChildById("value").ViewComponent;
		this.lblDrawCount = (XUiV_Label)base.GetChildById("lblDrawCount").GetChildById("value").ViewComponent;
		this.lblBgnCompDepth = (XUiV_Label)base.GetChildById("lblBgnCompDepth").GetChildById("value").ViewComponent;
		this.lblMaxCompDepth = (XUiV_Label)base.GetChildById("lblMaxCompDepth").GetChildById("value").ViewComponent;
		this.lblEndCompDepth = (XUiV_Label)base.GetChildById("lblEndCompDepth").GetChildById("value").ViewComponent;
		this.lblMaxUVDepth = (XUiV_Label)base.GetChildById("lblMaxUVDepth").GetChildById("value").ViewComponent;
	}

	public void Clear()
	{
		this.SetAll("-");
	}

	public void Populate(in LayerComplexityInfo layerComplexityInfo)
	{
		XUiV_Label xuiV_Label = this.lblGroupDepth;
		int num = layerComplexityInfo.GroupDepth;
		xuiV_Label.Text = num.ToString();
		XUiV_Label xuiV_Label2 = this.lblComplexity;
		float complexity = layerComplexityInfo.Complexity;
		xuiV_Label2.Text = complexity.ToString();
		XUiV_Label xuiV_Label3 = this.lblDescCount;
		num = layerComplexityInfo.DescriptorCount;
		xuiV_Label3.Text = num.ToString();
		XUiV_Label xuiV_Label4 = this.lblWarpCount;
		num = layerComplexityInfo.WarpCount;
		xuiV_Label4.Text = num.ToString();
		XUiV_Label xuiV_Label5 = this.lblDrawCount;
		num = layerComplexityInfo.DrawCount;
		xuiV_Label5.Text = num.ToString();
		XUiV_Label xuiV_Label6 = this.lblBgnCompDepth;
		num = layerComplexityInfo.StartCompStackIndex;
		xuiV_Label6.Text = num.ToString();
		XUiV_Label xuiV_Label7 = this.lblMaxCompDepth;
		num = layerComplexityInfo.MaxCompStackIndex;
		xuiV_Label7.Text = num.ToString();
		XUiV_Label xuiV_Label8 = this.lblEndCompDepth;
		num = layerComplexityInfo.EndCompStackIndex;
		xuiV_Label8.Text = num.ToString();
		XUiV_Label xuiV_Label9 = this.lblMaxUVDepth;
		num = layerComplexityInfo.MaxUVStackIndex;
		xuiV_Label9.Text = num.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAll(string value)
	{
		this.lblGroupDepth.Text = value;
		this.lblComplexity.Text = value;
		this.lblDescCount.Text = value;
		this.lblWarpCount.Text = value;
		this.lblDrawCount.Text = value;
		this.lblBgnCompDepth.Text = value;
		this.lblMaxCompDepth.Text = value;
		this.lblEndCompDepth.Text = value;
		this.lblMaxUVDepth.Text = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblGroupDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblComplexity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblDescCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWarpCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblDrawCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblBgnCompDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblMaxCompDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblEndCompDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblMaxUVDepth;
}
