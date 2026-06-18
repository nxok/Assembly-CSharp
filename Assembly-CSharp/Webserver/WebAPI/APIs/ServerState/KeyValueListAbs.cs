using System;
using System.Net;
using UnityEngine.Profiling;
using Utf8Json;

namespace Webserver.WebAPI.APIs.ServerState
{
	public abstract class KeyValueListAbs : AbsRestApi
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public KeyValueListAbs(string _listName)
			: base(null)
		{
			this.buildSampler = CustomSampler.Create("JSON_" + _listName + "_BuildSampler", false);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.EnsureCapacity(this.largestBuffer);
			jsonWriter.WriteBeginArray();
			bool flag = true;
			this.iterateList(ref jsonWriter, ref flag);
			jsonWriter.WriteEndArray();
			int num = jsonWriter.CurrentOffset + 128;
			if (num > this.largestBuffer)
			{
				this.largestBuffer = num;
			}
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeKeyType(ref JsonWriter _writer, ref bool _first, string _key, string _type)
		{
			if (!_first)
			{
				_writer.WriteValueSeparator();
			}
			_first = false;
			_writer.WriteRaw(KeyValueListAbs.keyName);
			_writer.WriteString(_key);
			_writer.WriteRaw(KeyValueListAbs.keyType);
			_writer.WriteString(_type);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeInt(ref JsonWriter _writer, ref bool _first, string _key, int _value)
		{
			this.writeKeyType(ref _writer, ref _first, _key, "int");
			_writer.WriteRaw(KeyValueListAbs.keyValue);
			_writer.WriteInt32(_value);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, int _value)
		{
			this.writeInt(ref _writer, ref _first, _key, _value);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, int _value, int? _default)
		{
			this.writeInt(ref _writer, ref _first, _key, _value);
			_writer.WriteRaw(KeyValueListAbs.keyDefault);
			if (_default != null)
			{
				_writer.WriteInt32(_default.Value);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeFloat(ref JsonWriter _writer, ref bool _first, string _key, float _value)
		{
			this.writeKeyType(ref _writer, ref _first, _key, "float");
			_writer.WriteRaw(KeyValueListAbs.keyValue);
			_writer.WriteSingle(_value);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, float _value)
		{
			this.writeFloat(ref _writer, ref _first, _key, _value);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, float _value, float? _default)
		{
			this.writeFloat(ref _writer, ref _first, _key, _value);
			_writer.WriteRaw(KeyValueListAbs.keyDefault);
			if (_default != null)
			{
				_writer.WriteSingle(_default.Value);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeBool(ref JsonWriter _writer, ref bool _first, string _key, bool _value)
		{
			this.writeKeyType(ref _writer, ref _first, _key, "bool");
			_writer.WriteRaw(KeyValueListAbs.keyValue);
			_writer.WriteBoolean(_value);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, bool _value)
		{
			this.writeBool(ref _writer, ref _first, _key, _value);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, bool _value, bool? _default)
		{
			this.writeBool(ref _writer, ref _first, _key, _value);
			_writer.WriteRaw(KeyValueListAbs.keyDefault);
			if (_default != null)
			{
				_writer.WriteBoolean(_default.Value);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeString(ref JsonWriter _writer, ref bool _first, string _key, string _value)
		{
			this.writeKeyType(ref _writer, ref _first, _key, "string");
			_writer.WriteRaw(KeyValueListAbs.keyValue);
			_writer.WriteString(_value);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, string _value)
		{
			this.writeString(ref _writer, ref _first, _key, _value);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void addItem(ref JsonWriter _writer, ref bool _first, string _key, string _value, string _default)
		{
			this.writeString(ref _writer, ref _first, _key, _value);
			_writer.WriteRaw(KeyValueListAbs.keyDefault);
			_writer.WriteString(_default);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void iterateList(ref JsonWriter _writer, ref bool _first);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CustomSampler buildSampler;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] keyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] keyType = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("type");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] keyValue = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("value");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] keyDefault = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("default");

		[PublicizedFrom(EAccessModifier.Private)]
		public int largestBuffer;
	}
}
