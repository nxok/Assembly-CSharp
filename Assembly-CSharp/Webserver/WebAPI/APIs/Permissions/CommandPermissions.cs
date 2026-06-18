using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.Permissions
{
	[Preserve]
	public class CommandPermissions : AbsRestApi
	{
		public static AdminCommands CommandsInstance
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return GameManager.Instance.adminTools.Commands;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			if (string.IsNullOrEmpty(requestPath))
			{
				jsonWriter.WriteBeginArray();
				bool flag = true;
				foreach (IConsoleCommand consoleCommand in SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands())
				{
					if (!flag)
					{
						jsonWriter.WriteValueSeparator();
					}
					flag = false;
					AdminCommands.CommandPermission adminToolsCommandPermission = CommandPermissions.CommandsInstance.GetAdminToolsCommandPermission(consoleCommand.GetCommands());
					bool flag2 = adminToolsCommandPermission.PermissionLevel == consoleCommand.DefaultPermissionLevel;
					if (adminToolsCommandPermission.Command == "")
					{
						adminToolsCommandPermission = new AdminCommands.CommandPermission(consoleCommand.GetCommands()[0], adminToolsCommandPermission.PermissionLevel);
					}
					this.writeCommandJson(ref jsonWriter, adminToolsCommandPermission, flag2);
				}
				jsonWriter.WriteEndArray();
				AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
				return;
			}
			jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.BadRequest, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeCommandJson(ref JsonWriter _writer, AdminCommands.CommandPermission _commandPermission, bool _isDefault)
		{
			_writer.WriteRaw(CommandPermissions.jsonKeyCommand);
			_writer.WriteString(_commandPermission.Command);
			_writer.WriteRaw(CommandPermissions.jsonKeyPermissionLevel);
			_writer.WriteInt32(_commandPermission.PermissionLevel);
			_writer.WriteRaw(CommandPermissions.jsonKeyIsDefault);
			_writer.WriteBoolean(_isDefault);
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
		{
			string requestPath = _context.RequestPath;
			if (string.IsNullOrEmpty(requestPath))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_COMMAND, null);
				return;
			}
			IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(requestPath, false);
			if (command == null)
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotFound, _jsonInputData, EApiErrorCode.UNKNOWN_COMMAND, null);
				return;
			}
			int num;
			if (!JsonCommons.TryGetJsonField(_jsonInput, "permissionLevel", out num))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, EApiErrorCode.NO_OR_INVALID_PERMISSION_LEVEL, null);
				return;
			}
			CommandPermissions.CommandsInstance.AddCommand(command.GetCommands()[0], num, true);
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.Created, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestDelete(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(requestPath, false);
			if (command == null || !CommandPermissions.CommandsInstance.IsPermissionDefined(command.GetCommands()))
			{
				AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotFound, null, null, null);
				return;
			}
			AbsRestApi.SendEmptyResponse(_context, CommandPermissions.CommandsInstance.RemoveCommand(command.GetCommands()) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound, null, null, null);
		}

		public override bool AllowPostWithId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return true;
			}
		}

		public override int[] DefaultMethodPermissionLevels()
		{
			return new int[] { -2147483647, int.MinValue, int.MinValue, -2147483647, int.MinValue };
		}

		public CommandPermissions()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyCommand = "command";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyPermissionLevel = "permissionLevel";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string propertyIsDefault = "default";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyCommand = JsonWriter.GetEncodedPropertyNameWithBeginObject("command");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPermissionLevel = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("permissionLevel");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyIsDefault = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("default");
	}
}
