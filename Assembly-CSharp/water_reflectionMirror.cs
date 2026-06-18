using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class water_reflectionMirror : MonoBehaviour
{
	public void OnWillRenderObject()
	{
		if (!base.enabled || !base.GetComponent<Renderer>() || !base.GetComponent<Renderer>().sharedMaterial || !base.GetComponent<Renderer>().enabled)
		{
			return;
		}
		Camera current = Camera.current;
		if (!current)
		{
			return;
		}
		if (water_reflectionMirror.s_InsideRendering)
		{
			return;
		}
		water_reflectionMirror.s_InsideRendering = true;
		Camera camera;
		this.CreateMirrorObjects(current, out camera);
		Vector3 position = base.transform.position;
		Vector3 up = base.transform.up;
		int pixelLightCount = QualitySettings.pixelLightCount;
		if (this.m_DisablePixelLights)
		{
			QualitySettings.pixelLightCount = 0;
		}
		this.UpdateCameraModes(current, camera);
		float num = -Vector3.Dot(up, position) - this.m_ClipPlaneOffset;
		Vector4 vector;
		vector..ctor(up.x, up.y, up.z, num);
		Matrix4x4 zero = Matrix4x4.zero;
		water_reflectionMirror.CalculateReflectionMatrix(ref zero, vector);
		Vector3 position2 = current.transform.position;
		Vector3 vector2 = zero.MultiplyPoint(position2);
		camera.worldToCameraMatrix = current.worldToCameraMatrix * zero;
		Vector4 vector3 = this.CameraSpacePlane(camera, position, up, 1f);
		Matrix4x4 projectionMatrix = current.projectionMatrix;
		water_reflectionMirror.CalculateObliqueMatrix(ref projectionMatrix, vector3);
		camera.projectionMatrix = projectionMatrix;
		camera.cullingMask = -17 & this.m_ReflectLayers.value;
		camera.targetTexture = water_reflectionMirror.m_ReflectionTexture;
		GL.invertCulling = true;
		camera.transform.position = vector2;
		Vector3 eulerAngles = current.transform.eulerAngles;
		camera.transform.eulerAngles = new Vector3(0f, eulerAngles.y, eulerAngles.z);
		camera.Render();
		camera.transform.position = position2;
		GL.invertCulling = false;
		Material[] sharedMaterials = base.GetComponent<Renderer>().sharedMaterials;
		foreach (Material material in sharedMaterials)
		{
			if (material.HasProperty("_ReflectionTex"))
			{
				material.SetTexture("_ReflectionTex", water_reflectionMirror.m_ReflectionTexture);
			}
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
		Vector3 lossyScale = base.transform.lossyScale;
		Matrix4x4 matrix4x2 = base.transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z));
		matrix4x2 = matrix4x * current.projectionMatrix * current.worldToCameraMatrix * matrix4x2;
		Material[] array = sharedMaterials;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetMatrix("_ProjMatrix", matrix4x2);
		}
		if (this.m_DisablePixelLights)
		{
			QualitySettings.pixelLightCount = pixelLightCount;
		}
		water_reflectionMirror.s_InsideRendering = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (water_reflectionMirror.m_ReflectionTexture)
		{
			Object.DestroyImmediate(water_reflectionMirror.m_ReflectionTexture);
			water_reflectionMirror.m_ReflectionTexture = null;
		}
		foreach (object obj in this.m_ReflectionCameras)
		{
			Object.DestroyImmediate(((Camera)((DictionaryEntry)obj).Value).gameObject);
		}
		this.m_ReflectionCameras.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCameraModes(Camera src, Camera dest)
	{
		if (dest == null)
		{
			return;
		}
		dest.clearFlags = src.clearFlags;
		dest.renderingPath = 1;
		dest.backgroundColor = src.backgroundColor;
		if (src.clearFlags == 1)
		{
			Skybox skybox = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox skybox2 = dest.GetComponent(typeof(Skybox)) as Skybox;
			if (!skybox || !skybox.material)
			{
				skybox2.enabled = false;
			}
			else
			{
				skybox2.enabled = true;
				skybox2.material = skybox.material;
			}
		}
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
	{
		reflectionCamera = null;
		if (!water_reflectionMirror.m_ReflectionTexture || this.m_OldReflectionTextureSize != this.m_TextureSize)
		{
			if (water_reflectionMirror.m_ReflectionTexture)
			{
				Object.DestroyImmediate(water_reflectionMirror.m_ReflectionTexture);
			}
			water_reflectionMirror.m_ReflectionTexture = new RenderTexture(this.m_TextureSize, this.m_TextureSize, 16);
			water_reflectionMirror.m_ReflectionTexture.name = "__MirrorReflection" + base.GetInstanceID().ToString();
			water_reflectionMirror.m_ReflectionTexture.isPowerOfTwo = true;
			water_reflectionMirror.m_ReflectionTexture.hideFlags = 52;
			this.m_OldReflectionTextureSize = this.m_TextureSize;
		}
		reflectionCamera = this.m_ReflectionCameras[currentCamera] as Camera;
		if (!reflectionCamera)
		{
			GameObject gameObject = new GameObject("Mirror Refl Camera id" + base.GetInstanceID().ToString() + " for " + currentCamera.GetInstanceID().ToString(), new Type[]
			{
				typeof(Camera),
				typeof(Skybox)
			});
			reflectionCamera = gameObject.GetComponent<Camera>();
			reflectionCamera.enabled = false;
			reflectionCamera.transform.position = base.transform.position;
			reflectionCamera.transform.rotation = base.transform.rotation;
			reflectionCamera.gameObject.AddComponent<FlareLayer>();
			gameObject.hideFlags = 61;
			this.m_ReflectionCameras[currentCamera] = reflectionCamera;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 vector = pos + normal * this.m_ClipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 vector2 = worldToCameraMatrix.MultiplyPoint(vector);
		Vector3 vector3 = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(vector3.x, vector3.y, vector3.z, -Vector3.Dot(vector2, vector3));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 vector = projection.inverse * new Vector4(water_reflectionMirror.sgn(clipPlane.x), water_reflectionMirror.sgn(clipPlane.y), 1f, 1f);
		Vector4 vector2 = clipPlane * (2f / Vector4.Dot(clipPlane, vector));
		projection[2] = vector2.x - projection[3];
		projection[6] = vector2.y - projection[7];
		projection[10] = vector2.z - projection[11];
		projection[14] = vector2.w - projection[15];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
	}

	public bool m_DisablePixelLights = true;

	public int m_TextureSize = 256;

	public float m_ClipPlaneOffset = 0.07f;

	public LayerMask m_ReflectLayers = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Hashtable m_ReflectionCameras = new Hashtable();

	public static RenderTexture m_ReflectionTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int m_OldReflectionTextureSize;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool s_InsideRendering;
}
