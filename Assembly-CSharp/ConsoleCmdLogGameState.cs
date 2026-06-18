using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLogGameState : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "loggamestate", "lgs" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Log the current state of the game";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Writes information on the current state of the game (like memory usage,\nentities) to the log file. The section will use the message parameter\nin its header.\nUsage:\n   loggamestate <message> [true/false]\nMessage is a string that will be included in the header of the generated\nlog section. The optional boolean parameter specifies if this command\nshould be run on the client (true) instead of the server (false) which\nis the default.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1 || _params.Count > 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1 or 2, found " + _params.Count.ToString() + ".");
			return;
		}
		string text = _params[0];
		bool flag = false;
		if (_params.Count == 2)
		{
			try
			{
				flag = ConsoleHelper.ParseParamBool(_params[1], false);
			}
			catch (ArgumentException)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid boolean value.");
				return;
			}
		}
		if (flag)
		{
			if (_senderInfo.RemoteClientInfo == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Second parameter may only be set to \"true\" if the command is executed from a game client.");
				return;
			}
			_senderInfo.RemoteClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("lgs \"" + text + "\"", true));
			return;
		}
		else
		{
			MicroStopwatch microStopwatch = new MicroStopwatch();
			this.WriteHeader(text);
			this.WriteGeneric();
			this.WriteCounts();
			this.WriteEntities();
			this.WritePlayers();
			this.WriteThreads();
			this.WriteUnityObjects();
			this.WriteGameObjects();
			this.WriteFooter(text);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Wrote game state to game log file, header includes \"{0}\", took {1} ms", text, microStopwatch.ElapsedMilliseconds));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteGeneric()
	{
		ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements((GameManager.Instance.World != null) ? GameManager.Instance.World.worldTime : 0UL);
		int item = valueTuple.Item1;
		int item2 = valueTuple.Item2;
		int item3 = valueTuple.Item3;
		int num = (int)Time.timeSinceLevelLoad;
		int num2 = num % 60;
		int num3 = num / 60 % 60;
		int num4 = num / 3600;
		this.newSection("Generic information");
		this.printLine("System time: {0}", new object[] { DateTime.Now.ToString("HH:mm:ss") });
		this.printLine("Game time:   Day {0}, {1:00}:{2:00}", new object[] { item, item2, item3 });
		this.printLine("Game uptime: {0:00}:{1:00}:{2:00}", new object[] { num4, num3, num2 });
		this.printLine("FPS:         {0}", new object[] { GameManager.Instance.fps.Counter.ToCultureInvariantString("F1") });
		this.printLine("Heap:        {0} MiB / max {1} MiB", new object[]
		{
			GC.GetTotalMemory(false) / 1024L / 1024L,
			GameManager.MaxMemoryConsumption / 1024L / 1024L
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteCounts()
	{
		this.newSection("Object counts");
		this.printLine("Active GameObjs:  {0}", new object[] { Object.FindObjectsByType<GameObject>(0).Length });
		this.printLine("Script instances: {0}", new object[] { Object.FindObjectsByType<MonoBehaviour>(0).Length });
		this.printLine("Total Objects:    {0}", new object[] { Object.FindObjectsByType<Object>(0).Length });
		if (GameManager.Instance.World != null)
		{
			this.printLine("Chunks:     {0}", new object[] { Chunk.InstanceCount });
			this.printLine("ChunkGOs:   {0}", new object[] { GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount() });
			this.printLine("Players:    {0}", new object[] { GameManager.Instance.World.Players.Count });
			this.printLine("Zombies:    {0}", new object[] { GameStats.GetInt(EnumGameStats.EnemyCount) });
			this.printLine("Entities:   {0} in world, {1} loaded", new object[]
			{
				GameManager.Instance.World.Entities.Count,
				Entity.InstanceCount
			});
			this.printLine("Items:      {0}", new object[] { EntityItem.ItemInstanceCount });
			this.printLine("ChunkObs:   {0}", new object[] { GameManager.Instance.World.m_ChunkManager.m_ObservedEntities.Count });
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteEntities()
	{
		if (GameManager.Instance.World != null)
		{
			this.newSection("Entities");
			int num = 0;
			for (int i = GameManager.Instance.World.Entities.list.Count - 1; i >= 0; i--)
			{
				Entity entity = GameManager.Instance.World.Entities.list[i];
				this.printLine("{0,3}. id={1}, name={2}, pos={3}, lifetime={4}, remote={5}, dead={6}", new object[]
				{
					++num,
					entity.entityId,
					entity.ToString(),
					entity.GetPosition().ToCultureInvariantString(),
					entity.lifetime.ToCultureInvariantString("F1"),
					entity.isEntityRemote,
					entity.IsDead()
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WritePlayers()
	{
		if (GameManager.Instance.World != null)
		{
			this.newSection("Players");
			int num = 0;
			foreach (KeyValuePair<int, EntityPlayer> keyValuePair in GameManager.Instance.World.Players.dict)
			{
				string text = "<unknown>";
				PlatformUserIdentifierAbs platformUserIdentifierAbs = null;
				PlatformUserIdentifierAbs platformUserIdentifierAbs2 = null;
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(keyValuePair.Key);
				if (clientInfo != null)
				{
					text = clientInfo.ip;
					platformUserIdentifierAbs = clientInfo.PlatformId;
					platformUserIdentifierAbs2 = clientInfo.CrossplatformId;
				}
				this.printLine("{0,3}. id={1}, {2}, pos={3}, remote={4}, pltfmid={5}, crossid={6}, ip={7}, ping={8}", new object[]
				{
					++num,
					keyValuePair.Value.entityId,
					keyValuePair.Value.EntityName,
					keyValuePair.Value.position.ToCultureInvariantString(),
					keyValuePair.Value.isEntityRemote,
					((platformUserIdentifierAbs != null) ? platformUserIdentifierAbs.CombinedString : null) ?? "<unknown>",
					((platformUserIdentifierAbs2 != null) ? platformUserIdentifierAbs2.CombinedString : null) ?? "<unknown>",
					text,
					keyValuePair.Value.pingToServer
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteThreads()
	{
		this.newSection("Threads");
		int num = 0;
		foreach (KeyValuePair<string, ThreadManager.ThreadInfo> keyValuePair in ThreadManager.ActiveThreads)
		{
			this.printLine("{0,3}. {1}", new object[]
			{
				++num,
				keyValuePair.Key
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteUnityObjects()
	{
		SortedDictionary<string, int> sortedDictionary = new SortedDictionary<string, int>();
		Object[] array = Object.FindObjectsByType<Object>(0);
		for (int i = 0; i < array.Length; i++)
		{
			string fullName = array[i].GetType().FullName;
			if (sortedDictionary.ContainsKey(fullName))
			{
				SortedDictionary<string, int> sortedDictionary2 = sortedDictionary;
				string text = fullName;
				int num = sortedDictionary2[text];
				sortedDictionary2[text] = num + 1;
			}
			else
			{
				sortedDictionary[fullName] = 1;
			}
		}
		this.newSection("Unity objects by type");
		foreach (KeyValuePair<string, int> keyValuePair in sortedDictionary)
		{
			this.printLine("{0,5} * {1}", new object[] { keyValuePair.Value, keyValuePair.Key });
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteGameObjects()
	{
		this.newSection("GameObjects");
		ConsoleCmdLogGameState.HierarchyElement hierarchyElement = new ConsoleCmdLogGameState.HierarchyElement("<top>", true);
		foreach (GameObject gameObject in Object.FindObjectsByType<GameObject>(0))
		{
			if (gameObject.transform.parent == null)
			{
				hierarchyElement.childCount += this.TraverseScene(hierarchyElement, gameObject);
			}
		}
		this.PrintGameObjectHierarchy(hierarchyElement, 0, "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int TraverseScene(ConsoleCmdLogGameState.HierarchyElement _parent, GameObject _go)
	{
		ConsoleCmdLogGameState.HierarchyElement hierarchyElement = new ConsoleCmdLogGameState.HierarchyElement(_go.name, _go.activeInHierarchy);
		_parent.children.Add(hierarchyElement);
		foreach (object obj in _go.transform)
		{
			Transform transform = (Transform)obj;
			hierarchyElement.childCount += this.TraverseScene(hierarchyElement, transform.gameObject);
		}
		return hierarchyElement.childCount + 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrintGameObjectHierarchy(ConsoleCmdLogGameState.HierarchyElement _he, int _indentation, string _prefix)
	{
		if (_he.active)
		{
			this.printLine("{0}{1} (children={2})", new object[] { _prefix, _he.name, _he.childCount });
			if (!this.prefixPerLevel.ContainsKey(_indentation + 1))
			{
				string text;
				if (_indentation == 0)
				{
					text = _prefix + ConsoleCmdLogGameState.indentationLastLevel;
				}
				else
				{
					text = ConsoleCmdLogGameState.indentationNormal + _prefix;
				}
				this.prefixPerLevel[_indentation + 1] = text;
			}
			for (int i = 0; i < _he.children.Count; i++)
			{
				ConsoleCmdLogGameState.HierarchyElement hierarchyElement = _he.children[i];
				this.PrintGameObjectHierarchy(hierarchyElement, _indentation + 1, this.prefixPerLevel[_indentation + 1]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteHeader(string _message)
	{
		Console.Out.WriteLine();
		Console.Out.WriteLine(ConsoleCmdLogGameState.startEndSep);
		Console.Out.WriteLine("WRITING GAME STATE: " + _message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteFooter(string _message)
	{
		Console.Out.WriteLine();
		Console.Out.WriteLine("END OF GAME STATE: " + _message);
		Console.Out.WriteLine(ConsoleCmdLogGameState.startEndSep);
		Console.Out.WriteLine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newSection(string _sectionName)
	{
		Console.Out.WriteLine();
		Console.Out.WriteLine(ConsoleCmdLogGameState.sectionSep);
		Console.Out.WriteLine(_sectionName + ":");
		Console.Out.WriteLine();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void print(string _format, params object[] _args)
	{
		Console.Out.Write(string.Format(_format, _args));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void printLine(string _format, params object[] _args)
	{
		Console.Out.WriteLine(string.Format(_format, _args));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, string> prefixPerLevel = new Dictionary<int, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string indentationNormal = "   |  ";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string indentationLastLevel = "   +- ";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string startEndSep = new string('*', 100);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string sectionSep = new string('-', 50);

	[PublicizedFrom(EAccessModifier.Private)]
	public class HierarchyElement
	{
		public HierarchyElement(string _name, bool _active)
		{
			this.name = _name;
			this.active = _active;
		}

		public string name;

		public bool active;

		public int childCount;

		public List<ConsoleCmdLogGameState.HierarchyElement> children = new List<ConsoleCmdLogGameState.HierarchyElement>();
	}
}
