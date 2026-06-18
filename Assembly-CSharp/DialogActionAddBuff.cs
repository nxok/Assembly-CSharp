using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogActionAddBuff : BaseDialogAction
{
	public override BaseDialogAction.ActionTypes ActionType
	{
		get
		{
			return BaseDialogAction.ActionTypes.AddBuff;
		}
	}

	public override void PerformAction(EntityPlayer player)
	{
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			EntityBuffs.BuffStatus buffStatus = primaryPlayer.Buffs.AddBuff(base.ID, -1, true, false, -1f);
			if (buffStatus != EntityBuffs.BuffStatus.Added)
			{
				switch (buffStatus)
				{
				case EntityBuffs.BuffStatus.FailedInvalidName:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: buff \"" + base.ID + "\" unknown");
					return;
				case EntityBuffs.BuffStatus.FailedImmune:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: entity is immune to \"" + base.ID);
					return;
				case EntityBuffs.BuffStatus.FailedFriendlyFire:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: entity is friendly");
					return;
				case EntityBuffs.BuffStatus.FailedEditor:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: buff " + base.ID + " not allowed in editor.");
					return;
				case EntityBuffs.BuffStatus.FailedGameStat:
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: missing required game stat.");
					break;
				default:
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}
