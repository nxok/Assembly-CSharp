using System;

public class BlockTextureData
{
	public static void InitStatic()
	{
		BlockTextureData.list = new BlockTextureData[256];
	}

	public void Init()
	{
		BlockTextureData.list[this.ID] = this;
	}

	public static void Cleanup()
	{
		BlockTextureData.list = null;
	}

	public static BlockTextureData GetDataByTextureID(int textureID)
	{
		for (int i = 0; i < BlockTextureData.list.Length; i++)
		{
			if (BlockTextureData.list[i] != null && (int)BlockTextureData.list[i].TextureID == textureID)
			{
				return BlockTextureData.list[i];
			}
		}
		return null;
	}

	public bool GetLocked(EntityPlayerLocal player)
	{
		if (this.LockedByPerk != "")
		{
			ProgressionValue progressionValue = player.Progression.GetProgressionValue(this.LockedByPerk);
			if (progressionValue != null && progressionValue.CalculatedLevel(player) >= (int)this.RequiredLevel)
			{
				return true;
			}
		}
		return false;
	}

	public static BlockTextureData[] list;

	public int ID;

	public ushort TextureID;

	public string Name;

	public string LocalizedName;

	public string Group;

	public ushort PaintCost;

	public bool Hidden;

	public byte SortIndex = byte.MaxValue;

	public string LockedByPerk = "";

	public ushort RequiredLevel;
}
