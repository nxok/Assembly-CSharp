using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderWindow : XUiController
{
	public TileEntityVendingMachine CurrentVending
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.xui.Trader.Trader as TileEntityVendingMachine;
		}
	}

	public EntityTrader CurrentTraderEntity
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.xui.Trader.Trader as EntityTrader;
		}
	}

	public bool IsVending
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentVending != null;
		}
	}

	public bool IsOwner
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CurrentVending != null && this.CurrentVending.LocalPlayerIsOwner();
		}
	}

	public bool IsPlayerOwned
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TraderData traderData = this.xui.Trader.TraderData;
			bool? flag;
			if (traderData == null)
			{
				flag = null;
			}
			else
			{
				TraderInfo traderInfo = traderData.TraderInfo;
				flag = ((traderInfo != null) ? new bool?(traderInfo.PlayerOwned) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}
	}

	public bool IsRentable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TraderData traderData = this.xui.Trader.TraderData;
			bool? flag;
			if (traderData == null)
			{
				flag = null;
			}
			else
			{
				TraderInfo traderInfo = traderData.TraderInfo;
				flag = ((traderInfo != null) ? new bool?(traderInfo.Rentable) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			if (this.page != value)
			{
				this.page = value;
				this.itemListGrid.Page = this.page;
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging == null)
				{
					return;
				}
				xuiC_Paging.SetPage(this.page);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.lblGeneralStock = Localization.Get("xuiGeneralStock", false, null);
		this.lblSecretStash = Localization.Get("xuiSecretStash", false, null);
		this.windowicon = base.GetChildById("windowicon");
		this.playerName = (XUiC_PlayerName)base.GetChildById("playerName");
		this.categoryList = this.windowGroup.Controller.GetChildByType<XUiC_CategoryList>();
		this.categoryList.CategoryChanged += this.HandleCategoryChanged;
		this.pager = base.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate
			{
				this.Page = this.pager.CurrentPageNumber;
				this.GetItemStackData(this.txtInput.Text);
			};
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnScroll += this.HandleOnScroll;
		}
		base.OnScroll += this.HandleOnScroll;
		this.itemListGrid = base.Parent.GetChildByType<XUiC_TraderItemList>();
		XUiController[] childrenByType = this.itemListGrid.GetChildrenByType<XUiC_TraderItemEntry>(null);
		XUiController[] array = childrenByType;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].OnScroll += this.HandleOnScroll;
			((XUiC_TraderItemEntry)array[j]).TraderWindow = this;
		}
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
		XUiController xuiController = base.GetChildById("collect");
		if (xuiController != null)
		{
			xuiController.OnPress += this.Collect_OnPress;
		}
		xuiController = base.GetChildById("takeAll");
		if (xuiController != null)
		{
			xuiController.OnPress += this.TakeAll_OnPress;
		}
		xuiController = base.GetChildById("rent");
		if (xuiController != null)
		{
			xuiController.OnPress += this.Rent_OnPress;
			this.rentButton = (XUiV_Button)xuiController.ViewComponent;
		}
		xuiController = base.GetChildById("showAll");
		if (xuiController != null)
		{
			xuiController.OnPress += this.ShowAll_OnPress;
			this.showAllButton = (XUiV_Button)xuiController.ViewComponent;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowAll_OnPress(XUiController _sender, int _mouseButton)
	{
		this.showAll = !this.showAll;
		this.showAllButton.Selected = this.showAll;
		this.RefreshTraderItems();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rent_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.IsVending)
		{
			if (this.serviceInfoWindow == null)
			{
				this.serviceInfoWindow = (XUiC_ServiceInfoWindow)this.windowGroup.Controller.GetChildById("serviceInfoPanel");
			}
			InGameService inGameService = new InGameService
			{
				Name = Localization.Get("rentVendingMachine", false, null),
				Description = Localization.Get("rentVendingMachineDesc", false, null),
				Icon = "ui_game_symbol_vending",
				Price = this.CurrentVending.TraderData.TraderInfo.RentCost,
				VisibleChangedHandler = delegate(bool visible)
				{
					if (this.rentButton != null)
					{
						this.rentButton.Selected = visible;
					}
				}
			};
			if (this.xui.CurrentSelectedEntry != null)
			{
				this.xui.CurrentSelectedEntry.IsSelected = false;
			}
			this.serviceInfoWindow.SetInfo(inGameService, this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Collect_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.IsVending && (this.IsPlayerOwned || this.IsRentable) && this.CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			ItemValue item = ItemClass.GetItem(TraderInfo.CurrencyItem, false);
			int availableMoney = this.xui.Trader.TraderData.AvailableMoney;
			XUiM_PlayerInventory playerInventory = this.xui.PlayerInventory;
			ItemStack itemStack = new ItemStack(item.Clone(), availableMoney);
			playerInventory.AddItem(itemStack);
			this.xui.Trader.TraderData.AvailableMoney = itemStack.count;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TakeAll_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.IsVending && (this.IsPlayerOwned || this.IsRentable) && this.CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			TraderData traderData = this.xui.Trader.TraderData;
			XUiM_PlayerInventory playerInventory = this.xui.PlayerInventory;
			bool flag = false;
			for (int i = 0; i < traderData.PrimaryInventory.Count; i++)
			{
				ItemStack item = traderData.PrimaryInventory[i].Item;
				int num = (item.itemValue.ItemClass.IsBlock() ? Block.list[item.itemValue.type].EconomicBundleSize : item.itemValue.ItemClass.EconomicBundleSize);
				int num2 = Math.Min(item.count, this.xui.PlayerInventory.CountAvailableSpaceForItem(item.itemValue, true)) / num * num;
				int num3 = item.count - num2;
				item.count = num2;
				if (playerInventory.AddItem(item, false))
				{
					flag = true;
				}
				item.count += num3;
				if (item.count == 0)
				{
					traderData.PrimaryInventory.RemoveAt(i--);
				}
			}
			if (flag && GameManager.Instance != null && GameManager.Instance.World != null)
			{
				Manager.Play(GameManager.Instance.World.GetPrimaryPlayer(), "UseActions/takeall1", 1f, false);
			}
			this.RefreshTraderItems();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		string text = _categoryEntry.CategoryName;
		if (text == "SECRET STASH")
		{
			text = "";
			this.isSecretStash = true;
		}
		else
		{
			this.isSecretStash = false;
		}
		this.RefreshHeader();
		this.Page = 0;
		this.SetCategory(text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshHeader()
	{
		if (this.IsVending)
		{
			if (this.IsRentable || this.IsPlayerOwned)
			{
				if (this.CurrentVending.GetOwner() != null)
				{
					string displayName = GameManager.Instance.persistentPlayers.GetPlayerData(this.CurrentVending.GetOwner()).PlayerName.DisplayName;
					this.playerName.SetGenericName(string.Format(Localization.Get("xuiVendingWithOwner", false, null), displayName));
				}
				else
				{
					this.playerName.SetGenericName(Localization.Get("xuiEmptyVendingMachine", false, null));
				}
			}
			else
			{
				this.playerName.SetGenericName(this.lblGeneralStock);
			}
			if (this.windowicon != null)
			{
				((XUiV_Sprite)this.windowicon.ViewComponent).SpriteName = "ui_game_symbol_vending";
				return;
			}
		}
		else
		{
			this.playerName.SetGenericName(this.isSecretStash ? this.lblSecretStash : this.lblGeneralStock);
			((XUiV_Sprite)this.windowicon.ViewComponent).SpriteName = "ui_game_symbol_map_trader";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.Page = 0;
		this.FilterByName(_text);
		this.itemListGrid.SetItems(this.currentInventory.ToArray(), this.currentIndexList);
		if (this.currentInventory.Count == 0 || this.currentInventory[0].IsEmpty())
		{
			base.GetChildById("searchControls").SelectCursorElement(true, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging == null)
			{
				return;
			}
			xuiC_Paging.PageDown();
			return;
		}
		else
		{
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 == null)
			{
				return;
			}
			xuiC_Paging2.PageUp();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetItemStackData(string _name)
	{
		if (_name == null)
		{
			_name = "";
		}
		this.currentInventory.Clear();
		this.length = this.itemListGrid.Length;
		this.FilterByName(_name);
		this.itemListGrid.SetItems(this.currentInventory.ToArray(), this.currentIndexList);
		if (!this.isSecretStash)
		{
			this.categoryList.SetupCategoriesBasedOnItems(this.buyInventory, this.traderStage);
		}
		if (this.currentInventory.Count == 0 || this.currentInventory[0].IsEmpty())
		{
			base.GetChildById("searchControls").SelectCursorElement(true, false);
			return;
		}
	}

	public void FilterByName(string _name)
	{
		this.currentIndexList.Clear();
		this.currentInventory.Clear();
		for (int i = 0; i < this.buyInventory.Count; i++)
		{
			TraderData.Entry entry = this.buyInventory[i];
			ItemStack item = entry.Item;
			if (item != null && item.count != 0)
			{
				ItemClass itemClass = item.itemValue.ItemClass;
				string text = itemClass.GetLocalizedItemName();
				if (text == null)
				{
					text = Localization.Get(itemClass.Name, false, null);
				}
				if (this.category == "")
				{
					if (_name == "" || itemClass.Name.ContainsCaseInsensitive(_name) || text.ContainsCaseInsensitive(_name) || item.itemValue.GetItemOrBlockId().ToString() == _name.Trim())
					{
						TraderStageTemplateGroup traderStageTemplateGroup = null;
						if (itemClass.TraderStageTemplate != null)
						{
							if (!TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
							{
								throw new Exception(string.Concat(new string[]
								{
									"TraderStageTemplate ",
									itemClass.TraderStageTemplate,
									" for item: ",
									itemClass.GetLocalizedItemName(),
									" does not exist."
								}));
							}
							traderStageTemplateGroup = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
						}
						if (traderStageTemplateGroup == null || this.traderStage == -1 || traderStageTemplateGroup.IsWithin(this.traderStage, (int)item.itemValue.Quality) || this.showAll || entry.AddedByPlayer)
						{
							this.currentIndexList.Add(i);
							this.currentInventory.Add(item);
						}
					}
				}
				else
				{
					string[] array = itemClass.Groups;
					if (itemClass.IsBlock())
					{
						array = Block.list[item.itemValue.type].GroupNames;
					}
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != null && array[j].EqualsCaseInsensitive(this.category) && (_name == "" || itemClass.Name.ContainsCaseInsensitive(_name) || text.ContainsCaseInsensitive(_name) || item.itemValue.GetItemOrBlockId().ToString() == _name.Trim()))
						{
							TraderStageTemplateGroup traderStageTemplateGroup2 = null;
							if (itemClass.TraderStageTemplate != null)
							{
								if (!TraderManager.TraderStageTemplates.ContainsKey(itemClass.TraderStageTemplate))
								{
									throw new Exception(string.Concat(new string[]
									{
										"TraderStageTemplate ",
										itemClass.TraderStageTemplate,
										" for item: ",
										itemClass.GetLocalizedItemName(),
										" does not exist."
									}));
								}
								traderStageTemplateGroup2 = TraderManager.TraderStageTemplates[itemClass.TraderStageTemplate];
							}
							if (traderStageTemplateGroup2 == null || this.traderStage == -1 || traderStageTemplateGroup2.IsWithin(this.traderStage, (int)item.itemValue.Quality) || this.showAll || entry.AddedByPlayer)
							{
								this.currentIndexList.Add(i);
								this.currentInventory.Add(item);
							}
						}
					}
				}
			}
		}
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging == null)
		{
			return;
		}
		xuiC_Paging.SetLastPageByElementsAndPageLength(this.currentInventory.Count, this.length);
	}

	public void SetCategory(string _category)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.category = _category;
		this.RefreshTraderItems();
	}

	public string GetCategory()
	{
		return this.category;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.xui.Trader.Trader != null && this.IsVending)
		{
			Manager.PlayInsidePlayerHead("open_vending", -1, 0f, false, false);
		}
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		if (this.CurrentTraderEntity != null)
		{
			this.traderStage = entityPlayer.GetTraderStage(entityPlayer.QuestJournal.GetCurrentFactionTier(this.CurrentTraderEntity.NPCInfo.QuestFaction, 0, false));
		}
		else
		{
			this.traderStage = -1;
		}
		this.CompletedTransaction = false;
		if (this.xui.Trader.TraderData != null)
		{
			this.categoryList.SetCategoryToFirst();
		}
		if (this.IsRentable && this.IsOwner && this.CurrentVending != null && this.CurrentVending.TryAutoBuy(true))
		{
			this.RefreshTraderItems();
		}
		this.Refresh();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.CurrentTraderEntity != null)
		{
			if (this.CompletedTransaction)
			{
				this.CurrentTraderEntity.PlayVoiceSetEntry("sale_accepted", this.xui.playerUI.entityPlayer, true, true);
			}
			else
			{
				this.CurrentTraderEntity.PlayVoiceSetEntry("sale_declined", this.xui.playerUI.entityPlayer, true, true);
			}
		}
		else
		{
			Manager.PlayInsidePlayerHead("close_vending", -1, 0f, false, false);
		}
		XUiM_Trader trader = this.xui.Trader;
		if (trader != null)
		{
			TraderData traderData = trader.TraderData;
			if (traderData != null)
			{
				traderData.SetModified(this.xui.Trader.Trader);
			}
		}
		if (this.CurrentVending != null)
		{
			this.CurrentVending.SetUserAccessing(false);
		}
		LockManager.Instance.UnlockRequestLocal();
		this.xui.Trader.Trader = null;
	}

	public void RefreshTraderItems()
	{
		this.buyInventory.Clear();
		ItemStack[] stashSlots = this.GetStashSlots();
		this.hasSecretStash = stashSlots != null;
		if (this.isSecretStash && this.hasSecretStash)
		{
			for (int i = 0; i < stashSlots.Length; i++)
			{
				this.buyInventory.Add(new TraderData.Entry(stashSlots[i], 0, false));
			}
		}
		else
		{
			this.buyInventory.AddRange(this.xui.Trader.TraderData.PrimaryInventory);
		}
		this.GetItemStackData(this.txtInput.Text);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] GetStashSlots()
	{
		float value = EffectManager.GetValue(PassiveEffects.SecretStash, null, (float)this.xui.playerUI.entityPlayer.Progression.Level, this.xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		for (int i = 0; i < this.xui.Trader.TraderData.TierItemGroups.Count; i++)
		{
			TraderInfo.TierItemGroup tierItemGroup = this.xui.Trader.TraderData.TraderInfo.TierItemGroups[i];
			if ((value >= (float)tierItemGroup.minLevel || tierItemGroup.minLevel == -1) && (value <= (float)tierItemGroup.maxLevel || tierItemGroup.maxLevel == -1))
			{
				return this.xui.Trader.TraderData.TierItemGroups[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1533283582U)
		{
			if (num <= 543700434U)
			{
				if (num != 142312684U)
				{
					if (num != 391295749U)
					{
						if (num == 543700434U)
						{
							if (bindingName == "renttimeleft")
							{
								if (this.IsVending)
								{
									if (this.IsOwner && this.CurrentVending.IsRentable)
									{
										int rentalEndDay = this.CurrentVending.RentalEndDay;
										value = this.rentTimeLeftFormatter.Format(rentalEndDay);
									}
								}
								else
								{
									value = "";
								}
								return true;
							}
						}
					}
					else if (bindingName == "timeleft")
					{
						if (this.xui.Trader.TraderData != null)
						{
							int num2 = GameUtils.WorldTimeToDays(this.xui.Trader.TraderData.NextResetTime);
							value = this.timeLeftFormatter.Format(num2);
						}
						return true;
					}
				}
				else if (bindingName == "availablemoney")
				{
					value = ((this.xui.Trader.TraderData != null) ? this.availableMoneyFormatter.Format(this.xui.Trader.TraderData.AvailableMoney) : "");
					return true;
				}
			}
			else if (num != 619389156U)
			{
				if (num != 916116330U)
				{
					if (num == 1533283582U)
					{
						if (bindingName == "isrentable")
						{
							if (this.IsVending)
							{
								if (this.CurrentVending == null)
								{
									value = "false";
									return true;
								}
								value = this.CurrentVending.IsRentable.ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
				}
				else if (bindingName == "tradername")
				{
					value = "";
					if (this.xui.Trader.TraderData != null)
					{
						if (this.CurrentTraderEntity != null)
						{
							value = this.CurrentTraderEntity.EntityName;
						}
						else if (this.IsVending)
						{
							value = Localization.Get("VendingMachine", false, null);
						}
					}
					return true;
				}
			}
			else if (bindingName == "restocklabel")
			{
				value = Localization.Get("xuiRestock", false, null);
				return true;
			}
		}
		else if (num <= 3170511085U)
		{
			if (num != 1549422821U)
			{
				if (num != 2585672714U)
				{
					if (num == 3170511085U)
					{
						if (bindingName == "isrenter")
						{
							if (this.IsVending)
							{
								if (this.CurrentVending == null)
								{
									value = "false";
									return true;
								}
								value = (this.CurrentVending.IsRentable && this.IsOwner).ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
				}
				else if (bindingName == "isownerorrentable")
				{
					if (this.IsVending)
					{
						if (this.CurrentVending == null)
						{
							value = "false";
							return true;
						}
						value = (this.CurrentVending.IsRentable || (this.IsPlayerOwned && (this.IsOwner || this.CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)))).ToString();
					}
					else
					{
						value = "false";
					}
					return true;
				}
			}
			else if (bindingName == "isnotowner")
			{
				if (this.IsVending)
				{
					if (this.CurrentVending == null)
					{
						value = "true";
						return true;
					}
					value = ((!this.IsPlayerOwned && !this.IsRentable) || !this.CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
				}
				else
				{
					value = "false";
				}
				return true;
			}
		}
		else if (num != 3218764573U)
		{
			if (num != 3392636521U)
			{
				if (num == 4243210436U)
				{
					if (bindingName == "isowner")
					{
						if (this.IsVending)
						{
							if (this.CurrentVending == null)
							{
								value = "false";
								return true;
							}
							value = ((this.IsPlayerOwned || this.IsRentable) && this.CurrentVending.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)).ToString();
						}
						else
						{
							value = "false";
						}
						return true;
					}
				}
			}
			else if (bindingName == "is_debug")
			{
				value = (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && !this.IsVending).ToString();
				return true;
			}
		}
		else if (bindingName == "showrestock")
		{
			value = (this.xui.Trader.TraderData != null && this.xui.Trader.TraderData.TraderInfo.ResetInterval > 0).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			if (this.xui.Trader.TraderData != null)
			{
				if (this.IsVending && this.IsRentable && this.CurrentVending.GetOwner() != null && this.CurrentVending.RentTimeRemaining <= 0f)
				{
					this.CurrentVending.ClearVendingMachine();
					this.Refresh();
					this.RefreshTraderItems();
				}
				if (this.xui.Trader.TraderData.CurrentTime <= 0f && !this.xui.Trader.TraderData.TraderInfo.PlayerOwned && !this.xui.Trader.TraderData.TraderInfo.Rentable && GameManager.Instance.traderManager.TraderInventoryRequested(this.xui.Trader.TraderData, XUiM_Player.GetPlayer().entityId))
				{
					this.xui.Trader.TraderData.SetModified(this.xui.Trader.Trader);
					XUiM_Player.GetPlayer().PlayOneShot("ui_trader_inv_reset", false, false, false, null, 1f);
					this.RefreshTraderItems();
					base.RefreshBindings();
				}
			}
		}
	}

	public void Refresh()
	{
		this.RefreshHeader();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServiceInfoWindow serviceInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderItemList itemListGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TraderData.Entry> buyInventory = new List<TraderData.Entry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> currentIndexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> currentInventory = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowicon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button rentButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button showAllButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayerName playerName;

	public bool CompletedTransaction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblGeneralStock;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSecretStash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showAll;

	[PublicizedFrom(EAccessModifier.Private)]
	public int traderStage = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> rentTimeLeftFormatter = new CachedStringFormatter<int>((int _i) => string.Format("{0}: {1} {2}", Localization.Get("xuiExpires", false, null), Localization.Get("xuiDay", false, null), _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> timeLeftFormatter = new CachedStringFormatter<int>((int _i) => string.Format("{0} {1}", Localization.Get("xuiDay", false, null), _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt availableMoneyFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	public enum TraderActionTypes
	{
		Buy,
		Sell,
		BuyBack
	}
}
