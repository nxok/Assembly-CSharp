using System;
using System.Collections.Generic;

namespace Services
{
	public class ServiceProvider
	{
		public static ServiceProvider Instance
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public static void Init()
		{
			if (ServiceProvider.Instance != null)
			{
				Log.Out("ServiceProvider has already exists");
				return;
			}
			ServiceProvider.Instance = new ServiceProvider();
			ServiceProvider.Instance.InternalInit();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void InternalInit()
		{
			this._services = new Dictionary<Type, object>();
		}

		public void Register(Type type, object obj)
		{
			this._services.Add(type, obj);
		}

		public T Get<T>()
		{
			return (T)((object)this._services[typeof(T)]);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<Type, object> _services;
	}
}
