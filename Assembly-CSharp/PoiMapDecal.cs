using System;

public class PoiMapDecal
{
	public PoiMapDecal(int _texIndex, BlockFace _face, float _prob)
	{
		this.textureIndex = _texIndex;
		this.face = _face;
		this.m_Prob = _prob;
	}

	public int textureIndex;

	public BlockFace face;

	public float m_Prob;
}
