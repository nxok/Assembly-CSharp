using System;
using System.Collections.Generic;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSaveDataManagerInfo : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "sdminfo" };
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
		return "SaveDataManager Information";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "sdminfo";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ISaveDataManager saveDataManager = SaveDataUtils.SaveDataManager;
		this.AppendSaveDataManagerInfo(stringBuilder, saveDataManager);
		IPlatformSaveGameProvider saveGameProvider = PlatformManager.MultiPlatform.SaveGameProvider;
		this.AppendSaveGameProviderInfo(stringBuilder, saveGameProvider);
		Log.Out(stringBuilder.TrimEnd().ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSaveDataManagerInfo(StringBuilder builder, ISaveDataManager saveDataManager)
	{
		if (saveDataManager == null)
		{
			builder.AppendLine("No ISaveDataManager available.");
			return;
		}
		builder.AppendLine("Save Data Manager Info:");
		builder.AppendLine(string.Format("\tWrite Mode: {0}", saveDataManager.GetWriteMode()));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSaveGameProviderInfo(StringBuilder builder, IPlatformSaveGameProvider saveGameProvider)
	{
		if (saveGameProvider == null)
		{
			builder.AppendLine("No IPlatformSaveGameProvider available.");
			return;
		}
		builder.AppendLine("Save Game Provider Info:");
		builder.AppendLine(string.Format("\tStatus: {0}", saveGameProvider.Status));
		builder.AppendLine(string.Format("\tShould Backup: {0}", saveGameProvider.ShouldBackup()));
		builder.AppendLine(string.Format("\tShould Commit: {0}", saveGameProvider.ShouldCommit()));
		this.AppendSaveGameProviderSizeInfo(builder, saveGameProvider);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AppendSaveGameProviderSizeInfo(StringBuilder builder, IPlatformSaveGameProvider saveGameProvider)
	{
		bool flag = saveGameProvider.ShouldLimitSize();
		builder.AppendLine(string.Format("\tShould Limit Size: {0}", flag));
		if (!flag)
		{
			return;
		}
		saveGameProvider.UpdateSizes();
		SaveDataSizes sizes = saveGameProvider.GetSizes();
		builder.AppendLine("\tUsed Size      : " + sizes.Used.FormatSize(true));
		builder.AppendLine("\tRemaining Size : " + sizes.Remaining.FormatSize(true));
		builder.AppendLine("\tTotal Size     : " + sizes.Total.FormatSize(true));
	}
}
