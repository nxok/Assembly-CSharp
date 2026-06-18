using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCVar : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "cvar" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return false;
		}
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Commands to set, get, track or list CVars.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usages of the commands. Add '-p <playerId>' to any command to apply that command to a remote player. \ncvar get <cvarName>\ncvar set <cvarName> <floatValue>\ncvar track <cvarName> <true|false>\ncvar list <searchFilter>";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		int num = GameManager.Instance.World.GetPrimaryPlayerId();
		if (_senderInfo.RemoteClientInfo != null)
		{
			num = _senderInfo.RemoteClientInfo.entityId;
		}
		string text = _params[0].ToLowerInvariant();
		if (text == "get")
		{
			this.ExecuteGet(_params, num);
			return;
		}
		if (text == "set")
		{
			this.ExecuteSet(_params, num);
			return;
		}
		if (text == "track")
		{
			this.ExecuteTrack(_params, num);
			return;
		}
		if (!(text == "list"))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Valid command not supplied. Use 'get', 'set', 'track' or 'list'.");
			return;
		}
		this.ExecuteList(_params, num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteGet(List<string> _params, int _playerId)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		if (_params.Count >= 4)
		{
			_playerId = this.GetPlayerId(_params);
		}
		EntityPlayer player = this.GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Could not find player matching ID {0}.", _playerId));
			return;
		}
		string text = _params[1];
		bool flag = player.Buffs.HasCustomVar(text);
		float num = (flag ? player.Buffs.GetCustomVar(text) : 0f);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Player {0} has cvar {1}: {2}. Value: {3}", new object[] { player.EntityName, text, flag, num }));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteSet(List<string> _params, int _playerId)
	{
		if (_params.Count < 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		float num;
		if (!float.TryParse(_params[2], out num))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Could not parse '" + _params[2] + "' into float.");
			return;
		}
		int num2 = _playerId;
		if (_params.Count >= 5)
		{
			_playerId = this.GetPlayerId(_params);
		}
		EntityPlayer player = this.GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Could not find player matching ID {0}.", _playerId));
			return;
		}
		string text = _params[1];
		float customVar = player.Buffs.GetCustomVar(text);
		if (_playerId == num2)
		{
			player.Buffs.SetCustomVar(text, num, true, CVarOperation.set);
		}
		else
		{
			player.Buffs.SetCustomVarNetwork(text, num, CVarOperation.set);
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Player {0} cvar {1} set from {2} to {3}", new object[] { player.EntityName, text, customVar, num }));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteTrack(List<string> _params, int _playerId)
	{
		if (_params.Count < 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		bool flag;
		if (!bool.TryParse(_params[2], out flag))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Could not parse '" + _params[2] + "' into bool.");
			return;
		}
		if (_params.Count >= 5)
		{
			_playerId = this.GetPlayerId(_params);
		}
		EntityPlayer player = this.GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Could not find player matching ID {0}.", _playerId));
			return;
		}
		string text = _params[1];
		player.Buffs.TrackCustomVar(text, flag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList(List<string> _params, int _playerId)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not enough arguments supplied.");
			return;
		}
		if (_params.Count >= 5)
		{
			_playerId = this.GetPlayerId(_params);
		}
		EntityPlayer player = this.GetPlayer(_playerId);
		if (player == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Could not find player matching ID {0}.", _playerId));
			return;
		}
		string text = _params[1];
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[] { "Listing CVars for ", player.EntityName, " which contain \"", text, "\"." }));
		foreach (KeyValuePair<string, float> keyValuePair in player.Buffs.EnumerateCustomVars(text, false))
		{
			string text2;
			float num;
			keyValuePair.Deconstruct(out text2, out num);
			string text3 = text2;
			float num2 = num;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("\t{0} : {1}", text3, num2));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetPlayerId(List<string> _params)
	{
		int i = 2;
		while (i < _params.Count - 1)
		{
			if (_params[i].EqualsCaseInsensitive("-p"))
			{
				int num;
				if (int.TryParse(_params[i + 1], out num))
				{
					return num;
				}
				break;
			}
			else
			{
				i++;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer GetPlayer(int _playerId)
	{
		if (_playerId == -1)
		{
			return null;
		}
		EntityPlayer entityPlayer;
		if (!GameManager.Instance.World.Players.dict.TryGetValue(_playerId, out entityPlayer))
		{
			return null;
		}
		return entityPlayer;
	}
}
