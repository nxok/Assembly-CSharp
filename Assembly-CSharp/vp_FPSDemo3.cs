using System;
using UnityEngine;

public class vp_FPSDemo3 : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.m_FPSCamera = Object.FindAnyObjectByType<vp_FPCamera>();
		this.m_Demo = new vp_FPSDemoManager(this.PlayerGameObject);
		this.m_Demo.CurrentFullScreenFadeTime = Time.time;
		this.m_Demo.DrawCrosshair = false;
		this.m_Demo.FadeGUIOnCursorLock = false;
		this.m_Demo.Input.MouseCursorZones = new Rect[2];
		this.m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
		this.m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
		vp_Utility.LockCursor = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.m_Demo.Update();
		if (Vector3.Distance(this.PlayerGameObject.transform.position, this.m_StartPos) > 100f)
		{
			this.m_Demo.Teleport(this.m_StartPos, this.m_StartAngle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoIntro()
	{
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.FirstFrame = false;
			this.m_Demo.DrawCrosshair = false;
			this.m_Demo.FreezePlayer(this.m_OverviewPos, this.m_OverviewAngle, true);
			this.m_Demo.Input.MouseCursorForced = true;
			this.m_BodyAnimator = Object.FindAnyObjectByType<vp_BodyAnimator>();
			if (this.m_BodyAnimator != null)
			{
				this.m_BodyAnimator.gameObject.SetActive(false);
			}
		}
		this.m_Demo.DrawBoxes("welcome", "Featuring the SMOOTHEST CONTROLS and the most POWERFUL FPS CAMERA\navailable for Unity, Ultimate FPS is an awesome script pack for achieving that special\n 'AAA FPS' feeling. This demo will walk you through some of its core features ...\n", null, this.ImageRightArrow, null, null, true);
		this.m_Demo.ForceCameraShake();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoGameplay()
	{
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.FirstFrame = false;
			this.m_Demo.DrawCrosshair = true;
			this.m_Demo.UnFreezePlayer();
			this.m_Demo.Teleport(this.m_StartPos, this.m_StartAngle);
			vp_Utility.LockCursor = true;
			this.m_Demo.Input.MouseCursorForced = false;
			if (this.m_BodyAnimator != null)
			{
				this.m_BodyAnimator.gameObject.SetActive(true);
			}
		}
		this.m_Demo.DrawBoxes("part i: some examples", "This level has some basic gameplay features.\n• Press SHIFT to SPRINT, C to CROUCH, and the RIGHT MOUSE BUTTON to AIM.\n• To SWITCH WEAPONS, press Q, E or 1-3.\n• Press R to RELOAD, F to INTERACT and V for 3RD PERSON.", this.ImageLeftArrow, this.ImageRightArrow, delegate
		{
			this.m_Demo.LoadLevel(1);
		}, null, true);
		if (this.m_Demo.ShowGUI && vp_Utility.LockCursor && !this.m_LoadingNextLevel && !this.m_Demo.ClosingDown)
		{
			GUI.color = new Color(1f, 1f, 1f, this.m_Demo.ClosingDown ? this.m_Demo.GlobalAlpha : 1f);
			GUI.Label(new Rect((float)(Screen.width / 2 - 200), 140f, 400f, 20f), "(Press ENTER to reenable mouse cursor.)", this.m_Demo.CenterStyle);
			GUI.color = new Color(1f, 1f, 1f, 1f * this.m_Demo.GlobalAlpha);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoOutro()
	{
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.FirstFrame = false;
			this.m_Demo.DrawCrosshair = false;
			this.m_Demo.FreezePlayer(this.m_OutroPos, this.m_OutroAngle, true);
			this.m_Demo.Input.MouseCursorForced = true;
			this.m_OutroStartTime = Time.time;
		}
		this.m_FPSCamera.Angle = new Vector2(this.m_OutroAngle.x, this.m_OutroAngle.y + Mathf.Cos((Time.time - this.m_OutroStartTime + 50f) * 0.03f) * 20f);
		this.m_Demo.DrawBoxes("putting it all together", "Included in the package is full, well commented C# source code, an in-depth 70-page MANUAL in PDF format, a game-ready FPS PLAYER prefab along with all the scripts and content used in this demo. A FANTASTIC starting point (or upgrade) for any FPS project.\nBest part? It can be yours in a minute. GET IT NOW on visionpunk.com!", this.ImageLeftArrow, this.ImageCheckmark, delegate
		{
			this.m_Demo.LoadLevel(0);
		}, null, true);
		this.m_Demo.DrawImage(this.ImageAllParams);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		this.m_Demo.OnGUI();
		switch (this.m_Demo.CurrentScreen)
		{
		case 1:
			this.DemoIntro();
			return;
		case 2:
			this.DemoGameplay();
			return;
		case 3:
			this.DemoOutro();
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPCamera m_FPSCamera;

	public GameObject PlayerGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPSDemoManager m_Demo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_BodyAnimator m_BodyAnimator;

	public Texture ImageLeftArrow;

	public Texture ImageRightArrow;

	public Texture ImageCheckmark;

	public Texture ImagePresetDialogs;

	public Texture ImageShooter;

	public Texture ImageAllParams;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_StartPos = new Vector3(113f, 106f, -87f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_OverviewPos = new Vector3(113f, 106f, -87f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_OutroPos = new Vector3(135f, 105.8f, -70.7f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_StartAngle = new Vector2(13f, 153.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_OverviewAngle = new Vector2(13f, 153.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_OutroAngle = new Vector2(-19.3f, 241.7f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float m_OutroStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_LoadingNextLevel;
}
