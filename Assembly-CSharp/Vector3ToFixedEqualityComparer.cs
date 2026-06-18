using System;
using System.Collections.Generic;
using UnityEngine;

public class Vector3ToFixedEqualityComparer : IEqualityComparer<Vector3>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3ToFixedEqualityComparer()
	{
	}

	public bool Equals(Vector3 _a, Vector3 _b)
	{
		return this.GetHashCode(_a) == this.GetHashCode(_b);
	}

	public int GetHashCode(Vector3 _a)
	{
		return this.FloatToFixed(_a.x).GetHashCode() ^ (this.FloatToFixed(_a.y).GetHashCode() << 2) ^ (this.FloatToFixed(_a.z).GetHashCode() >> 2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FloatToFixed(float a)
	{
		return (int)Math.Round((double)a * 32.0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFixedPointFractionalBits = 5;

	public static readonly Vector3ToFixedEqualityComparer Instance = new Vector3ToFixedEqualityComparer();
}
