using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine.Profiling;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI
{
	public abstract class AbsRestApi : AbsWebAPI
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public AbsRestApi(string _name = null)
			: this(null, _name)
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbsRestApi(Web _parentWeb, string _name = null)
			: base(_parentWeb, _name)
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void RegisterPermissions()
		{
			AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule(this.CachedApiModuleName, this.DefaultPermissionLevel(), this.DefaultMethodPermissionLevels(), true));
		}

		public sealed override void HandleRequest(RequestContext _context)
		{
			IDictionary<string, object> dictionary = null;
			byte[] array = null;
			if (_context.Request.HasEntityBody)
			{
				Stream inputStream = _context.Request.InputStream;
				array = new byte[_context.Request.ContentLength64];
				inputStream.Read(array, 0, (int)_context.Request.ContentLength64);
				try
				{
					dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(array);
				}
				catch (Exception ex)
				{
					AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "INVALID_BODY", ex);
					return;
				}
			}
			try
			{
				switch (_context.Method)
				{
				case ERequestMethod.GET:
					if (dictionary != null)
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "GET_WITH_BODY", null);
					}
					else
					{
						this.HandleRestGet(_context);
					}
					break;
				case ERequestMethod.POST:
					if (!this.AllowPostWithId && !string.IsNullOrEmpty(_context.RequestPath))
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "POST_WITH_ID", null);
					}
					else if (dictionary == null)
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "POST_WITHOUT_BODY", null);
					}
					else
					{
						this.HandleRestPost(_context, dictionary, array);
					}
					break;
				case ERequestMethod.PUT:
					if (string.IsNullOrEmpty(_context.RequestPath))
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "PUT_WITHOUT_ID", null);
					}
					else if (dictionary == null)
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "PUT_WITHOUT_BODY", null);
					}
					else
					{
						this.HandleRestPut(_context, dictionary, array);
					}
					break;
				case ERequestMethod.DELETE:
					if (string.IsNullOrEmpty(_context.RequestPath))
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, array, "DELETE_WITHOUT_ID", null);
					}
					else if (dictionary != null)
					{
						AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "DELETE_WITH_BODY", null);
					}
					else
					{
						this.HandleRestDelete(_context);
					}
					break;
				default:
					AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, null, "INVALID_METHOD", null);
					break;
				}
			}
			catch (Exception ex2)
			{
				try
				{
					AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.InternalServerError, array, "ERROR_PROCESSING", ex2);
				}
				catch (Exception ex3)
				{
					Log.Error("[Web] In AbsRestApi.HandleRequest(): Handler " + this.Name + " threw an exception while trying to send a previous exception to the client:");
					Log.Exception(ex3);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleRestGet(RequestContext _context)
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, null, "Unsupported", null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, _jsonInputData, "Unsupported", null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleRestPut(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, _jsonInputData, "Unsupported", null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleRestDelete(RequestContext _context)
		{
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.MethodNotAllowed, null, "Unsupported", null);
		}

		public override bool Authorized(RequestContext _context)
		{
			AdminWebModules.WebModule module = AdminWebModules.Instance.GetModule(this.CachedApiModuleName);
			if (module.LevelPerMethod == null)
			{
				return module.LevelGlobal >= _context.PermissionLevel;
			}
			int num = module.LevelPerMethod[(int)_context.Method];
			if (num == -2147483647)
			{
				return false;
			}
			if (num != -2147483648)
			{
				return num >= _context.PermissionLevel;
			}
			return module.LevelGlobal >= _context.PermissionLevel;
		}

		public virtual bool AllowPostWithId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return false;
			}
		}

		public virtual int[] DefaultMethodPermissionLevels()
		{
			return new int[] { -2147483647, int.MinValue, int.MinValue, int.MinValue, int.MinValue };
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static void PrepareEnvelopedResult(out JsonWriter _writer)
		{
			WebUtils.PrepareEnvelopedResult(out _writer);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static void SendEnvelopedResult(RequestContext _context, ref JsonWriter _writer, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
		{
			WebUtils.SendEnvelopedResult(_context, ref _writer, _statusCode, _jsonInputData, _errorCode, _exception);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static void SendEmptyResponse(RequestContext _context, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
		{
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, _statusCode, _jsonInputData, _errorCode, _exception);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static void SendEmptyResponse(RequestContext _context, HttpStatusCode _statusCode, byte[] _jsonInputData, EApiErrorCode _errorCode, Exception _exception = null)
		{
			AbsRestApi.SendEmptyResponse(_context, _statusCode, _jsonInputData, _errorCode.ToStringCached<EApiErrorCode>(), _exception);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly CustomSampler jsonDeserializeSampler = CustomSampler.Create("JSON_Deserialize", false);
	}
}
