using System;

public struct IntVect
{
	public IntVect(int x, int y, int z)
	{
		this.m_X = x;
		this.m_Y = y;
		this.m_Z = z;
	}

	public int X
	{
		get
		{
			return this.m_X;
		}
	}

	public int Y
	{
		get
		{
			return this.m_Y;
		}
	}

	public int Z
	{
		get
		{
			return this.m_Z;
		}
	}

	public override bool Equals(object obj)
	{
		IntVect intVect = (IntVect)obj;
		return intVect.X == this.m_X && intVect.Y == this.m_Y && intVect.Z == this.m_Z;
	}

	public override int GetHashCode()
	{
		return this.m_X * 8976890 + this.m_Y * 981131 + this.m_Z;
	}

	public static bool operator ==(IntVect one, IntVect other)
	{
		return one.X == other.X && one.Y == other.Y && one.Z == other.Z;
	}

	public static bool operator !=(IntVect one, IntVect other)
	{
		return !(one == other);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_X;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_Y;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_Z;
}
