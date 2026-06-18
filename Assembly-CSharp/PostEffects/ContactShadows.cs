using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PostEffects
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public sealed class ContactShadows : MonoBehaviour
	{
		public Light Light
		{
			get
			{
				return this._light;
			}
			set
			{
				this._light = value;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDestroy()
		{
			if (this._material != null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(this._material);
				}
				else
				{
					Object.DestroyImmediate(this._material);
				}
			}
			if (this._prevMaskRT1 != null)
			{
				RenderTexture.ReleaseTemporary(this._prevMaskRT1);
			}
			if (this._prevMaskRT2 != null)
			{
				RenderTexture.ReleaseTemporary(this._prevMaskRT2);
			}
			if (this._command1 != null)
			{
				this._command1.Release();
			}
			if (this._command2 != null)
			{
				this._command2.Release();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPreCull()
		{
			this.UpdateTempObjects();
			if (this._light != null)
			{
				this.BuildCommandBuffer();
				this._light.AddCommandBuffer(3, this._command1);
				this._light.AddCommandBuffer(3, this._command2);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPreRender()
		{
			if (this._light != null)
			{
				this._light.RemoveCommandBuffer(3, this._command1);
				this._light.RemoveCommandBuffer(3, this._command2);
				this._command1.Clear();
				this._command2.Clear();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			base.GetComponent<Camera>().depthTextureMode |= 1;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Matrix4x4 CalculateVPMatrix()
		{
			Camera current = Camera.current;
			Matrix4x4 nonJitteredProjectionMatrix = current.nonJitteredProjectionMatrix;
			Matrix4x4 worldToCameraMatrix = current.worldToCameraMatrix;
			return GL.GetGPUProjectionMatrix(nonJitteredProjectionMatrix, true) * worldToCameraMatrix;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2Int GetScreenSize()
		{
			Camera current = Camera.current;
			int num = (this._downsample ? 2 : 1);
			return new Vector2Int(current.pixelWidth / num, current.pixelHeight / num);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateTempObjects()
		{
			if (this._prevMaskRT2 != null)
			{
				RenderTexture.ReleaseTemporary(this._prevMaskRT2);
				this._prevMaskRT2 = null;
			}
			if (this._light == null)
			{
				return;
			}
			if (this._material == null)
			{
				this._material = new Material(this._shader);
				this._material.hideFlags = 52;
			}
			if (this._command1 == null)
			{
				this._command1 = new CommandBuffer();
				this._command2 = new CommandBuffer();
				this._command1.name = "Contact Shadow Ray Tracing";
				this._command2.name = "Contact Shadow Temporal Filter";
			}
			else
			{
				this._command1.Clear();
				this._command2.Clear();
			}
			this._material.SetFloat("_RejectionDepth", this._rejectionDepth);
			this._material.SetInt("_SampleCount", this._sampleCount);
			float num = Mathf.Pow(1f - this._temporalFilter, 2f);
			this._material.SetFloat("_Convergence", num);
			this._material.SetVector("_LightVector", base.transform.InverseTransformDirection(-this._light.transform.forward) * this._light.shadowBias / ((float)this._sampleCount - 1.5f));
			Texture2D texture = this._noiseTextures.GetTexture();
			Vector2 vector = this.GetScreenSize() / (float)texture.width;
			this._material.SetVector("_NoiseScale", vector);
			this._material.SetTexture("_NoiseTex", texture);
			this._material.SetMatrix("_Reprojection", this._previousVP * base.transform.localToWorldMatrix);
			this._previousVP = ContactShadows.CalculateVPMatrix();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void BuildCommandBuffer()
		{
			Vector2Int screenSize = this.GetScreenSize();
			RenderTextureFormat renderTextureFormat = 16;
			RenderTexture temporary = RenderTexture.GetTemporary(screenSize.x, screenSize.y, 0, renderTextureFormat);
			if (this._temporalFilter == 0f)
			{
				this._command1.SetGlobalTexture(Shader.PropertyToID("_ShadowMask"), 1);
				this._command1.SetRenderTarget(temporary);
				this._command1.DrawProcedural(Matrix4x4.identity, this._material, 0, 0, 3);
			}
			else
			{
				int num = Shader.PropertyToID("_UnfilteredMask");
				this._command1.SetGlobalTexture(Shader.PropertyToID("_ShadowMask"), 1);
				this._command1.GetTemporaryRT(num, screenSize.x, screenSize.y, 0, 0, renderTextureFormat);
				this._command1.SetRenderTarget(num);
				this._command1.DrawProcedural(Matrix4x4.identity, this._material, 0, 0, 3);
				this._command1.SetGlobalTexture(Shader.PropertyToID("_PrevMask"), this._prevMaskRT1);
				this._command1.SetRenderTarget(temporary);
				this._command1.DrawProcedural(Matrix4x4.identity, this._material, 1 + (Time.frameCount & 1), 0, 3);
			}
			if (this._downsample)
			{
				this._command2.SetGlobalTexture(Shader.PropertyToID("_TempMask"), temporary);
				this._command2.DrawProcedural(Matrix4x4.identity, this._material, 3, 0, 3);
			}
			else
			{
				this._command2.Blit(temporary, 1);
			}
			this._prevMaskRT2 = this._prevMaskRT1;
			this._prevMaskRT1 = temporary;
		}

		[SerializeField]
		[PublicizedFrom(EAccessModifier.Private)]
		public Light _light;

		[SerializeField]
		[Range(0f, 5f)]
		[PublicizedFrom(EAccessModifier.Private)]
		public float _rejectionDepth = 0.05f;

		[SerializeField]
		[Range(4f, 32f)]
		[PublicizedFrom(EAccessModifier.Private)]
		public int _sampleCount = 16;

		[SerializeField]
		[Range(0f, 1f)]
		[PublicizedFrom(EAccessModifier.Private)]
		public float _temporalFilter = 0.5f;

		[SerializeField]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool _downsample = true;

		[SerializeField]
		[HideInInspector]
		[PublicizedFrom(EAccessModifier.Private)]
		public Shader _shader;

		[SerializeField]
		[HideInInspector]
		[PublicizedFrom(EAccessModifier.Private)]
		public NoiseTextureSet _noiseTextures;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Material _material;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public RenderTexture _prevMaskRT1;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public RenderTexture _prevMaskRT2;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public CommandBuffer _command1;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public CommandBuffer _command2;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Matrix4x4 _previousVP = Matrix4x4.identity;
	}
}
