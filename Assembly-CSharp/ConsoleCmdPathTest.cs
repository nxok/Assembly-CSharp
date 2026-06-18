using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePath;
using Pathfinding;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPathTest : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return false;
		}
	}

	public override DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "pathtest" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Usage. Toggle a path test mode.");
		stringBuilder.AppendLine(" breakblocks - toggles path allowed to break blocks");
		stringBuilder.AppendLine(" climbladders - toggles path allowed to break blocks");
		stringBuilder.AppendLine(" climbwalls - toggles path allowed to break blocks");
		return stringBuilder.ToString();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (_params.Count > 0)
		{
			foreach (string text in _params)
			{
				string text2 = text.ToLower();
				bool on = text2.StartsWith("+");
				bool off = text2.StartsWith("-");
				if (on | off)
				{
					text2 = text2.Substring(1);
				}
				Func<bool, bool> func = (bool currentValue) => on || (!off && !currentValue);
				if (string.Equals(text2, "breakblocks", StringComparison.OrdinalIgnoreCase))
				{
					ConsoleCmdPathTest.canBreakBlocks = func(ConsoleCmdPathTest.canBreakBlocks);
				}
				else if (string.Equals(text2, "climbladders", StringComparison.OrdinalIgnoreCase))
				{
					ConsoleCmdPathTest.canClimbLadders = func(ConsoleCmdPathTest.canClimbLadders);
				}
				else if (string.Equals(text2, "climbwalls", StringComparison.OrdinalIgnoreCase))
				{
					ConsoleCmdPathTest.canClimbWalls = func(ConsoleCmdPathTest.canClimbWalls);
				}
			}
		}
		if (this.executionsCoroutine != null)
		{
			EAIPathTest eaipathTest = this.aiPathTest;
			if (eaipathTest != null)
			{
				eaipathTest.CancelTargetMove();
			}
			GameManager.Instance.StopCoroutine(this.executionsCoroutine);
			this.executionsCoroutine = null;
			return;
		}
		this.executionsCoroutine = GameManager.Instance.StartCoroutine(this.ExecutePathTestCoroutine());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ExecutePathTestCoroutine()
	{
		EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
		this.aiPathTest = null;
		string text;
		EntityAlive entityAlive = ItemActionAttack.FindHitEntityNoTagCheck(player.HitInfo, out text) as EntityAlive;
		if (entityAlive != null)
		{
			EAIManager aiManager = entityAlive.aiManager;
			EAIPathTest eaipathTest;
			if (aiManager == null)
			{
				eaipathTest = null;
			}
			else
			{
				List<EAIPathTest> tasks = aiManager.GetTasks<EAIPathTest>();
				eaipathTest = ((tasks != null) ? tasks.FirstOrDefault<EAIPathTest>() : null);
			}
			this.aiPathTest = eaipathTest;
		}
		PathInfoSingleTarget pathInfo = new PathInfoSingleTarget(player, Vector3.zero, false, 1f, null);
		ASPPathFinder pathFinder = new ASPPathFinder(pathInfo, false, false, false);
		GraphNode[] pathNodes = null;
		pathInfo.state = PathInfo.State.Done;
		pathInfo.OnPathResult = delegate(Path p)
		{
			List<GraphNode> path = p.path;
			pathNodes = ((path != null) ? path.ToArray() : null);
		};
		for (;;)
		{
			if (pathInfo.canBreakBlocks == ConsoleCmdPathTest.canBreakBlocks && pathFinder.canClimbLadders == ConsoleCmdPathTest.canClimbLadders)
			{
				bool flag = pathFinder.canClimbLadders != ConsoleCmdPathTest.canClimbLadders;
			}
			pathInfo.canBreakBlocks = ConsoleCmdPathTest.canBreakBlocks;
			pathFinder.canClimbLadders = ConsoleCmdPathTest.canClimbLadders;
			pathFinder.canClimbWalls = ConsoleCmdPathTest.canClimbWalls;
			WorldRayHitInfo hitInfo = player.HitInfo;
			if (hitInfo.bHitValid)
			{
				if (Input.GetMouseButton(2))
				{
					pathInfo.targetPos = World.blockToTransformPos(hitInfo.hit.blockPos);
					ConsoleCmdPathTest.recalculatePath = true;
				}
				else if (Input.GetMouseButton(3))
				{
					pathInfo.hasStart = !pathInfo.hasStart;
					pathInfo.startPos = World.blockToTransformPos(hitInfo.hit.blockPos);
					ConsoleCmdPathTest.recalculatePath = true;
				}
			}
			if (ConsoleCmdPathTest.recalculatePath)
			{
				ConsoleCmdPathTest.recalculatePath = false;
				if (this.aiPathTest != null)
				{
					pathNodes = null;
					this.aiPathTest.SetTargetMove(pathInfo.targetPos, player, ConsoleCmdPathTest.canBreakBlocks);
				}
				else if (pathInfo.state == PathInfo.State.Done)
				{
					ConsoleCmdPathTest.recalculatePath = false;
					pathFinder.Calculate(pathInfo.hasStart ? pathInfo.startPos : player.position);
				}
			}
			if (pathNodes != null)
			{
				for (int i = 1; i < pathNodes.Length; i++)
				{
					AstarVoxelGrid.VoxelNode voxelNode = (AstarVoxelGrid.VoxelNode)pathNodes[i - 1];
					AstarVoxelGrid.VoxelNode voxelNode2 = (AstarVoxelGrid.VoxelNode)pathNodes[i];
					Vector3 vector = (Vector3)voxelNode.position;
					Vector3 vector2 = (Vector3)voxelNode2.position;
					uint num = Math.Max(voxelNode.Tag, voxelNode2.Tag);
					Color color = Color.white;
					switch (num)
					{
					case 0U:
						color = Color.green;
						break;
					case 1U:
						color = Color.grey;
						break;
					case 2U:
						color = Color.yellow;
						break;
					case 3U:
						color = Color.blue;
						break;
					case 4U:
						color = Color.cyan;
						break;
					}
					Debug.DrawLine(vector, vector2, color, 0.11f);
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "enable a path testing utility mode";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine executionsCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAIPathTest aiPathTest;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool recalculatePath = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canBreakBlocks = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canClimbLadders = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canClimbWalls = false;
}
