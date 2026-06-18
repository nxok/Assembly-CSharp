using System;
using System.Globalization;

public readonly struct XUiSideSizes
{
	public int SumLeftRight
	{
		get
		{
			return this.Left + this.Right;
		}
	}

	public int SumTopBottom
	{
		get
		{
			return this.Top + this.Bottom;
		}
	}

	public XUiSideSizes(int _left, int _right, int _top, int _bottom)
	{
		this.Left = _left;
		this.Right = _right;
		this.Top = _top;
		this.Bottom = _bottom;
	}

	public XUiSideSizes SetLeft(int _value)
	{
		return new XUiSideSizes(_value, this.Right, this.Top, this.Bottom);
	}

	public XUiSideSizes SetRight(int _value)
	{
		return new XUiSideSizes(this.Left, _value, this.Top, this.Bottom);
	}

	public XUiSideSizes SetTop(int _value)
	{
		return new XUiSideSizes(this.Left, this.Right, _value, this.Bottom);
	}

	public XUiSideSizes SetBottom(int _value)
	{
		return new XUiSideSizes(this.Left, this.Right, this.Top, _value);
	}

	public static bool TryParse(string _value, out XUiSideSizes _result, string _valueName)
	{
		StringParsers.SeparatorPositions separatorPositions = StringParsers.GetSeparatorPositions(_value, ',', 3, 0, -1);
		if (separatorPositions.TotalFound > 3)
		{
			Log.Warning(string.Format("[XUi] Invalid number of values for {0}: {1}, max of 4 expected (input string: '{2}')", _valueName, separatorPositions.TotalFound, _value));
			_result = default(XUiSideSizes);
			return false;
		}
		int num;
		if (!StringParsers.TryParseSInt32(_value, out num, 0, separatorPositions.Sep1 - 1, NumberStyles.Integer))
		{
			Log.Warning(string.Concat(new string[] { "[XUi] ", _valueName, " can not be parsed, not an integer as 1st value (input string: '", _value, "')" }));
			_result = default(XUiSideSizes);
			return false;
		}
		if (separatorPositions.TotalFound == 0)
		{
			_result = new XUiSideSizes(num, num, num, num);
			return true;
		}
		int num2;
		if (!StringParsers.TryParseSInt32(_value, out num2, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1, NumberStyles.Integer))
		{
			Log.Warning(string.Concat(new string[] { "[XUi] ", _valueName, " can not be parsed, not an integer as 2nd value (input string: '", _value, "')" }));
			_result = default(XUiSideSizes);
			return false;
		}
		if (separatorPositions.TotalFound == 1)
		{
			_result = new XUiSideSizes(num2, num2, num, num);
			return true;
		}
		int num3;
		if (!StringParsers.TryParseSInt32(_value, out num3, separatorPositions.Sep2 + 1, separatorPositions.Sep3 - 1, NumberStyles.Integer))
		{
			Log.Warning(string.Concat(new string[] { "[XUi] ", _valueName, " can not be parsed, not an integer as 3rd value (input string: '", _value, "')" }));
			_result = default(XUiSideSizes);
			return false;
		}
		if (separatorPositions.TotalFound == 2)
		{
			_result = new XUiSideSizes(num2, num2, num, num3);
			return true;
		}
		int num4;
		if (!StringParsers.TryParseSInt32(_value, out num4, separatorPositions.Sep3 + 1, separatorPositions.Sep4 - 1, NumberStyles.Integer))
		{
			Log.Warning(string.Concat(new string[] { "[XUi] ", _valueName, " can not be parsed, not an integer as 4th value (input string: '", _value, "')" }));
			_result = default(XUiSideSizes);
			return false;
		}
		_result = new XUiSideSizes(num4, num2, num, num3);
		return true;
	}

	public readonly int Left;

	public readonly int Right;

	public readonly int Top;

	public readonly int Bottom;
}
