using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTurnInDetailsWindow : XUiController
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

	public override void OnOpen()
	{
		base.OnOpen();
		this.CurrentQuest = this.xui.Dialog.QuestTurnIn;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2940790865U)
		{
			if (num <= 1468586257U)
			{
				if (num != 161079706U)
				{
					if (num == 1468586257U)
					{
						if (bindingName == "questsubtitle")
						{
							value = ((this.currentQuest != null) ? this.questClass.SubTitle : "");
							return true;
						}
					}
				}
				else if (bindingName == "sharedbyname")
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
			}
			else if (num != 1985695849U)
			{
				if (num == 2940790865U)
				{
					if (bindingName == "questdescription")
					{
						value = ((this.currentQuest != null) ? this.currentQuest.GetParsedText(this.questClass.Description) : "");
						return true;
					}
				}
			}
			else if (bindingName == "questcategory")
			{
				value = ((this.currentQuest != null) ? this.questClass.Category : "");
				return true;
			}
		}
		else if (num <= 3270262403U)
		{
			if (num != 3047389681U)
			{
				if (num == 3270262403U)
				{
					if (bindingName == "npcportrait")
					{
						if (this.currentQuest == null)
						{
							value = "";
						}
						else
						{
							value = this.NPC.NPCInfo.Portrait;
						}
						return true;
					}
				}
			}
			else if (bindingName == "questtitle")
			{
				value = ((this.currentQuest != null) ? this.questtitleFormatter.Format(this.questClass.Category, this.questClass.SubTitle) : this.xuiQuestDescriptionLabel);
				return true;
			}
		}
		else if (num != 3357817217U)
		{
			if (num != 3846167827U)
			{
				if (num == 4060322893U)
				{
					if (bindingName == "showempty")
					{
						value = (this.currentQuest == null).ToString();
						return true;
					}
				}
			}
			else if (bindingName == "npcname")
			{
				if (this.currentQuest == null)
				{
					value = "";
				}
				else
				{
					value = Localization.Get(this.NPC.EntityName, false, null);
				}
				return true;
			}
		}
		else if (bindingName == "questcompletetext")
		{
			value = ((this.currentQuest != null) ? this.questClass.CompleteText : "Needs real complete text.");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiQuestDescriptionLabel;

	public EntityNPC NPC;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string> questtitleFormatter = new CachedStringFormatter<string, string>((string _s, string _s1) => string.Format("{0} : {1}", _s, _s1));
}
