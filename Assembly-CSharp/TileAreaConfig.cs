using System;

public struct TileAreaConfig
{
	public void checkCoordinates(ref int _tileX, ref int _tileZ)
	{
		Vector2i vector2i = this.tileEnd - this.tileStart + new Vector2i(1, 1);
		if (_tileX < this.tileStart.x)
		{
			if (this.bWrapAroundX)
			{
				_tileX += vector2i.x;
			}
			else
			{
				_tileX = this.tileStart.x;
			}
		}
		else if (_tileX > this.tileEnd.x)
		{
			if (this.bWrapAroundX)
			{
				_tileX -= vector2i.x;
			}
			else
			{
				_tileX = this.tileEnd.x;
			}
		}
		if (_tileZ >= this.tileStart.y)
		{
			if (_tileZ > this.tileEnd.y)
			{
				if (this.bWrapAroundZ)
				{
					_tileZ -= vector2i.y;
					return;
				}
				_tileZ = this.tileEnd.y;
			}
			return;
		}
		if (this.bWrapAroundZ)
		{
			_tileZ += vector2i.y;
			return;
		}
		_tileZ = this.tileStart.y;
	}

	public Vector2i tileStart;

	public Vector2i tileEnd;

	public int tileSizeInWorldUnits;

	public bool bWrapAroundX;

	public bool bWrapAroundZ;
}
