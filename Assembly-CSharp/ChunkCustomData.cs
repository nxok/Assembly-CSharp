using System;
using System.IO;

public class ChunkCustomData
{
	public ChunkCustomData()
	{
	}

	public ChunkCustomData(string _key, ulong _expiresInWorldTime, bool _isSavedToNetwork)
	{
		this.key = _key;
		this.expiresInWorldTime = _expiresInWorldTime;
		this.isSavedToNetwork = _isSavedToNetwork;
	}

	public void Read(BinaryReader _br)
	{
		this.key = _br.ReadString();
		this.expiresInWorldTime = _br.ReadUInt64();
		this.isSavedToNetwork = _br.ReadBoolean();
		int num = (int)_br.ReadUInt16();
		if (num > 0)
		{
			this.data = _br.ReadBytes(num);
			return;
		}
		this.data = null;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(this.key);
		_bw.Write(this.expiresInWorldTime);
		_bw.Write(this.isSavedToNetwork);
		if (this.TriggerWriteDataDelegate != null)
		{
			this.TriggerWriteDataDelegate();
		}
		_bw.Write((ushort)((this.data != null) ? this.data.Length : 0));
		if (this.data != null && this.data.Length != 0)
		{
			_bw.Write(this.data);
		}
	}

	public virtual void OnRemove(Chunk chunk)
	{
	}

	public string key;

	public ulong expiresInWorldTime;

	public bool isSavedToNetwork;

	public byte[] data;

	public ChunkCustomData.TriggerWriteData TriggerWriteDataDelegate;

	public delegate void TriggerWriteData();
}
