using System;

public static class ChunkConditions
{
	public static readonly ChunkConditions.Delegate Decorated = (Chunk chunk) => !chunk.NeedsDecoration && !chunk.NeedsLightCalculation;

	public static readonly ChunkConditions.Delegate MeshesCopied = (Chunk chunk) => !chunk.InProgressDecorating && !chunk.InProgressLighting && !chunk.InProgressRegeneration && !chunk.InProgressCopying && !chunk.NeedsDecoration && !chunk.NeedsLightCalculation && !chunk.NeedsRegeneration && !chunk.NeedsCopying;

	public static readonly ChunkConditions.Delegate Displayed = (Chunk chunk) => ChunkConditions.MeshesCopied(chunk) && chunk.displayState == Chunk.DisplayState.Done;

	public delegate bool Delegate(Chunk chunk);
}
