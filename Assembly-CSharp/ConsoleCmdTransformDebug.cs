using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTransformDebug : ConsoleCmdAbstract
{
	public ConsoleCmdTransformDebug()
	{
		this.ToggleDebugging();
	}

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
		return new string[] { "transformdebug", "tdbg" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Transform Debugging";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "tdbg - Toggle Transform Debugging";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			this.ToggleDebugging();
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.getHelp());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleDebugging()
	{
		if (!this.m_empty)
		{
			Log.Out("Creating TransformDebug GameObject");
			this.m_empty = new GameObject("TransformDebug");
			this.m_empty.AddComponent<TransformDebug>();
			Object.DontDestroyOnLoad(this.m_empty);
			return;
		}
		Log.Out("Destroying TransformDebug GameObject");
		Object.Destroy(this.m_empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EMPTY_NAME = "TransformDebug";

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject m_empty;
}
