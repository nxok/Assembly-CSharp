using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockEntityData
{
	public BlockEntityData()
	{
	}

	public BlockEntityData(BlockValue _blockValue, Vector3i _pos)
	{
		this.pos = _pos;
		this.blockValue = _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getRenderers()
	{
		if (this.matPropBlock == null)
		{
			this.matPropBlock = new MaterialPropertyBlock();
		}
		if (this.renderers != null)
		{
			this.renderers.Clear();
		}
		else
		{
			this.renderers = new List<Renderer>();
		}
		this.transform.GetComponentsInChildren<Renderer>(true, this.renderers);
	}

	public List<Renderer> GetRenderers()
	{
		return this.renderers;
	}

	public void Cleanup()
	{
		if (this.renderers != null)
		{
			this.renderers.Clear();
		}
	}

	public void SetMaterialColor(string name, Color value)
	{
		this.getRenderers();
		if (this.renderers == null)
		{
			return;
		}
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		for (int i = 0; i < this.renderers.Count; i++)
		{
			if (this.renderers[i] != null)
			{
				this.renderers[i].GetPropertyBlock(this.matPropBlock);
				this.matPropBlock.SetColor(name, value);
				this.renderers[i].SetPropertyBlock(this.matPropBlock);
			}
		}
	}

	public void SetMaterialValue(string name, float value)
	{
		this.getRenderers();
		if (this.renderers == null)
		{
			return;
		}
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		for (int i = 0; i < this.renderers.Count; i++)
		{
			if (this.renderers[i] != null)
			{
				this.renderers[i].GetPropertyBlock(this.matPropBlock);
				this.matPropBlock.SetFloat(name, value);
				this.renderers[i].SetPropertyBlock(this.matPropBlock);
			}
		}
	}

	public void SetMaterialColor(Color color)
	{
		this.getRenderers();
		for (int i = 0; i < this.renderers.Count; i++)
		{
			if (this.renderers[i] != null)
			{
				this.renderers[i].GetPropertyBlock(this.matPropBlock);
				this.matPropBlock.SetColor("_Color", color);
				this.renderers[i].SetPropertyBlock(this.matPropBlock);
			}
		}
	}

	public void UpdateTemperature()
	{
	}

	public override string ToString()
	{
		string text = "EntityBlockCreationData ";
		BlockValue blockValue = this.blockValue;
		return text + blockValue.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MaterialPropertyBlock matPropBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> renderers;

	public BlockValue blockValue;

	public Vector3i pos;

	public Transform transform;

	public bool bHasTransform;

	public bool bRenderingOn;

	public bool bNeedsTemperature;
}
