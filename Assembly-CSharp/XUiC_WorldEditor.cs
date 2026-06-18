using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldEditor : XUiC_EditingToolsDialogBase
{
	[XuiXmlBinding("startable")]
	public bool Startable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_WorldList xuiC_WorldList = this.worldList;
			return xuiC_WorldList != null && xuiC_WorldList.SelectedEntryIndex >= 0;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_WorldEditor.ID = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.worldList.SetUserDataStorageTypeFilter(new UserDataStorageType[1]);
		this.worldList.RebuildList(false);
		if (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.GameWorld)) || !this.worldList.SelectByName(GamePrefs.GetString(EnumGamePrefs.GameWorld)))
		{
			this.worldList.SelectedEntryIndex = 0;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent ?? this.children[0].ViewComponent, false) && this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			this.BtnStart_OnPressed(this, 0);
		}
	}

	[XuiBindEvent("SelectionChanged", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_List<XUiC_WorldList.WorldListEntry> _list, XUiC_WorldList.WorldListEntry _previousEntry, XUiC_WorldList.WorldListEntry _newEntry)
	{
		this.IsDirty = true;
	}

	[XuiBindEvent("ListEntryDoubleClicked", "worldList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnEntryDoubleClicked(XUiC_List<XUiC_WorldList.WorldListEntry> _list, XUiC_WorldList.WorldListEntry _entry)
	{
		if (this.worldList.SelectedEntryIndex >= 0)
		{
			this.BtnStart_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("OnPress", "btnDataManagement")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDataManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, new Action(this.OnDataManagementWindowClosed));
	}

	[XuiBindEvent("OnPress", "btnCreate")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCreateManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_WorldEditorCreateWorld.Open(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		this.worldList.RebuildList(false);
	}

	[XuiBindEvent("OnPress", "btnStart")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnStart.ViewComponent.Enabled)
		{
			return;
		}
		new GameModeEditWorld().ResetGamePrefs();
		PathAbstractions.AbstractedLocation selectedWorld = this.worldList.SelectedEntryData.Location;
		GamePrefs.Set(EnumGamePrefs.GameWorld, selectedWorld.Name);
		if (XUiC_WorldEditor.canSaveWorldIn(selectedWorld))
		{
			this.StartEditor();
			return;
		}
		string text = selectedWorld.Name + "_UserCopy";
		PathAbstractions.AbstractedLocation newLocation = XUiC_WorldEditor.LocationForNewWorld(text);
		int num = 0;
		while (newLocation.Exists())
		{
			string name = selectedWorld.Name;
			string text2 = "_UserCopy";
			int num2;
			num = (num2 = num + 1);
			text = name + text2 + num2.ToString();
			newLocation = XUiC_WorldEditor.LocationForNewWorld(text);
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, text);
		XUiC_MessageBoxWindowGroup.ShowOkCancel(this.xui, Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderTitle", false, null), string.Format(Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderText", false, null), GameIO.GetOsStylePath(newLocation.FullPath)), delegate
		{
			Log.Out(string.Concat(new string[] { "Will copy the world from '", selectedWorld.FullPath, "' to '", newLocation.FullPath, "' for editing." }));
			GameIO.CopyDirectory(selectedWorld.FullPath, newLocation.FullPath);
			this.StartEditor();
		}, delegate
		{
			this.xui.playerUI.windowManager.Open(this.windowGroup, true);
		}, false, true, false);
	}

	public void StartEditor()
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "WorldEditor");
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 0);
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			XUiC_MessageBoxWindowGroup.ShowNetworkError(this.xui, networkConnectionError, false);
		}
	}

	public static PathAbstractions.AbstractedLocation LocationForNewWorld(string _name)
	{
		PathAbstractions.EAbstractedLocationType eabstractedLocationType = PathAbstractions.EAbstractedLocationType.UserDataPath;
		return PathAbstractions.WorldsSearchPaths.BuildLocation(eabstractedLocationType, null, _name, null, new UserDataStorageType?(UserDataStorageType.DeviceLocal)).Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canSaveWorldIn(PathAbstractions.AbstractedLocation _location)
	{
		return _location.Type != PathAbstractions.EAbstractedLocationType.GameData;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const UserDataStorageType AllowedUserDataStorageType = UserDataStorageType.DeviceLocal;

	[XuiBindComponent("btnDataManagement", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDataManagement;

	[XuiBindComponent("btnStart", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnStart;

	[XuiBindComponent("btnCreate", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCreate;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldList worldList;
}
