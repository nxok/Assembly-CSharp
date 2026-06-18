using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.GameData
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Internal)]
	public class EntityClass : AbsRestApi
	{
		public EntityClass(Web _parent)
			: base(null)
		{
			JsonWriter jsonWriter = default(JsonWriter);
			jsonWriter.WriteBeginArray();
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
			{
				int num3;
				EntityClass entityClass;
				keyValuePair.Deconstruct(out num3, out entityClass);
				int num4 = num3;
				EntityClass entityClass2 = entityClass;
				if (num > 0)
				{
					jsonWriter.WriteValueSeparator();
				}
				num++;
				string entityClassName = entityClass2.entityClassName;
				EntityClass.UserSpawnType userSpawnType = entityClass2.userSpawnType;
				jsonWriter.WriteRaw(EntityClass.jsonKeyName);
				jsonWriter.WriteString(entityClassName);
				jsonWriter.WriteRaw(EntityClass.jsonKeyId);
				jsonWriter.WriteInt32(num4);
				if (entityClass2.userSpawnType != EntityClass.UserSpawnType.None)
				{
					num2++;
					jsonWriter.WriteRaw(EntityClass.jsonKeyCommandIndex);
					jsonWriter.WriteInt32(num2);
				}
				jsonWriter.WriteRaw(EntityClass.jsonKeyManualSpawnType);
				jsonWriter.WriteString(userSpawnType.ToStringCached<EntityClass.UserSpawnType>());
				jsonWriter.WriteEndObject();
			}
			jsonWriter.WriteEndArray();
			this.allClassesSerialized = jsonWriter.ToUtf8ByteArray();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(this.allClassesSerialized);
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		public override int DefaultPermissionLevel()
		{
			return 2000;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyId = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("id");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyCommandIndex = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("commandId");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyManualSpawnType = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("manualSpawnType");

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly byte[] allClassesSerialized;
	}
}
