using System;
using System.IO;

public class LiveStats
{
	public LiveStats(int _maxLiveLevel, int _oversaturationLevel)
	{
		this.maxLiveLevel = _maxLiveLevel;
		this.oversaturationLevel = _oversaturationLevel;
		this.Reset();
	}

	public void Copy(LiveStats other)
	{
		this.liveLevel = other.liveLevel;
		this.maxLiveLevel = other.maxLiveLevel;
		this.oversaturationLevel = other.oversaturationLevel;
		this.saturationLevel = other.saturationLevel;
		this.exhaustionLevel = other.exhaustionLevel;
		this.timer = other.timer;
	}

	public void Reset()
	{
		this.timer = 0;
		this.liveLevel = this.maxLiveLevel;
		this.saturationLevel = 0f;
		this.exhaustionLevel = 0f;
	}

	public void AddStats(int _addLifeValue)
	{
		this.liveLevel += _addLifeValue;
		if (this.liveLevel > this.maxLiveLevel)
		{
			this.saturationLevel += (float)(this.liveLevel - this.maxLiveLevel);
			this.liveLevel = this.maxLiveLevel;
			this.saturationLevel = Utils.FastMin(this.saturationLevel, (float)this.oversaturationLevel);
		}
		if (this.liveLevel < 0)
		{
			this.liveLevel = 0;
		}
	}

	public void OnUpdate(EntityPlayer _entityPlayer)
	{
		while (this.exhaustionLevel > 1f)
		{
			this.exhaustionLevel -= 1f;
			if (this.saturationLevel > 0f)
			{
				this.saturationLevel = Math.Max(this.saturationLevel - 1f, 0f);
			}
			else
			{
				this.liveLevel = Math.Max(this.liveLevel - 1, 0);
			}
		}
	}

	public void Read(BinaryReader _br)
	{
		this.liveLevel = (int)_br.ReadInt16();
		this.timer = (int)_br.ReadInt16();
		this.saturationLevel = _br.ReadSingle();
		this.exhaustionLevel = _br.ReadSingle();
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((ushort)this.liveLevel);
		_bw.Write((ushort)this.timer);
		_bw.Write(this.saturationLevel);
		_bw.Write(this.exhaustionLevel);
	}

	public int GetLifeLevel()
	{
		return this.liveLevel;
	}

	public int GetMaxLifeLevel()
	{
		return this.maxLiveLevel;
	}

	public void SetLifeLevel(int _value)
	{
		this.liveLevel = _value;
	}

	public bool IsFilledUp()
	{
		return this.liveLevel >= this.maxLiveLevel;
	}

	public void AddExhaustion(float _v)
	{
		this.exhaustionLevel = Math.Min(this.exhaustionLevel + _v, 40f);
	}

	public float GetSaturationLevel()
	{
		return this.saturationLevel;
	}

	public void SetSaturationLevel(float _level)
	{
		this.saturationLevel = _level;
	}

	public float GetExhaustionLevel()
	{
		return this.exhaustionLevel;
	}

	public void SetExhaustionLevel(float _level)
	{
		this.exhaustionLevel = _level;
	}

	public float GetLifeLevelFraction()
	{
		return (float)this.liveLevel / (float)this.maxLiveLevel;
	}

	public LiveStats Clone()
	{
		LiveStats liveStats = new LiveStats(this.maxLiveLevel, this.oversaturationLevel);
		liveStats.SetSaturationLevel(this.saturationLevel);
		liveStats.SetExhaustionLevel(this.exhaustionLevel);
		liveStats.SetLifeLevel(this.liveLevel);
		return liveStats;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int liveLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxLiveLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oversaturationLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float saturationLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float exhaustionLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int timer;
}
