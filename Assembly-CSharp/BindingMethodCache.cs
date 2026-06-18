using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

public class BindingMethodCache
{
	public static BindingMethodCache Instance
	{
		get
		{
			BindingMethodCache bindingMethodCache;
			if ((bindingMethodCache = BindingMethodCache.instance) == null)
			{
				bindingMethodCache = (BindingMethodCache.instance = new BindingMethodCache());
			}
			return bindingMethodCache;
		}
	}

	public bool TryGetBindingDelegate(XUiController _controller, string _bindingName, out XuiBindingDelegate _bindingDelegate)
	{
		Type type = _controller.GetType();
		BindingMethodCache.BindingMethodTypeCache bindingMethodTypeCache;
		if (!this.cachePerType.TryGetValue(type, out bindingMethodTypeCache))
		{
			bindingMethodTypeCache = new BindingMethodCache.BindingMethodTypeCache(type);
			this.cachePerType[type] = bindingMethodTypeCache;
		}
		return bindingMethodTypeCache.TryGetBindingDelegate(_bindingName, out _bindingDelegate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetBindingObjectDelegate(MethodInfo _method, out XuiBindingDelegate _func)
	{
		_func = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating binding method wrapper: MethodInfo null");
			return false;
		}
		if (_method.GetParameters().Length != 0)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating binding method wrapper: Method has parameters (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		Type returnType = _method.ReturnType;
		if (returnType == typeof(void))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating binding method wrapper: Method has no return value (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		MethodInfo methodInfo;
		if (returnType == typeof(bool))
		{
			methodInfo = new Func<MethodInfo, XuiBindingDelegate>(BindingMethodCache.<tryGetBindingObjectDelegate>g__GetBindingObjectDelegateCoreBool|11_1<XUiController>).Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { _method.DeclaringType });
		}
		else if (returnType == typeof(int))
		{
			methodInfo = new Func<MethodInfo, XuiBindingDelegate>(BindingMethodCache.<tryGetBindingObjectDelegate>g__GetBindingObjectDelegateCoreInt|11_2<XUiController>).Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { _method.DeclaringType });
		}
		else
		{
			methodInfo = new Func<MethodInfo, XuiBindingDelegate>(BindingMethodCache.<tryGetBindingObjectDelegate>g__GetBindingObjectDelegateCoreGeneric|11_0<XUiController, object>).Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { _method.DeclaringType, returnType });
		}
		_func = (XuiBindingDelegate)methodInfo.Invoke(null, new object[] { _method });
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static XuiBindingDelegate <tryGetBindingObjectDelegate>g__GetBindingObjectDelegateCoreGeneric|11_0<TController, TValue>(MethodInfo _methodInner) where TController : XUiController
	{
		Func<TController, TValue> typedFunc = (Func<TController, TValue>)Delegate.CreateDelegate(typeof(Func<TController, TValue>), _methodInner);
		return (XUiController _instance) => typedFunc((TController)((object)_instance));
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static XuiBindingDelegate <tryGetBindingObjectDelegate>g__GetBindingObjectDelegateCoreBool|11_1<TController>(MethodInfo _methodInner) where TController : XUiController
	{
		Func<TController, bool> typedFunc = (Func<TController, bool>)Delegate.CreateDelegate(typeof(Func<TController, bool>), _methodInner);
		return delegate(XUiController _instance)
		{
			if (!typedFunc((TController)((object)_instance)))
			{
				return BindingMethodCache.boxedFalse;
			}
			return BindingMethodCache.boxedTrue;
		};
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static XuiBindingDelegate <tryGetBindingObjectDelegate>g__GetBindingObjectDelegateCoreInt|11_2<TController>(MethodInfo _methodInner) where TController : XUiController
	{
		Func<TController, int> typedFunc = (Func<TController, int>)Delegate.CreateDelegate(typeof(Func<TController, int>), _methodInner);
		return delegate(XUiController _instance)
		{
			int num = typedFunc((TController)((object)_instance));
			object obj;
			if (num != 0)
			{
				if (num != 1)
				{
					obj = num;
				}
				else
				{
					obj = BindingMethodCache.boxed1;
				}
			}
			else
			{
				obj = BindingMethodCache.boxed0;
			}
			return obj;
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, BindingMethodCache.BindingMethodTypeCache> cachePerType = new Dictionary<Type, BindingMethodCache.BindingMethodTypeCache>(128);

	[PublicizedFrom(EAccessModifier.Private)]
	public static BindingMethodCache instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxed0 = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxed1 = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedFalse = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedTrue = true;

	public class BindingMethodData
	{
		public BindingMethodData(string _definingController, XuiBindingDelegate _delegate)
		{
			this.DefiningController = _definingController;
			this.Delegate = _delegate;
		}

		public readonly string DefiningController;

		public readonly XuiBindingDelegate Delegate;
	}

	public class BindingMethodTypeCache
	{
		public BindingMethodTypeCache(Type _controllerType)
		{
			this.controllerType = _controllerType;
			this.initCacheForController();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initCacheForController()
		{
			this.<initCacheForController>g__RecurseTypeTree|3_0(this.controllerType);
		}

		public bool TryGetBindingDelegate(string _bindingName, out XuiBindingDelegate _bindingDelegate)
		{
			BindingMethodCache.BindingMethodData bindingMethodData;
			if (this.methods.TryGetValue(_bindingName, out bindingMethodData))
			{
				_bindingDelegate = bindingMethodData.Delegate;
				return true;
			}
			_bindingDelegate = null;
			return false;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__RecurseTypeTree|3_0(Type _type)
		{
			if (_type == null)
			{
				return;
			}
			this.<initCacheForController>g__RecurseTypeTree|3_0(_type.BaseType);
			ReflectionHelpers.GetMethodsWithAttribute<XuiXmlBindingAttribute>(_type, new Action<MethodInfo, bool, XuiXmlBindingAttribute>(this.<initCacheForController>g__MethodFoundCallback|3_2), true, true, true);
			ReflectionHelpers.GetPropertiesWithAttribute<XuiXmlBindingAttribute>(_type, new Action<PropertyInfo, bool, XuiXmlBindingAttribute>(this.<initCacheForController>g__PropertyFoundCallback|3_1), true, true, true);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__PropertyFoundCallback|3_1(PropertyInfo _property, bool _hasMultiple, XuiXmlBindingAttribute _bindingAttribute)
		{
			MethodInfo getMethod = _property.GetGetMethod(true);
			if (getMethod == null)
			{
				Log.Error(string.Concat(new string[]
				{
					"[XUi] Failed creating binding property wrapper: Property has no getter (",
					_property.DeclaringType.FullName,
					".",
					_property.Name,
					")"
				}));
			}
			this.<initCacheForController>g__MethodFoundCallback|3_2(getMethod, _hasMultiple, _bindingAttribute);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__MethodFoundCallback|3_2(MethodInfo _method, bool _hasMultiple, XuiXmlBindingAttribute _bindingAttribute)
		{
			if (_method.IsStatic)
			{
				Log.Warning("[XUi] XML binding has to be non-static! " + _method.DeclaringType.FullName + "." + _method.Name);
				return;
			}
			XuiBindingDelegate xuiBindingDelegate;
			if (!BindingMethodCache.tryGetBindingObjectDelegate(_method, out xuiBindingDelegate))
			{
				return;
			}
			BindingMethodCache.BindingMethodData bindingMethodData = new BindingMethodCache.BindingMethodData(_method.DeclaringType.FullName, xuiBindingDelegate);
			BindingMethodCache.BindingMethodData bindingMethodData2;
			if (this.methods.TryGetValue(_bindingAttribute.BindingName, out bindingMethodData2))
			{
				Log.Warning(string.Concat(new string[] { "[XUi] Child class (", bindingMethodData.DefiningController, ") overriding parent (", bindingMethodData2.DefiningController, ") binding definition of '", _bindingAttribute.BindingName, "'" }));
			}
			this.methods[_bindingAttribute.BindingName] = bindingMethodData;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, BindingMethodCache.BindingMethodData> methods = new Dictionary<string, BindingMethodCache.BindingMethodData>(64, StringComparer.Ordinal);
	}
}
