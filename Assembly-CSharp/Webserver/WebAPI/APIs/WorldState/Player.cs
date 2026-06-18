using System;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.Permissions;

namespace Webserver.WebAPI.APIs.WorldState
{
	[Preserve]
	public class Player : AbsRestApi
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			bool flag = PermissionUtils.CanViewAllPlayers(_context.PermissionLevel);
			WebConnection connection = _context.Connection;
			PlatformUserIdentifierAbs platformUserIdentifierAbs = ((connection != null) ? connection.UserId : null);
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(Player.jsonPlayersKey);
			jsonWriter.WriteBeginArray();
			int num = 0;
			if (string.IsNullOrEmpty(requestPath))
			{
				for (int i = 0; i < SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.Count; i++)
				{
					ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List[i];
					this.writePlayerJson(ref jsonWriter, ref num, clientInfo.PlatformId, flag, platformUserIdentifierAbs);
				}
			}
			else
			{
				int num2;
				ClientInfo clientInfo2;
				if (!int.TryParse(requestPath, out num2) || (clientInfo2 = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num2)) == null)
				{
					jsonWriter.WriteEndArray();
					jsonWriter.WriteEndObject();
					AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.NotFound, null, null, null);
					return;
				}
				this.writePlayerJson(ref jsonWriter, ref num, clientInfo2.PlatformId, flag, platformUserIdentifierAbs);
			}
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writePlayerJson(ref JsonWriter _writer, ref int _written, PlatformUserIdentifierAbs _nativeUserId, bool _allowViewAll, PlatformUserIdentifierAbs _requesterNativeUserId)
		{
			if (!_allowViewAll && (_requesterNativeUserId == null || !_requesterNativeUserId.Equals(_nativeUserId)))
			{
				return;
			}
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(_nativeUserId);
			if (clientInfo == null)
			{
				Log.Warning("[Web] Player.GET: ClientInfo null");
				return;
			}
			int entityId = clientInfo.entityId;
			EntityPlayer entityPlayer;
			GameManager.Instance.World.Players.dict.TryGetValue(entityId, out entityPlayer);
			if (entityPlayer == null)
			{
				Log.Warning("[Web] Player.GET: EntityPlayer null");
				return;
			}
			if (_written > 0)
			{
				_writer.WriteValueSeparator();
			}
			_written++;
			bool flag = true;
			_writer.WriteRaw(Player.jsonEntityIdKey);
			_writer.WriteInt32(entityId);
			_writer.WriteRaw(Player.jsonNameKey);
			_writer.WriteString(clientInfo.playerName);
			_writer.WriteRaw(Player.jsonPlatformIdKey);
			JsonCommons.WritePlatformUserIdentifier(ref _writer, _nativeUserId);
			_writer.WriteRaw(Player.jsonCrossplatformIdKey);
			JsonCommons.WritePlatformUserIdentifier(ref _writer, clientInfo.CrossplatformId);
			_writer.WriteRaw(Player.jsonTotalPlayTimeKey);
			_writer.WriteNull();
			_writer.WriteRaw(Player.jsonLastOnlineKey);
			_writer.WriteNull();
			_writer.WriteRaw(Player.jsonOnlineKey);
			_writer.WriteBoolean(flag);
			_writer.WriteRaw(Player.jsonIpKey);
			if (flag)
			{
				_writer.WriteString(clientInfo.ip);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteRaw(Player.jsonPingKey);
			if (flag)
			{
				_writer.WriteInt32(clientInfo.ping);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteRaw(Player.jsonPositionKey);
			if (flag)
			{
				JsonCommons.WriteVector3(ref _writer, entityPlayer.GetPosition());
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteRaw(Player.jsonLevelKey);
			_writer.WriteInt32(entityPlayer.Progression.Level);
			_writer.WriteRaw(Player.jsonHealthKey);
			_writer.WriteInt32(entityPlayer.Health);
			_writer.WriteRaw(Player.jsonStaminaKey);
			_writer.WriteSingle(entityPlayer.Stamina);
			_writer.WriteRaw(Player.jsonScoreKey);
			_writer.WriteInt32(entityPlayer.Score);
			_writer.WriteRaw(Player.jsonDeathsKey);
			_writer.WriteInt32(entityPlayer.Died);
			_writer.WriteRaw(Player.jsonKillsKey);
			_writer.WriteRaw(Player.jsonKillsZombiesKey);
			_writer.WriteInt32(entityPlayer.KilledZombies);
			_writer.WriteRaw(Player.jsonKillsPlayersKey);
			_writer.WriteInt32(entityPlayer.KilledPlayers);
			_writer.WriteEndObject();
			_writer.WriteRaw(Player.jsonBannedKey);
			DateTime dateTime;
			string text;
			bool flag2 = GameManager.Instance.adminTools.Blacklist.IsBanned(_nativeUserId, out dateTime, out text);
			if (!flag2 && clientInfo.CrossplatformId != null)
			{
				flag2 = GameManager.Instance.adminTools.Blacklist.IsBanned(clientInfo.CrossplatformId, out dateTime, out text);
			}
			_writer.WriteRaw(Player.jsonBanActiveKey);
			_writer.WriteBoolean(flag2);
			_writer.WriteRaw(Player.jsonBanReasonKey);
			if (flag2)
			{
				_writer.WriteString(text);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteRaw(Player.jsonBanUntilKey);
			if (flag2)
			{
				JsonCommons.WriteDateTime(ref _writer, dateTime);
			}
			else
			{
				_writer.WriteNull();
			}
			_writer.WriteEndObject();
			_writer.WriteEndObject();
		}

		public override int DefaultPermissionLevel()
		{
			return 2000;
		}

		public Player()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonPlayersKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("players");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonEntityIdKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("entityId");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonNameKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonPlatformIdKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("platformId");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonCrossplatformIdKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("crossplatformId");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonTotalPlayTimeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("totalPlayTimeSeconds");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonLastOnlineKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("lastOnline");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonOnlineKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("online");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonIpKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("ip");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonPingKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("ping");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonPositionKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("position");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonLevelKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("level");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonHealthKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("health");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonStaminaKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("stamina");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonScoreKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("score");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonDeathsKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("deaths");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKillsKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("kills");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKillsZombiesKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("zombies");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKillsPlayersKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("players");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonBannedKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("banned");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonBanActiveKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("banActive");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonBanReasonKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("reason");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonBanUntilKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("until");
	}
}
