using System;

public struct VolumeKey : IEquatable<VolumeKey>
{
	public VolumeKey(Vector3i boxMin, Vector3i boxMax)
	{
		this.boxMin = boxMin;
		this.boxMax = boxMax;
	}

	public VolumeKey(SleeperVolume volume)
	{
		this.boxMin = volume.BoxMin;
		this.boxMax = volume.BoxMax;
	}

	public VolumeKey(TriggerVolume volume)
	{
		this.boxMin = volume.BoxMin;
		this.boxMax = volume.BoxMax;
	}

	public VolumeKey(WallVolume volume)
	{
		this.boxMin = volume.BoxMin;
		this.boxMax = volume.BoxMax;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, int>(this.boxMin.GetHashCode(), this.boxMax.GetHashCode());
	}

	public bool Equals(VolumeKey other)
	{
		return other.boxMin == this.boxMin && other.boxMax == this.boxMax;
	}

	public override bool Equals(object obj)
	{
		if (obj is VolumeKey)
		{
			VolumeKey volumeKey = (VolumeKey)obj;
			return this.Equals(volumeKey);
		}
		return false;
	}

	public override string ToString()
	{
		return string.Format("({0}) ({1})", this.boxMin, this.boxMax);
	}

	public Vector3i boxMin;

	public Vector3i boxMax;
}
