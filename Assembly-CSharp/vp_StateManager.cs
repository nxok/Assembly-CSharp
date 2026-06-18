using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_StateManager
{
	public vp_StateManager(vp_Component component, List<vp_State> states)
	{
		this.m_States = states;
		this.m_Component = component;
		this.m_Component.RefreshDefaultState();
		this.m_StateIds = new Dictionary<string, int>(StringComparer.CurrentCulture);
		foreach (vp_State vp_State in this.m_States)
		{
			vp_State.StateManager = this;
			if (!this.m_StateIds.ContainsKey(vp_State.Name))
			{
				this.m_StateIds.Add(vp_State.Name, this.m_States.IndexOf(vp_State));
			}
			else
			{
				string[] array = new string[7];
				array[0] = "Warning: ";
				int num = 1;
				Type type = this.m_Component.GetType();
				array[num] = ((type != null) ? type.ToString() : null);
				array[2] = " on '";
				array[3] = this.m_Component.name;
				array[4] = "' has more than one state named: '";
				array[5] = vp_State.Name;
				array[6] = "'. Only the topmost one will be used.";
				Debug.LogWarning(string.Concat(array));
				this.m_States[this.m_DefaultId].StatesToBlock.Add(this.m_States.IndexOf(vp_State));
			}
			if (vp_State.Preset == null)
			{
				vp_State.Preset = new vp_ComponentPreset();
			}
			if (vp_State.TextAsset != null)
			{
				vp_State.Preset.LoadFromTextAsset(vp_State.TextAsset);
			}
		}
		this.m_DefaultId = this.m_States.Count - 1;
	}

	public void ImposeBlockingList(vp_State blocker)
	{
		if (blocker == null)
		{
			return;
		}
		if (blocker.StatesToBlock == null)
		{
			return;
		}
		if (this.m_States == null)
		{
			return;
		}
		foreach (int num in blocker.StatesToBlock)
		{
			this.m_States[num].AddBlocker(blocker);
		}
	}

	public void RelaxBlockingList(vp_State blocker)
	{
		if (blocker == null)
		{
			return;
		}
		if (blocker.StatesToBlock == null)
		{
			return;
		}
		if (this.m_States == null)
		{
			return;
		}
		foreach (int num in blocker.StatesToBlock)
		{
			this.m_States[num].RemoveBlocker(blocker);
		}
	}

	public void SetState(string state, bool setEnabled = true)
	{
		if (!vp_StateManager.AppPlaying())
		{
			return;
		}
		if (!this.m_StateIds.TryGetValue(state, out this.m_TargetId))
		{
			return;
		}
		if (this.m_TargetId == this.m_DefaultId && !setEnabled)
		{
			Debug.LogWarning(vp_StateManager.m_DefaultStateNoDisableMessage);
			return;
		}
		this.m_States[this.m_TargetId].Enabled = setEnabled;
		this.CombineStates();
		this.m_Component.Refresh();
	}

	public void Reset()
	{
		if (!vp_StateManager.AppPlaying())
		{
			return;
		}
		foreach (vp_State vp_State in this.m_States)
		{
			vp_State.Enabled = false;
		}
		this.m_States[this.m_DefaultId].Enabled = true;
		this.m_TargetId = this.m_DefaultId;
		this.CombineStates();
	}

	public void CombineStates()
	{
		for (int i = this.m_States.Count - 1; i > -1; i--)
		{
			if ((i == this.m_DefaultId || (this.m_States[i].Enabled && !this.m_States[i].Blocked && !(this.m_States[i].TextAsset == null))) && this.m_States[i].Preset != null && !(this.m_States[i].Preset.ComponentType == null))
			{
				vp_ComponentPreset.Apply(this.m_Component, this.m_States[i].Preset);
			}
		}
	}

	public bool IsEnabled(string state)
	{
		return vp_StateManager.AppPlaying() && this.m_StateIds.TryGetValue(state, out this.m_TargetId) && this.m_States[this.m_TargetId].Enabled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool AppPlaying()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public vp_Component m_Component;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<vp_State> m_States;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> m_StateIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_AppNotPlayingMessage = "Error: StateManager can only be accessed while application is playing.";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_DefaultStateNoDisableMessage = "Warning: The 'Default' state cannot be disabled.";

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_DefaultId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_TargetId;
}
