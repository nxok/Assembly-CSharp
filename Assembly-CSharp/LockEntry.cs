using System;

public readonly struct LockEntry : IEquatable<LockEntry>
{
	public LockEntry(ILockTarget target, ushort channel = 0)
	{
		this.Target = target;
		this.Channel = channel;
	}

	public bool Equals(LockEntry other)
	{
		return this.Target == other.Target && this.Channel == other.Channel;
	}

	public override int GetHashCode()
	{
		ILockTarget target = this.Target;
		return HashCode.Combine<int, ushort>((target != null) ? target.GetHashCode() : 0, this.Channel);
	}

	public override bool Equals(object obj)
	{
		if (obj is LockEntry)
		{
			LockEntry lockEntry = (LockEntry)obj;
			return this.Equals(lockEntry);
		}
		return false;
	}

	public static bool operator ==(LockEntry left, LockEntry right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LockEntry left, LockEntry right)
	{
		return !left.Equals(right);
	}

	public readonly ILockTarget Target;

	public readonly ushort Channel;
}
