using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace XMLEditing
{
	public class BlockNode
	{
		public string Name { get; set; }

		public XElement Element { get; set; }

		public BlockNode Parent { get; set; }

		public List<BlockNode> Children { get; } = new List<BlockNode>();

		public Dictionary<string, BlockNode.ElementInfo> ElementInfos
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Dictionary<string, BlockNode.ElementInfo>();

		public void AddChild(BlockNode child)
		{
			child.Parent = this;
			this.Children.Add(child);
		}

		public bool TryGetPropertyParent(string targetPropertyName, out BlockNode propertyParentBlockNode, out BlockNode.ElementInfo propertyElementInfo, out int depth)
		{
			depth = 0;
			BlockNode blockNode = this;
			while (blockNode != null)
			{
				if (depth >= 100)
				{
					Debug.LogError("Max recursion depth exceeded!");
					break;
				}
				BlockNode.ElementInfo elementInfo;
				if (blockNode.ElementInfos.TryGetValue(targetPropertyName, out elementInfo))
				{
					if (elementInfo.Element != null)
					{
						propertyParentBlockNode = blockNode;
						propertyElementInfo = elementInfo;
						return true;
					}
					if (!elementInfo.CanInherit)
					{
						propertyParentBlockNode = null;
						propertyElementInfo = null;
						return false;
					}
				}
				blockNode = blockNode.Parent;
				depth++;
			}
			propertyParentBlockNode = null;
			propertyElementInfo = null;
			return false;
		}

		public bool TryGetModelOffset(out Vector3 modelOffset, out int depth, out BlockNode.ModelOffsetType modelOffsetType)
		{
			BlockNode blockNode;
			BlockNode.ElementInfo elementInfo;
			if (this.TryGetPropertyParent("ModelOffset", out blockNode, out elementInfo, out depth))
			{
				modelOffsetType = BlockNode.ModelOffsetType.Explicit;
				modelOffset = StringParsers.ParseVector3(elementInfo.Element.GetAttribute(XNames.value), 0, -1);
				return true;
			}
			BlockNode.ElementInfo elementInfo2;
			if (!this.TryGetPropertyParent("Shape", out blockNode, out elementInfo2, out depth))
			{
				depth = 0;
				BlockNode blockNode2 = this;
				while (blockNode2.Parent != null)
				{
					blockNode2 = blockNode2.Parent;
					depth++;
				}
				modelOffsetType = BlockNode.ModelOffsetType.DefaultShapeNew;
				modelOffset = new Vector3(1f, 0f, 1f);
				return true;
			}
			string text = elementInfo2.Element.GetAttribute(XNames.value).Trim();
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("BlockShape", text);
			if (typeWithPrefix == null)
			{
				modelOffsetType = BlockNode.ModelOffsetType.None;
				Debug.LogError("Failed to create shape type \"BlockShape" + text + "\" for block: " + this.Name);
				modelOffset = Vector3.zero;
				return false;
			}
			if (typeof(BlockShapeNew).IsAssignableFrom(typeWithPrefix))
			{
				modelOffsetType = BlockNode.ModelOffsetType.ShapeNew;
				modelOffset = new Vector3(1f, 0f, 1f);
				return true;
			}
			if (typeof(BlockShapeModelEntity).IsAssignableFrom(typeWithPrefix))
			{
				modelOffsetType = BlockNode.ModelOffsetType.ShapeModelEntity;
				modelOffset = new Vector3(0f, 0.5f, 0f);
				return true;
			}
			modelOffsetType = BlockNode.ModelOffsetType.None;
			modelOffset = Vector3.zero;
			return false;
		}

		public bool ShapeSupportsModelOffset()
		{
			BlockNode blockNode;
			BlockNode.ElementInfo elementInfo;
			int num;
			if (this.TryGetPropertyParent("Shape", out blockNode, out elementInfo, out num))
			{
				string text = elementInfo.Element.GetAttribute(XNames.value).Trim();
				Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("BlockShape", text);
				if (typeWithPrefix == null)
				{
					Debug.LogError("Failed to create shape type \"BlockShape" + text + "\" for block: " + this.Name);
					return false;
				}
				if (!typeof(BlockShapeNew).IsAssignableFrom(typeWithPrefix) && !typeof(BlockShapeModelEntity).IsAssignableFrom(typeWithPrefix))
				{
					return false;
				}
			}
			return true;
		}

		public enum ModelOffsetType
		{
			None,
			Explicit,
			ShapeNew,
			ShapeModelEntity,
			ShapeExt3dModel,
			ShapeOther,
			DefaultShapeNew
		}

		public class ElementInfo
		{
			public bool CanInherit { get; set; } = true;

			public bool IsClass { get; set; } = true;

			public XElement Element { get; set; }
		}
	}
}
