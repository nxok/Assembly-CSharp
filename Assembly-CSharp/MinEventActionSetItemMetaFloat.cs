using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetItemMetaFloat : MinEventActionBase
{
	public override void Execute(MinEventParams _params)
	{
		ItemValue itemValue = _params.ItemValue;
		if (!itemValue.HasMetadata(this.metaKey, TypedMetadataValue.TypeTag.None))
		{
			itemValue.SetMetadata(this.metaKey, 0f);
		}
		object metadata = itemValue.GetMetadata(this.metaKey);
		if (metadata is float)
		{
			float num = (float)metadata;
			if (this.relative)
			{
				itemValue.SetMetadata(this.metaKey, num + this.change);
			}
			else
			{
				itemValue.SetMetadata(this.metaKey, this.change);
			}
			if (num < 0f)
			{
				itemValue.SetMetadata(this.metaKey, 0f);
			}
			return;
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && !string.IsNullOrEmpty(this.metaKey);
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "change")
			{
				this.change = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				return true;
			}
			if (localName == "relative")
			{
				this.relative = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
			if (localName == "key")
			{
				this.metaKey = _attribute.Value;
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float change;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool relative = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string metaKey;
}
