using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerProfile : XUiC_PlayGameDialogBase
{
	public override void Init()
	{
		base.Init();
		XUiC_PlayerProfile.ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnProfileEdit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileEdit_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.openCustomCharacterWindow();
	}

	[XuiBindEvent("OnPress", "btnProfileDelete")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_ProfilesList.ListEntry selectedEntry = this.profiles.SelectedEntryData;
		Action <>9__1;
		Action <>9__2;
		XUiC_MessageBoxWindowGroup.ShowCustom(this.xui, Localization.Get("xuiDeleteProfile", false, null), string.Format(Localization.Get("xuiProfilesDeleteConfirmation", false, null), Utils.EscapeBbCodes(selectedEntry.Name, false, false)), delegate(XUiC_MessageBoxWindowGroup _box)
		{
			XUiC_MessageBoxWindowGroup.MessageButton messageButton = _box.Buttons[0];
			string text = "btnConfirm";
			Action action;
			if ((action = <>9__1) == null)
			{
				action = (<>9__1 = delegate
				{
					this.btnProfileDelete.SelectCursorElement(false, false);
					ProfileSDF.DeleteProfile(selectedEntry.Name);
					ProfileSDF.SetSelectedProfile("");
					this.profiles.RebuildList(false);
					this.selectFirstProfile();
					this.IsDirty = true;
				});
			}
			messageButton.DefaultConfirm(text, action, true, 0f, 1.5f, false);
			XUiC_MessageBoxWindowGroup.MessageButton messageButton2 = _box.Buttons[2];
			string text2 = "xuiCancel";
			Action action2;
			if ((action2 = <>9__2) == null)
			{
				action2 = (<>9__2 = delegate
				{
					this.btnProfileDelete.SelectCursorElement(false, false);
				});
			}
			messageButton2.DefaultCancel(text2, action2, true, 0f, 0f, false);
		}, false, false, -1);
	}

	[XuiBindEvent("OnPress", "btnProfileCreate")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnProfileCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_PlayerProfileCreate.Open(this.xui, delegate(string _name)
		{
			if (_name == null)
			{
				this.btnProfileCreate.SelectCursorElement(false, false);
				return;
			}
			ProfileSDF.SaveProfile(_name, "", true, "White", 1, "Blue01", "", "", "", "", "");
			ProfileSDF.SetSelectedProfile(_name);
			ProfileSDF.Save();
			this.profiles.RebuildList(false);
			this.openCustomCharacterWindow();
		});
		this.xui.playerUI.windowManager.Open("playerProfilesCreate", false);
	}

	[XuiBindEvent("SelectionChanged", "profiles")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Profiles_OnSelectionChanged(XUiC_List<XUiC_ProfilesList.ListEntry> _list, XUiC_ProfilesList.ListEntry _previousEntry, XUiC_ProfilesList.ListEntry _newEntry)
	{
		if (_newEntry != null)
		{
			ProfileSDF.SetSelectedProfile(_newEntry.Name);
			ProfileSDF.Save();
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openCustomCharacterWindow()
	{
		this.previewWindow.ViewComponent.IsVisible = false;
		XUiC_CustomCharacterWindowGroup.Open(this.xui, delegate
		{
			this.previewWindow.ViewComponent.IsVisible = true;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectFirstProfile()
	{
		string text = ProfileSDF.CurrentProfileName();
		if (!string.IsNullOrEmpty(text))
		{
			return;
		}
		using (IEnumerator<XUiC_ProfilesList.ListEntry> enumerator = this.profiles.AllEntries().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				text = enumerator.Current.Name;
				this.profiles.SelectByName(text);
			}
		}
		ProfileSDF.SetSelectedProfile(text);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.selectFirstProfile();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.onCloseAction == null)
		{
			return;
		}
		this.onCloseAction();
		this.onCloseAction = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	[XuiXmlBinding("user_profile_count")]
	public int UserProfileCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.profiles == null)
			{
				return 0;
			}
			int num = 0;
			using (IEnumerator<XUiC_ProfilesList.ListEntry> enumerator = this.profiles.AllEntries().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (Archetype.GetArchetype(PlayerProfile.LoadProfile(enumerator.Current.Name).ProfileArchetype).CanCustomize)
					{
						num++;
					}
				}
			}
			return num;
		}
	}

	[XuiXmlBinding("can_create_userprofile")]
	public bool MaxUserProfilesReached
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return true;
		}
	}

	[XuiXmlBinding("can_modify_profile")]
	public bool CanModifyProfile
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.profiles == null)
			{
				return false;
			}
			bool flag = !string.IsNullOrEmpty(ProfileSDF.CurrentProfileName());
			bool flag2 = this.profiles.SelectedEntryData != null;
			bool flag3 = flag && flag2;
			if (flag3)
			{
				PlayerProfile playerProfile = PlayerProfile.LoadLocalProfile();
				Archetype archetype;
				if ((archetype = Archetype.GetArchetype(playerProfile.ProfileArchetype)) == null)
				{
					archetype = Archetype.GetArchetype(playerProfile.IsMale ? "BaseMale" : "BaseFemale");
				}
				Archetype archetype2 = archetype;
				if (archetype2 != null)
				{
					flag3 &= archetype2.CanCustomize;
				}
			}
			return flag3;
		}
	}

	public static void Open(XUi _xuiInstance, Action _onCloseAction = null)
	{
		_xuiInstance.GetChildByType<XUiC_PlayerProfile>().onCloseAction = _onCloseAction;
		_xuiInstance.playerUI.windowManager.Open(XUiC_PlayerProfile.ID, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxUserProfiles = -1;

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ProfilesList profiles;

	[XuiBindComponent("btnProfileCreate", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnProfileCreate;

	[XuiBindComponent("btnProfileDelete", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnProfileDelete;

	[XuiBindComponent("btnProfileEdit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnProfileEdit;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SDCSPreviewWindow previewWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseAction;
}
