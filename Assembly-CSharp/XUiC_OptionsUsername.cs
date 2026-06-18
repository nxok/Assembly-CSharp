using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsUsername : XUiC_OptionsDialogBase
{
	public override void Init()
	{
		base.Init();
		XUiC_OptionsUsername.ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnSubmitHandler", "txtUsername")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtUsername_OnSubmitHandler(XUiController _sender, string _text)
	{
		base.Event_ApplyOnPress(_sender, -1);
	}

	[XuiBindEvent("OnChangeHandler", "txtUsername")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtUsername_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_changeFromCode)
		{
			return;
		}
		base.SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doSaveChangesInternal()
	{
		base.doSaveChangesInternal();
		GamePrefs.Set(EnumGamePrefs.PlayerName, this.txtUsername.Text);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtUsername.Text = GamePrefs.GetString(EnumGamePrefs.PlayerName);
	}

	public override bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtUsername;
}
