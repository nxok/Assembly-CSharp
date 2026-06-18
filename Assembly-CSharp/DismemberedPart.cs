using System;
using System.Collections.Generic;
using UnityEngine;

public class DismemberedPart
{
	public void SetDetachedTransform(Transform _detach, Transform _group)
	{
		this.detachT = _detach;
		this.detachRenderer = _group.GetComponentInChildren<MeshRenderer>();
	}

	public void FadeDetached(float value)
	{
		if (this.detachRenderer)
		{
			if (!this.fadeOutInit)
			{
				this.detachMats.Clear();
				this.detachMats.AddRange(this.detachRenderer.materials);
				this.fadeOutInit = true;
			}
			for (int i = 0; i < this.detachMats.Count; i++)
			{
				Material material = this.detachMats[i];
				if (material.HasProperty("_Fade"))
				{
					material.SetFloat("_Fade", value);
				}
			}
			this.detachRenderer.SetMaterials(this.detachMats);
		}
	}

	public DismemberedPart(DismemberedPartData _data, uint _bodyDamageFlag, EnumDamageTypes _damageType)
	{
		this.Data = _data;
		this.bodyDamageFlag = _bodyDamageFlag;
		this.damageType = _damageType;
	}

	public string prefabPath
	{
		get
		{
			return this.Data.prefabPath;
		}
	}

	public void Update()
	{
		this.elapsedTime += Time.deltaTime;
		if (this.pivotT && this.overrideHeadSize != 1f)
		{
			if (!this.startValuesSet)
			{
				this.startingHeadSize = this.overrideHeadSize;
				this.startingScale = this.pivotT.localScale;
				this.startValuesSet = true;
			}
			float num = this.elapsedTime / this.overrideHeadDismemberScaleTime;
			this.overrideHeadSize = Mathf.Lerp(this.startingHeadSize, 1f, num);
			this.pivotT.localScale = Vector3.Lerp(this.startingScale, Vector3.one, num);
		}
		if (!this.ReadyForCleanup)
		{
			if (this.elapsedTime > this.lifeTime - 0.5f)
			{
				this.fadeTime += Time.deltaTime;
				this.FadeDetached(Mathf.Lerp(1f, 0f, this.fadeTime / 0.5f));
			}
			if (this.elapsedTime >= this.lifeTime)
			{
				this.ReadyForCleanup = true;
			}
		}
	}

	public void Hide()
	{
		if (this.prefabT)
		{
			this.prefabT.gameObject.SetActive(false);
		}
	}

	public void CleanupDetached()
	{
		if (this.detachT)
		{
			Object.Destroy(this.detachT.gameObject);
			this.detachT = null;
		}
	}

	public DismemberedPartData Data;

	public uint bodyDamageFlag;

	public EnumDamageTypes damageType;

	public Transform prefabT;

	public Transform detachT;

	public bool ReadyForCleanup;

	public float lifeTime = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float elapsedTime;

	public Transform targetT;

	public Transform pivotT;

	public float overrideHeadSize = 1f;

	public float overrideHeadDismemberScaleTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startValuesSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public float startingHeadSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startingScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer detachRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> detachMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cFadeTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fadeOutInit;
}
