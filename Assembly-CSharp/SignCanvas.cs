using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SignCanvas : MonoBehaviour, ISignRenderingDataUpdateListener
{
	public SignCanvas.CanvasState State
	{
		get
		{
			return this._state;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			this._state = value;
			this._signDataAvailabilityDirty = true;
		}
	}

	public bool AllowedOnImposter
	{
		get
		{
			return this.allowedOnImposter;
		}
	}

	public float CanvasAspect
	{
		get
		{
			if (this.canvasDimensions.y == 0f)
			{
				return 1f;
			}
			return this.canvasDimensions.x / this.canvasDimensions.y;
		}
		set
		{
			this.canvasDimensions = new Vector2(value, 1f);
		}
	}

	public event Action RenderingDataUpdated;

	public Texture BakedTexture
	{
		get
		{
			return this.bakedTexture;
		}
	}

	public float SizeSquared
	{
		get
		{
			return this._sizeSquared;
		}
	}

	public GlobalSignId SignId
	{
		get
		{
			return this.State.SignId;
		}
		set
		{
			if (this.State.SignId.Equals(value))
			{
				return;
			}
			this.State.SignId = value;
			this._signDataAvailabilityDirty = true;
			SignDataManager.Instance.RegisterListener(this.DisplaySignId, this, true);
			SignTextureManager.Instance.Invalidate(this);
			this.RefreshRenderers();
			Action renderingDataUpdated = this.RenderingDataUpdated;
			if (renderingDataUpdated == null)
			{
				return;
			}
			renderingDataUpdated();
		}
	}

	public GlobalSignId DisplaySignId
	{
		get
		{
			if (this._signDataAvailabilityDirty)
			{
				SignData signData;
				this._signDataAvailable = SignDataManager.Instance.TryGetSignData(this.State.SignId, out signData);
				this._signDataAvailabilityDirty = false;
			}
			if (!this._signDataAvailable)
			{
				return SignDataManager.ErrorSignId;
			}
			return this.State.SignId;
		}
	}

	public SignCanvas.SignBlendMode BlendMode
	{
		get
		{
			return this.State.BlendMode;
		}
		set
		{
			if (this.State.BlendMode == value)
			{
				return;
			}
			this.State.BlendMode = value;
			this.RefreshRenderers();
		}
	}

	public bool ShowOnImposter
	{
		get
		{
			return this.State.ShowOnImposter;
		}
		set
		{
			this.State.ShowOnImposter = value;
		}
	}

	public CanvasRotationMode CanvasRotation
	{
		get
		{
			return this.State.CanvasRotation;
		}
		set
		{
			if (this.State.CanvasRotation == value)
			{
				return;
			}
			this.State.CanvasRotation = value;
			this.RefreshRenderers();
		}
	}

	public float CanvasRotationRadians
	{
		get
		{
			return (float)this.CanvasRotation * 3.1415927f * 0.5f;
		}
	}

	public bool IsDecal
	{
		get
		{
			using (List<SignRenderer>.Enumerator enumerator = this.SignRenderers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.IsDecal)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public void Initialize(SignDataManager.RenderingDataPatcher _alternativePatcher = null)
	{
		this._patchRenderingData = _alternativePatcher ?? new SignDataManager.RenderingDataPatcher(this.PatchRenderingData);
		base.GetComponentsInChildren<SignRenderer>(true, this.SignRenderers);
		this.CacheBoundsMetrics();
		this.Register();
		this.RefreshRenderers();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CacheBoundsMetrics()
	{
		if (this.SignRenderers.Count == 0)
		{
			this._localCenter = Vector3.zero;
			this._sizeSquared = 1f;
			return;
		}
		Bounds bounds = this.SignRenderers[0].Renderer.bounds;
		for (int i = 1; i < this.SignRenderers.Count; i++)
		{
			bounds.Encapsulate(this.SignRenderers[i].Renderer.bounds);
		}
		this._localCenter = base.transform.InverseTransformPoint(bounds.center);
		Vector3 size = bounds.size;
		float num = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
		this._sizeSquared = ((num > 0f) ? (num * num) : 1f);
	}

	public void SetTexture(Texture rt)
	{
		if (this.bakedTexture == rt)
		{
			return;
		}
		this.bakedTexture = rt;
		this.RefreshRenderers();
	}

	public Vector3 GetWorldPosition()
	{
		return base.transform.TransformPoint(this._localCenter) + Origin.position;
	}

	public void RefreshRenderers()
	{
		if (this.SignRenderers == null || this.SignRenderers.Count == 0)
		{
			return;
		}
		SignDataManager.Instance.TryApplyRenderingData(this.DisplaySignId, this._patchRenderingData, this.SignRenderers, this.State.BlendMode, null);
	}

	public void PatchRenderingData(MaterialPropertyBlock mpb)
	{
		if (this.bakedTexture != null)
		{
			mpb.SetInteger(SignShaderIDs._UseTexture, 1);
			mpb.SetTexture(SignShaderIDs._BakedTexture, this.bakedTexture);
		}
		else
		{
			mpb.SetInteger(SignShaderIDs._UseTexture, 0);
			mpb.SetTexture(SignShaderIDs._BakedTexture, Texture2D.whiteTexture);
		}
		mpb.SetFloat(SignShaderIDs._CanvasRotation, this.CanvasRotationRadians);
	}

	public void HandleRenderingDataUpdate()
	{
		this.RefreshRenderers();
		SignTextureManager.Instance.Invalidate(this);
		Action renderingDataUpdated = this.RenderingDataUpdated;
		if (renderingDataUpdated == null)
		{
			return;
		}
		renderingDataUpdated();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Register()
	{
		SignTextureManager.Instance.Register(this);
		SignDataManager.Instance.RegisterListener(this.DisplaySignId, this, true);
	}

	public void Cleanup()
	{
		SignDataManager.Instance.DeregisterListener(this);
		SignTextureManager.Instance.Deregister(this);
		this.bakedTexture = null;
	}

	[Conditional("DEBUG_LOG_SIGN_CANVAS")]
	public static void DebugLog(string message)
	{
		Log.Out(message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SignCanvas.CanvasState _state = new SignCanvas.CanvasState();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowedOnImposter = true;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 canvasDimensions = Vector2.one;

	public List<SignRenderer> SignRenderers = new List<SignRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture bakedTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SignDataManager.RenderingDataPatcher _patchRenderingData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool _signDataAvailabilityDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool _signDataAvailable = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 _localCenter = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float _sizeSquared = 1f;

	public enum SignBlendMode
	{
		Cutout,
		AlphaBlend
	}

	public class CanvasState
	{
		public SignCanvas.CanvasState Clone()
		{
			return new SignCanvas.CanvasState
			{
				SignId = this.SignId,
				BlendMode = this.BlendMode,
				ShowOnImposter = this.ShowOnImposter,
				CanvasRotation = this.CanvasRotation
			};
		}

		public void Write(BinaryWriter _bw)
		{
			this.SignId.ToStream(_bw);
			_bw.Write((byte)this.BlendMode);
			_bw.Write((byte)this.CanvasRotation);
			_bw.Write(this.ShowOnImposter);
		}

		public static SignCanvas.CanvasState Read(BinaryReader _br)
		{
			return new SignCanvas.CanvasState
			{
				SignId = GlobalSignId.FromStream(_br),
				BlendMode = (SignCanvas.SignBlendMode)_br.ReadByte(),
				CanvasRotation = (CanvasRotationMode)_br.ReadByte(),
				ShowOnImposter = _br.ReadBoolean()
			};
		}

		public GlobalSignId SignId = SignData.defaultSignDataID;

		public SignCanvas.SignBlendMode BlendMode;

		public bool ShowOnImposter;

		public CanvasRotationMode CanvasRotation;
	}
}
