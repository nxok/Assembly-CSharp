using System;
using System.Collections.Generic;

public class XUiControllerComparerDepth : IComparer<XUiController>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiControllerComparerDepth()
	{
	}

	public int Compare(XUiController _x, XUiController _y)
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
		return _x.ViewComponent.Depth.CompareTo(_y.ViewComponent.Depth);
	}

	public static XUiControllerComparerDepth Instance
	{
		get
		{
			XUiControllerComparerDepth xuiControllerComparerDepth;
			if ((xuiControllerComparerDepth = XUiControllerComparerDepth.instance) == null)
			{
				xuiControllerComparerDepth = (XUiControllerComparerDepth.instance = new XUiControllerComparerDepth());
			}
			return xuiControllerComparerDepth;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiControllerComparerDepth instance;
}
