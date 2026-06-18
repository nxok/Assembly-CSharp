using System;
using System.Collections.Generic;

public class BiomeLayer
{
	public BiomeLayer(int _depth, BiomeBlockDecoration _bb)
	{
		this.m_Block = _bb;
		this.m_Depth = _depth;
		this.m_Resources = new List<BiomeBlockDecoration>();
		this.SumResourceProbs = new List<float>();
		this.MaxResourceProb = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~BiomeLayer()
	{
	}

	public void AddResource(BiomeBlockDecoration _res)
	{
		this.m_Resources.Add(_res);
		this.MaxResourceProb = Utils.FastMax(_res.prob, this.MaxResourceProb);
		int count = this.SumResourceProbs.Count;
		this.SumResourceProbs.Add((count > 0) ? (this.SumResourceProbs[count - 1] + _res.prob) : _res.prob);
	}

	public BiomeBlockDecoration m_Block;

	public int m_Depth;

	public List<BiomeBlockDecoration> m_Resources;

	public List<float> SumResourceProbs;

	public float MaxResourceProb;
}
