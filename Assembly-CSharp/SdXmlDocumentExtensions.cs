using System;
using System.IO;
using System.Xml;

public static class SdXmlDocumentExtensions
{
	public static void SdLoad(this XmlDocument xmlDoc, string filename)
	{
		using (Stream stream = SdFile.OpenRead(filename))
		{
			xmlDoc.Load(stream);
		}
	}

	public static void SdSave(this XmlDocument xmlDoc, string filename)
	{
		using (Stream stream = SdFile.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			xmlDoc.Save(stream);
		}
	}
}
