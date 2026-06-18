using System;
using UnityEngine;

public class CloneToTransform : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.m_transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.m_parentEntity = base.GetComponentInParent<Entity>();
		if (this.m_parentEntity != null)
		{
			this.m_hasParentEntity = true;
		}
		if (this.m_parentEntity is EntityPlayerLocal)
		{
			this.m_hasParentEntityLocal = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.DestroyClone();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyClone()
	{
		if (!this.m_clone)
		{
			return;
		}
		this.m_cloneTransform = null;
		Object.Destroy(this.m_clone);
		this.m_clone = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (this.m_clone)
		{
			this.m_clone.SetActive(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (this.m_clone)
		{
			this.m_clone.SetActive(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (!this.m_storage)
		{
			this.m_storage = Object.Instantiate<GameObject>(base.gameObject, this.m_transform, true);
			this.m_storageTransform = this.m_storage.transform;
			this.m_storageTransform.name = this.m_transform.name + "(CloneToTransform)";
			this.m_storage.SetActive(false);
			CloneToTransform cloneToTransform;
			if (this.m_storage.TryGetComponent<CloneToTransform>(ref cloneToTransform))
			{
				Object.Destroy(cloneToTransform);
			}
			foreach (object obj in this.m_transform)
			{
				Transform transform = (Transform)obj;
				if (!(transform == this.m_storageTransform))
				{
					Object.Destroy(transform.gameObject);
				}
			}
			foreach (Component component in base.GetComponents<Component>())
			{
				if (!(component == this) && !(component is Transform))
				{
					Object.Destroy(component);
				}
			}
		}
		Transform transform2 = null;
		if ((this.m_hasParentEntityLocal || !this.m_hasParentEntity) && Camera.main)
		{
			transform2 = Camera.main.transform;
		}
		else if (this.m_parentEntity != null && this.m_parentEntity.emodel != null)
		{
			transform2 = this.m_parentEntity.emodel.GetModelTransformParent();
		}
		if (!transform2)
		{
			this.DestroyClone();
			this.m_lastCloneTarget = null;
			return;
		}
		if (!this.m_clone)
		{
			this.m_lastCloneTarget = transform2;
			this.m_clone = Object.Instantiate<GameObject>(this.m_storage, transform2, true);
			this.m_cloneTransform = this.m_clone.transform;
			this.m_cloneTransform.name = this.m_transform.name + "(clone)";
			this.m_clone.SetActive(true);
			CloneToTransform cloneToTransform2;
			if (this.m_clone.TryGetComponent<CloneToTransform>(ref cloneToTransform2))
			{
				Object.Destroy(cloneToTransform2);
			}
		}
		if (this.m_lastCloneTarget != transform2)
		{
			this.m_lastCloneTarget = transform2;
			this.m_cloneTransform.parent = transform2;
			this.m_lastLocalPosition = default(Vector3);
			this.m_lastLocalRotation = default(Quaternion);
		}
		this.CheckTransform();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckTransform()
	{
		if (this.m_lastLocalPosition == this.m_transform.localPosition && this.m_lastLocalRotation == this.m_transform.localRotation)
		{
			return;
		}
		this.m_lastLocalPosition = this.m_transform.localPosition;
		this.m_lastLocalRotation = this.m_transform.localRotation;
		this.m_cloneTransform.SetPositionAndRotation(this.m_transform.position, this.m_transform.rotation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_transform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject m_storage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_storageTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject m_clone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_cloneTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_lastCloneTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_lastLocalPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion m_lastLocalRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_hasParentEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_hasParentEntityLocal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Entity m_parentEntity;
}
