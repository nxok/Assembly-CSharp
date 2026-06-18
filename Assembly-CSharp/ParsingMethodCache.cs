using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

public class ParsingMethodCache
{
	public static ParsingMethodCache Instance
	{
		get
		{
			ParsingMethodCache parsingMethodCache;
			if ((parsingMethodCache = ParsingMethodCache.instance) == null)
			{
				parsingMethodCache = (ParsingMethodCache.instance = new ParsingMethodCache());
			}
			return parsingMethodCache;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ParsingMethodCache()
	{
		this.genericEnumTryParseMethod = new ParsingMethodCache.RawConverterFromStringDelegate<int>(EnumUtils.TryParseIgnoreCase<int>).Method.GetGenericMethodDefinition();
		ReflectionHelpers.FindTypesWithAttribute<XuiXmlAttributeConvertersClassAttribute>(new Action<Type, bool, XuiXmlAttributeConvertersClassAttribute>(this.<.ctor>g__TypeFoundCallback|9_0), true);
	}

	public bool TryGetParsingDelegate(IXUiElement _target, string _attributeName, out ParsingMethodCache.ParsingMethodData _parsingDelegate)
	{
		Type type = _target.GetType();
		ParsingMethodCache.ParsingMethodTypeCache parsingMethodTypeCache;
		if (!this.cachePerType.TryGetValue(type, out parsingMethodTypeCache))
		{
			parsingMethodTypeCache = new ParsingMethodCache.ParsingMethodTypeCache(type);
			this.cachePerType[type] = parsingMethodTypeCache;
		}
		return parsingMethodTypeCache.TryGetParsingDelegate(_attributeName, out _parsingDelegate);
	}

	public bool TryParseDirect(IXUiElement _target, string _attributeName, string _value)
	{
		ParsingMethodCache.ParsingMethodData parsingMethodData;
		if (!this.TryGetParsingDelegate(_target, _attributeName, out parsingMethodData))
		{
			return false;
		}
		XuiParsingDelegate xuiParsingDelegate;
		if (!parsingMethodData.TryGetDelegateForSourceType(typeof(string), out xuiParsingDelegate))
		{
			Log.Warning(string.Concat(new string[]
			{
				"[XUi] Found attribute parser but no suitable converter from string input. Attribute: ",
				_attributeName,
				", target type: ",
				parsingMethodData.NativeParseType.FullName,
				" on ",
				_target.GetType().FullName
			}));
			return true;
		}
		try
		{
			xuiParsingDelegate(_target, _value);
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Can not parse input ('",
				_value,
				"') into target type ",
				parsingMethodData.NativeParseType.FullName,
				" for attribute '",
				_attributeName,
				"' on ",
				_target.GetType().FullName,
				": Conversion to target type failed"
			}));
			Log.Exception(ex);
		}
		return true;
	}

	public bool TryGetTypeConverterFromString(Type _outputType, out XuiParsingConverterFromStringDelegate _converterDelegate)
	{
		if (this.convertersFromString.TryGetValue(_outputType, out _converterDelegate))
		{
			return true;
		}
		if (!_outputType.IsEnum)
		{
			return false;
		}
		Type type;
		if (!ParsingMethodCache.tryGetParsingConverterFromStringDelegate(this.genericEnumTryParseMethod.MakeGenericMethod(new Type[] { _outputType }), out _converterDelegate, out type))
		{
			return false;
		}
		this.convertersFromString[type] = _converterDelegate;
		return true;
	}

