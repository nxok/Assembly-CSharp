using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogQuestResponseEntry : BaseResponseEntry
{
	public DialogQuestResponseEntry(string _questID, string _type, string _returnStatementID, int _listIndex, int _tier)
	{
		base.ID = _questID;
		this.ListIndex = _listIndex;
		base.ResponseType = BaseResponseEntry.ResponseTypes.QuestAdd;
		this.questType = _type;
		this.Tier = _tier;
		this.ReturnStatementID = _returnStatementID;
	}

	public int ListIndex = -1;

	public string ReturnStatementID = "";

	public string questType = "";

	public int Tier = -1;
}
