using System;
using UnityEngine;

public class QuestGeneratorController : MonoBehaviour
{
	public bool IsRunning
	{
		get
		{
			return this.currentState == QuestGeneratorController.GeneratorStates.On;
		}
	}

	public void SetGeneratorState(QuestGeneratorController.GeneratorStates state, bool isInit)
	{
		if (state != this.currentState)
		{
			this.currentState = state;
			this.updateStateDisplay();
			if (!isInit)
			{
				PrefabInstance.RefreshTriggersInContainingPoi(base.transform.position + Origin.position);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateStateDisplay()
	{
		switch (this.currentState)
		{
		case QuestGeneratorController.GeneratorStates.OnNoQuest:
			this.OffState.SetActive(false);
			this.RebootState.SetActive(false);
			this.EnteringOnState.SetActive(false);
			this.OnState.SetActive(true);
			return;
		case QuestGeneratorController.GeneratorStates.Off:
			this.OffState.SetActive(true);
			this.RebootState.SetActive(false);
			this.EnteringOnState.SetActive(false);
			this.OnState.SetActive(false);
			return;
		case QuestGeneratorController.GeneratorStates.RebootState:
			this.OffState.SetActive(false);
			this.RebootState.SetActive(true);
			this.EnteringOnState.SetActive(false);
			this.OnState.SetActive(false);
			return;
		case QuestGeneratorController.GeneratorStates.EnteringOnState:
			this.OffState.SetActive(false);
			this.RebootState.SetActive(false);
			this.EnteringOnState.SetActive(true);
			this.OnState.SetActive(false);
			return;
		case QuestGeneratorController.GeneratorStates.On:
			this.OffState.SetActive(false);
			this.RebootState.SetActive(false);
			this.EnteringOnState.SetActive(false);
			this.OnState.SetActive(true);
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.updateStateDisplay();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	public Light MainLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public QuestGeneratorController.GeneratorStates currentState;

	public QuestGeneratorController.GeneratorStates TestingCurrentState = QuestGeneratorController.GeneratorStates.Off;

	public GameObject OffState;

	public GameObject RebootState;

	public GameObject EnteringOnState;

	public GameObject OnState;

	public enum GeneratorStates
	{
		OnNoQuest,
		Off,
		RebootState,
		EnteringOnState,
		On
	}
}
