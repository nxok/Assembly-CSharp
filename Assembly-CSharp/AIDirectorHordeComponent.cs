using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIDirectorHordeComponent : AIDirectorComponent
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public uint FindTargets(out Vector3 startPos, out Vector3 pitStop, out Vector3 endPos, List<AIDirectorPlayerState> outTargets)
	{
		startPos = Vector3.zero;
		pitStop = Vector3.zero;
		endPos = Vector3.zero;
		List<AIDirectorPlayerState> list = this.Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers.list;
		int num = base.Random.RandomRange(0, list.Count);
		AIDirectorPlayerState aidirectorPlayerState = list[num];
		int num2 = 1;
		while (num2 < list.Count && aidirectorPlayerState.Dead)
		{
			num = (num + num2) % list.Count;
			aidirectorPlayerState = list[num];
			num2++;
		}
		if (aidirectorPlayerState.Dead)
		{
			return 1U;
		}
		outTargets.Add(aidirectorPlayerState);
		int num3 = 1;
		Vector3 vector = aidirectorPlayerState.Player.position;
		for (int i = 0; i < list.Count; i++)
		{
			AIDirectorPlayerState aidirectorPlayerState2 = list[i];
			if (aidirectorPlayerState2 != aidirectorPlayerState)
			{
				Vector3 vector2 = aidirectorPlayerState2.Player.position - aidirectorPlayerState.Player.position;
				vector2.y = 0f;
				if (vector2.sqrMagnitude <= 900f)
				{
					vector += aidirectorPlayerState2.Player.position;
					num3++;
					outTargets.Add(aidirectorPlayerState2);
				}
			}
		}
		if (num3 == 1 && base.Random.RandomFloat < 0.3f)
		{
			return 12U;
		}
		vector /= (float)num3;
		vector.y += 10f;
		if (!this.FindOnGroundPos(ref vector))
		{
			AIDirector.LogAI("FindWanderingTargets !playerPos", Array.Empty<object>());
			return 1U;
		}
		Vector2 vector3 = base.Random.RandomOnUnitCircle;
		World world = this.Director.World;
		float num4 = 92f;
		int num5 = 8;
		while (num4 > 0f)
		{
			Vector2 vector4 = vector3 * num4;
			startPos.x = vector.x + vector4.x;
			startPos.y = vector.y;
			startPos.z = vector.z + vector4.y;
			Vector3i vector3i;
			vector3i.x = Utils.Fastfloor(startPos.x);
			vector3i.z = Utils.Fastfloor(startPos.z);
			Chunk chunk = (Chunk)world.GetChunkFromWorldPos(vector3i.x, vector3i.z);
			if (chunk != null && world.GetChunkFromWorldPos(vector3i.x - 16, vector3i.z - 16) != null && world.GetChunkFromWorldPos(vector3i.x + 16, vector3i.z + 16) != null)
			{
				if (num5 > 0)
				{
					bool flag = false;
					for (int j = 0; j < list.Count; j++)
					{
						AIDirectorPlayerState aidirectorPlayerState3 = list[j];
						if (!aidirectorPlayerState3.Dead)
						{
							Vector3 vector5 = aidirectorPlayerState3.Player.position - startPos;
							vector5.y = 0f;
							if (vector5.sqrMagnitude < 900f)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						num4 = 92f;
						vector3 = base.Random.RandomOnUnitCircle;
						num5--;
						continue;
					}
				}
				if (!this.FindOnGroundPos(ref startPos))
				{
					AIDirector.LogAI("FindWanderingTargets !start", Array.Empty<object>());
					return 1U;
				}
				vector3i = World.worldToBlockPos(startPos);
				bool flag2 = num5 >= 3;
				if (chunk.CanMobsSpawnAtPos(World.toBlockXZ(vector3i.x), vector3i.y, World.toBlockXZ(vector3i.z), false, flag2))
				{
					break;
				}
				if (num5 <= 0)
				{
					AIDirector.LogAI("FindWanderingTargets !CanMobsSpawnAtPos", Array.Empty<object>());
					return 1U;
				}
				num4 = 92f;
				vector3 = base.Random.RandomOnUnitCircle;
				num5--;
			}
			else
			{
				num4 -= 16f;
			}
		}
		if (num4 < 50f)
		{
			AIDirector.LogAI("FindWanderingTargets start too close {0}", new object[] { num4 });
			return 1U;
		}
		Vector2 vector6 = Vector2.Perpendicular(vector3);
		if (base.Random.RandomFloat < 0.5f)
		{
			vector6 *= -1f;
		}
		vector6 *= 40f + base.Random.RandomFloat * 20f;
		pitStop.x += vector.x + vector6.x;
		pitStop.z += vector.z + vector6.y;
		pitStop.y = startPos.y + 16f;
		if (!this.FindOnGroundPos(ref pitStop))
		{
			AIDirector.LogAI("FindWanderingTargets !pitStop", Array.Empty<object>());
			return 1U;
		}
		endPos.x = (pitStop.x - startPos.x) * 0.5f + pitStop.x;
		endPos.z = (pitStop.z - startPos.z) * 0.5f + pitStop.z;
		endPos.y = pitStop.y + 16f;
		if (!this.FindOnGroundPos(ref endPos))
		{
			AIDirector.LogAI("FindWanderingTargets !end", Array.Empty<object>());
			return 1U;
		}
		AIDirector.LogAIExtra("FindWanderingTargets at player '{0}', dist {1}", new object[] { aidirectorPlayerState.Player, vector6.magnitude });
		return 0U;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FindOnGroundPos(ref Vector3 pos)
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		if (this.Director.World.GetWorldExtent(out vector3i, out vector3i2))
		{
			pos.x = Mathf.Clamp(pos.x, (float)vector3i.x, (float)vector3i2.x);
			pos.z = Mathf.Clamp(pos.z, (float)vector3i.z, (float)vector3i2.z);
		}
		int num = Utils.Fastfloor(pos.x);
		int num2 = Utils.Fastfloor(pos.z);
		int num3 = Utils.Fastfloor(pos.y);
		num3 = Utils.FastClamp(num3, 0, 255);
		Chunk chunk = (Chunk)this.Director.World.GetChunkFromWorldPos(num, num2);
		if (chunk == null)
		{
			return false;
		}
		int num4 = World.toBlockXZ(num);
		int num5 = World.toBlockXZ(num2);
		while (chunk.GetBlockId(num4, num3, num5) == 0)
		{
			if (--num3 < 0)
			{
				return false;
			}
		}
		num3++;
		for (;;)
		{
			if (chunk.GetBlockId(num4, num3, num5) == 0)
			{
				if (chunk.GetBlockId(num4, num3 + 1, num5) == 0)
				{
					goto Block_7;
				}
				num3 += 2;
				if (num3 >= 255)
				{
					return false;
				}
			}
			else if (++num3 >= 255)
			{
				break;
			}
		}
		return false;
		Block_7:
		pos.x = (float)num;
		pos.y = (float)num3;
		pos.z = (float)num2;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindScoutStartPos(Vector3 endPos, out Vector3 startPos)
	{
		List<AIDirectorPlayerState> list = this.Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers.list;
		startPos = Vector3.zero;
		if (!this.FindOnGroundPos(ref endPos))
		{
			AIDirector.LogAI("FindScoutStartPos !end", Array.Empty<object>());
			return false;
		}
		World world = this.Director.World;
		Vector2 randomOnUnitCircle = base.Random.RandomOnUnitCircle;
		float num = 80f;
		int num2 = 15;
		for (;;)
		{
			if (--num2 < 0)
			{
				num -= 16f;
				if (num < 40f)
				{
					break;
				}
				num2 = 9;
			}
			Vector2 vector = base.Random.RandomOnUnitCircle * num;
			startPos.x = endPos.x + vector.x;
			startPos.y = endPos.y;
			startPos.z = endPos.z + vector.y;
			Vector3i vector3i;
			vector3i.x = Utils.Fastfloor(startPos.x);
			vector3i.z = Utils.Fastfloor(startPos.z);
			Chunk chunk = (Chunk)world.GetChunkFromWorldPos(vector3i.x, vector3i.z);
			if (chunk != null && world.GetChunkFromWorldPos(vector3i.x - 16, vector3i.z - 16) != null && world.GetChunkFromWorldPos(vector3i.x + 16, vector3i.z + 16) != null)
			{
				if (num2 > 0)
				{
					bool flag = false;
					for (int i = 0; i < list.Count; i++)
					{
						AIDirectorPlayerState aidirectorPlayerState = list[i];
						if (!aidirectorPlayerState.Dead)
						{
							Vector3 vector2 = aidirectorPlayerState.Player.position - startPos;
							vector2.y *= 0.1f;
							if (vector2.sqrMagnitude < 900f)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						continue;
					}
				}
				if (this.FindOnGroundPos(ref startPos))
				{
					vector3i = World.worldToBlockPos(startPos);
					bool flag2 = num2 >= 5;
					if (chunk.CanMobsSpawnAtPos(World.toBlockXZ(vector3i.x), vector3i.y, World.toBlockXZ(vector3i.z), false, flag2))
					{
						return true;
					}
				}
			}
		}
		AIDirector.LogAI("FindScoutStartPos !dist", Array.Empty<object>());
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorHordeComponent()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPitstopSideMin = 40f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPitstopSideRange = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerClosestDist = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSinglePlayerSkipPer = 0.3f;
}
