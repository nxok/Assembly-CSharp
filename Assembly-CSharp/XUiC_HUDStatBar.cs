using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_HUDStatBar : XUiController
{
	public HUDStatTypes StatType
	{
		get
		{
			return this.statType;
		}
		set
		{
			this.statType = value;
			this.setStatValues();
		}
	}

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
		this.activeAmmoItemValue = ItemValue.None;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.localPlayer == null && XUi.IsGameRunning())
		{
			this.localPlayer = this.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (windowManager.IsFullHUDDisabled() || (!this.xui.DragAndDropWindow.InMenu && windowManager.IsHUDPartialHidden()))
		{
			this.hudWasHidden = true;
			base.RefreshBindings();
			return;
		}
		if (this.hudWasHidden)
		{
			this.IsDirty = true;
			this.hudWasHidden = false;
		}
		if (this.localPlayer == null)
		{
			return;
		}
		if (this.statGroup == HUDStatGroups.Vehicle)
		{
			if (!this.vehicle)
			{
				EntityVehicle entityVehicle = this.localPlayer.AttachedToEntity as EntityVehicle;
				if (entityVehicle != null)
				{
					this.vehicle = entityVehicle;
					this.IsDirty = true;
					this.xui.CollectedItemList.SetYOffset(100);
					goto IL_0111;
				}
			}
			if (this.vehicle && !this.localPlayer.AttachedToEntity)
			{
				this.vehicle = null;
				this.IsDirty = true;
			}
		}
		IL_0111:
		if (this.statType == HUDStatTypes.Stamina)
		{
			EntityVehicle entityVehicle2 = this.vehicle;
			this.vehicle = this.localPlayer.AttachedToEntity as EntityVehicle;
			if (this.vehicle != entityVehicle2)
			{
				this.IsDirty = true;
			}
			if (this.vehicle)
			{
				bool isTurbo = this.vehicle.vehicle.IsTurbo;
				if (isTurbo != this.vehicleTurbo)
				{
					this.vehicleTurbo = isTurbo;
					this.IsDirty = true;
				}
			}
			else
			{
				bool flag = this.localPlayer.MovementRunning || this.localPlayer.MoveController.RunToggleActive;
				if (this.sprinting != flag)
				{
					this.sprinting = flag;
					this.IsDirty = true;
				}
			}
		}
		if (this.statType == HUDStatTypes.Stealth && this.localPlayer.IsCrouching != this.wasCrouching)
		{
			this.wasCrouching = this.localPlayer.IsCrouching;
			this.IsDirty = true;
		}
		bool flag2 = this.localPlayer.IsDead();
		if (flag2 != this.wasDead)
		{
			this.wasDead = flag2;
			this.IsDirty = true;
		}
		this.refreshFill();
		if (this.hasChanged() || this.IsDirty)
		{
			if (this.statType == HUDStatTypes.ActiveItem)
			{
				this.setupActiveItemEntry();
				this.updateActiveItemAmmo();
			}
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasChanged()
	{
		if (this.statGroup == HUDStatGroups.Vehicle && this.vehicle == null)
		{
			return false;
		}
		if (this.oldStatState != null)
		{
			Stat currentStat = this.getCurrentStat();
			bool flag = !XUiC_HUDStatBar.statEquals(currentStat, this.oldStatState);
			this.oldStatState.CopyFrom(currentStat);
			if (flag)
			{
				return true;
			}
		}
		switch (this.statType)
		{
		case HUDStatTypes.Health:
		case HUDStatTypes.Stamina:
		case HUDStatTypes.Water:
		case HUDStatTypes.Food:
		case HUDStatTypes.Stealth:
		case HUDStatTypes.VehicleHealth:
		case HUDStatTypes.VehicleFuel:
		case HUDStatTypes.VehicleBattery:
			return !Mathf.Approximately(this.oldValue, this.currentValue);
		case HUDStatTypes.ActiveItem:
		{
			if (this.currentSlotIndex != this.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx())
			{
				this.currentSlotIndex = this.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx();
				return true;
			}
			ItemAction itemAction = this.localPlayer.inventory.holdingItemItemValue.ItemClass.Actions[0];
			return itemAction != null && itemAction.IsEditingTool() && itemAction.IsStatChanged();
		}
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshFill()
	{
		if (this.localPlayer == null || (this.statGroup == HUDStatGroups.Vehicle && this.vehicle == null))
		{
			return;
		}
		this.oldValue = this.currentValue;
		float num = Time.deltaTime * 3f;
		float num2;
		switch (this.statType)
		{
		case HUDStatTypes.Health:
			num2 = this.localPlayer.Stats.Health.ValuePercentUI;
			break;
		case HUDStatTypes.Stamina:
			num2 = this.localPlayer.Stats.Stamina.ValuePercentUI;
			break;
		case HUDStatTypes.Water:
			num2 = this.localPlayer.Stats.Water.ValuePercentUI;
			break;
		case HUDStatTypes.Food:
			num2 = this.localPlayer.Stats.Food.ValuePercentUI;
			break;
		case HUDStatTypes.Stealth:
			num2 = this.localPlayer.Stealth.ValuePercentUI;
			break;
		case HUDStatTypes.ActiveItem:
			num2 = ((this.localPlayer.inventory.holdingItemItemValue == null || this.attackAction == null) ? 0f : ((float)this.localPlayer.inventory.holdingItemItemValue.Meta / EffectManager.GetValue(PassiveEffects.MagazineSize, this.localPlayer.inventory.holdingItemItemValue, (float)this.attackAction.BulletsPerMagazine, this.localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)));
			break;
		case HUDStatTypes.VehicleHealth:
			num2 = this.vehicle.GetVehicle().GetHealthPercent();
			break;
		case HUDStatTypes.VehicleFuel:
			num2 = this.vehicle.GetVehicle().GetFuelPercent();
			break;
		case HUDStatTypes.VehicleBattery:
			num2 = this.vehicle.GetVehicle().GetBatteryLevel();
			break;
		default:
			num2 = 0f;
			break;
		}
		float num3 = num2;
		this.currentValue = Mathf.Clamp01(Mathf.Lerp(this.oldValue, num3, num));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		EntityStats entityStats = ((this.localPlayer != null) ? this.localPlayer.Stats : null);
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2825508620U)
		{
			if (num <= 1542587592U)
			{
				if (num != 669092238U)
				{
					if (num != 1122103630U)
					{
						if (num == 1542587592U)
						{
							if (_bindingName == "statcurrent")
							{
								if (entityStats == null || (this.statGroup == HUDStatGroups.Vehicle && this.vehicle == null))
								{
									_value = "";
									return true;
								}
								string text;
								switch (this.statType)
								{
								case HUDStatTypes.Health:
									text = this.statcurrentFormatterInt.Format((int)entityStats.Health.Value);
									break;
								case HUDStatTypes.Stamina:
									text = this.statcurrentFormatterFloat.Format(entityStats.Stamina.Value);
									break;
								case HUDStatTypes.Water:
									text = this.statcurrentFormatterInt.Format((int)(entityStats.Water.ValuePercentUI * 100f));
									break;
								case HUDStatTypes.Food:
									text = this.statcurrentFormatterInt.Format((int)(entityStats.Food.ValuePercentUI * 100f));
									break;
								case HUDStatTypes.Stealth:
									text = this.statcurrentFormatterFloat.Format((float)((int)(this.localPlayer.Stealth.ValuePercentUI * 100f)));
									break;
								case HUDStatTypes.ActiveItem:
									text = ((this.attackAction is ItemActionTextureBlock) ? this.currentPaintAmmoFormatter.Format(this.currentAmmoCount) : this.statcurrentFormatterInt.Format(this.localPlayer.inventory.holdingItemItemValue.Meta));
									break;
								case HUDStatTypes.VehicleHealth:
									text = this.statcurrentFormatterInt.Format(this.vehicle.GetVehicle().GetHealth());
									break;
								case HUDStatTypes.VehicleFuel:
									text = this.statcurrentFormatterFloat.Format(this.vehicle.GetVehicle().GetFuelLevel());
									break;
								case HUDStatTypes.VehicleBattery:
									text = this.statcurrentFormatterFloat.Format(this.vehicle.GetVehicle().GetBatteryLevel());
									break;
								default:
									text = "";
									break;
								}
								_value = text;
								return true;
							}
						}
					}
					else if (_bindingName == "statcurrentwithmax")
					{
						if (entityStats == null || (this.statGroup == HUDStatGroups.Vehicle && this.vehicle == null))
						{
							_value = "";
							return true;
						}
						switch (this.statType)
						{
						case HUDStatTypes.Health:
						case HUDStatTypes.Stamina:
						case HUDStatTypes.Water:
						case HUDStatTypes.Food:
						{
							Stat currentStat = this.getCurrentStat();
							_value = this.statcurrentWMaxFormatterAOfB.Format(Mathf.RoundToInt(currentStat.Value), Mathf.RoundToInt(currentStat.Max));
							break;
						}
						case HUDStatTypes.Stealth:
							_value = this.statcurrentWMaxFormatterOf100.Format((int)(this.localPlayer.Stealth.ValuePercentUI * 100f));
							break;
						case HUDStatTypes.ActiveItem:
							if (this.attackAction is ItemActionTextureBlock)
							{
								_value = this.currentPaintAmmoFormatter.Format(this.currentAmmoCount);
							}
							else if (this.attackAction != null && this.attackAction.IsEditingTool())
							{
								ItemActionData itemActionDataInSlot = this.localPlayer.inventory.GetItemActionDataInSlot(this.currentSlotIndex, 1);
								_value = this.attackAction.GetStat(itemActionDataInSlot);
							}
							else
							{
								_value = this.statcurrentWMaxFormatterAOfB.Format(this.localPlayer.inventory.GetItem(this.currentSlotIndex).itemValue.Meta, this.currentAmmoCount);
							}
							break;
						case HUDStatTypes.VehicleHealth:
							_value = this.statcurrentWMaxFormatterPercent.Format((int)(this.vehicle.GetVehicle().GetHealthPercent() * 100f));
							break;
						case HUDStatTypes.VehicleFuel:
							_value = this.statcurrentWMaxFormatterPercent.Format((int)(this.vehicle.GetVehicle().GetFuelPercent() * 100f));
							break;
						case HUDStatTypes.VehicleBattery:
							_value = this.statcurrentWMaxFormatterPercent.Format((int)(this.vehicle.GetVehicle().GetBatteryLevel() * 100f));
							break;
						}
						return true;
					}
				}
				else if (_bindingName == "statfill")
				{
					_value = "0";
					if (entityStats == null || (this.statGroup == HUDStatGroups.Vehicle && this.vehicle == null))
					{
						return true;
					}
					_value = this.statfillFormatter.Format(this.currentValue);
					return true;
				}
			}
			else if (num != 1822678806U)
			{
				if (num != 2758588565U)
				{
					if (num == 2825508620U)
					{
						if (_bindingName == "statimage")
						{
							_value = this.statImage;
							return true;
						}
					}
				}
				else if (_bindingName == "staticonatlas")
				{
					_value = this.statAtlas;
					return true;
				}
			}
			else if (_bindingName == "staticon")
			{
				HUDStatTypes hudstatTypes = this.statType;
				string text;
				if (hudstatTypes != HUDStatTypes.ActiveItem)
				{
					if (hudstatTypes != HUDStatTypes.VehicleHealth)
					{
						text = this.statIcon;
					}
					else
					{
						text = ((this.vehicle != null) ? this.vehicle.GetMapIcon() : "");
					}
				}
				else
				{
					text = ((this.itemClass != null) ? this.itemClass.GetIconName() : "");
				}
				_value = text;
				return true;
			}
		}
		else if (num <= 3799067675U)
		{
			if (num != 3007315583U)
			{
				if (num != 3150708601U)
				{
					if (num == 3799067675U)
					{
						if (_bindingName == "statvisible")
						{
							_value = "true";
							if (this.localPlayer == null)
							{
								return true;
							}
							if (this.localPlayer.IsDead())
							{
								_value = "false";
								return true;
							}
							if (this.xui.playerUI.windowManager.IsFullHUDDisabled() || (!this.xui.DragAndDropWindow.InMenu && this.xui.playerUI.windowManager.IsHUDPartialHidden()))
							{
								_value = "false";
								return true;
							}
							if (this.statGroup == HUDStatGroups.Vehicle)
							{
								if (this.statType == HUDStatTypes.VehicleFuel)
								{
									_value = (this.vehicle != null && this.vehicle.GetVehicle().HasEnginePart() && EntityVehicle.VehicleFuelUsageModifier > 0f).ToString();
								}
								else
								{
									_value = (this.vehicle != null).ToString();
								}
							}
							else if (this.statType == HUDStatTypes.ActiveItem)
							{
								if (this.attackAction != null && (this.attackAction.IsEditingTool() || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, this.localPlayer.inventory.holdingItemItemValue, 0f, this.localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0))
								{
									_value = "true";
								}
								else
								{
									_value = "false";
								}
							}
							else if (this.statType == HUDStatTypes.Stealth)
							{
								if (this.localPlayer.Crouching)
								{
									this.xui.BuffPopoutList.SetYOffset(52);
									_value = "true";
								}
								else
								{
									this.xui.BuffPopoutList.SetYOffset(0);
									_value = "false";
								}
							}
							return true;
						}
					}
				}
				else if (_bindingName == "staticoncolor")
				{
					Color32 color = Color.white;
					if (this.statType == HUDStatTypes.ActiveItem && this.itemClass != null)
					{
						color = this.itemClass.GetIconTint(null);
					}
					_value = this.staticoncolorFormatter.Format(color);
					return true;
				}
			}
			else if (_bindingName == "sprintactive")
			{
				_value = "false";
				if (this.localPlayer)
				{
					if (this.vehicle)
					{
						if (this.vehicleTurbo)
						{
							_value = "true";
						}
					}
					else if (this.sprinting)
					{
						_value = "true";
					}
				}
				return true;
			}
		}
		else if (num != 3888153342U)
		{
			if (num != 3905392387U)
			{
				if (num == 3907838626U)
				{
					if (_bindingName == "statmodifiedmax")
					{
						_value = "0";
						if (entityStats == null || (this.statGroup == HUDStatGroups.Vehicle && this.vehicle == null))
						{
							return true;
						}
						Stat currentStat2 = this.getCurrentStat();
						if (currentStat2 != null)
						{
							_value = this.statmodifiedmaxFormatter.Format(currentStat2.ModifiedMax, currentStat2.Max);
						}
						return true;
					}
				}
			}
			else if (_bindingName == "stealthcolor")
			{
				_value = this.stealthColorFormatter.Format(this.localPlayer ? this.localPlayer.Stealth.ValueColorUI : default(Color32));
				return true;
			}
		}
		else if (_bindingName == "statregenrate")
		{
			_value = "0";
			if (entityStats == null || (this.statGroup == HUDStatGroups.Vehicle && this.vehicle == null))
			{
				return true;
			}
			Stat currentStat3 = this.getCurrentStat();
			if (currentStat3 != null)
			{
				_value = this.statregenrateFormatter.Format(currentStat3.RegenerationAmountUI);
			}
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "stat_type")
		{
			this.StatType = EnumUtils.Parse<HUDStatTypes>(_value, true);
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setStatValues()
	{
		switch (this.statType)
		{
		case HUDStatTypes.Health:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_add";
			this.statGroup = HUDStatGroups.Player;
			this.oldStatState = new Stat(Stat.StatTypes.Health, null, 0f, 0f);
			return;
		case HUDStatTypes.Stamina:
			this.statImage = "ui_game_stat_bar_stamina";
			this.statIcon = "ui_game_symbol_run";
			this.statGroup = HUDStatGroups.Player;
			this.oldStatState = new Stat(Stat.StatTypes.Stamina, null, 0f, 0f);
			return;
		case HUDStatTypes.Water:
			this.statImage = "ui_game_stat_bar_stamina";
			this.statIcon = "ui_game_symbol_water";
			this.statGroup = HUDStatGroups.Player;
			this.oldStatState = new Stat(Stat.StatTypes.Water, null, 0f, 0f);
			return;
		case HUDStatTypes.Food:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_hunger";
			this.statGroup = HUDStatGroups.Player;
			this.oldStatState = new Stat(Stat.StatTypes.Food, null, 0f, 0f);
			return;
		case HUDStatTypes.Stealth:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_stealth";
			this.statGroup = HUDStatGroups.Player;
			return;
		case HUDStatTypes.ActiveItem:
			this.statImage = "ui_game_popup";
			this.statIcon = "ui_game_symbol_battery";
			this.statGroup = HUDStatGroups.Player;
			this.statAtlas = "ItemIconAtlas";
			return;
		case HUDStatTypes.VehicleHealth:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_minibike";
			this.statGroup = HUDStatGroups.Vehicle;
			return;
		case HUDStatTypes.VehicleFuel:
			this.statImage = "ui_game_stat_bar_stamina";
			this.statIcon = "ui_game_symbol_gas";
			this.statGroup = HUDStatGroups.Vehicle;
			return;
		case HUDStatTypes.VehicleBattery:
			this.statImage = "ui_game_popup";
			this.statIcon = "ui_game_symbol_battery";
			this.statGroup = HUDStatGroups.Vehicle;
			return;
		default:
			return;
		}
	}

	public Stat getCurrentStat()
	{
		if (this.localPlayer == null)
		{
			return null;
		}
		Stat stat;
		switch (this.statType)
		{
		case HUDStatTypes.Health:
			stat = this.localPlayer.Stats.Health;
			break;
		case HUDStatTypes.Stamina:
			stat = this.localPlayer.Stats.Stamina;
			break;
		case HUDStatTypes.Water:
			stat = this.localPlayer.Stats.Water;
			break;
		case HUDStatTypes.Food:
			stat = this.localPlayer.Stats.Food;
			break;
		default:
			stat = null;
			break;
		}
		return stat;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.statType == HUDStatTypes.ActiveItem)
		{
			this.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
			this.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		this.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupActiveItemEntry()
	{
		this.itemClass = null;
		this.attackAction = null;
		this.activeAmmoItemValue = ItemValue.None;
		if (this.localPlayer == null)
		{
			return;
		}
		Inventory inventory = this.localPlayer.inventory;
		ItemValue itemValue = inventory.GetItem(this.currentSlotIndex).itemValue;
		this.itemClass = itemValue.ItemClass;
		if (this.itemClass == null)
		{
			this.currentAmmoCount = 0;
			this.xui.CollectedItemList.SetYOffset((this.localPlayer.AttachedToEntity is EntityVehicle) ? 100 : 0);
			return;
		}
		ItemActionAttack itemActionAttack = this.itemClass.Actions[0] as ItemActionAttack;
		if (itemActionAttack != null && itemActionAttack.IsEditingTool())
		{
			this.attackAction = itemActionAttack;
			this.xui.CollectedItemList.SetYOffset(46);
			return;
		}
		if (itemActionAttack == null || itemActionAttack is ItemActionMelee || !this.itemClass.IsGun() || (itemActionAttack.InfiniteAmmo && !itemActionAttack.ForceShowAmmo) || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, inventory.holdingItemItemValue, 0f, this.localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) <= 0)
		{
			this.currentAmmoCount = 0;
			this.xui.CollectedItemList.SetYOffset((this.localPlayer.AttachedToEntity is EntityVehicle) ? 100 : 0);
			return;
		}
		this.attackAction = itemActionAttack;
		if (itemActionAttack.MagazineItemNames != null && itemActionAttack.MagazineItemNames.Length != 0)
		{
			this.lastAmmoName = itemActionAttack.MagazineItemNames[(int)itemValue.SelectedAmmoTypeIndex];
			this.activeAmmoItemValue = ItemClass.GetItem(this.lastAmmoName, false);
			this.itemClass = ItemClass.GetItemClass(this.lastAmmoName, false);
		}
		this.xui.CollectedItemList.SetYOffset(46);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateActiveItemAmmo()
	{
		if (this.activeAmmoItemValue.type == 0)
		{
			return;
		}
		if (this.localPlayer == null)
		{
			return;
		}
		this.currentAmmoCount = this.localPlayer.inventory.GetItemCount(this.activeAmmoItemValue, false, -1, -1, true);
		this.currentAmmoCount += this.localPlayer.bag.GetItemCount(this.activeAmmoItemValue, -1, -1, true);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool statEquals(Stat _a, Stat _b)
	{
		return Mathf.Approximately(_a.Value, _b.Value) && Mathf.Approximately(_a.Max, _b.Max) && Mathf.Approximately(_a.MaxModifier, _b.MaxModifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatGroups statGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatTypes statType;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle vehicle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool vehicleTurbo;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statImage = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statIcon = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statAtlas = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stat oldStatState;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasDead;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasCrouching;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hudWasHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentSlotIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastAmmoName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentAmmoCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue activeAmmoItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionAttack attackAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sprinting;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statcurrentFormatterInt = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statcurrentFormatterFloat = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt currentPaintAmmoFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> statcurrentWMaxFormatterAOfB = new CachedStringFormatter<int, int>((int _i, int _i1) => string.Format("{0}/{1}", _i, _i1));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> statcurrentWMaxFormatterOf100 = new CachedStringFormatter<int>((int _i) => _i.ToString() + "/100");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> statcurrentWMaxFormatterPercent = new CachedStringFormatter<int>((int _i) => _i.ToString() + "%");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float, float> statmodifiedmaxFormatter = new CachedStringFormatter<float, float>((float _f1, float _f2) => (_f1 / _f2).ToCultureInvariantString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> statregenrateFormatter = new CachedStringFormatter<float>((float _f) => ((_f >= 0f) ? "+" : "") + _f.ToCultureInvariantString("0.00"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statfillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor staticoncolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor stealthColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastRegenAmount;
}
