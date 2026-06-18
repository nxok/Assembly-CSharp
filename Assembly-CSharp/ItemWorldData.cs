using System;

public class ItemWorldData
{
	public ItemWorldData(IGameManager _gm, ItemValue _itemValue, EntityItem _entityItem, int _belongsEntityId)
	{
		this.gameManager = _gm;
		this.world = _entityItem.world;
		this.entityItem = _entityItem;
		this.belongsEntityId = _belongsEntityId;
	}

	public IGameManager gameManager;

	public WorldBase world;

	public EntityItem entityItem;

	public int belongsEntityId;
}
