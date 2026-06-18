using System;

public abstract class BlockShapeBillboardRotatedAbstract : BlockShapeRotatedAbstract
{
	public BlockShapeBillboardRotatedAbstract()
	{
		this.IsSolidCube = false;
		this.IsSolidSpace = false;
		this.IsRotatable = true;
		this.LightOpacity = 0;
	}

	public override void Init(Block _block)
	{
		base.Init(_block);
		_block.IsDecoration = true;
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return false;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		for (int i = 0; i < _rotCount; i++)
		{
			byte b = _blockValue.rotation;
			if (b <= 3)
			{
				if (_bLeft)
				{
					b = ((b > 0) ? (b - 1) : 3);
				}
				else
				{
					b = ((b < 3) ? (b + 1) : 0);
				}
			}
			else if (b <= 7)
			{
				if (_bLeft)
				{
					b = ((b > 4) ? (b - 1) : 7);
				}
				else
				{
					b = ((b < 7) ? (b + 1) : 4);
				}
			}
			else if (b <= 11)
			{
				if (_bLeft)
				{
					b = ((b > 8) ? (b - 1) : 11);
				}
				else
				{
					b = ((b < 11) ? (b + 1) : 8);
				}
			}
			_blockValue.rotation = b;
		}
		return _blockValue;
	}
}
