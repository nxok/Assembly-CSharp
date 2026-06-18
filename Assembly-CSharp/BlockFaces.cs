using System;
using UnityEngine;

public static class BlockFaces
{
	public static BlockFace CharToFace(char c)
	{
		if (c <= 'W')
		{
			if (c <= 'E')
			{
				if (c == 'B')
				{
					return BlockFace.Bottom;
				}
				if (c != 'E')
				{
					return BlockFace.None;
				}
				return BlockFace.East;
			}
			else
			{
				if (c == 'N')
				{
					return BlockFace.North;
				}
				switch (c)
				{
				case 'S':
					return BlockFace.South;
				case 'T':
					break;
				case 'U':
				case 'V':
					return BlockFace.None;
				case 'W':
					return BlockFace.West;
				default:
					return BlockFace.None;
				}
			}
		}
		else if (c <= 'e')
		{
			if (c == 'b')
			{
				return BlockFace.Bottom;
			}
			if (c != 'e')
			{
				return BlockFace.None;
			}
			return BlockFace.East;
		}
		else
		{
			if (c == 'n')
			{
				return BlockFace.North;
			}
			switch (c)
			{
			case 's':
				return BlockFace.South;
			case 't':
				break;
			case 'u':
			case 'v':
				return BlockFace.None;
			case 'w':
				return BlockFace.West;
			default:
				return BlockFace.None;
			}
		}
		return BlockFace.Top;
	}

	public static BlockFace RotateFace(this BlockFace face, int rotation)
	{
		Vector3 vector = Vector3.zero;
		switch (face)
		{
		case BlockFace.Top:
			vector = Vector3.up;
			break;
		case BlockFace.Bottom:
			vector = Vector3.down;
			break;
		case BlockFace.North:
			vector = Vector3.forward;
			break;
		case BlockFace.West:
			vector = Vector3.left;
			break;
		case BlockFace.South:
			vector = Vector3.back;
			break;
		case BlockFace.East:
			vector = Vector3.right;
			break;
		}
		vector = BlockShapeNew.GetRotationStatic(rotation) * vector;
		if (vector.y > 0.9f)
		{
			return BlockFace.Top;
		}
		if (vector.y < -0.9f)
		{
			return BlockFace.Bottom;
		}
		if (vector.z > 0.9f)
		{
			return BlockFace.North;
		}
		if (vector.x < -0.9f)
		{
			return BlockFace.West;
		}
		if (vector.z < -0.9f)
		{
			return BlockFace.South;
		}
		if (vector.x > 0.9f)
		{
			return BlockFace.East;
		}
		return face;
	}

	public static Vector3 GetNormal(this BlockFace face)
	{
		Vector3 vector;
		switch (face)
		{
		case BlockFace.Top:
			vector = Vector3.up;
			break;
		case BlockFace.Bottom:
			vector = Vector3.down;
			break;
		case BlockFace.North:
			vector = Vector3.forward;
			break;
		case BlockFace.West:
			vector = Vector3.left;
			break;
		case BlockFace.South:
			vector = Vector3.back;
			break;
		case BlockFace.East:
			vector = Vector3.right;
			break;
		case BlockFace.Middle:
			vector = Vector3.zero;
			break;
		default:
			if (face != BlockFace.None)
			{
				throw new ArgumentOutOfRangeException("face", face, null);
			}
			vector = Vector3.zero;
			break;
		}
		return vector;
	}
}