	public bool TryGetTypeConverterFromType(Type _outputType, Type _inputType, out XuiParsingConverterTypeToTypeDelegate _converterDelegate)
	{
		Dictionary<Type, XuiParsingConverterTypeToTypeDelegate> dictionary;
		if (!this.convertersTypeToType.TryGetValue(_outputType, out dictionary))
		{
			_converterDelegate = null;
			return false;
		}
		return dictionary.TryGetValue(_inputType, out _converterDelegate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetParsingObjectDelegate(MethodInfo _method, out XuiParsingDelegate _func, out Type _inputType)
	{
		_func = null;
		_inputType = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating parsing method wrapper: MethodInfo null");
			return false;
		}
		if (_method.ReturnType != typeof(void))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing method wrapper: Method has return value (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		ParameterInfo[] parameters = _method.GetParameters();
		if (parameters.Length != 1)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing method wrapper: Method has invalid number of parameters, must be 1 (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		_inputType = parameters[0].ParameterType;
		MethodInfo methodInfo = new Func<MethodInfo, XuiParsingDelegate>(ParsingMethodCache.<tryGetParsingObjectDelegate>g__GetParsingObjectDelegateCore|14_0<XUiController, object>).Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { _method.DeclaringType, _inputType });
		_func = (XuiParsingDelegate)methodInfo.Invoke(null, new object[] { _method });
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetParsingConverterFromStringDelegate(MethodInfo _method, out XuiParsingConverterFromStringDelegate _func, out Type _outputType)
	{
		_func = null;
		_outputType = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: MethodInfo null");
			return false;
		}
		if (_method.ReturnType != typeof(bool))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing converter method wrapper: Method has no bool return value (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		ParameterInfo[] parameters = _method.GetParameters();
		if (parameters.Length != 2)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing converter method wrapper: Method has invalid number of parameters, must be 2 (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		if (parameters[0].ParameterType != typeof(string))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing converter method wrapper: Method has invalid first parameter, must be string (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		if (!parameters[1].IsOut)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing converter method wrapper: Method second parameter must be declared 'out' (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		_outputType = parameters[1].ParameterType.GetElementType();
		MethodInfo methodInfo = new Func<MethodInfo, XuiParsingConverterFromStringDelegate>(ParsingMethodCache.<tryGetParsingConverterFromStringDelegate>g__GetParsingConverterDelegateCore|15_0<object>).Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { _outputType });
		_func = (XuiParsingConverterFromStringDelegate)methodInfo.Invoke(null, new object[] { _method });
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetParsingConverterTypeToTypeDelegate(MethodInfo _method, out XuiParsingConverterTypeToTypeDelegate _func, out Type _outputType, out Type _inputType)
	{
		_func = null;
		_outputType = null;
		_inputType = null;
		if (_method == null)
		{
			Log.Error("[XUi] Failed creating parsing converter method wrapper: MethodInfo null");
			return false;
		}
		if (_method.ReturnType != typeof(bool))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing converter method wrapper: Method has no bool return value (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		ParameterInfo[] parameters = _method.GetParameters();
		if (parameters.Length != 2)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing converter method wrapper: Method has invalid number of parameters, must be 2 (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		if (!parameters[1].IsOut)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed creating parsing converter method wrapper: Method second parameter must be declared 'out' (",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				")"
			}));
			return false;
		}
		_inputType = parameters[0].ParameterType;
		_outputType = parameters[1].ParameterType.GetElementType();
		MethodInfo methodInfo = new Func<MethodInfo, XuiParsingConverterTypeToTypeDelegate>(ParsingMethodCache.<tryGetParsingConverterTypeToTypeDelegate>g__GetParsingConverterDelegateCore|17_0<object, object>).Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { _outputType, _inputType });
		_func = (XuiParsingConverterTypeToTypeDelegate)methodInfo.Invoke(null, new object[] { _method });
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <.ctor>g__TypeFoundCallback|9_0(Type _type, bool _hasMultiple, XuiXmlAttributeConvertersClassAttribute _xmlParsingConvertersClassAttribute)
	{
		ReflectionHelpers.GetMethodsWithAttribute<XuiXmlAttributeConverterAttribute>(_type, new Action<MethodInfo, bool, XuiXmlAttributeConverterAttribute>(this.<.ctor>g__MethodFoundCallback|9_1), true, false, true);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <.ctor>g__MethodFoundCallback|9_1(MethodInfo _method, bool _hasMultiple, XuiXmlAttributeConverterAttribute _xmlParsingConverterAttribute)
	{
		if (_method.GetParameters().Length < 1 || !(_method.GetParameters()[0].ParameterType == typeof(string)))
		{
			XuiParsingConverterTypeToTypeDelegate xuiParsingConverterTypeToTypeDelegate;
			Type type;
			Type type2;
			ParsingMethodCache.tryGetParsingConverterTypeToTypeDelegate(_method, out xuiParsingConverterTypeToTypeDelegate, out type, out type2);
			Dictionary<Type, XuiParsingConverterTypeToTypeDelegate> dictionary;
			if (!this.convertersTypeToType.TryGetValue(type, out dictionary))
			{
				dictionary = new Dictionary<Type, XuiParsingConverterTypeToTypeDelegate>();
				this.convertersTypeToType[type] = dictionary;
			}
			if (!dictionary.TryAdd(type2, xuiParsingConverterTypeToTypeDelegate))
			{
				Log.Warning(string.Concat(new string[]
				{
					"[XUi] Input type (",
					type2.FullName,
					") -> output type (",
					type.FullName,
					") converter already defined before. New implementation: ",
					_method.DeclaringType.FullName,
					".",
					_method.Name
				}));
			}
			return;
		}
		XuiParsingConverterFromStringDelegate xuiParsingConverterFromStringDelegate;
		Type type3;
		if (!ParsingMethodCache.tryGetParsingConverterFromStringDelegate(_method, out xuiParsingConverterFromStringDelegate, out type3))
		{
			return;
		}
		this.convertersFromString[type3] = xuiParsingConverterFromStringDelegate;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static XuiParsingDelegate <tryGetParsingObjectDelegate>g__GetParsingObjectDelegateCore|14_0<TInstance, TValue>(MethodInfo _methodInner) where TInstance : class
	{
		Action<TInstance, TValue> typedFunc = (Action<TInstance, TValue>)Delegate.CreateDelegate(typeof(Action<TInstance, TValue>), _methodInner);
		return delegate(IXUiElement _instance, object _value)
		{
			typedFunc((TInstance)((object)_instance), (TValue)((object)_value));
		};
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static XuiParsingConverterFromStringDelegate <tryGetParsingConverterFromStringDelegate>g__GetParsingConverterDelegateCore|15_0<TValue>(MethodInfo _methodInner)
	{
		ParsingMethodCache.RawConverterFromStringDelegate<TValue> typedFunc = (ParsingMethodCache.RawConverterFromStringDelegate<TValue>)Delegate.CreateDelegate(typeof(ParsingMethodCache.RawConverterFromStringDelegate<TValue>), _methodInner);
		return delegate(string _inputValue, out object _outputValue)
		{
			TValue tvalue;
			bool flag = typedFunc(_inputValue, out tvalue);
			_outputValue = tvalue;
			return flag;
		};
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static XuiParsingConverterTypeToTypeDelegate <tryGetParsingConverterTypeToTypeDelegate>g__GetParsingConverterDelegateCore|17_0<TOutput, TInput>(MethodInfo _methodInner)
	{
		ParsingMethodCache.RawConverterTypeToTypeDelegate<TOutput, TInput> typedFunc = (ParsingMethodCache.RawConverterTypeToTypeDelegate<TOutput, TInput>)Delegate.CreateDelegate(typeof(ParsingMethodCache.RawConverterTypeToTypeDelegate<TOutput, TInput>), _methodInner);
		return delegate(object _inputValue, out object _outputValue)
		{
			TOutput toutput;
			bool flag = typedFunc((TInput)((object)_inputValue), out toutput);
			_outputValue = toutput;
			return flag;
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, ParsingMethodCache.ParsingMethodTypeCache> cachePerType = new Dictionary<Type, ParsingMethodCache.ParsingMethodTypeCache>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, XuiParsingConverterFromStringDelegate> convertersFromString = new Dictionary<Type, XuiParsingConverterFromStringDelegate>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, Dictionary<Type, XuiParsingConverterTypeToTypeDelegate>> convertersTypeToType = new Dictionary<Type, Dictionary<Type, XuiParsingConverterTypeToTypeDelegate>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly MethodInfo genericEnumTryParseMethod;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ParsingMethodCache instance;

	public class ParsingMethodData
	{
		public ParsingMethodData(Type _controllerOrViewType, string _attributeName, string _definingController, Type _nativeParseType, XuiParsingDelegate _nativeParserDelegate)
		{
			this.controllerOrViewType = _controllerOrViewType;
			this.attributeName = _attributeName;
			this.DefiningController = _definingController;
			this.NativeParseType = _nativeParseType;
			this.nativeParserDelegate = _nativeParserDelegate;
		}

		public bool TryGetDelegateForSourceType(Type _sourceType, out XuiParsingDelegate _delegate)
		{
			if (_sourceType == this.NativeParseType)
			{
				_delegate = this.nativeParserDelegate;
				return true;
			}
			if (this.sourceTypeDelegates.TryGetValue(_sourceType, out _delegate))
			{
				return _delegate != null;
			}
			if (this.NativeParseType == typeof(string))
			{
				_delegate = delegate(IXUiElement _instance, object _value)
				{
					this.nativeParserDelegate(_instance, _value.ToString());
				};
				this.sourceTypeDelegates[_sourceType] = _delegate;
				return true;
			}
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Warning(string.Concat(new string[]
				{
					"[XUi] Trying to map from ",
					_sourceType.FullName,
					" to ",
					this.NativeParseType.FullName,
					" for attribute '",
					this.attributeName,
					"' on ",
					this.controllerOrViewType.FullName
				}));
			}
			XuiParsingConverterTypeToTypeDelegate typeToTypeConverter;
			if (ParsingMethodCache.Instance.TryGetTypeConverterFromType(this.NativeParseType, _sourceType, out typeToTypeConverter))
			{
				_delegate = delegate(IXUiElement _instance, object _value)
				{
					object obj;
					if (!typeToTypeConverter(_value, out obj))
					{
						Log.Error(string.Format("[XUi] Can not parse input ('{0}', type {1}) into target type {2} for attribute '{3}' on {4}: Conversion to target type failed. --- hierarchy: {5}", new object[]
						{
							_value,
							_value.GetType().FullName,
							this.NativeParseType.FullName,
							this.attributeName,
							this.controllerOrViewType.FullName,
							_instance.GetXuiHierarchy()
						}));
						return;
					}
					this.nativeParserDelegate(_instance, obj);
				};
				this.sourceTypeDelegates[_sourceType] = _delegate;
				return true;
			}
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out(string.Concat(new string[]
				{
					"[XUi] Conversion from source type (",
					_sourceType.FullName,
					") into target type (",
					this.NativeParseType.FullName,
					") requires parsing from string! (Attribute '",
					this.attributeName,
					"' on ",
					this.controllerOrViewType.FullName,
					")"
				}));
			}
			XuiParsingConverterFromStringDelegate converter;
			if (!ParsingMethodCache.Instance.TryGetTypeConverterFromString(this.NativeParseType, out converter))
			{
				Log.Error(string.Concat(new string[]
				{
					"[XUi] Can not parse input type ('",
					_sourceType.FullName,
					"') into target type ",
					this.NativeParseType.FullName,
					" for attribute '",
					this.attributeName,
					"' on ",
					this.controllerOrViewType.FullName,
					": No converter for target type"
				}));
				_delegate = null;
				this.sourceTypeDelegates[_sourceType] = _delegate;
				return false;
			}
			_delegate = delegate(IXUiElement _instance, object _value)
			{
				string text = _value.ToString();
				object obj2;
				if (!converter(text, out obj2))
				{
					Log.Error(string.Format("[XUi] Can not parse input ('{0}', type {1}) into target type {2} for attribute '{3}' on {4}: Conversion to target type failed. --- hierarchy: {5}", new object[]
					{
						_value,
						_value.GetType().FullName,
						this.NativeParseType.FullName,
						this.attributeName,
						this.controllerOrViewType.FullName,
						_instance.GetXuiHierarchy()
					}));
					return;
				}
				this.nativeParserDelegate(_instance, obj2);
			};
			this.sourceTypeDelegates[_sourceType] = _delegate;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerOrViewType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string attributeName;

		public readonly string DefiningController;

		public readonly Type NativeParseType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XuiParsingDelegate nativeParserDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<Type, XuiParsingDelegate> sourceTypeDelegates = new Dictionary<Type, XuiParsingDelegate>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ParsingMethodTypeCache
	{
		public ParsingMethodTypeCache(Type _controllerOrViewType)
		{
			this.controllerOrViewType = _controllerOrViewType;
			this.initCacheForController();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initCacheForController()
		{
			this.<initCacheForController>g__RecurseTypeTree|3_0(this.controllerOrViewType);
		}

		public bool TryGetParsingDelegate(string _attributeName, out ParsingMethodCache.ParsingMethodData _parsingDelegate)
		{
			ParsingMethodCache.ParsingMethodData parsingMethodData;
			if (this.methods.TryGetValue(_attributeName, out parsingMethodData))
			{
				_parsingDelegate = parsingMethodData;
				return true;
			}
			_parsingDelegate = null;
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
			ReflectionHelpers.GetMethodsWithAttribute<XuiXmlAttributeAttribute>(_type, new Action<MethodInfo, bool, XuiXmlAttributeAttribute>(this.<initCacheForController>g__MethodFoundCallback|3_2), true, true, false);
			ReflectionHelpers.GetPropertiesWithAttribute<XuiXmlAttributeAttribute>(_type, new Action<PropertyInfo, bool, XuiXmlAttributeAttribute>(this.<initCacheForController>g__PropertyFoundCallback|3_1), true, true, false);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__PropertyFoundCallback|3_1(PropertyInfo _property, bool _hasMultiple, XuiXmlAttributeAttribute _xmlParsingConverterAttribute)
		{
			MethodInfo setMethod = _property.GetSetMethod(true);
			if (setMethod == null)
			{
				Log.Error(string.Concat(new string[]
				{
					"[XUi] Failed creating parsing property wrapper: Property has no setter (",
					_property.DeclaringType.FullName,
					".",
					_property.Name,
					")"
				}));
			}
			this.<initCacheForController>g__MethodFoundCallback|3_2(setMethod, _hasMultiple, _xmlParsingConverterAttribute);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__MethodFoundCallback|3_2(MethodInfo _method, bool _hasMultiple, XuiXmlAttributeAttribute _parsingAttribute)
		{
			XuiParsingDelegate xuiParsingDelegate;
			Type type;
			if (!ParsingMethodCache.tryGetParsingObjectDelegate(_method, out xuiParsingDelegate, out type))
			{
				return;
			}
			ParsingMethodCache.ParsingMethodData parsingMethodData = new ParsingMethodCache.ParsingMethodData(this.controllerOrViewType, _parsingAttribute.AttributeName, _method.DeclaringType.FullName, type, xuiParsingDelegate);
			ParsingMethodCache.ParsingMethodData parsingMethodData2;
			if (!_parsingAttribute.Override && this.methods.TryGetValue(_parsingAttribute.AttributeName, out parsingMethodData2))
			{
				Log.Warning(string.Concat(new string[] { "[XUi] Child class (", parsingMethodData.DefiningController, ") overriding parent (", parsingMethodData2.DefiningController, ") parsing definition of '", _parsingAttribute.AttributeName, "'" }));
			}
			this.methods[_parsingAttribute.AttributeName] = parsingMethodData;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerOrViewType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, ParsingMethodCache.ParsingMethodData> methods = new Dictionary<string, ParsingMethodCache.ParsingMethodData>(64, StringComparer.Ordinal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool RawConverterFromStringDelegate<T>(string _in, out T _out);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool RawConverterTypeToTypeDelegate<TOutput, TInput>(TInput _in, out TOutput _out);
}
