using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorChunkData
{
	public float ActivityLevel
	{
		get
		{
			return this.activityLevel;
		}
	}

	public bool IsReady
	{
		get
		{
			return this.cooldownDelay <= 0f;
		}
	}

	public void StartNeighborCooldown(bool _isLong)
	{
		float num = (_isLong ? 720f : 180f);
		this.cooldownDelay = Utils.FastMax(this.cooldownDelay, num);
	}

	public int EventCount
	{
		get
		{
			return this.events.Count;
		}
	}

	public AIDirectorChunkEvent GetEvent(int _index)
	{
		return this.events[_index];
	}

	public void Write(BinaryWriter _stream)
	{
		_stream.Write(2);
		_stream.Write(this.activityLevel);
		_stream.Write(this.events.Count);
		for (int i = 0; i < this.events.Count; i++)
		{
			this.events[i].Write(_stream);
		}
		_stream.Write(this.cooldownDelay);
	}

	public void Read(BinaryReader _stream, int outerVersion)
	{
		int num = _stream.ReadInt32();
		this.activityLevel = _stream.ReadSingle();
		this.events.Clear();
		int num2 = _stream.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			this.events.Add(AIDirectorChunkEvent.Read(_stream));
		}
		if (num >= 2)
		{
			this.cooldownDelay = _stream.ReadSingle();
		}
	}

	public void AddEvent(AIDirectorChunkEvent _chunkEvent)
	{
		int num = this.events.FindIndex((AIDirectorChunkEvent e) => e.Position == _chunkEvent.Position && e.EventType == _chunkEvent.EventType);
		if (num < 0)
		{
			this.events.Add(_chunkEvent);
		}
		else
		{
			AIDirectorChunkEvent aidirectorChunkEvent = this.events[num];
			aidirectorChunkEvent.Value += _chunkEvent.Value;
			aidirectorChunkEvent.Duration = _chunkEvent.Duration;
		}
		this.activityLevel += _chunkEvent.Value;
	}

	public bool Tick(float _elapsed)
	{
		if (this.cooldownDelay > 0f)
		{
			this.cooldownDelay -= _elapsed;
			return true;
		}
		this.DecayEvents(_elapsed);
		return this.EventCount > 0;
	}

	public void DecayEvents(float _elapsed)
	{
		this.activityLevel = 0f;
		int i = 0;
		while (i < this.events.Count)
		{
			AIDirectorChunkEvent aidirectorChunkEvent = this.events[i];
			float num = _elapsed / aidirectorChunkEvent.Duration;
			aidirectorChunkEvent.Value -= aidirectorChunkEvent.Value * num;
			aidirectorChunkEvent.Duration -= _elapsed;
			if (aidirectorChunkEvent.Duration > 0f && aidirectorChunkEvent.Value > 0f)
			{
				this.activityLevel += aidirectorChunkEvent.Value;
				i++;
			}
			else
			{
				this.events.RemoveAt(i);
			}
		}
	}

	public AIDirectorChunkEvent FindBestEventAndReset()
	{
		AIDirectorChunkEvent aidirectorChunkEvent = null;
		if (this.events.Count > 0)
		{
			aidirectorChunkEvent = this.events[0];
			for (int i = 1; i < this.events.Count; i++)
			{
				if (this.events[i].Value > aidirectorChunkEvent.Value)
				{
					aidirectorChunkEvent = this.events[i];
				}
			}
			this.cooldownDelay = 240f;
		}
		this.ClearEvents();
		return aidirectorChunkEvent;
	}

	public void SetLongDelay()
	{
		this.cooldownDelay = 1320f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearEvents()
	{
		this.activityLevel = 0f;
		this.events.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownDelay = 240f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownLongDelay = 1320f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownNeighborDelay = 180f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownNeighborLongDelay = 720f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float activityLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorChunkEvent> events = new List<AIDirectorChunkEvent>();

	public float cooldownDelay;
}
