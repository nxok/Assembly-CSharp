using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveManagementPrompt : XUiController
{
	[XuiXmlBinding("showStorageType")]
	public bool ShowStorageType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("canChangeStorageType")]
	public bool CanChangeStorageType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("applyValid")]
	public bool ActionIsValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("canCopy")]
	public bool CanCopy
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !this.isRemoteSave;
		}
	}

	[XuiXmlBinding("showNameInput")]
	public bool ShowNameInput
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !this.isRemoteSave;
		}
	}

	[XuiXmlBinding("showSaveDataLimit")]
	public bool ShowSaveDataLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SaveInfoProvider.WorldEntryInfo worldEntryInfo = this.worldInfo;
			return worldEntryInfo != null && worldEntryInfo.WorldSize != null;
		}
	}

	[XuiXmlBinding("showIsArchived")]
	public bool ShowIsArchived
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsSupported;
		}
	}

	[XuiXmlBinding("canChangeIsArchived")]
	public bool CanChangeIsArchived
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<UserDataStorageType> xuiC_ComboBoxEnum = this.cbxStorageType;
			return xuiC_ComboBoxEnum != null && xuiC_ComboBoxEnum.Value.UsesDataLimit();
		}
	}

	[XuiXmlBinding("confirmTextKey")]
	public string ConfirmTextKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_SaveManagementPrompt.SaveManagementMode saveManagementMode = this.mode;
			string text;
			if (saveManagementMode != XUiC_SaveManagementPrompt.SaveManagementMode.Copy)
			{
				if (saveManagementMode != XUiC_SaveManagementPrompt.SaveManagementMode.Manage)
				{
					throw new Exception("Unhandled SaveManagementMode");
				}
				text = (this.isMove ? "xuiDmMoveSave" : "btnApply");
			}
			else
			{
				text = "xuiDmCopySave";
			}
			return text;
		}
	}

	[XuiXmlBinding("titleTextKey")]
	public string TitleTextKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_SaveManagementPrompt.SaveManagementMode saveManagementMode = this.mode;
			string text;
			if (saveManagementMode != XUiC_SaveManagementPrompt.SaveManagementMode.Copy)
			{
				if (saveManagementMode != XUiC_SaveManagementPrompt.SaveManagementMode.Manage)
				{
					throw new Exception("Unhandled SaveManagementMode");
				}
				text = "xuiDmManageSave";
			}
			else
			{
				text = "xuiDmCopySave";
			}
			return text;
		}
	}

	public event XUiC_SaveManagementPrompt.SaveManagementActionCopy OnCopyConfirmed;

	public event XUiC_SaveManagementPrompt.SaveManagementActionMove OnMoveConfirmed;

	public event XUiC_SaveManagementPrompt.SaveManagementActionApply OnApplyConfirmed;

	public override void Init()
	{
		base.Init();
		this.txtSaveName.UIInput.onValidate = new UIInput.OnValidate(GameUtils.ValidateGameNameInput);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.dataManagementBar == null)
		{
			return;
		}
		this.dataManagementBar.SetDeleteHovered(false);
		this.dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
		this.dataManagementBar.SetPendingBytes(0L);
		this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanLinks()
	{
		this.saveInfo = null;
		this.worldInfo = null;
		this.dataManagementBar = null;
		this.OnCopyConfirmed = null;
		this.OnMoveConfirmed = null;
		this.OnApplyConfirmed = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false) && this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames(new Action(this.Cancel), 1);
		}
		base.handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void configureSaveDataLimitAllowedValues(UserDataStorageType _storageType)
	{
		SaveDataLimitType value = this.cbxSaveDataLimit.Value;
		this.cbxSaveDataLimit.Min = null;
		if (_storageType.UsesDataLimit())
		{
			if (value == SaveDataLimitType.Unlimited)
			{
				this.cbxSaveDataLimit.Value = SaveDataLimitType.VeryLong;
			}
			this.cbxSaveDataLimit.Min = new SaveDataLimitType?(SaveDataLimitType.Short);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateSizeLimit(out long _pendingSaveBytes)
	{
		if (this.worldInfo.WorldSize == null)
		{
			_pendingSaveBytes = Math.Max(this.saveInfo.SizeInfo.BytesOnDisk, this.saveInfo.SizeInfo.BytesReserved);
			return true;
		}
		SaveDataLimitType value = this.cbxSaveDataLimit.Value;
		if (value == SaveDataLimitType.Unlimited)
		{
			_pendingSaveBytes = this.saveInfo.SizeInfo.BytesOnDisk;
			return true;
		}
		long num = value.CalculateTotalSize(this.worldInfo.WorldSize.Value);
		if (this.saveInfo.SizeInfo.BytesOnDisk > num)
		{
			_pendingSaveBytes = this.saveInfo.SizeInfo.BytesOnDisk;
			return false;
		}
		_pendingSaveBytes = num;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validate()
	{
		long num;
		bool flag = this.validateSizeLimit(out num);
		long num2 = (this.cbxIsArchived.Value ? this.saveInfo.SizeInfo.BytesOnDisk : num);
		if (this.worldInfo.ShouldBeMovedWithSave(this.cbxStorageType.Value))
		{
			num2 += this.worldInfo.WorldDataSize;
		}
		bool flag2 = !this.cbxStorageType.Value.UsesDataLimit() || num2 <= SaveInfoProvider.Instance.TotalAvailableBytes;
		this.cbxSaveDataLimit.TextColor = (flag ? Color.white : Color.red);
		bool flag3 = this.isRemoteSave || this.validateNameAvailable(this.txtSaveName.Text, this.cbxStorageType.Value);
		bool flag4 = !this.isRemoteSave && !string.Equals(this.txtSaveName.Text, this.saveInfo.Name, StringComparison.Ordinal);
		bool flag5 = this.cbxStorageType.Value != this.saveInfo.StorageType;
		this.isMove = flag4 || flag5;
		bool flag6 = flag3 || (this.mode == XUiC_SaveManagementPrompt.SaveManagementMode.Manage && !this.isMove);
		this.txtSaveName.ActiveTextColor = (flag6 ? Color.white : Color.red);
		this.ActionIsValid = flag && flag2 && flag6;
		this.updateDataBarPreview(num2, flag5);
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDataBarPreview(long _pendingBytes, bool _storageChanged)
	{
		if (this.dataManagementBar == null)
		{
			return;
		}
		bool flag = this.cbxStorageType.Value.UsesDataLimit();
		bool flag2 = this.saveInfo.StorageType.UsesDataLimit();
		if (flag && (this.mode == XUiC_SaveManagementPrompt.SaveManagementMode.Copy || !flag2))
		{
			this.dataManagementBar.SetDeleteHovered(false);
			this.dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
			this.dataManagementBar.SetPendingBytes(_pendingBytes);
			return;
		}
		if (this.mode == XUiC_SaveManagementPrompt.SaveManagementMode.Manage && flag2 && _storageChanged && !flag)
		{
			this.dataManagementBar.SetPendingBytes(0L);
			this.dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			this.dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
			this.dataManagementBar.SetDeleteHovered(true);
			return;
		}
		if (!flag || !flag2 || _storageChanged)
		{
			this.dataManagementBar.SetDeleteHovered(false);
			this.dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			this.dataManagementBar.SetPendingBytes(0L);
			this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			return;
		}
		long num = _pendingBytes - this.saveInfo.SizeInfo.ReportedSize;
		this.dataManagementBar.SetDeleteHovered(false);
		if (num < 0L)
		{
			long num2 = this.saveInfo.BarStartOffset + _pendingBytes;
			XUiC_DataManagementBar.BarRegion barRegion = new XUiC_DataManagementBar.BarRegion(num2, -num);
			this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			this.dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Secondary);
			this.dataManagementBar.SetArchivePreviewRegion(barRegion);
			return;
		}
		if (num > 0L)
		{
			this.dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
			this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
			this.dataManagementBar.SetPendingBytes(num);
			return;
		}
		this.dataManagementBar.SetArchivePreviewRegion(XUiC_DataManagementBar.BarRegion.None);
		this.dataManagementBar.SetPendingBytes(0L);
		this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateNameAvailable(string _name, UserDataStorageType _storageType)
	{
		return !string.IsNullOrEmpty(_name) && !SdDirectory.Exists(GameIO.GetSaveGameDir(this.worldInfo.Name, _name, _storageType));
	}

	[XuiBindEvent("OnChangeHandler", "txtSaveName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSaveName_OnChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.validate();
	}

	[XuiBindEvent("OnValueChanged", "cbxStorageType")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxStorageType_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		this.configureSaveDataLimitAllowedValues(_newValue);
		this.cbxIsArchived.Value = _newValue.UsesDataLimit() && this.cbxIsArchived.Value;
		this.validate();
	}

	[XuiBindEvent("OnValueChanged", "cbxIsArchived")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxIsArchived_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		this.validate();
	}

	[XuiBindEvent("OnValueChanged", "cbxSaveDataLimit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxSaveDataLimit_OnValueChanged(XUiController _sender, SaveDataLimitType _oldValue, SaveDataLimitType _newValue)
	{
		this.validate();
	}

	public void Cancel()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.cleanLinks();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.Cancel();
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		bool value = this.cbxIsArchived.Value;
		long num = ((this.worldInfo.WorldSize != null) ? this.cbxSaveDataLimit.Value.CalculateTotalSize(this.worldInfo.WorldSize.Value) : this.saveInfo.SizeInfo.BytesReserved);
		XUiC_SaveManagementPrompt.SaveManagementMode saveManagementMode = this.mode;
		if (saveManagementMode != XUiC_SaveManagementPrompt.SaveManagementMode.Copy)
		{
			if (saveManagementMode == XUiC_SaveManagementPrompt.SaveManagementMode.Manage)
			{
				if (this.isMove)
				{
					this.performMove(this.txtSaveName.Text, this.cbxStorageType.Value, value, num);
				}
				else
				{
					this.performApply(value, num);
				}
			}
		}
		else
		{
			this.performCopy(this.txtSaveName.Text, this.cbxStorageType.Value, value, num);
		}
		this.cleanLinks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performCopy(string _targetName, UserDataStorageType _storageType, bool _isArchived, long _reservedBytes)
	{
		UserDataManagement.GameSaveCopy gameSaveCopy;
		try
		{
			gameSaveCopy = new UserDataManagement.GameSaveCopy(this.saveInfo, _targetName, _storageType);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to create save copy operation: " + ex.Message);
			return;
		}
		UserDataManagement.SaveCopyResultInfo saveCopyResultInfo = gameSaveCopy.PerformCopy();
		if (saveCopyResultInfo.Result == UserDataManagement.Result.Success)
		{
			UserDataManagement.SetSaveArchived(saveCopyResultInfo.NewSaveDir, _isArchived);
			if (!this.isRemoteSave)
			{
				UserDataManagement.SetSaveDataLimit(saveCopyResultInfo.NewSaveDir, _reservedBytes);
			}
		}
		UserDataStorageType userDataStorageType = (gameSaveCopy.WorldRequiresMoving ? _storageType : this.worldInfo.Location.StorageType);
		XUiC_SaveManagementPrompt.SaveManagementActionCopy onCopyConfirmed = this.OnCopyConfirmed;
		if (onCopyConfirmed == null)
		{
			return;
		}
		onCopyConfirmed(saveCopyResultInfo.Result, _targetName, this.worldInfo.Name, userDataStorageType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performMove(string _targetName, UserDataStorageType _storageType, bool _isArchived, long _reservedBytes)
	{
		UserDataManagement.GameSaveMove gameSaveMove;
		try
		{
			gameSaveMove = new UserDataManagement.GameSaveMove(this.saveInfo, _targetName, _storageType);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to create save move operation: " + ex.Message);
			return;
		}
		UserDataManagement.SaveMoveResultInfo saveMoveResultInfo = gameSaveMove.PerformMove();
		if (saveMoveResultInfo.Result == UserDataManagement.Result.Success)
		{
			UserDataManagement.SetSaveArchived(saveMoveResultInfo.NewSaveDir, _isArchived);
			if (!this.isRemoteSave)
			{
				UserDataManagement.SetSaveDataLimit(saveMoveResultInfo.NewSaveDir, _reservedBytes);
			}
		}
		UserDataStorageType userDataStorageType = (gameSaveMove.WorldRequiresMoving ? _storageType : this.worldInfo.Location.StorageType);
		XUiC_SaveManagementPrompt.SaveManagementActionMove onMoveConfirmed = this.OnMoveConfirmed;
		if (onMoveConfirmed == null)
		{
			return;
		}
		onMoveConfirmed(saveMoveResultInfo.Result, _targetName, this.worldInfo.Name, userDataStorageType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performApply(bool _isArchived, long _reservedBytes)
	{
		UserDataManagement.SetSaveArchived(this.saveInfo.SaveDir, _isArchived);
		if (!this.isRemoteSave)
		{
			UserDataManagement.SetSaveDataLimit(this.saveInfo.SaveDir, _reservedBytes);
		}
		SaveInfoProvider.Instance.SetDirty();
		XUiC_SaveManagementPrompt.SaveManagementActionApply onApplyConfirmed = this.OnApplyConfirmed;
		if (onApplyConfirmed == null)
		{
			return;
		}
		onApplyConfirmed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setValues(XUiC_SaveManagementPrompt.SaveManagementMode _mode, SaveInfoProvider.SaveEntryInfo _saveInfo, SaveInfoProvider.WorldEntryInfo _worldInfo)
	{
		this.mode = _mode;
		this.saveInfo = _saveInfo;
		this.worldInfo = _worldInfo;
		this.isRemoteSave = _worldInfo.Type == SaveInfoProvider.RemoteWorldsType;
		this.ShowStorageType = PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
		this.CanChangeStorageType = true;
		this.txtSaveName.Text = ((_mode == XUiC_SaveManagementPrompt.SaveManagementMode.Copy) ? (_saveInfo.Name + " - Copy") : (_saveInfo.Name ?? ""));
		this.cbxIsArchived.Value = _saveInfo.StorageType.UsesDataLimit() && _saveInfo.SizeInfo.IsArchived;
		this.cbxSaveDataLimit.Min = null;
		if (_worldInfo.WorldSize != null)
		{
			SaveDataLimitType saveDataLimitType;
			if (!_saveInfo.StorageType.UsesDataLimit() && _saveInfo.SizeInfo.BytesReserved <= 0L)
			{
				this.cbxSaveDataLimit.Value = SaveDataLimitType.Unlimited;
			}
			else if (SaveDataLimitExtensions.TryCalculateSaveDataLimitType(_saveInfo.SizeInfo.BytesReserved, _worldInfo.WorldSize.Value, out saveDataLimitType))
			{
				this.cbxSaveDataLimit.Value = saveDataLimitType;
			}
			else
			{
				this.cbxSaveDataLimit.Value = SaveDataLimitType.VeryLong;
			}
			this.cbxStorageType.Value = _saveInfo.StorageType;
		}
		else if (_saveInfo.SizeInfo.BytesReserved > 0L)
		{
			this.cbxStorageType.Value = _saveInfo.StorageType;
			this.CanChangeStorageType = true;
		}
		else
		{
			this.cbxStorageType.Value = UserDataStorageType.DeviceLocal;
			this.CanChangeStorageType = false;
		}
		this.configureSaveDataLimitAllowedValues(this.cbxStorageType.Value);
		this.validate();
	}

	public static void Show(XUi _xui, XUiC_DataManagementBar _dataManagementBar, XUiC_SaveManagementPrompt.SaveManagementMode _mode, SaveInfoProvider.SaveEntryInfo _saveInfo, SaveInfoProvider.WorldEntryInfo _worldInfo, XUiC_SaveManagementPrompt.SaveManagementActionCopy _onCopied, XUiC_SaveManagementPrompt.SaveManagementActionMove _onMoved, XUiC_SaveManagementPrompt.SaveManagementActionApply _onApplied)
	{
		XUiC_SaveManagementPrompt childByType = _xui.GetChildByType<XUiC_SaveManagementPrompt>();
		childByType.dataManagementBar = _dataManagementBar;
		childByType.OnCopyConfirmed = _onCopied;
		childByType.OnMoveConfirmed = _onMoved;
		childByType.OnApplyConfirmed = _onApplied;
		childByType.setValues(_mode, _saveInfo, _worldInfo);
		_xui.playerUI.windowManager.Open(childByType.windowGroup, false);
	}

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent("txtSaveName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtSaveName;

	[XuiBindComponent("cbxStorageType", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxStorageType;

	[XuiBindComponent("cbxIsArchived", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool cbxIsArchived;

	[XuiBindComponent("cbxSaveDataLimit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SaveManagementPrompt.SaveManagementMode mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRemoteSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMove;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.SaveEntryInfo saveInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.WorldEntryInfo worldInfo;

	public enum SaveManagementMode
	{
		Copy,
		Manage
	}

	public delegate void SaveManagementActionCopy(UserDataManagement.Result _result, string _targetSaveName, string _worldName, UserDataStorageType _expectedWorldStorage);

	public delegate void SaveManagementActionMove(UserDataManagement.Result _result, string _targetSaveName, string _worldName, UserDataStorageType _expectedWorldStorage);

	public delegate void SaveManagementActionApply();
}
