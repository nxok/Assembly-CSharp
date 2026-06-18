using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.LiveData;

namespace Webserver.WebAPI.APIs.WorldState
{
	[Preserve]
	[PublicizedFrom(EAccessModifier.Internal)]
	public class Animal : AbsRestApi
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteBeginArray();
			List<EntityAnimal> list = this.entities;
			lock (list)
			{
				Animals.Instance.Get(this.entities);
				for (int i = 0; i < this.entities.Count; i++)
				{
					if (i > 0)
					{
						jsonWriter.WriteValueSeparator();
					}
					EntityAlive entityAlive = this.entities[i];
					Vector3i vector3i = new Vector3i(entityAlive.GetPosition());
					jsonWriter.WriteRaw(Animal.jsonKeyId);
					jsonWriter.WriteInt32(entityAlive.entityId);
					jsonWriter.WriteRaw(Animal.jsonKeyName);
					jsonWriter.WriteString((!string.IsNullOrEmpty(entityAlive.EntityName)) ? entityAlive.EntityName : string.Format("animal class #{0}", entityAlive.entityClass));
					jsonWriter.WriteRaw(Animal.jsonKeyPosition);
					JsonCommons.WriteVector3I(ref jsonWriter, vector3i);
					jsonWriter.WriteEndObject();
				}
			}
			jsonWriter.WriteEndArray();
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		public Animal()
			: base(null)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<EntityAnimal> entities = new List<EntityAnimal>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyId = JsonWriter.GetEncodedPropertyNameWithBeginObject("id");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("name");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyPosition = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("position");
	}
}
