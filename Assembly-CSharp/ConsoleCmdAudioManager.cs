using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAudioManager : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "audio" };
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
		return "Watch audio stats";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Just type audio and hit enter for the info.\n";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisplayHelp()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No help yet");
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			this.DisplayHelp();
			return;
		}
		if (_params.Count == 2)
		{
			if (_params[0].EqualsCaseInsensitive("occlusion"))
			{
				if (_params[1].EqualsCaseInsensitive("on"))
				{
					Manager.occlusionsOn = true;
					return;
				}
				if (_params[1].EqualsCaseInsensitive("off"))
				{
					Manager.occlusionsOn = false;
					return;
				}
			}
			else
			{
				if (_params[0].EqualsCaseInsensitive("hitdelay"))
				{
					int num = 0;
					int.TryParse(_params[1], out num);
					EntityAlive.HitDelay = (ulong)((long)num);
					return;
				}
				if (_params[0].EqualsCaseInsensitive("hitdis"))
				{
					float num2 = 0f;
					StringParsers.TryParseFloat(_params[1], out num2);
					EntityAlive.HitSoundDistance = num2;
					return;
				}
				if (_params[0].EqualsCaseInsensitive("play"))
				{
					Manager.Play(GameManager.Instance.World.GetPrimaryPlayer(), _params[1], 1f, false);
					return;
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid Input");
			this.DisplayHelp();
		}
	}
}
