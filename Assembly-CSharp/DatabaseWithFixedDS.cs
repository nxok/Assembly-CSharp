using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class DatabaseWithFixedDS<KEY, DATA> where KEY : struct where DATA : class
{
	public DatabaseWithFixedDS(int _magicBytes, int _sizeofKey, int _maxCountOfDataSets, int _sizeOfDataSet, KEY _invalidKeyValue, int _oldMaxCountOfDataSets = -1)
	{
		this.magicBytes = _magicBytes;
		this.sizeofKey = _sizeofKey;
		this.maxCountOfDataSets = _maxCountOfDataSets;
		this.oldMaxCountOfDataSets = _oldMaxCountOfDataSets;
		this.sizeOfDataSet = _sizeOfDataSet;
		this.invalidKeyValue = _invalidKeyValue;
	}

	public List<KEY> GetAllKeys()
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		List<KEY> list;
		lock (dictionaryKeyList)
		{
			list = this.catalog.list;
		}
		return list;
	}

	public DATA GetDS(KEY _key)
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		DATA data;
		lock (dictionaryKeyList)
		{
			if (!this.catalog.dict.ContainsKey(_key))
			{
				data = default(DATA);
				data = data;
			}
			else
			{
				data = this.database[_key];
			}
		}
		return data;
	}

	public void SetDS(KEY _key, DATA _data)
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		lock (dictionaryKeyList)
		{
			if (this.catalog.dict.Count >= this.maxCountOfDataSets)
			{
				int num = this.maxCountOfDataSets / 10;
				int num2 = 0;
				while (this.catalog.list.Count > 0 && num2 < num)
				{
					KEY key = this.catalog.list[0];
					this.catalog.Remove(key);
					this.database.Remove(key);
					num2++;
				}
				this.dirty.Clear();
				for (int i = 0; i < this.catalog.list.Count; i++)
				{
					this.dirty[this.catalog.list[i]] = true;
				}
			}
			if (!this.catalog.dict.ContainsKey(_key))
			{
				this.catalog.Add(_key, this.catalog.list.Count);
			}
			this.database[_key] = _data;
			this.dirty[_key] = true;
		}
	}

	public bool ContainsDS(KEY _key)
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		bool flag2;
		lock (dictionaryKeyList)
		{
			flag2 = this.catalog.dict.ContainsKey(_key);
		}
		return flag2;
	}

	public int CountDS()
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		int count;
		lock (dictionaryKeyList)
		{
			count = this.catalog.list.Count;
		}
		return count;
	}

	public virtual void Clear()
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		lock (dictionaryKeyList)
		{
			this.catalog.Clear();
			this.database.Clear();
			this.dirty.Clear();
		}
	}

	public void Load(string _dir, string _filename)
	{
		try
		{
			if (SdFile.Exists(_dir + "/" + _filename))
			{
				using (Stream stream = SdFile.OpenRead(_dir + "/" + _filename))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(stream);
						int num = pooledBinaryReader.ReadInt32();
						if (num != this.magicBytes)
						{
							Log.Error(string.Format("Map file has invalid magic bytes: 0x{0:X8}", num));
						}
						else
						{
							uint num2 = (uint)pooledBinaryReader.ReadByte();
							if (this.arrayReadBuf == null)
							{
								this.arrayReadBuf = new byte[this.sizeOfDataSet];
							}
							pooledBinaryReader.Read(this.arrayReadBuf, 0, 3);
							this.read(pooledBinaryReader, num2);
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat(new string[] { "Could not load file '", _dir, "/", _filename, "': ", ex.Message }));
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(BinaryReader _br, uint _version)
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		lock (dictionaryKeyList)
		{
			int num = this.oldMaxCountOfDataSets;
			if (_version <= 2U && this.oldMaxCountOfDataSets < 0)
			{
				throw new Exception("Can not load old dataset file, unknown size of key list");
			}
			if (_version > 2U)
			{
				num = _br.ReadInt32();
			}
			this.catalog.Clear();
			this.dirty.Clear();
			uint num2 = _br.ReadUInt32();
			for (uint num3 = 0U; num3 < num2; num3 += 1U)
			{
				KEY key = this.readKey(_br);
				this.catalog.Add(key, (int)num3);
			}
			uint num4 = num2;
			while ((ulong)num4 < (ulong)((long)num))
			{
				this.readKey(_br);
				num4 += 1U;
			}
			this.database.Clear();
			for (int i = 0; i < this.catalog.list.Count; i++)
			{
				_br.Read(this.arrayReadBuf, 0, this.arrayReadBuf.Length);
				DATA data = this.allocateDataStorage();
				this.copyFromRead(this.arrayReadBuf, data);
				this.database[this.catalog.list[i]] = data;
			}
			if (num != this.maxCountOfDataSets)
			{
				if (num > this.maxCountOfDataSets)
				{
					int num5 = num - this.maxCountOfDataSets;
					int num6 = 0;
					while (this.catalog.list.Count > 0 && num6 < num5)
					{
						KEY key2 = this.catalog.list[0];
						this.catalog.Remove(key2);
						this.database.Remove(key2);
						num6++;
					}
					this.dirty.Clear();
					for (int j = 0; j < this.catalog.list.Count; j++)
					{
						this.dirty[this.catalog.list[j]] = true;
					}
				}
				else
				{
					this.dirty.Clear();
					for (int k = 0; k < this.catalog.list.Count; k++)
					{
						this.dirty[this.catalog.list[k]] = true;
					}
				}
			}
		}
	}

	public void Save(string _dir, string _filename)
	{
		try
		{
			if (!SdDirectory.Exists(_dir))
			{
				SdDirectory.CreateDirectory(_dir);
			}
			lock (this)
			{
				using (Stream stream = SdFile.Open(_dir + "/" + _filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
				{
					using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
					{
						pooledBinaryWriter.SetBaseStream(stream);
						pooledBinaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
						pooledBinaryWriter.Write(this.magicBytes);
						pooledBinaryWriter.Write(3);
						pooledBinaryWriter.Write(0);
						pooledBinaryWriter.Write(0);
						pooledBinaryWriter.Write(0);
						this.write(pooledBinaryWriter);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat(new string[] { "Could not save file '", _dir, "/", _filename, "': ", ex.Message }));
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(BinaryWriter _bw)
	{
		DictionaryKeyList<KEY, int> dictionaryKeyList = this.catalog;
		lock (dictionaryKeyList)
		{
			_bw.Write((uint)this.maxCountOfDataSets);
			_bw.Write((uint)this.catalog.list.Count);
			for (int i = 0; i < this.catalog.list.Count; i++)
			{
				this.writeKey(_bw, this.catalog.list[i]);
			}
			for (int j = this.catalog.list.Count; j < this.maxCountOfDataSets; j++)
			{
				this.writeKey(_bw, this.invalidKeyValue);
			}
			int num = 0;
			if (this.arrayWriteBuf == null)
			{
				this.arrayWriteBuf = new byte[this.sizeOfDataSet];
			}
			int count = this.catalog.list.Count;
			for (int k = 0; k < count; k++)
			{
				if (!this.dirty.ContainsKey(this.catalog.list[k]) || !this.dirty[this.catalog.list[k]])
				{
					_bw.Seek(this.sizeOfDataSet, SeekOrigin.Current);
				}
				else
				{
					DATA data = this.database[this.catalog.list[k]];
					this.copyToWrite(data, this.arrayWriteBuf);
					_bw.Write(this.arrayWriteBuf, 0, this.arrayWriteBuf.Length);
					num++;
				}
			}
			this.dirty.Clear();
			Array.Clear(this.arrayWriteBuf, 0, this.arrayWriteBuf.Length);
			int num2 = (int)Mathf.Pow(2f, Mathf.Ceil(Mathf.Log((float)count) / Mathf.Log(2f)));
			int num3 = 16 + this.maxCountOfDataSets * this.sizeofKey + num2 * this.sizeOfDataSet;
			if (_bw.BaseStream.Length < (long)num3)
			{
				for (int l = count; l < num2; l++)
				{
					_bw.Write(this.arrayWriteBuf, 0, this.arrayWriteBuf.Length);
				}
			}
			else if (_bw.BaseStream.Length > (long)num3)
			{
				_bw.BaseStream.SetLength((long)num3);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract KEY readKey(BinaryReader _br);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void writeKey(BinaryWriter _bw, KEY _key);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void copyFromRead(byte[] _dataRead, DATA _data);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void copyToWrite(DATA _data, byte[] _dataWrite);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract DATA allocateDataStorage();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cHeaderSize = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int VERSION = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public int magicBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeofKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxCountOfDataSets;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldMaxCountOfDataSets;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeOfDataSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public KEY invalidKeyValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryKeyList<KEY, int> catalog = new DictionaryKeyList<KEY, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<KEY, DATA> database = new DictionarySave<KEY, DATA>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<KEY, bool> dirty = new Dictionary<KEY, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] arrayReadBuf;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] arrayWriteBuf;
}
