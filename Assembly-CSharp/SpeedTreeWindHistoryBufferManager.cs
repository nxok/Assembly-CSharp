using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class SpeedTreeWindHistoryBufferManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public SpeedTreeWindHistoryBufferManager()
	{
	}

	public static SpeedTreeWindHistoryBufferManager Instance
	{
		get
		{
			if (SpeedTreeWindHistoryBufferManager.m_Instance == null)
			{
				SpeedTreeWindHistoryBufferManager.m_Instance = new SpeedTreeWindHistoryBufferManager();
			}
			return SpeedTreeWindHistoryBufferManager.m_Instance;
		}
	}

	public bool TryRegisterActiveRenderer(Renderer renderer)
	{
		bool flag;
		using (SpeedTreeWindHistoryBufferManager.s_ManagerTotal.Auto())
		{
			using (SpeedTreeWindHistoryBufferManager.s_ManagerRegistrations.Auto())
			{
				if (renderer == null)
				{
					Debug.LogError("Cannot register a null renderer.");
					flag = false;
				}
				else
				{
					SpeedTreeWindHistoryBufferManager.SharedMaterialGroup sharedMaterialGroup;
					if (!this.rendererToGroupMap.TryGetValue(renderer, out sharedMaterialGroup))
					{
						this.newMaterialsSet.Clear();
						this.tempMaterialsList.Clear();
						BillboardRenderer billboardRenderer = renderer as BillboardRenderer;
						if (billboardRenderer != null)
						{
							this.tempMaterialsList.Add(billboardRenderer.billboard.material);
						}
						else
						{
							renderer.GetSharedMaterials(this.tempMaterialsList);
						}
						foreach (Material material in this.tempMaterialsList)
						{
							if (!(material == null) && !this.materialToGroupMap.TryGetValue(material, out sharedMaterialGroup))
							{
								this.newMaterialsSet.Add(material);
							}
						}
						if (sharedMaterialGroup == null)
						{
							if (this.newMaterialsSet.Count == 0)
							{
								return false;
							}
							sharedMaterialGroup = new SpeedTreeWindHistoryBufferManager.SharedMaterialGroup();
							this.sharedMaterialGroups.Add(sharedMaterialGroup);
						}
						if (this.newMaterialsSet.Count > 0)
						{
							sharedMaterialGroup.MergeMaterials(this.newMaterialsSet);
							foreach (Material material2 in this.newMaterialsSet)
							{
								this.materialToGroupMap[material2] = sharedMaterialGroup;
							}
						}
						this.rendererToGroupMap[renderer] = sharedMaterialGroup;
						this.newMaterialsSet.Clear();
						this.tempMaterialsList.Clear();
					}
					sharedMaterialGroup.RegisterActiveRenderer(renderer);
					flag = true;
				}
			}
		}
		return flag;
	}

	public void DeregisterActiveRenderer(Renderer renderer)
	{
		using (SpeedTreeWindHistoryBufferManager.s_ManagerTotal.Auto())
		{
			using (SpeedTreeWindHistoryBufferManager.s_ManagerDeregistrations.Auto())
			{
				SpeedTreeWindHistoryBufferManager.SharedMaterialGroup sharedMaterialGroup;
				if (this.rendererToGroupMap.TryGetValue(renderer, out sharedMaterialGroup))
				{
					sharedMaterialGroup.DeregisterActiveRenderer(renderer);
				}
			}
		}
	}

	public void Update()
	{
		using (SpeedTreeWindHistoryBufferManager.s_ManagerTotal.Auto())
		{
			using (SpeedTreeWindHistoryBufferManager.s_ManagerUpdate.Auto())
			{
				foreach (SpeedTreeWindHistoryBufferManager.SharedMaterialGroup sharedMaterialGroup in this.sharedMaterialGroups)
				{
					sharedMaterialGroup.Update();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_GetMatProps = new ProfilerMarker("SpeedTreeWindPropertyBuffer.GetMatProps");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_SetMatProps = new ProfilerMarker("SpeedTreeWindPropertyBuffer.SetMatProps");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CheckVis = new ProfilerMarker("SpeedTreeWindPropertyBuffer.CheckVis");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerUpdate = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerUpdate");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerRegistrations = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerRegistrations");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerDeregistrations = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerDeregistrations");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerGetFirstRenderer = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerGetFirstRenderer");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ManagerTotal = new ProfilerMarker("SpeedTreeWindPropertyBuffer.ManagerTotal");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindVector = Shader.PropertyToID("_ST_WindVector");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindGlobal = Shader.PropertyToID("_ST_WindGlobal");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranch = Shader.PropertyToID("_ST_WindBranch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchTwitch = Shader.PropertyToID("_ST_WindBranchTwitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchWhip = Shader.PropertyToID("_ST_WindBranchWhip");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchAnchor = Shader.PropertyToID("_ST_WindBranchAnchor");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindBranchAdherences = Shader.PropertyToID("_ST_WindBranchAdherences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindTurbulences = Shader.PropertyToID("_ST_WindTurbulences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf1Ripple = Shader.PropertyToID("_ST_WindLeaf1Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf1Tumble = Shader.PropertyToID("_ST_WindLeaf1Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf1Twitch = Shader.PropertyToID("_ST_WindLeaf1Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf2Ripple = Shader.PropertyToID("_ST_WindLeaf2Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf2Tumble = Shader.PropertyToID("_ST_WindLeaf2Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindLeaf2Twitch = Shader.PropertyToID("_ST_WindLeaf2Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindFrondRipple = Shader.PropertyToID("_ST_WindFrondRipple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_WindAnimation = Shader.PropertyToID("_ST_WindAnimation");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindVector = Shader.PropertyToID("_ST_PF_WindVector");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindGlobal = Shader.PropertyToID("_ST_PF_WindGlobal");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranch = Shader.PropertyToID("_ST_PF_WindBranch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchTwitch = Shader.PropertyToID("_ST_PF_WindBranchTwitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchWhip = Shader.PropertyToID("_ST_PF_WindBranchWhip");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchAnchor = Shader.PropertyToID("_ST_PF_WindBranchAnchor");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindBranchAdherences = Shader.PropertyToID("_ST_PF_WindBranchAdherences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindTurbulences = Shader.PropertyToID("_ST_PF_WindTurbulences");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf1Ripple = Shader.PropertyToID("_ST_PF_WindLeaf1Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf1Tumble = Shader.PropertyToID("_ST_PF_WindLeaf1Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf1Twitch = Shader.PropertyToID("_ST_PF_WindLeaf1Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf2Ripple = Shader.PropertyToID("_ST_PF_WindLeaf2Ripple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf2Tumble = Shader.PropertyToID("_ST_PF_WindLeaf2Tumble");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindLeaf2Twitch = Shader.PropertyToID("_ST_PF_WindLeaf2Twitch");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindFrondRipple = Shader.PropertyToID("_ST_PF_WindFrondRipple");

	[PublicizedFrom(EAccessModifier.Private)]
	public static int _ST_PF_WindAnimation = Shader.PropertyToID("_ST_PF_WindAnimation");

	[PublicizedFrom(EAccessModifier.Private)]
	public static SpeedTreeWindHistoryBufferManager m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Renderer, SpeedTreeWindHistoryBufferManager.SharedMaterialGroup> rendererToGroupMap = new Dictionary<Renderer, SpeedTreeWindHistoryBufferManager.SharedMaterialGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Material, SpeedTreeWindHistoryBufferManager.SharedMaterialGroup> materialToGroupMap = new Dictionary<Material, SpeedTreeWindHistoryBufferManager.SharedMaterialGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Material> tempMaterialsList = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Material> newMaterialsSet = new HashSet<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<SpeedTreeWindHistoryBufferManager.SharedMaterialGroup> sharedMaterialGroups = new HashSet<SpeedTreeWindHistoryBufferManager.SharedMaterialGroup>();

	public class SharedMaterialGroup
	{
		public SharedMaterialGroup()
		{
			this.previousProperties = new MaterialPropertyBlock();
		}

		public void MergeMaterials(HashSet<Material> newMaterialsSet)
		{
			this.sharedMaterials.UnionWith(newMaterialsSet);
		}

		public void RegisterActiveRenderer(Renderer renderer)
		{
			this.activeRenderers.Add(renderer);
		}

		public void DeregisterActiveRenderer(Renderer renderer)
		{
			this.activeRenderers.Remove(renderer);
		}

		public void Update()
		{
			using (SpeedTreeWindHistoryBufferManager.s_CheckVis.Auto())
			{
				if (this.activeRenderers.Count == 0)
				{
					return;
				}
			}
			Renderer renderer = null;
			using (SpeedTreeWindHistoryBufferManager.s_ManagerGetFirstRenderer.Auto())
			{
				using (HashSet<Renderer>.Enumerator enumerator = this.activeRenderers.GetEnumerator())
				{
					if (!enumerator.MoveNext())
					{
						return;
					}
					renderer = enumerator.Current;
				}
			}
			using (SpeedTreeWindHistoryBufferManager.s_GetMatProps.Auto())
			{
				renderer.GetPropertyBlock(this.previousProperties);
			}
			using (SpeedTreeWindHistoryBufferManager.s_SetMatProps.Auto())
			{
				foreach (Material material in this.sharedMaterials)
				{
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindVector, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindVector));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindGlobal, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindGlobal));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindBranch, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindBranch));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindBranchTwitch, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindBranchTwitch));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindBranchWhip, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindBranchWhip));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindBranchAnchor, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindBranchAnchor));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindBranchAdherences, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindBranchAdherences));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindTurbulences, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindTurbulences));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindLeaf1Ripple, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindLeaf1Ripple));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindLeaf1Tumble, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindLeaf1Tumble));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindLeaf1Twitch, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindLeaf1Twitch));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindLeaf2Ripple, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindLeaf2Ripple));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindLeaf2Tumble, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindLeaf2Tumble));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindLeaf2Twitch, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindLeaf2Twitch));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindFrondRipple, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindFrondRipple));
					material.SetVector(SpeedTreeWindHistoryBufferManager._ST_PF_WindAnimation, this.previousProperties.GetVector(SpeedTreeWindHistoryBufferManager._ST_WindAnimation));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<Renderer> activeRenderers = new HashSet<Renderer>();

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<Material> sharedMaterials = new HashSet<Material>();

		[PublicizedFrom(EAccessModifier.Private)]
		public MaterialPropertyBlock previousProperties;
	}
}
