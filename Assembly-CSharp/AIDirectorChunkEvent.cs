using System;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorChunkEvent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorChunkEvent()
	{
	}

	public AIDirectorChunkEvent(EnumAIDirectorChunkEvent _type, Vector3i _position, float _value, float _duration)
	{
		this.EventType = _type;
		this.Position = _position;
		this.Value = _value;
		this.Duration = _duration;
	}

	public void Write(BinaryWriter _stream)
	{
		_stream.Write(2);
		_stream.Write(this.Position.x);
		_stream.Write(this.Position.y);
		_stream.Write(this.Position.z);
		_stream.Write(this.Value);
		_stream.Write((byte)this.EventType);
		_stream.Write(this.Duration);
	}

	public static AIDirectorChunkEvent Read(BinaryReader _stream)
	{
		int num = _stream.ReadInt32();
		AIDirectorChunkEvent aidirectorChunkEvent = new AIDirectorChunkEvent();
		aidirectorChunkEvent.Position.x = _stream.ReadInt32();
		aidirectorChunkEvent.Position.y = _stream.ReadInt32();
		aidirectorChunkEvent.Position.z = _stream.ReadInt32();
		aidirectorChunkEvent.Value = _stream.ReadSingle();
		aidirectorChunkEvent.EventType = (EnumAIDirectorChunkEvent)_stream.ReadByte();
		if (num >= 2)
		{
			aidirectorChunkEvent.Duration = _stream.ReadSingle();
		}
		else
		{
			_stream.ReadUInt64();
		}
		return aidirectorChunkEvent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 2;

	public EnumAIDirectorChunkEvent EventType;

	public Vector3i Position;

	public float Value;

	public float Duration;
}
