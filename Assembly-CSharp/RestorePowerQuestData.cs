using System;
using UnityEngine.Scripting;

[Preserve]
public class RestorePowerQuestData : BaseQuestData
{
	public Vector3i PrefabPosition
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public RestorePowerQuestData(int _questCode, int _entityID, Vector3i _position, string _completeEvent)
	{
		this.CompleteEvent = _completeEvent;
		this.questCode = _questCode;
		this.entityList.Add(_entityID);
		this.PrefabPosition = _position;
	}

	public void UpdatePosition(Vector3i _pos)
	{
		this.PrefabPosition = _pos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void RemoveFromDictionary()
	{
		QuestEventManager.Current.BlockActivateQuestDictionary.Remove(this.questCode);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnRemove(EntityPlayer player)
	{
		if (this.CompleteEvent != "")
		{
			GameEventManager.Current.HandleAction(this.CompleteEvent, null, player, false, this.PrefabPosition, "", "", false, true, "", null, null);
		}
	}

	public string CompleteEvent = "";
}
