using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignEditorWindow : XUiController
{
	public XUiC_SignEditorWindow.AffectMode CurrentAffectMode
	{
		get
		{
			return this.currentAffectMode;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_SignEditorWindow.ID = base.WindowGroup.Id;
		base.WindowGroup.isEscClosable = false;
		this.canvasView = base.GetChildById("canvasView");
		this.canvasView.OnDrag += this.HandleCanvasDragged;
		this.canvasView.OnScroll += this.HandleCanvasScrolled;
		this.newLayerPanel = base.GetChildByType<XUiC_SignNewLayerPanel>();
		XUiC_SignNewLayerPanel xuiC_SignNewLayerPanel = this.newLayerPanel;
		xuiC_SignNewLayerPanel.OnLayerTypeSelected = (Action<SignData.SignLayer>)Delegate.Combine(xuiC_SignNewLayerPanel.OnLayerTypeSelected, new Action<SignData.SignLayer>(this.HandleLayerTypeSelected));
		this.signMaterial = base.GetChildById("canvasMaterial").ViewComponent as XUiV_Texture;
		this.signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xuiV_Texture = this.signMaterial;
		xuiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.OnWillRender));
		this.signMaterial.Texture = Texture2D.whiteTexture;
		this.previewAspect = (float)this.signMaterial.Size.x / (float)this.signMaterial.Size.y;
		this.pulseOverlayMaterial = base.GetChildById("pulseOverlay").ViewComponent as XUiV_Texture;
		this.pulseOverlayMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xuiV_Texture2 = this.pulseOverlayMaterial;
		xuiV_Texture2.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture2.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.OnPulseOverlayWillRender));
		this.pulseOverlayMaterial.Texture = null;
		this.hoverOverlayMaterial = base.GetChildById("hoverOverlay").ViewComponent as XUiV_Texture;
		this.hoverOverlayMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xuiV_Texture3 = this.hoverOverlayMaterial;
		xuiV_Texture3.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture3.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.OnHoverOverlayWillRender));
		this.hoverOverlayMaterial.Texture = null;
		this.categoryList = base.GetChildByType<XUiC_CategoryList>();
		this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
		this.signLayerGrid = base.GetChildByType<XUiC_SignLayerGrid>();
		XUiC_SignLayerGrid xuiC_SignLayerGrid = this.signLayerGrid;
		xuiC_SignLayerGrid.OnLayerSelected = (Action<int>)Delegate.Combine(xuiC_SignLayerGrid.OnLayerSelected, new Action<int>(this.HandleLayerSelected));
		XUiC_SignLayerGrid xuiC_SignLayerGrid2 = this.signLayerGrid;
		xuiC_SignLayerGrid2.OnLayerHovered = (Action<int>)Delegate.Combine(xuiC_SignLayerGrid2.OnLayerHovered, new Action<int>(this.HandleLayerHovered));
		XUiC_SignLayerGrid xuiC_SignLayerGrid3 = this.signLayerGrid;
		xuiC_SignLayerGrid3.OnMultiSelectionDeselect = (XUiC_SignLayerGrid.MultiSelectionDeselectCallback)Delegate.Combine(xuiC_SignLayerGrid3.OnMultiSelectionDeselect, new XUiC_SignLayerGrid.MultiSelectionDeselectCallback(this.HandleMultiSelectionDeselect));
		this.addWarpSettings = base.GetChildByType<XUiC_AddWarpSettings>();
		XUiC_AddWarpSettings xuiC_AddWarpSettings = this.addWarpSettings;
		xuiC_AddWarpSettings.OnWarpAdded = (Action<string>)Delegate.Combine(xuiC_AddWarpSettings.OnWarpAdded, new Action<string>(this.HandleWarpAdded));
		this.btnSave = (XUiC_SimpleButton)base.GetChildById("btnSave");
		this.btnSaveCopy = (XUiC_SimpleButton)base.GetChildById("btnSaveCopy");
		this.btnQuit = (XUiC_SimpleButton)base.GetChildById("btnQuit");
		this.btnSave.OnPressed += this.BtnSave_OnPressed;
		this.btnSaveCopy.OnPressed += this.BtnSaveCopy_OnPressed;
		this.btnQuit.OnPressed += this.BtnQuit_OnPressed;
		this.txtAspectHorizontal = (XUiC_TextInput)base.GetChildById("txtRatio").GetChildById("valueA");
		this.txtAspectVertical = (XUiC_TextInput)base.GetChildById("txtRatio").GetChildById("valueB");
		this.txtAspectHorizontal.OnChangeHandler += this.OnAspectText_OnChangeHandler;
		this.txtAspectVertical.OnChangeHandler += this.OnAspectText_OnChangeHandler;
		XUiC_SignEditorControl xuiC_SignEditorControl = base.GetChildById("txtRatio") as XUiC_SignEditorControl;
		if (xuiC_SignEditorControl != null)
		{
			xuiC_SignEditorControl.defaultValue = new ValueTuple<float, float>(1f, 1f);
		}
		this.btnAspectSquare = (XUiC_SimpleButton)base.GetChildById("btnAspectSquare");
		this.btnAspectFit = (XUiC_SimpleButton)base.GetChildById("btnAspectFit");
		this.btnAspectFit.OnPressed += this.BtnAspectFit_OnPressed;
		this.btnAspectSquare.OnPressed += this.BtnAspectSquare_OnPressed;
		this.confirmationPrompt = base.GetChildById("confirmation_prompt_controller") as XUiC_ConfirmationPrompt;
		this.txtSignName = base.GetChildById("txtSignName") as XUiC_TextInput;
		this.txtSignName.OnChangeHandler += this.TxtSignName_OnChangeHandler;
		this.txtLayerName = base.GetChildById("txtLayerName") as XUiC_TextInput;
		this.txtLayerName.OnChangeHandler += this.TxtLayerName_OnChangeHandler;
		this.sizeBar = base.GetChildByType<XUiC_SizeBar>();
		this.sizeBar.SetAllowance(600f);
		base.GetChildrenByType<XUiC_SignLayerSettings>(this.allSettings);
		base.GetChildrenByType<XUiC_SignWarpSettings>(this.allWarpSettings);
		base.GetChildrenByType<XUiC_SignColorSettings>(this.allColorSettings);
		this.transformSettings = base.GetChildByType<XUiC_SignTransformSettings>();
		foreach (XUiC_SignLayerSettings xuiC_SignLayerSettings in this.allSettings)
		{
			xuiC_SignLayerSettings.OnPreLayerSettingsChanged = (Action<string, bool>)Delegate.Combine(xuiC_SignLayerSettings.OnPreLayerSettingsChanged, new Action<string, bool>(delegate(string changeDescription, bool forceDirty)
			{
				this.OnBeforeApplyingChange(changeDescription, forceDirty);
			}));
			xuiC_SignLayerSettings.OnLayerSettingsChanged = (Action)Delegate.Combine(xuiC_SignLayerSettings.OnLayerSettingsChanged, new Action(this.MarkChanged));
			XUiC_SignGroupSettings xuiC_SignGroupSettings = xuiC_SignLayerSettings as XUiC_SignGroupSettings;
			if (xuiC_SignGroupSettings != null)
			{
				XUiC_SignGroupSettings xuiC_SignGroupSettings2 = xuiC_SignGroupSettings;
				xuiC_SignGroupSettings2.OnAffectModePressed = (Action<XUiC_SignEditorWindow.AffectMode>)Delegate.Combine(xuiC_SignGroupSettings2.OnAffectModePressed, new Action<XUiC_SignEditorWindow.AffectMode>(this.RequestAffectMode));
				this.OnAffectModeChanged = (Action<XUiC_SignEditorWindow.AffectMode>)Delegate.Combine(this.OnAffectModeChanged, new Action<XUiC_SignEditorWindow.AffectMode>(xuiC_SignGroupSettings.SetAffectModeVisual));
			}
		}
		foreach (XUiC_SignWarpSettings xuiC_SignWarpSettings in this.allWarpSettings)
		{
			xuiC_SignWarpSettings.OnWarpRemoved = (Action)Delegate.Combine(xuiC_SignWarpSettings.OnWarpRemoved, new Action(this.HandleWarpRemoved));
		}
		foreach (XUiC_SignColorSettings xuiC_SignColorSettings in this.allColorSettings)
		{
			xuiC_SignColorSettings.OnMaskModeChanged = (Action)Delegate.Combine(xuiC_SignColorSettings.OnMaskModeChanged, new Action(this.HandleMaskModeChanged));
		}
		this.debugPanel = base.GetChildByType<XUiC_SignDebugPanel>();
		this.sourceSignData = SignData.GetTestSignData("Sign Editor", 0f);
		this.sourceSignData.UnpackGroups(false);
		this.sourceSignId = SignDataManager.Instance.AddSignToLibrary("[I]", this.sourceSignData);
		this.dragAndDropIcon = base.GetChildByType<XUiC_SignLayerDragAndDropIcon>();
		XUiC_SignLayerGrid xuiC_SignLayerGrid4 = this.signLayerGrid;
		xuiC_SignLayerGrid4.OnDragAndDropStarted = (Action<int>)Delegate.Combine(xuiC_SignLayerGrid4.OnDragAndDropStarted, new Action<int>(this.OnDragAndDropStarted));
		XUiC_SignLayerGrid xuiC_SignLayerGrid5 = this.signLayerGrid;
		xuiC_SignLayerGrid5.OnDragAndDropReleased = (Action<int>)Delegate.Combine(xuiC_SignLayerGrid5.OnDragAndDropReleased, new Action<int>(this.OnDragAndDropReleased));
		XUiC_SignLayerGrid xuiC_SignLayerGrid6 = this.signLayerGrid;
		xuiC_SignLayerGrid6.OnAddLayerPressed = (Action<int>)Delegate.Combine(xuiC_SignLayerGrid6.OnAddLayerPressed, new Action<int>(this.BeginAddLayerFlow));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtLayerName_OnChangeHandler(XUiController sender, string text, bool changeFromCode)
	{
		if (changeFromCode)
		{
			return;
		}
		this.signLayerGrid.SetLayerName(text);
		this.MarkChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMultiSelectionDeselect(bool wasPrimary)
	{
		if (!wasPrimary)
		{
			this.RefreshComplexityInfo();
		}
	}

	public static void Open(LocalPlayerUI _playerUi, SignData signData, GlobalSignId signId)
	{
		XUiC_SignEditorWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_SignEditorWindow.ID).GetChildByType<XUiC_SignEditorWindow>();
		childByType.targetCanvas = null;
		childByType.targetEntity = null;
		childByType.sourceSignId = signId;
		childByType.sourceSignData = signData;
		_playerUi.windowManager.Open(XUiC_SignEditorWindow.ID, true);
	}

	public static void Open(LocalPlayerUI _playerUi, TEFeatureCanvas _feature)
	{
		XUiC_SignEditorWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_SignEditorWindow.ID).GetChildByType<XUiC_SignEditorWindow>();
		SignCanvas canvas = _feature.Canvas;
		childByType.targetCanvas = canvas;
		childByType.targetEntity = _feature;
		if (canvas != null && SignDataManager.Instance.TryGetSignData(canvas.SignId, out childByType.sourceSignData))
		{
			childByType.sourceSignId = canvas.SignId;
		}
		else
		{
			childByType.sourceSignId = GlobalSignId.InvalidId;
			childByType.sourceSignData = null;
		}
		_playerUi.windowManager.Open(XUiC_SignEditorWindow.ID, true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.CreateWorkingCopy();
		this.UpdateAspect();
		this.ResetView();
		this.hasUnsavedChanges = false;
		this.showNewLayerPanel = false;
		this.txtSignName.Text = this.workingSignData.name;
		int count = this.workingSignData.layers.Count;
		if (count > 0)
		{
			this.signLayerGrid.MultiSelectedLayerIndices.Add(0);
		}
		this.signLayerGrid.UpdateLayers(this.workingSignId, 0, count == 0, true);
		this.Refresh();
		this.RefreshComplexityInfo();
		this.historyStateManager.Init("Editor Opened", this.signLayerGrid, this.txtSignName, this.workingSignId, this.workingSignData, this);
		this.xui.playerUI.windowManager.IsInputLocked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.IsInputLocked = false;
		this.historyStateManager.Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnBeforeApplyingChange(string changeDescription, bool forceDirty = false)
	{
		this.historyStateManager.ProcessPendingChange(changeDescription, forceDirty);
	}

	public void RequestAffectMode(XUiC_SignEditorWindow.AffectMode requestedMode)
	{
		if (!(this.selectedLayer is SignData.GroupSignLayer))
		{
			this.SetAffectMode(XUiC_SignEditorWindow.AffectMode.None);
			return;
		}
		if (this.currentAffectMode == requestedMode)
		{
			this.SetAffectMode(XUiC_SignEditorWindow.AffectMode.None);
			return;
		}
		this.SetAffectMode(requestedMode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAffectMode(XUiC_SignEditorWindow.AffectMode newMode)
	{
		if (this.currentAffectMode == newMode)
		{
			return;
		}
		this.currentAffectMode = newMode;
		this.historyStateManager.ForceDirty();
		Action<XUiC_SignEditorWindow.AffectMode> onAffectModeChanged = this.OnAffectModeChanged;
		if (onAffectModeChanged == null)
		{
			return;
		}
		onAffectModeChanged(this.currentAffectMode);
	}

	public void RestoreAffectModeFromHistory(XUiC_SignEditorWindow.AffectMode mode)
	{
		if (!(this.selectedLayer is SignData.GroupSignLayer))
		{
			mode = XUiC_SignEditorWindow.AffectMode.None;
		}
		if (this.currentAffectMode != mode)
		{
			this.currentAffectMode = mode;
			Action<XUiC_SignEditorWindow.AffectMode> onAffectModeChanged = this.OnAffectModeChanged;
			if (onAffectModeChanged == null)
			{
				return;
			}
			onAffectModeChanged(this.currentAffectMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 WorldDeltaToGroupLocalDelta(SignData.GroupSignLayer group, Vector2 worldDelta)
	{
		float num = -group.transform.rotation * 0.017453292f;
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		Vector2 vector;
		vector..ctor(worldDelta.x * num2 - worldDelta.y * num3, worldDelta.x * num3 + worldDelta.y * num2);
		Vector2 scale = group.transform.scale;
		float num4 = (Mathf.Approximately(scale.x, 0f) ? 0f : (1f / scale.x));
		float num5 = (Mathf.Approximately(scale.y, 0f) ? 0f : (1f / scale.y));
		return new Vector2(vector.x * num4, vector.y * num5);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RotateGroupChildrenAroundOrigin(SignData.GroupSignLayer group, float deltaDegrees)
	{
		float num = deltaDegrees * 0.017453292f;
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		for (int i = 0; i < group.layers.Count; i++)
		{
			SignData.SignLayer signLayer = group.layers[i];
			Vector2 position = signLayer.transform.position;
			signLayer.transform.position = new Vector2(position.x * num2 - position.y * num3, position.x * num3 + position.y * num2);
			signLayer.transform.rotation = signLayer.transform.rotation + deltaDegrees;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ScaleGroupChildrenAroundOrigin(SignData.GroupSignLayer group, Vector2 factor)
	{
		for (int i = 0; i < group.layers.Count; i++)
		{
			SignData.SignLayer signLayer = group.layers[i];
			signLayer.transform.position = new Vector2(signLayer.transform.position.x * factor.x, signLayer.transform.position.y * factor.y);
			signLayer.transform.scale = new Vector2(signLayer.transform.scale.x * factor.x, signLayer.transform.scale.y * factor.y);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string DecorateChangeDescription(string baseDescription)
	{
		XUiC_SignEditorWindow.AffectMode affectMode = this.currentAffectMode;
		if (affectMode == XUiC_SignEditorWindow.AffectMode.Children)
		{
			return baseDescription + " [Affect Children]";
		}
		if (affectMode != XUiC_SignEditorWindow.AffectMode.Pivot)
		{
			return baseDescription;
		}
		return baseDescription + " [Affect Pivot]";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetAffectTargetGroup(out SignData.GroupSignLayer group)
	{
		group = null;
		if (this.currentAffectMode == XUiC_SignEditorWindow.AffectMode.None)
		{
			return false;
		}
		SignData.GroupSignLayer groupSignLayer = this.selectedLayer as SignData.GroupSignLayer;
		if (groupSignLayer == null || groupSignLayer.layers == null || groupSignLayer.layers.Count == 0)
		{
			return false;
		}
		group = groupSignLayer;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAspect()
	{
		if (this.targetCanvas != null && this.aspectMaskEnabled)
		{
			this.canvasAspect = ((this.targetCanvas.CanvasAspect > 1f) ? new Vector2(1f, 1f / this.targetCanvas.CanvasAspect) : new Vector2(this.targetCanvas.CanvasAspect, 1f));
			if (this.targetCanvas != null && (this.targetCanvas.CanvasRotation == CanvasRotationMode.Rot90 || this.targetCanvas.CanvasRotation == CanvasRotationMode.Rot270))
			{
				this.canvasAspect = new Vector2(this.canvasAspect.y, this.canvasAspect.x);
			}
		}
		else
		{
			this.canvasAspect = Vector2.one;
		}
		this.OnFrameAspectChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAspectText_OnChangeHandler(XUiController sender, string text, bool changeFromCode)
	{
		if (changeFromCode)
		{
			return;
		}
		float num;
		float num2;
		if (!float.TryParse(this.txtAspectHorizontal.Text, out num) || !float.TryParse(this.txtAspectVertical.Text, out num2))
		{
			return;
		}
		if (num <= 0f || num2 <= 0f)
		{
			return;
		}
		float num3 = num / num2;
		this.canvasAspect = ((num3 > 1f) ? new Vector2(1f, 1f / num3) : new Vector2(num3, 1f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnAspectFit_OnPressed(XUiController sender, int mouseButton)
	{
		this.canvasAspect = ((this.targetCanvas.CanvasAspect > 1f) ? new Vector2(1f, 1f / this.targetCanvas.CanvasAspect) : new Vector2(this.targetCanvas.CanvasAspect, 1f));
		if (this.targetCanvas != null && (this.targetCanvas.CanvasRotation == CanvasRotationMode.Rot90 || this.targetCanvas.CanvasRotation == CanvasRotationMode.Rot270))
		{
			this.canvasAspect = new Vector2(this.canvasAspect.y, this.canvasAspect.x);
		}
		this.OnFrameAspectChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnAspectSquare_OnPressed(XUiController sender, int mouseButton)
	{
		this.canvasAspect = Vector2.one;
		this.OnFrameAspectChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFrameAspectChanged()
	{
		this.txtAspectHorizontal.Text = this.canvasAspect.x.ToString();
		this.txtAspectVertical.Text = this.canvasAspect.y.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateWorkingCopy()
	{
		this.workingSignData = SignData.Duplicate(this.sourceSignData, "");
		this.workingSignId = SignDataManager.Instance.AddSignToLibrary("[TEMP]", this.workingSignData);
		SignDataManager.Instance.TryGetSignComplexityInfo(this.workingSignId, out this.signComplexityInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkChanged()
	{
		this.hasUnsavedChanges = true;
		SignDataManager.Instance.MarkSignDirty(this.workingSignId);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshComplexityInfo()
	{
		float num = 0f;
		if (this.signComplexityInfo.IsValid)
		{
			this.sizeBar.SetUsed(this.signComplexityInfo.TotalComplexity);
			foreach (int num2 in this.signLayerGrid.MultiSelectedLayerIndices)
			{
				SignData.SignLayer signLayer = this.workingSignData.layers[num2];
				LayerComplexityInfo layerComplexityInfo;
				if (this.signComplexityInfo.TryGetLayerComplexityInfo(signLayer, out layerComplexityInfo))
				{
					if (signLayer == this.selectedLayer)
					{
						this.debugPanel.Populate(in layerComplexityInfo);
					}
					num += layerComplexityInfo.Complexity;
				}
			}
			this.sizeBar.SetSelectedRegion(new XUiC_SizeBar.BarRegionFloat(0f, num));
			this.complexityHoverOffset = num;
			return;
		}
		this.sizeBar.SetUsed(0f);
		this.sizeBar.SetSelectedRegion(new XUiC_SizeBar.BarRegionFloat(0f, 0f));
		this.complexityHoverOffset = 0f;
		this.debugPanel.Clear();
		Log.Error(string.Format("Failed to retrieve valid complexity info for sign: {0}", this.workingSignId));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLayerSelected(SignData.SignLayer layer)
	{
		if (layer != null)
		{
			this.<SetLayerSelected>g__ToggleNewLayerPopup|94_0(false);
			if (this.signLayerGrid.HasPlaceholder)
			{
				int num = this.workingSignData.layers.IndexOf(layer);
				this.signLayerGrid.MultiSelectedLayerIndices.Clear();
				this.signLayerGrid.MultiSelectedLayerIndices.Add(num);
				this.signLayerGrid.UpdateLayers(this.workingSignId, num, false, true);
				return;
			}
		}
		else
		{
			this.<SetLayerSelected>g__ToggleNewLayerPopup|94_0(true);
		}
		if (this.selectedLayer == layer)
		{
			return;
		}
		this.selectedLayer = layer;
		if (this.currentAffectMode != XUiC_SignEditorWindow.AffectMode.None)
		{
			this.SetAffectMode(XUiC_SignEditorWindow.AffectMode.None);
		}
		if (layer != null)
		{
			this.highlightStartTime = Time.time;
			this.pulseOverlayMaterial.Texture = Texture2D.whiteTexture;
		}
		else
		{
			this.pulseOverlayMaterial.Texture = null;
		}
		XUiC_CategoryList xuiC_CategoryList = this.categoryList;
		string text;
		if (xuiC_CategoryList == null)
		{
			text = null;
		}
		else
		{
			XUiC_CategoryEntry currentCategory = xuiC_CategoryList.CurrentCategory;
			text = ((currentCategory != null) ? currentCategory.CategoryName : null);
		}
		this.RefreshLayerSettings(text ?? string.Empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshLayerSettings(string autoSelectCategory)
	{
		XUiC_SignEditorWindow.<>c__DisplayClass95_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		int num = this.signLayerGrid.SelectedLayerIndex + 1;
		CS$<>8__locals1.i = 0;
		XUiC_TextInput xuiC_TextInput = this.txtLayerName;
		SignData.SignLayer signLayer = this.selectedLayer;
		xuiC_TextInput.Text = ((signLayer != null) ? signLayer.name : null);
		this.RefreshComplexityInfo();
		SignData.SignLayer signLayer2 = this.selectedLayer;
		int num2;
		if (!(signLayer2 is SignData.GroupSignLayer))
		{
			if (!(signLayer2 is SignData.TextSignLayer))
			{
				if (!(signLayer2 is SignData.PolygonSignLayer))
				{
					if (!(signLayer2 is SignData.NoiseSignLayer))
					{
						if (signLayer2 != null)
						{
							Log.Error("Unsupported layer type selected.");
						}
						else
						{
							this.selectedLayerName = string.Empty;
						}
					}
					else
					{
						this.selectedLayerName = string.Format(Localization.Get("lblSignLayerName", false, null), num.ToString("00")) + " [" + Localization.Get("lblSignNoise", false, null) + "]";
						XUiC_CategoryList xuiC_CategoryList = this.categoryList;
						num2 = CS$<>8__locals1.i;
						CS$<>8__locals1.i = num2 + 1;
						xuiC_CategoryList.SetCategoryEntry(num2, "Noise", "ui_game_symbol_chemistry", Localization.Get("lblSignNoise", false, null));
						XUiC_CategoryList xuiC_CategoryList2 = this.categoryList;
						num2 = CS$<>8__locals1.i;
						CS$<>8__locals1.i = num2 + 1;
						xuiC_CategoryList2.SetCategoryEntry(num2, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor", false, null));
						XUiC_CategoryList xuiC_CategoryList3 = this.categoryList;
						num2 = CS$<>8__locals1.i;
						CS$<>8__locals1.i = num2 + 1;
						xuiC_CategoryList3.SetCategoryEntry(num2, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform", false, null));
						this.<RefreshLayerSettings>g__AddWarpCategories|95_0(ref CS$<>8__locals1);
					}
				}
				else
				{
					this.selectedLayerName = string.Format(Localization.Get("lblSignLayerName", false, null), num.ToString("00")) + " [" + Localization.Get("lblSignPolygon", false, null) + "]";
					XUiC_CategoryList xuiC_CategoryList4 = this.categoryList;
					num2 = CS$<>8__locals1.i;
					CS$<>8__locals1.i = num2 + 1;
					xuiC_CategoryList4.SetCategoryEntry(num2, "Polygon", "ui_game_symbol_all_blocks", Localization.Get("lblSignPolygon", false, null));
					XUiC_CategoryList xuiC_CategoryList5 = this.categoryList;
					num2 = CS$<>8__locals1.i;
					CS$<>8__locals1.i = num2 + 1;
					xuiC_CategoryList5.SetCategoryEntry(num2, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor", false, null));
					XUiC_CategoryList xuiC_CategoryList6 = this.categoryList;
					num2 = CS$<>8__locals1.i;
					CS$<>8__locals1.i = num2 + 1;
					xuiC_CategoryList6.SetCategoryEntry(num2, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform", false, null));
					this.<RefreshLayerSettings>g__AddWarpCategories|95_0(ref CS$<>8__locals1);
				}
			}
			else
			{
				this.selectedLayerName = string.Format(Localization.Get("lblSignLayerName", false, null), num.ToString("00")) + " [" + Localization.Get("lblSignText", false, null) + "]";
				XUiC_CategoryList xuiC_CategoryList7 = this.categoryList;
				num2 = CS$<>8__locals1.i;
				CS$<>8__locals1.i = num2 + 1;
				xuiC_CategoryList7.SetCategoryEntry(num2, "Text", "ui_game_symbol_shape_text", Localization.Get("lblSignText", false, null));
				XUiC_CategoryList xuiC_CategoryList8 = this.categoryList;
				num2 = CS$<>8__locals1.i;
				CS$<>8__locals1.i = num2 + 1;
				xuiC_CategoryList8.SetCategoryEntry(num2, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor", false, null));
				XUiC_CategoryList xuiC_CategoryList9 = this.categoryList;
				num2 = CS$<>8__locals1.i;
				CS$<>8__locals1.i = num2 + 1;
				xuiC_CategoryList9.SetCategoryEntry(num2, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform", false, null));
				this.<RefreshLayerSettings>g__AddWarpCategories|95_0(ref CS$<>8__locals1);
			}
		}
		else
		{
			this.selectedLayerName = string.Format(Localization.Get("lblSignLayerName", false, null), num.ToString("00")) + " [" + Localization.Get("lblSignGroup", false, null) + "]";
			XUiC_CategoryList xuiC_CategoryList10 = this.categoryList;
			num2 = CS$<>8__locals1.i;
			CS$<>8__locals1.i = num2 + 1;
			xuiC_CategoryList10.SetCategoryEntry(num2, "Group", "ui_game_symbol_bundle", Localization.Get("lblSignGroup", false, null));
			XUiC_CategoryList xuiC_CategoryList11 = this.categoryList;
			num2 = CS$<>8__locals1.i;
			CS$<>8__locals1.i = num2 + 1;
			xuiC_CategoryList11.SetCategoryEntry(num2, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor", false, null));
			XUiC_CategoryList xuiC_CategoryList12 = this.categoryList;
			num2 = CS$<>8__locals1.i;
			CS$<>8__locals1.i = num2 + 1;
			xuiC_CategoryList12.SetCategoryEntry(num2, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform", false, null));
			this.<RefreshLayerSettings>g__AddWarpCategories|95_0(ref CS$<>8__locals1);
		}
		using (List<XUiC_SignLayerSettings>.Enumerator enumerator = this.allSettings.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XUiC_SignLayerSettings xuiC_SignLayerSettings = enumerator.Current;
				xuiC_SignLayerSettings.SetLayer(this.selectedLayer);
			}
			goto IL_047C;
		}
		IL_045A:
		this.categoryList.SetCategoryEmpty(CS$<>8__locals1.i);
		num2 = CS$<>8__locals1.i;
		CS$<>8__locals1.i = num2 + 1;
		IL_047C:
		if (CS$<>8__locals1.i >= this.categoryList.CategoryButtons.Count)
		{
			if (string.IsNullOrEmpty(autoSelectCategory) || !this.categoryList.TrySetCategory(autoSelectCategory))
			{
				this.categoryList.SetCategoryToFirst();
			}
			base.RefreshBindings();
			return;
		}
		goto IL_045A;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWarpAdded(string warpName)
	{
		SignData.SignLayer signLayer = this.selectedLayer;
		this.RefreshLayerSettings((((signLayer != null) ? signLayer.warps : null) != null) ? string.Format("{0}_{1}", warpName, this.selectedLayer.warps.Count - 1) : string.Empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWarpRemoved()
	{
		int num = this.categoryList.CategoryButtons.IndexOf(this.categoryList.CurrentCategory);
		XUiC_CategoryEntry categoryByIndex = this.categoryList.GetCategoryByIndex(num + 1);
		this.RefreshLayerSettings((categoryByIndex == null) ? string.Empty : categoryByIndex.CategoryName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMaskModeChanged()
	{
		this.signLayerGrid.RefreshSelectedLayerBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		SignDataManager.Instance.TryApplyRenderingData(this.workingSignId, this.previewAspect, mat, null, SignUIStyle.MainCanvas);
		mat.SetVector(SignShaderIDs._CanvasAspect, this.canvasAspect);
		Vector4 vector = new Vector4(2f, 2f, -1f - this.viewOffset.x, -1f - this.viewOffset.y) * this.viewScale;
		mat.SetVector(SignShaderIDs._AtlasArray_ST, vector);
		if (this.currentAffectMode != XUiC_SignEditorWindow.AffectMode.None)
		{
			SignData.GroupSignLayer groupSignLayer = this.selectedLayer as SignData.GroupSignLayer;
			if (groupSignLayer != null)
			{
				mat.SetVector(SignShaderIDs._Crosshair, new Vector4(groupSignLayer.transform.position.x, groupSignLayer.transform.position.y, 0.017453292f * groupSignLayer.transform.rotation, 1f));
				return;
			}
		}
		mat.SetVector(SignShaderIDs._Crosshair, new Vector4(0f, 0f, 0f, 0f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPulseOverlayWillRender(Material mat)
	{
		if (this.selectedLayer == null)
		{
			return;
		}
		float num = Mathf.Clamp01((Time.time - this.highlightStartTime) / 1f);
		SignDataManager.Instance.TryApplyRenderingData(this.workingSignId, this.previewAspect, mat, this.selectedLayer, SignUIStyle.LayerSelectHighlight);
		mat.SetVector(SignShaderIDs._CanvasAspect, this.canvasAspect);
		mat.SetFloat(SignShaderIDs._HighlightPhase, num);
		Vector4 vector = new Vector4(2f, 2f, -1f - this.viewOffset.x, -1f - this.viewOffset.y) * this.viewScale;
		mat.SetVector(SignShaderIDs._AtlasArray_ST, vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHoverOverlayWillRender(Material mat)
	{
		if (this.hoverOverlayLayer == null)
		{
			return;
		}
		SignDataManager.Instance.TryApplyRenderingData(this.workingSignId, this.previewAspect, mat, this.hoverOverlayLayer, SignUIStyle.LayerHoverHighlight);
		mat.SetVector(SignShaderIDs._CanvasAspect, this.canvasAspect);
		mat.SetFloat(SignShaderIDs._HighlightPhase, this.hoverOverlayOpacity);
		Vector4 vector = new Vector4(2f, 2f, -1f - this.viewOffset.x, -1f - this.viewOffset.y) * this.viewScale;
		mat.SetVector(SignShaderIDs._AtlasArray_ST, vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		int num = _categoryEntry.CategoryName.IndexOf("_");
		int num2;
		if (num > -1 && int.TryParse(_categoryEntry.CategoryName.Substring(num + 1), out num2))
		{
			foreach (XUiC_SignWarpSettings xuiC_SignWarpSettings in this.allWarpSettings)
			{
				xuiC_SignWarpSettings.SetWarp(this.selectedLayer.warps[num2]);
			}
		}
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.Save();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSaveCopy_OnPressed(XUiController sender, int mouseButton)
	{
		this.SaveCopy();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnQuit_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.Quit(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (!this.hasUnsavedChanges || !this.GetSavingAllowed())
		{
			return;
		}
		this.sourceSignData.CloneLayersFrom(this.workingSignData);
		this.sourceSignData.name = this.workingSignData.name;
		if (!SignDataManager.Instance.MarkSignDirty(this.sourceSignId))
		{
			Log.Error(string.Format("Failed to mark source sign data dirty. Source ID: {0}", this.sourceSignId));
		}
		this.hasUnsavedChanges = false;
		this.Refresh();
		if (PrefabEditModeManager.Instance != null)
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveCopy()
	{
		if (!this.GetSavingAllowed())
		{
			return;
		}
		string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
		SignData signData = SignData.Duplicate(this.workingSignData, SignDataManager.Instance.GetDuplicateName(fileNameNoExtension, this.workingSignData.name) ?? "");
		GlobalSignId globalSignId = SignDataManager.Instance.AddSignToLibrary(fileNameNoExtension, signData);
		this.sourceSignId = globalSignId;
		this.sourceSignData = signData;
		this.CreateWorkingCopy();
		this.txtSignName.Text = this.workingSignData.name;
		if (this.targetCanvas != null)
		{
			this.targetCanvas.SignId = this.sourceSignId;
		}
		this.signLayerGrid.UpdateLayers(this.workingSignId, 0, this.workingSignData.layers.Count == 0, true);
		this.historyStateManager.Init("Save As", this.signLayerGrid, this.txtSignName, this.workingSignId, this.workingSignData, this);
		this.hasUnsavedChanges = false;
		this.Refresh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Quit(bool force = false)
	{
		if (this.hasUnsavedChanges && !force)
		{
			this.confirmationPrompt.ShowPrompt(Localization.Get("lblSignDiscardTitle", false, null), Localization.Get("lblSignDiscardMessage", false, null), Localization.Get("xuiCancel", false, null), Localization.Get("xuiConfirm", false, null), new Action<XUiC_ConfirmationPrompt.Result>(this.OnDiscardChangesPromptResult));
			return;
		}
		if (!SignDataManager.Instance.TryDeleteSign(this.workingSignId))
		{
			Log.Error(string.Format("Failed to clean up temporary editor sign data for sign ID: {0}", this.workingSignId));
		}
		XUiC_SignGalleryWindow.Open(this.xui.playerUI, this.targetEntity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDiscardChangesPromptResult(XUiC_ConfirmationPrompt.Result result)
	{
		if (result != XUiC_ConfirmationPrompt.Result.Cancelled && result == XUiC_ConfirmationPrompt.Result.Confirmed)
		{
			this.Quit(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSignName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_changeFromCode)
		{
			return;
		}
		this.OnBeforeApplyingChange("Changed Sign Name", false);
		this.workingSignData.name = _text;
		this.MarkChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCanvasScrolled(XUiController _sender, float _delta)
	{
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count == 0 || this.signLayerGrid.HasPlaceholder)
		{
			return;
		}
		bool mouseButton = Input.GetMouseButton(1);
		float num = (mouseButton ? 0.995f : 0.95f);
		float num2 = ((_delta < 0f) ? num : (1f / num));
		if (InputUtils.ControlKeyPressed)
		{
			this.ScaleSelection(new Vector2(num2, num2));
		}
		else if (InputUtils.ShiftKeyPressed)
		{
			this.ScaleSelection(new Vector2(1f, num2));
		}
		else if (InputUtils.AltKeyPressed)
		{
			this.ScaleSelection(new Vector2(num2, 1f));
		}
		else
		{
			float num3 = (mouseButton ? 0.1f : 1f);
			this.RotateSelection(_delta * 50f * num3);
		}
		this.transformSettings.SetLayer(this.selectedLayer);
		this.MarkChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCanvasDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		switch (UICamera.currentKey)
		{
		case 323:
			this.HandleLeftMouseDragged(_dragType, _mousePositionDelta);
			return;
		case 324:
			break;
		case 325:
			this.HandleMiddleMouseDragged(_dragType, _mousePositionDelta);
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetView()
	{
		this.viewOffset = Vector2.zero;
		this.viewScale = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMiddleMouseDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (_dragType == EDragType.DragStart)
		{
			this.<HandleMiddleMouseDragged>g__BeginDrag|121_1();
		}
		else
		{
			if (_dragType == EDragType.DragEnd)
			{
				this.<HandleMiddleMouseDragged>g__EndDrag|121_2();
				return;
			}
			if (this.dragStartViewScaleMode != InputUtils.ControlKeyPressed)
			{
				this.<HandleMiddleMouseDragged>g__EndDrag|121_2();
				this.<HandleMiddleMouseDragged>g__BeginDrag|121_1();
			}
		}
		Vector2 vector = this.xui.GetPixelRatioFactor() * (2f * _mousePositionDelta / (float)this.signMaterial.Size.y);
		this.middleMouseDrag.RawPosition = this.middleMouseDrag.RawPosition + vector;
		this.<HandleMiddleMouseDragged>g__UpdateViewOffset|121_0();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLeftMouseDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count == 0 || this.signLayerGrid.HasPlaceholder)
		{
			this.leftMouseDrag.IsDragging = false;
			return;
		}
		Vector2 vector = this.xui.GetPixelRatioFactor() * (2f * _mousePositionDelta * this.viewScale / (float)this.signMaterial.Size.y);
		SignData.GroupSignLayer groupSignLayer;
		bool flag = this.TryGetAffectTargetGroup(out groupSignLayer);
		if (_dragType == EDragType.DragStart)
		{
			this.layerDragStartPositions.Clear();
			foreach (int num in this.signLayerGrid.MultiSelectedLayerIndices)
			{
				this.layerDragStartPositions.Add(this.workingSignData.layers[num].transform.position);
			}
			this.leftMouseDrag.StartPosition = this.selectedLayer.transform.position;
			this.leftMouseDrag.RawPosition = this.leftMouseDrag.StartPosition;
			this.leftMouseDrag.IsDragging = true;
			this.affectDragChildStartPositions.Clear();
			if (flag)
			{
				this.affectDragGroupStartPosition = groupSignLayer.transform.position;
				for (int i = 0; i < groupSignLayer.layers.Count; i++)
				{
					this.affectDragChildStartPositions.Add(groupSignLayer.layers[i].transform.position);
				}
			}
		}
		else if (_dragType == EDragType.DragEnd)
		{
			this.leftMouseDrag.IsDragging = false;
			return;
		}
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count != this.layerDragStartPositions.Count)
		{
			Log.Error(string.Format("Data error in HandleCanvasDragged: Selected ({0}) and tracked ({1}) layer counts are not equal.", this.signLayerGrid.MultiSelectedLayerIndices.Count, this.layerDragStartPositions.Count));
			return;
		}
		this.leftMouseDrag.RawPosition = this.leftMouseDrag.RawPosition + vector;
		Vector2 vector2 = this.leftMouseDrag.RawPosition - this.leftMouseDrag.StartPosition;
		if (InputUtils.ShiftKeyPressed)
		{
			if (Mathf.Abs(vector2.x) > Mathf.Abs(vector2.y))
			{
				vector2.y = 0f;
			}
			else
			{
				vector2.x = 0f;
			}
		}
		this.OnBeforeApplyingChange(this.DecorateChangeDescription("Changed Transform Position"), false);
		if (flag && this.affectDragChildStartPositions.Count == groupSignLayer.layers.Count)
		{
			Vector2 vector3 = XUiC_SignEditorWindow.WorldDeltaToGroupLocalDelta(groupSignLayer, vector2);
			if (this.currentAffectMode == XUiC_SignEditorWindow.AffectMode.Children)
			{
				for (int j = 0; j < groupSignLayer.layers.Count; j++)
				{
					groupSignLayer.layers[j].transform.position = this.affectDragChildStartPositions[j] + vector3;
				}
			}
			else if (this.currentAffectMode == XUiC_SignEditorWindow.AffectMode.Pivot)
			{
				groupSignLayer.transform.position = this.affectDragGroupStartPosition + vector2;
				for (int k = 0; k < groupSignLayer.layers.Count; k++)
				{
					groupSignLayer.layers[k].transform.position = this.affectDragChildStartPositions[k] - vector3;
				}
			}
		}
		else
		{
			for (int l = 0; l < this.signLayerGrid.MultiSelectedLayerIndices.Count; l++)
			{
				int num2 = this.signLayerGrid.MultiSelectedLayerIndices[l];
				this.workingSignData.layers[num2].transform.position = this.layerDragStartPositions[l] + vector2;
			}
		}
		this.transformSettings.SetLayer(this.selectedLayer);
		this.MarkChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDragAndDropStarted(int dragStartIdx)
	{
		this.dragAndDropIcon.SetLayer(this.workingSignId, this.workingSignData.layers[this.signLayerGrid.draggedLayerIndices[0]]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDragAndDropReleased(int insertIndex)
	{
		if (insertIndex < 0)
		{
			this.dragAndDropIcon.SetLayer(this.workingSignId, null);
			return;
		}
		this.OnBeforeApplyingChange("Drag and dropped layer", false);
		this.originalLayerIndices.Clear();
		this.originalLayerIndices.AddRange(this.signLayerGrid.draggedLayerIndices);
		int count = this.originalLayerIndices.Count;
		this.originalLayers.Clear();
		for (int i = 0; i < count; i++)
		{
			this.originalLayers.Add(this.workingSignData.layers[this.originalLayerIndices[i]].Clone());
		}
		for (int j = count - 1; j >= 0; j--)
		{
			this.workingSignData.layers.RemoveAt(this.originalLayerIndices[j]);
		}
		int num = insertIndex;
		for (int k = 0; k < count; k++)
		{
			if (this.originalLayerIndices[k] < insertIndex)
			{
				num--;
			}
		}
		for (int l = 0; l < count; l++)
		{
			this.workingSignData.layers.Insert(num + l, this.originalLayers[l]);
		}
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		for (int m = 0; m < count; m++)
		{
			this.signLayerGrid.MultiSelectedLayerIndices.Add(num + m);
		}
		this.MarkChanged();
		this.signLayerGrid.UpdateLayers(this.workingSignId, num, false, true);
		this.dragAndDropIcon.SetLayer(this.workingSignId, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLayerSelected(int selectedLayerIndex)
	{
		int num = this.signLayerGrid.GridIndexToLayerIndex(selectedLayerIndex);
		if (num < 0 || num >= this.workingSignData.layers.Count)
		{
			this.SetLayerSelected(null);
			return;
		}
		SignData.SignLayer signLayer = this.workingSignData.layers[num];
		this.SetLayerSelected(signLayer);
		if (signLayer == this.hoveredLayer)
		{
			this.HandleLayerHovered(selectedLayerIndex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLayerHovered(int hoveredDisplayIndex)
	{
		int num = this.signLayerGrid.GridIndexToLayerIndex(hoveredDisplayIndex);
		this.hoveredLayer = ((num >= 0 && num < this.workingSignData.layers.Count) ? this.workingSignData.layers[num] : null);
		if (this.hoveredLayer != null)
		{
			this.hoverOverlayLayer = this.hoveredLayer;
		}
		if (num < 0 || num >= this.workingSignData.layers.Count)
		{
			this.sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(0f, 0f));
			return;
		}
		SignData.SignLayer signLayer = this.workingSignData.layers[num];
		LayerComplexityInfo layerComplexityInfo;
		if (!this.signComplexityInfo.TryGetLayerComplexityInfo(signLayer, out layerComplexityInfo))
		{
			this.sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(0f, 0f));
			return;
		}
		if (this.signLayerGrid.MultiSelectedLayerIndices.Contains(hoveredDisplayIndex))
		{
			this.sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(this.complexityHoverOffset - layerComplexityInfo.Complexity, layerComplexityInfo.Complexity));
			return;
		}
		this.sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(this.complexityHoverOffset, layerComplexityInfo.Complexity));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BeginAddLayerFlow(int index)
	{
		this.addLayerIndex = index;
		this.signLayerGrid.UpdateLayers(this.workingSignId, this.addLayerIndex, true, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLayerTypeSelected(SignData.SignLayer layer)
	{
		if (layer == null)
		{
			return;
		}
		int num = this.addLayerIndex;
		this.OnBeforeApplyingChange("Added Layer", true);
		this.workingSignData.SetLayerDefaultName(layer);
		int num2 = ((this.workingSignData.layers.Count == 0) ? 0 : ((num < 0) ? (this.workingSignData.layers.Count - 1) : num));
		this.workingSignData.layers.Insert(num2, layer);
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		this.signLayerGrid.MultiSelectedLayerIndices.Add(num2);
		this.MarkChanged();
		this.signLayerGrid.UpdateLayers(this.workingSignId, num2, false, true);
		this.addLayerIndex = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeleteSelectedLayers(string changeDescription = "Deleted Layer(s)")
	{
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count == 0 || this.signLayerGrid.HasPlaceholder)
		{
			return;
		}
		this.OnBeforeApplyingChange(changeDescription, true);
		this.signLayerGrid.MultiSelectedLayerIndices.Sort();
		int num = this.signLayerGrid.MultiSelectedLayerIndices[0];
		for (int i = this.signLayerGrid.MultiSelectedLayerIndices.Count - 1; i >= 0; i--)
		{
			this.workingSignData.layers.RemoveAt(this.signLayerGrid.MultiSelectedLayerIndices[i]);
		}
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		num = Mathf.Min(num, this.workingSignData.layers.Count - 1);
		if (num >= 0)
		{
			this.signLayerGrid.MultiSelectedLayerIndices.Add(num);
			this.signLayerGrid.UpdateLayers(this.workingSignId, num, false, true);
		}
		else
		{
			this.BeginAddLayerFlow(0);
		}
		this.MarkChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectAllLayers()
	{
		if (this.workingSignData.layers.Count == 0)
		{
			return;
		}
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		for (int i = 0; i < this.workingSignData.layers.Count; i++)
		{
			this.signLayerGrid.MultiSelectedLayerIndices.Add(i);
		}
		this.signLayerGrid.UpdateLayers(this.workingSignId, 0, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DuplicateSelectedLayers()
	{
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count == 0 || this.signLayerGrid.HasPlaceholder)
		{
			return;
		}
		this.OnBeforeApplyingChange("Duplicated Layer(s)", true);
		this.signLayerGrid.MultiSelectedLayerIndices.Sort();
		int count = this.signLayerGrid.MultiSelectedLayerIndices.Count;
		int num = this.signLayerGrid.MultiSelectedLayerIndices[count - 1] + 1;
		for (int i = count - 1; i >= 0; i--)
		{
			this.workingSignData.layers.Insert(num, this.workingSignData.layers[this.signLayerGrid.MultiSelectedLayerIndices[i]].Clone());
		}
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		for (int j = 0; j < count; j++)
		{
			this.signLayerGrid.MultiSelectedLayerIndices.Add(num + j);
		}
		this.MarkChanged();
		this.signLayerGrid.UpdateLayers(this.workingSignId, num + count - 1, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopySelectedLayersToClipboard()
	{
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count == 0 || this.signLayerGrid.HasPlaceholder)
		{
			return;
		}
		XUiC_SignEditorWindow.layerClipboard.Clear();
		this.signLayerGrid.MultiSelectedLayerIndices.Sort();
		foreach (int num in this.signLayerGrid.MultiSelectedLayerIndices)
		{
			XUiC_SignEditorWindow.layerClipboard.Add(this.workingSignData.layers[num].Clone());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PasteLayersFromClipboard()
	{
		if (XUiC_SignEditorWindow.layerClipboard.Count == 0)
		{
			return;
		}
		this.OnBeforeApplyingChange("Pasted Layer(s)", true);
		int num = Mathf.Min(this.signLayerGrid.SelectedLayerIndex, this.workingSignData.layers.Count - 1);
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		foreach (SignData.SignLayer signLayer in XUiC_SignEditorWindow.layerClipboard)
		{
			num++;
			this.workingSignData.layers.Insert(num, signLayer.Clone());
			this.signLayerGrid.MultiSelectedLayerIndices.Add(num);
		}
		this.MarkChanged();
		this.signLayerGrid.UpdateLayers(this.workingSignId, num, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UngroupSelectedLayers()
	{
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count < 1)
		{
			return;
		}
		this.OnBeforeApplyingChange("Ungrouped Layer(s)", true);
		this.signLayerGrid.MultiSelectedLayerIndices.Sort();
		int num = this.signLayerGrid.MultiSelectedLayerIndices[0];
		this.indicesToSelect.Clear();
		for (int i = this.signLayerGrid.MultiSelectedLayerIndices.Count - 1; i >= 0; i--)
		{
			int num2 = this.signLayerGrid.MultiSelectedLayerIndices[i];
			int num3;
			if (this.workingSignData.TryUnpackTopLevelGroup(num2, false, out num3))
			{
				for (int j = 0; j < this.indicesToSelect.Count; j++)
				{
					List<int> list = this.indicesToSelect;
					int num4 = j;
					list[num4] += num3 - 1;
				}
				for (int k = 0; k < num3; k++)
				{
					this.indicesToSelect.Add(num2 + k);
				}
			}
		}
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		this.signLayerGrid.MultiSelectedLayerIndices.AddRange(this.indicesToSelect);
		this.MarkChanged();
		this.signLayerGrid.UpdateLayers(this.workingSignId, num, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupSelectedLayers()
	{
		if (this.signLayerGrid.MultiSelectedLayerIndices.Count < 1)
		{
			return;
		}
		this.OnBeforeApplyingChange("Grouped Layer(s)", true);
		this.signLayerGrid.MultiSelectedLayerIndices.Sort();
		int num = this.signLayerGrid.MultiSelectedLayerIndices[0];
		Vector2 vector = this.workingSignData.layers[num].transform.position;
		Vector2 vector2 = this.workingSignData.layers[num].transform.position;
		for (int i = 1; i < this.signLayerGrid.MultiSelectedLayerIndices.Count; i++)
		{
			int num2 = this.signLayerGrid.MultiSelectedLayerIndices[i];
			Vector2 position = this.workingSignData.layers[num2].transform.position;
			vector = Vector2.Min(vector, position);
			vector2 = Vector2.Max(vector2, position);
		}
		Vector2 vector3 = 0.5f * (vector + vector2);
		SignData.GroupSignLayer groupSignLayer = new SignData.GroupSignLayer("Group", vector3, 0f, Vector2.one, new SignData.SignRenderSettings(Color.white, SignData.SignRenderSettings.Mode.ColorOnly), new List<SignData.SignWarp>(), new List<SignData.SignLayer>(this.signLayerGrid.MultiSelectedLayerIndices.Count), SignData.GroupSignLayer.OffsetTarget.All, 0f, 0f, SignData.GroupSignLayer.ColorMode.Multiply);
		this.workingSignData.SetLayerDefaultName(groupSignLayer);
		for (int j = this.signLayerGrid.MultiSelectedLayerIndices.Count - 1; j >= 0; j--)
		{
			int num3 = this.signLayerGrid.MultiSelectedLayerIndices[j];
			SignData.SignLayer signLayer = this.workingSignData.layers[num3];
			this.workingSignData.layers.RemoveAt(num3);
			groupSignLayer.layers.Insert(0, signLayer);
			SignData.SignLayer signLayer2 = signLayer;
			signLayer2.transform.position = signLayer2.transform.position - vector3;
		}
		this.workingSignData.layers.Insert(num, groupSignLayer);
		this.signLayerGrid.MultiSelectedLayerIndices.Clear();
		this.signLayerGrid.MultiSelectedLayerIndices.Add(num);
		this.MarkChanged();
		this.signLayerGrid.UpdateLayers(this.workingSignId, num, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TranslateSelection(Vector2 delta)
	{
		this.OnBeforeApplyingChange(this.DecorateChangeDescription("Changed Transform Position"), false);
		SignData.GroupSignLayer groupSignLayer;
		if (!this.TryGetAffectTargetGroup(out groupSignLayer))
		{
			foreach (int num in this.signLayerGrid.MultiSelectedLayerIndices)
			{
				SignData.SignLayer signLayer = this.workingSignData.layers[num];
				signLayer.transform.position = signLayer.transform.position + delta;
			}
			return;
		}
		Vector2 vector = XUiC_SignEditorWindow.WorldDeltaToGroupLocalDelta(groupSignLayer, delta);
		if (this.currentAffectMode == XUiC_SignEditorWindow.AffectMode.Children)
		{
			for (int i = 0; i < groupSignLayer.layers.Count; i++)
			{
				SignData.SignLayer signLayer2 = groupSignLayer.layers[i];
				signLayer2.transform.position = signLayer2.transform.position + vector;
			}
			return;
		}
		SignData.GroupSignLayer groupSignLayer2 = groupSignLayer;
		groupSignLayer2.transform.position = groupSignLayer2.transform.position + delta;
		for (int j = 0; j < groupSignLayer.layers.Count; j++)
		{
			SignData.SignLayer signLayer3 = groupSignLayer.layers[j];
			signLayer3.transform.position = signLayer3.transform.position - vector;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateSelection(float deltaDegrees)
	{
		this.OnBeforeApplyingChange(this.DecorateChangeDescription("Changed Transform Rotation"), false);
		SignData.GroupSignLayer groupSignLayer;
		if (!this.TryGetAffectTargetGroup(out groupSignLayer))
		{
			foreach (int num in this.signLayerGrid.MultiSelectedLayerIndices)
			{
				SignData.SignLayer signLayer = this.workingSignData.layers[num];
				this.OnBeforeApplyingChange("Changed Transform Rotation", false);
				signLayer.transform.rotation = signLayer.transform.rotation + deltaDegrees;
			}
			return;
		}
		if (this.currentAffectMode == XUiC_SignEditorWindow.AffectMode.Children)
		{
			XUiC_SignEditorWindow.RotateGroupChildrenAroundOrigin(groupSignLayer, deltaDegrees);
			return;
		}
		SignData.GroupSignLayer groupSignLayer2 = groupSignLayer;
		groupSignLayer2.transform.rotation = groupSignLayer2.transform.rotation + deltaDegrees;
		XUiC_SignEditorWindow.RotateGroupChildrenAroundOrigin(groupSignLayer, -deltaDegrees);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ScaleSelection(Vector2 scaleFactor)
	{
		this.OnBeforeApplyingChange(this.DecorateChangeDescription("Changed Transform Scale"), false);
		SignData.GroupSignLayer groupSignLayer;
		if (!this.TryGetAffectTargetGroup(out groupSignLayer))
		{
			foreach (int num in this.signLayerGrid.MultiSelectedLayerIndices)
			{
				SignData.SignLayer signLayer = this.workingSignData.layers[num];
				signLayer.transform.scale = signLayer.transform.scale * scaleFactor;
			}
			return;
		}
		if (this.currentAffectMode == XUiC_SignEditorWindow.AffectMode.Children)
		{
			XUiC_SignEditorWindow.ScaleGroupChildrenAroundOrigin(groupSignLayer, scaleFactor);
			return;
		}
		groupSignLayer.transform.scale = new Vector2(groupSignLayer.transform.scale.x * scaleFactor.x, groupSignLayer.transform.scale.y * scaleFactor.y);
		Vector2 vector;
		vector..ctor(Mathf.Approximately(scaleFactor.x, 0f) ? 1f : (1f / scaleFactor.x), Mathf.Approximately(scaleFactor.y, 0f) ? 1f : (1f / scaleFactor.y));
		XUiC_SignEditorWindow.ScaleGroupChildrenAroundOrigin(groupSignLayer, vector);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.UpdateInputs();
		if (Time.time - this.highlightStartTime >= 1f && this.pulseOverlayMaterial.Texture != null)
		{
			this.pulseOverlayMaterial.Texture = null;
		}
		float num = ((this.hoveredLayer != null) ? 1f : 0f);
		float num2 = 4f;
		this.hoverOverlayOpacity = Mathf.MoveTowards(this.hoverOverlayOpacity, num, num2 * _dt);
		if (this.hoverOverlayOpacity > 0f && this.hoverOverlayMaterial.Texture == null)
		{
			this.hoverOverlayMaterial.Texture = Texture2D.whiteTexture;
		}
		else if (this.hoverOverlayOpacity <= 0f && this.hoverOverlayMaterial.Texture != null)
		{
			this.hoverOverlayMaterial.Texture = null;
			this.hoverOverlayLayer = null;
		}
		if (this.viewComponent.IsVisible && this.IsDirty)
		{
			this.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInputs()
	{
		if (this.xui.playerUI.windowManager.IsInputActive())
		{
			return;
		}
		if (this.xui.playerUI.CursorController.GetMouseButtonDown(0) | this.xui.playerUI.CursorController.GetMouseButtonUp(0))
		{
			this.historyStateManager.ForceDirty();
		}
		if (this.xui.playerUI.playerInput != null && this.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			if (!this.confirmationPrompt.IsVisible)
			{
				this.Quit(false);
				return;
			}
			this.confirmationPrompt.Cancel();
		}
		if (this.confirmationPrompt.IsVisible)
		{
			return;
		}
		if (this.leftMouseDrag.IsDragging && (Input.GetKeyDown(304) || Input.GetKeyDown(303) || Input.GetKeyUp(304) || Input.GetKeyUp(303)))
		{
			this.HandleLeftMouseDragged(EDragType.Dragging, Vector2.zero);
		}
		if (InputUtils.ControlKeyPressed)
		{
			DateTime now = DateTime.Now;
			if (Input.GetKeyDown(122))
			{
				this.heldInputRetriggerTime = now + XUiC_SignEditorWindow.firstRetriggerDelay;
				if (this.historyStateManager.TryUndo())
				{
					this.MarkChanged();
					return;
				}
			}
			else if (Input.GetKey(122) && now > this.heldInputRetriggerTime)
			{
				this.heldInputRetriggerTime = now + XUiC_SignEditorWindow.subsequentRetriggerDelay;
				if (this.historyStateManager.TryUndo())
				{
					this.MarkChanged();
					return;
				}
			}
			else if (Input.GetKeyDown(121))
			{
				this.heldInputRetriggerTime = now + XUiC_SignEditorWindow.firstRetriggerDelay;
				if (this.historyStateManager.TryRedo())
				{
					this.MarkChanged();
					return;
				}
			}
			else if (Input.GetKey(121) && now > this.heldInputRetriggerTime)
			{
				this.heldInputRetriggerTime = now + XUiC_SignEditorWindow.subsequentRetriggerDelay;
				if (this.historyStateManager.TryRedo())
				{
					this.MarkChanged();
					return;
				}
			}
			else if (Input.GetKeyDown(115))
			{
				if (InputUtils.ShiftKeyPressed)
				{
					this.SaveCopy();
					return;
				}
				this.Save();
				return;
			}
			else
			{
				if (Input.GetKeyDown(97))
				{
					this.SelectAllLayers();
					return;
				}
				if (Input.GetKeyDown(100))
				{
					this.DuplicateSelectedLayers();
					return;
				}
				if (Input.GetKeyDown(120))
				{
					this.CopySelectedLayersToClipboard();
					this.DeleteSelectedLayers("Cut Layer(s)");
					return;
				}
				if (Input.GetKeyDown(99))
				{
					this.CopySelectedLayersToClipboard();
					return;
				}
				if (Input.GetKeyDown(118))
				{
					this.PasteLayersFromClipboard();
					return;
				}
				if (Input.GetKeyDown(103))
				{
					if (InputUtils.ShiftKeyPressed)
					{
						this.UngroupSelectedLayers();
						return;
					}
					this.GroupSelectedLayers();
					return;
				}
				else if (Input.GetKeyDown(110) && this.signLayerGrid.MultiSelectedLayerIndices.Count > 0)
				{
					if (InputUtils.ShiftKeyPressed)
					{
						this.addLayerIndex = this.signLayerGrid.MultiSelectedLayerIndices[0];
					}
					else
					{
						this.addLayerIndex = this.signLayerGrid.MultiSelectedLayerIndices[0] + 1;
					}
					this.BeginAddLayerFlow(this.addLayerIndex);
					return;
				}
			}
		}
		else
		{
			if (Input.GetKeyDown(102))
			{
				this.ResetView();
			}
			else if (Input.GetKeyDown(127))
			{
				this.DeleteSelectedLayers("Deleted Layer(s)");
			}
			Vector2 vector = Vector2.zero;
			if (Input.GetKeyDown(273))
			{
				vector.y += 0.01f;
			}
			if (Input.GetKeyDown(274))
			{
				vector.y -= 0.01f;
			}
			if (Input.GetKeyDown(275))
			{
				vector.x += 0.01f;
			}
			if (Input.GetKeyDown(276))
			{
				vector.x -= 0.01f;
			}
			if (vector != Vector2.zero && !this.signLayerGrid.HasPlaceholder)
			{
				bool altKeyPressed = InputUtils.AltKeyPressed;
				if (InputUtils.ShiftKeyPressed)
				{
					vector *= (altKeyPressed ? 5f : 10f);
				}
				else if (altKeyPressed)
				{
					vector *= 0.1f;
				}
				this.TranslateSelection(vector);
				this.transformSettings.SetLayer(this.selectedLayer);
				this.MarkChanged();
			}
			if (Input.GetMouseButtonUp(0))
			{
				this.signLayerGrid.ReleaseDragAndDrop();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Refresh()
	{
		SignDataManager.Instance.TryGetSignComplexityInfo(this.workingSignId, out this.signComplexityInfo);
		this.RefreshComplexityInfo();
		bool savingAllowed = this.GetSavingAllowed();
		this.btnSave.Enabled = this.hasUnsavedChanges && savingAllowed;
		this.btnSaveCopy.Enabled = savingAllowed;
		base.RefreshBindings();
		this.IsDirty = false;
		Action onRefreshed = this.OnRefreshed;
		if (onRefreshed == null)
		{
			return;
		}
		onRefreshed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetSavingAllowed()
	{
		return this.signComplexityInfo.IsValid && this.signComplexityInfo.TotalComplexity <= 600f && this.signComplexityInfo.StackInfo.MaxCompStackIndex <= 7 && this.signComplexityInfo.StackInfo.MaxUVStackIndex <= 7;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2407419735U)
		{
			if (num <= 1429645657U)
			{
				if (num <= 279784058U)
				{
					if (num != 25739702U)
					{
						if (num != 75534831U)
						{
							if (num == 279784058U)
							{
								if (bindingName == "selectedlayername")
								{
									value = this.selectedLayerName;
									return true;
								}
							}
						}
						else if (bindingName == "shownewlayerpanel")
						{
							value = this.showNewLayerPanel.ToString();
							return true;
						}
					}
					else if (bindingName == "showstretchwarpsettings")
					{
						XUiC_CategoryList xuiC_CategoryList = this.categoryList;
						bool? flag;
						if (xuiC_CategoryList == null)
						{
							flag = null;
						}
						else
						{
							XUiC_CategoryEntry currentCategory = xuiC_CategoryList.CurrentCategory;
							if (currentCategory == null)
							{
								flag = null;
							}
							else
							{
								string categoryName = currentCategory.CategoryName;
								flag = ((categoryName != null) ? new bool?(categoryName.ContainsCaseInsensitive(typeof(SignData.StretchWarp).Name)) : null);
							}
						}
						bool? flag2 = flag;
						value = flag2.GetValueOrDefault().ToString();
						return true;
					}
				}
				else if (num <= 1369692037U)
				{
					if (num != 738251961U)
					{
						if (num == 1369692037U)
						{
							if (bindingName == "showtwirlwarpsettings")
							{
								XUiC_CategoryList xuiC_CategoryList2 = this.categoryList;
								bool? flag3;
								if (xuiC_CategoryList2 == null)
								{
									flag3 = null;
								}
								else
								{
									XUiC_CategoryEntry currentCategory2 = xuiC_CategoryList2.CurrentCategory;
									if (currentCategory2 == null)
									{
										flag3 = null;
									}
									else
									{
										string categoryName2 = currentCategory2.CategoryName;
										flag3 = ((categoryName2 != null) ? new bool?(categoryName2.ContainsCaseInsensitive(typeof(SignData.TwirlWarp).Name)) : null);
									}
								}
								bool? flag2 = flag3;
								value = flag2.GetValueOrDefault().ToString();
								return true;
							}
						}
					}
					else if (bindingName == "showperspectivewarpsettings")
					{
						XUiC_CategoryList xuiC_CategoryList3 = this.categoryList;
						bool? flag4;
						if (xuiC_CategoryList3 == null)
						{
							flag4 = null;
						}
						else
						{
							XUiC_CategoryEntry currentCategory3 = xuiC_CategoryList3.CurrentCategory;
							if (currentCategory3 == null)
							{
								flag4 = null;
							}
							else
							{
								string categoryName3 = currentCategory3.CategoryName;
								flag4 = ((categoryName3 != null) ? new bool?(categoryName3.ContainsCaseInsensitive(typeof(SignData.PerspectiveWarp).Name)) : null);
							}
						}
						bool? flag2 = flag4;
						value = flag2.GetValueOrDefault().ToString();
						return true;
					}
				}
				else if (num != 1394595989U)
				{
					if (num == 1429645657U)
					{
						if (bindingName == "showskewwarpsettings")
						{
							XUiC_CategoryList xuiC_CategoryList4 = this.categoryList;
							bool? flag5;
							if (xuiC_CategoryList4 == null)
							{
								flag5 = null;
							}
							else
							{
								XUiC_CategoryEntry currentCategory4 = xuiC_CategoryList4.CurrentCategory;
								if (currentCategory4 == null)
								{
									flag5 = null;
								}
								else
								{
									string categoryName4 = currentCategory4.CategoryName;
									flag5 = ((categoryName4 != null) ? new bool?(categoryName4.ContainsCaseInsensitive(typeof(SignData.SkewWarp).Name)) : null);
								}
							}
							bool? flag2 = flag5;
							value = flag2.GetValueOrDefault().ToString();
							return true;
						}
					}
				}
				else if (bindingName == "showsavenotice")
				{
					bool flag6 = false;
					if (this.signComplexityInfo.IsValid)
					{
						flag6 = this.signComplexityInfo.TotalComplexity > 600f || this.signComplexityInfo.StackInfo.MaxCompStackIndex > 7 || this.signComplexityInfo.StackInfo.MaxUVStackIndex > 7;
					}
					value = flag6.ToString();
					return true;
				}
			}
			else if (num <= 1574880052U)
			{
				if (num != 1475391586U)
				{
					if (num != 1543894150U)
					{
						if (num == 1574880052U)
						{
							if (bindingName == "showaddwarpsettings")
							{
								XUiC_CategoryList xuiC_CategoryList5 = this.categoryList;
								bool? flag7;
								if (xuiC_CategoryList5 == null)
								{
									flag7 = null;
								}
								else
								{
									XUiC_CategoryEntry currentCategory5 = xuiC_CategoryList5.CurrentCategory;
									if (currentCategory5 == null)
									{
										flag7 = null;
									}
									else
									{
										string categoryName5 = currentCategory5.CategoryName;
										flag7 = ((categoryName5 != null) ? new bool?(categoryName5.EqualsCaseInsensitive("AddWarp")) : null);
									}
								}
								bool? flag2 = flag7;
								value = flag2.GetValueOrDefault().ToString();
								return true;
							}
						}
					}
					else if (bindingName == "showtextsettings")
					{
						XUiC_CategoryList xuiC_CategoryList6 = this.categoryList;
						bool? flag8;
						if (xuiC_CategoryList6 == null)
						{
							flag8 = null;
						}
						else
						{
							XUiC_CategoryEntry currentCategory6 = xuiC_CategoryList6.CurrentCategory;
							if (currentCategory6 == null)
							{
								flag8 = null;
							}
							else
							{
								string categoryName6 = currentCategory6.CategoryName;
								flag8 = ((categoryName6 != null) ? new bool?(categoryName6.EqualsCaseInsensitive("Text")) : null);
							}
						}
						bool? flag2 = flag8;
						value = flag2.GetValueOrDefault().ToString();
						return true;
					}
				}
				else if (bindingName == "showgroupsettings")
				{
					XUiC_CategoryList xuiC_CategoryList7 = this.categoryList;
					bool? flag9;
					if (xuiC_CategoryList7 == null)
					{
						flag9 = null;
					}
					else
					{
						XUiC_CategoryEntry currentCategory7 = xuiC_CategoryList7.CurrentCategory;
						if (currentCategory7 == null)
						{
							flag9 = null;
						}
						else
						{
							string categoryName7 = currentCategory7.CategoryName;
							flag9 = ((categoryName7 != null) ? new bool?(categoryName7.EqualsCaseInsensitive("Group")) : null);
						}
					}
					bool? flag2 = flag9;
					value = flag2.GetValueOrDefault().ToString();
					return true;
				}
			}
			else if (num <= 2312798212U)
			{
				if (num != 2231848975U)
				{
					if (num == 2312798212U)
					{
						if (bindingName == "showkaleidowarpsettings")
						{
							XUiC_CategoryList xuiC_CategoryList8 = this.categoryList;
							bool? flag10;
							if (xuiC_CategoryList8 == null)
							{
								flag10 = null;
							}
							else
							{
								XUiC_CategoryEntry currentCategory8 = xuiC_CategoryList8.CurrentCategory;
								if (currentCategory8 == null)
								{
									flag10 = null;
								}
								else
								{
									string categoryName8 = currentCategory8.CategoryName;
									flag10 = ((categoryName8 != null) ? new bool?(categoryName8.ContainsCaseInsensitive(typeof(SignData.KaleidoWarp).Name)) : null);
								}
							}
							bool? flag2 = flag10;
							value = flag2.GetValueOrDefault().ToString();
							return true;
						}
					}
				}
				else if (bindingName == "showarcwarpsettings")
				{
					XUiC_CategoryList xuiC_CategoryList9 = this.categoryList;
					bool? flag11;
					if (xuiC_CategoryList9 == null)
					{
						flag11 = null;
					}
					else
					{
						XUiC_CategoryEntry currentCategory9 = xuiC_CategoryList9.CurrentCategory;
						if (currentCategory9 == null)
						{
							flag11 = null;
						}
						else
						{
							string categoryName9 = currentCategory9.CategoryName;
							flag11 = ((categoryName9 != null) ? new bool?(categoryName9.ContainsCaseInsensitive(typeof(SignData.ArcWarp).Name)) : null);
						}
					}
					bool? flag2 = flag11;
					value = flag2.GetValueOrDefault().ToString();
					return true;
				}
			}
			else if (num != 2347086190U)
			{
				if (num == 2407419735U)
				{
					if (bindingName == "showgridwarpsettings")
					{
						XUiC_CategoryList xuiC_CategoryList10 = this.categoryList;
						bool? flag12;
						if (xuiC_CategoryList10 == null)
						{
							flag12 = null;
						}
						else
						{
							XUiC_CategoryEntry currentCategory10 = xuiC_CategoryList10.CurrentCategory;
							if (currentCategory10 == null)
							{
								flag12 = null;
							}
							else
							{
								string categoryName10 = currentCategory10.CategoryName;
								flag12 = ((categoryName10 != null) ? new bool?(categoryName10.ContainsCaseInsensitive(typeof(SignData.GridWarp).Name)) : null);
							}
						}
						bool? flag2 = flag12;
						value = flag2.GetValueOrDefault().ToString();
						return true;
					}
				}
			}
			else if (bindingName == "warp_icon_arc")
			{
				value = this.warpIconArc;
				return true;
			}
		}
		else if (num <= 3368053736U)
		{
			if (num <= 2931353358U)
			{
				if (num != 2433615239U)
				{
					if (num != 2797060081U)
					{
						if (num == 2931353358U)
						{
							if (bindingName == "showcolorsettings")
							{
								XUiC_CategoryList xuiC_CategoryList11 = this.categoryList;
								bool? flag13;
								if (xuiC_CategoryList11 == null)
								{
									flag13 = null;
								}
								else
								{
									XUiC_CategoryEntry currentCategory11 = xuiC_CategoryList11.CurrentCategory;
									if (currentCategory11 == null)
									{
										flag13 = null;
									}
									else
									{
										string categoryName11 = currentCategory11.CategoryName;
										flag13 = ((categoryName11 != null) ? new bool?(categoryName11.EqualsCaseInsensitive("Color")) : null);
									}
								}
								bool? flag2 = flag13;
								value = flag2.GetValueOrDefault().ToString();
								return true;
							}
						}
					}
					else if (bindingName == "warp_icon_kaleido")
					{
						value = this.warpIconKaleido;
						return true;
					}
				}
				else if (bindingName == "showpolygonsettings")
				{
					XUiC_CategoryList xuiC_CategoryList12 = this.categoryList;
					bool? flag14;
					if (xuiC_CategoryList12 == null)
					{
						flag14 = null;
					}
					else
					{
						XUiC_CategoryEntry currentCategory12 = xuiC_CategoryList12.CurrentCategory;
						if (currentCategory12 == null)
						{
							flag14 = null;
						}
						else
						{
							string categoryName12 = currentCategory12.CategoryName;
							flag14 = ((categoryName12 != null) ? new bool?(categoryName12.EqualsCaseInsensitive("Polygon")) : null);
						}
					}
					bool? flag2 = flag14;
					value = flag2.GetValueOrDefault().ToString();
					return true;
				}
			}
			else if (num <= 3111165525U)
			{
				if (num != 3049165781U)
				{
					if (num == 3111165525U)
					{
						if (bindingName == "showtransformsettings")
						{
							XUiC_CategoryList xuiC_CategoryList13 = this.categoryList;
							bool? flag15;
							if (xuiC_CategoryList13 == null)
							{
								flag15 = null;
							}
							else
							{
								XUiC_CategoryEntry currentCategory13 = xuiC_CategoryList13.CurrentCategory;
								if (currentCategory13 == null)
								{
									flag15 = null;
								}
								else
								{
									string categoryName13 = currentCategory13.CategoryName;
									flag15 = ((categoryName13 != null) ? new bool?(categoryName13.EqualsCaseInsensitive("Transform")) : null);
								}
							}
							bool? flag2 = flag15;
							value = flag2.GetValueOrDefault().ToString();
							return true;
						}
					}
				}
				else if (bindingName == "warp_icon_bulge")
				{
					value = this.warpIconBulge;
					return true;
				}
			}
			else if (num != 3167422962U)
			{
				if (num == 3368053736U)
				{
					if (bindingName == "warp_icon_skew")
					{
						value = this.warpIconSkew;
						return true;
					}
				}
			}
			else if (bindingName == "warp_icon_grid")
			{
				value = this.warpIconGrid;
				return true;
			}
		}
		else if (num <= 3634381192U)
		{
			if (num != 3434567931U)
			{
				if (num != 3489864167U)
				{
					if (num == 3634381192U)
					{
						if (bindingName == "warp_icon_perspective")
						{
							value = this.warpIconPerspective;
							return true;
						}
					}
				}
				else if (bindingName == "warp_icon_stretch")
				{
					value = this.warpIconStretch;
					return true;
				}
			}
			else if (bindingName == "shownoisesettings")
			{
				XUiC_CategoryList xuiC_CategoryList14 = this.categoryList;
				bool? flag16;
				if (xuiC_CategoryList14 == null)
				{
					flag16 = null;
				}
				else
				{
					XUiC_CategoryEntry currentCategory14 = xuiC_CategoryList14.CurrentCategory;
					if (currentCategory14 == null)
					{
						flag16 = null;
					}
					else
					{
						string categoryName14 = currentCategory14.CategoryName;
						flag16 = ((categoryName14 != null) ? new bool?(categoryName14.EqualsCaseInsensitive("Noise")) : null);
					}
				}
				bool? flag2 = flag16;
				value = flag2.GetValueOrDefault().ToString();
				return true;
			}
		}
		else if (num <= 3960464195U)
		{
			if (num != 3886611224U)
			{
				if (num == 3960464195U)
				{
					if (bindingName == "showdebugpanel")
					{
						value = XUiC_SignEditorWindow.ShowDebugPanel.ToString();
						return true;
					}
				}
			}
			else if (bindingName == "showbulgewarpsettings")
			{
				XUiC_CategoryList xuiC_CategoryList15 = this.categoryList;
				bool? flag17;
				if (xuiC_CategoryList15 == null)
				{
					flag17 = null;
				}
				else
				{
					XUiC_CategoryEntry currentCategory15 = xuiC_CategoryList15.CurrentCategory;
					if (currentCategory15 == null)
					{
						flag17 = null;
					}
					else
					{
						string categoryName15 = currentCategory15.CategoryName;
						flag17 = ((categoryName15 != null) ? new bool?(categoryName15.ContainsCaseInsensitive(typeof(SignData.BulgeWarp).Name)) : null);
					}
				}
				bool? flag2 = flag17;
				value = flag2.GetValueOrDefault().ToString();
				return true;
			}
		}
		else if (num != 4088187011U)
		{
			if (num == 4265127676U)
			{
				if (bindingName == "warp_icon_twirl")
				{
					value = this.warpIconTwirl;
					return true;
				}
			}
		}
		else if (bindingName == "savenoticetext")
		{
			this.stringBuilder.Clear();
			if (this.signComplexityInfo.IsValid)
			{
				if (this.signComplexityInfo.TotalComplexity > 600f)
				{
					this.stringBuilder.AppendLine(Localization.Get("xuiSignComplexityExceeded", false, null));
				}
				if (this.signComplexityInfo.StackInfo.MaxCompStackIndex > 7)
				{
					if (this.stringBuilder.Length > 0)
					{
						this.stringBuilder.AppendLine();
					}
					this.stringBuilder.AppendLine(string.Format(Localization.Get("xuiSignNestedMaskLimit", false, null), this.signComplexityInfo.StackInfo.MaxCompStackIndex, 7));
				}
				if (this.signComplexityInfo.StackInfo.MaxUVStackIndex > 7)
				{
					if (this.stringBuilder.Length > 0)
					{
						this.stringBuilder.AppendLine();
					}
					this.stringBuilder.AppendLine(string.Format(Localization.Get("xuiSignNestedWarpLimit", false, null), this.signComplexityInfo.StackInfo.MaxUVStackIndex, 7));
				}
			}
			value = this.stringBuilder.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
			if (num <= 3167422962U)
			{
				if (num <= 2797060081U)
				{
					if (num != 2347086190U)
					{
						if (num == 2797060081U)
						{
							if (name == "warp_icon_kaleido")
							{
								this.warpIconKaleido = value;
								return true;
							}
						}
					}
					else if (name == "warp_icon_arc")
					{
						this.warpIconArc = value;
						return true;
					}
				}
				else if (num != 3049165781U)
				{
					if (num == 3167422962U)
					{
						if (name == "warp_icon_grid")
						{
							this.warpIconGrid = value;
							return true;
						}
					}
				}
				else if (name == "warp_icon_bulge")
				{
					this.warpIconBulge = value;
					return true;
				}
			}
			else if (num <= 3489864167U)
			{
				if (num != 3368053736U)
				{
					if (num == 3489864167U)
					{
						if (name == "warp_icon_stretch")
						{
							this.warpIconStretch = value;
							return true;
						}
					}
				}
				else if (name == "warp_icon_skew")
				{
					this.warpIconSkew = value;
					return true;
				}
			}
			else if (num != 3634381192U)
			{
				if (num == 4265127676U)
				{
					if (name == "warp_icon_twirl")
					{
						this.warpIconTwirl = value;
						return true;
					}
				}
			}
			else if (name == "warp_icon_perspective")
			{
				this.warpIconPerspective = value;
				return true;
			}
		}
		return flag;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <SetLayerSelected>g__ToggleNewLayerPopup|94_0(bool visible)
	{
		if (this.showNewLayerPanel == visible)
		{
			return;
		}
		if (!visible)
		{
			this.addLayerIndex = -1;
		}
		this.showNewLayerPanel = visible;
		base.RefreshBindings();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <RefreshLayerSettings>g__AddWarpCategories|95_0(ref XUiC_SignEditorWindow.<>c__DisplayClass95_0 A_1)
	{
		for (int i = 0; i < this.selectedLayer.warps.Count; i++)
		{
			SignData.SignWarp signWarp = this.selectedLayer.warps[i];
			if (!(signWarp is SignData.SkewWarp))
			{
				if (!(signWarp is SignData.BulgeWarp))
				{
					if (!(signWarp is SignData.TwirlWarp))
					{
						if (!(signWarp is SignData.KaleidoWarp))
						{
							if (!(signWarp is SignData.PerspectiveWarp))
							{
								if (!(signWarp is SignData.ArcWarp))
								{
									if (!(signWarp is SignData.StretchWarp))
									{
										if (!(signWarp is SignData.GridWarp))
										{
											Log.Error(string.Format("Unsupported layer warp type at index {0}.", i));
										}
										else
										{
											XUiC_CategoryList xuiC_CategoryList = this.categoryList;
											int num = A_1.i;
											A_1.i = num + 1;
											xuiC_CategoryList.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.GridWarp).Name, i), this.warpIconGrid, Localization.Get("lblSignGridWarpHeader", false, null));
										}
									}
									else
									{
										XUiC_CategoryList xuiC_CategoryList2 = this.categoryList;
										int num = A_1.i;
										A_1.i = num + 1;
										xuiC_CategoryList2.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.StretchWarp).Name, i), this.warpIconStretch, Localization.Get("lblSignStretchWarpHeader", false, null));
									}
								}
								else
								{
									XUiC_CategoryList xuiC_CategoryList3 = this.categoryList;
									int num = A_1.i;
									A_1.i = num + 1;
									xuiC_CategoryList3.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.ArcWarp).Name, i), this.warpIconArc, Localization.Get("lblSignArcWarpHeader", false, null));
								}
							}
							else
							{
								XUiC_CategoryList xuiC_CategoryList4 = this.categoryList;
								int num = A_1.i;
								A_1.i = num + 1;
								xuiC_CategoryList4.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.PerspectiveWarp).Name, i), this.warpIconPerspective, Localization.Get("lblSignPerspectiveWarpHeader", false, null));
							}
						}
						else
						{
							XUiC_CategoryList xuiC_CategoryList5 = this.categoryList;
							int num = A_1.i;
							A_1.i = num + 1;
							xuiC_CategoryList5.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.KaleidoWarp).Name, i), this.warpIconKaleido, Localization.Get("lblSignKaleidoWarpHeader", false, null));
						}
					}
					else
					{
						XUiC_CategoryList xuiC_CategoryList6 = this.categoryList;
						int num = A_1.i;
						A_1.i = num + 1;
						xuiC_CategoryList6.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.TwirlWarp).Name, i), this.warpIconTwirl, Localization.Get("lblSignTwirlWarpHeader", false, null));
					}
				}
				else
				{
					XUiC_CategoryList xuiC_CategoryList7 = this.categoryList;
					int num = A_1.i;
					A_1.i = num + 1;
					xuiC_CategoryList7.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.BulgeWarp).Name, i), this.warpIconBulge, Localization.Get("lblSignPinchBulgeWarpHeader", false, null));
				}
			}
			else
			{
				XUiC_CategoryList xuiC_CategoryList8 = this.categoryList;
				int num = A_1.i;
				A_1.i = num + 1;
				xuiC_CategoryList8.SetCategoryEntry(num, string.Format("{0}_{1}", typeof(SignData.SkewWarp).Name, i), this.warpIconSkew, Localization.Get("lblSignSkewWarpHeader", false, null));
			}
		}
		if (this.selectedLayer.warps.Count < 4)
		{
			XUiC_CategoryList xuiC_CategoryList9 = this.categoryList;
			int num = A_1.i;
			A_1.i = num + 1;
			xuiC_CategoryList9.SetCategoryEntry(num, "AddWarp", "ui_game_symbol_add", Localization.Get("lblSignAddWarp", false, null));
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <HandleMiddleMouseDragged>g__UpdateViewOffset|121_0()
	{
		Vector2 vector = this.middleMouseDrag.RawPosition - this.middleMouseDrag.StartPosition;
		if (this.dragStartViewScaleMode)
		{
			float num = this.dragStartViewScale * Mathf.Pow(5f, vector.y);
			this.viewScale = num;
			this.viewOffset = this.dragStartViewOffset * (this.dragStartViewScale / num);
			return;
		}
		this.viewScale = this.dragStartViewScale;
		this.viewOffset = this.dragStartViewOffset + vector;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <HandleMiddleMouseDragged>g__BeginDrag|121_1()
	{
		this.middleMouseDrag.StartPosition = this.xui.GetMouseXUiPosition().AsVector2();
		this.middleMouseDrag.RawPosition = this.middleMouseDrag.StartPosition;
		this.middleMouseDrag.IsDragging = true;
		this.dragStartViewOffset = this.viewOffset;
		this.dragStartViewScale = this.viewScale;
		this.dragStartViewScaleMode = InputUtils.ControlKeyPressed;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <HandleMiddleMouseDragged>g__EndDrag|121_2()
	{
		this.middleMouseDrag.IsDragging = false;
		this.<HandleMiddleMouseDragged>g__UpdateViewOffset|121_0();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxWarpsPerLayer = 4;

	public static string ID = "";

	public static bool ShowDebugPanel = false;

	public Action OnRefreshed;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas targetCanvas;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCanvas targetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId sourceSignId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData sourceSignData;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId workingSignId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData workingSignData;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture pulseOverlayMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHighlightPulseDuration = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float highlightStartTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture hoverOverlayMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHoverFadeDuration = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hoverOverlayOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer hoveredLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer hoverOverlayLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previewAspect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerGrid signLayerGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer selectedLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_SignLayerSettings> allSettings = new List<XUiC_SignLayerSettings>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_SignWarpSettings> allWarpSettings = new List<XUiC_SignWarpSettings>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_SignColorSettings> allColorSettings = new List<XUiC_SignColorSettings>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AddWarpSettings addWarpSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignTransformSettings transformSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController canvasView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignNewLayerPanel newLayerPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSaveCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnQuit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtAspectHorizontal;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtAspectVertical;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAspectFit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAspectSquare;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSignName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtLayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SizeBar sizeBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SignData.SignLayer> layerClipboard = new List<SignData.SignLayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ConfirmationPrompt confirmationPrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignDebugPanel debugPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerDragAndDropIcon dragAndDropIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedLayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showNewLayerPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasUnsavedChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 canvasAspect = Vector2.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool aspectMaskEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SignEditorWindow.HistoryStateManager historyStateManager = new XUiC_SignEditorWindow.HistoryStateManager();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignEditorWindow.AffectMode currentAffectMode;

	public Action<XUiC_SignEditorWindow.AffectMode> OnAffectModeChanged;

	public SignComplexityInfo signComplexityInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconSkew;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconBulge;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconTwirl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconKaleido;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconPerspective;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconArc;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconStretch;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder stringBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> originalLayerIndices = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SignData.SignLayer> originalLayers = new List<SignData.SignLayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int addLayerIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float complexityHoverOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignEditorWindow.DragInfo middleMouseDrag;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 viewOffset = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float viewScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 dragStartViewOffset = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dragStartViewScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dragStartViewScaleMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignEditorWindow.DragInfo leftMouseDrag;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2> layerDragStartPositions = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 affectDragGroupStartPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2> affectDragChildStartPositions = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> indicesToSelect = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime heldInputRetriggerTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan firstRetriggerDelay = TimeSpan.FromSeconds(0.3499999940395355);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan subsequentRetriggerDelay = TimeSpan.FromSeconds(0.05000000074505806);

	public enum AffectMode
	{
		None,
		Children,
		Pivot
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class HistoryStateManager
	{
		public void Init(string baseChangeDescription, XUiC_SignLayerGrid signLayerGrid, XUiC_TextInput txtSignName, GlobalSignId workingSignId, SignData workingSignData, XUiC_SignEditorWindow editorWindow)
		{
			this.Reset();
			this.signLayerGrid = signLayerGrid;
			this.txtSignName = txtSignName;
			this.workingSignId = workingSignId;
			this.workingSignData = workingSignData;
			this.editorWindow = editorWindow;
			XUiC_SignEditorWindow.HistoryStateManager.HistoryState historyState = this.UnpoolOrCreateHistoryState();
			historyState.InitPending(baseChangeDescription);
			this.historyStates.Add(historyState);
			this.currentStateIndex++;
		}

		public void Reset()
		{
			this.PoolHistoryStates(0, this.historyStates.Count);
			this.currentStateIndex = -1;
			this.signLayerGrid = null;
			this.workingSignData = null;
			this.workingSignId = GlobalSignId.InvalidId;
			this.editorWindow = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PoolHistoryStates(int index, int count)
		{
			if (index < 0 || index + count > this.historyStates.Count)
			{
				Log.Error(string.Format("Invalid history state range specified in PoolHistoryStates. Inputs were ({0}, {1}). historyStates.Count is {2}.", index, count, this.historyStates.Count));
				return;
			}
			for (int i = index; i < index + count; i++)
			{
				this.historyStates[i].Clear();
				this.historyStatePool.Push(this.historyStates[i]);
			}
			this.historyStates.RemoveRange(index, count);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_SignEditorWindow.HistoryStateManager.HistoryState UnpoolOrCreateHistoryState()
		{
			if (this.historyStatePool.Count > 0)
			{
				return this.historyStatePool.Pop();
			}
			return new XUiC_SignEditorWindow.HistoryStateManager.HistoryState();
		}

		public void ProcessPendingChange(string changeDescription, bool forceDirty = false)
		{
			if (this.currentStateIndex < this.historyStates.Count - 1)
			{
				this.PoolHistoryStates(this.currentStateIndex + 1, this.historyStates.Count - (this.currentStateIndex + 1));
			}
			if (this.currentStateIndex < 0)
			{
				Log.Warning(string.Format("Unexpected state in HistoryStateManager. Current index is {0}. Expected >= 0. Likely missed call to Init.", this.currentStateIndex));
			}
			else
			{
				this.historyStateDirty = this.historyStateDirty || forceDirty;
				if (!this.historyStateDirty && this.historyStates[this.currentStateIndex].Description == changeDescription)
				{
					this.historyStates[this.currentStateIndex].Refresh();
					return;
				}
				if (!this.historyStates[this.currentStateIndex].HasSnapshot)
				{
					this.historyStates[this.currentStateIndex].SetSnapshotData(this.signLayerGrid, this.workingSignData, this.editorWindow);
				}
			}
			XUiC_SignEditorWindow.HistoryStateManager.HistoryState historyState = this.UnpoolOrCreateHistoryState();
			historyState.InitPending(changeDescription);
			this.historyStates.Add(historyState);
			this.currentStateIndex++;
			if (this.historyStates.Count > 100)
			{
				int num = this.historyStates.Count - 100;
				this.PoolHistoryStates(0, num);
				this.currentStateIndex -= num;
			}
			if (this.currentStateIndex != this.historyStates.Count - 1)
			{
				Log.Error(string.Format("Invalid state index in HistoryStateManager. Current index is {0}. Expected {1}.", this.currentStateIndex, this.historyStates.Count - 1));
			}
			this.historyStateDirty = false;
		}

		public bool TryUndo()
		{
			if (this.currentStateIndex <= 0)
			{
				return false;
			}
			if (!this.historyStates[this.currentStateIndex].HasSnapshot)
			{
				this.historyStates[this.currentStateIndex].SetSnapshotData(this.signLayerGrid, this.workingSignData, this.editorWindow);
			}
			this.currentStateIndex--;
			this.ApplyHistoryState(this.historyStates[this.currentStateIndex]);
			return true;
		}

		public bool TryRedo()
		{
			if (this.currentStateIndex >= this.historyStates.Count - 1)
			{
				return false;
			}
			if (!this.historyStates[this.currentStateIndex].HasSnapshot)
			{
				Log.Error(string.Format("Unexpected state in HistoryStateManager. No snapshot exists for current state {0} which is not the most recent recorded state. ", this.currentStateIndex) + "Only the most recent pending state should lack a snapshot.");
				this.historyStates[this.currentStateIndex].SetSnapshotData(this.signLayerGrid, this.workingSignData, this.editorWindow);
			}
			this.currentStateIndex++;
			this.ApplyHistoryState(this.historyStates[this.currentStateIndex]);
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ApplyHistoryState(XUiC_SignEditorWindow.HistoryStateManager.HistoryState historyState)
		{
			this.workingSignData.CloneLayersFrom(historyState.SnapshotSignData);
			this.workingSignData.name = historyState.SnapshotSignData.name;
			this.signLayerGrid.MultiSelectedLayerIndices.Clear();
			int count = this.workingSignData.layers.Count;
			if (count == 0)
			{
				this.editorWindow.BeginAddLayerFlow(0);
			}
			else
			{
				this.signLayerGrid.MultiSelectedLayerIndices.AddRange(historyState.MultiSelectedLayerIndices);
				this.signLayerGrid.UpdateLayers(this.workingSignId, Mathf.Clamp(historyState.SelectedLayerIndex, 0, count - 1), false, true);
			}
			this.txtSignName.Text = historyState.SnapshotSignData.name;
			this.editorWindow.RestoreAffectModeFromHistory(historyState.AffectMode);
			this.ForceDirty();
		}

		public void ForceDirty()
		{
			this.historyStateDirty = true;
		}

		[Conditional("DEBUG_LOG_SIGN_HISTORY")]
		public void LogUndoRedoStack()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[Sign History]\n");
			for (int i = 0; i < this.historyStates.Count; i++)
			{
				XUiC_SignEditorWindow.HistoryStateManager.HistoryState historyState = this.historyStates[i];
				if (i == this.currentStateIndex)
				{
					stringBuilder.Append("-> ");
				}
				stringBuilder.Append(string.Concat(new string[]
				{
					"[",
					historyState.Time.ToString("u"),
					"] ",
					historyState.Description,
					" (Snapshot: ",
					historyState.HasSnapshot.ToString(),
					")\n"
				}));
			}
			Log.Out(stringBuilder.ToString());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cMaxHistoryStates = 100;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_SignLayerGrid signLayerGrid;

		[PublicizedFrom(EAccessModifier.Private)]
		public GlobalSignId workingSignId = GlobalSignId.InvalidId;

		[PublicizedFrom(EAccessModifier.Private)]
		public SignData workingSignData;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_TextInput txtSignName;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_SignEditorWindow editorWindow;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Stack<XUiC_SignEditorWindow.HistoryStateManager.HistoryState> historyStatePool = new Stack<XUiC_SignEditorWindow.HistoryStateManager.HistoryState>(100);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<XUiC_SignEditorWindow.HistoryStateManager.HistoryState> historyStates = new List<XUiC_SignEditorWindow.HistoryStateManager.HistoryState>(100);

		[PublicizedFrom(EAccessModifier.Private)]
		public int currentStateIndex = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool historyStateDirty;

		[PublicizedFrom(EAccessModifier.Private)]
		public class HistoryState
		{
			public string Description
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public DateTime Time
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public int SelectedLayerIndex
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public List<int> MultiSelectedLayerIndices
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			} = new List<int>(10);

			public SignData SnapshotSignData
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public XUiC_SignEditorWindow.AffectMode AffectMode
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public bool HasSnapshot
			{
				get
				{
					return this.SnapshotSignData != null;
				}
			}

			public void InitPending(string description)
			{
				this.Description = description;
				this.Time = DateTime.Now;
			}

			public void Refresh()
			{
				this.Time = DateTime.Now;
			}

			public void SetSnapshotData(XUiC_SignLayerGrid signLayerGrid, SignData workingSignData, XUiC_SignEditorWindow editorWindow)
			{
				this.SetSnapshotData(signLayerGrid.SelectedLayerIndex, signLayerGrid.MultiSelectedLayerIndices, workingSignData, editorWindow.CurrentAffectMode);
			}

			public void SetSnapshotData(int selectedLayerIndex, List<int> multiSelectedLayerIndices, SignData workingSignData, XUiC_SignEditorWindow.AffectMode affectMode)
			{
				this.SelectedLayerIndex = selectedLayerIndex;
				this.MultiSelectedLayerIndices.Clear();
				this.MultiSelectedLayerIndices.AddRange(multiSelectedLayerIndices);
				this.SnapshotSignData = SignData.Duplicate(workingSignData, "");
				this.AffectMode = affectMode;
			}

			public void Clear()
			{
				this.MultiSelectedLayerIndices.Clear();
				this.SnapshotSignData = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct DragInfo
	{
		public Vector2 StartPosition;

		public Vector2 RawPosition;

		public bool IsDragging;
	}
}
