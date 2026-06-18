using System;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public struct RemoteWorldInfo
{
	public RemoteWorldInfo(string _gameName, string _worldName, VersionInformation _gameVersion, long _saveSize)
	{
		this.GameName = _gameName;
		this.WorldName = _worldName;
		this.GameVersion = _gameVersion;
		this.SaveSize = _saveSize;
	}

	public static bool TryRead(string _filePath, out RemoteWorldInfo _remoteWorldInfo)
	{
		if (!SdFile.Exists(_filePath))
		{
			_remoteWorldInfo = default(RemoteWorldInfo);
			return false;
		}
		bool flag;
		try
		{
			XElement root = SdXDocument.Load(_filePath).Root;
			string text;
			string text2;
			string text3;
			VersionInformation versionInformation;
			string text4;
			long num;
			if (root == null)
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + _filePath + "\". Could not find root node.");
				_remoteWorldInfo = default(RemoteWorldInfo);
				flag = false;
			}
			else if (!root.TryGetAttribute("gameName", out text))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + _filePath + "\". Could not find gameName attribute.");
				_remoteWorldInfo = default(RemoteWorldInfo);
				flag = false;
			}
			else if (!root.TryGetAttribute("worldName", out text2))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + _filePath + "\". Could not find worldName attribute.");
				_remoteWorldInfo = default(RemoteWorldInfo);
				flag = false;
			}
			else if (!root.TryGetAttribute("gameVersion", out text3))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + _filePath + "\". Could not find gameVersion attribute.");
				_remoteWorldInfo = default(RemoteWorldInfo);
				flag = false;
			}
			else if (!VersionInformation.TryParseSerializedString(text3, out versionInformation))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + _filePath + "\". Failed to parse gameVersion value.");
				_remoteWorldInfo = default(RemoteWorldInfo);
				flag = false;
			}
			else if (!root.TryGetAttribute("saveSize", out text4))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + _filePath + "\". Could not find saveSize attribute.");
				_remoteWorldInfo = default(RemoteWorldInfo);
				flag = false;
			}
			else if (!long.TryParse(text4, out num))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + _filePath + "\". Failed to parse saveSize value.");
				_remoteWorldInfo = default(RemoteWorldInfo);
				flag = false;
			}
			else
			{
				_remoteWorldInfo = new RemoteWorldInfo(text, text2, versionInformation, num);
				flag = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("Failed to read RemoteWorldInfo at path \"{0}\". Failed with exception: \n\n{1}", _filePath, ex));
			_remoteWorldInfo = default(RemoteWorldInfo);
			flag = false;
		}
		return flag;
	}

	public void Write(string _filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement xmlElement = xmlDocument.AddXmlElement("RemoteWorldInfo");
		xmlElement.SetAttrib("gameName", this.GameName);
		xmlElement.SetAttrib("worldName", this.WorldName);
		xmlElement.SetAttrib("gameVersion", this.GameVersion.SerializableString);
		xmlElement.SetAttrib("saveSize", this.SaveSize.ToString());
		xmlDocument.SdSave(_filePath);
	}

	public const string FileNameString = "RemoteWorldInfo.xml";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameNameString = "gameName";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string WorldNameString = "worldName";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameVersionString = "gameVersion";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SaveSizeString = "saveSize";

	public readonly string GameName;

	public readonly string WorldName;

	public readonly VersionInformation GameVersion;

	public readonly long SaveSize;
}
