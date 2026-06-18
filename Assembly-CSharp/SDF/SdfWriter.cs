using System;
using System.Collections.Generic;
using System.IO;

namespace SDF
{
	public static class SdfWriter
	{
		public static void Write(Stream fs, Dictionary<string, SdfTag> sdfTags)
		{
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(fs);
				pooledBinaryWriter.Seek(0, SeekOrigin.Begin);
				foreach (KeyValuePair<string, SdfTag> keyValuePair in sdfTags)
				{
					SdfTag value = keyValuePair.Value;
					pooledBinaryWriter.Write((byte)value.TagType);
					pooledBinaryWriter.Write((short)value.Name.Length);
					pooledBinaryWriter.Write(value.Name);
					value.WritePayload(pooledBinaryWriter);
				}
			}
		}
	}
}
