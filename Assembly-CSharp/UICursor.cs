using System;
using UnityEngine;

[RequireComponent(typeof(UISprite))]
[AddComponentMenu("NGUI/Examples/UI Cursor")]
public class UICursor : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		UICursor.instance = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		UICursor.instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mTrans = base.transform;
		this.mSprite = base.GetComponentInChildren<UISprite>();
		if (this.uiCamera == null)
		{
			this.uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		if (this.mSprite != null)
		{
			this.mAtlas = this.mSprite.atlas;
			this.mSpriteName = this.mSprite.spriteName;
			if (this.mSprite.depth < 100)
			{
				this.mSprite.depth = 100;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		if (this.uiCamera != null)
		{
			mousePosition.x = Mathf.Clamp01(mousePosition.x / (float)Screen.width);
			mousePosition.y = Mathf.Clamp01(mousePosition.y / (float)Screen.height);
			this.mTrans.position = this.uiCamera.ViewportToWorldPoint(mousePosition);
			if (this.uiCamera.orthographic)
			{
				Vector3 localPosition = this.mTrans.localPosition;
				localPosition.x = Mathf.Round(localPosition.x);
				localPosition.y = Mathf.Round(localPosition.y);
				this.mTrans.localPosition = localPosition;
				return;
			}
		}
		else
		{
			mousePosition.x -= (float)Screen.width * 0.5f;
			mousePosition.y -= (float)Screen.height * 0.5f;
			mousePosition.x = Mathf.Round(mousePosition.x);
			mousePosition.y = Mathf.Round(mousePosition.y);
			this.mTrans.localPosition = mousePosition;
		}
	}

	public static void Clear()
	{
		if (UICursor.instance != null && UICursor.instance.mSprite != null)
		{
			UICursor.Set(UICursor.instance.mAtlas, UICursor.instance.mSpriteName);
		}
	}

	public static void Set(INGUIAtlas atlas, string sprite)
	{
		if (UICursor.instance != null && UICursor.instance.mSprite)
		{
			UICursor.instance.mSprite.atlas = atlas;
			UICursor.instance.mSprite.spriteName = sprite;
			UICursor.instance.mSprite.MakePixelPerfect();
			UICursor.instance.Update();
		}
	}

	public static UICursor instance;

	public Camera uiCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UISprite mSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public INGUIAtlas mAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string mSpriteName;
}
