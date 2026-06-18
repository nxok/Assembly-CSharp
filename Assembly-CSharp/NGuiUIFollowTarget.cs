using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Follow Target")]
public class NGuiUIFollowTarget : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.mTrans = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (this.target != null)
		{
			if (this.gameCamera == null)
			{
				this.gameCamera = NGUITools.FindCameraForLayer(this.target.gameObject.layer);
			}
			if (this.uiCamera == null)
			{
				this.uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
			}
			this.SetVisible(false);
			return;
		}
		Log.Error("Expected to have 'target' set to a valid transform", new object[] { this });
		base.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetVisible(bool val)
	{
		this.mIsVisible = val;
		int i = 0;
		int childCount = this.mTrans.childCount;
		while (i < childCount)
		{
			NGUITools.SetActive(this.mTrans.GetChild(i).gameObject, val);
			i++;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		if (this.target == null || this.gameCamera == null)
		{
			return;
		}
		Vector3 vector = this.gameCamera.WorldToViewportPoint(this.target.position + this.offset);
		bool flag = (this.gameCamera.orthographic || vector.z > 0f) && (!this.disableIfInvisible || (vector.x > 0f && vector.x < 1f && vector.y > 0f && vector.y < 1f));
		if (this.mIsVisible != flag)
		{
			this.SetVisible(flag);
		}
		if (flag)
		{
			base.transform.position = this.uiCamera.ViewportToWorldPoint(vector);
			vector = this.mTrans.localPosition;
			vector.x = (float)Mathf.FloorToInt(vector.x);
			vector.y = (float)Mathf.FloorToInt(vector.y);
			vector.z = 0f;
			this.mTrans.localPosition = vector;
		}
		this.OnUpdate(flag);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnUpdate(bool isVisible)
	{
	}

	public Transform target;

	public Camera gameCamera;

	public Camera uiCamera;

	public bool disableIfInvisible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool mIsVisible;

	public Vector3 offset = Vector3.zero;
}
