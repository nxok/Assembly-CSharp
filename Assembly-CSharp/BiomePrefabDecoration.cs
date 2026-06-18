using System;

public class BiomePrefabDecoration
{
	public BiomePrefabDecoration(string _prefabName, float _prob, bool _isDecorateOnSlopes, int _checkResource = 2147483647)
	{
		this.prefabName = _prefabName;
		this.prob = _prob;
		this.checkResourceOffsetY = _checkResource;
		this.isDecorateOnSlopes = _isDecorateOnSlopes;
	}

	public string prefabName;

	public float prob;

	public int checkResourceOffsetY;

	public bool isDecorateOnSlopes;
}
