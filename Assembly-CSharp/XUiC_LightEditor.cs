using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LightEditor : XUiController
{
	public static void Open(LocalPlayerUI _playerUi, TileEntityLight _te, Vector3i _blockPos, World _world, BlockLight _block)
	{
		XUiC_LightEditor childByType = _playerUi.xui.FindWindowGroupByName(XUiC_LightEditor.ID).GetChildByType<XUiC_LightEditor>();
		childByType.tileEntityLight = _te;
		childByType.blockPos = _blockPos;
		childByType.world = _world;
		ChunkCluster chunkCache = _world.ChunkCache;
		childByType.chunk = (Chunk)chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z));
		Transform transform = childByType.chunk.GetBlockEntity(_blockPos).transform.Find("MainLight");
		if (transform != null)
		{
			childByType.lightLOD = transform.GetComponent<LightLOD>();
			if (childByType.lightLOD != null)
			{
				childByType.light = childByType.lightLOD.GetLight();
			}
		}
		childByType.OpenAllValues();
		childByType.block = _block;
		_playerUi.windowManager.Open(XUiC_LightEditor.ID, true);
		if (childByType.light != null && childByType.light.type == 2)
		{
			childByType.panelAngle.IsVisible = false;
			childByType.rangeGizmo = Object.Instantiate<WireFrameSphere>(Resources.Load<WireFrameSphere>("Prefabs/prefabSphereWF"), childByType.light.transform);
			childByType.rangeGizmo.center = childByType.light.gameObject.transform.position;
			childByType.rangeGizmo.name = "Range Gizmo";
			childByType.rangeGizmo.newRadius = childByType.light.range;
		}
		if (childByType.lightLOD != null)
		{
			if (childByType.lightLOD.LightStateType == LightStateType.Static || childByType.lightLOD.LightStateType == LightStateType.Fluctuating)
			{
				childByType.panelRate.IsVisible = false;
			}
			if (childByType.lightLOD.LightStateType != LightStateType.Fluctuating)
			{
				childByType.panelDelay.IsVisible = false;
			}
		}
		if (!XUiC_LightEditor.hasCopiedValues)
		{
			childByType.btnPaste.ViewComponent.IsVisible = false;
		}
		childByType.CopyUIValues(out childByType.startValues);
	}

	public override void Init()
	{
		base.Init();
		XUiC_LightEditor.ID = base.WindowGroup.Id;
		this.cbxLightType = (XUiC_ComboBoxList<LightType>)base.GetChildById("cbxLightType");
		this.cbxLightType.Elements.Add(2);
		this.cbxLightType.Elements.Add(0);
		this.cbxLightType.OnValueChanged += this.CbxLightType_OnValueChanged;
		this.cbxLightShadow = (XUiC_ComboBoxEnum<LightShadows>)base.GetChildById("cbxLightShadow");
		this.cbxLightShadow.OnValueChanged += this.CbxLightShadow_OnValueChanged;
		this.cbxColorPresetList = (XUiC_ComboBoxList<string>)base.GetChildById("cbxPresets");
		this.cbxLightState = (XUiC_ComboBoxEnum<LightStateType>)base.GetChildById("cbxLightState");
		this.cbxLightState.OnValueChanged += this.CbxLightState_OnValueChanged;
		foreach (KeyValuePair<string, Color> keyValuePair in XUiC_LightEditor.ColorPresets)
		{
			this.cbxColorPresetList.Elements.Add(keyValuePair.Key);
		}
		this.cbxColorPresetList.OnValueChanged += this.CbxColorPresetList_OnValueChanged;
		this.colorPicker = (XUiC_ColorPicker)base.GetChildById("lightColor");
		this.colorPicker.OnSelectedColorChanged += this.ColorPicker_OnSelectedColorChanged;
		this.panelAngle = base.GetChildById("panelAngle").ViewComponent;
		this.panelRate = base.GetChildById("panelRate").ViewComponent;
		this.panelDelay = base.GetChildById("panelDelay").ViewComponent;
		this.cbxRange = (XUiC_ComboBoxFloat)base.GetChildById("cbxRange");
		this.cbxRange.OnValueChanged += this.CbxRange_OnValueChanged;
		this.cbxAngle = (XUiC_ComboBoxFloat)base.GetChildById("cbxAngle");
		this.cbxAngle.OnValueChanged += this.CbxAngle_OnValueChanged;
		this.cbxRate = (XUiC_ComboBoxFloat)base.GetChildById("cbxRate");
		this.cbxRate.OnValueChanged += this.CbxRate_OnValueChanged;
		this.cbxDelay = (XUiC_ComboBoxFloat)base.GetChildById("cbxDelay");
		this.cbxDelay.OnValueChanged += this.CbxDelay_OnValueChanged;
		this.cbxIntensity = (XUiC_ComboBoxFloat)base.GetChildById("cbxIntensity");
		this.cbxIntensity.OnValueChanged += this.CbxIntensity_OnValueChanged;
		this.btnOk = base.GetChildById("btnOk").GetChildByType<XUiC_SimpleButton>();
		this.btnOk.OnPressed += this.BtnSave_OnPressed;
		this.btnCancel = base.GetChildById("btnCancel").GetChildByType<XUiC_SimpleButton>();
		this.btnCancel.OnPressed += this.BtnCancel_OnPressed;
		this.btnOnOff = base.GetChildById("btnOnOff").GetChildByType<XUiC_SimpleButton>();
		this.btnOnOff.OnPressed += this.BtnOnOff_OnPressed;
		this.btnRestore = base.GetChildById("btnRestoreDefaults").GetChildByType<XUiC_SimpleButton>();
		this.btnRestore.OnPressed += this.BtnRestore_OnPressed;
		this.btnCopy = base.GetChildById("btnCopy").GetChildByType<XUiC_SimpleButton>();
		this.btnCopy.OnPressed += this.BtnCopy_OnPressed;
		this.btnPaste = base.GetChildById("btnPaste").GetChildByType<XUiC_SimpleButton>();
		this.btnPaste.OnPressed += this.BtnPaste_OnPressed;
		this.txtColorR = base.GetChildById("txtColorR") as XUiC_TextInput;
		this.txtColorR.OnChangeHandler += this.TxtColorR_OnChangeHandler;
		this.txtColorG = base.GetChildById("txtColorG") as XUiC_TextInput;
		this.txtColorG.OnChangeHandler += this.TxtColorG_OnChangeHandler;
		this.txtColorB = base.GetChildById("txtColorB") as XUiC_TextInput;
		this.txtColorB.OnChangeHandler += this.TxtColorB_OnChangeHandler;
		this.txtHex = base.GetChildById("txtHex") as XUiC_TextInput;
		UIInput uiinput = this.txtHex.UIInput;
		uiinput.onValidate = (UIInput.OnValidate)Delegate.Combine(uiinput.onValidate, new UIInput.OnValidate(GameUtils.ValidateHexInput));
		this.txtHex.OnChangeHandler += this.TxtHex_OnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPaste_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (XUiC_LightEditor.hasCopiedValues)
		{
			this.SetIntensity((float)(this.cbxIntensity.Value = (double)XUiC_LightEditor.copyValues.intensity));
			this.SetRange((float)(this.cbxRange.Value = (double)XUiC_LightEditor.copyValues.range));
			this.cbxLightType.Value = XUiC_LightEditor.copyValues.type;
			this.SetType(XUiC_LightEditor.copyValues.type);
			this.SetPreset(this.cbxColorPresetList.Value = XUiC_LightEditor.copyColorPreset);
			this.cbxLightState.Value = XUiC_LightEditor.copyValues.stateType;
			this.SetState(XUiC_LightEditor.copyValues.stateType);
			this.cbxLightShadow.Value = XUiC_LightEditor.copyValues.shadows;
			this.SetShadow(XUiC_LightEditor.copyValues.shadows);
			this.SetAngle((float)(this.cbxAngle.Value = (double)XUiC_LightEditor.copyValues.spotAngle));
			this.SetRate((float)(this.cbxRate.Value = (double)XUiC_LightEditor.copyValues.stateRate));
			this.SetDelay((float)(this.cbxDelay.Value = (double)XUiC_LightEditor.copyValues.stateDelay));
			this.colorPicker.SelectedColor = XUiC_LightEditor.copyValues.color;
			this.SetHex(XUiC_LightEditor.copyValues.color);
			this.SetRGB(XUiC_LightEditor.copyValues.color);
			this.panelAngle.IsVisible = XUiC_LightEditor.copyValues.type != 2;
			if (this.rangeGizmo != null)
			{
				this.rangeGizmo.gameObject.GetComponent<LineRenderer>().enabled = XUiC_LightEditor.copyValues.type == 2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopy_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.CopyUIValues(out XUiC_LightEditor.copyValues);
		XUiC_LightEditor.hasCopiedValues = true;
		XUiC_LightEditor.copyColorPreset = this.cbxColorPresetList.Value;
		this.btnPaste.ViewComponent.IsVisible = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyUIValues(out XUiC_LightEditor.LightValues values)
	{
		values = default(XUiC_LightEditor.LightValues);
		values.type = this.cbxLightType.Value;
		values.shadows = this.cbxLightShadow.Value;
		values.intensity = (float)this.cbxIntensity.Value;
		values.range = (float)this.cbxRange.Value;
		values.color = this.colorPicker.SelectedColor;
		values.spotAngle = (float)this.cbxAngle.Value;
		if (this.lightLOD)
		{
			values.emissiveColor = this.lightLOD.EmissiveColor;
		}
		values.stateType = this.cbxLightState.Value;
		values.stateRate = (float)this.cbxRate.Value;
		values.stateDelay = (float)this.cbxDelay.Value;
	}

	public void AssignRate(float _rate)
	{
		this.cbxRate.Value = (double)_rate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOnOff_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.lightLOD != null)
		{
			this.lightLOD.SwitchOnOff(!this.lightLOD.bSwitchedOn, true);
			this.SetEmissiveColor(this.light.color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtHex_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length < 6)
		{
			return;
		}
		if (this.isOpen)
		{
			NumberStyles numberStyles = NumberStyles.HexNumber;
			byte b = (byte)int.Parse(_text.Substring(0, 2), numberStyles);
			byte b2 = (byte)int.Parse(_text.Substring(2, 2), numberStyles);
			byte b3 = (byte)int.Parse(_text.Substring(4, 2), numberStyles);
			Color32 color;
			color..ctor(b, b2, b3, byte.MaxValue);
			this.colorPicker.SelectedColor = (this.light.color = color);
			this.SetRGB(color);
			this.SetEmissiveColor(color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtColorB_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		int num;
		if (this.isOpen && int.TryParse(_text, out num))
		{
			num = Mathf.Clamp(num, 0, 255);
			if (!_text.Equals(num.ToString()))
			{
				this.txtColorB.Text = num.ToString();
			}
			if (this.light != null)
			{
				Color color;
				color..ctor(this.light.color.r, this.light.color.g, (float)num / 255f);
				this.light.color = color;
				this.SetEmissiveColor(color);
				this.colorPicker.SelectedColor = color;
				this.SetHex(color);
				if (this.shouldChangePreset)
				{
					this.cbxColorPresetList.Value = XUiC_LightEditor.ColorPresets.Keys.First<string>();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtColorG_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		int num;
		if (this.isOpen && int.TryParse(_text, out num))
		{
			num = Mathf.Clamp(num, 0, 255);
			if (!_text.Equals(num.ToString()))
			{
				this.txtColorG.Text = num.ToString();
			}
			if (this.light != null)
			{
				Color color;
				color..ctor(this.light.color.r, (float)num / 255f, this.light.color.b);
				this.light.color = color;
				this.SetEmissiveColor(color);
				this.colorPicker.SelectedColor = color;
				this.SetHex(color);
				if (this.shouldChangePreset)
				{
					this.cbxColorPresetList.Value = XUiC_LightEditor.ColorPresets.Keys.First<string>();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtColorR_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		int num;
		if (this.isOpen && int.TryParse(_text, out num))
		{
			num = Mathf.Clamp(num, 0, 255);
			if (!_text.Equals(num.ToString()))
			{
				this.txtColorR.Text = num.ToString();
			}
			if (this.light != null)
			{
				Color color;
				color..ctor((float)num / 255f, this.light.color.g, this.light.color.b);
				this.light.color = color;
				this.SetEmissiveColor(color);
				this.colorPicker.SelectedColor = color;
				this.SetHex(color);
				if (this.shouldChangePreset)
				{
					this.cbxColorPresetList.Value = XUiC_LightEditor.ColorPresets.Keys.First<string>();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxLightType_OnValueChanged(XUiController _sender, LightType _oldValue, LightType _newValue)
	{
		this.SetType(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxColorPresetList_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		this.SetPreset(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxRange_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		this.SetRange((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxIntensity_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		this.SetIntensity((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxAngle_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		this.SetAngle((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxRate_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		this.SetRate((float)_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxLightState_OnValueChanged(XUiController _sender, LightStateType _oldValue, LightStateType _newValue)
	{
		this.SetState(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxLightShadow_OnValueChanged(XUiController _sender, LightShadows _oldValue, LightShadows _newValue)
	{
		this.SetShadow(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxDelay_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRestore_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.light || !this.lightLOD)
		{
			return;
		}
		this.LoadDefaultValues();
		this.cbxLightType.Value = this.defaultValues.type;
		this.cbxLightShadow.Value = this.defaultValues.shadows;
		this.cbxRange.Value = (double)this.defaultValues.range;
		this.cbxIntensity.Value = (double)this.defaultValues.intensity;
		this.colorPicker.SelectedColor = this.defaultValues.color;
		this.cbxAngle.Value = (double)this.defaultValues.spotAngle;
		this.cbxLightState.Value = this.defaultValues.stateType;
		this.cbxRate.Value = (double)this.defaultValues.stateRate;
		this.cbxDelay.Value = (double)this.defaultValues.stateDelay;
		this.panelRate.IsVisible = false;
		this.panelDelay.IsVisible = false;
		this.SetRGB(this.defaultValues.color);
		this.SetHex(this.defaultValues.color);
		if (this.rangeGizmo != null)
		{
			this.rangeGizmo.newRadius = this.defaultValues.range;
		}
		this.SetLightFromValues(this.defaultValues);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.LoadDefaultValues();
		this.CopyUIValues(out this.startValues);
		if (this.defaultValues.IsEqual(this.startValues))
		{
			if (this.tileEntityLight != null)
			{
				this.chunk.RemoveTileEntity(this.world, this.tileEntityLight);
			}
		}
		else
		{
			if (this.tileEntityLight == null)
			{
				BlockLight blockLight = this.world.GetBlock(this.blockPos).Block as BlockLight;
				if (blockLight != null)
				{
					TileEntity tileEntity = this.chunk.GetTileEntity(World.toBlock(this.blockPos));
					if (tileEntity == null)
					{
						tileEntity = blockLight.CreateTileEntity(this.chunk);
						tileEntity.localChunkPos = World.toBlock(this.blockPos);
						this.chunk.AddTileEntity(tileEntity);
					}
					this.tileEntityLight = tileEntity as TileEntityLight;
				}
			}
			this.tileEntityLight.LightType = this.startValues.type;
			this.tileEntityLight.LightShadows = this.startValues.shadows;
			this.tileEntityLight.LightRange = this.startValues.range;
			this.tileEntityLight.LightIntensity = this.startValues.intensity;
			this.tileEntityLight.LightColor = this.startValues.color;
			this.tileEntityLight.LightAngle = this.startValues.spotAngle;
			this.tileEntityLight.LightState = this.startValues.stateType;
			this.tileEntityLight.Rate = this.startValues.stateRate;
			this.tileEntityLight.Delay = this.startValues.stateDelay;
			this.tileEntityLight.SetModified();
		}
		this.xui.playerUI.windowManager.Close("lightProperties");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenLightType()
	{
		if (this.light != null)
		{
			this.cbxLightType.Value = this.light.type;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenLightShadows()
	{
		if (this.light != null)
		{
			this.cbxLightShadow.Value = this.light.shadows;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenRange()
	{
		if (this.light != null)
		{
			this.cbxRange.Value = (double)this.light.range;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenIntensity()
	{
		if (this.lightLOD != null)
		{
			this.cbxIntensity.Value = (double)this.lightLOD.MaxIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenSelectedColor()
	{
		if (this.light != null)
		{
			this.colorPicker.SelectedColor = this.light.color;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenAngle()
	{
		if (this.light != null && this.light.type == null)
		{
			this.cbxAngle.Value = (double)this.light.spotAngle;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenColorRGB()
	{
		if (this.light != null)
		{
			Color32 color = this.light.color;
			this.txtColorR.Text = color.r.ToString();
			this.txtColorG.Text = color.g.ToString();
			this.txtColorB.Text = color.b.ToString();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenHex()
	{
		if (this.light != null)
		{
			this.SetHex(this.light.color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenState()
	{
		if (this.lightLOD != null)
		{
			this.cbxLightState.Value = this.lightLOD.LightStateType;
			if (this.lightLOD.LightStateType == LightStateType.Static || this.lightLOD.LightStateType == LightStateType.Fluctuating)
			{
				this.panelRate.IsVisible = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenRate()
	{
		if (this.lightLOD != null)
		{
			this.cbxRate.Value = (double)this.lightLOD.StateRate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenDelay()
	{
		if (this.lightLOD != null)
		{
			this.cbxDelay.Value = (double)this.lightLOD.FluxDelay;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenAllValues()
	{
		if (this.light != null && this.lightLOD != null)
		{
			this.startValues = this.MakeValues(this.lightLOD);
		}
		this.OpenLightType();
		this.OpenLightShadows();
		this.OpenRange();
		this.OpenAngle();
		this.OpenIntensity();
		this.OpenSelectedColor();
		this.OpenColorRGB();
		this.OpenHex();
		this.OpenState();
		this.OpenRate();
		this.OpenDelay();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadDefaultValues()
	{
		Transform transform = DataLoader.LoadAsset<GameObject>(this.block.Properties.GetValue("Model"), false).transform.Find("MainLight");
		if (!transform)
		{
			Log.Error("MainLight missing for {0}", new object[] { this.block.Properties.GetValue("Model") });
			return;
		}
		LightLOD component = transform.GetComponent<LightLOD>();
		this.defaultValues = this.MakeValues(component);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LightEditor.LightValues MakeValues(LightLOD _lightLOD)
	{
		Light light = _lightLOD.GetLight();
		XUiC_LightEditor.LightValues lightValues;
		lightValues.type = light.type;
		lightValues.shadows = light.shadows;
		lightValues.range = light.range;
		lightValues.intensity = light.intensity;
		lightValues.color = light.color;
		lightValues.spotAngle = light.spotAngle;
		lightValues.emissiveColor = _lightLOD.EmissiveColor;
		lightValues.stateType = LightStateType.Static;
		lightValues.stateRate = 1f;
		lightValues.stateDelay = 1f;
		return lightValues;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetType(LightType _newType)
	{
		if (this.light != null)
		{
			this.light.type = _newType;
		}
		if (_newType == 2)
		{
			this.panelAngle.IsVisible = false;
			if (this.rangeGizmo != null)
			{
				this.rangeGizmo.gameObject.GetComponent<LineRenderer>().enabled = true;
				return;
			}
		}
		else
		{
			this.panelAngle.IsVisible = true;
			if (this.rangeGizmo != null)
			{
				this.rangeGizmo.gameObject.GetComponent<LineRenderer>().enabled = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPreset(string _newPreset)
	{
		if (_newPreset.ToString() != "Custom" && this.light != null)
		{
			this.light.color = XUiC_LightEditor.ColorPresets[_newPreset];
			this.SetEmissiveColor(this.light.color);
			this.shouldChangePreset = false;
			this.SetRGB(this.light.color);
			this.SetHex(this.light.color);
			this.shouldChangePreset = true;
			this.colorPicker.SelectedColor = XUiC_LightEditor.ColorPresets[_newPreset];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRange(float _newRange)
	{
		if (this.light != null && this.lightLOD != null)
		{
			this.lightLOD.SetRange(_newRange);
			if (this.rangeGizmo != null)
			{
				this.rangeGizmo.newRadius = _newRange;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetIntensity(float _newIntensity)
	{
		if (this.lightLOD != null)
		{
			this.lightLOD.MaxIntensity = _newIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAngle(float _newAngle)
	{
		if (this.light != null)
		{
			this.light.spotAngle = _newAngle;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRate(float _newRate)
	{
		if (this.lightLOD != null)
		{
			this.lightLOD.StateRate = _newRate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetState(LightStateType _newState)
	{
		if (this.lightLOD != null)
		{
			this.lightLOD.LightStateType = _newState;
			this.panelRate.IsVisible = _newState == LightStateType.Pulsing || _newState == LightStateType.Blinking;
			if (this.panelRate.IsVisible)
			{
				this.cbxRate.Value = (double)this.lightLOD.StateRate;
			}
			this.panelDelay.IsVisible = _newState == LightStateType.Fluctuating;
			if (this.panelDelay.IsVisible)
			{
				this.cbxDelay.Value = (double)this.lightLOD.FluxDelay;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetShadow(LightShadows _newShadow)
	{
		if (this.light != null)
		{
			this.light.shadows = _newShadow;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDelay(float _newDelay)
	{
		if (this.lightLOD != null)
		{
			this.lightLOD.FluxDelay = _newDelay;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRGB(Color32 color)
	{
		this.txtColorR.Text = color.r.ToString();
		this.txtColorG.Text = color.g.ToString();
		this.txtColorB.Text = color.b.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetHex(Color32 color)
	{
		string text = ((color.r < 16) ? ("0" + color.r.ToString("X")) : color.r.ToString("X"));
		string text2 = ((color.g < 16) ? ("0" + color.g.ToString("X")) : color.g.ToString("X"));
		string text3 = ((color.b < 16) ? ("0" + color.b.ToString("X")) : color.b.ToString("X"));
		this.txtHex.Text = text + text2 + text3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLightFromValues(XUiC_LightEditor.LightValues values)
	{
		if (this.light && this.lightLOD)
		{
			this.light.type = values.type;
			this.light.shadows = values.shadows;
			this.lightLOD.SetRange(values.range);
			this.lightLOD.MaxIntensity = values.intensity;
			this.light.color = values.color;
			this.light.spotAngle = values.spotAngle;
			this.SetEmissiveColor(values.emissiveColor);
			this.lightLOD.LightStateType = values.stateType;
			this.lightLOD.StateRate = values.stateRate;
			this.lightLOD.FluxDelay = values.stateDelay;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetEmissiveColor(Color _color)
	{
		if (this.lightLOD)
		{
			this.lightLOD.SetEmissiveColor(_color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ColorPicker_OnSelectedColorChanged(Color color)
	{
		if (this.light != null)
		{
			this.light.color = this.colorPicker.SelectedColor;
			this.cbxColorPresetList.Value = XUiC_LightEditor.ColorPresets.Keys.First<string>();
			this.SetRGB(this.light.color);
			this.SetHex(this.light.color);
		}
		this.SetEmissiveColor(color);
	}

	public override void OnClose()
	{
		Log.Out("CLOSE " + this.startValues.stateType.ToString());
		this.SetLightFromValues(this.startValues);
		base.OnClose();
		if (this.rangeGizmo != null)
		{
			this.rangeGizmo.KillWF();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.isOpen = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Color> ColorPresets = new Dictionary<string, Color>
	{
		{
			Localization.Get("xuiLightPropColorPresetCustom", false, null),
			new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetCandle", false, null),
			new Color32(byte.MaxValue, 147, 41, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColor40WattWhite", false, null),
			new Color32(byte.MaxValue, 197, 143, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPreset100WattWhite", false, null),
			new Color32(byte.MaxValue, 214, 170, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHalogen", false, null),
			new Color32(byte.MaxValue, 241, 224, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetCarbonArc", false, null),
			new Color32(byte.MaxValue, 250, 244, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHighNoonSun", false, null),
			new Color32(byte.MaxValue, byte.MaxValue, 251, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetFluorescent", false, null),
			new Color32(244, byte.MaxValue, 250, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetWarmFluorescent", false, null),
			new Color32(byte.MaxValue, 244, 229, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetCoolFluorescent", false, null),
			new Color32(212, 235, byte.MaxValue, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetFullSpectrum", false, null),
			new Color32(byte.MaxValue, 244, 242, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetMercury", false, null),
			new Color32(216, 247, byte.MaxValue, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetSodium", false, null),
			new Color32(byte.MaxValue, 209, 178, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHighPressureSodium", false, null),
			new Color32(byte.MaxValue, 183, 76, byte.MaxValue)
		},
		{
			Localization.Get("xuiLightPropColorPresetHalide", false, null),
			new Color32(242, 252, byte.MaxValue, byte.MaxValue)
		}
	};

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<LightType> cbxLightType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> cbxColorPresetList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<LightShadows> cbxLightShadow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<LightStateType> cbxLightState;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxRate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat cbxDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ColorPicker colorPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView panelAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView panelRate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView panelDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPaste;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRestore;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtColorR;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtColorG;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtColorB;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtHex;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityLight tileEntityLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldChangePreset = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LightEditor.LightValues startValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_LightEditor.LightValues defaultValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool hasCopiedValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_LightEditor.LightValues copyValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string copyColorPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public WireFrameSphere rangeGizmo;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockLight block;

	public LightLOD lightLOD;

	[PublicizedFrom(EAccessModifier.Private)]
	public Light light;

	public struct LightValues
	{
		public bool IsEqual(XUiC_LightEditor.LightValues other)
		{
			return this.type == other.type && this.shadows == other.shadows && this.range == other.range && this.intensity == other.intensity && !(this.color != other.color) && this.spotAngle == other.spotAngle && !(this.emissiveColor != other.emissiveColor) && this.stateType == other.stateType && this.stateRate == other.stateRate && this.stateDelay == other.stateDelay;
		}

		public LightType type;

		public LightShadows shadows;

		public float range;

		public float intensity;

		public Color color;

		public float spotAngle;

		public Color emissiveColor;

		public LightStateType stateType;

		public float stateRate;

		public float stateDelay;
	}
}
