using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public sealed class SaveDataPrefsFile : ISaveDataPrefs
{
	public SaveDataPrefsFile(string filePath)
	{
		foreach (PrefType prefType in EnumUtils.Values<PrefType>())
		{
			if (!SaveDataPrefsFile.PrefTypeMapping.ContainsKey(prefType))
			{
				throw new KeyNotFoundException(string.Format("Expected {0} to have key '{1}'.", "PrefTypeMapping", prefType));
			}
		}
		this.m_storageFilePath = filePath;
		this.m_storage = new Dictionary<string, SaveDataPrefsFile.Pref>();
		this.Load();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void Escape(TextWriter writer, ReadOnlySpan<char> raw, bool ignoreSeparator)
	{
		bool flag = false;
		ReadOnlySpan<char> readOnlySpan = raw;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = (char)(*readOnlySpan[i]);
			if ((!ignoreSeparator || c != '=') && SaveDataPrefsFile.EscapeMapping.ContainsKey(c))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			writer.Write(raw);
			return;
		}
		readOnlySpan = raw;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c2 = (char)(*readOnlySpan[i]);
			char c3;
			if ((ignoreSeparator && c2 == '=') || !SaveDataPrefsFile.EscapeMapping.TryGetValue(c2, out c3))
			{
				writer.Write(c2);
			}
			else
			{
				writer.Write('\\');
				writer.Write(c3);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void Unescape(StringBuilder builder, ReadOnlySpan<char> escaped)
	{
		if (escaped.IndexOf('\\') < 0)
		{
			builder.Append(escaped);
			return;
		}
		bool flag = false;
		int num = -1;
		ReadOnlySpan<char> readOnlySpan = escaped;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = (char)(*readOnlySpan[i]);
			num++;
			if (flag)
			{
				flag = false;
				char c2;
				if (!SaveDataPrefsFile.UnescapeMapping.TryGetValue(c, out c2))
				{
					Log.Warning(string.Format("Unexpected character after escape prefix at offset {0} (will be taken as-is): {1}", num, c));
					builder.Append(c);
				}
				builder.Append(c2);
			}
			else if (c == '\\')
			{
				flag = true;
			}
			else
			{
				builder.Append(c);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static int IndexOfFirstUnescapedSeparator(ReadOnlySpan<char> search)
	{
		bool flag = false;
		int num = -1;
		ReadOnlySpan<char> readOnlySpan = search;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = (char)(*readOnlySpan[i]);
			num++;
			if (flag)
			{
				flag = false;
			}
			else if (c == '\\')
			{
				flag = true;
			}
			else if (c == '=')
			{
				return num;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveInternal()
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			if (this.m_dirty)
			{
				try
				{
					string directoryName = Path.GetDirectoryName(this.m_storageFilePath);
					if (!SdDirectory.Exists(directoryName))
					{
						SdDirectory.CreateDirectory(directoryName);
					}
					using (StreamWriter streamWriter = SdFile.CreateText(this.m_storageFilePath))
					{
						int num = 0;
						foreach (KeyValuePair<string, SaveDataPrefsFile.Pref> keyValuePair in this.m_storage)
						{
							string text;
							SaveDataPrefsFile.Pref pref;
							keyValuePair.Deconstruct(out text, out pref);
							string text2 = text;
							SaveDataPrefsFile.Pref pref2 = pref;
							string text3;
							if (!pref2.TryToString(out text3))
							{
								Log.Out(string.Format("[{0}] Failed to convert pref '{1}' of type {2} to a string representation.", "SaveDataPrefsFile", text2, pref2.Type));
							}
							else
							{
								SaveDataPrefsFile.Escape(streamWriter, text2, false);
								streamWriter.Write('=');
								SaveDataPrefsFile.Escape(streamWriter, text3, true);
								streamWriter.WriteLine();
								num++;
							}
						}
						this.m_dirty = false;
						Log.Out(string.Format("[{0}] Saved {1} player pref(s) to: {2}", "SaveDataPrefsFile", num, this.m_storageFilePath));
					}
				}
				catch (IOException ex)
				{
					Log.Error("[SaveDataPrefsFile] Failed to Save: " + ex.Message);
					Log.Exception(ex);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadInternal()
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			this.m_storage.Clear();
			if (SdFile.Exists(this.m_storageFilePath))
			{
				try
				{
					using (StreamReader streamReader = SdFile.OpenText(this.m_storageFilePath))
					{
						StringBuilder stringBuilder = new StringBuilder();
						int num = 0;
						int num2 = 0;
						for (;;)
						{
							string text = streamReader.ReadLine();
							if (text == null)
							{
								break;
							}
							num2++;
							ReadOnlySpan<char> readOnlySpan = text;
							int num3 = SaveDataPrefsFile.IndexOfFirstUnescapedSeparator(readOnlySpan);
							if (num3 < 0)
							{
								Log.Error(string.Format("[{0}] Skipping line {1} since is missing unescaped separator '{2}'. Contents: {3}", new object[] { "SaveDataPrefsFile", num2, '=', text }));
							}
							else
							{
								StringBuilder stringBuilder2 = stringBuilder;
								ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
								SaveDataPrefsFile.Unescape(stringBuilder2, readOnlySpan2.Slice(0, num3));
								string text2 = stringBuilder.ToString();
								stringBuilder.Clear();
								StringBuilder stringBuilder3 = stringBuilder;
								readOnlySpan2 = readOnlySpan;
								int num4 = num3 + 1;
								SaveDataPrefsFile.Unescape(stringBuilder3, readOnlySpan2.Slice(num4, readOnlySpan2.Length - num4));
								string text3 = stringBuilder.ToString();
								stringBuilder.Clear();
								SaveDataPrefsFile.Pref pref;
								if (!SaveDataPrefsFile.Pref.TryParse(text3, out pref))
								{
									Log.Error("[SaveDataPrefsFile] Failed to parse pref '" + text2 + "' with string representation: " + text3);
								}
								else
								{
									this.m_storage[text2] = pref;
									num++;
								}
							}
						}
						Log.Out(string.Format("[{0}] Loaded {1} player pref(s) from: {2}", "SaveDataPrefsFile", num, this.m_storageFilePath));
					}
					return;
				}
				catch (IOException ex)
				{
					Log.Error("[SaveDataPrefsFile] Failed to Load: " + ex.Message);
					Log.Exception(ex);
					return;
				}
			}
			Log.Out("[SaveDataPrefsFile] Using empty player prefs, as none exists at: " + this.m_storageFilePath);
		}
	}

	public float GetFloat(string key, float defaultValue)
	{
		object storageLock = this.m_storageLock;
		float num;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			float num2;
			num = ((this.m_storage.TryGetValue(key, out pref) && pref.TryGet(out num2)) ? num2 : defaultValue);
		}
		return num;
	}

	public void SetFloat(string key, float value)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			if (this.m_storage.TryGetValue(key, out pref))
			{
				float num;
				if (!pref.TryGet(out num) || num != value)
				{
					pref.Set(value);
					this.m_dirty = true;
				}
			}
			else
			{
				pref = new SaveDataPrefsFile.Pref(value);
				this.m_storage[key] = pref;
				this.m_dirty = true;
			}
		}
	}

	public int GetInt(string key, int defaultValue)
	{
		object storageLock = this.m_storageLock;
		int num;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			int num2;
			num = ((this.m_storage.TryGetValue(key, out pref) && pref.TryGet(out num2)) ? num2 : defaultValue);
		}
		return num;
	}

	public void SetInt(string key, int value)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			if (this.m_storage.TryGetValue(key, out pref))
			{
				int num;
				if (!pref.TryGet(out num) || num != value)
				{
					pref.Set(value);
					this.m_dirty = true;
				}
			}
			else
			{
				pref = new SaveDataPrefsFile.Pref(value);
				this.m_storage[key] = pref;
				this.m_dirty = true;
			}
		}
	}

	public string GetString(string key, string defaultValue)
	{
		object storageLock = this.m_storageLock;
		string text;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			string text2;
			text = ((this.m_storage.TryGetValue(key, out pref) && pref.TryGet(out text2)) ? text2 : defaultValue);
		}
		return text;
	}

	public void SetString(string key, string value)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			if (this.m_storage.TryGetValue(key, out pref))
			{
				string text;
				if (!pref.TryGet(out text) || !(text == value))
				{
					pref.Set(value);
					this.m_dirty = true;
				}
			}
			else
			{
				pref = new SaveDataPrefsFile.Pref(value);
				this.m_storage[key] = pref;
				this.m_dirty = true;
			}
		}
	}

	public bool HasKey(string key)
	{
		object storageLock = this.m_storageLock;
		bool flag2;
		lock (storageLock)
		{
			flag2 = this.m_storage.ContainsKey(key);
		}
		return flag2;
	}

	public void DeleteKey(string key)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			this.m_storage.Remove(key);
		}
	}

	public void DeleteAll()
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			this.m_storage.Clear();
		}
	}

	public void Save()
	{
		this.SaveInternal();
	}

	public void Load()
	{
		this.LoadInternal();
	}

	public bool CanLoad
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, char> EscapeMapping = new Dictionary<char, char>
	{
		{ '\0', '0' },
		{ '\r', 'r' },
		{ '\n', 'n' },
		{ '=', '=' },
		{ '\\', '\\' }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, char> UnescapeMapping = SaveDataPrefsFile.EscapeMapping.ToDictionary((KeyValuePair<char, char> pair) => pair.Value, (KeyValuePair<char, char> pair) => pair.Key);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<PrefType, char> PrefTypeMapping = new Dictionary<PrefType, char>
	{
		{
			PrefType.Float,
			'F'
		},
		{
			PrefType.Int,
			'I'
		},
		{
			PrefType.String,
			'S'
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, PrefType> PrefTypeUnmapping = SaveDataPrefsFile.PrefTypeMapping.ToDictionary((KeyValuePair<PrefType, char> kv) => kv.Value, (KeyValuePair<PrefType, char> kv) => kv.Key);

	[PublicizedFrom(EAccessModifier.Private)]
	public const char KeyValueSeparator = '=';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char EscapePrefix = '\\';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char PrefTypeSeparator = ':';

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_storageFilePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SaveDataPrefsFile.Pref> m_storage;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_storageLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_dirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public class Pref
	{
		public Pref(float value)
		{
			this.Set(value);
		}

		public Pref(int value)
		{
			this.Set(value);
		}

		public Pref(string value)
		{
			this.Set(value);
		}

		public PrefType Type
		{
			get
			{
				return this.m_type;
			}
		}

		public bool TryGet(out float value)
		{
			if (this.m_type != PrefType.Float)
			{
				value = 0f;
				return false;
			}
			value = this.m_values.Float;
			return true;
		}

		public bool TryGet(out int value)
		{
			if (this.m_type != PrefType.Int)
			{
				value = 0;
				return false;
			}
			value = this.m_values.Int;
			return true;
		}

		public bool TryGet(out string value)
		{
			if (this.m_type != PrefType.String)
			{
				value = null;
				return false;
			}
			value = this.m_refs.String;
			return true;
		}

		public void Set(float value)
		{
			this.m_type = PrefType.Float;
			this.m_values.Float = value;
			this.m_refs.String = null;
		}

		public void Set(int value)
		{
			this.m_type = PrefType.Int;
			this.m_values.Int = value;
			this.m_refs.String = null;
		}

		public void Set(string value)
		{
			this.m_type = PrefType.String;
			this.m_refs.String = value;
			this.m_values.Int = 0;
		}

		public bool TryToString(out string stringRepresentation)
		{
			char c;
			if (!SaveDataPrefsFile.PrefTypeMapping.TryGetValue(this.m_type, out c))
			{
				Log.Warning(string.Format("[{0}] No char mapping for pref type '{1}'.", "SaveDataPrefsFile", this.m_type));
				stringRepresentation = null;
				return false;
			}
			switch (this.m_type)
			{
			case PrefType.Float:
				stringRepresentation = string.Format("{0}{1}{2:R}", c, ':', this.m_values.Float);
				return true;
			case PrefType.Int:
				stringRepresentation = string.Format("{0}{1}{2}", c, ':', this.m_values.Int);
				return true;
			case PrefType.String:
				stringRepresentation = string.Format("{0}{1}{2}", c, ':', this.m_refs.String);
				return true;
			default:
				Log.Error(string.Format("[{0}] Missing to string implementation for '{1}'.", "SaveDataPrefsFile", this.m_type));
				stringRepresentation = null;
				return false;
			}
		}

		public unsafe static bool TryParse(ReadOnlySpan<char> stringRepresentation, out SaveDataPrefsFile.Pref pref)
		{
			if (stringRepresentation.Length < 2 || *stringRepresentation[1] != 58)
			{
				pref = null;
				return false;
			}
			PrefType prefType;
			if (!SaveDataPrefsFile.PrefTypeUnmapping.TryGetValue((char)(*stringRepresentation[0]), out prefType))
			{
				pref = null;
				return false;
			}
			ReadOnlySpan<char> readOnlySpan = stringRepresentation;
			ReadOnlySpan<char> readOnlySpan2 = readOnlySpan.Slice(2, readOnlySpan.Length - 2);
			switch (prefType)
			{
			case PrefType.Float:
			{
				float num;
				if (!float.TryParse(readOnlySpan2, out num))
				{
					pref = null;
					return false;
				}
				pref = new SaveDataPrefsFile.Pref(num);
				return true;
			}
			case PrefType.Int:
			{
				int num2;
				if (!int.TryParse(readOnlySpan2, out num2))
				{
					pref = null;
					return false;
				}
				pref = new SaveDataPrefsFile.Pref(num2);
				return true;
			}
			case PrefType.String:
				pref = new SaveDataPrefsFile.Pref(new string(readOnlySpan2));
				return true;
			default:
				Log.Error(string.Format("[{0}] Missing parse implementation for '{1}'.", "SaveDataPrefsFile", prefType));
				pref = null;
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PrefType m_type;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataPrefsFile.Pref.PrefValues m_values;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataPrefsFile.Pref.PrefRefs m_refs;

		[PublicizedFrom(EAccessModifier.Private)]
		[StructLayout(LayoutKind.Explicit)]
		public struct PrefValues
		{
			[FieldOffset(0)]
			public float Float;

			[FieldOffset(0)]
			public int Int;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[StructLayout(LayoutKind.Explicit)]
		public struct PrefRefs
		{
			[FieldOffset(0)]
			public string String;
		}
	}
}
