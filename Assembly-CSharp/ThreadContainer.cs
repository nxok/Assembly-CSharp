using System;

public class ThreadContainer
{
	public ThreadContainer(DistantTerrain _TerExt, DistantChunk _DChunk, DistantChunkBasicMesh _BMesh, bool _WasReset)
	{
		this.Init(_TerExt, _DChunk, _BMesh, _WasReset);
	}

	public ThreadContainer()
	{
		this.TerExt = null;
		this.DChunk = null;
		this.BMesh = null;
		this.WasReset = false;
	}

	public void Init(DistantTerrain _TerExt, DistantChunk _DChunk, DistantChunkBasicMesh _BMesh, bool _WasReset)
	{
		this.TerExt = _TerExt;
		this.DChunk = _DChunk;
		this.BMesh = _BMesh;
		this.WasReset = _WasReset;
	}

	public void ThreadExtraWork()
	{
		this.TerExt.ThreadExtraWork(this.DChunk, this.BMesh, this.WasReset);
	}

	public void MainExtraWork()
	{
		this.TerExt.MainExtraWork(this.DChunk, this.BMesh);
	}

	public void Clear(bool IsClearItem)
	{
		if (IsClearItem)
		{
			this.TerExt = null;
			this.DChunk = null;
			this.BMesh = null;
			this.WasReset = false;
		}
	}

	public int DEBUG_TCId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public DistantTerrain TerExt;

	public DistantChunk DChunk;

	public DistantChunkBasicMesh BMesh;

	public bool WasReset;
}
