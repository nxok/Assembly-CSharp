using System;
using UnityEngine;

public class RenderTextureSystem
{
	public void Create(string _name, GameObject _target, Vector3 _targetRelPos, Vector3 _lightSourceRelPos, Vector2i _renderTexSize, bool _isAA, bool _orthographic = false, float _orthoSize = 1f)
	{
		GameObject gameObject = GameObject.Find("3D RenderTextures");
		if (gameObject == null)
		{
			gameObject = new GameObject("3D RenderTextures");
			gameObject.layer = 11;
		}
		this.ParentGO = new GameObject(_name);
		this.ParentGO.transform.parent = gameObject.transform;
		this.ParentGO.layer = 11;
		this.TargetGO = _target;
		this.TargetGO.transform.parent = this.ParentGO.transform;
		this.TargetGO.transform.localPosition = _targetRelPos;
		this.TargetGO.layer = 11;
		this.CameraGO = new GameObject("Camera");
		this.CameraGO.transform.parent = this.ParentGO.transform;
		this.CameraGO.transform.LookAt(_target.transform);
		this.CameraGO.layer = 11;
		this.cam = this.CameraGO.AddComponent<Camera>();
		this.cam.nearClipPlane = 0.01f;
		this.cam.farClipPlane = 20f;
		this.cam.cullingMask = 2048;
		this.cam.renderingPath = 1;
		this.cam.clearFlags = 2;
		if (_orthographic)
		{
			this.cam.orthographic = _orthographic;
			this.cam.orthographicSize = _orthoSize;
			Bounds bounds;
			bounds..ctor(Vector3.zero, Vector3.zero);
			foreach (Renderer renderer in this.TargetGO.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}
			this.TargetGO.transform.localPosition = _targetRelPos - bounds.center / 2f;
			if (bounds.size.y > bounds.size.x && bounds.size.y > bounds.size.z)
			{
				this.cam.orthographicSize = bounds.size.magnitude / 2f * _orthoSize;
			}
			else
			{
				this.cam.orthographicSize = bounds.size.magnitude / 3f * _orthoSize;
			}
		}
		int num = (_isAA ? 2 : 1);
		this.RenderTex = new RenderTexture(_renderTexSize.x * num, _renderTexSize.y * num, 24);
		this.RenderTex.autoGenerateMips = _isAA;
		this.RenderTex.useMipMap = _isAA;
		this.cam.targetTexture = this.RenderTex;
		this.LightGO = new GameObject("Light");
		this.LightGO.transform.parent = this.ParentGO.transform;
		this.LightGO.transform.localPosition = _lightSourceRelPos;
		this.LightGO.layer = 11;
		Light light = this.LightGO.AddComponent<Light>();
		light.type = 2;
		light.intensity = 1.5f;
		light.bounceIntensity = 0f;
		light.range = _targetRelPos.magnitude * 10f;
		light.cullingMask = 2048;
	}

	public void SetTarget(GameObject _target, Vector3 _targetRelPos, bool _orthographic, float _orthoSize = 1f)
	{
		if (this.TargetGO != null)
		{
			Object.Destroy(this.TargetGO);
		}
		this.TargetGO = Object.Instantiate<GameObject>(_target);
		this.TargetGO.transform.parent = this.ParentGO.transform;
		this.TargetGO.transform.localPosition = Vector3.zero;
		this.recursiveLayerSetup(this.TargetGO, 11);
		if (_orthographic)
		{
			this.cam.orthographic = _orthographic;
			this.cam.orthographicSize = _orthoSize;
			Bounds bounds;
			bounds..ctor(Vector3.zero, Vector3.zero);
			foreach (Renderer renderer in this.TargetGO.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}
			this.TargetGO.transform.localPosition = _targetRelPos - bounds.center / 2f;
			if (bounds.size.y > bounds.size.x && bounds.size.y > bounds.size.z)
			{
				this.cam.orthographicSize = bounds.size.magnitude / 2f * _orthoSize;
				return;
			}
			this.cam.orthographicSize = bounds.size.magnitude / 3f * _orthoSize;
		}
	}

	public void SetTargetNoCopy(GameObject _target, Vector3 _targetRelPos, bool _orthographic, float _orthoSize = 1f)
	{
		if (this.TargetGO != null)
		{
			Object.Destroy(this.TargetGO);
		}
		Light component = this.LightGO.GetComponent<Light>();
		component.range = 14f;
		component.intensity = 2f;
		this.TargetGO = _target;
		this.TargetGO.transform.parent = this.ParentGO.transform;
		this.TargetGO.transform.localPosition = Vector3.zero;
		this.recursiveLayerSetup(_target, 11);
		if (_orthographic)
		{
			this.cam.orthographic = _orthographic;
			this.cam.orthographicSize = _orthoSize;
			this.cam.backgroundColor = new Color32(0, 0, 0, 0);
			Bounds bounds;
			bounds..ctor(Vector3.zero, Vector3.zero);
			foreach (Renderer renderer in this.TargetGO.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}
			this.TargetGO.transform.localPosition = _targetRelPos - bounds.center / 2f;
		}
	}

	public void SetOrtho(bool enabled, float _orthoSize)
	{
		this.cam.orthographic = enabled;
		this.cam.orthographicSize = _orthoSize;
	}

	public void RotateTarget(float _amount)
	{
		if (this.TargetGO != null)
		{
			Vector3 eulerAngles = this.TargetGO.transform.localRotation.eulerAngles;
			this.TargetGO.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y + _amount, eulerAngles.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recursiveLayerSetup(GameObject _go, int _layer)
	{
		Transform transform = _go.transform;
		transform.gameObject.layer = _layer;
		foreach (object obj in transform)
		{
			Transform transform2 = (Transform)obj;
			transform2.gameObject.layer = _layer;
			this.recursiveLayerSetup(transform2.gameObject, _layer);
		}
	}

	public void Create(string _name, Camera _existingCamera, Vector2i _renderTexSize)
	{
		GameObject gameObject = GameObject.Find("3D RenderTextures");
		if (gameObject == null)
		{
			gameObject = new GameObject("3D RenderTextures");
			gameObject.layer = 11;
		}
		this.ParentGO = new GameObject(_name);
		this.ParentGO.transform.parent = gameObject.transform;
		this.ParentGO.layer = 11;
		this.RenderTex = new RenderTexture(_renderTexSize.x, _renderTexSize.y, 24);
		_existingCamera.targetTexture = this.RenderTex;
	}

	public void SetEnabled(bool _b)
	{
		this.ParentGO.SetActive(_b);
	}

	public void Cleanup()
	{
		if (this.ParentGO != null)
		{
			Object.Destroy(this.ParentGO);
		}
		if (this.RenderTex != null)
		{
			Object.Destroy(this.RenderTex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cSceneParentName = "3D RenderTextures";

	public GameObject ParentGO;

	public GameObject CameraGO;

	public GameObject TargetGO;

	public GameObject LightGO;

	public RenderTexture RenderTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera cam;
}
