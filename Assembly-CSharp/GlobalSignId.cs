using System;
using System.IO;

public readonly struct GlobalSignId : IEquatable<GlobalSignId>
{
	public GlobalSignId(string libraryId, Guid signGuid)
	{
		this.libraryId = libraryId;
		this.signGuid = signGuid;
	}

	public static GlobalSignId InvalidId
	{
		get
		{
			return new GlobalSignId(string.Empty, Guid.Empty);
		}
	}

	public bool IsValid
	{
		get
		{
			return !string.IsNullOrEmpty(this.libraryId);
		}
	}

	public bool Equals(GlobalSignId other)
	{
		return this.libraryId == other.libraryId && this.signGuid == other.signGuid;
	}

	public override bool Equals(object obj)
	{
		if (obj is GlobalSignId)
		{
			GlobalSignId globalSignId = (GlobalSignId)obj;
			return this.Equals(globalSignId);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<string, Guid>(this.libraryId, this.signGuid);
	}

	public override string ToString()
	{
		return string.Format("{0}:{1}", this.libraryId, this.signGuid);
	}

	public static bool operator ==(GlobalSignId a, GlobalSignId b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(GlobalSignId a, GlobalSignId b)
	{
		return !a.Equals(b);
	}

	public unsafe static GlobalSignId FromStream(BinaryReader _br)
	{
		string text = _br.ReadString();
		Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)16], 16);
		if (_br.Read(span) != 16)
		{
			Log.Error("Invalid Guid data.");
		}
		Guid guid = new Guid(span);
		return new GlobalSignId(text, guid);
	}

	public unsafe void ToStream(BinaryWriter _bw)
	{
		_bw.Write(this.libraryId);
		Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)16], 16);
		this.signGuid.TryWriteBytes(span);
		_bw.Write(span);
	}

	public readonly string libraryId;

	public readonly Guid signGuid;
}
