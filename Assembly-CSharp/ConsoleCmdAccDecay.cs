using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAccDecay : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "AccDecay", "SetAccDecay", "SetAccuracyDecay", "sad" };
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
		return "Accuracy Decay for guns, show/hide/reset/<Decimal value>";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "reset - apply the default settings for this command\n<Decimal Value> - Sets the decay constant for accuracy calculations using ItemActionRanged";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("ItemActionRanged.DecayConstant: {0}", ItemActionRanged.AccuracyUpdateDecayConstant));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("ItemActionRanged.LogOldAccuracy: {0}", ItemActionRanged.LogOldAccuracy));
			return;
		}
		float num;
		if (float.TryParse(_params[0], out num))
		{
			ItemActionRanged.AccuracyUpdateDecayConstant = num;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Set ItemActionRanged.DecayConstant to {0} and reset old accuracy for checking", num));
			return;
		}
		if (_params[0].ToLower() == "reset")
		{
			ItemActionRanged.LogOldAccuracy = false;
			ItemActionRanged.AccuracyUpdateDecayConstant = 9.1f;
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("ItemActionRanged.DecayConstant: {0}", ItemActionRanged.AccuracyUpdateDecayConstant));
	}
}
