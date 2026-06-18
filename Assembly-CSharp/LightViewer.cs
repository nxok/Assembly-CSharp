using System;
using System.Collections.Generic;
using UnityEngine;

public class LightViewer : MonoBehaviour
{
	public static void SetEnabled(bool _on)
	{
		LightViewer.IsEnabled = _on;
		LightLOD.DebugViewDistance = (_on ? float.MaxValue : 0f);
	}

	public void OnDestroy()
	{
		this.Disable();
	}

	public void SetUpdateFrequency(float _updateFrequency)
	{
		this.updateFrequency = _updateFrequency;
	}

	public void Disable()
	{
		for (int i = 0; i < this.spheres.Count; i++)
		{
			Object.DestroyImmediate(this.spheres[i]);
		}
		this.spheres.Clear();
		this.TurnOnAllLights();
		this.allLights = null;
	}

	public void TurnOnAllLights()
	{
		foreach (KeyValuePair<Light, bool> keyValuePair in this.lightsOn)
		{
			if (keyValuePair.Key != null)
			{
				keyValuePair.Key.enabled = keyValuePair.Value;
			}
		}
		this.lightsOn.Clear();
	}

	public void TurnOffAllLights()
	{
		if (this.allLights == null)
		{
			this.allLights = Object.FindObjectsByType<Light>(0);
		}
		this.lightsOn.Clear();
		for (int i = 0; i < this.allLights.Length; i++)
		{
			if (this.allLights[i].type != 1)
			{
				this.lightsOn.Add(this.allLights[i], this.allLights[i].enabled);
				this.allLights[i].enabled = false;
			}
		}
	}

