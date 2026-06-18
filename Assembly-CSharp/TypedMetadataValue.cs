using System;
using System.IO;

public class TypedMetadataValue
{
	public static TypedMetadataValue.TypeTag StringToTag(string str)
	{
		TypedMetadataValue.TypeTag typeTag;
		if (!(str == "float"))
		{
			if (!(str == "int"))
			{
				if (!(str == "string"))
				{
					typeTag = TypedMetadataValue.TypeTag.None;
				}
				else
				{
					typeTag = TypedMetadataValue.TypeTag.String;
				}
			}
			else
			{
				typeTag = TypedMetadataValue.TypeTag.Integer;
			}
		}
		else
		{
			typeTag = TypedMetadataValue.TypeTag.Float;
		}
		return typeTag;
	}

	public TypedMetadataValue.TypeTag GetTypeTag()
	{
		return this.typeTag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TypedMetadataValue(object val, TypedMetadataValue.TypeTag tag)
	{
		this.typeTag = tag;
		this.value = val;
	}

	public static bool TryCreate(object val, TypedMetadataValue.TypeTag tag, out TypedMetadataValue result)
	{
		if (!TypedMetadataValue.ValueMatchesTag(val, tag))
		{
			result = null;
			return false;
		}
		result = new TypedMetadataValue(val, tag);
		return true;
	}

	public object GetValue()
	{
		return this.value;
	}

	public bool SetValue(object val)
	{
		if (TypedMetadataValue.ValueMatchesTag(val, this.typeTag))
		{
			this.value = val;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ValueMatchesTag(object val, TypedMetadataValue.TypeTag tag)
	{
		if (val == null)
		{
			return false;
		}
		switch (tag)
		{
		case TypedMetadataValue.TypeTag.Float:
			return val is float;
		case TypedMetadataValue.TypeTag.Integer:
			return val is int;
		case TypedMetadataValue.TypeTag.String:
			return val is string;
		default:
			return false;
		}
	}

	public static void Write(TypedMetadataValue tmv, BinaryWriter writer)
	{
		if (tmv == null)
		{
			return;
		}
		writer.Write((int)tmv.typeTag);
		switch (tmv.typeTag)
		{
		case TypedMetadataValue.TypeTag.Float:
			writer.Write((float)tmv.value);
			return;
		case TypedMetadataValue.TypeTag.Integer:
			writer.Write((int)tmv.value);
			return;
		case TypedMetadataValue.TypeTag.String:
			writer.Write((string)tmv.value);
			return;
		default:
			return;
		}
	}

	public static TypedMetadataValue Read(BinaryReader reader)
	{
		object obj = null;
		TypedMetadataValue.TypeTag typeTag = (TypedMetadataValue.TypeTag)reader.ReadInt32();
		switch (typeTag)
		{
		case TypedMetadataValue.TypeTag.Float:
			obj = reader.ReadSingle();
			break;
		case TypedMetadataValue.TypeTag.Integer:
			obj = reader.ReadInt32();
			break;
		case TypedMetadataValue.TypeTag.String:
			obj = reader.ReadString();
			break;
		}
		return new TypedMetadataValue(obj, typeTag);
	}

	public override bool Equals(object other)
	{
		TypedMetadataValue typedMetadataValue = other as TypedMetadataValue;
		return typedMetadataValue != null && this.typeTag == typedMetadataValue.typeTag && this.value.Equals(typedMetadataValue.value);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public TypedMetadataValue Clone()
	{
		TypedMetadataValue.TypeTag typeTag = this.typeTag;
		return new TypedMetadataValue(this.value, this.typeTag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TypedMetadataValue.TypeTag typeTag;

	[PublicizedFrom(EAccessModifier.Private)]
	public object value;

	public enum TypeTag
	{
		None,
		Float,
		Integer,
		String
	}
}
