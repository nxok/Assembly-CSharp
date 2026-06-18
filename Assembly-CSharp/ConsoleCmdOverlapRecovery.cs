using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdOverlapRecovery : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "overlap" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggle LocalPlayer's Character Controller Overlap Recovery";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		CharacterControllerUnity characterControllerUnity = ((primaryPlayer != null) ? primaryPlayer.m_characterController : null) as CharacterControllerUnity;
		if (characterControllerUnity != null)
		{
			characterControllerUnity.enableOverlapRecovery = !characterControllerUnity.enableOverlapRecovery;
			Log.Out(string.Format("CharacterController.enableOverlapRecovery set to {0}", characterControllerUnity.enableOverlapRecovery));
		}
	}
}
