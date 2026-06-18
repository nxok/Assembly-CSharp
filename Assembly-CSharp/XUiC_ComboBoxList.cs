using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxList<TElement> : XUiC_ComboBox<TElement>
{
	[XuiXmlAttribute("localization_prefix", false)]
	public string LocalizationPrefix
	{
		get
		{
			return this.localizationPrefix;
		}
		set
		{
			this.localizationPrefix = value;
			this.UpdateLabel();
		}
	}

	[XuiXmlAttribute("localization_key_caseinsensitive", false)]
	public bool LocalizationKeyCaseInsensitive { get; set; }

	public override TElement Value
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			if (this.Elements.Contains(value))
			{
				int num = this.Elements.IndexOf(value);
				this.SelectedIndex = num;
				this.useCustomValue = false;
				return;
			}
			if (value != null)
			{
				this.CustomValue = value;
				this.useCustomValue = true;
				this.SelectedIndex = -1;
			}
		}
	}

	public override double RelativeValue
	{
		get
		{
			if (this.MaxIndex <= this.MinIndex)
			{
				return 0.0;
			}
			return ((double)((float)this.SelectedIndex) + 0.5 - (double)this.MinIndex) / (double)(this.MaxIndex - this.MinIndex + 1);
		}
		set
		{
			if (!this.UsesIndexMarkers)
			{
				return;
			}
			if (value >= 1.0)
			{
				this.SelectedIndex = this.MaxIndex;
				return;
			}
			if (value <= 0.0)
			{
				this.SelectedIndex = this.MinIndex;
				return;
			}
			this.SelectedIndex = Mathf.RoundToInt((float)value * (float)(this.MaxIndex - this.MinIndex + 1) + (float)this.MinIndex - 0.5f);
		}
	}

	public override long ValueGeneric
	{
		get
		{
			return (long)this.SelectedIndex;
		}
		set
		{
			this.SelectedIndex = (int)value;
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return (long)this.MinIndex;
		}
		set
		{
			this.MinIndex = (int)value;
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return (long)this.MaxIndex;
		}
		set
		{
			this.MaxIndex = (int)value;
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.MaxIndex - this.MinIndex + 1;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!this.reverseList)
			{
				return this.SelectedIndex - this.MinIndex;
			}
			return this.MaxIndex - this.SelectedIndex;
		}
	}

	public int SelectedIndex
	{
		get
		{
			return this.currentIndex;
		}
		set
		{
			if (value < 0)
			{
				if (this.useCustomValue)
				{
					this.currentIndex = value;
					this.currentValue = this.CustomValue;
				}
				else
				{
					this.currentIndex = this.MinIndex;
					this.currentValue = ((this.currentIndex < 0 || this.currentIndex >= this.Elements.Count) ? default(TElement) : this.Elements[this.currentIndex]);
				}
			}
			else
			{
				if (value >= this.Elements.Count)
				{
					value = this.Elements.Count - 1;
				}
				if (value < this.MinIndex)
				{
					value = this.MinIndex;
				}
				else if (value > this.MaxIndex)
				{
					value = this.MaxIndex;
				}
				this.currentIndex = value;
				this.currentValue = ((this.currentIndex < 0 || this.currentIndex >= this.Elements.Count) ? default(TElement) : this.Elements[this.currentIndex]);
			}
			this.IsDirty = true;
			this.UpdateLabel();
		}
	}

	[XuiXmlAttribute("index_min", false)]
	public int MinIndex
	{
		get
		{
			if (this.minIndex >= 0)
			{
				return this.minIndex;
			}
			return 0;
		}
		set
		{
			if (value != this.minIndex)
			{
				this.minIndex = value;
				if (this.SelectedIndex < this.minIndex)
				{
					this.SelectedIndex = this.minIndex;
				}
				base.UpdateIndexMarkerPositions();
				this.IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("index_max", false)]
	public int MaxIndex
	{
		get
		{
			if (this.maxIndex < this.Elements.Count && this.maxIndex >= 0)
			{
				return this.maxIndex;
			}
			return this.Elements.Count - 1;
		}
		set
		{
			if (value != this.maxIndex)
			{
				this.maxIndex = value;
				if (this.maxIndex >= 0 && this.SelectedIndex > this.maxIndex)
				{
					this.SelectedIndex = this.maxIndex;
				}
				base.UpdateIndexMarkerPositions();
				this.IsDirty = true;
			}
		}
	}

	public override void OnOpen()
	{
		if (this.Elements.Count > 0 && !this.useCustomValue)
		{
			if (this.SelectedIndex < 0 && !this.useCustomValue)
			{
				this.SelectedIndex = 0;
			}
			if (this.SelectedIndex > this.Elements.Count)
			{
				this.SelectedIndex = this.Elements.Count - 1;
			}
			if (this.SelectedIndex < this.MinIndex)
			{
				this.SelectedIndex = this.MinIndex;
			}
			if (this.SelectedIndex > this.MaxIndex)
			{
				this.SelectedIndex = this.MaxIndex;
			}
			if (!this.useCustomValue)
			{
				this.currentValue = ((this.SelectedIndex < this.Elements.Count) ? this.Elements[this.SelectedIndex] : default(TElement));
			}
		}
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		if (this.isEmpty())
		{
			base.ValueText = "";
			return;
		}
		TElement currentValue = this.currentValue;
		string text = ((currentValue == null) ? "" : currentValue.ToString());
		base.ValueText = ((!string.IsNullOrEmpty(this.LocalizationPrefix)) ? Localization.Get(this.LocalizationPrefix + text, this.LocalizationKeyCaseInsensitive, null) : text);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TElement _oldVal, TElement _currentValue)
	{
		return (_oldVal != null || _currentValue != null) && (_oldVal == null || _currentValue == null || !_oldVal.Equals(_currentValue));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		this.ChangeIndex(this.reverseList ? 1 : (-1));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		this.ChangeIndex(this.reverseList ? (-1) : 1);
	}

	public void ChangeIndex(int _direction)
	{
		int num = this.SelectedIndex + _direction;
		if (num < this.MinIndex)
		{
			num = (this.Wrap ? Utils.FastMin(this.Elements.Count - 1, this.MaxIndex) : this.MinIndex);
		}
		if (num > this.MaxIndex)
		{
			num = (this.Wrap ? Utils.FastMax(0, this.MinIndex) : this.MaxIndex);
		}
		if (num < 0)
		{
			num = (this.Wrap ? (this.Elements.Count - 1) : 0);
		}
		if (num >= this.Elements.Count)
		{
			num = (this.Wrap ? 0 : (this.Elements.Count - 1));
		}
		this.SelectedIndex = num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		if (!this.reverseList)
		{
			return this.isMaxIndex();
		}
		return this.isMinIndex();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		if (!this.reverseList)
		{
			return this.isMinIndex();
		}
		return this.isMaxIndex();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return this.Elements.Count == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMaxIndex()
	{
		return this.SelectedIndex == this.MaxIndex || this.SelectedIndex == this.Elements.Count - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMinIndex()
	{
		return this.SelectedIndex == this.MinIndex || this.SelectedIndex == 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		if (_value > 0.0)
		{
			this.TryPageUp();
			return;
		}
		if (_value < 0.0)
		{
			this.TryPageDown();
		}
	}

	[XuiXmlAttribute("values", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValues(string _value)
	{
		if (_value.EqualsCaseInsensitive("@def"))
		{
			return;
		}
		string[] array = _value.Split(',', StringSplitOptions.None);
		Type typeFromHandle = typeof(TElement);
		if (typeFromHandle == typeof(string))
		{
			foreach (string text in array)
			{
				this.Elements.Add((TElement)((object)text.Trim()));
			}
			return;
		}
		if (typeof(IConvertible).IsAssignableFrom(typeFromHandle))
		{
			foreach (string text2 in array)
			{
				try
				{
					this.Elements.Add((TElement)((object)Convert.ChangeType(text2, typeFromHandle)));
				}
				catch (Exception ex)
				{
					Log.Error(string.Format("[XUi] Value \"{0}\" not supported for the ComboBox type {1}", text2, typeFromHandle));
					Log.Exception(ex);
				}
			}
			return;
		}
		ConstructorInfo constructor = typeFromHandle.GetConstructor(new Type[] { typeof(string) });
		if (constructor == null)
		{
			Log.Error(string.Format("[XUi] ComboBox type {0} does not support values from string elements", typeFromHandle));
			return;
		}
		foreach (string text3 in array)
		{
			object[] array3 = new object[1];
			try
			{
				array3[0] = text3;
				this.Elements.Add((TElement)((object)constructor.Invoke(array3)));
			}
			catch (Exception ex2)
			{
				Log.Error(string.Format("[XUi] Value \"{0}\" not supported for the ComboBox type {1}", text3, typeFromHandle));
				Log.Exception(ex2);
			}
		}
	}

	[XuiXmlAttribute("reverse_list", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeReverseList(string _value)
	{
		if (_value.EqualsCaseInsensitive("@def"))
		{
			return;
		}
		this.reverseList = StringParsers.ParseBool(_value, 0, -1, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizationPrefix;

	public readonly List<TElement> Elements = new List<TElement>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex = int.MinValue;

	public TElement CustomValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useCustomValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minIndex = int.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxIndex = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool reverseList;
}
