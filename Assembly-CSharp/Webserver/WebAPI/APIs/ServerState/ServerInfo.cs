using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs.ServerState
{
	[Preserve]
	public class ServerInfo : KeyValueListAbs
	{
		public ServerInfo()
			: base("ServerInfo")
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void iterateList(ref JsonWriter _writer, ref bool _first)
		{
			GameServerInfo localServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo;
			IList<GameInfoString> list = EnumUtils.Values<GameInfoString>();
			for (int i = 0; i < list.Count; i++)
			{
				GameInfoString gameInfoString = list[i];
				base.addItem(ref _writer, ref _first, gameInfoString.ToStringCached<GameInfoString>(), localServerInfo.GetValue(gameInfoString));
			}
			IList<GameInfoInt> list2 = EnumUtils.Values<GameInfoInt>();
			for (int j = 0; j < list2.Count; j++)
			{
				GameInfoInt gameInfoInt = list2[j];
				base.addItem(ref _writer, ref _first, gameInfoInt.ToStringCached<GameInfoInt>(), localServerInfo.GetValue(gameInfoInt));
			}
			IList<GameInfoBool> list3 = EnumUtils.Values<GameInfoBool>();
			for (int k = 0; k < list3.Count; k++)
			{
				GameInfoBool gameInfoBool = list3[k];
				base.addItem(ref _writer, ref _first, gameInfoBool.ToStringCached<GameInfoBool>(), localServerInfo.GetValue(gameInfoBool));
			}
		}

		public override int DefaultPermissionLevel()
		{
			return 2000;
		}
	}
}
