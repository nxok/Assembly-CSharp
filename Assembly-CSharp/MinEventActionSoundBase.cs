using System;
using System.Xml.Linq;

public class MinEventActionSoundBase : MinEventActionTargetedBase
{
	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			uint num = <PrivateImplementationDetails>.ComputeStringHash(localName);
			if (num <= 1866706639U)
			{
				if (num != 235771284U)
				{
					if (num != 681419758U)
					{
						if (num != 1866706639U)
						{
							return flag;
						}
						if (!(localName == "silent_on_equip"))
						{
							return flag;
						}
						this.silentOnEquip = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
						return true;
					}
					else
					{
						if (!(localName == "toggle_dms"))
						{
							return flag;
						}
						this.toggleDMS = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
						return true;
					}
				}
				else if (!(localName == "sound"))
				{
					return flag;
				}
			}
			else if (num <= 2660347774U)
			{
				if (num != 1956785724U)
				{
					if (num != 2660347774U)
					{
						return flag;
					}
					if (!(localName == "play_in_head"))
					{
						return flag;
					}
					this.localPlayerOnly = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
					return true;
				}
				else
				{
					if (!(localName == "play_at_self"))
					{
						return flag;
					}
					this.playAtSelf = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
					return true;
				}
			}
			else if (num != 2992025991U)
			{
				if (num != 3723446379U)
				{
					return flag;
				}
				if (!(localName == "loop"))
				{
					return flag;
				}
				this.loop = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
			else if (!(localName == "soundGroup"))
			{
				return flag;
			}
			this.soundGroup = _attribute.Value.Trim();
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundGroupForTarget()
	{
		int num = this.soundGroup.IndexOfAny(MinEventActionSoundBase.convertChars);
		if (num < 0)
		{
			return this.soundGroup;
		}
		if (this.soundGroup[num] == '#')
		{
			return this.soundGroup.Replace("#", this.targets[0].IsMale ? "1" : "2");
		}
		return this.soundGroup.Replace("$", this.targets[0].IsMale ? "Male" : "Female");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundGroup;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool localPlayerOnly;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool loop;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool toggleDMS;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool playAtSelf;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool silentOnEquip;

	[PublicizedFrom(EAccessModifier.Private)]
	public static char[] convertChars = new char[] { '#', '$' };
}