	public void Update()
	{
		if (this.lightsOn.Count > 0)
		{
			return;
		}
		if (!LightViewer.IsEnabled)
		{
			this.Disable();
			return;
		}
		if (this.sourceSphereClear == null)
		{
			this.sourceSphereClear = Resources.Load<GameObject>("Prefabs/LightViewerClear");
		}
		if (this.sourceSphereInc == null)
		{
			this.sourceSphereInc = Resources.Load<GameObject>("Prefabs/LightViewerInc");
		}
		if (this.sourceSphereInvInc == null)
		{
			this.sourceSphereInvInc = Resources.Load<GameObject>("Prefabs/LightViewerInvInc");
		}
		if (this.sourceSphereColor == null)
		{
			this.sourceSphereColor = Resources.Load<GameObject>("Prefabs/LightViewerColor");
		}
		if (this.sourceSphereInvColor == null)
		{
			this.sourceSphereInvColor = Resources.Load<GameObject>("Prefabs/LightViewerInvColor");
		}
		if (this.sourceSphereNoShadowColor == null)
		{
			this.sourceSphereNoShadowColor = Resources.Load<GameObject>("Prefabs/LightViewerNoShadowColor");
		}
		if (this.sourceSphereNoShadowInvColor == null)
		{
			this.sourceSphereNoShadowInvColor = Resources.Load<GameObject>("Prefabs/LightViewerNoShadowInvColor");
		}
		if ((this.prevCameraPos - Camera.main.transform.position).magnitude > 1f || Time.realtimeSinceStartup >= this.timeLastGathered + this.updateFrequency)
		{
			this.allLights = Object.FindObjectsByType<Light>(0);
			for (int i = 0; i < this.spheres.Count; i++)
			{
				Object.DestroyImmediate(this.spheres[i]);
			}
			this.spheres.Clear();
			if (this.noShadowList == null)
			{
				this.noShadowList = new List<Light>();
			}
			if (this.shadowList == null)
			{
				this.shadowList = new List<Light>();
			}
			for (int j = 0; j < this.allLights.Length; j++)
			{
				if (this.allLights[j].type != 1)
				{
					if (this.allLights[j].shadows == null)
					{
						this.noShadowList.Add(this.allLights[j]);
					}
					else
					{
						this.shadowList.Add(this.allLights[j]);
					}
				}
			}
			for (int k = 0; k < this.noShadowList.Count; k++)
			{
				if (this.noShadowList[k].type != 1)
				{
					GameObject gameObject = Object.Instantiate<GameObject>(this.sourceSphereClear);
					gameObject.transform.position = this.noShadowList[k].transform.position;
					gameObject.transform.localScale = Vector3.one * this.noShadowList[k].range * 2f;
					gameObject.transform.parent = GameManager.Instance.gameObject.transform;
					gameObject.GetComponent<Renderer>().sortingOrder = k;
					this.spheres.Add(gameObject);
					if (this.noShadowList[k].enabled)
					{
						bool flag = (Camera.main.transform.position - this.noShadowList[k].transform.position).magnitude < this.noShadowList[k].range;
						gameObject = Object.Instantiate<GameObject>(flag ? this.sourceSphereInvInc : this.sourceSphereInc);
						gameObject.transform.position = this.noShadowList[k].transform.position;
						gameObject.transform.localScale = Vector3.one * this.noShadowList[k].range * 2f;
						gameObject.transform.parent = GameManager.Instance.gameObject.transform;
						gameObject.GetComponent<Renderer>().sortingOrder = k + this.noShadowList.Count;
						this.spheres.Add(gameObject);
						gameObject = Object.Instantiate<GameObject>(flag ? this.sourceSphereNoShadowInvColor : this.sourceSphereNoShadowColor);
						gameObject.transform.position = this.noShadowList[k].transform.position;
						gameObject.transform.localScale = Vector3.one * this.noShadowList[k].range * 2f;
						gameObject.transform.parent = GameManager.Instance.gameObject.transform;
						gameObject.GetComponent<Renderer>().sortingOrder = k + this.noShadowList.Count * 2;
						this.spheres.Add(gameObject);
					}
				}
			}
			for (int l = 0; l < this.shadowList.Count; l++)
			{
				if (this.shadowList[l].type != 1)
				{
					GameObject gameObject2 = Object.Instantiate<GameObject>(this.sourceSphereClear);
					gameObject2.transform.position = this.shadowList[l].transform.position;
					gameObject2.transform.localScale = Vector3.one * this.shadowList[l].range * 2f;
					gameObject2.transform.parent = GameManager.Instance.gameObject.transform;
					gameObject2.GetComponent<Renderer>().sortingOrder = l + this.noShadowList.Count * 3;
					this.spheres.Add(gameObject2);
					if (this.shadowList[l].enabled)
					{
						bool flag2 = (Camera.main.transform.position - this.shadowList[l].transform.position).magnitude < this.shadowList[l].range;
						gameObject2 = Object.Instantiate<GameObject>(flag2 ? this.sourceSphereInvInc : this.sourceSphereInc);
						gameObject2.transform.position = this.shadowList[l].transform.position;
						gameObject2.transform.localScale = Vector3.one * this.shadowList[l].range * 2f;
						gameObject2.transform.parent = GameManager.Instance.gameObject.transform;
						gameObject2.GetComponent<Renderer>().sortingOrder = l + this.shadowList.Count + this.noShadowList.Count * 3;
						this.spheres.Add(gameObject2);
						gameObject2 = Object.Instantiate<GameObject>(flag2 ? this.sourceSphereInvColor : this.sourceSphereColor);
						gameObject2.transform.position = this.shadowList[l].transform.position;
						gameObject2.transform.localScale = Vector3.one * this.shadowList[l].range * 2f;
						gameObject2.transform.parent = GameManager.Instance.gameObject.transform;
						gameObject2.GetComponent<Renderer>().sortingOrder = l + this.shadowList.Count * 2 + this.noShadowList.Count * 3;
						this.spheres.Add(gameObject2);
					}
				}
			}
			this.noShadowList.Clear();
			this.shadowList.Clear();
			this.prevCameraPos = Camera.main.transform.position;
			this.timeLastGathered = Time.realtimeSinceStartup;
		}
	}

	public static bool IsEnabled;

	public static bool IsAllOff;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light[] allLights;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<Light, bool> lightsOn = new Dictionary<Light, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameObject> spheres = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject sourceSphereClear;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject sourceSphereInc;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject sourceSphereInvInc;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject sourceSphereColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject sourceSphereInvColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject sourceSphereNoShadowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject sourceSphereNoShadowInvColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeLastGathered;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float updateFrequency = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 prevCameraPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Light> noShadowList;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Light> shadowList;
}
