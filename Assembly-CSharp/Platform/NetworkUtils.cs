using System;
using System.Net;

namespace Platform
{
	public static class NetworkUtils
	{
		public static uint ToInt(string _addr)
		{
			return (uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(IPAddress.Parse(_addr).GetAddressBytes(), 0));
		}

		public static string ToAddr(uint _address)
		{
			string[] array = new IPAddress((long)((ulong)_address)).ToString().Split('.', StringSplitOptions.None);
			return string.Concat(new string[]
			{
				array[3],
				".",
				array[2],
				".",
				array[1],
				".",
				array[0]
			});
		}

		public static string BuildGameTags(GameServerInfo _game)
		{
			string text;
			using (PooledMemoryStream pooledMemoryStream = MemoryPools.poolMS.AllocSync(true))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
				{
					pooledBinaryWriter.SetBaseStream(pooledMemoryStream);
					foreach (GameInfoInt gameInfoInt in GameServerInfo.IntInfosInGameTags)
					{
						pooledBinaryWriter.Write7BitEncodedSignedInt(_game.GetValue(gameInfoInt));
					}
					byte b = 0;
					int j;
					for (j = 0; j < GameServerInfo.BoolInfosInGameTags.Length; j++)
					{
						b |= (byte)((_game.GetValue(GameServerInfo.BoolInfosInGameTags[j]) ? 1 : 0) << ((j % 8) & 31));
						if (j % 8 == 7)
						{
							pooledBinaryWriter.Write(b);
							b = 0;
						}
					}
					if (j % 8 != 0)
					{
						pooledBinaryWriter.Write(b);
					}
					text = Convert.ToBase64String(pooledMemoryStream.GetBuffer(), 0, (int)pooledMemoryStream.Length);
				}
			}
			return text;
		}

		public static bool ParseGameTags(string _tags, GameServerInfo _gameInfo)
		{
			return _tags.IndexOf(';') < 0 && NetworkUtils.ParseGameTags2(_tags, _gameInfo);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool ParseGameTags2(string _tags, GameServerInfo _gameInfo)
		{
			byte[] array;
			try
			{
				array = Convert.FromBase64String(_tags);
			}
			catch (Exception)
			{
				Log.Warning(string.Concat(new string[]
				{
					"Parsing gametags for server ",
					_gameInfo.GetValue(GameInfoString.IP),
					":",
					_gameInfo.GetValue(GameInfoInt.Port).ToString(),
					" failed: \"",
					_tags,
					"\""
				}));
				return false;
			}
			bool flag2;
			using (PooledMemoryStream pooledMemoryStream = MemoryPools.poolMS.AllocSync(true))
			{
				pooledMemoryStream.Write(array, 0, array.Length);
				pooledMemoryStream.Position = 0L;
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
				{
					pooledBinaryReader.SetBaseStream(pooledMemoryStream);
					try
					{
						foreach (GameInfoInt gameInfoInt in GameServerInfo.IntInfosInGameTags)
						{
							int num = pooledBinaryReader.Read7BitEncodedSignedInt();
							_gameInfo.SetValue(gameInfoInt, num);
						}
						for (int j = 0; j < GameServerInfo.BoolInfosInGameTags.Length; j += 8)
						{
							byte b = pooledBinaryReader.ReadByte();
							int num2 = 0;
							while (j + num2 < GameServerInfo.BoolInfosInGameTags.Length && num2 < 8)
							{
								GameInfoBool gameInfoBool = GameServerInfo.BoolInfosInGameTags[j + num2];
								bool flag = ((int)b & (1 << num2)) != 0;
								_gameInfo.SetValue(gameInfoBool, flag);
								num2++;
							}
						}
					}
					catch (Exception)
					{
						return false;
					}
					flag2 = true;
				}
			}
			return flag2;
		}
	}
}
