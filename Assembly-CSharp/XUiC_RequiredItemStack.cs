using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RequiredItemStack : XUiC_ItemStack
{
	public event XUiEvent_RequiredSlotFailedSwapEventHandler FailedSwap;

	public bool HasAllowedItemClass
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.RequiredType == XUiC_RequiredItemStack.RequiredTypes.ItemClass && this.allowedItemClasses.Count > 0 && this.allowedItemClasses[0] != null;
		}
	}

	public override string ItemIcon
	{
		get
		{
			if (this.HasAllowedItemClass && this.itemStack.IsEmpty())
			{
				return this.allowedItemClasses[0].GetIconName();
			}
			return base.ItemIcon;
		}
	}

	public override string ItemIconColor
	{
		get
		{
			if (base.itemClass != null)
			{
				base.GreyedOut = false;
				return base.ItemIconColor;
			}
			if (this.HasAllowedItemClass && !base.StackLock)
			{
				base.GreyedOut = true;
				return "255,255,255,255";
			}
			base.GreyedOut = false;
			return "255,255,255,0";
		}
	}

	public bool ItemAllowed(ItemStack _stack)
	{
		bool flag;
		switch (this.RequiredType)
		{
		case XUiC_RequiredItemStack.RequiredTypes.ItemClass:
			flag = !this.HasAllowedItemClass || !this.RequiredItemOnly || this.allowedItemClasses.IndexOf(_stack.itemValue.ItemClass) >= 0;
			break;
		case XUiC_RequiredItemStack.RequiredTypes.IsPart:
			flag = _stack.itemValue.ItemClass.PartParentId != null;
			break;
		case XUiC_RequiredItemStack.RequiredTypes.HasQuality:
			flag = _stack.itemValue.HasQuality;
			break;
		case XUiC_RequiredItemStack.RequiredTypes.HasQualityNoParts:
			flag = _stack.itemValue.HasQuality && !_stack.itemValue.ItemClass.HasSubItems;
			break;
		default:
			flag = true;
			break;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack _stack)
	{
		if (this.TakeOnly && !_stack.IsEmpty())
		{
			return false;
		}
		bool flag = this.ItemAllowed(_stack);
		if (!flag)
		{
			XUiEvent_RequiredSlotFailedSwapEventHandler failedSwap = this.FailedSwap;
			if (failedSwap == null)
			{
				return flag;
			}
			failedSwap(_stack);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleDropOne()
	{
		ItemStack currentStack = this.xui.DragAndDropWindow.CurrentStack;
		if (!this.TryStack(currentStack, 1))
		{
			return;
		}
		this.xui.DragAndDropWindow.CurrentStack = currentStack;
		base.PlayPlaceSound(null);
	}

	public bool TryStack(ItemStack _stack, int _countToPlace = -1)
	{
		if (_stack.IsEmpty())
		{
			return false;
		}
		if (_countToPlace < 0)
		{
			_countToPlace = _stack.count;
		}
		if (_countToPlace > _stack.count)
		{
			_countToPlace = _stack.count;
		}
		if (this.itemStack.IsEmpty())
		{
			if (!this.ItemAllowed(_stack))
			{
				return false;
			}
			ItemStack itemStack = _stack.Clone();
			itemStack.count = _countToPlace;
			_stack.count -= _countToPlace;
			base.ItemStack = itemStack;
			base.HandleSlotChangeEvent();
			return true;
		}
		else
		{
			if (_stack.itemValue.type != this.itemStack.itemValue.type)
			{
				return false;
			}
			ItemClass itemClassOrMissing = this.itemStack.itemValue.ItemClassOrMissing;
			int num = ((this.OverrideStackCount == -1) ? itemClassOrMissing.Stacknumber.Value : Mathf.Min(itemClassOrMissing.Stacknumber.Value, this.OverrideStackCount)) - this.itemStack.count;
			if (num <= 0)
			{
				return false;
			}
			if (num < _countToPlace)
			{
				_countToPlace = num;
			}
			ItemStack itemStack2 = this.itemStack.Clone();
			itemStack2.count += _countToPlace;
			_stack.count -= _countToPlace;
			base.ItemStack = itemStack2;
			base.HandleSlotChangeEvent();
			return true;
		}
	}

	public void ClearAllowedItemClasses()
	{
		this.IsDirty = true;
		this.allowedItemClasses.Clear();
	}

	public void SetAllowedItemClassSingle(string _itemClassName)
	{
		this.SetAllowedItemClassSingle(ItemClass.GetItemClass(_itemClassName, false));
	}

	public void SetAllowedItemClassSingle(ItemClass _itemClass)
	{
		this.IsDirty = true;
		this.allowedItemClasses.Clear();
		if (_itemClass != null)
		{
			this.allowedItemClasses.Add(_itemClass);
		}
	}

	public void SetAllowedItemClasses(ItemClass[] _itemClasses)
	{
		this.IsDirty = true;
		this.allowedItemClasses.Clear();
		this.allowedItemClasses.AddRange(_itemClasses);
	}

	public static void ParseItemClassesFromString(IList<ItemClass> _targetList, string _itemClassesString)
	{
		XUiC_RequiredItemStack.<>c__DisplayClass22_0 CS$<>8__locals1;
		CS$<>8__locals1._targetList = _targetList;
		if (string.IsNullOrEmpty(_itemClassesString))
		{
			return;
		}
		foreach (string text in _itemClassesString.Split('+', StringSplitOptions.RemoveEmptyEntries))
		{
			if (text.StartsWith("tags(", StringComparison.Ordinal))
			{
				XUiC_RequiredItemStack.<ParseItemClassesFromString>g__ParseTagsBasedEntry|22_0(text, ref CS$<>8__locals1);
			}
			else
			{
				XUiC_RequiredItemStack.<ParseItemClassesFromString>g__AddItemClassByName|22_1(text, ref CS$<>8__locals1);
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <ParseItemClassesFromString>g__ParseTagsBasedEntry|22_0(string _text, ref XUiC_RequiredItemStack.<>c__DisplayClass22_0 A_1)
	{
		if (_text[_text.Length - 1] != ')')
		{
			Log.Warning("ItemClasses: Malformed tags() entry: '" + _text + "'. From: " + StackTraceUtility.ExtractStackTrace());
			return;
		}
		foreach (ItemClass itemClass in ItemClass.GetItemsWithTag(FastTags<TagGroup.Global>.Parse(_text.Substring("tags(".Length, _text.Length - 1 - "tags(".Length))))
		{
			XUiC_RequiredItemStack.<ParseItemClassesFromString>g__AddItemClass|22_2(itemClass, ref A_1);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <ParseItemClassesFromString>g__AddItemClassByName|22_1(string _itemClassName, ref XUiC_RequiredItemStack.<>c__DisplayClass22_0 A_1)
	{
		ItemClass itemClass = ItemClass.GetItemClass(_itemClassName, false);
		if (itemClass == null)
		{
			Log.Warning("ItemClasses: ItemClass '" + _itemClassName + "' not found. From: " + StackTraceUtility.ExtractStackTrace());
			return;
		}
		XUiC_RequiredItemStack.<ParseItemClassesFromString>g__AddItemClass|22_2(itemClass, ref A_1);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <ParseItemClassesFromString>g__AddItemClass|22_2(ItemClass _itemClass, ref XUiC_RequiredItemStack.<>c__DisplayClass22_0 A_1)
	{
		if (A_1._targetList.IndexOf(_itemClass) >= 0)
		{
			return;
		}
		A_1._targetList.Add(_itemClass);
	}

	public XUiC_RequiredItemStack.RequiredTypes RequiredType;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ItemClass> allowedItemClasses = new List<ItemClass>();

	public bool RequiredItemOnly = true;

	public bool TakeOnly;

	public enum RequiredTypes
	{
		ItemClass,
		IsPart,
		HasQuality,
		HasQualityNoParts
	}
}
