using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public class NameIdMapping : IMemoryPoolableObject, IDisposable
{
	public Dictionary<string, int>.Enumerator NamesToIdsIterator
	{
		get
		{
			return this.namesToIds.GetEnumerator();
		}
	}

	public NameIdMapping()
	{
	}

	public NameIdMapping(string _filename, int _maxIds)
	{
		this.InitMapping(_filename, _maxIds);
	}

	public void InitMapping(string _filename, int _maxIds)
	{
		this.filename = _filename;
		this.path = Path.GetDirectoryName(_filename);
		if (this.namesToIds == null)
		{
			this.namesToIds = new Dictionary<string, int>(_maxIds);
		}
		if (this.idsToNames == null || this.idsToNames.Length < _maxIds)
		{
			this.idsToNames = new string[_maxIds];
		}
	}

	public void AddMapping(int _id, string _name, bool _force = false)
	{
		lock (this)
		{
			if (this.idsToNames[_id] == null || _force)
			{
				this.idsToNames[_id] = _name;
				this.namesToIds[_name] = _id;
				this.isDirty = true;
			}
		}
	}

	public int GetIdForName(string _name)
	{
		lock (this)
		{
			int num;
			if (this.namesToIds.TryGetValue(_name, out num))
			{
				return num;
			}
		}
		return -1;
	}

	public string GetNameForId(int _id)
	{
		string text;
		lock (this)
		{
			text = this.idsToNames[_id];
		}
		return text;
	}

	public ArrayListMP<int> createIdTranslationTable(Func<string, int> _getDstId, NameIdMapping.MissingEntryCallbackDelegate _onMissingDestination = null)
	{
		ArrayListMP<int> arrayListMP = new ArrayListMP<int>(MemoryPools.poolInt, Block.MAX_BLOCKS);
		int[] items = arrayListMP.Items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i] = -1;
		}
		for (int j = 0; j < this.idsToNames.Length; j++)
		{
			string text = this.idsToNames[j];
			if (text != null)
			{
				int num = _getDstId(text);
				if (num < 0)
				{
					if (_onMissingDestination == null)
					{
						Log.Error(string.Format("Creating id translation table from \"{0}\" failed: Entry \"{1}\" ({2}) in source map is unknown.", this.filename, text, j));
						return null;
					}
					num = _onMissingDestination(text, j);
					if (num < 0)
					{
						return null;
					}
				}
				items[j] = num;
			}
		}
		return arrayListMP;
	}

	public int ReplaceNames([TupleElementNames(new string[] { "oldName", "newName" })] IEnumerable<ValueTuple<string, string>> _replacementList)
	{
		int num = 0;
		lock (this)
		{
			foreach (ValueTuple<string, string> valueTuple in _replacementList)
			{
				string item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				int num2;
				if (this.namesToIds.TryGetValue(item, out num2))
				{
					this.idsToNames[num2] = item2;
					this.namesToIds.Remove(item);
					this.namesToIds[item2] = num2;
					num++;
				}
			}
			if (num > 0)
			{
				this.isDirty = true;
			}
		}
		return num;
	}

	public void SaveIfDirty(bool _async = true)
	{
		if (this.isDirty)
		{
			if (_async)
			{
				ThreadManager.AddSingleTask(delegate(ThreadManager.TaskInfo _info)
				{
					this.WriteToFile();
				}, null, null, true);
				return;
			}
			this.WriteToFile();
		}
	}

	public void WriteToFile()
	{
		try
		{
			if (this.filename == null)
			{
				Log.Error("Can not save mapping, no filename specified");
			}
			else
			{
				if (!SdDirectory.Exists(this.path))
				{
					SdDirectory.CreateDirectory(this.path);
				}
				using (Stream stream = SdFile.Open(this.filename, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
					{
						pooledBinaryWriter.SetBaseStream(stream);
						pooledBinaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
						this.SaveToWriter(pooledBinaryWriter);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not save file '" + this.filename + "': " + ex.Message);
			Log.Exception(ex);
		}
	}

	public byte[] SaveToArray()
	{
		byte[] array;
		using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
		{
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.SaveToWriter(pooledBinaryWriter);
			}
			array = pooledExpandableMemoryStream.ToArray();
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveToWriter(BinaryWriter _writer)
	{
		_writer.Write(1);
		lock (this)
		{
			int num = 0;
			long position = _writer.BaseStream.Position;
			_writer.Write(num);
			for (int i = 0; i < this.idsToNames.Length; i++)
			{
				string text = this.idsToNames[i];
				if (text != null)
				{
					_writer.Write(i);
					_writer.Write(text);
					num++;
				}
			}
			_writer.BaseStream.Position = position;
			_writer.Write(num);
			_writer.BaseStream.Position = _writer.BaseStream.Length;
			this.isDirty = false;
		}
	}

	public bool LoadFromFile()
	{
		bool flag;
		try
		{
			if (this.filename == null)
			{
				Log.Error("Can not load mapping, no filename specified");
				flag = false;
			}
			else if (!SdFile.Exists(this.filename))
			{
				flag = false;
			}
			else
			{
				using (Stream stream = SdFile.OpenRead(this.filename))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(stream);
						this.LoadFromReader(pooledBinaryReader);
					}
				}
				flag = true;
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not load file '" + this.filename + "': " + ex.Message);
			Log.Exception(ex);
			flag = false;
		}
		return flag;
	}

	public bool LoadFromArray(byte[] _data)
	{
		bool flag;
		try
		{
			using (MemoryStream memoryStream = new MemoryStream(_data))
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader.SetBaseStream(memoryStream);
					this.LoadFromReader(pooledBinaryReader);
				}
			}
			flag = true;
		}
		catch (Exception ex)
		{
			Log.Error("Could not load mapping from array: " + ex.Message);
			Log.Exception(ex);
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadFromReader(BinaryReader _reader)
	{
		_reader.ReadInt32();
		lock (this)
		{
			Array.Clear(this.idsToNames, 0, this.idsToNames.Length);
			this.namesToIds.Clear();
			int num = _reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int num2 = _reader.ReadInt32();
				string text = _reader.ReadString();
				this.idsToNames[num2] = text;
				this.namesToIds[text] = num2;
			}
			this.isDirty = false;
		}
	}

	public void Reset()
	{
		this.filename = null;
		this.path = null;
		if (this.idsToNames != null)
		{
			Array.Clear(this.idsToNames, 0, this.idsToNames.Length);
		}
		if (this.namesToIds != null)
		{
			this.namesToIds.Clear();
		}
		this.isDirty = false;
	}

	public void Cleanup()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void System.IDisposable.Dispose()
	{
		MemoryPools.poolNameIdMapping.FreeSync(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILE_VERSION = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> namesToIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] idsToNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string path;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filename;

	public delegate int MissingEntryCallbackDelegate(string _entryName, int _sourceId);
}
