using System;

public class ChunkProviderParameter
{
	public ChunkProviderParameter(int _id, string _name, float _val, float _minVal, float _maxVal)
	{
		this.id = _id;
		this.name = _name;
		this.val = _val;
		this.minVal = _minVal;
		this.maxVal = _maxVal;
	}

	public int id;

	public string name;

	public float val;

	public float minVal;

	public float maxVal;
}
