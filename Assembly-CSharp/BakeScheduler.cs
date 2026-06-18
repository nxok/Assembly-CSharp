using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class BakeScheduler
{
	public BakeScheduler.BakeJobState CurrentState
	{
		get
		{
			return this._current.State;
		}
	}

	public void Clear()
	{
		using (BakeScheduler.s_SignTextureManagerClear.Auto())
		{
			this._requests.Clear();
			this._index = 0;
			this._current.End();
			BakeScheduler.s_PendingBakeJobs.Value = 0;
			BakeScheduler.s_BakeJobProgress.Value = 0.0;
		}
	}

	public void ResetRequests(List<SignBakeRequest> newRequests)
	{
		using (BakeScheduler.s_SignTextureManagerResetRequests.Auto())
		{
			this._requests.Clear();
			for (int i = 0; i < newRequests.Count; i++)
			{
				this._requests.Add(newRequests[i]);
			}
			this._index = 0;
			this._current.End();
		}
	}

	public bool TickOnce(int tileSize, Material material, CommandBuffer cmd, SignTextureStore store, SignPrioritizer prioritizer, bool showProgress = false)
	{
		bool flag;
		using (BakeScheduler.s_SignTextureManagerTickOnce.Auto())
		{
			BakeScheduler.s_PendingBakeJobs.Value = this._requests.Count - this._index;
			if (this._current.State == BakeScheduler.BakeJobState.None)
			{
				while (this._index < this._requests.Count)
				{
					List<SignBakeRequest> requests = this._requests;
					int index = this._index;
					this._index = index + 1;
					SignBakeRequest signBakeRequest = requests[index];
					RenderTexture renderTexture = store.AcquireForBake(signBakeRequest.Tier);
					if (!(renderTexture == null))
					{
						this._current.Begin(signBakeRequest, renderTexture);
						break;
					}
					Log.Warning(string.Format("[STM] Pool exhausted at tier {0}; skipping requested bake.", signBakeRequest.Tier));
				}
				if (this._current.State == BakeScheduler.BakeJobState.None)
				{
					BakeScheduler.s_BakeJobProgress.Value = 0.0;
					return false;
				}
			}
			if (this._current.RT == null)
			{
				this._current.End();
				flag = true;
			}
			else
			{
				RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(this._current.RT);
				cmd.Clear();
				if (this._current.State == BakeScheduler.BakeJobState.Pending)
				{
					GlobalSignId groupSignId = prioritizer.GetGroupSignId(this._current.Request.GroupIndex);
					SignDataManager.Instance.TryApplyRenderingData(groupSignId, 1f, material, null, SignUIStyle.Baked);
					material.SetVector(SignShaderIDs._CanvasAspect, Vector2.one);
					cmd.SetRenderTarget(renderTargetIdentifier);
					cmd.ClearRenderTarget(true, true, Color.clear);
					if (showProgress)
					{
						prioritizer.ApplyToGroupCanvases(this._current.Request.GroupIndex, this._current.RT);
					}
					this._current.State = BakeScheduler.BakeJobState.InProgress;
				}
				if (this._current.State == BakeScheduler.BakeJobState.InProgress)
				{
					int num = Mathf.Max(this._current.RT.width / tileSize, 1);
					int num2 = num * num;
					int num3 = this._current.Step % num;
					int num4 = this._current.Step / num;
					Vector2 vector = Vector2.one / (float)num * 2f;
					Vector2 vector2;
					vector2..ctor(-1f + (float)num3 * vector.x, -1f + (float)num4 * vector.y);
					material.SetVector(SignShaderIDs._MinUV, vector2);
					material.SetVector(SignShaderIDs._MaxUV, vector2 + vector);
					cmd.Blit(0, renderTargetIdentifier, material, 0);
					Graphics.ExecuteCommandBuffer(cmd);
					this._current.Step = this._current.Step + 1;
					BakeScheduler.s_BakeJobProgress.Value = (double)(100f * (float)this._current.Step / (float)num2);
					if (this._current.Step >= num2)
					{
						this._current.State = BakeScheduler.BakeJobState.Complete;
					}
					flag = true;
				}
				else if (this._current.State == BakeScheduler.BakeJobState.Complete)
				{
					GlobalSignId groupSignId2 = prioritizer.GetGroupSignId(this._current.Request.GroupIndex);
					store.SetBaked(groupSignId2, this._current.Request.Tier, this._current.RT);
					prioritizer.ApplyToGroupCanvases(this._current.Request.GroupIndex, this._current.RT);
					this._current.End();
					flag = true;
				}
				else
				{
					Log.Error(string.Format("Unexpected case in SignTextureManager: TickOnce reached failsafe return case. Current job state is: {0}.", this._current.State));
					this._current.State = BakeScheduler.BakeJobState.None;
					flag = false;
				}
			}
		}
		return flag;
	}

	public static readonly ProfilerMarker s_SignTextureManagerClear = new ProfilerMarker("SignTextureManager.BakeScheduler.Clear");

	public static readonly ProfilerMarker s_SignTextureManagerResetRequests = new ProfilerMarker("SignTextureManager.BakeScheduler.ResetRequests");

	public static readonly ProfilerMarker s_SignTextureManagerTickOnce = new ProfilerMarker("SignTextureManager.BakeScheduler.TickOnce");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<double> s_BakeJobProgress = new ProfilerCounterValue<double>(ProfilerCategory.Scripts, "STM Job Progress", 4, 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_PendingBakeJobs = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "STM Pending Jobs", 3, 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SignBakeRequest> _requests = new List<SignBakeRequest>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public int _index;

	[PublicizedFrom(EAccessModifier.Private)]
	public BakeScheduler.BakeJob _current;

	public enum BakeJobState
	{
		None,
		Pending,
		InProgress,
		GeneratingMips,
		Complete
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BakeJob
	{
		public void Begin(SignBakeRequest request, RenderTexture rt)
		{
			this.Request = request;
			this.RT = rt;
			this.Step = 0;
			this.State = BakeScheduler.BakeJobState.Pending;
		}

		public void End()
		{
			this.RT = null;
			this.Step = 0;
			this.State = BakeScheduler.BakeJobState.None;
			this.Request = default(SignBakeRequest);
		}

		public SignBakeRequest Request;

		public RenderTexture RT;

		public int Step;

		public BakeScheduler.BakeJobState State;
	}
}
