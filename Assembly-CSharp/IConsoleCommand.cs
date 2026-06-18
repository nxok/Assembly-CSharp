using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public interface IConsoleCommand
{
	bool IsExecuteOnClient { get; }

	int DefaultPermissionLevel { get; }

	bool AllowedInMainMenu { get; }

	DeviceFlag AllowedDeviceTypes { get; }

	DeviceFlag AllowedDeviceTypesClient { get; }

	bool CanExecuteForDevice
	{
		get
		{
			if (!Submission.Enabled)
			{
				return true;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return this.AllowedDeviceTypesClient.IsCurrent();
			}
			return this.AllowedDeviceTypes.IsCurrent();
		}
	}

	string[] GetCommands();

	string PrimaryCommand { get; }

	string GetDescription();

	string GetHelp();

	void Execute(List<string> _params, CommandSenderInfo _senderInfo);
}
