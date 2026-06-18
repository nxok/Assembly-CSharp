using System;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;

public static class InputUtils
{
	public static void EnableAllPlayerActions(bool _enable)
	{
		if (ActionSetManager.DebugLevel != ActionSetManager.EDebugLevel.Off)
		{
			Log.Out("EnableAllPlayerActions: " + _enable.ToString());
		}
		if (!_enable)
		{
			InputUtils.previousState.Clear();
			using (IEnumerator<PlayerActionsBase> enumerator = PlatformManager.NativePlatform.Input.ActionSets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PlayerActionsBase playerActionsBase = enumerator.Current;
					if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
					{
						Log.Out(string.Format("PAS: {0} IsInDict {1}", playerActionsBase, InputUtils.previousState.ContainsKey(playerActionsBase)));
					}
					if (!InputUtils.previousState.ContainsKey(playerActionsBase))
					{
						if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
						{
							Log.Out(string.Format("Disabling: {0} was {1}", playerActionsBase, playerActionsBase.Enabled));
						}
						InputUtils.previousState.Add(playerActionsBase, playerActionsBase.Enabled);
						playerActionsBase.Enabled = false;
					}
				}
				return;
			}
		}
		foreach (PlayerActionsBase playerActionsBase2 in PlatformManager.NativePlatform.Input.ActionSets)
		{
			if (InputUtils.previousState.ContainsKey(playerActionsBase2))
			{
				if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
				{
					Log.Out(string.Format("PrevState contains: {0} was {1}", playerActionsBase2, InputUtils.previousState[playerActionsBase2]));
				}
				playerActionsBase2.Enabled = InputUtils.previousState[playerActionsBase2];
			}
			else
			{
				if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
				{
					Log.Out(string.Format("PrevState does not contain: {0}", playerActionsBase2));
				}
				playerActionsBase2.Enabled = true;
			}
		}
	}

	public static bool IsMac
	{
		get
		{
			if (InputUtils.isMac == null)
			{
				RuntimePlatform platform = Application.platform;
				InputUtils.isMac = new bool?(platform == null || platform == 1);
			}
			return InputUtils.isMac.Value;
		}
	}

	public static bool ControlKeyPressed
	{
		get
		{
			if (!InputUtils.IsMac)
			{
				return Input.GetKey(306) || Input.GetKey(305);
			}
			return Input.GetKey(310) || Input.GetKey(309);
		}
	}

	public static bool ShiftKeyPressed
	{
		get
		{
			return Input.GetKey(304) || Input.GetKey(303);
		}
	}

	public static bool AltKeyPressed
	{
		get
		{
			return Input.GetKey(308) || Input.GetKey(307);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<PlayerActionSet, bool> previousState = new Dictionary<PlayerActionSet, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool? isMac;
}
