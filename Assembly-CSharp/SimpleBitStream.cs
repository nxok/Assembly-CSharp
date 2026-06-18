using System;
using System.Collections.Generic;
using System.IO;

[PublicizedFrom(EAccessModifier.Internal)]
public class SimpleBitStream
{
	public SimpleBitStream(int _initialCapacity = 1000)
	{
		this.data = new List<byte>(_initialCapacity);
		this.Reset();
	}

	public void Reset()
	{
		this.data.Clear();
		this.curBitIdx = 0;
		this.curByteIdx = 0;
		this.curByteData = 0;
	}

	public void Add(bool _b)
	{
		if (_b)
		{
			this.curByteData = (byte)((int)this.curByteData | (1 << this.curBitIdx));
		}
		this.curBitIdx++;
		if (this.curBitIdx > 7)
		{
			this.data.Add(this.curByteData);
			this.curBitIdx = 0;
			this.curByteIdx++;
			this.curByteData = 0;
		}
	}

	public bool GetNext()
	{
		if (this.curBitIdx > 7)
		{
			List<byte> list = this.data;
			int num = this.curByteIdx + 1;
			this.curByteIdx = num;
			this.curByteData = list[num];
			this.curBitIdx = 0;
		}
		bool flag = (this.curByteData & 1) > 0;
		this.curByteData = (byte)(this.curByteData >> 1);
		this.curBitIdx++;
		return flag;
	}

	public int GetNextOffset()
	{
		bool flag = false;
		for (;;)
		{
			if (this.curBitIdx > 7)
			{
				this.curByteIdx++;
				if (this.curByteIdx >= this.data.Count)
				{
					break;
				}
				this.curByteData = this.data[this.curByteIdx];
				this.curBitIdx = 0;
			}
			if (this.curByteData == 0)
			{
				this.curBitIdx = 8;
			}
			else
			{
				flag = (this.curByteData & 1) == 1;
				this.curByteData = (byte)(this.curByteData >> 1);
				this.curBitIdx++;
			}
			if (flag)
			{
				goto Block_4;
			}
		}
		return -1;
		Block_4:
		return this.curByteIdx * 8 + this.curBitIdx - 1;
	}

	public void Write(BinaryWriter _bw)
	{
		if (this.curBitIdx > 0)
		{
			this.data.Add(this.curByteData);
		}
		_bw.Write(this.data.Count);
		for (int i = 0; i < this.data.Count; i++)
		{
			_bw.Write(this.data[i]);
		}
	}

	public void Read(BinaryReader _br)
	{
		int num = _br.ReadInt32();
		byte[] array = _br.ReadBytes(num);
		this.data.Clear();
		this.data.AddRange(array);
		if (num > 0)
		{
			this.curByteData = this.data[0];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<byte> data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curBitIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curByteIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte curByteData;
}
