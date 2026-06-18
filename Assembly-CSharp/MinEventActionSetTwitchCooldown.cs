using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetTwitchCooldown : MinEventActionTargetedBase
{
	public EntityPlayer.TwitchActionsStates state
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (this.targets == null)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			EntityPlayer entityPlayer = this.targets[i] as EntityPlayer;
			if (entityPlayer != null && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled)
			{
				entityPlayer.TwitchActionsEnabled = this.state;
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "state")
		{
			this.state = (EntityPlayer.TwitchActionsStates)StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
			if (this.state == EntityPlayer.TwitchActionsStates.Disabled)
			{
				this.state = EntityPlayer.TwitchActionsStates.TempDisabled;
			}
		}
		return flag;
	}
}
