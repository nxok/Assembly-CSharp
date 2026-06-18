using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityAnimalSnake : EntityEnemyAnimal
{
	public override Vector3 GetAttackTargetHitPosition()
	{
		Vector3 position = this.attackTarget.position;
		position.y += 0.5f;
		return position;
	}
}
