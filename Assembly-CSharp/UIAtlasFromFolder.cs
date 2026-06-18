using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class UIAtlasFromFolder
{
	public static IEnumerator CreateUiAtlasFromFolder(string _folder, Shader _shader, Action<UIAtlas> _resultHandler)
	{
		MicroStopwatch coRoutineSw = new MicroStopwatch(true);
		string[] files = null;
		try
		{
			files = SdDirectory.GetFiles(_folder);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		if (files != null)
		{
			Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
			int num;
			for (int i = 0; i < files.Length; i = num + 1)
			{
				string text = files[i];
				try
				{
					if (text.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
					{
						Texture2D texture2D = new Texture2D(1, 1, 5, false);
						texture2D.name = "ModMgrAtlas";
						if (ImageConversion.LoadImage(texture2D, SdFile.ReadAllBytes(text)))
						{
							string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
							icons[fileNameWithoutExtension] = texture2D;
						}
						else
						{
							Object.Destroy(texture2D);
						}
					}
					else if (text.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
					{
						Texture2D texture2D2 = TGALoader.LoadTGA(text, false);
						texture2D2.name = "ModMgrAtlas";
						string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text);
						icons[fileNameWithoutExtension2] = texture2D2;
					}
				}
				catch (Exception ex2)
				{
					Log.Error("Adding file " + text + " failed:");
					Log.Exception(ex2);
				}
				if (coRoutineSw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					coRoutineSw.ResetAndRestart();
				}
				num = i;
			}
			if (icons.Count == 0)
			{
				yield break;
			}
			Dictionary<string, UISpriteData> dictionary = UIAtlasFromFolder.loadSpriteSettings(_folder + "/settings.xml");
			UIAtlas uiatlas = UIAtlasFromFolder.createUiAtlasFromTextures(Path.GetFileName(_folder), icons, dictionary, _shader, _folder + "/..", true);
			try
			{
				foreach (KeyValuePair<string, Texture2D> keyValuePair in icons)
				{
					Object.Destroy(keyValuePair.Value);
				}
			}
			catch (Exception ex3)
			{
				Log.Exception(ex3);
			}
			_resultHandler(uiatlas);
			icons = null;
		}
		yield break;
	}

	public static UIAtlas createUiAtlasFromTextures(string _name, Dictionary<string, Texture2D> _textures, Dictionary<string, UISpriteData> _customSpriteSettings, Shader _shader, string _dumpFolder = null, bool _applyAndUnreadable = true)
	{
		string[] array = new string[_textures.Count];
		Texture2D[] array2 = new Texture2D[_textures.Count];
		int num = 0;
		foreach (KeyValuePair<string, Texture2D> keyValuePair in _textures)
		{
			array[num] = keyValuePair.Key;
			array2[num] = keyValuePair.Value;
			num++;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		Texture2D texture2D = new Texture2D(1, 1, 5, false);
		Rect[] array3 = texture2D.PackTextures(array2, 2, 8192);
		if (!string.IsNullOrEmpty(_dumpFolder) && GameUtils.GetLaunchArgument("exportcustomatlases") != null)
		{
			SdFile.WriteAllBytes(_dumpFolder + "/" + _name + "_atlas.png", ImageConversion.EncodeToPNG(texture2D));
		}
		if (_applyAndUnreadable)
		{
			texture2D.Compress(true);
			texture2D.Apply(false, true);
		}
		Log.Out(string.Format("UIAtlas {0}: Pack took {1} us", _name, microStopwatch.ElapsedMicroseconds));
		int width = texture2D.width;
		int height = texture2D.height;
		List<UISpriteData> list = new List<UISpriteData>(_textures.Count);
		for (int i = 0; i < _textures.Count; i++)
		{
			UISpriteData uispriteData = new UISpriteData
			{
				name = array[i]
			};
			Rect rect = NGUIMath.ConvertToPixels(array3[i], width, height, true);
			uispriteData.SetRect((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
			UISpriteData uispriteData2;
			if (_customSpriteSettings != null && _customSpriteSettings.TryGetValue(array[i], out uispriteData2))
			{
				uispriteData.borderTop = uispriteData2.borderTop;
				uispriteData.borderBottom = uispriteData2.borderBottom;
				uispriteData.borderLeft = uispriteData2.borderLeft;
				uispriteData.borderRight = uispriteData2.borderRight;
			}
			list.Add(uispriteData);
		}
		UIAtlas uiatlas = new GameObject(_name).AddComponent<UIAtlas>();
		uiatlas.spriteList = list;
		uiatlas.spriteMaterial = new Material(_shader);
		uiatlas.pixelSize = 1f;
		uiatlas.spriteMaterial.mainTexture = texture2D;
		return uiatlas;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, UISpriteData> loadSpriteSettings(string _filename)
	{
		if (!SdFile.Exists(_filename))
		{
			return null;
		}
		Dictionary<string, UISpriteData> dictionary = new CaseInsensitiveStringDictionary<UISpriteData>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.SdLoad(_filename);
		foreach (object obj in xmlDocument.DocumentElement.ChildNodes)
		{
			XmlNode xmlNode = (XmlNode)obj;
			if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name.EqualsCaseInsensitive("sprite"))
			{
				XmlElement xmlElement = (XmlElement)xmlNode;
				if (xmlElement.HasAttribute("name"))
				{
					string attribute = xmlElement.GetAttribute("name");
					UISpriteData uispriteData = new UISpriteData();
					if (xmlElement.HasAttribute("borderleft"))
					{
						uispriteData.borderLeft = int.Parse(xmlElement.GetAttribute("borderleft"));
					}
					if (xmlElement.HasAttribute("borderright"))
					{
						uispriteData.borderRight = int.Parse(xmlElement.GetAttribute("borderright"));
					}
					if (xmlElement.HasAttribute("bordertop"))
					{
						uispriteData.borderTop = int.Parse(xmlElement.GetAttribute("bordertop"));
					}
					if (xmlElement.HasAttribute("borderbottom"))
					{
						uispriteData.borderBottom = int.Parse(xmlElement.GetAttribute("borderbottom"));
					}
					dictionary.Add(attribute, uispriteData);
				}
			}
		}
		return dictionary;
	}
}
