using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebugJiggle : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "debugjiggle", "dgj" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		if (_params.Count <= 0)
		{
			foreach (EntityAlive entityAlive in world.EntityAlives)
			{
				this.LogEntityJiggles(entityAlive);
			}
			return;
		}
		int num;
		if (!int.TryParse(_params[0], out num))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Expected an entity id");
			return;
		}
		Entity entity = world.GetEntity(num);
		if (entity == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Could not find entity with id {0}", num));
			return;
		}
		this.LogEntityJiggles(entity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogEntityJiggles(Entity entity)
	{
		EModelBase emodel = entity.emodel;
		if (emodel == null)
		{
			return;
		}
		emodel.LogJiggles();
	}
}
