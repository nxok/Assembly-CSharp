using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignInfoWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.signMaterial = base.GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		this.signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xuiV_Texture = this.signMaterial;
		xuiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.OnWillRender));
		this.signMaterial.Texture = Texture2D.whiteTexture;
		this.btnEdit = (XUiC_SimpleButton)base.GetChildById("btnEdit");
		this.btnDuplicate = (XUiC_SimpleButton)base.GetChildById("btnDuplicate");
		this.btnDelete = (XUiC_SimpleButton)base.GetChildById("btnDelete");
		this.btnExport = (XUiC_SimpleButton)base.GetChildById("btnExport");
		this.btnEdit.OnPressed += this.BtnEdit_OnPressed;
		this.btnDuplicate.OnPressed += delegate(XUiController _, int _)
		{
			this.DuplicateSign();
		};
		this.btnDelete.OnPressed += delegate(XUiController _, int _)
		{
			this.DeleteSign();
		};
		this.btnExport.OnPressed += this.BtnExport_OnPressed;
		this.confirmationPrompt = base.GetChildById("confirmation_prompt_controller") as XUiC_ConfirmationPrompt;
		this.galleryWindow = this.windowGroup.Controller.GetChildByType<XUiC_SignGalleryWindow>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExport_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.signId.IsValid || this.signData == null)
		{
			return;
		}
		string text = SignTextureExporter.Instance.ExportSignToTexture(this.signData.name, this.signId);
		Log.Out("Sign png saved to: " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEdit_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.galleryWindow.targetEntity != null)
		{
			XUiC_SignEditorWindow.Open(this.xui.playerUI, this.galleryWindow.targetEntity);
			return;
		}
		XUiC_SignEditorWindow.Open(this.xui.playerUI, this.signData, this.signId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DuplicateSign()
	{
		if (!PrefabEditModeManager.Instance.IsActive())
		{
			Log.Error("Duplicate function can only be used in the Prefab Editor.");
			return;
		}
		string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
		SignData signData = SignData.Duplicate(this.signData, SignDataManager.Instance.GetDuplicateName(fileNameNoExtension, this.signData.name) ?? "");
		GlobalSignId globalSignId = SignDataManager.Instance.AddSignToLibrary(fileNameNoExtension, signData);
		PrefabEditModeManager.Instance.NeedsSaving = true;
		this.galleryWindow.RefreshAndSelect(globalSignId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeleteSign()
	{
		if (!PrefabEditModeManager.Instance.IsActive())
		{
			Log.Error("Delete function can only be used in the Prefab Editor.");
			return;
		}
		this.confirmationPrompt.ShowPrompt(Localization.Get("lblSignDeleteTitle", false, null), Localization.Get("lblSignDeleteMessage", false, null), Localization.Get("xuiCancel", false, null), Localization.Get("xuiConfirm", false, null), new Action<XUiC_ConfirmationPrompt.Result>(this.OnDeletionPromptResult));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeletionPromptResult(XUiC_ConfirmationPrompt.Result result)
	{
		if (result != XUiC_ConfirmationPrompt.Result.Cancelled && result == XUiC_ConfirmationPrompt.Result.Confirmed)
		{
			if (!SignDataManager.Instance.TryDeleteSign(this.signId))
			{
				Log.Error(string.Format("Failed to delete sign with ID: {0}", this.signId));
				return;
			}
			PrefabEditModeManager.Instance.NeedsSaving = true;
			this.galleryWindow.RefreshAndSelect(SignData.defaultSignDataID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		SignDataManager.Instance.TryApplyRenderingData(this.signId, 1f, mat, null, SignUIStyle.Standard);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.UpdateInputs();
		if (this.IsDirty && base.ViewComponent.IsVisible)
		{
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInputs()
	{
		if (this.confirmationPrompt.IsVisible || this.signData == null)
		{
			return;
		}
		if (InputUtils.ControlKeyPressed && Input.GetKeyDown(100))
		{
			this.DuplicateSign();
		}
		if (Input.GetKeyDown(127))
		{
			this.DeleteSign();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "signname")
		{
			if (!this.signId.IsValid)
			{
				value = Localization.Get("lblSignNone", false, null);
			}
			else if (this.signData == null)
			{
				value = string.Format(Localization.Get("lblSignMissing", false, null), this.signId.signGuid.ToString());
			}
			else
			{
				value = this.signData.name;
			}
			return true;
		}
		if (bindingName == "libraryname")
		{
			value = SignDataManager.GetLibraryNiceName(this.signId);
			return true;
		}
		if (!(bindingName == "lastmodified"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = ((this.signId.IsValid && this.signData != null) ? this.signData.lastModified.ToString("g") : "-");
		return true;
	}

	public void SetSignInfo(GlobalSignId signId)
	{
		SignData signData;
		bool flag = SignDataManager.Instance.TryGetSignData(signId, out signData);
		this.signId = signId;
		this.signData = signData;
		this.signMaterial.IsVisible = signId.IsValid && flag;
		bool flag2 = signId.IsValid && flag && signId.libraryId != "[D]";
		this.btnEdit.Enabled = flag2;
		this.btnDelete.Enabled = flag2;
		this.btnDuplicate.Enabled = flag;
		this.btnExport.Enabled = flag;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId signId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData signData;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnEdit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDuplicate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDelete;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnExport;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignGalleryWindow galleryWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ConfirmationPrompt confirmationPrompt;
}
