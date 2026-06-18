using System;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxPresetSelector : XUiController
{
	public event SandboxPresetSelectionChangedDelegate SandboxPresetSelectionChanged;

	[XuiXmlBinding("description")]
	public string Description
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<SandboxPresetInfo> xuiC_ComboBoxList = this.cbxPreset;
			return ((xuiC_ComboBoxList != null) ? xuiC_ComboBoxList.Value.Description : null) ?? "";
		}
	}

	[XuiXmlBinding("icon")]
	public string Icon
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<SandboxPresetInfo> xuiC_ComboBoxList = this.cbxPreset;
			return ((xuiC_ComboBoxList != null) ? xuiC_ComboBoxList.Value.Icon : null) ?? "";
		}
	}

	[XuiXmlBinding("difficulty")]
	public int Difficulty
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<SandboxPresetInfo> xuiC_ComboBoxList = this.cbxPreset;
			if (xuiC_ComboBoxList == null)
			{
				return 0;
			}
			return xuiC_ComboBoxList.Value.Difficulty;
		}
	}

	[XuiXmlBinding("userpreset")]
	public bool IsUserPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<SandboxPresetInfo> xuiC_ComboBoxList = this.cbxPreset;
			return xuiC_ComboBoxList != null && xuiC_ComboBoxList.Value.IsUserPreset;
		}
	}

	[XuiXmlBinding("custompreset")]
	public bool IsCustomPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<SandboxPresetInfo> xuiC_ComboBoxList = this.cbxPreset;
			return xuiC_ComboBoxList != null && xuiC_ComboBoxList.Value.IsCustomPreset;
		}
	}

	public SandboxPresetInfo SelectedPreset
	{
		get
		{
			return this.cbxPreset.Value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePresetGroups(bool _allowCustom = false)
	{
		string text = this.cbxPresetGroups.Value.InternalName ?? "";
		this.cbxPresetGroups.Elements.Clear();
		foreach (string text2 in this.sandboxManager.GetAllPresetGroups())
		{
			if (!(text2 == "Custom") && this.sandboxManager.GetPresetsForGroup(text2))
			{
				SandboxPresetGroupData sandboxPresetGroupData = new SandboxPresetGroupData(text2, Localization.Get(text2, false, null));
				this.cbxPresetGroups.Elements.Add(sandboxPresetGroupData);
			}
		}
		if (_allowCustom)
		{
			this.cbxPresetGroups.Elements.Add(new SandboxPresetGroupData("Custom", Localization.Get("Custom", false, null)));
		}
		this.selectGroupByName(text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePresetsForSelectedGroup(string _selectPreset = null, bool _forceChangedCallback = false)
	{
		string text = this.cbxPresetGroups.Value.InternalName ?? "";
		SandboxPresetInfo oldSelectedPreset = this.cbxPreset.Value;
		this.cbxPreset.Elements.Clear();
		foreach (SandboxOptionPreset sandboxOptionPreset in this.sandboxManager.SandboxPresets)
		{
			if (!(sandboxOptionPreset.Group != text))
			{
				this.cbxPreset.Elements.Add(new SandboxPresetInfo(sandboxOptionPreset));
			}
		}
		if (_selectPreset != null)
		{
			this.cbxPreset.SelectedIndex = this.cbxPreset.Elements.FindIndex((SandboxPresetInfo _data) => _data.InternalName == _selectPreset);
		}
		else
		{
			this.cbxPreset.SelectedIndex = this.cbxPreset.Elements.FindIndex((SandboxPresetInfo _data) => _data.Equals(oldSelectedPreset));
		}
		if (_forceChangedCallback || !this.cbxPreset.Value.Equals(oldSelectedPreset))
		{
			this.CbxPreset_OnValueChanged(this, oldSelectedPreset, this.cbxPreset.Value);
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxPresetGroups")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPresetGroups_OnValueChanged(XUiController _sender, SandboxPresetGroupData _oldValue, SandboxPresetGroupData _newValue)
	{
		this.updatePresetsForSelectedGroup(null, false);
		this.IsDirty = true;
	}

	[XuiBindEvent("OnValueChanged", "cbxPreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPreset_OnValueChanged(XUiController _sender, SandboxPresetInfo _oldValue, SandboxPresetInfo _newValue)
	{
		this.IsDirty = true;
		SandboxPresetSelectionChangedDelegate sandboxPresetSelectionChanged = this.SandboxPresetSelectionChanged;
		if (sandboxPresetSelectionChanged == null)
		{
			return;
		}
		sandboxPresetSelectionChanged(_oldValue, _newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectGroupByName(string _groupName)
	{
		this.cbxPresetGroups.SelectedIndex = this.cbxPresetGroups.Elements.FindIndex((SandboxPresetGroupData _data) => _data.InternalName == _groupName);
	}

	public void SelectPresetByName(string _presetName, string _tryForceGroup = null, bool _forceChangedCallback = false)
	{
		SandboxOptionPreset sandboxOptionPreset = this.sandboxManager.GetPreset(_presetName);
		string text;
		if (sandboxOptionPreset != null)
		{
			text = sandboxOptionPreset.Group;
		}
		else
		{
			sandboxOptionPreset = this.sandboxManager.GetDefaultPreset();
			text = _tryForceGroup ?? sandboxOptionPreset.Group;
		}
		this.updatePresetGroups(sandboxOptionPreset.IsCustomPreset);
		this.selectGroupByName(text);
		this.updatePresetsForSelectedGroup(sandboxOptionPreset.Name, _forceChangedCallback);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.updatePresetGroups(false);
		this.updatePresetsForSelectedGroup(null, false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	[XuiBindComponent("cbxPresetGroup", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<SandboxPresetGroupData> cbxPresetGroups;

	[XuiBindComponent("cbxPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<SandboxPresetInfo> cbxPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxPresetDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplate;
}
