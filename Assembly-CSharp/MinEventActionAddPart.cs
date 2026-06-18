using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddPart : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (_params.Self == null)
		{
			return;
		}
		string propertyOverride = _params.ItemValue.GetPropertyOverride(this.itemPropertyName, "");
		if (this.goToInstantiate == null && propertyOverride == "")
		{
			return;
		}
		if (propertyOverride != "")
		{
			this.goToInstantiate = DataLoader.LoadAsset<GameObject>(propertyOverride, false);
		}
		Transform transform = _params.Self.RootTransform;
		if (!string.IsNullOrEmpty(this.parentTransformPath))
		{
			if (this.parentTransformPath.EqualsCaseInsensitive("#HeldItemRoot") && _params.Self.emodel != null)
			{
				transform = _params.Self.inventory.models[_params.Self.inventory.holdingItemIdx];
				if (transform == null)
				{
					transform = GameUtils.FindDeepChildActive(_params.Self.RootTransform, this.parentTransformPath);
				}
			}
			else
			{
				EntityPlayerLocal entityPlayerLocal = _params.Self as EntityPlayerLocal;
				if (entityPlayerLocal != null && entityPlayerLocal.emodel.IsFPV && entityPlayerLocal.vp_FPCamera.Locked3rdPerson)
				{
					transform = GameUtils.FindDeepChildActive(entityPlayerLocal.vp_FPCamera.Transform, this.parentTransformPath);
				}
				else
				{
					transform = GameUtils.FindDeepChildActive(_params.Self.RootTransform, this.parentTransformPath);
				}
			}
		}
		if (transform == null)
		{
			return;
		}
		if (int.Parse(_params.ItemValue.GetPropertyOverride(ItemClass.PropMatEmission, "0")) > 0)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
			for (int i = componentsInChildren.Length - 1; i >= 0; i--)
			{
				componentsInChildren[i].material.EnableKeyword("_EMISSION");
			}
		}
		string text = string.Format("part!" + this.goToInstantiate.name, Array.Empty<object>());
		Transform transform2 = GameUtils.FindDeepChild(transform, text);
		if (transform2 == null)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.goToInstantiate);
			if (gameObject == null)
			{
				return;
			}
			transform2 = gameObject.transform;
			gameObject.name = text;
			Utils.SetLayerRecursively(gameObject, transform.gameObject.layer);
			transform2.SetParent(transform, false);
			transform2.SetLocalPositionAndRotation(this.localPos, Quaternion.Euler(this.localRot.x, this.localRot.y, this.localRot.z));
		}
		if (this.colorTint && transform2 != null)
		{
			UpdateLightOnAllMaterials updateLightOnAllMaterials = NGUITools.AddMissingComponent<UpdateLightOnAllMaterials>(transform2.gameObject);
			string @string = _params.ItemValue.ItemClass.Properties.GetString(Block.PropTintColor);
			if (@string.Length > 0)
			{
				updateLightOnAllMaterials.SetTintColorForItem(Block.StringToVector3(_params.ItemValue.GetPropertyOverride(Block.PropTintColor, @string)));
			}
			else
			{
				updateLightOnAllMaterials.SetTintColorForItem(Block.StringToVector3(_params.ItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255")));
			}
		}
		_params.Self.AddPart(this.partName, transform2);
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && _params.Self != null && (this.goToInstantiate != null || this.itemPropertyName != null);
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "part")
			{
				this.partName = _attribute.Value;
				return true;
			}
			if (localName == "prefab")
			{
				this.itemPropertyName = _attribute.Value;
				if (!this.itemPropertyName.StartsWith("property?"))
				{
					this.goToInstantiate = DataLoader.LoadAsset<GameObject>(_attribute.Value, false);
				}
				else
				{
					this.itemPropertyName = this.itemPropertyName.Replace("property?", "");
				}
				return true;
			}
			if (localName == "parentTransform")
			{
				this.parentTransformPath = _attribute.Value;
				return true;
			}
			if (localName == "localPos")
			{
				this.localPos = StringParsers.ParseVector3(_attribute.Value, 0, -1);
				return true;
			}
			if (localName == "localRot")
			{
				this.localRot = StringParsers.ParseVector3(_attribute.Value, 0, -1);
				return true;
			}
			if (localName == "colorTint")
			{
				this.colorTint = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string partName;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goToInstantiate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string itemPropertyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string parentTransformPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 localPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 localRot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool colorTint;
}
