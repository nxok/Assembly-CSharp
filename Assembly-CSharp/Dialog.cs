using System;
using System.Collections.Generic;

public class Dialog
{
	public DialogStatement CurrentStatement
	{
		get
		{
			if (this.ChildDialog != null)
			{
				return this.ChildDialog.CurrentStatement;
			}
			return this.currentStatement;
		}
		set
		{
			if (this.ChildDialog != null)
			{
				this.ChildDialog.CurrentStatement = value;
				return;
			}
			this.currentStatement = value;
		}
	}

	public Dialog(string newID)
	{
		this.ID = newID;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DialogStatement GetStatement(string currentStatementID)
	{
		if (this.ChildDialog != null)
		{
			return this.ChildDialog.GetStatement(currentStatementID);
		}
		if (currentStatementID == "")
		{
			currentStatementID = this.StartStatementID;
		}
		for (int i = 0; i < this.Statements.Count; i++)
		{
			if (this.Statements[i].ID == currentStatementID)
			{
				return this.Statements[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DialogResponse GetResponse(string currentResponseID)
	{
		if (this.ChildDialog != null)
		{
			return this.ChildDialog.GetResponse(currentResponseID);
		}
		for (int i = 0; i < this.Responses.Count; i++)
		{
			if (this.Responses[i].ID == currentResponseID)
			{
				return this.Responses[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public DialogStatement GetFirstStatment(EntityPlayer player)
	{
		string text = this.StartStatementID;
		for (int i = 0; i < this.Phases.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < this.Phases[i].RequirementList.Count; j++)
			{
				if (!this.Phases[i].RequirementList[j].CheckRequirement(player, this.CurrentOwner))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				text = this.Phases[i].StartStatementID;
				break;
			}
		}
		for (int k = 0; k < this.Statements.Count; k++)
		{
			if (this.Statements[k].ID == text)
			{
				return this.Statements[k];
			}
		}
		return null;
	}

	public void RestartDialog(EntityPlayer player)
	{
		this.CurrentStatement = this.GetFirstStatment(player);
		this.ChildDialog = null;
	}

	public void SelectResponse(DialogResponse response, EntityPlayer player)
	{
		if (this.ChildDialog != null)
		{
			this.ChildDialog.SelectResponse(response, player);
			return;
		}
		if (response.Actions.Count > 0)
		{
			for (int i = 0; i < response.Actions.Count; i++)
			{
				response.Actions[i].PerformAction(player);
			}
		}
		if (response is DialogResponseQuest)
		{
			DialogResponseQuest dialogResponseQuest = response as DialogResponseQuest;
			QuestClass questClass = dialogResponseQuest.Quest.QuestClass;
			this.CurrentStatement = new DialogStatement("");
			this.CurrentStatement.NextStatementID = dialogResponseQuest.NextStatementID;
			this.CurrentStatement.Text = dialogResponseQuest.Quest.GetParsedText(questClass.StatementText);
			return;
		}
		this.CurrentStatement = this.GetStatement(response.NextStatementID);
	}

	public static void Cleanup()
	{
		Dialog.DialogList.Clear();
	}

	public static void ReloadDialogs()
	{
		Dialog.Cleanup();
		WorldStaticData.Reset("dialogs");
	}

	public static Dictionary<string, Dialog> DialogList = new Dictionary<string, Dialog>();

	public string ID = "";

	public string StartStatementID = "";

	public string StartResponseID = "";

	public List<DialogPhase> Phases = new List<DialogPhase>();

	public List<DialogStatement> Statements = new List<DialogStatement>();

	public List<DialogResponse> Responses = new List<DialogResponse>();

	public EntityNPC CurrentOwner;

	public Dialog ChildDialog;

	public List<QuestEntry> QuestEntryList = new List<QuestEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DialogStatement currentStatement;

	public string currentReturnStatement = "";
}
