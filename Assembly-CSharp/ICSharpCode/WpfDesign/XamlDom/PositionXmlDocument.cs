using System;
using System.Reflection;
using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom
{
	public class PositionXmlDocument : XmlDocument
	{
		public PositionXmlDocument()
		{
			this.setImplementation();
		}

		[PublicizedFrom(EAccessModifier.ProtectedInternal)]
		public PositionXmlDocument(XmlImplementation _imp)
			: base(_imp)
		{
			this.setImplementation();
		}

		public PositionXmlDocument(XmlNameTable _nt)
			: base(_nt)
		{
			this.setImplementation();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setImplementation()
		{
			if (this.implementationFieldInfo == null)
			{
				this.implementationFieldInfo = typeof(XmlDocument).GetField("implementation", BindingFlags.Instance | BindingFlags.NonPublic);
			}
			this.implementationFieldInfo.SetValue(this, new PositionXmlImplementation());
		}

		public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
		{
			return new PositionXmlElement(prefix, localName, namespaceURI, this, this.lineInfo);
		}

		public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
		{
			return new PositionXmlAttribute(prefix, localName, namespaceURI, this, this.lineInfo);
		}

		public override XmlCDataSection CreateCDataSection(string data)
		{
			return new PositionXmlCDataSection(data, this, this.lineInfo);
		}

		public override XmlComment CreateComment(string data)
		{
			return new PositionXmlComment(data, this, this.lineInfo);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override XmlAttribute CreateDefaultAttribute(string prefix, string localName, string namespaceURI)
		{
			return new PositionXmlAttribute(prefix, localName, namespaceURI, this, this.lineInfo);
		}

		public override XmlDocumentFragment CreateDocumentFragment()
		{
			return new PositionXmlDocumentFragment(this, this.lineInfo);
		}

		public override XmlDocumentType CreateDocumentType(string name, string publicId, string systemId, string internalSubset)
		{
			return new PositionXmlDocumentType(name, publicId, systemId, internalSubset, this, this.lineInfo);
		}

		public override XmlEntityReference CreateEntityReference(string name)
		{
			return new PositionXmlEntityReference(name, this, this.lineInfo);
		}

		public override XmlNode CreateNode(string nodeTypeString, string name, string namespaceURI)
		{
			Console.WriteLine("CREATING NODE1: " + name);
			return base.CreateNode(nodeTypeString, name, namespaceURI);
		}

		public override XmlNode CreateNode(XmlNodeType type, string name, string namespaceURI)
		{
			Console.WriteLine("CREATING NODE2: " + name);
			return base.CreateNode(type, name, namespaceURI);
		}

		public override XmlNode CreateNode(XmlNodeType type, string prefix, string name, string namespaceURI)
		{
			Console.WriteLine("CREATING NODE3: " + name);
			return base.CreateNode(type, prefix, name, namespaceURI);
		}

		public override XmlProcessingInstruction CreateProcessingInstruction(string target, string data)
		{
			return new PositionXmlProcessingInstruction(target, data, this, this.lineInfo);
		}

		public override XmlSignificantWhitespace CreateSignificantWhitespace(string text)
		{
			return new PositionXmlSignificantWhitespace(text, this, this.lineInfo);
		}

		public override XmlText CreateTextNode(string text)
		{
			return new PositionXmlText(text, this, this.lineInfo);
		}

		public override XmlWhitespace CreateWhitespace(string text)
		{
			return new PositionXmlWhitespace(text, this, this.lineInfo);
		}

		public override XmlDeclaration CreateXmlDeclaration(string version, string encoding, string standalone)
		{
			return new PositionXmlDeclaration(version, encoding, standalone, this, this.lineInfo);
		}

		public override void Load(XmlReader reader)
		{
			this.lineInfo = reader as IXmlLineInfo;
			try
			{
				base.Load(reader);
			}
			finally
			{
				this.lineInfo = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IXmlLineInfo lineInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public FieldInfo implementationFieldInfo;
	}
}
