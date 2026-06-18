using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestDescriptionWindow : XUiController
{
	public Quest CurrentQuest
	{
		get
		{
			return this.currentQuest;
		}
		set
		{
			this.currentQuest = value;
			this.questClass = ((value != null) ? QuestClass.GetQuest(this.currentQuest.ID) : null);
			base.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		this.xuiQuestDescriptionLabel = Localization.Get("xuiDescriptionLabel", false, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "questdescription")
		{
			value = ((this.currentQuest != null) ? this.currentQuest.GetParsedText(this.questClass.Description) : "");
			return true;
		}
		if (bindingName == "questcategory")
		{
			value = ((this.currentQuest != null) ? this.questClass.Category : "");
			return true;
		}
		if (bindingName == "questsubtitle")
		{
			value = ((this.currentQuest != null) ? this.questClass.SubTitle : "");
			return true;
		}
		if (bindingName == "questtitle")
		{
			value = ((this.currentQuest != null) ? this.questtitleFormatter.Format(this.questClass.Category, this.questClass.SubTitle, (this.currentQuest.GetSharedWithCount() == 0) ? "" : ("(" + this.currentQuest.GetSharedWithCount().ToString() + ")")) : this.xuiQuestDescriptionLabel);
			return true;
		}
		if (bindingName == "sharedbyname")
		{
			if (this.currentQuest == null)
			{
				value = "";
			}
			else
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.currentQuest.SharedOwnerID);
				if (playerDataFromEntityID != null)
				{
					value = GameUtils.SafeStringFormat(playerDataFromEntityID.PlayerName.DisplayName);
				}
				else
				{
					value = "";
				}
			}
			return true;
		}
		if (!(bindingName == "showempty"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = (this.currentQuest == null).ToString();
		return true;
	}

	public void SetQuest(XUiC_QuestEntry questEntry)
	{
		this.entry = questEntry;
		if (this.entry != null)
		{
			this.CurrentQuest = this.entry.Quest;
			return;
		}
		this.CurrentQuest = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestEntry entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiQuestDescriptionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string, string> questtitleFormatter = new CachedStringFormatter<string, string, string>((string _s, string _s1, string _s2) => string.Format("{0} : {1} {2}", _s, _s1, _s2));
}
