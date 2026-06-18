using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BuffValue
{
	public BuffClass BuffClass
	{
		get
		{
			if (this.cachedBuff == null && !BuffManager.Buffs.TryGetValue(this.buffName, out this.cachedBuff))
			{
				Log.Error("Buff Class not found for '{0}'", new object[] { this.buffName });
			}
			return this.cachedBuff;
		}
	}

	public bool Remove
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.buffFlags & BuffValue.BuffFlags.Remove) > BuffValue.BuffFlags.None;
		}
		set
		{
			if (value)
			{
				this.buffFlags |= BuffValue.BuffFlags.Remove;
				return;
			}
			this.buffFlags &= (BuffValue.BuffFlags)251;
		}
	}

	public bool Finished
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.buffFlags & BuffValue.BuffFlags.Finished) > BuffValue.BuffFlags.None;
		}
		set
		{
			if (value)
			{
				this.buffFlags |= BuffValue.BuffFlags.Finished;
				return;
			}
			this.buffFlags &= (BuffValue.BuffFlags)253;
		}
	}

	public bool Started
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.buffFlags & BuffValue.BuffFlags.Started) > BuffValue.BuffFlags.None;
		}
		set
		{
			if (value)
			{
				this.buffFlags |= BuffValue.BuffFlags.Started;
				return;
			}
			this.buffFlags &= (BuffValue.BuffFlags)254;
		}
	}

	public bool Invalid
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.buffFlags & BuffValue.BuffFlags.Invalid) > BuffValue.BuffFlags.None;
		}
		set
		{
			if (value)
			{
				this.buffFlags |= BuffValue.BuffFlags.Invalid;
				return;
			}
			this.buffFlags &= (BuffValue.BuffFlags)239;
		}
	}

	public bool Update
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.buffFlags & BuffValue.BuffFlags.Update) > BuffValue.BuffFlags.None;
		}
		set
		{
			if (value)
			{
				this.buffFlags |= BuffValue.BuffFlags.Update;
				return;
			}
			this.buffFlags &= (BuffValue.BuffFlags)247;
		}
	}

	public bool Paused
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.buffFlags & BuffValue.BuffFlags.Paused) > BuffValue.BuffFlags.None;
		}
		set
		{
			if (value)
			{
				this.buffFlags |= BuffValue.BuffFlags.Paused;
				return;
			}
			this.buffFlags &= (BuffValue.BuffFlags)223;
		}
	}

	public int StackEffectMultiplier
	{
		get
		{
			return (int)this.stackEffectMultiplier;
		}
		set
		{
			this.stackEffectMultiplier = (byte)Mathf.Clamp(value, 0, 255);
		}
	}

	public float DurationInSeconds
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.durationTicks / 20f;
		}
	}

	public uint DurationInTicks
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.durationTicks;
		}
		set
		{
			this.durationTicks = value;
			this.updateTicks = (ushort)value;
		}
	}

	public void DurationTick()
	{
		this.durationTicks += 1U;
		ushort num = this.updateTicks + 1;
		this.updateTicks = num;
		if ((int)num >= this.BuffClass.UpdateRateTicks)
		{
			this.Update = true;
			this.updateTicks = 0;
		}
	}

	public void DurationTriggerUpdate()
	{
		this.updateTicks = 32767;
	}

	public string BuffName
	{
		get
		{
			return this.buffName;
		}
	}

	public int InstigatorId
	{
		get
		{
			return this.instigatorId;
		}
	}

	public Vector3i InstigatorPos
	{
		get
		{
			return this.instigatorPos;
		}
	}

	public BuffValue()
	{
	}

	public BuffValue(string _buffEffectGroupId, Vector3i _instigatorPos, int _instigatorId = -1, BuffClass _buffClass = null)
	{
		this.buffName = _buffEffectGroupId;
		this.stackEffectMultiplier = 1;
		this.durationTicks = 0U;
		this.instigatorId = _instigatorId;
		this.buffFlags = BuffValue.BuffFlags.None;
		this.updateTicks = 0;
		this.instigatorPos = _instigatorPos;
		if (_buffClass == null)
		{
			this.cacheBuffClassPointer();
			return;
		}
		this.cachedBuff = _buffClass;
	}

	public void ClearBuffClassLink()
	{
		this.cachedBuff = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cacheBuffClassPointer()
	{
		if (!BuffManager.Buffs.TryGetValue(this.buffName, out this.cachedBuff))
		{
			this.Remove = true;
		}
	}

	public void Tick()
	{
		BuffClass buffClass = this.BuffClass;
		if (buffClass != null)
		{
			buffClass.Tick(this);
			return;
		}
		this.Remove = true;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(this.buffName);
		_bw.Write(this.stackEffectMultiplier);
		_bw.Write(this.durationTicks);
		_bw.Write(this.instigatorId);
		_bw.Write((byte)this.buffFlags);
		_bw.Write(this.updateTicks);
		StreamUtils.Write(_bw, this.instigatorPos);
	}

	public void Read(BinaryReader _br, int _version)
	{
		if (_version < 2)
		{
			int num = _br.ReadInt32();
			using (Dictionary<string, BuffClass>.KeyCollection.Enumerator enumerator = BuffManager.Buffs.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string text = enumerator.Current;
					if (text.GetHashCode() == num)
					{
						this.buffName = BuffManager.Buffs[text].Name;
						break;
					}
				}
				goto IL_0070;
			}
		}
		this.buffName = _br.ReadString().ToLower();
		IL_0070:
		this.stackEffectMultiplier = _br.ReadByte();
		this.durationTicks = _br.ReadUInt32();
		this.instigatorId = _br.ReadInt32();
		this.buffFlags = (BuffValue.BuffFlags)_br.ReadByte();
		if (_version == 0)
		{
			this.updateTicks = (ushort)_br.ReadByte();
		}
		else
		{
			this.updateTicks = _br.ReadUInt16();
		}
		if (_version >= 3)
		{
			this.instigatorPos = StreamUtils.ReadVector3i(_br);
		}
		this.cacheBuffClassPointer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffClass cachedBuff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte stackEffectMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint durationTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int instigatorId;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue.BuffFlags buffFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort updateTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i instigatorPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum BuffFlags : byte
	{
		None,
		Started,
		Finished,
		Remove = 4,
		Update = 8,
		Invalid = 16,
		Paused = 32
	}
}
