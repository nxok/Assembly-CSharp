using System;
using System.Collections.Generic;

public class TileEntityCompositeData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static void Init()
	{
		if (TileEntityCompositeData.knownFeatures.Count > 0)
		{
			return;
		}
		ReflectionHelpers.FindTypesImplementingBase(typeof(ITileEntityFeature), delegate(Type _type)
		{
			string name = _type.Name;
			Type type;
			if (TileEntityCompositeData.knownFeatures.TryGetValue(name, out type))
			{
				Log.Warning(string.Concat(new string[] { "Redeclaration of CompositeTileEntity feature ", name, ": ", type.FullName, " vs ", _type.FullName }));
				return;
			}
			if (_type.GetConstructor(Type.EmptyTypes) == null)
			{
				Log.Warning("CompositeTileEntity feature " + name + " has no parameterless constructor!");
				return;
			}
			TileEntityCompositeData.knownFeatures[name] = _type;
		}, false);
	}

	public static void Cleanup()
	{
		TileEntityCompositeData.FeaturesByBlock.Clear();
	}

	public static TileEntityCompositeData ParseBlock(BlockCompositeTileEntity _block)
	{
		DynamicProperties dynamicProperties;
		if (!_block.Properties.Classes.TryGetValue("CompositeFeatures", out dynamicProperties))
		{
			throw new ArgumentException("Block " + _block.GetBlockName() + " uses class BlockCompositeTileEntity but has no CompositeFeatures property");
		}
		return new TileEntityCompositeData(_block, dynamicProperties);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCompositeData(BlockCompositeTileEntity _block, DynamicProperties _compositeProps)
	{
		TileEntityCompositeData.Init();
		this.Block = _block;
		this.CompositeProps = _compositeProps;
		int num = 0;
		foreach (KeyValuePair<string, DynamicProperties> keyValuePair in _compositeProps.Classes)
		{
			string text;
			DynamicProperties dynamicProperties;
			keyValuePair.Deconstruct(out text, out dynamicProperties);
			string text2 = text;
			DynamicProperties dynamicProperties2 = dynamicProperties;
			Type type;
			if (!TileEntityCompositeData.knownFeatures.TryGetValue(text2, out type))
			{
				throw new ArgumentException(string.Concat(new string[]
				{
					"Block \"",
					_block.GetBlockName(),
					"\": CompositeFeature class \"",
					text2,
					"\" not found!"
				}));
			}
			TileEntityFeatureData tileEntityFeatureData = new TileEntityFeatureData(this, text2, num, type, dynamicProperties2);
			this.Features.Add(tileEntityFeatureData);
			num++;
		}
		if (this.Features.Count == 0)
		{
			throw new ArgumentException("Block \"" + _block.GetBlockName() + "\": No CompositeFeatures specified!");
		}
		this.Features.Sort(TileEntityFeatureData.FeatureDataSorterByName.Instance);
		for (int i = 0; i < this.Features.Count; i++)
		{
			TileEntityFeatureData tileEntityFeatureData2 = this.Features[i];
			this.featureIndexByName[tileEntityFeatureData2.Name.AsMemory()] = i;
			int stableHashCode = tileEntityFeatureData2.Name.GetStableHashCode();
			int num2;
			if (this.featureIndexByHash.TryGetValue(stableHashCode, out num2))
			{
				TileEntityFeatureData tileEntityFeatureData3 = this.Features[num2];
				Log.Error(string.Format("Composite block \"{0}\": Feature hash collision 0x{1:X8} for \"{2}\" vs \"{3}\". ", new object[]
				{
					_block.GetBlockName(),
					stableHashCode,
					tileEntityFeatureData3.Name,
					tileEntityFeatureData2.Name
				}) + "This hash will be ambiguous during load.");
			}
			else
			{
				this.featureIndexByHash.Add(stableHashCode, i);
			}
		}
		TileEntityCompositeData.FeaturesByBlock[_block] = this;
	}

	public int GetFeatureIndex(ReadOnlyMemory<char> _featureName)
	{
		return this.featureIndexByName.GetValueOrDefault(_featureName, -1);
	}

	public int GetFeatureIndex(int _featureNameHash)
	{
		return this.featureIndexByHash.GetValueOrDefault(_featureNameHash, -1);
	}

	public int GetFeatureIndex<T>() where T : class
	{
		Type typeFromHandle = typeof(T);
		int num;
		if (this.featureIndexByType.TryGetValue(typeFromHandle, out num))
		{
			return num;
		}
		for (int i = 0; i < this.Features.Count; i++)
		{
			TileEntityFeatureData tileEntityFeatureData = this.Features[i];
			if (typeFromHandle.IsAssignableFrom(tileEntityFeatureData.Type))
			{
				this.featureIndexByType[typeFromHandle] = i;
				return i;
			}
		}
		this.featureIndexByType[typeFromHandle] = -1;
		return -1;
	}

	public bool HasFeature(ReadOnlyMemory<char> _featureName)
	{
		return this.GetFeatureIndex(_featureName) >= 0;
	}

	public bool HasFeature(int _featureNameHash)
	{
		return this.GetFeatureIndex(_featureNameHash) >= 0;
	}

	public bool HasFeature<T>() where T : class
	{
		return this.GetFeatureIndex<T>() >= 0;
	}

	public bool TryGetFeatureData<T>(out TileEntityFeatureData featureData) where T : class, ITileEntityFeature
	{
		int featureIndex = this.GetFeatureIndex<T>();
		if (featureIndex >= 0)
		{
			featureData = this.Features[featureIndex];
			return true;
		}
		featureData = null;
		return false;
	}

	public void PrintConfig()
	{
		Log.Out("Composite block: " + this.Block.GetBlockName() + ":");
		foreach (KeyValuePair<string, string> keyValuePair in this.CompositeProps.Values)
		{
			string text;
			string text2;
			keyValuePair.Deconstruct(out text, out text2);
			string text3 = text;
			string text4 = text2;
			Log.Out("    " + text3 + "=" + text4);
		}
		foreach (TileEntityFeatureData tileEntityFeatureData in this.Features)
		{
			Log.Out(string.Format("  Feature: {0} (class {1} in assembly ({2})), manual order = {3}:", new object[]
			{
				tileEntityFeatureData.Name,
				tileEntityFeatureData.Type.FullName,
				tileEntityFeatureData.Type.Assembly.FullName,
				tileEntityFeatureData.CustomOrder
			}));
			foreach (KeyValuePair<string, string> keyValuePair in tileEntityFeatureData.Props.Values)
			{
				string text;
				string text2;
				keyValuePair.Deconstruct(out text2, out text);
				string text5 = text2;
				string text6 = text;
				Log.Out("    " + text5 + "=" + text6);
			}
		}
	}

	public static readonly Dictionary<BlockCompositeTileEntity, TileEntityCompositeData> FeaturesByBlock = new Dictionary<BlockCompositeTileEntity, TileEntityCompositeData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, Type> knownFeatures = new Dictionary<string, Type>();

	public readonly BlockCompositeTileEntity Block;

	public readonly DynamicProperties CompositeProps;

	public readonly List<TileEntityFeatureData> Features = new List<TileEntityFeatureData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, int> featureIndexByType = new Dictionary<Type, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ReadOnlyMemory<char>, int> featureIndexByName = new Dictionary<ReadOnlyMemory<char>, int>(TileEntityCompositeData.MemStringEqualityComparer.Instance);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, int> featureIndexByHash = new Dictionary<int, int>();

	public class MemStringEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public MemStringEqualityComparer()
		{
		}

		public int GetHashCode(ReadOnlyMemory<char> _obj)
		{
			return _obj.Span.GetStableHashCode();
		}

		public bool Equals(ReadOnlyMemory<char> _x, ReadOnlyMemory<char> _y)
		{
			return _x.Span.Equals(_y.Span, StringComparison.Ordinal);
		}

		public static readonly TileEntityCompositeData.MemStringEqualityComparer Instance = new TileEntityCompositeData.MemStringEqualityComparer();
	}
}
