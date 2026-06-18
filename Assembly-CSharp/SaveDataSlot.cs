using System;

public readonly struct SaveDataSlot : IEquatable<SaveDataSlot>, IComparable<SaveDataSlot>, IComparable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataSlot(SaveDataType type, StringSpan slotPath)
	{
		this = new SaveDataSlot(new SaveDataManagedPath((slotPath.Length > 0) ? SpanUtils.Concat(type.GetPathRaw(), "/", slotPath, "/d") : SpanUtils.Concat(type.GetPathRaw(), "/d")));
		if (this.Type != type)
		{
			throw new ArgumentException(string.Format("Got type {0} but expected {1}. Make sure that concatenating the slot path does not match another type.", this.Type, type), "type");
		}
		if (this.SlotPath != slotPath)
		{
			throw new ArgumentException(SpanUtils.Concat("Expected slot path to be '", slotPath, "', but was: ", this.SlotPath), "slotPath");
		}
	}

	public SaveDataSlot(SaveDataManagedPath managedPath)
	{
		this.m_internalPath = managedPath;
	}

	public SaveDataType Type
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_internalPath.Type;
		}
	}

	public StringSpan SlotPath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_internalPath.SlotPath;
		}
	}

	public SaveDataSlot GetSimpleSlot()
	{
		if (!(this.m_internalPath.PathRelativeToSlot == "d"))
		{
			return new SaveDataSlot(this.Type, this.SlotPath);
		}
		return this;
	}

	public override string ToString()
	{
		if (this.SlotPath.Length != 0)
		{
			return SpanUtils.Concat(this.Type.ToStringCached<SaveDataType>(), "[", this.SlotPath, "]");
		}
		return this.Type.ToStringCached<SaveDataType>();
	}

	public bool Equals(SaveDataSlot other)
	{
		if (other.m_internalPath == null)
		{
			return this.m_internalPath == null;
		}
		return this.Type == other.Type && this.SlotPath == other.SlotPath;
	}

	public override bool Equals(object obj)
	{
		if (obj is SaveDataSlot)
		{
			SaveDataSlot saveDataSlot = (SaveDataSlot)obj;
			return this.Equals(saveDataSlot);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)((this.Type * (SaveDataType)397) ^ (SaveDataType)this.SlotPath.GetHashCode());
	}

	public static bool operator ==(SaveDataSlot left, SaveDataSlot right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SaveDataSlot left, SaveDataSlot right)
	{
		return !left.Equals(right);
	}

	public int CompareTo(SaveDataSlot other)
	{
		int num = this.Type.CompareTo(other.Type);
		if (num != 0)
		{
			return num;
		}
		return this.SlotPath.CompareTo(other.SlotPath, StringComparison.Ordinal);
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (obj is SaveDataSlot)
		{
			SaveDataSlot saveDataSlot = (SaveDataSlot)obj;
			return this.CompareTo(saveDataSlot);
		}
		throw new ArgumentException("Object must be of type SaveDataSlot");
	}

	public static bool operator <(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator <=(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static bool operator >=(SaveDataSlot left, SaveDataSlot right)
	{
		return left.CompareTo(right) >= 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DUMMY_POSTFIX_WITHOUT_SLASH = "d";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DUMMY_POSTFIX_WITH_SLASH = "/d";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SaveDataManagedPath m_internalPath;
}
