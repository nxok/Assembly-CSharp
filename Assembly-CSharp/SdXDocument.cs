using System;
using System.IO;
using System.Xml.Linq;

public static class SdXDocument
{
	public static XDocument Load(string filename)
	{
		XDocument xdocument;
		using (Stream stream = SdFile.OpenRead(filename))
		{
			xdocument = XDocument.Load(stream);
		}
		return xdocument;
	}
}
