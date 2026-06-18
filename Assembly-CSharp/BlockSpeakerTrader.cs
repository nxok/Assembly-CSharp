using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpeakerTrader : Block
{
	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("OpenSound"))
		{
			this.openSound = base.Properties.Values["OpenSound"];
		}
		if (base.Properties.Values.ContainsKey("CloseSound"))
		{
			this.closeSound = base.Properties.Values["CloseSound"];
		}
		if (base.Properties.Values.ContainsKey("WarningSound"))
		{
			this.warningSound = base.Properties.Values["WarningSound"];
		}
	}

	public void PlayOpen(Vector3i _blockPos, EntityTrader _trader)
	{
		string text = this.openSound;
		if (string.IsNullOrEmpty(text))
		{
			text = ((_trader != null) ? (_trader.NPCInfo.VoiceSet + "_announce_open") : "");
		}
		if (text != "")
		{
			Manager.BroadcastPlay(_blockPos.ToVector3(), text, 0f);
		}
	}

	public void PlayClose(Vector3i _blockPos, EntityTrader _trader)
	{
		string text = this.closeSound;
		if (string.IsNullOrEmpty(text))
		{
			text = ((_trader != null) ? (_trader.NPCInfo.VoiceSet + "_announce_closed") : "");
		}
		if (text != "")
		{
			Manager.BroadcastPlay(_blockPos.ToVector3(), text, 0f);
		}
	}

	public void PlayWarning(Vector3i _blockPos, EntityTrader _trader)
	{
		string text = this.warningSound;
		if (string.IsNullOrEmpty(text))
		{
			text = ((_trader != null) ? (_trader.NPCInfo.VoiceSet + "_announce_closing") : "");
		}
		if (text != "")
		{
			Manager.BroadcastPlay(_blockPos.ToVector3(), text, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string openSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string closeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warningSound;
}
