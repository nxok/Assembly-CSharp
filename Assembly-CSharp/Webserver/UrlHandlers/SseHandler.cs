using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Webserver.Permissions;
using Webserver.SSE;

namespace Webserver.UrlHandlers
{
	public class SseHandler : AbsHandler
	{
		public SseHandler(string _moduleName = null)
			: base(_moduleName, 0)
		{
			SseHandler.ctorParams[0] = this;
			ReflectionHelpers.FindTypesImplementingBase(typeof(AbsEvent), new Action<Type>(this.apiFoundCallback), false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiFoundCallback(Type _type)
		{
			ConstructorInfo constructor = _type.GetConstructor(SseHandler.ctorTypes);
			if (constructor == null)
			{
				return;
			}
			AbsEvent absEvent = (AbsEvent)constructor.Invoke(SseHandler.ctorParams);
			this.AddEvent(absEvent.Name, absEvent);
		}

		public override void SetBasePathAndParent(Web _parent, string _relativePath)
		{
			base.SetBasePathAndParent(_parent, _relativePath);
			this.queueThead = ThreadManager.StartThread("SSE-Processing_" + this.urlBasePath, new ThreadManager.ThreadFunctionDelegate(this.QueueProcessThread), ThreadPriority.BelowNormal, null, true, false);
		}

		public override void Shutdown()
		{
			base.Shutdown();
			this.shutdown = true;
			this.SignalSendQueue();
		}

		public void AddEvent(string _eventName, AbsEvent _eventInstance)
		{
			this.events.Add(_eventName, _eventInstance);
			AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule("webevent." + _eventName, _eventInstance.DefaultPermissionLevel(), true));
		}

		public override void HandleRequest(RequestContext _context)
		{
			string text = _context.QueryParameters["events"];
			if (string.IsNullOrEmpty(text))
			{
				Log.Warning("[Web] [SSE] In SseHandler.HandleRequest(): No 'events' query parameter given");
				_context.Response.StatusCode = 400;
				return;
			}
			SseClient sseClient;
			try
			{
				sseClient = new SseClient(this, _context);
			}
			catch (Exception ex)
			{
				Log.Error("[Web] [SSE] In SseHandler.HandleRequest(): Could not create client:");
				Log.Exception(ex);
				_context.Response.StatusCode = 500;
				return;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (string text2 in text.Split(',', StringSplitOptions.RemoveEmptyEntries))
			{
				AbsEvent absEvent;
				if (!this.events.TryGetValue(text2, out absEvent))
				{
					Log.Warning("[Web] [SSE] In SseHandler.HandleRequest(): No handler found for event \"" + text2 + "\"");
				}
				else
				{
					num++;
					if (this.IsAuthorizedForEvent(text2, _context.PermissionLevel))
					{
						num2++;
						try
						{
							absEvent.AddListener(sseClient);
						}
						catch (Exception ex2)
						{
							Log.Error("[Web] [SSE] In SseHandler.HandleRequest(): Handler " + absEvent.Name + " threw an exception:");
							Log.Exception(ex2);
							_context.Response.StatusCode = 500;
						}
						num3++;
					}
				}
			}
			if (num == 0)
			{
				_context.Response.StatusCode = 400;
				_context.Response.Close();
				return;
			}
			if (num2 == 0)
			{
				_context.Response.StatusCode = 403;
				WebConnection connection = _context.Connection;
				_context.Response.Close();
				return;
			}
			this.clients.Add(sseClient);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsAuthorizedForEvent(string _eventName, int _permissionLevel)
		{
			return AdminWebModules.Instance.ModuleAllowedWithLevel("webevent." + _eventName, _permissionLevel);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void QueueProcessThread(ThreadManager.ThreadInfo _threadInfo)
		{
			while (!this.shutdown && !_threadInfo.TerminationRequested())
			{
				this.evSendRequest.WaitOne(500);
				foreach (KeyValuePair<string, AbsEvent> keyValuePair in this.events)
				{
					string text;
					AbsEvent absEvent;
					keyValuePair.Deconstruct(out text, out absEvent);
					string text2 = text;
					AbsEvent absEvent2 = absEvent;
					try
					{
						absEvent2.ProcessSendQueue();
					}
					catch (Exception ex)
					{
						Log.Error("[Web] [SSE] '" + text2 + "': Error processing send queue");
						Log.Exception(ex);
					}
				}
				for (int i = this.clients.Count - 1; i >= 0; i--)
				{
					this.clients[i].HandleKeepAlive();
				}
			}
		}

		public void SignalSendQueue()
		{
			this.evSendRequest.Set();
		}

		public void ClientClosed(SseClient _client)
		{
			foreach (KeyValuePair<string, AbsEvent> keyValuePair in this.events)
			{
				string text;
				AbsEvent absEvent;
				keyValuePair.Deconstruct(out text, out absEvent);
				string text2 = text;
				AbsEvent absEvent2 = absEvent;
				try
				{
					absEvent2.ClientClosed(_client);
				}
				catch (Exception ex)
				{
					Log.Error("[Web] [SSE] '" + text2 + "': Error closing client");
					Log.Exception(ex);
				}
			}
			this.clients.Remove(_client);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, AbsEvent> events = new CaseInsensitiveStringDictionary<AbsEvent>();

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreadManager.ThreadInfo queueThead;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AutoResetEvent evSendRequest = new AutoResetEvent(false);

		[PublicizedFrom(EAccessModifier.Private)]
		public bool shutdown;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Type[] ctorTypes = new Type[] { typeof(SseHandler) };

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly object[] ctorParams = new object[1];

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<SseClient> clients = new List<SseClient>();
	}
}
