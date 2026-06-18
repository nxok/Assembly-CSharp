using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Utf8Json;

namespace Webserver.WebAPI
{
	public static class JsonCommons
	{
		public static void WriteVector3I(ref JsonWriter _writer, Vector3i _position)
		{
			_writer.WriteRaw(JsonCommons.jsonKeyPositionX);
			_writer.WriteInt32(_position.x);
			_writer.WriteRaw(JsonCommons.jsonKeyPositionY);
			_writer.WriteInt32(_position.y);
			_writer.WriteRaw(JsonCommons.jsonKeyPositionZ);
			_writer.WriteInt32(_position.z);
			_writer.WriteEndObject();
		}

		public static void WriteVector3(ref JsonWriter _writer, Vector3 _position)
		{
			_writer.WriteRaw(JsonCommons.jsonKeyPositionX);
			_writer.WriteSingle(_position.x);
			_writer.WriteRaw(JsonCommons.jsonKeyPositionY);
			_writer.WriteSingle(_position.y);
			_writer.WriteRaw(JsonCommons.jsonKeyPositionZ);
			_writer.WriteSingle(_position.z);
			_writer.WriteEndObject();
		}

		public static void WriteVector2I(ref JsonWriter _writer, Vector2i _position)
		{
			_writer.WriteRaw(JsonCommons.jsonKeyPositionX);
			_writer.WriteInt32(_position.x);
			_writer.WriteRaw(JsonCommons.jsonKeyPositionY);
			_writer.WriteInt32(_position.y);
			_writer.WriteEndObject();
		}

		public static void WriteVector2(ref JsonWriter _writer, Vector2 _position)
		{
			_writer.WriteRaw(JsonCommons.jsonKeyPositionX);
			_writer.WriteSingle(_position.x);
			_writer.WriteRaw(JsonCommons.jsonKeyPositionY);
			_writer.WriteSingle(_position.y);
			_writer.WriteEndObject();
		}

		public static void WriteGameTimeObject(ref JsonWriter _writer, int _days, int _hours, int _minutes)
		{
			_writer.WriteRaw(JsonCommons.jsonKeyDays);
			_writer.WriteInt32(_days);
			_writer.WriteRaw(JsonCommons.jsonKeyHours);
			_writer.WriteInt32(_hours);
			_writer.WriteRaw(JsonCommons.jsonKeyMinutes);
			_writer.WriteInt32(_minutes);
			_writer.WriteEndObject();
		}

		public static void WritePlatformUserIdentifier(ref JsonWriter _writer, PlatformUserIdentifierAbs _userIdentifier)
		{
			if (_userIdentifier == null)
			{
				_writer.WriteNull();
				return;
			}
			_writer.WriteRaw(JsonCommons.jsonKeyCombinedString);
			_writer.WriteString(_userIdentifier.CombinedString);
			_writer.WriteRaw(JsonCommons.jsonKeyPlatformId);
			_writer.WriteString(_userIdentifier.PlatformIdentifierString);
			_writer.WriteRaw(JsonCommons.jsonKeyUserId);
			_writer.WriteString(_userIdentifier.ReadablePlatformUserIdentifier);
			_writer.WriteEndObject();
		}

		public static bool TryReadPlatformUserIdentifier(IDictionary<string, object> _jsonInput, out PlatformUserIdentifierAbs _userIdentifier)
		{
			string text;
			if (JsonCommons.TryGetJsonField(_jsonInput, "combinedString", out text))
			{
				_userIdentifier = PlatformUserIdentifierAbs.FromCombinedString(text, false);
				if (_userIdentifier != null)
				{
					return true;
				}
			}
			string text2;
			if (!JsonCommons.TryGetJsonField(_jsonInput, "platformId", out text2))
			{
				_userIdentifier = null;
				return false;
			}
			string text3;
			if (!JsonCommons.TryGetJsonField(_jsonInput, "userId", out text3))
			{
				_userIdentifier = null;
				return false;
			}
			_userIdentifier = PlatformUserIdentifierAbs.FromPlatformAndId(text2, text3, false);
			return _userIdentifier != null;
		}

		public static void WriteDateTime(ref JsonWriter _writer, DateTime _dateTime)
		{
			_writer.WriteString(_dateTime.ToString("o"));
		}

		public static bool TryReadDateTime(IDictionary<string, object> _jsonInput, string _fieldName, out DateTime _result)
		{
			_result = default(DateTime);
			string text;
			return JsonCommons.TryGetJsonField(_jsonInput, _fieldName, out text) && DateTime.TryParse(text, null, DateTimeStyles.RoundtripKind, out _result);
		}

		public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out int _value)
		{
			_value = 0;
			object obj;
			if (!_jsonObject.TryGetValue(_fieldName, out obj))
			{
				return false;
			}
			if (obj is double)
			{
				double num = (double)obj;
				bool flag;
				try
				{
					_value = (int)num;
					flag = true;
				}
				catch (Exception)
				{
					flag = false;
				}
				return flag;
			}
			return false;
		}

		public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out double _value)
		{
			_value = 0.0;
			object obj;
			if (!_jsonObject.TryGetValue(_fieldName, out obj))
			{
				return false;
			}
			if (obj is double)
			{
				double num = (double)obj;
				bool flag;
				try
				{
					_value = num;
					flag = true;
				}
				catch (Exception)
				{
					flag = false;
				}
				return flag;
			}
			return false;
		}

		public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out string _value)
		{
			_value = null;
			object obj;
			if (!_jsonObject.TryGetValue(_fieldName, out obj))
			{
				return false;
			}
			string text = obj as string;
			if (text == null)
			{
				return false;
			}
			bool flag;
			try
			{
				_value = text;
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public static bool TryGetJsonField(IDictionary<string, object> _jsonObject, string _fieldName, out IDictionary<string, object> _value)
		{
			_value = null;
			object obj;
			if (!_jsonObject.TryGetValue(_fieldName, out obj))
			{
				return false;
			}
			IDictionary<string, object> dictionary = obj as IDictionary<string, object>;
			if (dictionary == null)
			{
				return false;
			}
			bool flag;
			try
			{
				_value = dictionary;
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPositionX = JsonWriter.GetEncodedPropertyNameWithBeginObject("x");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPositionY = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("y");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPositionZ = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("z");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyDays = JsonWriter.GetEncodedPropertyNameWithBeginObject("days");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyHours = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("hours");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyMinutes = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("minutes");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyCombinedString = JsonWriter.GetEncodedPropertyNameWithBeginObject("combinedString");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPlatformId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("platformId");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyUserId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("userId");
	}
}
