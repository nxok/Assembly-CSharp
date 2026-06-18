using System;

public struct SNetPackageInfo
{
	public SNetPackageInfo(int _id, int _size)
	{
		this.Tick = GameTimer.Instance.ticks;
		this.Id = _id;
		this.Size = _size;
	}

	public readonly ulong Tick;

	public readonly int Id;

	public readonly int Size;
}
