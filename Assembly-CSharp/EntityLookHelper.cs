using System;

public class EntityLookHelper
{
	public EntityLookHelper(EntityAlive _e)
	{
		this.entity = _e;
	}

	public void onUpdateLook()
	{
		if (this.entity.rotation.x > 1f)
		{
			EntityAlive entityAlive = this.entity;
			entityAlive.rotation.x = entityAlive.rotation.x - 1f;
			return;
		}
		if (this.entity.rotation.x < -1f)
		{
			EntityAlive entityAlive2 = this.entity;
			entityAlive2.rotation.x = entityAlive2.rotation.x + 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;
}
