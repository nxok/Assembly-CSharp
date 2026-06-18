using System;
using System.Collections;
using System.Collections.Generic;
using RaycastPathing;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class RaycastEntityPathGenerator
{
	public World GameWorld
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public EntityAlive Entity
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual RaycastPath Path
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public bool isBuildingPath
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool isPathReady
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public RaycastEntityPathGenerator(World _world, EntityAlive _entity)
	{
		this.GameWorld = _world;
		this.Entity = _entity;
	}

	public Vector3[] pathToArray()
	{
		Vector3[] array = new Vector3[this.Path.Nodes.Count - 1];
		for (int i = 0; i < this.Path.Nodes.Count; i++)
		{
			array[i] = this.Path.Nodes[i].Position;
		}
		return array;
	}

	public List<Vector3> pathToList()
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < this.Path.Nodes.Count; i++)
		{
			list.Add(this.Path.Nodes[i].Position);
		}
		list.Reverse();
		return list;
	}

	public void CreatePath(Vector3 start, Vector3 end, float speed, bool canBreakBlocks, float yHeightOffset = 0f)
	{
		this.cleanupPath();
		this.InitPath(start, end);
		this.beginPathProc();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitPath(Vector3 start, Vector3 end)
	{
		this.Path = new RaycastPath(start, end);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IEnumerator BuildPathProc()
	{
		this.finalizePathProc();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void beginPathProc()
	{
		this.isBuildingPath = true;
		this.StartCoroutine(this.BuildPathProc());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void abortPathProc()
	{
		this.StopCoroutine(this.BuildPathProc());
		this.isBuildingPath = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void finalizePathProc()
	{
		this.isBuildingPath = false;
		this.isPathReady = true;
	}

	public void Clear()
	{
		this.cleanupPath();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanupPath()
	{
		this.isPathReady = false;
		if (this.Path != null)
		{
			this.abortPathProc();
			this.Path.Destruct();
			this.Path = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StartCoroutine(IEnumerator task)
	{
		GameManager.Instance.StartCoroutine(task);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StopCoroutine(IEnumerator task)
	{
		GameManager.Instance.StopCoroutine(task);
	}

	public bool IsConfinedSpace(Vector3 pos, float size, bool debugDraw = false)
	{
		return RaycastPathWorldUtils.IsConfinedSpace(this.GameWorld, pos, size, debugDraw);
	}
}
