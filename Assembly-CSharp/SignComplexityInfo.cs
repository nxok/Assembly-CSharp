using System;
using System.Collections.Generic;

public struct SignComplexityInfo
{
	public bool IsValid
	{
		get
		{
			return this.ComplexityByLayer != null;
		}
	}

	public static SignComplexityInfo Invalid
	{
		get
		{
			return new SignComplexityInfo(0f, null, default(LayerStackInfo));
		}
	}

	public SignComplexityInfo(float totalComplexity, Dictionary<SignData.SignLayer, LayerComplexityInfo> complexityByLayer, LayerStackInfo stackInfo)
	{
		this.TotalComplexity = totalComplexity;
		this.ComplexityByLayer = complexityByLayer;
		this.StackInfo = stackInfo;
	}

	public bool TryGetLayerComplexityInfo(SignData.SignLayer layer, out LayerComplexityInfo layerComplexityInfo)
	{
		layerComplexityInfo = default(LayerComplexityInfo);
		return layer != null && this.ComplexityByLayer != null && this.ComplexityByLayer.TryGetValue(layer, out layerComplexityInfo);
	}

	public readonly float TotalComplexity;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<SignData.SignLayer, LayerComplexityInfo> ComplexityByLayer;

	public readonly LayerStackInfo StackInfo;
}
