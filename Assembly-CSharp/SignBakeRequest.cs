using System;

public readonly struct SignBakeRequest : IComparable<SignBakeRequest>
{
	public SignBakeRequest(int groupIndex, int tier, float groupMinDistanceSquared)
	{
		this.GroupIndex = groupIndex;
		this.Tier = tier;
		this.GroupMinDistanceSquared = groupMinDistanceSquared;
	}

	public int CompareTo(SignBakeRequest other)
	{
		int num = this.Tier.CompareTo(other.Tier);
		if (num != 0)
		{
			return num;
		}
		return this.GroupMinDistanceSquared.CompareTo(other.GroupMinDistanceSquared);
	}

	public readonly int GroupIndex;

	public readonly int Tier;

	public readonly float GroupMinDistanceSquared;
}
