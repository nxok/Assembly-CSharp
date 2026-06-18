using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldToolsWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_WorldToolsWindow.ID = base.WindowGroup.Id;
		this.btnLevelStartPoint = base.GetChildById("btnLevelStartPoint");
		this.btnLevelStartPoint.GetChildById("clickable").OnPress += this.BtnLevelStartPoint_Controller_OnPress;
		this.cbxBoxSideTransparency = base.GetChildById("cbxBoxSideTransparency").GetChildByType<XUiC_ComboBoxFloat>();
		this.cbxBoxSideTransparency.OnValueChanged += this.CbxBoxSideTransparency_OnValueChanged;
		this.cbxBoxSelectionCaptions = base.GetChildById("cbxBoxSelectionCaptions").GetChildByType<XUiC_ComboBoxBool>();
		this.cbxBoxSelectionCaptions.OnValueChanged += this.CbxBoxSelectionCaptions_OnValueChanged;
		this.cbxBoxSelectionCaptions.Value = true;
		this.cbxBoxPrefabPreviewLimit = base.GetChildById("cbxBoxPrefabPreviewLimit").GetChildByType<XUiC_ComboBoxInt>();
		this.cbxBoxPrefabPreviewLimit.OnValueChanged += this.CbxBoxPrefabPreviewLimit_OnValueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLevelStartPoint_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(100f, 0f);
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i vector3i = World.worldToBlockPos(raycastHitPoint);
			GameManager.Instance.GetSpawnPointList().Add(new SpawnPoint(vector3i));
			SelectionCategory categoryStartPoint = SelectionBoxManager.Instance.CategoryStartPoint;
			Vector3i vector3i2 = vector3i;
			SelectionBox selectionBox = categoryStartPoint.AddBox(vector3i2.ToString() ?? "", vector3i, Vector3i.one, true, false);
			SelectionBoxManager.Instance.SetActive(selectionBox, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxBoxSelectionCaptions_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		SelectionBoxManager.Instance.CategoryDynamicPrefab.SetCaptionVisibility(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxBoxSideTransparency_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		SelectionBoxManager.Instance.AlphaMultiplier = (float)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxBoxPrefabPreviewLimit_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		DynamicPrefabDecorator.PrefabPreviewLimit = (int)_newValue;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.cbxBoxSideTransparency.Value = (double)SelectionBoxManager.Instance.AlphaMultiplier;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.btnLevelStartPoint.ViewComponent.IsVisible = false;
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnLevelStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxBoxSideTransparency;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxBoxSelectionCaptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxBoxPrefabPreviewLimit;
}
