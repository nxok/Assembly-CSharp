using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Webserver.FileCache
{
	[Preserve]
	public class InvalidateCachesCmd : ConsoleCmdAbstract
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override string[] getCommands()
		{
			return new string[] { "invalidatecaches" };
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override string getDescription()
		{
			return "Invalidate contents of web file caches";
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override string getHelp()
		{
			return "TODO";
		}

		public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
		{
			ValueTuple<int, int> valueTuple = AbstractCache.InvalidateAllCaches();
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Caches invalidated, dropped {0} files with {1} Bytes", item, item2));
		}
	}
}
