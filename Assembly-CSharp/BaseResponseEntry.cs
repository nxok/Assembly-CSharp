using System;
using UnityEngine.Scripting;

[Preserve]
public class BaseResponseEntry
{
	public string ID { get; set; }

	public BaseResponseEntry.ResponseTypes ResponseType { get; set; }

	public string UniqueID = "";

	public DialogResponse Response;

	public enum ResponseTypes
	{
		Response,
		QuestAdd
	}
}
