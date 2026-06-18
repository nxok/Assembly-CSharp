using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBents : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "bents" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switches block entities on/off or counts them";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Use 'on' or 'off', or 'info' to count";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Specify 'on', 'off' or 'info'");
			return;
		}
		if (_params[0] == "on")
		{
			this.setAll(true, null);
			return;
		}
		if (_params[0] == "off")
		{
			this.setAll(false, null);
			return;
		}
		if (_params[0] == "info")
		{
			this.bentsPerName.Clear();
			int num = this.countAll(this.bentsPerName, null);
			int num2 = 1;
			foreach (KeyValuePair<string, int> keyValuePair in this.bentsPerName)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format(" {0,3}. {1} = {2}", num2, keyValuePair.Key, keyValuePair.Value));
				num2++;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total: " + num.ToString());
			return;
		}
		if (_params[0] == "cullon")
		{
			int num3 = 0;
			MicroStopwatch microStopwatch = new MicroStopwatch();
			foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num3 += chunk.EnableInsideBlockEntities(true);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num3.ToString() + " to ON took " + microStopwatch.ElapsedMilliseconds.ToString());
			return;
		}
		if (_params[0] == "culloff")
		{
			int num4 = 0;
			MicroStopwatch microStopwatch2 = new MicroStopwatch();
			foreach (Chunk chunk2 in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num4 += chunk2.EnableInsideBlockEntities(false);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num4.ToString() + " to OFF took " + microStopwatch2.ElapsedMilliseconds.ToString());
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown parameter");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setAll(bool _bOn, Transform _t = null)
	{
		if (_t == null)
		{
			foreach (ChunkGameObject chunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
			{
				this.setAll(_bOn, chunkGameObject.transform);
			}
			return;
		}
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				child.gameObject.SetActive(_bOn);
			}
			else
			{
				this.setAll(_bOn, child);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int countAll(Dictionary<string, int> _bentsPerName, Transform _t = null)
	{
		int num = 0;
		if (_t == null)
		{
			foreach (ChunkGameObject chunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
			{
				num += this.countAll(_bentsPerName, chunkGameObject.transform);
			}
			return num;
		}
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				num += child.childCount;
				for (int j = 0; j < child.childCount; j++)
				{
					string name = child.GetChild(j).name;
					int num2;
					_bentsPerName[name] = (_bentsPerName.TryGetValue(name, out num2) ? (num2 + 1) : 1);
				}
			}
			else
			{
				num += this.countAll(_bentsPerName, child);
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, int> bentsPerName = new Dictionary<string, int>();
}
