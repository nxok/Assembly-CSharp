using System;

public class VertexDistortion
{
	[PublicizedFrom(EAccessModifier.Private)]
	static VertexDistortion()
	{
		for (int i = 0; i < VertexDistortion.arrayB.Length; i++)
		{
			VertexDistortion.arrayB[i] *= 1.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] arrayB = new float[] { 0f, 0.2f, 0.15f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f };
}
