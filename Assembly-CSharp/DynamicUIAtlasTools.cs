using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

public static class DynamicUIAtlasTools
{
	public static void Prebake(int _elementWidth, int _elementHeight, int _paddingSize, Dictionary<string, Texture2D> _textures, string _outputfile)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int count = _textures.Count;
		Vector2i vector2i = DynamicUIAtlasTools.findMinimumAtlasSize(_elementWidth, _elementHeight, _paddingSize, count, new Vector2i(1, 1));
		int num = vector2i.x / (_elementWidth + _paddingSize);
		int num2 = vector2i.y / (_elementHeight + _paddingSize);
		Texture2D texture2D = new Texture2D(vector2i.x, vector2i.y, 5, false);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("{0}\t{1}\t{2}\n", _elementWidth, _elementHeight, _paddingSize);
		texture2D.FillTexture(new Color(0f, 0f, 0f, 0f), false, false);
		int num3 = 0;
		foreach (KeyValuePair<string, Texture2D> keyValuePair in _textures)
		{
			if (keyValuePair.Value.width != _elementWidth || keyValuePair.Value.height != _elementHeight)
			{
				Log.Warning(string.Format("Sprite {0} has incorrect resolution {1}*{2}, expected {3}*{4}", new object[]
				{
					keyValuePair.Key,
					keyValuePair.Value.width,
					keyValuePair.Value.height,
					_elementWidth,
					_elementHeight
				}));
			}
			else
			{
				int num4 = num3 / num;
				int num5 = num3 % num * (_elementWidth + _paddingSize);
				int num6 = num4 * (_elementHeight + _paddingSize);
				keyValuePair.Value.CopyTexturePart(Vector2i.zero, texture2D, new Vector2i(num5, vector2i.y - (_elementHeight + _paddingSize) - num6), new Vector2i(_elementWidth, _elementHeight));
				stringBuilder.AppendFormat("{0}\t{1}\t{2}\n", keyValuePair.Key, num5, num6);
				num3++;
			}
		}
		SdFile.WriteAllText(_outputfile + ".txt", stringBuilder.ToString());
		SdFile.WriteAllBytes(_outputfile + ".png", ImageConversion.EncodeToPNG(texture2D));
		stopwatch.Stop();
		Log.Out(string.Concat(new string[]
		{
			"Creating UIAtlas (",
			vector2i.x.ToString(),
			"*",
			vector2i.y.ToString(),
			") took ",
			stopwatch.ElapsedMilliseconds.ToString(),
			" ms"
		}));
	}

	public static void AddSprites(int _elementWidth, int _elementHeight, int _paddingSize, Dictionary<string, Texture2D> _textures, ref Texture2D _tex, List<UISpriteData> _spriteList)
	{
		int count = _spriteList.Count;
		Vector2i vector2i = new Vector2i(_tex.width, _tex.height);
		int num = vector2i.x / (_elementWidth + _paddingSize);
		int num2 = vector2i.y / (_elementHeight + _paddingSize);
		int num3 = ((count > 0) ? ((count - 1) / num + 1) : 0);
		int num4 = count % num;
		Dictionary<string, UISpriteData> dictionary = new Dictionary<string, UISpriteData>();
		for (int i = 0; i < _spriteList.Count; i++)
		{
			UISpriteData uispriteData = _spriteList[i];
			dictionary.Add(uispriteData.name, uispriteData);
		}
		int num5 = count;
		foreach (KeyValuePair<string, Texture2D> keyValuePair in _textures)
		{
			if (!dictionary.ContainsKey(keyValuePair.Key))
			{
				num5++;
			}
		}
		Vector2i vector2i2 = DynamicUIAtlasTools.findMinimumAtlasSize(_elementWidth, _elementHeight, _paddingSize, num5, vector2i);
		int num6 = vector2i2.x / (_elementWidth + _paddingSize);
		int num7 = vector2i2.y / (_elementHeight + _paddingSize);
		if (vector2i2 != vector2i)
		{
			Texture2D texture2D = new Texture2D(vector2i2.x, vector2i2.y, _tex.format, false);
			texture2D.FillTexture(new Color(0f, 0f, 0f, 0f), false, false);
			_tex.CopyTexturePart(Vector2i.zero, texture2D, new Vector2i(0, vector2i2.y - vector2i.y), vector2i);
			Object.Destroy(_tex);
			_tex = texture2D;
		}
		else
		{
			Log.Out("Atlas got enough free room to fit icons. Old {0} icons, {1}x{2}, now {3} icons", new object[] { count, vector2i.x, vector2i.y, num5 });
		}
		int num8 = 0;
		int j = ((num3 > 1) ? num : num4);
		foreach (KeyValuePair<string, Texture2D> keyValuePair2 in _textures)
		{
			if (keyValuePair2.Value.width != _elementWidth || keyValuePair2.Value.height != _elementHeight)
			{
				Log.Warning(string.Format("Sprite {0} has incorrect resolution {1}*{2}, expected {3}*{4}", new object[]
				{
					keyValuePair2.Key,
					keyValuePair2.Value.width,
					keyValuePair2.Value.height,
					_elementWidth,
					_elementHeight
				}));
			}
			else
			{
				int num9;
				int num10;
				if (dictionary.ContainsKey(keyValuePair2.Key))
				{
					num9 = dictionary[keyValuePair2.Key].x;
					num10 = dictionary[keyValuePair2.Key].y;
				}
				else
				{
					while (j >= num6)
					{
						num8++;
						if (num8 + 1 < num3)
						{
							j = num;
						}
						else if (num8 + 1 == num3)
						{
							j = num4;
						}
						else
						{
							j = 0;
						}
					}
					num9 = j * (_elementWidth + _paddingSize);
					num10 = num8 * (_elementHeight + _paddingSize);
					UISpriteData uispriteData2 = new UISpriteData();
					uispriteData2.name = keyValuePair2.Key;
					uispriteData2.SetRect(num9, num10, _elementWidth, _elementHeight);
					dictionary.Add(keyValuePair2.Key, uispriteData2);
					j++;
				}
				keyValuePair2.Value.CopyTexturePart(Vector2i.zero, _tex, new Vector2i(num9, vector2i2.y - (_elementHeight + _paddingSize) - num10), new Vector2i(_elementWidth, _elementHeight));
			}
		}
		_spriteList.Clear();
		foreach (KeyValuePair<string, UISpriteData> keyValuePair3 in dictionary)
		{
			_spriteList.Add(keyValuePair3.Value);
		}
	}

	public static int GetMaxElementsWithSize(int _atlasWidthMax, int _atlasHeightMax, int _elementWidth, int _elementHeight, int _paddingSize)
	{
		int num = _atlasWidthMax / (_elementWidth + _paddingSize);
		int num2 = _atlasHeightMax / (_elementHeight + _paddingSize);
		return num * num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2i findMinimumAtlasSize(int _elementWidth, int _elementHeight, int _paddingSize, int _elementCount, Vector2i _startSize)
	{
		Vector2i vector2i = _startSize;
		_elementHeight += _paddingSize;
		_elementWidth += _paddingSize;
		int num = vector2i.x / _elementWidth;
		int num2 = vector2i.y / _elementHeight;
		while (num * num2 < _elementCount)
		{
			if (vector2i.x <= vector2i.y)
			{
				vector2i.x *= 2;
			}
			else
			{
				vector2i.y *= 2;
			}
			num = vector2i.x / _elementWidth;
			num2 = vector2i.y / _elementHeight;
		}
		return vector2i;
	}

	public static bool ReadPrebakedAtlasDescriptor(string _resourceName, out List<UISpriteData> _sprites, out int _elementWidth, out int _elementHeight, out int _paddingSize)
	{
		DataLoader.DataPathIdentifier dataPathIdentifier = DataLoader.ParseDataPathIdentifier(_resourceName + ".txt");
		TextAsset textAsset = DataLoader.LoadAsset<TextAsset>(dataPathIdentifier, false);
		_sprites = null;
		_elementWidth = 0;
		_elementHeight = 0;
		_paddingSize = 1;
		if (textAsset == null)
		{
			Log.Error("Could not open prebaked atlas from {0} (missing sprite list)", new object[] { _resourceName });
			return false;
		}
		_sprites = new List<UISpriteData>();
		try
		{
			using (StringReader stringReader = new StringReader(textAsset.text))
			{
				int num = 1;
				string text = stringReader.ReadLine();
				if (text != null)
				{
					string[] array = text.Split('\t', StringSplitOptions.None);
					if (array.Length < 2)
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor at line 1: {1}", new object[] { _resourceName, text });
						return false;
					}
					if (!int.TryParse(array[0], out _elementWidth))
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line 1 parameter 1: {1}", new object[] { _resourceName, text });
						return false;
					}
					if (!int.TryParse(array[1], out _elementHeight))
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line 1 parameter 2: {1}", new object[] { _resourceName, text });
						return false;
					}
					if (array.Length > 2 && !int.TryParse(array[2], out _paddingSize))
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line 1 parameter 3: {1}", new object[] { _resourceName, text });
						return false;
					}
				}
				while ((text = stringReader.ReadLine()) != null)
				{
					num++;
					string[] array = text.Split('\t', StringSplitOptions.None);
					if (array.Length != 3)
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor at line {2}: {1}", new object[] { _resourceName, text, num });
						return false;
					}
					string text2 = array[0];
					int num2;
					if (!int.TryParse(array[1], out num2))
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line {2} parameter 2: {1}", new object[] { _resourceName, text, num });
						return false;
					}
					int num3;
					if (!int.TryParse(array[2], out num3))
					{
						Log.Error("Prebaked atlas from {0}: Invalid descriptor value at line {2} parameter 3: {1}", new object[] { _resourceName, text, num });
						return false;
					}
					UISpriteData uispriteData = new UISpriteData();
					uispriteData.name = text2;
					uispriteData.SetRect(num2, num3, _elementWidth, _elementHeight);
					_sprites.Add(uispriteData);
				}
			}
		}
		finally
		{
			DataLoader.UnloadAsset(dataPathIdentifier, textAsset);
		}
		return true;
	}

	public static bool ReadPrebakedAtlasTexture(string _resourceName, out Texture2D _tex)
	{
		_tex = DataLoader.LoadAsset<Texture2D>(_resourceName + ".png", false);
		if (_tex == null)
		{
			Log.Error("Could not open prebaked atlas from {0} (missing texture)", new object[] { _resourceName });
			return false;
		}
		return true;
	}

	public static void UnloadTex(string _resourceName, Texture2D _tex)
	{
		DataLoader.UnloadAsset(_resourceName, _tex);
	}
}
