using System;

public class PoiMapBlock
{
	public PoiMapBlock(BlockValue _blockValue, float _prob, int _offset)
	{
		this.blockValue = _blockValue;
		this.m_Prob = _prob;
		this.offset = _offset;
	}

	public BlockValue blockValue;

	public float m_Prob;

	public int offset;
}
