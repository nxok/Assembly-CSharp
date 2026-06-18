using System;
using System.Collections.Generic;

public class TileEntityFeatureData
{
	public TileEntityFeatureData(TileEntityCompositeData _parent, string _name, int _customOrder, Type _type, DynamicProperties _props)
	{
		this.Parent = _parent;
		this.Name = _name;
		this.NameHash = this.Name.GetStableHashCode();
		this.CustomOrder = _customOrder;
		this.Type = _type;
		this.Props = _props;
	}

	public ITileEntityFeature InstantiateModule()
	{
		return ReflectionHelpers.Instantiate<ITileEntityFeature>(this.Type);
	}

	public readonly TileEntityCompositeData Parent;

	public readonly string Name;

	public readonly int NameHash;

	public readonly int CustomOrder;

	public readonly Type Type;

	public readonly DynamicProperties Props;

	public class FeatureDataSorterByName : IComparer<TileEntityFeatureData>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public FeatureDataSorterByName()
		{
		}

		public int Compare(TileEntityFeatureData _x, TileEntityFeatureData _y)
		{
			if (_x == _y)
			{
				return 0;
			}
			if (_y == null)
			{
				return 1;
			}
			if (_x == null)
			{
				return -1;
			}
			return string.Compare(_x.Name, _y.Name, StringComparison.Ordinal);
		}

		public static readonly TileEntityFeatureData.FeatureDataSorterByName Instance = new TileEntityFeatureData.FeatureDataSorterByName();
	}
}
