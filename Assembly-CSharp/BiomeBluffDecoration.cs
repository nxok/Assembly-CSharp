using System;

public class BiomeBluffDecoration
{
	public BiomeBluffDecoration(string _name, float _prob, float _minScale, float _maxScale)
	{
		this.m_sName = _name;
		this.m_Prob = _prob;
		this.m_MinScale = _minScale;
		this.m_MaxScale = _maxScale;
	}

	public string m_sName;

	public float m_Prob;

	public float m_MinScale;

	public float m_MaxScale;
}
