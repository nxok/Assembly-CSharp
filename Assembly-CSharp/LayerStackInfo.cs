using System;

public struct LayerStackInfo
{
	public LayerStackInfo(int startCompStackIndex, int startUVStackIndex)
	{
		this.GroupDepth = 0;
		this.MaxCompStackIndex = startCompStackIndex;
		this.MaxUVStackIndex = startUVStackIndex;
	}

	public int GroupDepth;

	public int MaxCompStackIndex;

	public int MaxUVStackIndex;
}
