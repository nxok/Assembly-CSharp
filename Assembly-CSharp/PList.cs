using System;
using System.Collections.Generic;
using System.IO;

public class PList<T> : List<T>
{
	public PList()
		: this(1U)
	{
	}

	public PList(uint _saveVersion)
	{
		this.saveVersion = _saveVersion;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((ushort)base.Count);
		_bw.Write((byte)this.saveVersion);
		foreach (T t in this)
		{
			this.writeElement(_bw, t);
		}
	}

	public void Read(BinaryReader _br)
	{
		base.Clear();
		int num = (int)_br.ReadUInt16();
		uint num2 = (uint)_br.ReadByte();
		for (int i = 0; i < num; i++)
		{
			T t = this.readElement(_br, num2);
			base.Add(t);
		}
	}

	public void MarkToRemove(T _v)
	{
		this.toRemove.Add(_v);
	}

	public void RemoveAllMarked()
	{
		foreach (T t in this.toRemove)
		{
			base.Remove(t);
		}
		this.toRemove.Clear();
	}

	public Action<BinaryWriter, T> writeElement;

	public Func<BinaryReader, uint, T> readElement;

	[PublicizedFrom(EAccessModifier.Protected)]
	public uint saveVersion;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<T> toRemove = new List<T>();
}
