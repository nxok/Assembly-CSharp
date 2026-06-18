using System;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLight : MonoBehaviour
{
	public void AddRendererNameToIgnore(string _name)
	{
		if (this.IgnoreNamedRenderersList == null)
		{
			this.IgnoreNamedRenderersList = new List<string>();
		}
		this.IgnoreNamedRenderersList.Add(_name);
	}

	public void SetTintColorForItem(Vector3 _color)
	{
		Color color;
		color..ctor(_color.x, _color.y, _color.z, 1f);
		base.gameObject.GetComponentsInChildren<Renderer>(UpdateLight.rendererList);
		for (int i = 0; i < UpdateLight.rendererList.Count; i++)
		{
			Renderer renderer = UpdateLight.rendererList[i];
			if (renderer && (this.IgnoreNamedRenderersList == null || !this.IgnoreNamedRenderersList.ContainsCaseInsensitive(renderer.gameObject.name)))
			{
				UpdateLight.SetTintColor(renderer, color);
			}
		}
		UpdateLight.rendererList.Clear();
	}

	public static void SetTintColor(Transform _t, Color _color)
	{
		_t.GetComponentsInChildren<Renderer>(UpdateLight.rendererList);
		for (int i = 0; i < UpdateLight.rendererList.Count; i++)
		{
			UpdateLight.SetTintColor(UpdateLight.rendererList[i], _color);
		}
		UpdateLight.rendererList.Clear();
	}

	public static void SetTintColor(Renderer _r, Color _color)
	{
		Material[] materials = _r.materials;
		if (materials != null)
		{
			foreach (Material material in materials)
			{
				if (material != null)
				{
					material.SetColor("_Color", _color);
					material.SetColor("TintColor", _color);
					material.SetVector("TintColor", _color);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (GameManager.IsDedicatedServer)
		{
			Object.Destroy(this);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
	}

	public void Reset()
	{
		this.appliedLit = -1f;
	}

	public virtual void ManagerFirstUpdate()
	{
		base.gameObject.TryGetComponent<Entity>(ref this.entity);
		this.currentLit = 0.5f;
		this.appliedLit = -1f;
		this.UpdateLighting(1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetFloatProp<T>(string varName, float value, List<T> _rends) where T : Renderer
	{
		_rends[0].GetPropertyBlock(UpdateLight.props);
		UpdateLight.props.SetFloat(varName, value);
		for (int i = 0; i < _rends.Count; i++)
		{
			_rends[i].SetPropertyBlock(UpdateLight.props);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLit(float _lit)
	{
		this.appliedLit = _lit;
		GameObject gameObject = base.gameObject;
		gameObject.GetComponentsInChildren<MeshRenderer>(true, UpdateLight.meshRendererList);
		gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true, UpdateLight.skinnedMeshRendererList);
		if (UpdateLight.props == null)
		{
			UpdateLight.props = new MaterialPropertyBlock();
		}
		if (UpdateLight.meshRendererList.Count > 0)
		{
			this.SetFloatProp<MeshRenderer>("_MacroAO", _lit, UpdateLight.meshRendererList);
			UpdateLight.meshRendererList.Clear();
		}
		if (UpdateLight.skinnedMeshRendererList.Count > 0)
		{
			this.SetFloatProp<SkinnedMeshRenderer>("_MacroAO", _lit, UpdateLight.skinnedMeshRendererList);
			UpdateLight.skinnedMeshRendererList.Clear();
		}
	}

	public void UpdateLighting(float _step)
	{
		if (this.entity)
		{
			this.targetLit = this.entity.GetLightBrightness();
		}
		else
		{
			this.targetLit = 1f;
			Vector3i vector3i = World.worldToBlockPos(base.transform.position + Origin.position);
			if (vector3i.y < 255)
			{
				IChunk chunkFromWorldPos = GameManager.Instance.World.GetChunkFromWorldPos(vector3i);
				if (chunkFromWorldPos != null)
				{
					float num = (float)chunkFromWorldPos.GetLight(vector3i.x, vector3i.y, vector3i.z, Chunk.LIGHT_TYPE.SUN);
					float num2 = (float)chunkFromWorldPos.GetLight(vector3i.x, vector3i.y + 1, vector3i.z, Chunk.LIGHT_TYPE.SUN);
					this.targetLit = Utils.FastMax(num, num2);
					this.targetLit /= 15f;
				}
			}
		}
		this.currentLit = Mathf.MoveTowards(this.currentLit, this.targetLit, _step);
		if (this.currentLit != this.appliedLit)
		{
			this.ApplyLit(this.currentLit);
		}
	}

	public bool IsDynamicObject;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentLit;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetLit;

	public float appliedLit = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> IgnoreNamedRenderersList;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Entity entity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<Renderer> rendererList = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<MeshRenderer> meshRendererList = new List<MeshRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<SkinnedMeshRenderer> skinnedMeshRendererList = new List<SkinnedMeshRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static MaterialPropertyBlock props;
}
