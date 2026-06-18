using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShow : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "show" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Shows custom layers of rendering.";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Enable(ConsoleCmdShow.DebugView dView)
	{
		ConsoleCmdShow.Disable();
		ConsoleCmdShow.enabledKeyword = dView.key;
		Shader.EnableKeyword(ConsoleCmdShow.enabledKeyword);
		ConsoleCmdShow.savedShadowsOption = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality);
		ConsoleCmdShow.savedSSAOOption = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSSAO);
		ConsoleCmdShow.savedDOFOption = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDOF);
		if (dView.disableShadows)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 0);
		}
		if (dView.disableSSAO)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, false);
		}
		if (dView.disableDOF)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, false);
		}
		GameManager.Instance.ApplyAllOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Disable()
	{
		if (ConsoleCmdShow.enabledKeyword.Length < 1)
		{
			return;
		}
		Shader.DisableKeyword(ConsoleCmdShow.enabledKeyword);
		ConsoleCmdShow.enabledKeyword = "";
		GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, ConsoleCmdShow.savedShadowsOption);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, ConsoleCmdShow.savedSSAOOption);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, ConsoleCmdShow.savedDOFOption);
		GameManager.Instance.ApplyAllOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsEnabled(string key)
	{
		return ConsoleCmdShow.enabledKeyword.Length >= 1 && ConsoleCmdShow.enabledKeyword.EqualsCaseInsensitive(key);
	}

	public static void Init()
	{
		for (int i = 0; i < ConsoleCmdShow.Commands.Length; i++)
		{
			Shader.DisableKeyword(ConsoleCmdShow.Commands[i].key);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Switch(ConsoleCmdShow.DebugView dView)
	{
		if (ConsoleCmdShow.IsEnabled(dView.key))
		{
			ConsoleCmdShow.Disable();
			return;
		}
		ConsoleCmdShow.Enable(dView);
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetDescription());
			return;
		}
		if (_params.Count == 1)
		{
			for (int i = 0; i < ConsoleCmdShow.Commands.Length; i++)
			{
				if (_params[0].EqualsCaseInsensitive(ConsoleCmdShow.Commands[i].cmd))
				{
					ConsoleCmdShow.Switch(ConsoleCmdShow.Commands[i]);
					return;
				}
			}
			if (_params[0].EqualsCaseInsensitive("none") || _params[0].EqualsCaseInsensitive("off"))
			{
				ConsoleCmdShow.Disable();
			}
			return;
		}
		if (_params.Count > 1)
		{
			StringParsers.ParseFloat(_params[1], 0, -1, NumberStyles.Any);
		}
		_params[0].EqualsCaseInsensitive("NA");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool DISABLE_SHADOWS = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ENABLE_SHADOWS = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool DISABLE_SSAO = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ENABLE_SSAO = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool DISABLE_DOF = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ENABLE_DOF = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConsoleCmdShow.DebugView[] Commands = new ConsoleCmdShow.DebugView[]
	{
		new ConsoleCmdShow.DebugView("blockAO", "SHOW_BLOCK_AO", ConsoleCmdShow.DISABLE_SHADOWS, ConsoleCmdShow.DISABLE_SSAO, ConsoleCmdShow.DISABLE_DOF),
		new ConsoleCmdShow.DebugView("occlusion", "SHOW_OCCLUSION", ConsoleCmdShow.DISABLE_SHADOWS, ConsoleCmdShow.DISABLE_SSAO, ConsoleCmdShow.DISABLE_DOF),
		new ConsoleCmdShow.DebugView("lighting", "SHOW_LIGHTING", ConsoleCmdShow.ENABLE_SHADOWS, ConsoleCmdShow.ENABLE_SSAO, ConsoleCmdShow.DISABLE_DOF),
		new ConsoleCmdShow.DebugView("normals", "SHOW_NORMALS", ConsoleCmdShow.DISABLE_SHADOWS, ConsoleCmdShow.DISABLE_SSAO, ConsoleCmdShow.DISABLE_DOF)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static string enabledKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static int savedShadowsOption = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool savedSSAOOption = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool savedDOFOption = false;

	public class DebugView
	{
		public DebugView(string _cmd, string _key, bool _disableShadows, bool _disableSSAO, bool _disableDOF)
		{
			this.cmd = _cmd;
			this.key = _key;
			this.disableShadows = _disableShadows;
			this.disableSSAO = _disableSSAO;
			this.disableDOF = _disableDOF;
		}

		public string cmd;

		public string key;

		public bool disableShadows;

		public bool disableSSAO;

		public bool disableDOF;
	}
}
