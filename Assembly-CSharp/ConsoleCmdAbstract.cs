using System;
using System.Collections.Generic;
using Platform;

public abstract class ConsoleCmdAbstract : IConsoleCommand
{
	public virtual bool IsExecuteOnClient
	{
		get
		{
			return false;
		}
	}

	public virtual int DefaultPermissionLevel
	{
		get
		{
			return 0;
		}
	}

	public virtual bool AllowedInMainMenu
	{
		get
		{
			return false;
		}
	}

	public virtual DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;
		}
	}

	public virtual DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;
		}
	}

	public ConsoleCmdAbstract()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract string[] getCommands();

	public virtual string[] GetCommands()
	{
		string[] array;
		if ((array = this.commandNamesCache) == null)
		{
			array = (this.commandNamesCache = this.getCommands());
		}
		return array;
	}

	public virtual string PrimaryCommand
	{
		get
		{
			string text;
			if ((text = this.primaryCommand) == null)
			{
				text = (this.primaryCommand = this.GetCommands()[0]);
			}
			return text;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract string getDescription();

	public virtual string GetDescription()
	{
		string text;
		if ((text = this.commandDescriptionCache) == null)
		{
			text = (this.commandDescriptionCache = this.getDescription());
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string getHelp()
	{
		return null;
	}

	public virtual string GetHelp()
	{
		string text;
		if ((text = this.commandHelpCache) == null)
		{
			text = (this.commandHelpCache = this.getHelp());
		}
		return text;
	}

	public abstract void Execute(List<string> _params, CommandSenderInfo _senderInfo);

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] commandNamesCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandDescriptionCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandHelpCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string primaryCommand;
}
