using System;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.GameData
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Internal)]
	public class Item : AbsRestApi
	{
		public Item(Web _parent)
			: base(null)
		{
			JsonWriter jsonWriter = default(JsonWriter);
			jsonWriter.WriteBeginArray();
			int num = 0;
			for (int i = 0; i < ItemClass.list.Length; i++)
			{
				ItemClass itemClass = ItemClass.list[i];
				if (itemClass != null)
				{
					if (num > 0)
					{
						jsonWriter.WriteValueSeparator();
					}
					num++;
					string name = itemClass.Name;
					string localizedItemName = itemClass.GetLocalizedItemName();
					bool flag = itemClass.IsBlock();
					jsonWriter.WriteRaw(Item.jsonKeyName);
					jsonWriter.WriteString(name);
					jsonWriter.WriteRaw(Item.jsonKeyLocalizedName);
					jsonWriter.WriteString(localizedItemName);
					jsonWriter.WriteRaw(Item.jsonKeyIsBlock);
					jsonWriter.WriteBoolean(flag);
					jsonWriter.WriteEndObject();
				}
			}
			jsonWriter.WriteEndArray();
			this.allItemsSerialized = jsonWriter.ToUtf8ByteArray();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(this.allItemsSerialized);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		public override int DefaultPermissionLevel()
		{
			return 2000;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyLocalizedName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("localizedName");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyIsBlock = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("isBlock");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly byte[] allItemsSerialized;
	}
}
