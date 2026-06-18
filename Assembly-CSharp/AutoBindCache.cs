using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

public class AutoBindCache
{
	public static AutoBindCache Instance
	{
		get
		{
			AutoBindCache autoBindCache;
			if ((autoBindCache = AutoBindCache.instance) == null)
			{
				autoBindCache = (AutoBindCache.instance = new AutoBindCache());
			}
			return autoBindCache;
		}
	}

	public void BindComponents(XUiController _controller)
	{
		this.getCache(_controller).BindComponents(_controller);
	}

	public void BindEvents(XUiController _controller)
	{
		this.getCache(_controller).BindEvents(_controller);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoBindCache.AutoBindTypeCache getCache(XUiController _controller)
	{
		Type type = _controller.GetType();
		AutoBindCache.AutoBindTypeCache autoBindTypeCache;
		if (!this.cachePerType.TryGetValue(type, out autoBindTypeCache))
		{
			autoBindTypeCache = new AutoBindCache.AutoBindTypeCache(type);
			this.cachePerType[type] = autoBindTypeCache;
		}
		return autoBindTypeCache;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, AutoBindCache.AutoBindTypeCache> cachePerType = new Dictionary<Type, AutoBindCache.AutoBindTypeCache>(128);

	[PublicizedFrom(EAccessModifier.Private)]
	public static AutoBindCache instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<XUiController> controllersList = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<XUiView> viewsList = new List<XUiView>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class BindComponentInfo
	{
		public BindComponentInfo(FieldInfo _targetField, AutoBindCache.BindComponentInfo.EFieldXuiType _fieldXuiType, AutoBindCache.BindComponentInfo.EMultiplicity _multiplicity, Type _lookupType, string _lookupId, bool _required, bool _isParentLookup)
		{
			this.targetField = _targetField;
			this.fieldXuiType = _fieldXuiType;
			this.multiplicity = _multiplicity;
			this.lookupType = _lookupType;
			this.lookupId = _lookupId ?? "";
			this.required = _required;
			this.parentLookup = _isParentLookup;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void bindParent(XUiController _controller)
		{
			bool flag;
			object obj;
			if (this.fieldXuiType == AutoBindCache.BindComponentInfo.EFieldXuiType.Controller)
			{
				XUiController xuiController;
				flag = _controller.TryGetParentController(this.lookupType, out xuiController);
				obj = xuiController;
			}
			else
			{
				XUiView xuiView;
				flag = _controller.TryGetParentView(this.lookupType, out xuiView);
				obj = xuiView;
			}
			if (flag)
			{
				this.targetField.SetValue(_controller, obj);
				return;
			}
			if (this.required)
			{
				this.LogNoViewFound(_controller, "Could not find matching controller / view for auto bind parent field.");
				return;
			}
		}

		public void Bind(XUiController _controller)
		{
			if (this.parentLookup)
			{
				this.bindParent(_controller);
				return;
			}
			XUiController xuiController = _controller;
			ReadOnlyMemory<char> readOnlyMemory = this.lookupId.AsMemory();
			for (int i = readOnlyMemory.Span.IndexOf('.'); i > 0; i = readOnlyMemory.Span.IndexOf('.'))
			{
				ReadOnlyMemory<char> readOnlyMemory2 = readOnlyMemory.Slice(0, i);
				readOnlyMemory = readOnlyMemory.Slice(i + 1);
				XUiController xuiController2;
				if (xuiController.TryGetChildController(typeof(XUiController), readOnlyMemory2, out xuiController2))
				{
					xuiController = xuiController2;
				}
				else if (this.required)
				{
					this.LogNoViewFound(_controller, "Could not find matching parent view for auto bind field.");
					return;
				}
			}
			AutoBindCache.BindComponentInfo.EMultiplicity emultiplicity = this.multiplicity;
			if (emultiplicity != AutoBindCache.BindComponentInfo.EMultiplicity.Single)
			{
				if (emultiplicity != AutoBindCache.BindComponentInfo.EMultiplicity.Array)
				{
					Log.Error(string.Format("[XUi] Auto bind field: Multiplicity value '{0}' not known.", this.multiplicity));
					return;
				}
				IList list;
				if (this.fieldXuiType == AutoBindCache.BindComponentInfo.EFieldXuiType.Controller)
				{
					list = AutoBindCache.controllersList;
					list.Clear();
					xuiController.GetChildControllers(this.lookupType, readOnlyMemory, AutoBindCache.controllersList);
				}
				else
				{
					list = AutoBindCache.viewsList;
					list.Clear();
					xuiController.GetChildViews(this.lookupType, readOnlyMemory, AutoBindCache.viewsList);
				}
				Array array = Array.CreateInstance(this.targetField.FieldType.GetElementType(), list.Count);
				for (int j = 0; j < list.Count; j++)
				{
					array.SetValue(list[j], j);
				}
				this.targetField.SetValue(_controller, array);
				if (list.Count == 0 && this.required)
				{
					this.LogNoViewFound(_controller, "Could not find any matching views for auto bind field.");
				}
				list.Clear();
				return;
			}
			else
			{
				bool flag;
				object obj;
				if (this.fieldXuiType == AutoBindCache.BindComponentInfo.EFieldXuiType.Controller)
				{
					XUiController xuiController3;
					flag = xuiController.TryGetChildController(this.lookupType, readOnlyMemory, out xuiController3);
					obj = xuiController3;
				}
				else
				{
					XUiView xuiView;
					flag = xuiController.TryGetChildView(this.lookupType, readOnlyMemory, out xuiView);
					obj = xuiView;
				}
				if (flag)
				{
					this.targetField.SetValue(_controller, obj);
					return;
				}
				if (this.required)
				{
					this.LogNoViewFound(_controller, "Could not find matching view for auto bind field.");
					return;
				}
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LogNoViewFound(XUiController _controller, string _message)
		{
			Log.Error(string.IsNullOrEmpty(this.lookupId) ? string.Concat(new string[]
			{
				"[XUi] ",
				_message,
				" ",
				this.targetField.DeclaringType.FullName,
				".",
				this.targetField.Name,
				" (expected type: ",
				this.lookupType.Name,
				"). Hierarchy: ",
				_controller.GetXuiHierarchy()
			}) : string.Concat(new string[]
			{
				"[XUi] ",
				_message,
				" ",
				this.targetField.DeclaringType.FullName,
				".",
				this.targetField.Name,
				" (expected type: ",
				this.lookupType.Name,
				", expected name: '",
				this.lookupId,
				"'). Hierarchy: ",
				_controller.GetXuiHierarchy()
			}));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly FieldInfo targetField;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AutoBindCache.BindComponentInfo.EFieldXuiType fieldXuiType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AutoBindCache.BindComponentInfo.EMultiplicity multiplicity;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type lookupType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string lookupId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool required;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool parentLookup;

		public enum EFieldXuiType
		{
			Controller,
			View
		}

		public enum EMultiplicity
		{
			Single,
			Array
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class BindEventInfo
	{
		public BindEventInfo(EventInfo _eventInfo, FieldInfo _eventComponentField, MethodInfo _eventMethod)
		{
			this.eventInfo = _eventInfo;
			this.eventComponentField = _eventComponentField;
			this.eventMethod = _eventMethod;
		}

		public void Bind(XUiController _controller)
		{
			Delegate @delegate = this.eventMethod.CreateDelegate(this.eventInfo.EventHandlerType, _controller);
			if (this.eventComponentField == null)
			{
				this.eventInfo.AddEventHandler(_controller, @delegate);
				return;
			}
			if (!this.eventComponentField.FieldType.IsArray)
			{
				object value = this.eventComponentField.GetValue(_controller);
				if (value == null)
				{
					if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
					{
						Log.Warning("[XUi] Failed binding event on component to method: Component field is null. Field " + this.eventComponentField.DeclaringType.FullName + "." + this.eventComponentField.Name);
					}
					return;
				}
				this.eventInfo.AddEventHandler(value, @delegate);
				return;
			}
			else
			{
				Array array = (Array)this.eventComponentField.GetValue(_controller);
				if (array == null)
				{
					if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
					{
						Log.Warning("[XUi] Failed binding event on components array to method: Component array field is null. Field " + this.eventComponentField.DeclaringType.FullName + "." + this.eventComponentField.Name);
					}
					return;
				}
				for (int i = 0; i < array.Length; i++)
				{
					object value2 = array.GetValue(i);
					if (value2 == null)
					{
						Log.Error(string.Format("[XUi] Failed binding event on component list to method: Entry {0} is null. Field {1}.{2}", i, this.eventComponentField.DeclaringType.FullName, this.eventComponentField.Name));
					}
					else
					{
						this.eventInfo.AddEventHandler(value2, @delegate);
					}
				}
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EventInfo eventInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly FieldInfo eventComponentField;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MethodInfo eventMethod;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class AutoBindTypeCache
	{
		public AutoBindTypeCache(Type _controllerType)
		{
			this.controllerType = _controllerType;
			this.initCacheForController();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initCacheForController()
		{
			this.<initCacheForController>g__RecurseTypeTree|4_0(this.controllerType);
		}

		public void BindComponents(XUiController _controller)
		{
			for (int i = 0; i < this.fields.Count; i++)
			{
				this.fields[i].Bind(_controller);
			}
		}

		public void BindEvents(XUiController _controller)
		{
			for (int i = 0; i < this.events.Count; i++)
			{
				this.events[i].Bind(_controller);
			}
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__RecurseTypeTree|4_0(Type _type)
		{
			if (_type == null)
			{
				return;
			}
			this.<initCacheForController>g__RecurseTypeTree|4_0(_type.BaseType);
			ReflectionHelpers.GetFieldsWithAttribute<XuiBindComponentAttribute>(_type, new Action<FieldInfo, bool, XuiBindComponentAttribute>(this.<initCacheForController>g__ComponentFieldFoundCallback|4_2), true, true, false);
			ReflectionHelpers.GetFieldsWithAttribute<XuiBindParentAttribute>(_type, new Action<FieldInfo, bool, XuiBindParentAttribute>(this.<initCacheForController>g__ParentFieldFoundCallback|4_1), true, true, false);
			ReflectionHelpers.GetMethodsWithAttribute<XuiBindEventAttribute>(_type, new Action<MethodInfo, bool, XuiBindEventAttribute>(this.<initCacheForController>g__EventMethodFoundCallback|4_4), true, true, false);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__ParentFieldFoundCallback|4_1(FieldInfo _fieldInfo, bool _hasMultiple, XuiBindParentAttribute _attribute)
		{
			this.<initCacheForController>g__FieldFound|4_3(_fieldInfo, _hasMultiple, null, _attribute.Required, true);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__ComponentFieldFoundCallback|4_2(FieldInfo _fieldInfo, bool _hasMultiple, XuiBindComponentAttribute _attribute)
		{
			this.<initCacheForController>g__FieldFound|4_3(_fieldInfo, _hasMultiple, _attribute.XmlElementName, _attribute.Required, false);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__FieldFound|4_3(FieldInfo _fieldInfo, bool _hasMultiple, string _xmlElementName, bool _required, bool _isParentLookup)
		{
			Type fieldType = _fieldInfo.FieldType;
			bool isArray = fieldType.IsArray;
			if (_isParentLookup && isArray)
			{
				throw new Exception("[XUi] Field marked as XuiBindParent is an array. Field " + _fieldInfo.DeclaringType.FullName + "." + _fieldInfo.Name);
			}
			Type type = (isArray ? fieldType.GetElementType() : fieldType);
			AutoBindCache.BindComponentInfo.EFieldXuiType efieldXuiType;
			if (typeof(XUiController).IsAssignableFrom(type))
			{
				efieldXuiType = AutoBindCache.BindComponentInfo.EFieldXuiType.Controller;
			}
			else
			{
				if (!typeof(XUiView).IsAssignableFrom(type))
				{
					throw new Exception("[XUi] Field marked as XuiBindComponent is neither a XUiController nor a XUiView or descendant. Field " + _fieldInfo.DeclaringType.FullName + "." + _fieldInfo.Name);
				}
				efieldXuiType = AutoBindCache.BindComponentInfo.EFieldXuiType.View;
			}
			AutoBindCache.BindComponentInfo.EMultiplicity emultiplicity = (isArray ? AutoBindCache.BindComponentInfo.EMultiplicity.Array : AutoBindCache.BindComponentInfo.EMultiplicity.Single);
			AutoBindCache.BindComponentInfo bindComponentInfo = new AutoBindCache.BindComponentInfo(_fieldInfo, efieldXuiType, emultiplicity, type, _xmlElementName, _required, _isParentLookup);
			this.fields.Add(bindComponentInfo);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <initCacheForController>g__EventMethodFoundCallback|4_4(MethodInfo _methodInfo, bool _hasMultiple, XuiBindEventAttribute _attribute)
		{
			Type declaringType = _methodInfo.DeclaringType;
			foreach (XuiBindEventAttribute xuiBindEventAttribute in _methodInfo.GetCustomAttributes<XuiBindEventAttribute>())
			{
				string componentFieldName = xuiBindEventAttribute.ComponentFieldName;
				FieldInfo fieldInfo = null;
				Type type;
				if (string.IsNullOrEmpty(componentFieldName))
				{
					type = declaringType;
				}
				else
				{
					fieldInfo = declaringType.GetField(componentFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (fieldInfo == null)
					{
						Log.Error(string.Concat(new string[] { "[XUi] Event method component field '", componentFieldName, "' not found on type ", declaringType.FullName, ". Method ", declaringType.FullName, ".", _methodInfo.Name }));
						break;
					}
					Type fieldType = fieldInfo.FieldType;
					type = (fieldType.IsArray ? fieldType.GetElementType() : fieldType);
				}
				string targetEvent = xuiBindEventAttribute.TargetEvent;
				if (string.IsNullOrEmpty(targetEvent))
				{
					Log.Error("[XUi] Event method without non-empty event name. Method " + declaringType.FullName + "." + _methodInfo.Name);
					break;
				}
				EventInfo @event = type.GetEvent(targetEvent, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (@event == null)
				{
					Log.Error(string.Concat(new string[] { "[XUi] Event method target event '", targetEvent, "' not found on type '", type.FullName, "'. Method ", declaringType.FullName, ".", _methodInfo.Name }));
					break;
				}
				if (!ReflectionHelpers.MethodCompatibleWithDelegate(@event.EventHandlerType, _methodInfo, false))
				{
					Log.Error(string.Concat(new string[] { "[XUi] Event method incompatible with target event '", type.FullName, ".", targetEvent, "'. Method ", declaringType.FullName, ".", _methodInfo.Name }));
					break;
				}
				AutoBindCache.BindEventInfo bindEventInfo = new AutoBindCache.BindEventInfo(@event, fieldInfo, _methodInfo);
				this.events.Add(bindEventInfo);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Type controllerType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<AutoBindCache.BindComponentInfo> fields = new List<AutoBindCache.BindComponentInfo>(64);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<AutoBindCache.BindEventInfo> events = new List<AutoBindCache.BindEventInfo>();
	}
}
