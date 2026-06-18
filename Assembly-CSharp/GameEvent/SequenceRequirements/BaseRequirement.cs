using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnInit()
		{
		}

		public void Init()
		{
			this.OnInit();
		}

		public virtual bool CanPerform(Entity target)
		{
			return true;
		}

		public virtual void ParseProperties(DynamicProperties properties)
		{
			this.Properties = properties;
			this.Owner.HandleVariablesForProperties(properties);
			if (properties.Values.ContainsKey(BaseRequirement.PropInvert))
			{
				this.Invert = StringParsers.ParseBool(properties.Values[BaseRequirement.PropInvert], 0, -1, true);
			}
		}

		public virtual BaseRequirement Clone()
		{
			BaseRequirement baseRequirement = this.CloneChildSettings();
			if (this.Properties != null)
			{
				baseRequirement.Properties = new DynamicProperties();
				baseRequirement.Properties.CopyFrom(this.Properties, null);
			}
			return baseRequirement;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual BaseRequirement CloneChildSettings()
		{
			return null;
		}

		public DynamicProperties Properties;

		public GameEventActionSequence Owner;

		public bool Invert;

		public static string PropInvert = "invert";
	}
}
