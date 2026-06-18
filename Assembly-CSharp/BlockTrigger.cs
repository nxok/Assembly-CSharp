using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockTrigger
{
	public Chunk Chunk
	{
		get
		{
			return this.chunk;
		}
		set
		{
			this.chunk = value;
			long num = 0L;
			if (this.chunk != null)
			{
				num = this.chunk.Key;
			}
			this.chunkKey = num;
		}
	}

	public BlockTrigger(Chunk chunkNew)
	{
		this.Chunk = chunkNew;
	}

	public BlockValue BlockValue
	{
		get
		{
			return this.Chunk.GetBlock(this.LocalChunkPos);
		}
	}

	public void Refresh(FastTags<TagGroup.Global> questTag)
	{
		this.chunk = (Chunk)GameManager.Instance.World.GetChunkSync(this.chunkKey);
		if (this.chunk == null)
		{
			string text = "BlockTrigger.Refresh: Chunk null. ChunkKey={0}, LocalChunkPos={1}, PrefabInstance={2}. From: {3}";
			object[] array = new object[4];
			array[0] = this.chunkKey;
			array[1] = this.LocalChunkPos;
			int num = 2;
			PrefabTriggerData triggerDataOwner = this.TriggerDataOwner;
			object obj;
			if (triggerDataOwner == null)
			{
				obj = null;
			}
			else
			{
				PrefabInstance prefabInstance = triggerDataOwner.PrefabInstance;
				obj = ((prefabInstance != null) ? prefabInstance.name : null);
			}
			array[num] = obj;
			array[3] = StackTraceUtility.ExtractStackTrace();
			Log.Error(string.Format(text, array));
			return;
		}
		BlockValue blockValue = this.BlockValue;
		blockValue.Block.OnTriggerRefresh(this, blockValue, questTag);
	}

	public void Read(PooledBinaryReader _br)
	{
		this.currentVersion = _br.ReadUInt16();
		if (this.currentVersion >= 2)
		{
			this.NeedsTriggered = (BlockTrigger.TriggeredStates)_br.ReadByte();
		}
		int num = (int)_br.ReadByte();
		this.TriggersIndices.Clear();
		for (int i = 0; i < num; i++)
		{
			this.TriggersIndices.Add(_br.ReadByte());
		}
		num = (int)_br.ReadByte();
		this.TriggeredByIndices.Clear();
		for (int j = 0; j < num; j++)
		{
			this.TriggeredByIndices.Add(_br.ReadByte());
		}
		num = (int)_br.ReadByte();
		this.TriggeredValues.Clear();
		for (int k = 0; k < num; k++)
		{
			this.TriggeredValues.Add(_br.ReadByte());
		}
		if (this.currentVersion >= 3)
		{
			this.ExcludeIcon = _br.ReadBoolean();
		}
		if (this.currentVersion >= 4)
		{
			this.UseOrForMultipleTriggers = _br.ReadBoolean();
		}
		if (this.currentVersion >= 5)
		{
			this.Unlock = _br.ReadBoolean();
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(5);
		_bw.Write((byte)this.NeedsTriggered);
		_bw.Write((byte)this.TriggersIndices.Count);
		for (int i = 0; i < this.TriggersIndices.Count; i++)
		{
			_bw.Write(this.TriggersIndices[i]);
		}
		_bw.Write((byte)this.TriggeredByIndices.Count);
		for (int j = 0; j < this.TriggeredByIndices.Count; j++)
		{
			_bw.Write(this.TriggeredByIndices[j]);
		}
		_bw.Write((byte)this.TriggeredValues.Count);
		for (int k = 0; k < this.TriggeredValues.Count; k++)
		{
			_bw.Write(this.TriggeredValues[k]);
		}
		_bw.Write(this.ExcludeIcon);
		_bw.Write(this.UseOrForMultipleTriggers);
		_bw.Write(this.Unlock);
	}

	public BlockTrigger Clone()
	{
		BlockTrigger blockTrigger = new BlockTrigger(this.Chunk);
		blockTrigger.LocalChunkPos = this.LocalChunkPos;
		blockTrigger.TriggersIndices.Clear();
		blockTrigger.TriggeredByIndices.Clear();
		blockTrigger.TriggeredValues.Clear();
		for (int i = 0; i < this.TriggersIndices.Count; i++)
		{
			blockTrigger.TriggersIndices.Add(this.TriggersIndices[i]);
		}
		for (int j = 0; j < this.TriggeredByIndices.Count; j++)
		{
			blockTrigger.TriggeredByIndices.Add(this.TriggeredByIndices[j]);
		}
		for (int k = 0; k < this.TriggeredValues.Count; k++)
		{
			blockTrigger.TriggeredValues.Add(this.TriggeredValues[k]);
		}
		blockTrigger.ExcludeIcon = this.ExcludeIcon;
		blockTrigger.UseOrForMultipleTriggers = this.UseOrForMultipleTriggers;
		blockTrigger.Unlock = this.Unlock;
		return blockTrigger;
	}

	public void CopyFrom(BlockTrigger _other)
	{
		this.LocalChunkPos = _other.LocalChunkPos;
		this.TriggersIndices.Clear();
		this.TriggeredByIndices.Clear();
		this.TriggeredValues.Clear();
		for (int i = 0; i < _other.TriggersIndices.Count; i++)
		{
			this.TriggersIndices.Add(_other.TriggersIndices[i]);
		}
		for (int j = 0; j < _other.TriggeredByIndices.Count; j++)
		{
			this.TriggeredByIndices.Add(_other.TriggeredByIndices[j]);
		}
		for (int k = 0; k < _other.TriggeredValues.Count; k++)
		{
			this.TriggeredValues.Add(_other.TriggeredValues[k]);
		}
		_other.ExcludeIcon = this.ExcludeIcon;
		_other.UseOrForMultipleTriggers = this.UseOrForMultipleTriggers;
		_other.Unlock = this.Unlock;
	}

	public void SetTriggersFlag(byte index)
	{
		if (!this.TriggersIndices.Contains(index))
		{
			this.TriggersIndices.Add(index);
		}
	}

	public void RemoveTriggersFlag(byte index)
	{
		this.TriggersIndices.Remove(index);
	}

	public void RemoveAllTriggersFlags()
	{
		this.TriggersIndices.Clear();
	}

	public bool HasTriggers(byte index)
	{
		return this.TriggersIndices.Contains(index);
	}

	public void ToggleTriggersFlag(byte layer)
	{
		int num = this.TriggersIndices.IndexOf(layer);
		if (num >= 0)
		{
			this.TriggersIndices.RemoveAt(num);
			return;
		}
		this.TriggersIndices.Add(layer);
	}

	public bool HasAnyTriggers()
	{
		return this.TriggersIndices.Count > 0;
	}

	public void SetTriggeredByFlag(byte index)
	{
		if (!this.TriggeredByIndices.Contains(index))
		{
			this.TriggeredByIndices.Add(index);
		}
	}

	public void RemoveTriggeredByFlag(byte index)
	{
		this.TriggeredByIndices.Remove(index);
	}

	public bool HasTriggeredBy(byte index)
	{
		return this.TriggeredByIndices.Contains(index);
	}

	public void ToggleTriggeredByFlag(byte layer)
	{
		int num = this.TriggeredByIndices.IndexOf(layer);
		if (num >= 0)
		{
			this.TriggeredByIndices.RemoveAt(num);
			return;
		}
		this.TriggeredByIndices.Add(layer);
	}

	public bool HasAnyTriggeredBy()
	{
		return this.TriggeredByIndices.Count > 0;
	}

	public void SetTriggeredValueFlag(byte index)
	{
		if (this.TriggeredValues.Contains(index))
		{
			this.TriggeredValues.Remove(index);
			return;
		}
		this.TriggeredValues.Add(index);
	}

	public bool CheckIsTriggered()
	{
		if (this.UseOrForMultipleTriggers)
		{
			using (List<byte>.Enumerator enumerator = this.TriggeredByIndices.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int num = (int)enumerator.Current;
					if (!this.TriggeredValues.Contains((byte)num))
					{
						return true;
					}
				}
			}
			return false;
		}
		using (List<byte>.Enumerator enumerator = this.TriggeredByIndices.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				int num2 = (int)enumerator.Current;
				if (!this.TriggeredValues.Contains((byte)num2))
				{
					return false;
				}
			}
		}
		return true;
	}

	public string TriggerDisplay()
	{
		if (this.TriggeredByIndices.Count != 0 && this.TriggersIndices.Count == 0)
		{
			return string.Format("[0000FF]{0}[-]", string.Join<byte>(",", this.TriggeredByIndices));
		}
		if (this.TriggersIndices.Count != 0 && this.TriggeredByIndices.Count == 0)
		{
			return string.Format("[FF0000]{0}[-][0000FF]{1}[-]", string.Join<byte>(",", this.TriggersIndices), string.Join<byte>(",", this.TriggeredByIndices));
		}
		return string.Format("[FF0000]{0}[-] | [0000FF]{1}[-]", string.Join<byte>(",", this.TriggersIndices), string.Join<byte>(",", this.TriggeredByIndices));
	}

	public Vector3i ToWorldPos()
	{
		if (this.Chunk != null)
		{
			return new Vector3i(this.Chunk.X * 16, this.Chunk.Y * 256, this.Chunk.Z * 16) + this.LocalChunkPos;
		}
		return Vector3i.zero;
	}

	public void TriggerUpdated(List<BlockChangeInfo> _blockChanges)
	{
		BlockValue block = this.Chunk.GetBlock(this.LocalChunkPos);
		if (_blockChanges != null)
		{
			block.Block.OnTriggerChanged(this, this.Chunk, this.ToWorldPos(), block, _blockChanges);
			return;
		}
		block.Block.OnTriggerChanged(this, this.Chunk, this.ToWorldPos(), block);
	}

	public void OnTriggered(EntityPlayer _player, World _world, int index, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy = null)
	{
		this.SetTriggeredValueFlag((byte)index);
		if (this.CheckIsTriggered())
		{
			BlockValue block = this.Chunk.GetBlock(this.LocalChunkPos);
			block.Block.OnTriggered(_player, _world, this.ToWorldPos(), block, _blockChanges, _triggeredBy);
			this.TriggeredValues.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public const ushort version = 5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ushort currentVersion;

	public Vector3i LocalChunkPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	public PrefabTriggerData TriggerDataOwner;

	public List<byte> TriggersIndices = new List<byte>();

	public List<byte> TriggeredByIndices = new List<byte>();

	public List<byte> TriggeredValues = new List<byte>();

	public bool ExcludeIcon;

	public bool UseOrForMultipleTriggers;

	public bool Unlock;

	public BlockTrigger.TriggeredStates NeedsTriggered;

	public enum TriggeredStates
	{
		NotTriggered,
		NeedsTriggered,
		HasTriggered
	}
}
