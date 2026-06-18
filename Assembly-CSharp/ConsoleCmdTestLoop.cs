using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTestLoop : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "testloop" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Test code in a loop";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\np - player";
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
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
		MicroStopwatch microStopwatch = new MicroStopwatch();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 1296978897U)
		{
			if (num <= 1094184410U)
			{
				if (num != 388133425U)
				{
					if (num == 1094184410U)
					{
						if (text == "meshnew")
						{
							this.meshes = new Mesh[10000];
							for (int i = 0; i < 10000; i++)
							{
								this.meshes[i] = new Mesh();
							}
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time new {0}ms", new object[] { (float)microStopwatch.ElapsedMicroseconds * 0.001f });
							return;
						}
					}
				}
				else if (text == "f2")
				{
					VoxelMeshTerrain voxelMeshTerrain = new VoxelMeshTerrain(5, 100000);
					Vector2 zero = Vector2.zero;
					ArrayListMP<Vector2> uvs = voxelMeshTerrain.Uvs;
					for (int j = 0; j < 100000; j++)
					{
						voxelMeshTerrain.m_Uvs.Add(zero);
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", new object[] { (float)microStopwatch.ElapsedMicroseconds * 0.001f });
					return;
				}
			}
			else if (num != 1159932677U)
			{
				if (num != 1277980168U)
				{
					if (num == 1296978897U)
					{
						if (text == "pd")
						{
							if (world != null)
							{
								EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
								for (int k = 0; k < 10000; k++)
								{
									GameUtils.FindDeepChildActive(primaryPlayer.transform, "test");
								}
								SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", new object[] { (float)microStopwatch.ElapsedMicroseconds * 0.001f });
								return;
							}
							return;
						}
					}
				}
				else if (text == "meshd")
				{
					if (this.meshes != null && this.meshes[0])
					{
						for (int l = 0; l < 10000; l++)
						{
							Object.Destroy(this.meshes[l]);
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time destroy {0}ms", new object[] { (float)microStopwatch.ElapsedMicroseconds * 0.001f });
						return;
					}
					return;
				}
			}
			else if (text == "containermove")
			{
				if (ConsoleCmdTestLoop.containerT)
				{
					if (!ConsoleCmdTestLoop.containerT.parent)
					{
						ConsoleCmdTestLoop.containerT.SetParent(ConsoleCmdTestLoop.containerBaseT, false);
						ConsoleCmdTestLoop.containerT.SetAsFirstSibling();
					}
					else
					{
						ConsoleCmdTestLoop.containerT.SetParent(null, false);
					}
					Log.Warning("container {0} of {1}", new object[]
					{
						ConsoleCmdTestLoop.containerT.hierarchyCount,
						ConsoleCmdTestLoop.containerT.hierarchyCapacity
					});
					return;
				}
				return;
			}
		}
		else if (num <= 3050838227U)
		{
			if (num != 1311535406U)
			{
				if (num != 2377118072U)
				{
					if (num == 3050838227U)
					{
						if (text == "meshclr")
						{
							if (this.meshes != null && this.meshes[0])
							{
								for (int m = 0; m < 10000; m++)
								{
									this.meshes[m].Clear(false);
								}
								SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time clear {0}ms", new object[] { (float)microStopwatch.ElapsedMicroseconds * 0.001f });
								return;
							}
							return;
						}
					}
				}
				else if (text == "container")
				{
					int num2 = 1;
					if (_params.Count >= 2)
					{
						int.TryParse(_params[1], out num2);
					}
					if (!ConsoleCmdTestLoop.containerT)
					{
						ConsoleCmdTestLoop.containerT = new GameObject("containerTest").transform;
						ConsoleCmdTestLoop.containerBaseT = new GameObject("containerBaseTest").transform;
					}
					for (int n = 0; n < num2; n++)
					{
						Transform transform = new GameObject("child").transform;
						transform.SetParent(ConsoleCmdTestLoop.containerT, false);
						transform.SetAsFirstSibling();
					}
					return;
				}
			}
			else if (text == "meshf")
			{
				MeshFilter meshFilter = new GameObject().AddComponent<MeshFilter>();
				Mesh mesh = new Mesh();
				mesh.name = "Test";
				Vector3[] array = new Vector3[] { Vector3.up };
				int[] array2 = new int[3];
				mesh.SetVertices(array);
				mesh.SetTriangles(array2, 0);
				meshFilter.sharedMesh = mesh;
				Mesh mesh2 = meshFilter.mesh;
				Log.Warning("sm {0} ({1:x}), mesh {2} ({3:x}) ", new object[]
				{
					mesh.name,
					mesh.GetInstanceID(),
					mesh2.name,
					mesh2.GetInstanceID()
				});
				Object.Destroy(mesh2);
				return;
			}
		}
		else if (num != 3809224601U)
		{
			if (num != 3819107837U)
			{
				if (num == 4111221743U)
				{
					if (text == "p")
					{
						if (world != null)
						{
							EntityPlayerLocal primaryPlayer2 = world.GetPrimaryPlayer();
							for (int num3 = 0; num3 < 10000; num3++)
							{
								primaryPlayer2.PlayerStats.UpdatePlayerHealthOT(0.05f);
							}
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", new object[] { (float)microStopwatch.ElapsedMicroseconds * 0.001f });
							return;
						}
						return;
					}
				}
			}
			else if (text == "mat")
			{
				MeshRenderer meshRenderer = new GameObject().AddComponent<MeshRenderer>();
				Material material = Object.Instantiate<Material>(Resources.Load<Material>("Materials/DistantPOI"));
				meshRenderer.sharedMaterial = material;
				Material material2 = meshRenderer.material;
				Log.Warning("mat {0} ({1:x}), m2 {2} ({3:x})", new object[]
				{
					material.name,
					material.GetInstanceID(),
					material2.name,
					material2.GetInstanceID()
				});
				Object.Destroy(material);
				Object.Destroy(material);
				Object.Destroy(material);
				return;
			}
		}
		else if (text == "f")
		{
			VoxelMeshTerrain voxelMeshTerrain2 = new VoxelMeshTerrain(5, 100000);
			Vector2 zero2 = Vector2.zero;
			for (int num4 = 0; num4 < 100000; num4++)
			{
				voxelMeshTerrain2.Uvs.Add(zero2);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", new object[] { (float)microStopwatch.ElapsedMicroseconds * 0.001f });
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMeshCount = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh[] meshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform containerT;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform containerBaseT;
}
