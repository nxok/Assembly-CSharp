using System;
using Unity.Mathematics;

public struct GroundWaterBounds
{
	public bool IsGroundWater
	{
		get
		{
			return this.state > 0;
		}
	}

	public GroundWaterBounds(int _groundHeight, int _waterHeight)
	{
		this.state = 1;
		this.waterHeight = (byte)math.clamp(_waterHeight, 0, 255);
		this.bottom = (byte)math.clamp(_groundHeight, 0, (int)this.waterHeight);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte state;

	public byte waterHeight;

	public byte bottom;
}
