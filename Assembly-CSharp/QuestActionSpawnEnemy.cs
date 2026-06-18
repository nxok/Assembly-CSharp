using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionSpawnEnemy : BaseQuestAction
{
	public override void SetupAction()
	{
		string[] array = this.ID.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
			{
				if (keyValuePair.Value.entityClassName == array[i])
				{
					this.entityIDs.Add(keyValuePair.Key);
					if (this.entityIDs.Count == array.Length)
					{
						break;
					}
				}
			}
		}
	}

	public override void PerformAction(Quest ownerQuest)
	{
		if (!GameStats.GetBool(EnumGameStats.EnemySpawnMode))
		{
			return;
		}
		this.HandleSpawnEnemies(ownerQuest);
	}

	public void HandleSpawnEnemies(Quest ownerQuest)
	{
		if (this.Value != null && this.Value != "" && !int.TryParse(this.Value, out this.count) && this.Value.Contains("-"))
		{
			string[] array = this.Value.Split('-', StringSplitOptions.None);
			int num = Convert.ToInt32(array[0]);
			int num2 = Convert.ToInt32(array[1]);
			World world = GameManager.Instance.World;
			this.count = world.GetGameRandom().RandomRange(num, num2);
		}
		GameManager.Instance.StartCoroutine(this.SpawnEnemies(ownerQuest));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SpawnEnemies(Quest ownerQuest)
	{
		EntityPlayerLocal player = ownerQuest.OwnerJournal.OwnerPlayer;
		int num2;
		for (int i = 0; i < this.count; i = num2 + 1)
		{
			yield return new WaitForSeconds(0.5f);
			World world = GameManager.Instance.World;
			int num = this.entityIDs[world.GetGameRandom().RandomRange(this.entityIDs.Count)];
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestActionSpawnEnemy.SpawnQuestEntity(num, -1, player);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEntitySpawn>().Setup(num, player.entityId), false);
			}
			num2 = i;
		}
		yield break;
	}

	public static void SpawnQuestEntity(int spawnedEntityID, int entityIDQuestHolder, EntityPlayer player = null)
	{
		World world = GameManager.Instance.World;
		if (player == null)
		{
			player = world.GetEntity(entityIDQuestHolder) as EntityPlayer;
		}
		Vector3 vector;
		vector..ctor(world.GetGameRandom().RandomFloat * 2f + -1f, 0f, world.GetGameRandom().RandomFloat * 2f + -1f);
		vector.Normalize();
		float num = world.GetGameRandom().RandomFloat * 12f + 12f;
		Vector3 vector2 = player.position + vector * num;
		Vector3 vector3;
		vector3..ctor(0f, player.transform.eulerAngles.y + 180f, 0f);
		float num2 = (float)GameManager.Instance.World.GetHeight((int)vector2.x, (int)vector2.z);
		float num3 = (float)GameManager.Instance.World.GetTerrainHeight((int)vector2.x, (int)vector2.z);
		vector2.y = (num2 + num3) / 2f + 1.5f;
		Entity entity = EntityFactory.CreateEntity(spawnedEntityID, vector2, vector3);
		entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		GameManager.Instance.World.SpawnEntityInWorld(entity);
		(entity as EntityAlive).SetAttackTarget(player, 200);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionSpawnEnemy questActionSpawnEnemy = new QuestActionSpawnEnemy();
		base.CopyValues(questActionSpawnEnemy);
		questActionSpawnEnemy.entityIDs.AddRange(this.entityIDs);
		return questActionSpawnEnemy;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> entityIDs = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;
}
