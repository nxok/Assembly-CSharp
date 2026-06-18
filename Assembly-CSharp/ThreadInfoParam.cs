using System;

public class ThreadInfoParam
{
	public ThreadInfoParam()
	{
		this.IsBigCapacity = false;
		this.ThreadContListA = new ThreadContainer[this.TmpArraySizeFristDim * 20];
		this.ForwardChunkToDeleteIdA = new DistantChunk[this.TmpArraySizeFristDim];
		this.BackwardChunkToDeleteIdA = new DistantChunk[this.TmpArraySizeFristDim * 2];
		this.ForwardChunkSeamToAdjust = new DistantChunk[this.TmpArraySizeFristDim][];
		this.BackwardChunkSeamToAdjust = new DistantChunk[this.TmpArraySizeFristDim][];
		this.ForwardEdgeId = new int[this.TmpArraySizeFristDim][];
		this.BackwardEdgeId = new int[this.TmpArraySizeFristDim][];
		this.SDLengthForwardChunkSeamToAdjust = new int[this.TmpArraySizeFristDim];
		this.SDLengthBackwardChunkSeamToAdjust = new int[this.TmpArraySizeFristDim];
		for (int i = 0; i < this.TmpArraySizeFristDim; i++)
		{
			this.ForwardChunkSeamToAdjust[i] = new DistantChunk[this.TmpArraySizeSecondDim];
			this.BackwardChunkSeamToAdjust[i] = new DistantChunk[this.TmpArraySizeSecondDim];
			this.ForwardEdgeId[i] = new int[this.TmpArraySizeSecondDim];
			this.BackwardEdgeId[i] = new int[this.TmpArraySizeSecondDim];
		}
		this.FDLengthForwardChunkSeamToAdjust = 0;
		this.FDLengthBackwardChunkSeamToAdjust = 0;
		this.CntForwardChunkSeamToAdjust = 0;
		this.CntBackwardChunkSeamToAdjust = 0;
		this.ResLevel = 0;
		this.OutId = 0;
		this.IsThreadDone = false;
		this.IsCoroutineDone = false;
	}

	public ThreadInfoParam(DistantChunkMap _CMap, int _ResLevel, int _OutId, bool _IsBigCapacity)
	{
		this.ThreadContListA = new ThreadContainer[this.TmpArraySizeFristDim * 20];
		this.ForwardChunkToDeleteIdA = new DistantChunk[this.TmpArraySizeFristDim];
		this.BackwardChunkToDeleteIdA = new DistantChunk[this.TmpArraySizeFristDim * 2];
		this.ForwardChunkSeamToAdjust = new DistantChunk[this.TmpArraySizeFristDim][];
		this.BackwardChunkSeamToAdjust = new DistantChunk[this.TmpArraySizeFristDim][];
		this.ForwardEdgeId = new int[this.TmpArraySizeFristDim][];
		this.BackwardEdgeId = new int[this.TmpArraySizeFristDim][];
		this.SDLengthForwardChunkSeamToAdjust = new int[this.TmpArraySizeFristDim];
		this.SDLengthBackwardChunkSeamToAdjust = new int[this.TmpArraySizeFristDim];
		for (int i = 0; i < this.TmpArraySizeFristDim; i++)
		{
			this.ForwardChunkSeamToAdjust[i] = new DistantChunk[this.TmpArraySizeSecondDim];
			this.BackwardChunkSeamToAdjust[i] = new DistantChunk[this.TmpArraySizeSecondDim];
			this.ForwardEdgeId[i] = new int[this.TmpArraySizeSecondDim];
			this.BackwardEdgeId[i] = new int[this.TmpArraySizeSecondDim];
			this.SDLengthForwardChunkSeamToAdjust[i] = 0;
			this.SDLengthBackwardChunkSeamToAdjust[i] = 0;
		}
		this.Init(_CMap, _ResLevel, _OutId, _IsBigCapacity);
	}

	public void Init(DistantChunkMap _CMap, int _ResLevel, int _OutId, bool _IsBigCapacity)
	{
		this.IsBigCapacity = _IsBigCapacity;
		this.CntThreadContList = 0;
		this.LengthThreadContList = 0;
		this.CntForwardChunkToDeleteId = 0;
		this.LengthForwardChunkToDeleteId = 0;
		this.CntBackwardChunkToDeleteId = 0;
		this.LengthBackwardChunkToDeleteId = 0;
		for (int i = 0; i < this.TmpArraySizeFristDim; i++)
		{
			this.SDLengthForwardChunkSeamToAdjust[i] = 0;
			this.SDLengthBackwardChunkSeamToAdjust[i] = 0;
		}
		this.CntForwardChunkSeamToAdjust = 0;
		this.CntBackwardChunkSeamToAdjust = 0;
		this.FDLengthForwardChunkSeamToAdjust = 0;
		this.FDLengthBackwardChunkSeamToAdjust = 0;
		this.ResLevel = _ResLevel;
		this.OutId = _OutId;
		this.IsThreadDone = false;
		this.IsCoroutineDone = false;
	}

	public void ClearAll(ThreadContainerPool TmpThContPool = null)
	{
		if (TmpThContPool != null)
		{
			while (this.CntThreadContList < this.LengthThreadContList)
			{
				TmpThContPool.ReturnObject(this.ThreadContListA[this.CntThreadContList], true);
				this.ThreadContListA[this.CntThreadContList] = null;
				this.CntThreadContList++;
			}
		}
		this.IsThreadDone = true;
		this.IsCoroutineDone = true;
	}

	public ThreadContainer[] ThreadContListA;

	public int CntThreadContList;

	public int LengthThreadContList;

	public int ResLevel;

	public int OutId;

	public DistantChunk[] ForwardChunkToDeleteIdA;

	public DistantChunk[] BackwardChunkToDeleteIdA;

	public int CntForwardChunkToDeleteId;

	public int LengthForwardChunkToDeleteId;

	public int CntBackwardChunkToDeleteId;

	public int LengthBackwardChunkToDeleteId;

	public DistantChunk[][] ForwardChunkSeamToAdjust;

	public DistantChunk[][] BackwardChunkSeamToAdjust;

	public int[][] ForwardEdgeId;

	public int[][] BackwardEdgeId;

	public int CntForwardChunkSeamToAdjust;

	public int CntBackwardChunkSeamToAdjust;

	public int FDLengthForwardChunkSeamToAdjust;

	public int FDLengthBackwardChunkSeamToAdjust;

	public int[] SDLengthForwardChunkSeamToAdjust;

	public int[] SDLengthBackwardChunkSeamToAdjust;

	[PublicizedFrom(EAccessModifier.Private)]
	public int TmpArraySizeFristDim = 150;

	[PublicizedFrom(EAccessModifier.Private)]
	public int TmpArraySizeSecondDim = 64;

	public bool IsThreadDone;

	public bool IsCoroutineDone;

	public bool IsBigCapacity;

	public bool IsAsynchronous;
}
