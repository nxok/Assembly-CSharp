using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.lblName = (XUiV_Label)base.GetChildById("characterName").ViewComponent;
		this.textPreview = (XUiV_Texture)base.GetChildById("playerPreviewSDCS").ViewComponent;
		this.IsDirty = true;
		XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		this.xui.playerUI.OnUIShutdown += this.HandleUIShutdown;
		this.xui.OnShutdown += this.HandleUIShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleUIShutdown()
	{
		this.xui.playerUI.OnUIShutdown -= this.HandleUIShutdown;
		this.xui.OnShutdown -= this.HandleUIShutdown;
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnHover(XUiController _sender, bool _isOver)
	{
		this.isMouseOverPreview = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!this.xui.DragAndDropWindow.CurrentStack.IsEmpty())
		{
			ItemStack itemStack = this.xui.PlayerEquipment.EquipItem(this.xui.DragAndDropWindow.CurrentStack);
			if (this.xui.DragAndDropWindow.CurrentStack != itemStack)
			{
				this.xui.DragAndDropWindow.CurrentStack = itemStack;
				this.xui.DragAndDropWindow.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		if (this.ep == null)
		{
			this.ep = this.xui.playerUI.entityPlayer;
		}
		if (this.IsDirty)
		{
			if (this.player == null)
			{
				return;
			}
			this.lblName.Text = this.player.PlayerDisplayName;
			this.IsDirty = false;
			base.RefreshBindings();
		}
		if (this.isPreviewDirty)
		{
			this.MakePreview();
		}
		this.textPreview.Texture = this.renderTextureSystem.RenderTex;
		if (this.characterPivot != null)
		{
			XUi xui = this.xui;
			PlayerActionsGUI playerActionsGUI;
			if (xui == null)
			{
				playerActionsGUI = null;
			}
			else
			{
				LocalPlayerUI playerUI = xui.playerUI;
				if (playerUI == null)
				{
					playerActionsGUI = null;
				}
				else
				{
					PlayerActionsLocal playerInput = playerUI.playerInput;
					playerActionsGUI = ((playerInput != null) ? playerInput.GUIActions : null);
				}
			}
			PlayerActionsGUI playerActionsGUI2 = playerActionsGUI;
			if (playerActionsGUI2 != null)
			{
				bool flag = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard;
				this.xui.playerUI.CursorController.GetMouseButton(0);
				bool flag2 = false;
				if (flag)
				{
					flag2 = true;
					float x = playerActionsGUI2.Camera.Value.x;
					if (Mathf.Abs(x) > 0.15f)
					{
						float num = -1f;
						this.characterPivot.Rotate(Vector3.up, num * x * 200f * _dt, 0);
					}
				}
				if (this.cursorLockActive)
				{
					if (flag2)
					{
						this.MaintainCursorLock();
					}
					else
					{
						this.EndCursorLock();
					}
				}
			}
			bool mouseButton = this.xui.playerUI.CursorController.GetMouseButton(0);
			bool mouseButton2 = this.xui.playerUI.CursorController.GetMouseButton(1);
			if (mouseButton || mouseButton2)
			{
				if (!this.isDragging && this.isMouseOverPreview)
				{
					this.isDragging = true;
					this.lastMousePos = Input.mousePosition;
				}
				if (this.isDragging)
				{
					Vector2 vector = Input.mousePosition;
					Vector2 vector2 = vector - this.lastMousePos;
					this.lastMousePos = vector;
					if (Mathf.Abs(vector2.x) > 0.01f)
					{
						float num2 = vector2.x * 0.4f;
						this.characterPivot.Rotate(Vector3.up, -num2, 0);
					}
				}
			}
			else
			{
				this.isDragging = false;
			}
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
		this.isPreviewDirty = true;
		this.player = this.xui.playerUI.entityPlayer;
		this.previewFrame = base.GetChildById("previewFrameSDCS");
		if (this.previewFrame != null)
		{
			this.previewFrame.OnPress += this.PreviewFrame_OnPress;
			this.previewFrame.OnHover += this.PreviewFrame_OnHover;
		}
		this.textPreview = (XUiV_Texture)base.GetChildById("playerPreviewSDCS").ViewComponent;
		if (this.renderTextureSystem.ParentGO == null)
		{
			this.renderTextureSystem.Create("playerpreview", new GameObject(), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), this.textPreview.Size, true, false, 1f);
		}
		this.renderCamera = this.renderTextureSystem.CameraGO.GetComponent<Camera>();
		this.renderCamera.transform.localPosition = new Vector3(0f, 1.5f, 0f);
		this.renderCamera.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);
		this.renderCamera.orthographic = false;
		this.renderCamera.fieldOfView = 54f;
		this.renderCamera.renderingPath = 3;
		if (this.characterPivot == null)
		{
			GameObject gameObject = new GameObject("CharacterPivot");
			this.characterPivot = gameObject.transform;
			this.characterPivot.SetParent(this.renderTextureSystem.ParentGO.transform, false);
			this.characterPivot.localPosition = new Vector3(0f, 0f, 2.15f);
			this.characterPivot.localRotation = Quaternion.AngleAxis(-30f, Vector3.up);
		}
		this.renderTextureSystem.LightGO.GetComponent<Light>().enabled = false;
		GameObject gameObject2 = new GameObject("Key Light", new Type[] { typeof(Light) });
		gameObject2.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject2.transform.SetPositionAndRotation(new Vector3(1.5f, 2.5f, -1.5f), Quaternion.Euler(20f, -20f, 0f));
		Light component = gameObject2.GetComponent<Light>();
		component.color = new Color(0.9f, 0.8f, 0.7f, 1f);
		component.type = 0;
		component.range = 20f;
		component.spotAngle = 60f;
		component.intensity = 1.5f;
		component.shadows = 1;
		component.shadowStrength = 0.2f;
		component.shadowBias = 0.005f;
		component.cullingMask = 2048;
		GameObject gameObject3 = new GameObject("Fill Light", new Type[] { typeof(Light) });
		gameObject3.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject3.transform.SetPositionAndRotation(new Vector3(-2f, 3f, 0f), Quaternion.Euler(35f, 45f, 0f));
		Light component2 = gameObject3.GetComponent<Light>();
		component2.color = new Color(1f, 1f, 1f, 1f);
		component2.type = 0;
		component2.range = 20f;
		component2.spotAngle = 60f;
		component2.intensity = 0.5f;
		component2.shadows = 1;
		component2.shadowStrength = 0.2f;
		component2.shadowBias = 0.005f;
		component2.cullingMask = 2048;
		GameObject gameObject4 = new GameObject("Fill 2 Light", new Type[] { typeof(Light) });
		gameObject4.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject4.transform.SetPositionAndRotation(new Vector3(0f, -0.5f, -1.5f), Quaternion.Euler(-20f, 0f, 0f));
		Light component3 = gameObject4.GetComponent<Light>();
		component3.color = new Color(1f, 1f, 1f, 1f);
		component3.type = 0;
		component3.range = 20f;
		component3.spotAngle = 60f;
		component3.intensity = 0.5f;
		component3.shadows = 1;
		component3.shadowStrength = 0.2f;
		component3.shadowBias = 0.005f;
		component3.cullingMask = 2048;
		GameObject gameObject5 = new GameObject("Back Light", new Type[] { typeof(Light) });
		gameObject5.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject5.transform.SetPositionAndRotation(new Vector3(-2f, 5f, 2f), Quaternion.Euler(60f, 105f, 0f));
		Light component4 = gameObject5.GetComponent<Light>();
		component4.color = new Color(0.4f, 0.75f, 1f, 1f);
		component4.type = 0;
		component4.spotAngle = 60f;
		component4.range = 20f;
		component4.intensity = 1.5f;
		component4.shadows = 1;
		component4.shadowStrength = 0.2f;
		component4.shadowBias = 0.005f;
		component4.cullingMask = 2048;
		if (this.player as EntityPlayerLocal != null && this.player.emodel as EModelSDCS != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment1;
		}
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment1(XUiM_PlayerEquipment playerEquipment)
	{
		if (!base.IsOpen)
		{
			return;
		}
		this.MakePreview();
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment1;
		SDCSUtils.UnloadViz(this.previewSDCSObj);
		this.renderTextureSystem.Cleanup();
		this.EndCursorLock();
		this.characterPivot = null;
		this.isMouseOverPreview = false;
		this.isDragging = false;
		if (this.previewFrame != null)
		{
			this.previewFrame.OnPress -= this.PreviewFrame_OnPress;
			this.previewFrame.OnHover -= this.PreviewFrame_OnHover;
			this.previewFrame = null;
		}
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
	}

	public void MakePreview()
	{
		if (this.ep == null)
		{
			return;
		}
		if (this.ep.emodel == null)
		{
			return;
		}
		EModelSDCS emodelSDCS = this.ep.emodel as EModelSDCS;
		if (emodelSDCS != null)
		{
			this.isPreviewDirty = false;
			SDCSUtils.CreateVizUI(emodelSDCS.Archetype, ref this.previewSDCSObj, ref this.transformCatalog, this.ep, true);
			Utils.SetLayerRecursively(this.previewSDCSObj, 11);
			Transform transform = this.previewSDCSObj.transform;
			transform.SetParent((this.characterPivot != null) ? this.characterPivot : this.renderTextureSystem.ParentGO.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			CharacterGazeController componentInChildren = this.previewSDCSObj.GetComponentInChildren<CharacterGazeController>();
			if (componentInChildren && this.renderCamera != null)
			{
				componentInChildren.LookAtTransformOverride = this.renderCamera.transform;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BeginCursorLock()
	{
		if (this.cursorLockActive)
		{
			return;
		}
		this.cursorLockActive = true;
		this.lockedCursorPos = this.xui.playerUI.CursorController.GetScreenPosition();
		this.prevCursorHidden = this.xui.playerUI.CursorController.VirtualCursorHidden;
		this.xui.playerUI.CursorController.VirtualCursorHidden = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MaintainCursorLock()
	{
		this.xui.playerUI.CursorController.SetScreenPosition(this.lockedCursorPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EndCursorLock()
	{
		if (!this.cursorLockActive)
		{
			return;
		}
		this.cursorLockActive = false;
		this.xui.playerUI.CursorController.VirtualCursorHidden = this.prevCursorHidden;
	}

	[XuiBindEvent("OnPressed", "btnClearAll")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnClearAll_OnPress(XUiController _sender, int _mouseButton)
	{
		Equipment equipment = this.xui.PlayerEquipment.Equipment;
		equipment.ClearTempCosmeticSlot();
		equipment.ClearCosmeticSlots();
		XUiC_CharacterCosmeticsListWindow xuiC_CharacterCosmeticsListWindow;
		if (base.WindowGroup.Controller.TryGetChildController<XUiC_CharacterCosmeticsListWindow>("", out xuiC_CharacterCosmeticsListWindow))
		{
			xuiC_CharacterCosmeticsListWindow.SetEmptyCosmeticForCurrentCategory();
		}
		((XUiC_CharacterCosmeticWindowGroup)base.WindowGroup.Controller).ResetPreview();
		EModelSDCS emodelSDCS = this.xui.playerUI.entityPlayer.emodel as EModelSDCS;
		if (emodelSDCS != null)
		{
			emodelSDCS.GenerateMeshes();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[XuiBindComponent("btnClearAll", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SimpleButton btnClearAll;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController previewFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer ep;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera cam;

	public RuntimeAnimatorController animationController;

	public float atlasResolutionScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RenderTextureSystem renderTextureSystem = new RenderTextureSystem();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPreviewDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform characterPivot;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float DragRotateSpeed = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ControllerRotateSpeed = 200f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMouseOverPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 lastMousePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RotationStickDeadzone = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const bool InvertRotationStick = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cursorLockActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 lockedCursorPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool prevCursorHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewSDCSObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog transformCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;
}
