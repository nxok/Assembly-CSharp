using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace XMLEditing
{
	public class BlockNodeMap : IEnumerable<KeyValuePair<string, BlockNode>>, IEnumerable
	{
		public IEnumerator<KeyValuePair<string, BlockNode>> GetEnumerator()
		{
			return this.blockNodes.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.blockNodes.GetEnumerator();
		}

		public bool TryGetValue(string targetName, out BlockNode blockNode)
		{
			return this.blockNodes.TryGetValue(targetName, out blockNode);
		}

		public int Count
		{
			get
			{
				return this.blockNodes.Count;
			}
		}

		public void PopulateFromFile(string blocksFilePath)
		{
			XDocument xdocument = XMLUtils.LoadXDocument(blocksFilePath);
			this.root = xdocument.Root;
			this.Refresh();
		}

		public void PopulateFromRoot(XElement root)
		{
			this.root = root;
			this.Refresh();
		}

		public void Refresh()
		{
			if (this.root == null)
			{
				Debug.LogError("Refresh failed: root element is null. This may occur if you have not called one of the PopulateFrom[...] methods prior to calling Refresh. Otherwise there may be an error in the source xml.");
				return;
			}
			if (!this.root.HasElements)
			{
				Debug.LogError("Refresh failed: root element has no child elements.");
				return;
			}
			this.blockNodes.Clear();
			foreach (XElement xelement in this.root.Elements(XNames.block))
			{
				string attribute = xelement.GetAttribute(XNames.name);
				BlockNode blockNode = new BlockNode
				{
					Name = attribute,
					Element = xelement
				};
				this.blockNodes[attribute] = blockNode;
			}
			foreach (BlockNode blockNode2 in this.blockNodes.Values)
			{
				foreach (XElement xelement2 in blockNode2.Element.Elements(XNames.property))
				{
					string text = xelement2.GetAttribute(XNames.name);
					if (text == "Extends")
					{
						string attribute2 = xelement2.GetAttribute(XNames.value);
						BlockNode blockNode3;
						if (this.blockNodes.TryGetValue(attribute2, out blockNode3))
						{
							blockNode3.AddChild(blockNode2);
							foreach (string text2 in xelement2.GetAttribute(XNames.param1).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
							{
								BlockNode.ElementInfo elementInfo;
								if (!blockNode2.ElementInfos.TryGetValue(text2, out elementInfo))
								{
									elementInfo = new BlockNode.ElementInfo();
									blockNode2.ElementInfos[text2] = elementInfo;
								}
								elementInfo.CanInherit = false;
							}
							BlockNode.ElementInfo elementInfo2 = new BlockNode.ElementInfo();
							elementInfo2.CanInherit = false;
							elementInfo2.Element = xelement2;
							elementInfo2.IsClass = false;
							blockNode2.ElementInfos["Extends"] = elementInfo2;
						}
						else
						{
							Debug.LogError(string.Concat(new string[] { "Failed to find parent BlockNode \"", attribute2, "\" for block \"", blockNode2.Name, "\"" }));
						}
					}
					else
					{
						bool flag = false;
						if (string.IsNullOrWhiteSpace(text))
						{
							string attribute3 = xelement2.GetAttribute(XNames.class_);
							if (string.IsNullOrWhiteSpace(attribute3))
							{
								continue;
							}
							flag = true;
							text = attribute3;
						}
						BlockNode.ElementInfo elementInfo3;
						if (!blockNode2.ElementInfos.TryGetValue(text, out elementInfo3))
						{
							elementInfo3 = new BlockNode.ElementInfo();
							elementInfo3.CanInherit = text != "CreativeMode";
							blockNode2.ElementInfos[text] = elementInfo3;
						}
						elementInfo3.Element = xelement2;
						elementInfo3.IsClass = flag;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, BlockNode> blockNodes = new Dictionary<string, BlockNode>(StringComparer.OrdinalIgnoreCase);

		[PublicizedFrom(EAccessModifier.Private)]
		public XElement root;
	}
}
