using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityNewStyleAvatar : Entity
{
	public void EnableSubmesh(string submeshName, bool enable)
	{
		Transform transform = base.transform;
		Transform transform2 = transform.Find("Graphics/Model");
		if (transform2 == null)
		{
			transform2 = transform;
		}
		Transform transform3 = transform2.Find("base");
		if (transform3 != null)
		{
			int childCount = transform3.childCount;
			for (int i = 0; i < childCount; i++)
			{
				GameObject gameObject = transform3.GetChild(i).gameObject;
				if (gameObject.name == submeshName)
				{
					gameObject.SetActive(enable);
				}
			}
		}
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
		Transform transform = base.transform;
		Transform transform2 = transform.Find("Graphics/Model");
		if (transform2 == null)
		{
			transform2 = transform;
		}
		Transform transform3 = null;
		if (transform2 != null)
		{
			transform3 = DataLoader.LoadAsset<Transform>("@:Entities/Player/Male/maleTestPrefab.prefab", false);
			if (transform3 != null)
			{
				transform3 = Object.Instantiate<Transform>(transform3, transform2);
				transform3.name = "base";
			}
		}
		if (transform3)
		{
			int childCount = transform3.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform3.GetChild(i);
				Renderer component = child.GetComponent<Renderer>();
				if (!(component == null) && component.sharedMaterials != null)
				{
					child.gameObject.SetActive(false);
				}
			}
		}
		base.gameObject.AddComponent<NewAvatarRootMotion>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, EntityNewStyleAvatar.BodySlot> m_entitySlots = new Dictionary<string, EntityNewStyleAvatar.BodySlot>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public class StringTags
	{
		public void AddTag(string tag)
		{
			this.tags.Add(tag);
		}

		public bool HasTag(string tag)
		{
			return this.tags.Contains(tag);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<string> tags;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class BodySlot
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public string submeshName;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityNewStyleAvatar.StringTags tags;
	}
}
