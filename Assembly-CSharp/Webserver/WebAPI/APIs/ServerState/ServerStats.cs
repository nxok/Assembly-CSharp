using System;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.LiveData;

namespace Webserver.WebAPI.APIs.ServerState
{
	[Preserve]
	public class ServerStats : AbsRestApi
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(ServerStats.jsonKeyGameTime);
			ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			int item3 = valueTuple.Item3;
			JsonCommons.WriteGameTimeObject(ref jsonWriter, item, item2, item3);
			jsonWriter.WriteRaw(ServerStats.jsonKeyPlayers);
			jsonWriter.WriteInt32(GameManager.Instance.World.Players.Count);
			jsonWriter.WriteRaw(ServerStats.jsonKeyHostiles);
			jsonWriter.WriteInt32(Hostiles.Instance.GetCount());
			jsonWriter.WriteRaw(ServerStats.jsonKeyAnimals);
			jsonWriter.WriteInt32(Animals.Instance.GetCount());
			jsonWriter.WriteEndObject();
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		public override int DefaultPermissionLevel()
		{
			return 2000;
		}

		public ServerStats()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyGameTime = JsonWriter.GetEncodedPropertyNameWithBeginObject("gameTime");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPlayers = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("players");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyHostiles = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("hostiles");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyAnimals = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("animals");
	}
}
