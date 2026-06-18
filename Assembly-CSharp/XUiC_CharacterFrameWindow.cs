using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterFrameWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.tabs = base.GetChildByType<XUiC_TabSelector>();
		this.previewFrame = base.GetChildById("previewFrameSDCS");
		if (this.previewFrame != null)
		{
			this.previewFrame.OnPress += this.PreviewFrame_OnPress;
			this.previewFrame.OnHover += this.PreviewFrame_OnHover;
		}
		this.lblLevel = (XUiV_Label)base.GetChildById("levelNumber").ViewComponent;
		this.lblName = (XUiV_Label)base.GetChildById("characterName").ViewComponent;
		this.textPreview = (XUiV_Texture)base.GetChildById("playerPreviewSDCS").ViewComponent;
		this.IsDirty = true;
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("coreStatsMovementModeToggle") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.coreStatsMovementModeToggle_OnPress;
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnCosmetics") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += this.btnCosmetic_OnPress;
		}
		XUiC_EquipmentStackGrid childByType = base.GetChildByType<XUiC_EquipmentStackGrid>();
		if (childByType != null)
		{
			XUiController childById = base.GetChildById("badgeSlots");
			if (childById != null)
			{
				childByType.RegisterExtraSlots(childById.GetChildrenByType<XUiC_EquipmentStack>(null));
			}
			XUiController childById2 = base.GetChildById("clothingSlots");
			if (childById2 != null)
			{
				childByType.RegisterExtraSlots(childById2.GetChildrenByType<XUiC_EquipmentStack>(null));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCosmetic_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_CharacterCosmeticWindowGroup.Open(this.xui, EquipmentSlots.Head);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void coreStatsMovementModeToggle_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.currentMovementTag.Equals(EntityAlive.MovementTagIdle))
		{
			this.currentMovementTag = EntityAlive.MovementTagWalking;
		}
		else if (this.currentMovementTag.Equals(EntityAlive.MovementTagWalking))
		{
			this.currentMovementTag = EntityAlive.MovementTagRunning;
		}
		else
		{
			this.currentMovementTag = EntityAlive.MovementTagIdle;
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnHover(XUiController _sender, bool _isOver)
	{
		this.isMouseOverPreview = _isOver;
		XUiC_Paging.IsOverPagingOverrideElement = _isOver;
		if (_isOver)
		{
			this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview, 0f);
			return;
		}
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterPreview);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnPress(XUiController _sender, int _mouseButton)
	{
		ItemStack currentStack = this.xui.DragAndDropWindow.CurrentStack;
		if (!currentStack.IsEmpty() && currentStack.itemValue.ItemClass.CanEquip())
		{
			ItemStack itemStack = this.xui.PlayerEquipment.EquipItem(currentStack);
			if (currentStack != itemStack)
			{
				this.xui.DragAndDropWindow.CurrentStack = itemStack;
				this.xui.DragAndDropWindow.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
			}
		}
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		XUiC_TabSelector xuiC_TabSelector = this.tabs;
		string text;
		if (xuiC_TabSelector == null)
		{
			text = null;
		}
		else
		{
			XUiC_TabSelectorTab selectedTab = xuiC_TabSelector.SelectedTab;
			text = ((selectedTab != null) ? selectedTab.TabKey : null);
		}
		string text2 = text;
		if (text2 != "armor" && text2 != "clothing" && text2 != "gear" && Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 0.25f;
			base.RefreshBindings();
		}
		if (this.IsDirty)
		{
			if (this.player == null)
			{
				return;
			}
			this.lblLevel.Text = string.Format(Localization.Get("lblLevel", false, null), this.player.Progression.GetLevel());
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
				if (flag && this.isMouseOverPreview)
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
		if (this.previewFrame != null)
		{
			this.previewFrame.OnPress -= this.PreviewFrame_OnPress;
			this.previewFrame.OnHover -= this.PreviewFrame_OnHover;
		}
		this.previewFrame = base.GetChildById("previewFrameSDCS");
		if (this.previewFrame != null)
		{
			this.previewFrame.OnPress += this.PreviewFrame_OnPress;
			this.previewFrame.OnHover += this.PreviewFrame_OnHover;
		}
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
		this.displayInfoEntries = UIDisplayInfoManager.Current.GetCharacterDisplayInfo();
		if (this.player as EntityPlayerLocal != null && this.player.emodel as EModelSDCS != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		}
		XUiM_PlayerEquipment.SlotChanged += this.XUiM_PlayerEquipmentOnSlotChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipmentOnSlotChanged(XUiM_PlayerEquipment _playerEquipment, EquipmentSlots _slot, ItemStack _newStack)
	{
		if (this.tabs == null)
		{
			return;
		}
		if (_slot <= EquipmentSlots.Feet)
		{
			this.tabs.SelectTabByName("armor");
			return;
		}
		if (_slot >= EquipmentSlots.BiomeBadge && _slot <= EquipmentSlots.BiomeBadge4)
		{
			this.tabs.SelectTabByName("gear");
			return;
		}
		if (_slot >= EquipmentSlots.ClothingHead && _slot <= EquipmentSlots.ClothingFeet)
		{
			this.tabs.SelectTabByName("clothing");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
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
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		XUiM_PlayerEquipment.SlotChanged -= this.XUiM_PlayerEquipmentOnSlotChanged;
		SDCSUtils.UnloadViz(this.previewSDCSObj);
		this.renderTextureSystem.Cleanup();
		this.EndCursorLock();
		this.characterPivot = null;
		this.isMouseOverPreview = false;
		this.isDragging = false;
		XUiC_Paging.IsOverPagingOverrideElement = false;
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
		if (this.player == null)
		{
			return;
		}
		if (this.player.emodel == null)
		{
			return;
		}
		EModelSDCS emodelSDCS = this.player.emodel as EModelSDCS;
		if (emodelSDCS == null)
		{
			return;
		}
		this.isPreviewDirty = false;
		SDCSUtils.CreateVizUI(emodelSDCS.Archetype, ref this.previewSDCSObj, ref this.transformCatalog, this.player, false);
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2347010444U)
		{
			if (num <= 782575427U)
			{
				if (num <= 234495987U)
				{
					if (num <= 75296162U)
					{
						if (num != 8937094U)
						{
							if (num == 75296162U)
							{
								if (_bindingName == "playerfoodtitle")
								{
									_value = Localization.Get("xuiFood", false, null);
									return true;
								}
							}
						}
						else if (_bindingName == "playerhealth")
						{
							_value = ((this.player != null) ? this.playerHealthFormatter.Format((int)XUiM_Player.GetHealth(this.player)) : "");
							return true;
						}
					}
					else if (num != 125916223U)
					{
						if (num != 145239280U)
						{
							if (num == 234495987U)
							{
								if (_bindingName == "playerdeathstitle")
								{
									_value = Localization.Get("xuiDeaths", false, null);
									return true;
								}
							}
						}
						else if (_bindingName == "playerarmorratingtitle")
						{
							_value = Localization.Get("statPhysicalDamageResist", false, null);
							return true;
						}
					}
					else if (_bindingName == "playercurrentlife")
					{
						_value = ((this.player != null) ? XUiM_Player.GetCurrentLife(this.player) : "");
						return true;
					}
				}
				else if (num <= 526317834U)
				{
					if (num != 304163417U)
					{
						if (num == 526317834U)
						{
							if (_bindingName == "playergamestagetitle")
							{
								_value = Localization.Get("xuiGameStage", false, null);
								return true;
							}
						}
					}
					else if (_bindingName == "playercoretemp")
					{
						_value = ((this.player != null) ? XUiM_Player.GetCoreTemp(this.player) : "");
						return true;
					}
				}
				else if (num != 672688503U)
				{
					if (num != 696376978U)
					{
						if (num == 782575427U)
						{
							if (_bindingName == "playerdeaths")
							{
								_value = ((this.player != null) ? this.playerDeathsFormatter.Format(XUiM_Player.GetDeaths(this.player)) : "");
								return true;
							}
						}
					}
					else if (_bindingName == "playerlootstagetitle")
					{
						_value = Localization.Get("xuiLootstage", false, null);
						return true;
					}
				}
				else if (_bindingName == "playercurrentlifetitle")
				{
					_value = Localization.Get("xuiCurrentLife", false, null);
					return true;
				}
			}
			else if (num <= 1477941828U)
			{
				if (num <= 965025103U)
				{
					if (num != 885900949U)
					{
						if (num == 965025103U)
						{
							if (_bindingName == "playerwater")
							{
								_value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetWater(this.player)) : "");
								return true;
							}
						}
					}
					else if (_bindingName == "playertravelledtitle")
					{
						_value = Localization.Get("xuiKMTravelled", false, null);
						return true;
					}
				}
				else if (num != 1009276468U)
				{
					if (num != 1092850731U)
					{
						if (num == 1477941828U)
						{
							if (_bindingName == "playerlongestlife")
							{
								_value = ((this.player != null) ? XUiM_Player.GetLongestLife(this.player) : "");
								return true;
							}
						}
					}
					else if (_bindingName == "movementmode")
					{
						_value = (this.currentMovementTag.Equals(EntityAlive.MovementTagIdle) ? "Idle" : (this.currentMovementTag.Equals(EntityAlive.MovementTagWalking) ? "Walking" : "Running"));
						return true;
					}
				}
				else if (_bindingName == "playerxptonextleveltitle")
				{
					_value = Localization.Get("xuiXPToNextLevel", false, null);
					return true;
				}
			}
			else if (num <= 2023588471U)
			{
				if (num != 1811778199U)
				{
					if (num == 2023588471U)
					{
						if (_bindingName == "playerzombiekillstitle")
						{
							_value = Localization.Get("xuiZombieKills", false, null);
							return true;
						}
					}
				}
				else if (_bindingName == "playeritemscraftedtitle")
				{
					_value = Localization.Get("xuiItemsCrafted", false, null);
					return true;
				}
			}
			else if (num != 2186126559U)
			{
				if (num != 2219475343U)
				{
					if (num == 2347010444U)
					{
						if (_bindingName == "playergamestage")
						{
							_value = ((this.player != null) ? this.player.gameStage.ToString() : "");
							return true;
						}
					}
				}
				else if (_bindingName == "playermaxstamina")
				{
					_value = ((this.player != null) ? this.playerMaxStaminaFormatter.Format((int)XUiM_Player.GetMaxStamina(this.player)) : "");
					return true;
				}
			}
			else if (_bindingName == "playeritemscrafted")
			{
				_value = ((this.player != null) ? this.playerItemsCraftedFormatter.Format(XUiM_Player.GetItemsCrafted(this.player)) : "");
				return true;
			}
		}
		else if (num <= 3532909960U)
		{
			if (num <= 3145112967U)
			{
				if (num <= 2532548756U)
				{
					if (num != 2395478116U)
					{
						if (num == 2532548756U)
						{
							if (_bindingName == "playerfood")
							{
								_value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetFood(this.player)) : "");
								return true;
							}
						}
					}
					else if (_bindingName == "playerlootstage")
					{
						_value = ((this.player != null) ? this.player.GetLootStage(0f, 0f).ToString() : "");
						return true;
					}
				}
				else if (num != 2974192615U)
				{
					if (num != 3042900123U)
					{
						if (num == 3145112967U)
						{
							if (_bindingName == "gear_visible")
							{
								_value = World.BiomeProgressionEnabled.ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "playerpvpkills")
					{
						_value = ((this.player != null) ? this.playerPvpKillsFormatter.Format(XUiM_Player.GetPlayerKills(this.player)) : "");
						return true;
					}
				}
				else if (_bindingName == "playerwatertitle")
				{
					_value = Localization.Get("xuiWater", false, null);
					return true;
				}
			}
			else if (num <= 3275992332U)
			{
				if (num != 3249756066U)
				{
					if (num == 3275992332U)
					{
						if (_bindingName == "playermaxhealth")
						{
							_value = ((this.player != null) ? this.playerMaxHealthFormatter.Format((int)XUiM_Player.GetMaxHealth(this.player)) : "");
							return true;
						}
					}
				}
				else if (_bindingName == "playerfoodmax")
				{
					_value = ((this.player != null) ? this.playerFoodMaxFormatter.Format(XUiM_Player.GetFoodMax(this.player)) : "");
					return true;
				}
			}
			else if (num != 3371877161U)
			{
				if (num != 3484390642U)
				{
					if (num == 3532909960U)
					{
						if (_bindingName == "clothing_visible")
						{
							_value = (World.TemperatureSurvival || World.StormFrequency > 0f).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "playerlongestlifetitle")
				{
					_value = Localization.Get("xuiLongestLife", false, null);
					return true;
				}
			}
			else if (_bindingName == "playerstaminatitle")
			{
				_value = Localization.Get("lblStamina", false, null);
				return true;
			}
		}
		else if (num <= 3900606022U)
		{
			if (num <= 3705263762U)
			{
				if (num != 3537464933U)
				{
					if (num == 3705263762U)
					{
						if (_bindingName == "playerxptonextlevel")
						{
							_value = ((this.player != null) ? this.playerXpToNextLevelFormatter.Format(XUiM_Player.GetXPToNextLevel(this.player) + this.player.Progression.ExpDeficit) : "");
							return true;
						}
					}
				}
				else if (_bindingName == "playerstamina")
				{
					_value = ((this.player != null) ? this.playerStaminaFormatter.Format((int)XUiM_Player.GetStamina(this.player)) : "");
					return true;
				}
			}
			else if (num != 3712331684U)
			{
				if (num != 3887827771U)
				{
					if (num == 3900606022U)
					{
						if (_bindingName == "playerarmorrating")
						{
							_value = ((this.player != null) ? this.playerArmorRatingFormatter.Format((int)EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, this.player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)) : "");
							return true;
						}
					}
				}
				else if (_bindingName == "playerpvpkillstitle")
				{
					_value = Localization.Get("xuiPlayerKills", false, null);
					return true;
				}
			}
			else if (_bindingName == "playermodifiedcurrentfood")
			{
				_value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetModifiedCurrentFood(this.player)) : "");
				return true;
			}
		}
		else if (num <= 4031300656U)
		{
			if (num != 3931175545U)
			{
				if (num != 4025935093U)
				{
					if (num == 4031300656U)
					{
						if (_bindingName == "playerhealthtitle")
						{
							_value = Localization.Get("lblHealth", false, null);
							return true;
						}
					}
				}
				else if (_bindingName == "playercoretemptitle")
				{
					_value = Localization.Get("xuiFeelsLike", false, null);
					return true;
				}
			}
			else if (_bindingName == "playertravelled")
			{
				_value = ((this.player != null) ? XUiM_Player.GetKMTraveled(this.player) : "");
				return true;
			}
		}
		else if (num != 4077864767U)
		{
			if (num != 4107995367U)
			{
				if (num == 4159374943U)
				{
					if (_bindingName == "playermodifiedcurrentwater")
					{
						_value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetModifiedCurrentWater(this.player)) : "");
						return true;
					}
				}
			}
			else if (_bindingName == "playerwatermax")
			{
				_value = ((this.player != null) ? this.playerWaterMaxFormatter.Format(XUiM_Player.GetWaterMax(this.player)) : "");
				return true;
			}
		}
		else if (_bindingName == "playerzombiekills")
		{
			_value = ((this.player != null) ? this.playerZombieKillsFormatter.Format(XUiM_Player.GetZombieKills(this.player)) : "");
			return true;
		}
		if (_bindingName.StartsWith("playerstattitle"))
		{
			if (this.player != null)
			{
				int num2 = Convert.ToInt32(_bindingName.Replace("playerstattitle", "")) - 1;
				_value = this.GetStatTitle(num2);
			}
			else
			{
				_value = "";
			}
			return true;
		}
		if (_bindingName.StartsWith("playerstat"))
		{
			if (this.player != null)
			{
				int num3 = Convert.ToInt32(_bindingName.Replace("playerstat", "")) - 1;
				_value = this.GetStatValue(num3);
			}
			else
			{
				_value = "";
			}
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int _index)
	{
		if (this.displayInfoEntries.Count <= _index)
		{
			return "";
		}
		if (this.displayInfoEntries[_index].TitleOverride != null)
		{
			return this.displayInfoEntries[_index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(this.displayInfoEntries[_index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int _index)
	{
		if (this.displayInfoEntries.Count <= _index)
		{
			return "";
		}
		DisplayInfoEntry displayInfoEntry = this.displayInfoEntries[_index];
		return XUiM_Player.GetStatValue(displayInfoEntry.StatType, this.xui.playerUI.entityPlayer, displayInfoEntry, this.currentMovementTag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RenderTextureSystem renderTextureSystem = new RenderTextureSystem();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPreviewDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DisplayInfoEntry> displayInfoEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> currentMovementTag = EntityAlive.MovementTagRunning;

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
	public XUiController previewFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewSDCSObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog transformCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerDeathsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerHealthFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerStaminaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerMaxHealthFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerMaxStaminaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat playerFoodFormatter = new CachedStringFormatterFloat("0");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat playerWaterFormatter = new CachedStringFormatterFloat("0");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerFoodMaxFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerWaterMaxFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerItemsCraftedFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerPvpKillsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerZombieKillsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerXpToNextLevelFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerArmorRatingFormatter = new CachedStringFormatterInt();
}
