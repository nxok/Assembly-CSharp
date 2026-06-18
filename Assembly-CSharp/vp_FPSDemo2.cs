using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPSDemo2 : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.m_FPSCamera = Object.FindAnyObjectByType<vp_FPCamera>();
		this.m_Demo = new vp_FPSDemoManager(this.Player);
		this.m_Demo.CurrentFullScreenFadeTime = Time.time;
		this.m_Demo.DrawCrosshair = false;
		this.m_Demo.Input.MouseCursorZones = new Rect[3];
		this.m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
		this.m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
		this.m_Demo.Input.MouseCursorZones[2] = new Rect(0f, 0f, 150f, (float)Screen.height);
		vp_Utility.LockCursor = false;
		this.m_LookPoints[1] = new Vector3(129.3f, 122f, -186f);
		this.m_LookPoints[2] = new Vector3(129.3f, 85f, -186f);
		this.m_LookPoints[3] = new Vector3(147f, 85f, -186f);
		this.m_LookPoints[4] = new Vector3(12f, 85f, -214f);
		this.m_LookPoints[5] = new Vector3(129f, 122f, -118f);
		this.m_LookPoints[6] = new Vector3(125.175f, 106.1071f, -97.58212f);
		this.m_LookPoints[7] = new Vector3(119.6f, 104.2f, -89.1f);
		this.m_LookPoints[8] = new Vector3(129f, 112f, -150f);
		this.m_Demo.PlayerEventHandler.SetWeapon.Disallow(10000000f);
		this.m_Demo.PlayerEventHandler.SetPrevWeapon.Try = () => false;
		this.m_Demo.PlayerEventHandler.SetNextWeapon.Try = () => false;
		this.m_Demo.PlayerEventHandler.FallImpact.Register(this, "FallImpact", 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.m_Demo.Update();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoPhysics()
	{
		this.m_Demo.DrawBoxes("part iii: physics", "Ultimate FPS features a cool, tweakable MOTOR and PHYSICS simulation.\nAll motion is forwarded to the camera and weapon for some CRAZY MOVES that you won't see in an everyday FPS. Click these buttons for some quick examples ...", null, this.ImageRightArrow, null, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.DrawCrosshair = true;
			this.m_Demo.Teleport(this.m_SlimePos, this.m_SlimeAngle);
			this.m_Demo.FirstFrame = false;
			this.m_Demo.ButtonSelection = 0;
			this.m_Demo.Camera.SnapSprings();
			this.m_Demo.RefreshDefaultState();
			this.m_Demo.Input.MouseCursorForced = true;
			this.m_Demo.Teleport(this.m_SlimePos, this.m_SlimeAngle);
			this.m_Demo.WeaponHandler.SetWeapon(1);
			this.m_ExamplesCurrentSel = -1;
			this.m_RunForward = false;
			this.m_LookPoint = 0;
			this.m_Demo.LockControls();
		}
		if (this.m_Demo.ShowGUI && !this.m_GoAgainTimer.Active && this.m_Demo.ButtonSelection != 3)
		{
			GUI.color = new Color(1f, 1f, 1f, this.m_GoAgainButtonAlpha);
			this.m_GoAgainButtonAlpha = Mathf.Lerp(0f, 1f, this.m_GoAgainButtonAlpha + Time.deltaTime);
			if (GUI.Button(new Rect((float)(Screen.width / 2 - 60), 210f, 120f, 30f), "Go again!"))
			{
				this.m_GoAgainButtonAlpha = 0f;
				this.m_ExamplesCurrentSel = -1;
			}
			GUI.color = new Color(1f, 1f, 1f, 1f * this.m_Demo.GlobalAlpha);
		}
		if (this.m_Demo.ButtonSelection != this.m_ExamplesCurrentSel)
		{
			this.m_WASDInfoClickTime = Time.time;
			this.m_Demo.Controller.Stop();
			this.m_Jump = false;
			this.m_LookPoint = 0;
			this.m_GoAgainButtonAlpha = 0f;
			this.m_ForceTimer.Cancel();
			this.m_Demo.Controller.Stop();
			this.m_Demo.PlayerEventHandler.RefreshActivityStates();
			this.m_Demo.Input.MouseCursorForced = true;
			this.m_Demo.Controller.PhysicsSlopeSlidiness = 0.15f;
			this.m_Demo.Controller.MotorAirSpeed = 0.7f;
			this.m_Demo.Controller.MotorAcceleration = 0.18f;
			this.m_Demo.Controller.PhysicsWallBounce = 0f;
			this.m_HeadBumpTimer1.Cancel();
			this.m_HeadBumpTimer2.Cancel();
			this.m_ActionTimer1.Cancel();
			this.m_ActionTimer2.Cancel();
			this.m_ActionTimer3.Cancel();
			this.m_ActionTimer4.Cancel();
			this.m_ActionTimer5.Cancel();
			this.m_GoAgainTimer.Cancel();
			this.m_ForceTimer.Cancel();
			vp_Utility.LockCursor = true;
			this.m_Demo.Camera.SnapSprings();
			if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				this.m_Demo.Camera.SnapSprings();
			}
			this.m_Demo.PlayerEventHandler.Platform.Set(null);
			switch (this.m_Demo.ButtonSelection)
			{
			case 0:
				vp_Timer.In(29f, delegate
				{
				}, this.m_GoAgainTimer);
				this.m_Demo.Teleport(this.m_SlimePos, this.m_SlimeAngle);
				break;
			case 1:
				vp_Timer.In(5f, delegate
				{
				}, this.m_GoAgainTimer);
				this.m_Demo.Teleport(this.m_WetRoofPos, this.m_WetRoofAngle);
				this.m_Demo.Controller.PhysicsSlopeSlidiness = 1f;
				break;
			case 2:
				this.m_Demo.Controller.MotorAirSpeed = 0f;
				this.m_Demo.Teleport(this.m_ActionPos, this.m_ActionAngle);
				this.m_Demo.SnapLookAt(this.m_LookPoints[1]);
				this.m_LookPoint = 1;
				this.m_RunForward = true;
				vp_Timer.In(1.75f, delegate
				{
					this.m_LookPoint = 2;
					this.m_Jump = true;
					this.m_Demo.LookDamping = 1f;
				}, this.m_ActionTimer1);
				vp_Timer.In(2.25f, delegate
				{
					this.m_LookPoint = 3;
					this.m_Jump = false;
					this.m_Demo.LookDamping = 1f;
				}, this.m_ActionTimer2);
				vp_Timer.In(3.5f, delegate
				{
					this.m_LookPoint = 4;
					this.m_Demo.Controller.MotorAcceleration = 0f;
					this.m_Demo.LookDamping = 3f;
				}, this.m_ActionTimer3);
				vp_Timer.In(5f, delegate
				{
					this.m_LookPoint = 5;
					this.m_RunForward = false;
					this.m_Demo.Controller.MotorAcceleration = 0.18f;
					this.m_Demo.LookDamping = 1f;
				}, this.m_ActionTimer4);
				vp_Timer.In(9f, delegate
				{
					this.m_LookPoint = 8;
				}, this.m_ActionTimer5);
				vp_Timer.In(11f, delegate
				{
				}, this.m_GoAgainTimer);
				break;
			case 3:
				this.m_Demo.Input.MouseCursorForced = false;
				this.m_Demo.WeaponHandler.SetWeapon(2);
				this.m_Demo.Teleport(this.m_ExplorePos, this.m_ExploreAngle);
				this.m_Demo.Input.AllowGameplayInput = true;
				break;
			case 4:
				this.m_Demo.Teleport(this.m_HeadBumpPos, this.m_HeadBumpAngle);
				vp_Timer.In(1f, delegate
				{
					this.m_Jump = true;
				}, this.m_HeadBumpTimer1);
				vp_Timer.In(1.25f, delegate
				{
					this.m_Jump = false;
				}, this.m_HeadBumpTimer2);
				vp_Timer.In(2f, delegate
				{
				}, this.m_GoAgainTimer);
				break;
			case 5:
				this.m_Demo.Teleport(this.m_WallBouncePos, this.m_WallBounceAngle);
				this.m_LookPoint = 6;
				this.m_Demo.LookDamping = 0f;
				vp_Timer.In(1f, delegate
				{
					this.m_LookPoint = 7;
					this.m_Demo.LookDamping = 3f;
					this.m_Demo.Controller.PhysicsWallBounce = 0f;
					Object.Instantiate<GameObject>(this.m_ExplosionFX, this.m_Demo.Controller.transform.position + new Vector3(3f, 0f, 0f), Quaternion.identity);
					this.m_Demo.PlayerEventHandler.CameraBombShake.Send(0.3f);
					this.m_Demo.Controller.AddForce(Vector3.right * 3f);
					if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						this.m_Demo.WeaponHandler.CurrentWeapon.GetComponent<AudioSource>().PlayOneShot(this.m_ExplosionSound);
					}
				}, this.m_ForceTimer);
				vp_Timer.In(5f, delegate
				{
					this.m_Demo.Controller.PhysicsWallBounce = 0f;
					this.m_LookPoint = 6;
					this.m_Demo.LookDamping = 0.5f;
					this.m_Demo.Teleport(this.m_WallBouncePos, this.m_WallBounceAngle);
				}, this.m_GoAgainTimer);
				break;
			case 6:
				vp_Timer.In(5f, delegate
				{
				}, this.m_GoAgainTimer);
				this.m_Demo.Teleport(this.m_FallDeflectPos, this.m_FallDeflectAngle);
				break;
			case 7:
				vp_Timer.In(7f, delegate
				{
				}, this.m_GoAgainTimer);
				vp_Timer.In(1f, delegate
				{
					Object.Instantiate<GameObject>(this.m_ExplosionFX, this.m_Demo.Controller.transform.position + new Vector3(-3f, 0f, 0f), Quaternion.identity);
					this.m_Demo.PlayerEventHandler.CameraBombShake.Send(0.5f);
					this.m_Demo.Controller.AddForce(Vector3.forward * 0.55f);
					if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						this.m_Demo.WeaponHandler.CurrentWeapon.GetComponent<AudioSource>().PlayOneShot(this.m_ExplosionSound);
					}
				}, this.m_ForceTimer);
				this.m_Demo.Teleport(this.m_BlownAwayPos, this.m_BlownAwayAngle);
				break;
			}
			this.m_Demo.LastInputTime = Time.time;
			this.m_ExamplesCurrentSel = this.m_Demo.ButtonSelection;
		}
		if (this.m_Demo.ButtonSelection != 2 && this.m_Demo.ButtonSelection != 3)
		{
			this.m_Demo.LockControls();
			this.m_Demo.Input.AllowGameplayInput = false;
		}
		else if (this.m_Demo.ButtonSelection != 3)
		{
			this.m_Demo.LockControls();
			this.m_Demo.Input.AllowGameplayInput = false;
		}
		if (this.m_Demo.ButtonSelection != 3 && this.m_Demo.WeaponHandler.CurrentWeaponIndex != 1)
		{
			this.m_Demo.WeaponHandler.SetWeapon(1);
		}
		switch (this.m_Demo.ButtonSelection)
		{
		case 0:
			this.m_Demo.Camera.Angle = this.m_SlimeAngle;
			break;
		case 2:
		{
			Vector2 vector = this.m_Demo.PlayerEventHandler.InputMoveVector.Get();
			vector.y = (this.m_RunForward ? 1f : 0f);
			this.m_Demo.PlayerEventHandler.InputMoveVector.Set(vector);
			this.m_Demo.PlayerEventHandler.Jump.Active = this.m_Jump;
			if (this.m_Demo.Controller.StateEnabled("Run") != this.m_RunForward)
			{
				this.m_Demo.Controller.SetState("Run", this.m_RunForward, true, false);
			}
			this.m_Demo.SmoothLookAt(this.m_LookPoints[this.m_LookPoint]);
			break;
		}
		case 3:
			if (this.m_Demo.ShowGUI && vp_Utility.LockCursor)
			{
				GUI.color = new Color(1f, 1f, 1f, this.m_Demo.ClosingDown ? this.m_Demo.GlobalAlpha : 1f);
				GUI.Label(new Rect((float)(Screen.width / 2 - 200), 140f, 400f, 20f), "(Press ENTER to reenable menu)", this.m_Demo.CenterStyle);
				GUI.color = new Color(0f, 0f, 0f, 1f * (1f - (Time.time - this.m_WASDInfoClickTime) * 0.05f));
				GUI.Label(new Rect((float)(Screen.width / 2 - 200), 170f, 400f, 20f), "(Use WASD to move around freely)", this.m_Demo.CenterStyle);
				GUI.color = new Color(1f, 1f, 1f, 1f * this.m_Demo.GlobalAlpha);
			}
			break;
		case 4:
			this.m_Demo.PlayerEventHandler.Jump.Active = this.m_Jump;
			break;
		case 5:
			this.m_Demo.SmoothLookAt(this.m_LookPoints[this.m_LookPoint]);
			break;
		}
		if (this.m_Demo.ShowGUI)
		{
			this.m_ExamplesCurrentSel = this.m_Demo.ButtonSelection;
			string[] array = new string[] { "Mud... or Slime", "Wet Roof", "Action Hero", "Moving Platforms", "Head Bumps", "Wall Deflection", "Fall Deflection", "Blown Away" };
			this.m_Demo.ButtonSelection = this.m_Demo.ToggleColumn(140, 150, this.m_Demo.ButtonSelection, array, false, true, this.ImageRightPointer, this.ImageLeftPointer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoPresets()
	{
		if (this.m_Demo.FirstFrame)
		{
			this.m_GoAgainTimer.Cancel();
			this.m_Demo.FirstFrame = false;
			this.m_Demo.DrawCrosshair = false;
			this.m_Demo.FreezePlayer(this.m_OverViewPos, this.m_OverViewAngle, true);
			this.m_Demo.WeaponHandler.CancelTimers();
			this.m_Demo.WeaponHandler.SetWeapon(0);
			this.m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
			this.m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
			this.m_Demo.Input.MouseCursorForced = true;
		}
		this.m_Demo.DrawBoxes("states & presets", "You may easily design custom movement STATES (like running, crouching or proning).\nWhen happy with your tweaks, save them to PRESET FILES, and the STATE MANAGER\nwill blend smoothly between them at runtime.", this.ImageLeftArrow, this.ImageRightArrow, delegate
		{
			this.m_Demo.LoadLevel(0);
		}, null, true);
		this.m_Demo.DrawImage(this.ImagePresetDialogs);
		this.m_Demo.ForceCameraShake();
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
			this.m_Demo.PlayerEventHandler.Platform.Set(null);
		}
		this.m_FPSCamera.Angle = new Vector2(this.m_OutroAngle.x, this.m_OutroAngle.y + Mathf.Cos((Time.time - this.m_OutroStartTime + 50f) * 0.03f) * 20f);
		this.m_Demo.DrawBoxes("WHAT YOU GET", "• An in-depth 100+ page MANUAL with many tutorials to get you started EASILY.\n• Tons of scripts, art & sound FX. • Full, well commented C# SOURCE CODE.\n• A FANTASTIC starting point (or upgrade) for any FPS project.\nBest part? It can be yours in a minute! GET IT NOW on visionpunk.com ...", this.ImageLeftArrow, this.ImageCheckmark, delegate
		{
			this.m_Demo.LoadLevel(0);
		}, null, true);
		this.m_Demo.DrawImage(this.ImageAllParams);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FallImpact(float f)
	{
		if (f > 0.2f)
		{
			vp_AudioUtility.PlayRandomSound(this.Player.GetComponent<AudioSource>(), this.FallImpactSounds);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		this.m_Demo.OnGUI();
		int currentScreen = this.m_Demo.CurrentScreen;
		if (currentScreen == 1)
		{
			this.DemoPhysics();
			return;
		}
		if (currentScreen != 2)
		{
			return;
		}
		this.DemoOutro();
	}

	public GameObject Player;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPSDemoManager m_Demo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPCamera m_FPSCamera;

	public Texture ImageLeftArrow;

	public Texture ImageRightArrow;

	public Texture ImageRightPointer;

	public Texture ImageLeftPointer;

	public Texture ImageCheckmark;

	public Texture ImagePresetDialogs;

	public Texture ImageShooter;

	public Texture ImageAllParams;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int m_ExamplesCurrentSel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float m_GoAgainButtonAlpha;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float m_WASDInfoClickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_SlimePos = new Vector3(115.3f, 113.3f, -94.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_WetRoofPos = new Vector3(115.3f, 113.3f, -86.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_FallDeflectPos = new Vector3(106.6f, 116.8f, -97.1f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_BlownAwayPos = new Vector3(132f, 122.18f, -100.6f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_ActionPos = new Vector3(127f, 122.18f, -97.6f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_HeadBumpPos = new Vector3(106.4f, 102.4f, -99.89f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_WallBouncePos = new Vector3(114.2f, 104.6f, -91.9f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_ExplorePos = new Vector3(134.0023f, 107.64261f, -109.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_OverViewPos = new Vector3(135f, 105.8f, -70.7f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_OutroPos = new Vector3(135f, 205.8f, -70.7f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_OutroAngle = new Vector2(-19.3f, 241.7f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_SlimeAngle = new Vector2(0f, 180f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_WetRoofAngle = new Vector2(30f, 230f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_FallDeflectAngle = new Vector2(25f, 180f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_BlownAwayAngle = new Vector2(0f, -90f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_ActionAngle = new Vector2(0f, 180f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_HeadBumpAngle = new Vector2(0f, 180f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_WallBounceAngle = new Vector2(0f, 130f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_ExploreAngle = new Vector2(30f, 40f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_OverViewAngle = new Vector2(-16.5f, 215f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_ForceTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_GoAgainTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_HeadBumpTimer1 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_HeadBumpTimer2 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_ActionTimer1 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_ActionTimer2 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_ActionTimer3 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_ActionTimer4 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_ActionTimer5 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float m_OutroStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_RunForward;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_Jump;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int m_LookPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3[] m_LookPoints = new Vector3[9];

	public AudioClip m_ExplosionSound;

	public List<AudioClip> FallImpactSounds;

	public GameObject m_ExplosionFX;
}
