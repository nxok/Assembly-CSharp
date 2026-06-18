using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAI : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "ai" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "AI commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "AI commands:\nactivityclear - remove all activity areas (heat)\nanim <name> - trigger an animation (attack, attack2)\nanimmove <forward> <strafe> - set animation forward and strafe motion\nfreezepos - toggles movement\nlatency - toggles drawing\npathlines - toggles drawing editor path lines\npathgrid - force grid update\nragdoll <force> <time>\nrage <speed> <time> - make all zombies rage (0 - 2, 0 stops) (seconds)\nsendnames - toggles admin clients receiving debug name info";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		World world = GameManager.Instance.World;
		string text = _params[0].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 2125639436U)
		{
			if (num <= 1098165981U)
			{
				if (num != 511673672U)
				{
					if (num != 942821878U)
					{
						if (num != 1098165981U)
						{
							goto IL_0598;
						}
						if (!(text == "pathlines"))
						{
							goto IL_0598;
						}
						GameManager.Instance.DebugAILines = !GameManager.Instance.DebugAILines;
						return;
					}
					else
					{
						if (!(text == "aim"))
						{
							goto IL_0598;
						}
						GameManager.Instance.DebugAIAim = !GameManager.Instance.DebugAIAim;
						return;
					}
				}
				else
				{
					if (!(text == "ragdoll"))
					{
						goto IL_0598;
					}
					float num2 = 1f;
					float num3 = 1f;
					if (_params.Count >= 2)
					{
						float.TryParse(_params[1], out num2);
					}
					if (_params.Count >= 3)
					{
						float.TryParse(_params[2], out num3);
					}
					using (List<Entity>.Enumerator enumerator = world.Entities.list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Entity entity = enumerator.Current;
							EntityAlive entityAlive = entity as EntityAlive;
							if (entityAlive && !(entityAlive is EntityPlayer) && !(entityAlive is EntityTrader))
							{
								entityAlive.emodel.DoRagdoll(EModelBase.RagdollMode.Default, num3, EnumBodyPartHit.None, -entityAlive.GetForwardVector() * num2, Vector3.zero, false);
							}
						}
						return;
					}
				}
			}
			else if (num <= 1311049565U)
			{
				if (num != 1141294589U)
				{
					if (num != 1311049565U)
					{
						goto IL_0598;
					}
					if (!(text == "ac"))
					{
						goto IL_0598;
					}
					goto IL_024C;
				}
				else
				{
					if (!(text == "animmove"))
					{
						goto IL_0598;
					}
					goto IL_031D;
				}
			}
			else if (num != 1428345803U)
			{
				if (num != 2125639436U)
				{
					goto IL_0598;
				}
				if (!(text == "rage"))
				{
					goto IL_0598;
				}
			}
			else
			{
				if (!(text == "fp"))
				{
					goto IL_0598;
				}
				goto IL_03BA;
			}
			float num4 = 1f;
			float num5 = 5f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out num4);
			}
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out num5);
			}
			using (List<Entity>.Enumerator enumerator = world.Entities.list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Entity entity2 = enumerator.Current;
					EntityHuman entityHuman = entity2 as EntityHuman;
					if (entityHuman)
					{
						if (num4 <= 0f)
						{
							entityHuman.StopRage();
						}
						else
						{
							entityHuman.StartRage(num4, num5);
						}
					}
				}
				return;
			}
			goto IL_057F;
		}
		if (num <= 2689840001U)
		{
			if (num != 2540159512U)
			{
				if (num != 2670967086U)
				{
					if (num != 2689840001U)
					{
						goto IL_0598;
					}
					if (!(text == "activityclear"))
					{
						goto IL_0598;
					}
				}
				else
				{
					if (!(text == "pathgrid"))
					{
						goto IL_0598;
					}
					if (AstarManager.Instance)
					{
						AstarManager.Instance.OriginChanged();
						return;
					}
					return;
				}
			}
			else
			{
				if (!(text == "freezepos"))
				{
					goto IL_0598;
				}
				goto IL_03BA;
			}
		}
		else if (num <= 3319670096U)
		{
			if (num != 2708253965U)
			{
				if (num != 3319670096U)
				{
					goto IL_0598;
				}
				if (!(text == "anim"))
				{
					goto IL_0598;
				}
				if (_params.Count >= 2)
				{
					using (List<Entity>.Enumerator enumerator = world.Entities.list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Entity entity3 = enumerator.Current;
							EntityAlive entityAlive2 = entity3 as EntityAlive;
							if (entityAlive2 && entityAlive2.entityType != EntityType.Player)
							{
								string text2 = _params[1].ToLower();
								if (!(text2 == "attack"))
								{
									if (text2 == "attack2")
									{
										entityAlive2.StartAnimAction(3000);
									}
								}
								else
								{
									entityAlive2.StartAnimAction(0);
								}
							}
						}
						return;
					}
					goto IL_031D;
				}
				return;
			}
			else
			{
				if (!(text == "sendnames"))
				{
					goto IL_0598;
				}
				goto IL_057F;
			}
		}
		else
		{
			if (num != 3909890315U)
			{
				if (num != 4076569433U)
				{
					goto IL_0598;
				}
				if (!(text == "latency"))
				{
					goto IL_0598;
				}
			}
			else if (!(text == "l"))
			{
				goto IL_0598;
			}
			int num6 = world.GetPrimaryPlayerId();
			if (_senderInfo.RemoteClientInfo != null)
			{
				num6 = _senderInfo.RemoteClientInfo.entityId;
			}
			if (num6 != -1)
			{
				AIDirector.DebugToggleSendLatency(num6);
				return;
			}
			return;
		}
		IL_024C:
		GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>().Clear();
		return;
		IL_031D:
		float num7 = 0f;
		float num8 = 0f;
		if (_params.Count >= 2)
		{
			float.TryParse(_params[1], out num7);
		}
		if (_params.Count >= 3)
		{
			float.TryParse(_params[2], out num8);
		}
		using (List<Entity>.Enumerator enumerator = world.Entities.list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Entity entity4 = enumerator.Current;
				EntityAlive entityAlive3 = entity4 as EntityAlive;
				if (entityAlive3 && entityAlive3.entityType != EntityType.Player)
				{
					entityAlive3.speedForward = num7;
					entityAlive3.speedStrafe = num8;
				}
			}
			return;
		}
		IL_03BA:
		AIDirector.DebugToggleFreezePos();
		return;
		IL_057F:
		if (_senderInfo.RemoteClientInfo != null)
		{
			AIDirector.DebugToggleSendNameInfo(_senderInfo.RemoteClientInfo.entityId);
			return;
		}
		return;
		IL_0598:
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text + ".");
	}
}
