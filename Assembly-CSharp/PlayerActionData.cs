using System;

public class PlayerActionData
{
	public static readonly PlayerActionData.ActionTab TabMovement = new PlayerActionData.ActionTab("inpTabPlayerControl", 0);

	public static readonly PlayerActionData.ActionTab TabToolbelt = new PlayerActionData.ActionTab("inpTabToolbelt", 10);

	public static readonly PlayerActionData.ActionTab TabVehicle = new PlayerActionData.ActionTab("inpTabVehicle", 15);

	public static readonly PlayerActionData.ActionTab TabMenus = new PlayerActionData.ActionTab("inpTabMenus", 20);

	public static readonly PlayerActionData.ActionTab TabUi = new PlayerActionData.ActionTab("inpTabUi", 30);

	public static readonly PlayerActionData.ActionTab TabOther = new PlayerActionData.ActionTab("inpTabOther", 40);

	public static readonly PlayerActionData.ActionTab TabEdit = new PlayerActionData.ActionTab("inpTabEdit", 50);

	public static readonly PlayerActionData.ActionTab TabGlobal = new PlayerActionData.ActionTab("inpTabGlobal", 60);

	public static readonly PlayerActionData.ActionGroup GroupPlayerControl = new PlayerActionData.ActionGroup("inpGrpPlayerControlName", null, 0, PlayerActionData.TabMovement);

	public static readonly PlayerActionData.ActionGroup GroupToolbelt = new PlayerActionData.ActionGroup("inpGrpToolbeltName", null, 10, PlayerActionData.TabToolbelt);

	public static readonly PlayerActionData.ActionGroup GroupVehicle = new PlayerActionData.ActionGroup("inpGrpVehicleName", null, 15, PlayerActionData.TabVehicle);

	public static readonly PlayerActionData.ActionGroup GroupMenu = new PlayerActionData.ActionGroup("inpGrpMenuName", null, 20, PlayerActionData.TabMenus);

	public static readonly PlayerActionData.ActionGroup GroupDialogs = new PlayerActionData.ActionGroup("inpGrpDialogsName", null, 30, PlayerActionData.TabMenus);

	public static readonly PlayerActionData.ActionGroup GroupUI = new PlayerActionData.ActionGroup("inpGrpUiName", null, 40, PlayerActionData.TabUi);

	public static readonly PlayerActionData.ActionGroup GroupMp = new PlayerActionData.ActionGroup("inpGrpMpName", null, 50, PlayerActionData.TabOther);

	public static readonly PlayerActionData.ActionGroup GroupAdmin = new PlayerActionData.ActionGroup("inpGrpAdminName", null, 60, PlayerActionData.TabOther);

	public static readonly PlayerActionData.ActionGroup GroupGlobalFunctions = new PlayerActionData.ActionGroup("inpGrpGlobalFunctionsName", null, 80, PlayerActionData.TabGlobal);

	public static readonly PlayerActionData.ActionGroup GroupDebugFunctions = new PlayerActionData.ActionGroup("inpGrpDebugFunctionsName", null, 100, PlayerActionData.TabGlobal);

	public static readonly PlayerActionData.ActionGroup GroupEditCamera = new PlayerActionData.ActionGroup("inpGrpCameraName", null, 20, PlayerActionData.TabEdit);

	public static readonly PlayerActionData.ActionGroup GroupEditSelection = new PlayerActionData.ActionGroup("inpGrpSelectionName", null, 40, PlayerActionData.TabEdit);

	public static readonly PlayerActionData.ActionGroup GroupEditOther = new PlayerActionData.ActionGroup("inpGrpOtherName", null, 60, PlayerActionData.TabEdit);

	public enum EAppliesToInputType
	{
		None,
		KbdMouseOnly,
		ControllerOnly,
		Both
	}

	public class ActionSetUserData
	{
		public ActionSetUserData(params PlayerActionsBase[] _bindingsConflictWithSet)
		{
			this.bindingsConflictWithSet = _bindingsConflictWithSet;
		}

		public readonly PlayerActionsBase[] bindingsConflictWithSet;
	}

	public class ActionTab : IComparable<PlayerActionData.ActionTab>
	{
		public ActionTab(string _tabNameKey, int _tabPriority)
		{
			this.tabNameKey = _tabNameKey;
			this.tabPriority = _tabPriority;
		}

		public int CompareTo(PlayerActionData.ActionTab _other)
		{
			return this.tabPriority.CompareTo(_other.tabPriority);
		}

		public string LocalizedName
		{
			get
			{
				return Localization.Get(this.tabNameKey, false, null);
			}
		}

		public readonly string tabNameKey;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int tabPriority;
	}

	public class ActionGroup : IComparable<PlayerActionData.ActionGroup>
	{
		public ActionGroup(string _groupNameKey, string _groupDescKey, int _groupPriority, PlayerActionData.ActionTab _actionTab)
		{
			this.groupNameKey = _groupNameKey;
			this.groupDescKey = _groupDescKey ?? (this.groupNameKey.Replace("Name", "") + "Desc");
			this.groupPriority = _groupPriority;
			this.actionTab = _actionTab;
		}

		public int CompareTo(PlayerActionData.ActionGroup _other)
		{
			return this.groupPriority.CompareTo(_other.groupPriority);
		}

		public string LocalizedName
		{
			get
			{
				return Localization.Get(this.groupNameKey, false, null);
			}
		}

		public string LocalizedDescription
		{
			get
			{
				string text = Localization.Get(this.groupDescKey, false, null);
				if (!(text != this.groupDescKey))
				{
					return null;
				}
				return text;
			}
		}

		public readonly string groupNameKey;

		public readonly string groupDescKey;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int groupPriority;

		public readonly PlayerActionData.ActionTab actionTab;
	}

	public class ActionUserData
	{
		public ActionUserData(string _actionNameKey, string _actionDescKey, PlayerActionData.ActionGroup _actionGroup, PlayerActionData.EAppliesToInputType _appliesToInputType = PlayerActionData.EAppliesToInputType.Both, bool _allowRebind = true, bool _allowMultipleRebindings = false, bool _doNotDisplay = false, bool _defaultOnStartup = true)
		{
			this.actionNameKey = _actionNameKey;
			this.actionDescKey = _actionDescKey ?? (this.actionNameKey.Replace("Name", "") + "Desc");
			this.actionGroup = _actionGroup;
			this.appliesToInputType = _appliesToInputType;
			this.allowRebind = _allowRebind;
			this.allowMultipleBindings = _allowMultipleRebindings;
			this.doNotDisplay = _doNotDisplay;
			this.defaultOnStartup = _defaultOnStartup;
			if (this.actionGroup == null)
			{
				throw new ArgumentNullException("_actionGroup");
			}
		}

		public string LocalizedName
		{
			get
			{
				return Localization.Get(this.actionNameKey, false, null);
			}
		}

		public string LocalizedDescription
		{
			get
			{
				string text = Localization.Get(this.actionDescKey, false, null);
				if (!(text != this.actionDescKey))
				{
					return null;
				}
				return text;
			}
		}

		public readonly string actionNameKey;

		public readonly string actionDescKey;

		public readonly PlayerActionData.ActionGroup actionGroup;

		public readonly PlayerActionData.EAppliesToInputType appliesToInputType;

		public readonly bool allowRebind;

		public readonly bool allowMultipleBindings;

		public readonly bool doNotDisplay;

		public readonly bool defaultOnStartup;
	}
}
