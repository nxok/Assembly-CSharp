using System;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver;
using Webserver.WebAPI;

namespace MapRendering.Api
{
	[Preserve]
	public class Map : AbsRestApi
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			string requestPath = _context.RequestPath;
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			if (requestPath == "config")
			{
				this.writeConfig(ref jsonWriter);
				AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
				return;
			}
			AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotImplemented, null, "INVALID_ID", null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void writeConfig(ref JsonWriter writer)
		{
			writer.WriteRaw(Map.jsonKeyEnabled);
			writer.WriteBoolean(MapRenderer.Enabled);
			writer.WriteRaw(Map.jsonKeyMapBlockSize);
			writer.WriteInt32(Constants.MapBlockSize);
			writer.WriteRaw(Map.jsonKeyMaxZoom);
			writer.WriteInt32(Constants.Zoomlevels - 1);
			Vector3i vector3i;
			Vector3i vector3i2;
			GameManager.Instance.World.GetWorldExtent(out vector3i, out vector3i2);
			Vector3i vector3i3 = vector3i2 - vector3i;
			writer.WriteRaw(Map.jsonKeyMapSize);
			JsonCommons.WriteVector3I(ref writer, vector3i3);
			writer.WriteEndObject();
		}

		public override int[] DefaultMethodPermissionLevels()
		{
			return new int[] { -2147483647, 2000, int.MinValue, int.MinValue, int.MinValue };
		}

		public Map()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyEnabled = JsonWriter.GetEncodedPropertyNameWithBeginObject("enabled");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyMapBlockSize = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("mapBlockSize");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyMaxZoom = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("maxZoom");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyMapSize = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("mapSize");
	}
}
