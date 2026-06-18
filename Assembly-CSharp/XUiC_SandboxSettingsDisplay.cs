using System;
using System.Collections.Generic;
using System.Text;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxSettingsDisplay : XUiController
{
	[XuiXmlAttribute("sandbox_values_columns", false)]
	public int SandboxValuesColumns
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.sandboxValuesColumns;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.sandboxValuesColumns = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("sandbox_values_template", false)]
	public string SandBoxValuesTemplate
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.sandBoxValuesTemplate;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				this.sandBoxValuesTemplate = "";
				this.IsDirty = true;
				return;
			}
			this.sandBoxValuesTemplate = value.Replace("%0", "{0}").Replace("%1", "{1}");
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("sandbox_values_template_defaults", false)]
	public string SandBoxValuesTemplateDefaults
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.sandBoxValuesTemplateDefaults;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				this.sandBoxValuesTemplateDefaults = "";
				this.IsDirty = true;
				return;
			}
			this.sandBoxValuesTemplateDefaults = value.Replace("%0", "{0}").Replace("%1", "{1}");
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("sandboxcode", false)]
	[XuiXmlBinding("sandboxcode")]
	public string SandboxCode
	{
		get
		{
			return this.sandboxCode ?? "";
		}
		set
		{
			if (string.Equals(value, this.sandboxCode))
			{
				return;
			}
			this.sandboxName = null;
			this.sandboxCode = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("sandboxname", false)]
	[XuiXmlBinding("sandboxname")]
	public string SandboxName
	{
		get
		{
			return this.sandboxName ?? "";
		}
		set
		{
			if (string.Equals(value, this.sandboxName))
			{
				return;
			}
			this.sandboxCode = null;
			this.sandboxName = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("descriptiontemplate", false)]
	public string SandboxDescriptionTemplate { get; set; }

	[XuiXmlBinding("sandboxcodevalid")]
	public bool Valid
	{
		get
		{
			return this.valid;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.valid == value)
			{
				return;
			}
			this.valid = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("sandboxvalues")]
	public Dictionary<string, string> SandboxValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	} = new Dictionary<string, string>();

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.RefreshBindings();
		if (this.IsDirty)
		{
			this.IsDirty = false;
			this.updateSandboxData();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSandboxData()
	{
		this.SandboxValues.Clear();
		if (this.sandboxName == null && this.sandboxCode == null)
		{
			return;
		}
		SandboxOptionPreset sandboxOptionPreset;
		if (this.sandboxName != null)
		{
			sandboxOptionPreset = SandboxOptionManager.Current.GetPreset(this.sandboxName);
			this.sandboxCode = sandboxOptionPreset.SandboxCode;
		}
		else
		{
			sandboxOptionPreset = new SandboxOptionPreset();
			this.Valid = this.sandboxManager.LoadOptionsFromCode(this.SandboxCode, sandboxOptionPreset);
			if (!this.valid)
			{
				return;
			}
		}
		List<ValueTuple<string, string, bool>> list;
		if (!this.sandboxManager.GetChangedPresetOptions(sandboxOptionPreset, out list))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < this.SandboxValuesColumns; i++)
		{
			for (int j = 0; j < list.Count; j++)
			{
				int num = i + j * this.SandboxValuesColumns;
				if (num >= list.Count)
				{
					break;
				}
				stringBuilder.AppendFormat(list[num].Item3 ? this.SandBoxValuesTemplateDefaults : this.SandBoxValuesTemplate, list[num].Item1, list[num].Item2);
			}
			this.SandboxValues[(i + 1).ToString()] = stringBuilder.ToString();
			stringBuilder.Clear();
		}
		ThreadManager.RunTaskAfterFrames(delegate
		{
			if (this.sandboxPresetValuesScroll != null)
			{
				this.sandboxPresetValuesScroll.ResetPosition();
				return;
			}
			if (this.parentScroll != null)
			{
				this.parentScroll.ResetPosition();
			}
		}, 3);
	}

	[XuiBindEvent("OnPress", "btnCopyCode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCopyCode_OnPress(XUiController _sender, int _mouseButton)
	{
		GUIUtility.systemCopyBuffer = this.SandboxCode;
	}

	[XuiBindEvent("OnPress", "btnSaveAsPreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSaveAsPreset_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_SandboxSettingsSaveAsPreset.Open(LocalPlayerUI.primaryUI.xui, this.SandboxCode, this.SandboxDescriptionTemplate, null);
	}

	[XuiBindComponent("sandboxPresetValues.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView sandboxPresetValuesScroll;

	[XuiBindParent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView parentScroll;

	[XuiBindComponent("btnCopyCode", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCopyCode;

	[XuiBindComponent("btnSaveAsPreset", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSaveAsPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sandboxValuesColumns;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplateDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandboxCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandboxName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valid;
}
