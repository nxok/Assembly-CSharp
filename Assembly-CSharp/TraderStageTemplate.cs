using System;

public class TraderStageTemplate
{
	public bool IsWithin(int traderStage, int quality)
	{
		return (this.Min == -1 || this.Min <= traderStage) && (this.Max == -1 || this.Max >= traderStage) && (this.Quality == -1 || quality == this.Quality);
	}

	public int Min = -1;

	public int Max = -1;

	public int Quality = -1;
}
