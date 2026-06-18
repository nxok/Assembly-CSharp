using System;

public class vp_FPWeaponHandler : vp_WeaponHandler
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_AutoReload()
	{
		return this.ReloadAutomatically && this.m_Player.Reload.TryStart(true);
	}
}
