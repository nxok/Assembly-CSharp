using System;
using System.Collections;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationPreview : XUiController
{
	[XuiXmlBinding("ishovered")]
	public bool IsHovered
	{
		get
		{
			return this.isHovered;
		}
		set
		{
			this.isHovered = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("iscontrolling")]
	public bool Controlling
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.xui.playerUI.windowManager.IsInputLocked;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.Controlling == value)
			{
				return;
			}
			this.xui.playerUI.windowManager.IsInputLocked = value;
			this.xui.playerUI.CursorController.Locked = value;
			this.xui.playerUI.CursorController.SetCursorHidden(value);
			if (value)
			{
				PlatformManager.NativePlatform.Input.ActionSetManager.Push(this.xui.playerUI.playerInput);
			}
			else
			{
				PlatformManager.NativePlatform.Input.ActionSetManager.Pop(null);
			}
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.uiPreviewTexture.Controller.OnPress += this.worldPreview_OnPress;
		this.uiPreviewTexture.Controller.OnHover += this.onHover;
	}

	[XuiBindEvent("OnHover", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onHover(XUiController _sender, bool _isOver)
	{
		this.IsHovered = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldPreview_OnPress(XUiController _sender, int _button)
	{
		this.Controlling = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (this.renderTextureSystem == null || !this.Controlling)
		{
			return;
		}
		if (this.xui.playerUI.playerInput.Primary.WasReleased || this.xui.playerUI.playerInput.Menu.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames(delegate
			{
				this.Controlling = false;
			}, 1);
			return;
		}
		Transform transform = this.renderTextureSystem.CameraGO.transform;
		if (this.xui.playerUI.playerInput.Reload.WasReleased || this.xui.playerUI.playerInput.PermanentActions.Reload.WasReleased)
		{
			this.resetCamera();
			return;
		}
		Vector3 vector = this.updateMovementUnified(_dt);
		vector *= 150f * Time.deltaTime;
		Vector3 vector2 = transform.position;
		vector2 += transform.forward * vector.z;
		vector2 += transform.right * vector.x;
		vector2 += transform.up * vector.y;
		vector2.y = Utils.FastMax(40f, vector2.y);
		transform.position = vector2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 updateMovementUnified(float _dt)
	{
		PlayerActionsLocal playerInput = this.xui.playerUI.playerInput;
		Vector3 vector = Vector3.zero;
		vector..ctor(playerInput.Move.X, playerInput.JumpCrouch.Value - playerInput.ToggleCrouch.Value, playerInput.Move.Y);
		if (playerInput.Run.IsPressed)
		{
			vector *= 10f;
		}
		Transform transform = this.renderTextureSystem.CameraGO.transform;
		this.cameraRotX += playerInput.Look.Y * _dt * -165f;
		this.cameraRotY += playerInput.Look.X * _dt * 165f;
		transform.rotation = Quaternion.Euler(this.cameraRotX, this.cameraRotY, 0f);
		return vector;
	}

	public Vector3 GetCameraPosition()
	{
		return this.renderTextureSystem.CameraGO.transform.position;
	}

	public override void OnOpen()
	{
		this.renderTextureInit();
		base.OnOpen();
	}

	public override void OnClose()
	{
		this.Controlling = false;
		this.renderTextureCleanup();
		this.previewTextureCleanup();
		this.TerrainCleanup();
		this.poiPreviewsCleanup();
		base.OnClose();
	}

	public IEnumerator ShowPreview(XUiC_WorldGenerationPreview.PreviewStep _step)
	{
		if (this.uiWinGroup.PreviewQualityLevel == XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
		{
			yield break;
		}
		switch (_step)
		{
		case XUiC_WorldGenerationPreview.PreviewStep.Biome:
			this.PreviewInit();
			yield return XUiC_WorldGenerationPreview.WorldBuilder.SetMessage(Localization.Get("xuiRwgCreatingPreview", false, null), true, false);
			XUiC_WorldGenerationPreview.WorldBuilder.PreviewTextureUpdateBiomes(this.previewColors);
			XUiC_WorldGenerationPreview.previewImage.SetPixels32(this.previewColors);
			XUiC_WorldGenerationPreview.previewImage.Apply(true);
			WorldPreviewTerrain.SetTexture(XUiC_WorldGenerationPreview.previewImage);
			break;
		case XUiC_WorldGenerationPreview.PreviewStep.Terrain:
			this.TerrainUpdate();
			break;
		case XUiC_WorldGenerationPreview.PreviewStep.Done:
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			yield return XUiC_WorldGenerationPreview.WorldBuilder.PreviewTextureUpdateFinal(this.previewColors);
			Log.Out("CreatePreviewTexture in {0}", new object[] { (float)ms.ElapsedMilliseconds * 0.001f });
			XUiC_WorldGenerationPreview.previewImage.SetPixels32(this.previewColors);
			XUiC_WorldGenerationPreview.previewImage.Apply(true);
			WorldPreviewTerrain.SetTexture(XUiC_WorldGenerationPreview.previewImage);
			this.TerrainUpdate();
			break;
		}
		}
		yield break;
	}

	public void PreviewInit()
	{
		this.previewTextureInit();
		this.TerrainCleanup();
		if (!this.terrainPreviewRootObj || XUiC_WorldGenerationPreview.WorldBuilder == null)
		{
			return;
		}
		WorldPreviewTerrain.Init(this.uiWinGroup.WorldBuilder, this.terrainPreviewRootObj.transform);
		this.TerrainUpdate();
		this.resetCamera();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void previewTextureInit()
	{
		this.previewTextureCleanup();
		if (this.uiWinGroup.PreviewQualityLevel == XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
		{
			return;
		}
		int worldSize = this.uiWinGroup.WorldSize;
		int num = worldSize * worldSize;
		if (this.previewColors == null || this.previewColors.Length != num)
		{
			this.previewColors = new Color32[num];
		}
		XUiC_WorldGenerationPreview.previewImage = new Texture2D(worldSize, worldSize)
		{
			filterMode = 0
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void previewTextureCleanup()
	{
		if (!XUiC_WorldGenerationPreview.previewImage)
		{
			return;
		}
		Object.Destroy(XUiC_WorldGenerationPreview.previewImage);
		XUiC_WorldGenerationPreview.previewImage = null;
	}

	public void PreviewTextureDraw()
	{
		if (!XUiC_WorldGenerationPreview.previewImage || XUiC_WorldGenerationPreview.WorldBuilder == null)
		{
			return;
		}
		XUiC_WorldGenerationPreview.previewImage.SetPixels32(this.previewColors);
		XUiC_WorldGenerationPreview.previewImage.Apply(true);
		WorldPreviewTerrain.SetTexture(XUiC_WorldGenerationPreview.previewImage);
	}

	public void TerrainUpdate()
	{
		if (this.terrainPreviewRootObj && XUiC_WorldGenerationPreview.WorldBuilder != null)
		{
			WorldPreviewTerrain.GenerateTerrain();
		}
	}

	public void TerrainCleanup()
	{
		WorldPreviewTerrain.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void poiPreviewsCleanup()
	{
		XUiC_WorldGenerationWindow xuiC_WorldGenerationWindow = this.uiWinGroup;
		if (xuiC_WorldGenerationWindow == null)
		{
			return;
		}
		xuiC_WorldGenerationWindow.CleanupPreviewManager();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void renderTextureInit()
	{
		if (this.renderTextureSystem == null)
		{
			this.renderTextureSystem = new RenderTextureSystem();
			this.terrainPreviewRootObj = new GameObject("TerrainMesh");
			this.renderTextureSystem.Create("worldpreview", this.terrainPreviewRootObj, new Vector3(0f, 0f, 0f), new Vector3(0f, 4000f, 0f), this.uiPreviewTexture.Size, false, false, 1f);
			Camera component = this.renderTextureSystem.CameraGO.transform.GetComponent<Camera>();
			component.nearClipPlane = 0.1f;
			component.farClipPlane = 20000f;
			this.resetCamera();
			Transform transform = this.renderTextureSystem.LightGO.transform;
			transform.localPosition = new Vector3(0f, 2000f, 0f);
			transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
			transform.GetComponent<Light>().type = 1;
			transform.GetComponent<Light>().intensity = 1.2f;
			this.terrainPreviewRootObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			this.terrainPreviewRootObj.transform.position = new Vector3(-10240f, 0f, -10240f);
			this.uiPreviewTexture.Texture = this.renderTextureSystem.RenderTex;
		}
		this.renderTextureSystem.SetEnabled(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void renderTextureCleanup()
	{
		this.renderTextureSystem.SetEnabled(false);
		Object.Destroy(this.renderTextureSystem.ParentGO);
		this.renderTextureSystem = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetCamera()
	{
		if (this.renderTextureSystem == null || XUiC_WorldGenerationPreview.WorldBuilder == null)
		{
			return;
		}
		this.cameraRotX = 90f;
		this.cameraRotY = 0f;
		Transform transform = this.renderTextureSystem.CameraGO.transform;
		transform.localPosition = new Vector3(0f, (float)XUiC_WorldGenerationPreview.WorldBuilder.WorldSize * 0.8745f, 0f);
		transform.localRotation = Quaternion.Euler(this.cameraRotX, this.cameraRotY, 0f);
	}

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationWindow uiWinGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject terrainPreviewRootObj;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture uiPreviewTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotX;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float CurrentFlySpeed = 150f;

	public static WorldBuilder WorldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D previewImage;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] previewColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	public enum PreviewStep
	{
		Start,
		Biome,
		Terrain,
		Done
	}

	public class PrefabNameHandler : MonoBehaviour
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			this.textMesh = base.transform.GetComponent<TextMesh>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnMouseOver(bool _isOver)
		{
			if (this.textMesh == null)
			{
				return;
			}
			base.gameObject.layer = (_isOver ? 11 : 0);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public TextMesh textMesh;
	}
}
