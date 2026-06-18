using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

public class WaterDebugManager
{
	public bool RenderingEnabled { get; set; } = true;

	public void InitializeDebugRender(Chunk chunk)
	{
		WaterDebugRenderer waterDebugRenderer = WaterDebugPools.rendererPool.AllocSync(true);
		waterDebugRenderer.LoadFromChunk(chunk);
		chunk.AssignWaterDebugRenderer(new WaterDebugManager.RendererHandle(chunk, this));
		this.newRenderers.Enqueue(new WaterDebugManager.InitializedRenderer
		{
			chunkKey = chunk.Key,
			renderer = waterDebugRenderer
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReturnRenderer(long key)
	{
		this.renderersToRemove.Enqueue(key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRenderers()
	{
		WaterDebugManager.InitializedRenderer initializedRenderer;
		while (this.newRenderers.TryDequeue(out initializedRenderer))
		{
			WaterDebugRenderer waterDebugRenderer;
			if (this.activeRenderers.TryGetValue(initializedRenderer.chunkKey, out waterDebugRenderer))
			{
				WaterDebugPools.rendererPool.FreeSync(waterDebugRenderer);
				this.activeRenderers.Remove(initializedRenderer.chunkKey);
			}
			this.activeRenderers.Add(initializedRenderer.chunkKey, initializedRenderer.renderer);
		}
		long num;
		while (this.renderersToRemove.TryDequeue(out num))
		{
			WaterDebugRenderer waterDebugRenderer2;
			if (this.activeRenderers.TryGetValue(num, out waterDebugRenderer2))
			{
				WaterDebugPools.rendererPool.FreeSync(waterDebugRenderer2);
				this.activeRenderers.Remove(num);
			}
		}
	}

	public void DebugDraw()
	{
		this.UpdateRenderers();
		if (this.RenderingEnabled)
		{
			foreach (WaterDebugRenderer waterDebugRenderer in this.activeRenderers.Values)
			{
				waterDebugRenderer.Draw();
			}
		}
	}

	public void Cleanup()
	{
		WaterDebugManager.InitializedRenderer initializedRenderer;
		while (this.newRenderers.TryDequeue(out initializedRenderer))
		{
			WaterDebugPools.rendererPool.FreeSync(initializedRenderer.renderer);
		}
		foreach (WaterDebugRenderer waterDebugRenderer in this.activeRenderers.Values)
		{
			WaterDebugPools.rendererPool.FreeSync(waterDebugRenderer);
		}
		this.activeRenderers.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<WaterDebugManager.InitializedRenderer> newRenderers = new ConcurrentQueue<WaterDebugManager.InitializedRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, WaterDebugRenderer> activeRenderers = new Dictionary<long, WaterDebugRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<long> renderersToRemove = new ConcurrentQueue<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct InitializedRenderer
	{
		public long chunkKey;

		public WaterDebugRenderer renderer;
	}

	public struct RendererHandle
	{
		public bool IsValid
		{
			get
			{
				return this.manager != null && this.key != null;
			}
		}

		public RendererHandle(Chunk _chunk, WaterDebugManager _manager)
		{
			this.manager = _manager;
			this.key = new long?(_chunk.Key);
		}

		[Conditional("UNITY_EDITOR")]
		public void SetChunkOrigin(Vector3i _origin)
		{
			if (!this.IsValid)
			{
				return;
			}
			WaterDebugRenderer waterDebugRenderer;
			if (this.manager.activeRenderers.TryGetValue(this.key.Value, out waterDebugRenderer))
			{
				waterDebugRenderer.SetChunkOrigin(_origin);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void SetWater(int _x, int _y, int _z, float mass)
		{
			if (!this.IsValid)
			{
				return;
			}
			WaterDebugRenderer waterDebugRenderer;
			if (this.manager.activeRenderers.TryGetValue(this.key.Value, out waterDebugRenderer))
			{
				waterDebugRenderer.SetWater(_x, _y, _z, mass);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void Reset()
		{
			if (this.IsValid)
			{
				this.manager.ReturnRenderer(this.key.Value);
			}
			this.manager = null;
			this.key = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public WaterDebugManager manager;

		[PublicizedFrom(EAccessModifier.Private)]
		public long? key;
	}
}
