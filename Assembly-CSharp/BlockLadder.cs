using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockLadder : Block
{
	public override bool IsElevator()
	{
		return true;
	}

	public override bool IsElevator(int rotation)
	{
		return BlockLadder.climbableRotations[rotation] > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] climbableRotations = new byte[]
	{
		1, 1, 1, 1, 1, 1, 1, 1, 0, 1,
		0, 1, 1, 0, 1, 0, 0, 1, 0, 1,
		1, 0, 1, 0, 0, 0, 0, 0, 0, 0,
		0, 0
	};
}
