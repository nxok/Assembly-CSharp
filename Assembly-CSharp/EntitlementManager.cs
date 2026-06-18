using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;

public class EntitlementManager
{
	public static EntitlementManager Instance
	{
		get
		{
			if (EntitlementManager._instance == null)
			{
				EntitlementManager._instance = new EntitlementManager();
				if (PlatformManager.MultiPlatform.EntitlementValidators != null)
				{
					PlatformManager.NativePlatform.User.UserLoggedIn += delegate(IPlatform _)
					{
						object obj = EntitlementManager.lockObj;
						lock (obj)
						{
							EntitlementManager._instance.entitlementValidators = PlatformManager.MultiPlatform.EntitlementValidators;
						}
					};
				}
			}
			return EntitlementManager._instance;
		}
	}

	public bool HasEntitlement(object _addressableKey)
	{
		EntitlementSetEnum entitlementSet = this.GetEntitlementSet(_addressableKey);
		return this.HasEntitlement(entitlementSet);
	}

	public bool HasEntitlement(EntitlementSetEnum _set)
	{
		ValueTuple<bool, bool> valueTuple = this.CheckOverride(_set);
		bool item = valueTuple.Item1;
		bool item2 = valueTuple.Item2;
		if (item)
		{
			return item2;
		}
		if (_set == EntitlementSetEnum.None)
		{
			return true;
		}
		object obj = EntitlementManager.lockObj;
		lock (obj)
		{
			using (IEnumerator<IEntitlementValidator> enumerator = this.entitlementValidators.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.HasEntitlement(_set))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsAvailableOnPlatform(object _addressableKey)
	{
		EntitlementSetEnum setForAsset = this.GetSetForAsset(_addressableKey);
		return this.IsAvailableOnPlatform(setForAsset);
	}

	public bool IsAvailableOnPlatform(EntitlementSetEnum _set)
	{
		ValueTuple<bool, bool> valueTuple = this.CheckOverride(_set);
		bool item = valueTuple.Item1;
		bool item2 = valueTuple.Item2;
		if (item)
		{
			return item2;
		}
		if (_set == EntitlementSetEnum.None)
		{
			return true;
		}
		object obj = EntitlementManager.lockObj;
		lock (obj)
		{
			using (IEnumerator<IEntitlementValidator> enumerator = this.entitlementValidators.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsAvailableOnPlatform(_set))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public unsafe EntitlementSetEnum GetSetForAsset(object addressableKey)
	{
		string text = addressableKey as string;
		if (text == null)
		{
			EntitlementSetEnum entitlementSetEnum;
			if (EntitlementAddressablesMaps.AddressablesKeyMap.TryGetValue(addressableKey, out entitlementSetEnum))
			{
				return entitlementSetEnum;
			}
			return EntitlementSetEnum.None;
		}
		else
		{
			if (string.IsNullOrEmpty(text))
			{
				return EntitlementSetEnum.None;
			}
			StringSpan stringSpan = text.ToLowerInvariant().Trim();
			if (stringSpan.Length >= 2 && *stringSpan[0] == 64 && *stringSpan[1] == 58)
			{
				stringSpan = stringSpan.Slice(2);
			}
			while (stringSpan.Length > 0)
			{
				EntitlementSetEnum entitlementSetEnum2;
				if (EntitlementAddressablesMaps.AddressablesStringMap.TryGetValue(stringSpan, out entitlementSetEnum2))
				{
					return entitlementSetEnum2;
				}
				int num = stringSpan.LastIndexOf('/');
				if (num == -1)
				{
					break;
				}
				stringSpan = stringSpan.Slice(0, num);
			}
			return EntitlementSetEnum.None;
		}
	}

	public bool IsEntitlementPurchasable(object _addressableKey)
	{
		EntitlementSetEnum entitlementSet = this.GetEntitlementSet(_addressableKey);
		return this.IsEntitlementPurchasable(entitlementSet);
	}

	public bool IsEntitlementPurchasable(EntitlementSetEnum _set)
	{
		if (_set == EntitlementSetEnum.None)
		{
			return true;
		}
		object obj = EntitlementManager.lockObj;
		lock (obj)
		{
			using (IEnumerator<IEntitlementValidator> enumerator = this.entitlementValidators.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsEntitlementPurchasable(_set))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public unsafe EntitlementSetEnum GetEntitlementSet(object addressableKey)
	{
		string text = addressableKey as string;
		if (text == null)
		{
			EntitlementSetEnum entitlementSetEnum;
			if (EntitlementAddressablesMaps.AddressablesKeyMap.TryGetValue(addressableKey, out entitlementSetEnum))
			{
				return entitlementSetEnum;
			}
			return EntitlementSetEnum.None;
		}
		else
		{
			if (string.IsNullOrEmpty(text))
			{
				return EntitlementSetEnum.None;
			}
			StringSpan stringSpan = text.ToLowerInvariant().Trim();
			if (stringSpan.Length >= 2 && *stringSpan[0] == 64 && *stringSpan[1] == 58)
			{
				stringSpan = stringSpan.Slice(2);
			}
			while (stringSpan.Length > 0)
			{
				EntitlementSetEnum entitlementSetEnum2;
				if (EntitlementAddressablesMaps.AddressablesStringMap.TryGetValue(stringSpan, out entitlementSetEnum2))
				{
					return entitlementSetEnum2;
				}
				int num = stringSpan.LastIndexOf('/');
				if (num == -1)
				{
					break;
				}
				stringSpan = stringSpan.Slice(0, num);
			}
			return EntitlementSetEnum.None;
		}
	}

	public void OpenStore(EntitlementSetEnum _set, Action<EntitlementSetEnum> _onPurchased)
	{
		if (_set == EntitlementSetEnum.None)
		{
			return;
		}
		object obj = EntitlementManager.lockObj;
		lock (obj)
		{
			using (IEnumerator<IEntitlementValidator> enumerator = this.entitlementValidators.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.OpenStore(_set, _onPurchased))
					{
						break;
					}
				}
			}
		}
	}

	[return: TupleElementNames(new string[] { "hasOverride", "overrideValue" })]
	public ValueTuple<bool, bool> CheckOverride(EntitlementSetEnum _set)
	{
		return new ValueTuple<bool, bool>(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntitlementManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public IList<IEntitlementValidator> entitlementValidators = new List<IEntitlementValidator>();
}
