using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_OptionEntryGamePrefAbs : XUiC_OptionEntryComboAbs
{
	public EnumGamePrefs? GamePref
	{
		get
		{
			return this.gamePref;
		}
	}

	public abstract GamePrefs.EnumType expectedPrefType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public abstract object SelectedValue { get; set; }

	public override void Init()
	{
		base.Init();
		this.parseGamePref();
	}

	public sealed override void ApplySelection()
	{
		if (this.gamePref == null)
		{
			return;
		}
		if (this.DoSaveOverride != null && !this.DoSaveOverride())
		{
			XUiC_OptionEntryAbs.DebugLog(string.Format("*NOT* SAVING VALUE FOR {0}, disabled by override", this.gamePref));
			return;
		}
		this.applySelectionInternal();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void applySelectionInternal();

	[PublicizedFrom(EAccessModifier.Private)]
	public void parseGamePref()
	{
		if (string.IsNullOrEmpty(base.PrefName))
		{
			Log.Error("[XUi] " + base.GetType().Name + ": No GamePref specified. Hierarchy: " + base.GetXuiHierarchy());
			return;
		}
		EnumGamePrefs enumGamePrefs;
		if (!EnumUtils.TryParse<EnumGamePrefs>(base.PrefName, out enumGamePrefs, true))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] ",
				base.GetType().Name,
				": Given GamePref '",
				base.PrefName,
				"' does not exist. Hierarchy: ",
				base.GetXuiHierarchy()
			}));
			return;
		}
		GamePrefs.EnumType? prefType = GamePrefs.GetPrefType(enumGamePrefs);
		if (prefType == null)
		{
			Log.Error(string.Format("[XUi] {0}: Given GamePref '{1}' does not have a pref type. Hierarchy: {2}", base.GetType().Name, enumGamePrefs, base.GetXuiHierarchy()));
			return;
		}
		GamePrefs.EnumType expectedPrefType = this.expectedPrefType;
		if (prefType.Value != expectedPrefType)
		{
			Log.Error(string.Format("[XUi] {0}: Given GamePref '{1}' is not a {2} pref. Hierarchy: {3}", new object[]
			{
				base.GetType().Name,
				enumGamePrefs,
				expectedPrefType.ToStringCached<GamePrefs.EnumType>(),
				base.GetXuiHierarchy()
			}));
			return;
		}
		this.gamePref = new EnumGamePrefs?(enumGamePrefs);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionEntryGamePrefAbs()
	{
	}

	public Func<bool> DoSaveOverride;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumGamePrefs? gamePref;
}
