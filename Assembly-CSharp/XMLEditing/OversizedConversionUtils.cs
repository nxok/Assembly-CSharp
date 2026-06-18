using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace XMLEditing
{
	public static class OversizedConversionUtils
	{
		public static string OversizedConversionTargetsFilePath
		{
			get
			{
				return GameIO.GetGameDir("Data/Config") + "/OversizedConversionTargets.txt";
			}
		}

		public static void AutoApplyOversizedConversion(XElement root)
		{
			string oversizedConversionTargetsFilePath = OversizedConversionUtils.OversizedConversionTargetsFilePath;
			HashSet<string> hashSet = new HashSet<string>();
			if (!SdFile.Exists(oversizedConversionTargetsFilePath))
			{
				Debug.LogWarning("Oversized conversion not applied: File \"" + oversizedConversionTargetsFilePath + "\" does not exist.");
				return;
			}
			foreach (string text in SdFile.ReadLines(oversizedConversionTargetsFilePath))
			{
				hashSet.Add(text);
			}
			if (hashSet.Count == 0)
			{
				return;
			}
			OversizedConversionUtils.ConversionDebugInfo conversionDebugInfo = new OversizedConversionUtils.ConversionDebugInfo();
			OversizedConversionUtils.ApplyOversizedConversion(root, hashSet, conversionDebugInfo);
			Debug.Log("Automatic oversized conversion complete.\n\n" + string.Format("targetNames.Count: {0}\n", hashSet.Count) + conversionDebugInfo.CountsToString());
		}

		public static void ApplyOversizedConversion(XElement root, HashSet<string> targetNames, OversizedConversionUtils.ConversionDebugInfo debugInfo = null)
		{
			BlockNodeMap blockNodeMap = new BlockNodeMap();
			blockNodeMap.PopulateFromRoot(root);
			Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
			XMLUtils.PopulateReplacementMap(dictionary);
			OversizedConversionUtils.FixUndersizedHelpers(blockNodeMap, dictionary);
			HashSet<string> hashSet = new HashSet<string>();
			foreach (string text in targetNames)
			{
				HashSet<string> hashSet2;
				if (dictionary.TryGetValue(text, out hashSet2))
				{
					hashSet.UnionWith(hashSet2);
				}
			}
			HashSet<string> hashSet3 = new HashSet<string>(targetNames);
			hashSet3.UnionWith(hashSet);
			HashSet<BlockNode> targetRootNodes = new HashSet<BlockNode>();
			OversizedConversionUtils.FindMultiBlockDimRootNodes(blockNodeMap, hashSet3, delegate(BlockNode node)
			{
				targetRootNodes.Add(node);
			});
			OversizedConversionUtils.ConvertBlocksToOversized(targetRootNodes, debugInfo);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void FixUndersizedHelpers(BlockNodeMap blockNodes, Dictionary<string, HashSet<string>> replacementMap)
		{
			Bounds bounds;
			bounds..ctor(GameUtils.GetMultiBlockBoundsOffset(Vector3i.one), Vector3i.one);
			foreach (KeyValuePair<string, HashSet<string>> keyValuePair in replacementMap)
			{
				string key = keyValuePair.Key;
				HashSet<string> value = keyValuePair.Value;
				BlockNode blockNode;
				if (!blockNodes.TryGetValue(key, out blockNode))
				{
					Debug.LogWarning("No blockNode found for block: \"" + key + "\"");
				}
				else
				{
					BlockNode blockNode2;
					BlockNode.ElementInfo elementInfo;
					int num;
					Bounds bounds2;
					BlockNode.ElementInfo elementInfo2;
					if (blockNode.TryGetPropertyParent("MultiBlockDim", out blockNode2, out elementInfo, out num) && elementInfo.Element != null)
					{
						Vector3i vector3i = StringParsers.ParseVector3i(elementInfo.Element.GetAttribute(XNames.value), 0, -1, false);
						bounds2..ctor(GameUtils.GetMultiBlockBoundsOffset(vector3i), vector3i);
					}
					else if (blockNode.TryGetPropertyParent("OversizedBounds", out blockNode2, out elementInfo2, out num) && elementInfo2.Element != null)
					{
						bounds2 = StringParsers.ParseBounds(elementInfo2.Element.GetAttribute(XNames.value));
					}
					else
					{
						bounds2 = bounds;
					}
					Bounds bounds3 = bounds;
					foreach (string text in value)
					{
						BlockNode blockNode3;
						BlockNode.ElementInfo elementInfo3;
						if (!blockNodes.TryGetValue(text, out blockNode3))
						{
							Debug.LogWarning("No blockNode found for block: \"" + key + "\"");
						}
						else if (blockNode3.TryGetPropertyParent("MultiBlockDim", out blockNode2, out elementInfo3, out num) && elementInfo3.Element != null)
						{
							Vector3i vector3i2 = StringParsers.ParseVector3i(elementInfo3.Element.GetAttribute(XNames.value), 0, -1, false);
							Bounds bounds4;
							bounds4..ctor(GameUtils.GetMultiBlockBoundsOffset(vector3i2), vector3i2);
							bounds3.Encapsulate(bounds4);
						}
					}
					if (!bounds2.Contains(bounds3.min) || !bounds2.Contains(bounds3.max))
					{
						Vector3 vector = bounds3.center - bounds2.center;
						BlockNode.ElementInfo elementInfo4;
						if ((vector.x != 0f || vector.z != 0f) && blockNode.ElementInfos.TryGetValue("ModelOffset", out elementInfo4) && elementInfo4.Element != null)
						{
							Vector3 vector2 = StringParsers.ParseVector3(elementInfo4.Element.GetAttribute(XNames.value), 0, -1);
							vector2.x -= vector.x;
							vector2.z -= vector.z;
							elementInfo4.Element.SetAttributeValue(XNames.value, string.Format("{0},{1},{2}", vector2.x, vector2.y, vector2.z));
						}
						BlockNode.ElementInfo elementInfo5;
						bool flag = blockNode.ElementInfos.TryGetValue("MultiBlockDim", out elementInfo5);
						BlockNode.ElementInfo elementInfo6;
						if (flag && !elementInfo5.CanInherit && elementInfo5.Element == null && blockNode.Parent.TryGetPropertyParent("MultiBlockDim", out blockNode2, out elementInfo6, out num) && elementInfo6.Element != null)
						{
							Vector3i vector3i3 = StringParsers.ParseVector3i(elementInfo6.Element.GetAttribute(XNames.value), 0, -1, false);
							BlockNode.ElementInfo elementInfo7;
							if (bounds3.size.Equals(vector3i3) && blockNode.ElementInfos.TryGetValue("Extends", out elementInfo7) && elementInfo7.Element != null)
							{
								string text2 = elementInfo7.Element.GetAttribute(XNames.param1);
								List<string> list = text2.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
								if (text2.ContainsCaseInsensitive("MultiBlockDim"))
								{
									list.RemoveAll((string val) => string.Equals(val, "MultiBlockDim", StringComparison.OrdinalIgnoreCase));
									text2 = string.Join(",", list);
									if (string.IsNullOrWhiteSpace(text2))
									{
										elementInfo7.Element.Attribute(XNames.param1).Remove();
									}
									else
									{
										elementInfo7.Element.SetAttributeValue(XNames.param1, text2);
									}
									elementInfo5.CanInherit = true;
									continue;
								}
							}
						}
						XElement xelement = XMLUtils.SetProperty(blockNode.Element, "MultiBlockDim", XNames.value, string.Format("{0},{1},{2}", bounds3.size.x, bounds3.size.y, bounds3.size.z));
						if (flag)
						{
							elementInfo5.Element = xelement;
						}
						else
						{
							elementInfo5 = new BlockNode.ElementInfo
							{
								CanInherit = true,
								Element = xelement,
								IsClass = false
							};
							blockNode.ElementInfos["MultiBlockDim"] = elementInfo5;
						}
					}
				}
			}
		}

		public static void ConvertBlocksToOversized(HashSet<BlockNode> targetRootNodes, OversizedConversionUtils.ConversionDebugInfo debugInfo = null)
		{
			foreach (BlockNode blockNode in targetRootNodes)
			{
				BlockNode.ElementInfo elementInfo;
				if (!blockNode.ElementInfos.TryGetValue("MultiBlockDim", out elementInfo))
				{
					Debug.LogError("targetRootNode does not contain ElementInfo for MultiBlockDim.");
				}
				else
				{
					Vector3i vector3i = StringParsers.ParseVector3i(elementInfo.Element.GetAttribute(XNames.value), 0, -1, false);
					if (vector3i.x <= 1 && vector3i.z <= 1)
					{
						Debug.LogError("Block \"" + blockNode.Name + "\" appears in targetRootNodes despite having XZ dimension of 1.");
					}
					else if (!blockNode.ShapeSupportsModelOffset())
					{
						Debug.LogError("Block \"" + blockNode.Name + "\" has MultiBlockDim but is not a BlockShape type which supports ModelOffset. Conversion of such blocks has not been implemented; please contact engineering if this is required.");
					}
					else
					{
						Vector3 vector;
						int num;
						BlockNode.ModelOffsetType modelOffsetType;
						bool flag = blockNode.TryGetModelOffset(out vector, out num, out modelOffsetType);
						if (debugInfo != null)
						{
							debugInfo.OnOriginModelOffsetTypeCalculated(modelOffsetType);
						}
						if (!flag)
						{
							Debug.LogError("Block \"" + blockNode.Name + "\" has MultiBlockDim but is not a BlockShape type which supports ModelOffset.");
						}
						else
						{
							OversizedConversionUtils.BakeExplicitModelOffsetRecursive(blockNode, debugInfo);
							OversizedConversionUtils.ConvertToOversizedRecursive(blockNode, Vector3.zero, debugInfo);
							if (debugInfo != null)
							{
								debugInfo.OnBaseBlockConverted(blockNode);
							}
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ConvertToOversizedRecursive(BlockNode targetNode, Vector3 inheritedAutoOffset, OversizedConversionUtils.ConversionDebugInfo debugInfo = null)
		{
			Vector3 vector = Vector3.zero;
			bool flag = false;
			bool flag2 = true;
			bool flag3 = false;
			BlockNode.ElementInfo elementInfo;
			if (targetNode.ElementInfos.TryGetValue("MultiBlockDim", out elementInfo))
			{
				if (!elementInfo.CanInherit)
				{
					flag2 = false;
				}
				if (elementInfo.Element != null)
				{
					Vector3i vector3i = StringParsers.ParseVector3i(elementInfo.Element.GetAttribute(XNames.value), 0, -1, false);
					if (vector3i.x <= 1 && vector3i.z <= 1)
					{
						inheritedAutoOffset = Vector3.zero;
						Debug.LogWarning("Oversized conversion child block \"" + targetNode.Name + "\" specifies single-block MultiBlockDim override.");
						flag2 = false;
					}
					else
					{
						inheritedAutoOffset = (vector = GameUtils.GetMultiBlockBoundsOffset(vector3i));
						XMLUtils.SetProperty(targetNode.Element, "OversizedBounds", XNames.value, string.Format("({0},{1},{2}),({3},{4},{5})", new object[] { inheritedAutoOffset.x, inheritedAutoOffset.y, inheritedAutoOffset.z, vector3i.x, vector3i.y, vector3i.z }));
						if (elementInfo.Element.Parent != null)
						{
							elementInfo.Element.Remove();
							elementInfo.Element = null;
						}
						flag3 = true;
						flag = true;
					}
				}
			}
			BlockNode.ElementInfo elementInfo2;
			if (targetNode.ElementInfos.TryGetValue("ModelOffset", out elementInfo2))
			{
				inheritedAutoOffset = (elementInfo2.CanInherit ? inheritedAutoOffset : vector);
				if (elementInfo2.Element != null)
				{
					Vector3 vector2 = StringParsers.ParseVector3(elementInfo2.Element.GetAttribute(XNames.value), 0, -1);
					vector2.x += inheritedAutoOffset.x;
					vector2.z += inheritedAutoOffset.z;
					Vector3 vector3;
					int num;
					BlockNode.ModelOffsetType modelOffsetType;
					if (elementInfo2.CanInherit && targetNode.Parent != null && targetNode.Parent.TryGetModelOffset(out vector3, out num, out modelOffsetType) && modelOffsetType == BlockNode.ModelOffsetType.Explicit && vector3 == vector2)
					{
						elementInfo2.Element.Remove();
						elementInfo2.Element = null;
					}
					else
					{
						XMLUtils.SetProperty(targetNode.Element, "ModelOffset", XNames.value, string.Format("{0},{1},{2}", vector2.x, vector2.y, vector2.z));
					}
					flag = true;
				}
			}
			BlockNode.ElementInfo elementInfo3;
			if (targetNode.ElementInfos.TryGetValue("Extends", out elementInfo3) && elementInfo3.Element != null)
			{
				string text = elementInfo3.Element.GetAttribute(XNames.param1);
				List<string> list = text.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
				bool flag4 = false;
				if (targetNode.Parent != null)
				{
					int num;
					BlockNode blockNode;
					BlockNode.ElementInfo elementInfo4;
					bool flag5 = targetNode.Parent.TryGetPropertyParent("MultiBlockDim", out blockNode, out elementInfo4, out num) && elementInfo4.Element != null;
					bool flag6 = text.ContainsCaseInsensitive("MultiBlockDim");
					if (flag6 && !flag5)
					{
						list.RemoveAll((string val) => string.Equals(val, "MultiBlockDim", StringComparison.OrdinalIgnoreCase));
						flag4 = true;
					}
					else if (!flag6 && flag5 && flag3)
					{
						list.Add("MultiBlockDim");
						flag4 = true;
					}
				}
				if (!flag2 && !text.ContainsCaseInsensitive("OversizedBounds"))
				{
					list.Add("OversizedBounds");
					flag4 = true;
				}
				if (flag4)
				{
					text = string.Join(",", list);
					XMLUtils.SetProperty(targetNode.Element, "Extends", XNames.param1, text);
					flag = true;
				}
			}
			else if (!flag2)
			{
				Debug.LogError("Failed to find extends element on block \"" + targetNode.Name + "\".");
			}
			if (flag && debugInfo != null)
			{
				debugInfo.OnBlockModified(targetNode);
			}
			foreach (BlockNode blockNode2 in targetNode.Children)
			{
				OversizedConversionUtils.ConvertToOversizedRecursive(blockNode2, inheritedAutoOffset, debugInfo);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void BakeExplicitModelOffsetRecursive(BlockNode blockNode, OversizedConversionUtils.ConversionDebugInfo debugInfo = null)
		{
			foreach (BlockNode blockNode2 in blockNode.Children)
			{
				OversizedConversionUtils.BakeExplicitModelOffsetRecursive(blockNode2, debugInfo);
			}
			Vector3 vector;
			int num;
			BlockNode.ModelOffsetType modelOffsetType;
			if (!blockNode.TryGetModelOffset(out vector, out num, out modelOffsetType))
			{
				Debug.LogError("BlockNode " + blockNode.Name + " does not have a valid implict or explicit model offset.");
				return;
			}
			XElement xelement = XMLUtils.SetProperty(blockNode.Element, "ModelOffset", XNames.value, string.Format("{0},{1},{2}", vector.x, vector.y, vector.z));
			BlockNode.ElementInfo elementInfo;
			if (blockNode.ElementInfos.TryGetValue("ModelOffset", out elementInfo))
			{
				elementInfo.Element = xelement;
			}
			else
			{
				elementInfo = new BlockNode.ElementInfo
				{
					CanInherit = true,
					Element = xelement,
					IsClass = false
				};
				blockNode.ElementInfos["ModelOffset"] = elementInfo;
			}
			if (debugInfo != null)
			{
				debugInfo.OnBlockModified(blockNode);
			}
		}

		public static void FindMultiBlockDimRootNodes(BlockNodeMap blockNodes, HashSet<string> targetNames, Action<BlockNode> onRootNodeFound)
		{
			foreach (string text in targetNames)
			{
				BlockNode blockNode;
				if (!blockNodes.TryGetValue(text, out blockNode))
				{
					Debug.LogWarning("No blockNode found for block: \"" + text + "\"");
				}
				else
				{
					BlockNode blockNode2 = null;
					for (BlockNode blockNode3 = blockNode; blockNode3 != null; blockNode3 = blockNode3.Parent)
					{
						BlockNode.ElementInfo elementInfo;
						if (blockNode3.ElementInfos.TryGetValue("MultiBlockDim", out elementInfo) && elementInfo.Element != null)
						{
							Vector3i vector3i = StringParsers.ParseVector3i(elementInfo.Element.GetAttribute(XNames.value), 0, -1, false);
							if (vector3i.x > 1 || vector3i.z > 1)
							{
								blockNode2 = blockNode3;
							}
						}
					}
					if (blockNode2 != null && onRootNodeFound != null)
					{
						onRootNodeFound(blockNode2);
					}
				}
			}
		}

		public class ConversionDebugInfo
		{
			public OversizedConversionUtils.ConversionDebugInfo.OffsetCounts Offsets
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			} = new OversizedConversionUtils.ConversionDebugInfo.OffsetCounts();

			public int ModifiedBlockNodeCount
			{
				get
				{
					return this.modifiedBlockNodes.Count;
				}
			}

			public int BaseConversionCount
			{
				get
				{
					return this.baseConversionCount;
				}
			}

			public void Reset()
			{
				this.Offsets.Explicit = 0;
				this.Offsets.ShapeNew = 0;
				this.Offsets.ShapeModelEntity = 0;
				this.Offsets.ShapeExt3dModel = 0;
				this.Offsets.ShapeOther = 0;
				this.Offsets.Default = 0;
				this.modifiedBlockNodes.Clear();
				this.baseConversionCount = 0;
			}

			public void Log()
			{
				Debug.Log(this.CountsToString());
			}

			public string CountsToString()
			{
				return string.Concat(new string[]
				{
					string.Format("modifiedBlocksCount: {0}\n", this.ModifiedBlockNodeCount),
					string.Format("baseConversionCount: {0}\n", this.BaseConversionCount),
					string.Format("Offsets.Explicit: {0}\n", this.Offsets.Explicit),
					string.Format("Offsets.ShapeNew: {0}\n", this.Offsets.ShapeNew),
					string.Format("Offsets.ShapeModelEntity: {0}\n", this.Offsets.ShapeModelEntity),
					string.Format("Offsets.ShapeExt3dModel: {0}\n", this.Offsets.ShapeExt3dModel),
					string.Format("Offsets.ShapeOther: {0}\n", this.Offsets.ShapeOther),
					string.Format("Offsets.Default: {0}", this.Offsets.Default)
				});
			}

			public void OnBlockModified(BlockNode blockNode)
			{
				this.modifiedBlockNodes.Add(blockNode);
			}

			public void OnBaseBlockConverted(BlockNode node)
			{
				this.baseConversionCount++;
			}

			public void OnOriginModelOffsetTypeCalculated(BlockNode.ModelOffsetType modelOffsetType)
			{
				int num;
				switch (modelOffsetType)
				{
				case BlockNode.ModelOffsetType.Explicit:
				{
					OversizedConversionUtils.ConversionDebugInfo.OffsetCounts offsets = this.Offsets;
					num = offsets.Explicit;
					offsets.Explicit = num + 1;
					return;
				}
				case BlockNode.ModelOffsetType.ShapeNew:
				{
					OversizedConversionUtils.ConversionDebugInfo.OffsetCounts offsets2 = this.Offsets;
					num = offsets2.ShapeNew;
					offsets2.ShapeNew = num + 1;
					return;
				}
				case BlockNode.ModelOffsetType.ShapeModelEntity:
				{
					OversizedConversionUtils.ConversionDebugInfo.OffsetCounts offsets3 = this.Offsets;
					num = offsets3.ShapeModelEntity;
					offsets3.ShapeModelEntity = num + 1;
					return;
				}
				case BlockNode.ModelOffsetType.ShapeExt3dModel:
				{
					OversizedConversionUtils.ConversionDebugInfo.OffsetCounts offsets4 = this.Offsets;
					num = offsets4.ShapeExt3dModel;
					offsets4.ShapeExt3dModel = num + 1;
					return;
				}
				case BlockNode.ModelOffsetType.ShapeOther:
				{
					OversizedConversionUtils.ConversionDebugInfo.OffsetCounts offsets5 = this.Offsets;
					num = offsets5.ShapeOther;
					offsets5.ShapeOther = num + 1;
					return;
				}
				}
				OversizedConversionUtils.ConversionDebugInfo.OffsetCounts offsets6 = this.Offsets;
				num = offsets6.Default;
				offsets6.Default = num + 1;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public HashSet<BlockNode> modifiedBlockNodes = new HashSet<BlockNode>();

			[PublicizedFrom(EAccessModifier.Private)]
			public int baseConversionCount;

			public class OffsetCounts
			{
				public int Explicit { get; set; }

				public int ShapeNew { get; set; }

				public int ShapeModelEntity { get; set; }

				public int ShapeExt3dModel { get; set; }

				public int ShapeOther { get; set; }

				public int Default { get; set; }
			}
		}
	}
}
