using System;
using System.IO;

public struct AnimParamData
{
	public AnimParamData(int _nameHash, AnimParamData.ValueTypes _valueType, bool _value)
	{
		this.NameHash = _nameHash;
		this.ValueType = _valueType;
		this.FloatValue = 0f;
		this.IntValue = (_value ? 1 : 0);
	}

	public AnimParamData(int _nameHash, AnimParamData.ValueTypes _valueType, float _value)
	{
		this.NameHash = _nameHash;
		this.ValueType = _valueType;
		this.FloatValue = _value;
		this.IntValue = 0;
	}

	public AnimParamData(int _nameHash, AnimParamData.ValueTypes _valueType, int _value)
	{
		this.NameHash = _nameHash;
		this.ValueType = _valueType;
		this.FloatValue = 0f;
		this.IntValue = _value;
	}

	public static AnimParamData CreateFromBinary(BinaryReader _br)
	{
		int num = _br.ReadInt32();
		AnimParamData.ValueTypes valueTypes = (AnimParamData.ValueTypes)_br.ReadByte();
		switch (valueTypes)
		{
		case AnimParamData.ValueTypes.Bool:
		case AnimParamData.ValueTypes.Trigger:
			return new AnimParamData(num, valueTypes, _br.ReadBoolean());
		case AnimParamData.ValueTypes.Float:
		case AnimParamData.ValueTypes.DataFloat:
			return new AnimParamData(num, valueTypes, _br.ReadSingle());
		case AnimParamData.ValueTypes.Int:
			return new AnimParamData(num, valueTypes, _br.ReadInt32());
		default:
		{
			string text = "Invalid Value Type: ";
			byte b = (byte)valueTypes;
			throw new InvalidDataException(text + b.ToString());
		}
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(this.NameHash);
		_bw.Write((byte)this.ValueType);
		switch (this.ValueType)
		{
		case AnimParamData.ValueTypes.Bool:
		case AnimParamData.ValueTypes.Trigger:
			_bw.Write(this.IntValue != 0);
			return;
		case AnimParamData.ValueTypes.Float:
		case AnimParamData.ValueTypes.DataFloat:
			_bw.Write(this.FloatValue);
			return;
		case AnimParamData.ValueTypes.Int:
			_bw.Write(this.IntValue);
			return;
		default:
			return;
		}
	}

	public string ToString(AvatarController _controller)
	{
		string parameterName = _controller.GetParameterName(this.NameHash);
		return string.Format("{0} {1}, {2}, f{3}, i{4}", new object[] { parameterName, this.NameHash, this.ValueType, this.FloatValue, this.IntValue });
	}

	public readonly int NameHash;

	public readonly AnimParamData.ValueTypes ValueType;

	public readonly float FloatValue;

	public readonly int IntValue;

	public enum ValueTypes : byte
	{
		Bool,
		Trigger,
		Float,
		Int,
		DataFloat
	}
}
