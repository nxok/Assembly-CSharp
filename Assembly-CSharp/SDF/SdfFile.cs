using System;
using System.Collections.Generic;
using System.IO;

namespace SDF
{
	public class SdfFile
	{
		public SdfFile(string path)
		{
			this.data = new SdfData();
			this.filePath = path;
		}

		public void Load()
		{
			try
			{
				this.valuesChanged = false;
				if (SdFile.Exists(this.filePath))
				{
					using (Stream stream = SdFile.Open(this.filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						this.data.Nodes = SdfReader.Read(stream);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error opening SDF file: " + ex.Message);
			}
		}

		public void Save()
		{
			try
			{
				if (this.valuesChanged)
				{
					if (!SdDirectory.Exists(Path.GetDirectoryName(this.filePath)))
					{
						SdDirectory.CreateDirectory(Path.GetDirectoryName(this.filePath));
					}
					using (Stream stream = SdFile.Open(this.filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
					{
						SdfWriter.Write(stream, this.data.Nodes);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error opening SDF file:");
				Log.Exception(ex);
			}
		}

		public void Set(string name, int val)
		{
			this.data.Add(new SdfInt(name, val));
			this.valuesChanged = true;
		}

		public void Set(string name, float val)
		{
			this.data.Add(new SdfFloat(name, val));
			this.valuesChanged = true;
		}

		public void Set(string name, string val)
		{
			this.Set(name, val, false);
		}

		public void Set(string name, string val, bool isBinary)
		{
			if (!isBinary)
			{
				this.data.Add(new SdfString(name, val));
			}
			else
			{
				this.data.Add(new SdfBinary(name, val));
			}
			this.valuesChanged = true;
		}

		public void Set(string name, byte[] byteArray)
		{
			this.data.Add(new SdfByteArray(name, byteArray));
		}

		public void Set(string name, bool val)
		{
			this.data.Add(new SdfBool(name, val));
			this.valuesChanged = true;
		}

		public float? GetFloat(string name)
		{
			return this.data.GetFloat(name);
		}

		public int? GetInt(string name)
		{
			return this.data.GetInt(name);
		}

		public string GetString(string name)
		{
			return this.GetString(name, false);
		}

		public string GetString(string name, bool isBinary)
		{
			if (!isBinary)
			{
				return this.data.GetString(name);
			}
			return Utils.FromBase64(this.data.GetString(name));
		}

		public byte[] GetByteArray(string name)
		{
			return this.data.GetByteArray(name);
		}

		public bool? GetBool(string name)
		{
			return this.data.GetBool(name);
		}

		public void Remove(string name)
		{
			this.data.Remove(name);
			this.valuesChanged = true;
		}

		public string[] GetKeys()
		{
			string[] array = new string[this.data.Nodes.Count];
			this.data.Nodes.CopyKeysTo(array);
			return array;
		}

		public string[] GetStoredGamePrefs()
		{
			string[] array = new string[this.data.Nodes.Count];
			this.data.Nodes.CopyKeysTo(array);
			return array;
		}

		public void CopyFrom(SdfFile other)
		{
			foreach (KeyValuePair<string, SdfTag> keyValuePair in other.data.Nodes)
			{
				string text;
				SdfTag sdfTag;
				keyValuePair.Deconstruct(out text, out sdfTag);
				string text2 = text;
				SdfTag sdfTag2 = sdfTag;
				this.data.Nodes[text2] = sdfTag2;
			}
			this.valuesChanged = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SdfData data;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string filePath;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool valuesChanged;
	}
}
