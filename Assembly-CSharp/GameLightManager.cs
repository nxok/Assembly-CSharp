using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLightManager
{
	public static GameLightManager Create(EntityPlayerLocal player)
	{
		GameLightManager gameLightManager = new GameLightManager();
		gameLightManager.player = player;
		gameLightManager.Init();
		return gameLightManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		GameLightManager.Instance = this;
		this.UpdateLightInit();
	}

	public void Destroy()
	{
		this.lights.Clear();
		this.priorityLights.Clear();
		this.removeLights.Clear();
		this.UpdateLightCleanup();
		GameLightManager.Instance = null;
	}

	public void FrameUpdate()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if (MeshDescription.bDebugStability || LightViewer.IsAllOff)
		{
			return;
		}
		this.isUpdating = true;
		Vector3 position = this.player.cameraTransform.position;
		int count = this.lights.Count;
		int num = (count + 19) / 20;
		if (this.lightUpdateIndex >= count)
		{
			this.lightUpdateIndex = 0;
		}
		for (int i = 0; i < num; i++)
		{
			LightLOD lightLOD = this.lights[this.lightUpdateIndex];
			if (lightLOD.priority <= 0f)
			{
				lightLOD.FrameUpdate(position);
				if (lightLOD.priority > 0f)
				{
					this.priorityLights.Add(lightLOD);
				}
			}
			int num2 = this.lightUpdateIndex + 1;
			this.lightUpdateIndex = num2;
			if (num2 >= count)
			{
				this.lightUpdateIndex = 0;
			}
		}
		int j = 0;
		while (j < this.priorityLights.Count)
		{
			LightLOD lightLOD2 = this.priorityLights[j];
			lightLOD2.FrameUpdate(position);
			if (lightLOD2.priority <= 0f)
			{
				this.priorityLights.RemoveAt(j);
			}
			else
			{
				j++;
			}
		}
		int count2 = this.removeLights.Count;
		if (count2 > 0)
		{
			for (int k = count2 - 1; k >= 0; k--)
			{
				LightLOD lightLOD3 = this.removeLights[k];
				this.RemoveLightFromLists(lightLOD3);
			}
			this.removeLights.Clear();
		}
		if (this.isWaterLevelChanged)
		{
			this.isWaterLevelChanged = false;
			foreach (LightLOD lightLOD4 in this.lights)
			{
				if (!lightLOD4.bWorksUnderwater)
				{
					lightLOD4.WaterLevelDirty = true;
				}
			}
		}
		this.isUpdating = false;
		this.UpdateLightFrameUpdate();
	}

	public void AddLight(LightLOD lightLOD)
	{
		this.lights.Add(lightLOD);
		lightLOD.priority = 1f;
		this.priorityLights.Add(lightLOD);
		if (OcclusionManager.Instance.cullLights)
		{
			OcclusionManager.AddLight(lightLOD);
		}
	}

	public void RemoveLight(LightLOD lightLOD)
	{
		if (this.isUpdating)
		{
			this.removeLights.Add(lightLOD);
		}
		else
		{
			this.RemoveLightFromLists(lightLOD);
		}
		if (OcclusionManager.Instance.cullLights)
		{
			OcclusionManager.RemoveLight(lightLOD);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveLightFromLists(LightLOD lightLOD)
	{
		int num = this.lights.IndexOf(lightLOD);
		if (num < 0)
		{
			Log.Warning("RemoveLightFromLists none");
			return;
		}
		this.lights.RemoveAt(num);
		if (num < this.lightUpdateIndex)
		{
			this.lightUpdateIndex--;
		}
		this.priorityLights.Remove(lightLOD);
	}

	public void MakeLightAPriority(LightLOD lightLOD)
	{
		if (lightLOD.priority <= 0f)
		{
			lightLOD.priority = 1f;
			this.priorityLights.Add(lightLOD);
		}
	}

	public Vector3 CameraPos()
	{
		return this.player.cameraTransform.position;
	}

	public void HandleWaterLevelChanged()
	{
		this.isWaterLevelChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLightInit()
	{
		for (int i = 0; i < this.fastULs.Length; i++)
		{
			this.fastULs[i] = new List<UpdateLight>(64);
		}
		for (int j = 0; j < this.slowULs.Length; j++)
		{
			this.slowULs[j] = new List<UpdateLight>(256);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLightCleanup()
	{
		this.newULs.Clear();
		for (int i = 0; i < this.fastULs.Length; i++)
		{
			this.fastULs[i] = null;
		}
		for (int j = 0; j < this.slowULs.Length; j++)
		{
			this.slowULs[j] = null;
		}
	}

	public void AddUpdateLight(UpdateLight _ul)
	{
		this.newULs.Add(_ul);
	}

	public void RemoveUpdateLight(UpdateLight _ul)
	{
		bool flag;
		if (_ul.IsDynamicObject)
		{
			int num = (_ul.GetHashCode() >> 2) & 3;
			flag = this.fastULs[num].Remove(_ul);
		}
		else
		{
			int num2 = (_ul.GetHashCode() >> 2) & 63;
			flag = this.slowULs[num2].Remove(_ul);
		}
		if (!flag && !this.newULs.Remove(_ul))
		{
			Log.Warning("RemoveUpdateLight {0} dy{1} missing!", new object[]
			{
				_ul.transform.name,
				_ul.IsDynamicObject
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLightFrameUpdate()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		float num = Time.deltaTime * 4f;
		this.fastULUpdateIndex = (this.fastULUpdateIndex + 1) & 3;
		List<UpdateLight> list = this.fastULs[this.fastULUpdateIndex];
		for (int i = list.Count - 1; i >= 0; i--)
		{
			UpdateLight updateLight = list[i];
			if (updateLight)
			{
				updateLight.UpdateLighting(num);
			}
			else
			{
				list.RemoveAt(i);
			}
		}
		this.slowULUpdateIndex = (this.slowULUpdateIndex + 1) & 63;
		List<UpdateLight> list2 = this.slowULs[this.slowULUpdateIndex];
		for (int j = list2.Count - 1; j >= 0; j--)
		{
			UpdateLight updateLight2 = list2[j];
			if (updateLight2)
			{
				if (updateLight2.appliedLit < 0f)
				{
					updateLight2.UpdateLighting(1f);
				}
			}
			else
			{
				list2.RemoveAt(j);
			}
		}
		int num2 = Utils.FastMin(160, this.newULs.Count);
		for (int k = 0; k < num2; k++)
		{
			UpdateLight updateLight3 = this.newULs[k];
			if (updateLight3)
			{
				updateLight3.ManagerFirstUpdate();
				int num3 = updateLight3.GetHashCode() >> 2;
				if (updateLight3.IsDynamicObject)
				{
					this.fastULs[num3 & 3].Add(updateLight3);
				}
				else
				{
					this.slowULs[num3 & 63].Add(updateLight3);
				}
			}
		}
		this.newULs.RemoveRange(0, num2);
	}

	public static GameLightManager Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<LightLOD> lights = new List<LightLOD>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<LightLOD> priorityLights = new List<LightLOD>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<LightLOD> removeLights = new List<LightLOD>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int lightUpdateIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUpdating;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isWaterLevelChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<UpdateLight> newULs = new List<UpdateLight>(512);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFastULGroups = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cFastULGroupMask = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<UpdateLight>[] fastULs = new List<UpdateLight>[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public int fastULUpdateIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cULGroups = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSlowULGroupMask = 63;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<UpdateLight>[] slowULs = new List<UpdateLight>[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public int slowULUpdateIndex;
}
