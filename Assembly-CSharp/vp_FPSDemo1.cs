using System;
using UnityEngine;

public class vp_FPSDemo1 : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.m_Demo = new vp_FPSDemoManager(this.Player);
		this.m_Demo.PlayerEventHandler.Register(this);
		this.m_Demo.CurrentFullScreenFadeTime = Time.time;
		this.m_Demo.DrawCrosshair = false;
		this.m_Demo.Input.MouseCursorZones = new Rect[3];
		this.m_Demo.Input.MouseCursorZones[0] = new Rect((float)Screen.width * 0.5f - 370f, 40f, 80f, 80f);
		this.m_Demo.Input.MouseCursorZones[1] = new Rect((float)Screen.width * 0.5f + 290f, 40f, 80f, 80f);
		this.m_Demo.Input.MouseCursorZones[2] = new Rect(0f, 0f, 150f, (float)Screen.height);
		vp_Utility.LockCursor = false;
		this.m_Demo.Camera.RenderingFieldOfView = 20f;
		this.m_Demo.Camera.SnapZoom();
		this.m_Demo.Camera.PositionOffset = new Vector3(0f, 1.75f, 0.1f);
		this.m_AudioSource = this.m_Demo.Camera.gameObject.AddComponent<AudioSource>();
		this.m_Demo.PlayerEventHandler.SetWeapon.Disallow(10000000f);
		this.m_Demo.PlayerEventHandler.SetPrevWeapon.Try = () => false;
		this.m_Demo.PlayerEventHandler.SetNextWeapon.Try = () => false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (this.m_Demo.PlayerEventHandler != null)
		{
			this.m_Demo.PlayerEventHandler.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.m_Demo.Update();
		if (this.m_Demo.CurrentScreen == 1 && this.m_Demo.WeaponHandler.CurrentWeapon != null)
		{
			this.m_Demo.WeaponHandler.SetWeapon(0);
		}
		if (this.m_Demo.CurrentScreen == 2)
		{
			if (Input.GetKeyDown(8))
			{
				this.m_Demo.ButtonSelection = 0;
			}
			if (Input.GetKeyDown(49))
			{
				this.m_Demo.ButtonSelection = 1;
			}
			if (Input.GetKeyDown(50))
			{
				this.m_Demo.ButtonSelection = 2;
			}
			if (Input.GetKeyDown(51))
			{
				this.m_Demo.ButtonSelection = 3;
			}
			if (Input.GetKeyDown(52))
			{
				this.m_Demo.ButtonSelection = 4;
			}
			if (Input.GetKeyDown(53))
			{
				this.m_Demo.ButtonSelection = 5;
			}
			if (Input.GetKeyDown(54))
			{
				this.m_Demo.ButtonSelection = 6;
			}
			if (Input.GetKeyDown(55))
			{
				this.m_Demo.ButtonSelection = 7;
			}
			if (Input.GetKeyDown(56))
			{
				this.m_Demo.ButtonSelection = 8;
			}
			if (Input.GetKeyDown(57))
			{
				this.m_Demo.ButtonSelection = 9;
			}
			if (Input.GetKeyDown(48))
			{
				this.m_Demo.ButtonSelection = 10;
			}
			if (Input.GetKeyDown(113))
			{
				this.m_Demo.ButtonSelection--;
				if (this.m_Demo.ButtonSelection < 1)
				{
					this.m_Demo.ButtonSelection = 10;
				}
			}
			if (Input.GetKeyDown(101))
			{
				this.m_Demo.ButtonSelection++;
				if (this.m_Demo.ButtonSelection > 10)
				{
					this.m_Demo.ButtonSelection = 1;
				}
			}
		}
		this.m_Demo.Input.MouseCursorBlocksMouseLook = false;
		if (this.m_Demo.CurrentScreen != 3 && this.m_ChrashingAirplaneRestoreTimer.Active)
		{
			this.m_ChrashingAirplaneRestoreTimer.Cancel();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoIntro()
	{
		this.m_Demo.DrawBoxes("part ii: under the hood", "Ultimate FPS features a NEXT-GEN first person camera system with ultra smooth PROCEDURAL ANIMATION of player movements. Camera and weapons are manipulated using over 100 parameters, allowing for a vast range of super-lifelike behaviors.", null, this.m_ImageRightArrow, null, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.DrawCrosshair = false;
			this.m_Demo.FirstFrame = false;
			this.m_Demo.Camera.RenderingFieldOfView = 20f;
			this.m_Demo.Camera.SnapZoom();
			this.m_Demo.WeaponHandler.SetWeapon(0);
			this.m_Demo.FreezePlayer(this.m_OverviewPos, this.m_OverviewAngle, true);
			this.m_Demo.LastInputTime -= 20f;
			this.m_Demo.RefreshDefaultState();
			this.m_Demo.Input.MouseCursorForced = true;
		}
		this.m_Demo.Input.MouseCursorForced = true;
		this.m_Demo.ForceCameraShake();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetWeapon(int i, string state = null, bool drawCrosshair = true, bool wieldMotion = true)
	{
		this.m_Demo.DrawCrosshair = drawCrosshair;
		if (this.m_Demo.WeaponHandler.CurrentWeaponIndex != i)
		{
			if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				if (this.m_ExamplesCurrentSel == 0)
				{
					((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapToExit();
				}
				else if (wieldMotion)
				{
					this.m_Demo.WeaponHandler.CurrentWeapon.Wield(false);
				}
			}
			vp_Timer.In(wieldMotion ? 0.2f : 0f, delegate
			{
				this.m_Demo.WeaponHandler.SetWeapon(i);
				if (this.m_Demo.WeaponHandler.CurrentWeapon != null && wieldMotion)
				{
					this.m_Demo.WeaponHandler.CurrentWeapon.Wield(true);
				}
				if (state != null)
				{
					this.m_Demo.PlayerEventHandler.ResetActivityStates();
					this.m_Demo.PlayerEventHandler.SetState(state, true, true, false);
				}
			}, this.m_WeaponSwitchTimer);
			return;
		}
		if (state != null)
		{
			this.m_Demo.PlayerEventHandler.SetState(state, true, true, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoExamples()
	{
		this.m_Demo.DrawBoxes("examples", "Try MOVING, JUMPING and STRAFING with the demo presets on the left.\nNote that NO ANIMATIONS are used in this demo. Instead, the camera and weapons are manipulated using realtime SPRING PHYSICS, SINUS BOB and NOISE SHAKING.\nCombining this with traditional animations (e.g. reload) can be very powerful!", this.m_ImageLeftArrow, this.m_ImageRightArrow, null, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_AudioSource.Stop();
			this.m_Demo.DrawCrosshair = true;
			this.m_Demo.Teleport(this.m_StartPos, this.m_StartAngle);
			this.m_Demo.FirstFrame = false;
			this.m_UnFreezePosition = this.m_Demo.Controller.transform.position;
			this.m_Demo.ButtonSelection = 0;
			this.m_Demo.WeaponHandler.SetWeapon(3);
			this.m_Demo.PlayerEventHandler.SetState("Freeze", false, true, false);
			this.m_Demo.PlayerEventHandler.SetState("SystemOFF", true, true, false);
			if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
			}
			this.m_Demo.Camera.SnapZoom();
			this.m_Demo.Camera.SnapSprings();
			this.m_Demo.Input.MouseCursorForced = true;
		}
		if (this.m_Demo.ButtonSelection != this.m_ExamplesCurrentSel)
		{
			vp_Utility.LockCursor = true;
			this.m_Demo.ResetState();
			this.m_Demo.PlayerEventHandler.Attack.Stop(0.5f);
			this.m_Demo.Camera.BobStepCallback = null;
			this.m_Demo.Camera.SnapSprings();
			if (this.m_ExamplesCurrentSel == 9 && this.m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
				((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapSprings();
				((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapPivot();
			}
			switch (this.m_Demo.ButtonSelection)
			{
			case 0:
				this.m_Demo.PlayerEventHandler.Attack.Stop(10000000f);
				this.m_Demo.DrawCrosshair = true;
				this.m_Demo.Controller.Stop();
				if (this.m_Demo.WeaponHandler.CurrentWeaponIndex == 5)
				{
					this.m_Demo.WeaponHandler.SetWeapon(1);
					this.m_Demo.PlayerEventHandler.SetState("SystemOFF", true, true, false);
				}
				else
				{
					this.m_Demo.Camera.SnapZoom();
					this.m_Demo.PlayerEventHandler.SetState("SystemOFF", true, true, false);
					if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						this.m_Demo.WeaponHandler.CurrentWeapon.SnapSprings();
						((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
					}
				}
				break;
			case 1:
				this.SetWeapon(3, "MafiaBoss", true, true);
				break;
			case 2:
				this.SetWeapon(1, "ModernShooter", true, true);
				break;
			case 3:
				this.SetWeapon(4, "Barbarian", true, true);
				break;
			case 4:
				this.SetWeapon(2, "SniperBreath", true, true);
				this.m_Demo.Controller.Stop();
				this.m_Demo.Teleport(this.m_SniperPos, this.m_SniperAngle);
				break;
			case 5:
				this.SetWeapon(0, "Astronaut", false, true);
				this.m_Demo.Controller.Stop();
				this.m_Demo.Teleport(this.m_AstronautPos, this.m_AstronautAngle);
				break;
			case 6:
				this.SetWeapon(5, "MechOrDino", true, false);
				this.m_UnFreezePosition = this.m_DrunkPos;
				this.m_Demo.Controller.Stop();
				this.m_Demo.Teleport(this.m_MechPos, this.m_MechAngle);
				this.m_Demo.Camera.BobStepCallback = delegate
				{
					this.m_Demo.Camera.AddForce2(new Vector3(0f, -1f, 0f));
					if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).AddForce(new Vector3(0f, 0f, 0f), new Vector3(-0.3f, 0f, 0f));
					}
					this.m_AudioSource.pitch = Time.timeScale;
					this.m_AudioSource.PlayOneShot(this.m_StompSound);
				};
				break;
			case 7:
				this.SetWeapon(3, "TankTurret", true, false);
				this.m_Demo.FreezePlayer(this.m_OverviewPos, this.m_OverviewAngle);
				this.m_Demo.Controller.Stop();
				break;
			case 8:
				this.m_Demo.Controller.Stop();
				this.SetWeapon(0, "DrunkPerson", false, true);
				this.m_Demo.Controller.Stop();
				this.m_Demo.Teleport(this.m_DrunkPos, this.m_DrunkAngle);
				this.m_Demo.Camera.StopSprings();
				this.m_Demo.Camera.Refresh();
				break;
			case 9:
				this.SetWeapon(1, "OldSchool", true, true);
				this.m_Demo.Controller.Stop();
				this.m_Demo.Teleport(this.m_OldSchoolPos, this.m_OldSchoolAngle);
				this.m_Demo.Camera.SnapSprings();
				this.m_Demo.Camera.SnapZoom();
				vp_Timer.In(0.3f, delegate
				{
					if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
					{
						vp_Shooter componentInChildren = this.m_Demo.WeaponHandler.CurrentWeapon.GetComponentInChildren<vp_Shooter>();
						componentInChildren.MuzzleFlashPosition = new Vector3(0.0025736f, -0.0813138f, 1.662671f);
						componentInChildren.Refresh();
					}
				}, null);
				break;
			case 10:
				this.SetWeapon(2, "CrazyCowboy", true, true);
				this.m_Demo.Teleport(this.m_StartPos, this.m_StartAngle);
				this.m_Demo.Controller.Stop();
				break;
			}
			this.m_ExamplesCurrentSel = this.m_Demo.ButtonSelection;
		}
		if (this.m_Demo.ShowGUI)
		{
			this.m_ExamplesCurrentSel = this.m_Demo.ButtonSelection;
			string[] array = new string[]
			{
				"System OFF", "Mafia Boss", "Modern Shooter", "Barbarian", "Sniper Breath", "Astronaut", "Mech... or Dino?", "Tank Turret", "Drunk Person", "Old School",
				"Crazy Cowboy"
			};
			this.m_Demo.ButtonSelection = this.m_Demo.ToggleColumn(140, 150, this.m_Demo.ButtonSelection, array, false, true, this.m_ImageRightPointer, this.m_ImageLeftPointer);
		}
		if (this.m_Demo.ShowGUI && vp_Utility.LockCursor)
		{
			GUI.color = new Color(1f, 1f, 1f, this.m_Demo.ClosingDown ? this.m_Demo.GlobalAlpha : 1f);
			GUI.Label(new Rect((float)(Screen.width / 2 - 200), 140f, 400f, 20f), "(Press ENTER to reenable menu)", this.m_Demo.CenterStyle);
			GUI.color = new Color(1f, 1f, 1f, 1f * this.m_Demo.GlobalAlpha);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoForces()
	{
		this.m_Demo.DrawBoxes("external forces", "The camera and weapon are mounted on 8 positional and angular SPRINGS.\nEXTERNAL FORCES can be applied to these in various ways, creating unique movement patterns every time. This is useful for shockwaves, explosion knockback and earthquakes.", this.m_ImageLeftArrow, this.m_ImageRightArrow, null, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.DrawCrosshair = false;
			this.m_Demo.ResetState();
			this.m_Demo.Camera.Load(this.StompingCamera);
			this.m_Demo.Input.Load(this.StompingInput);
			this.m_Demo.WeaponHandler.SetWeapon(1);
			this.m_Demo.Controller.Load(this.SmackController);
			this.m_Demo.Camera.SnapZoom();
			this.m_Demo.FirstFrame = false;
			this.m_Demo.Teleport(this.m_ForcesPos, this.m_ForcesAngle);
			this.m_Demo.ButtonColumnArrowY = -100f;
			this.m_Demo.Input.MouseCursorForced = true;
		}
		if (this.m_Demo.ShowGUI)
		{
			this.m_Demo.ButtonSelection = -1;
			string[] array = new string[] { "Earthquake", "Boss Stomp", "Incoming Artillery", "Crashing Airplane" };
			this.m_Demo.ButtonSelection = this.m_Demo.ButtonColumn(150, this.m_Demo.ButtonSelection, array, this.m_ImageRightPointer);
			if (this.m_Demo.ButtonSelection != -1)
			{
				switch (this.m_Demo.ButtonSelection)
				{
				case 0:
					this.m_Demo.Camera.Load(this.StompingCamera);
					this.m_Demo.Input.Load(this.StompingInput);
					this.m_Demo.Controller.Load(this.SmackController);
					this.m_Demo.PlayerEventHandler.CameraEarthQuake.TryStart<Vector3>(new Vector3(0.2f, 0.2f, 10f));
					this.m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 9f;
					this.m_AudioSource.Stop();
					this.m_AudioSource.pitch = Time.timeScale;
					this.m_AudioSource.PlayOneShot(this.m_EarthquakeSound);
					break;
				case 1:
					this.m_Demo.PlayerEventHandler.CameraEarthQuake.Stop(0f);
					this.m_Demo.Camera.Load(this.ArtilleryCamera);
					this.m_Demo.Input.Load(this.ArtilleryInput);
					this.m_Demo.Controller.Load(this.SmackController);
					this.m_Demo.PlayerEventHandler.CameraGroundStomp.Send(1f);
					this.m_Demo.ButtonColumnArrowFadeoutTime = Time.time;
					this.m_AudioSource.Stop();
					this.m_AudioSource.pitch = Time.timeScale;
					this.m_AudioSource.PlayOneShot(this.m_StompSound);
					break;
				case 2:
				{
					this.m_Demo.PlayerEventHandler.CameraEarthQuake.Stop(0f);
					this.m_Demo.Camera.Load(this.ArtilleryCamera);
					this.m_Demo.Input.Load(this.ArtilleryInput);
					this.m_Demo.Controller.Load(this.ArtilleryController);
					this.m_Demo.PlayerEventHandler.CameraBombShake.Send(1f);
					this.m_Demo.Controller.AddForce(Random.Range(-1.5f, 1.5f), 0.5f, Random.Range(-1.5f, -0.5f));
					this.m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 1f;
					this.m_AudioSource.Stop();
					this.m_AudioSource.pitch = Time.timeScale;
					this.m_AudioSource.PlayOneShot(this.m_ExplosionSound);
					Vector3 vector = this.m_Demo.Controller.transform.TransformPoint(Vector3.forward * (float)Random.Range(1, 2));
					vector.y = this.m_Demo.Controller.transform.position.y + 1f;
					Object.Instantiate<GameObject>(this.m_ArtilleryFX, vector, Quaternion.identity);
					break;
				}
				case 3:
					this.m_Demo.Camera.Load(this.StompingCamera);
					this.m_Demo.Input.Load(this.StompingInput);
					this.m_Demo.Controller.Load(this.SmackController);
					this.m_Demo.PlayerEventHandler.CameraEarthQuake.TryStart<Vector3>(new Vector3(0.25f, 0.2f, 10f));
					this.m_Demo.ButtonColumnArrowFadeoutTime = Time.time + 9f;
					this.m_AudioSource.Stop();
					this.m_AudioSource.pitch = Time.timeScale;
					this.m_AudioSource.PlayOneShot(this.m_EarthquakeSound);
					this.m_Demo.Camera.RenderingFieldOfView = 80f;
					this.m_Demo.Camera.RotationEarthQuakeFactor = 6.5f;
					this.m_Demo.Camera.Zoom();
					vp_Timer.In(9f, delegate
					{
						this.m_Demo.Camera.RenderingFieldOfView = 60f;
						this.m_Demo.Camera.RotationEarthQuakeFactor = 0f;
						this.m_Demo.Camera.Zoom();
					}, this.m_ChrashingAirplaneRestoreTimer);
					break;
				}
				this.m_Demo.LastInputTime = Time.time;
			}
			this.m_Demo.DrawEditorPreview(this.m_ImageWeaponPosition, this.m_ImageEditorPreview, this.m_ImageEditorScreenshot);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoMouseInput()
	{
		this.m_Demo.DrawBoxes("mouse input", "Any good FPS should offer configurable MOUSE SMOOTHING and ACCELERATION.\n• Smoothing interpolates mouse input over several frames to reduce jittering.\n • Acceleration + low mouse sensitivity allows high precision without loss of turn speed.\n• Click the below buttons to compare some example setups.", this.m_ImageLeftArrow, this.m_ImageRightArrow, null, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.ResetState();
			this.m_AudioSource.Stop();
			this.m_Demo.DrawCrosshair = true;
			this.m_Demo.FreezePlayer(this.m_MouseLookPos, this.m_MouseLookAngle);
			this.m_Demo.FirstFrame = false;
			this.m_Demo.WeaponHandler.SetWeapon(0);
			this.m_Demo.Input.MouseCursorForced = true;
			this.m_Demo.Camera.Load(this.MouseRawUnityCamera);
			this.m_Demo.Input.Load(this.MouseRawUnityInput);
		}
		if (this.m_Demo.ShowGUI)
		{
			int buttonSelection = this.m_Demo.ButtonSelection;
			bool flag = this.m_Demo.ButtonSelection != 2;
			string[] array = new string[] { "Raw Mouse Input", "Mouse Smoothing", "Low Sens. + Acceleration" };
			this.m_Demo.ButtonSelection = this.m_Demo.ToggleColumn(200, 150, this.m_Demo.ButtonSelection, array, true, flag, this.m_ImageRightPointer, this.m_ImageLeftPointer);
			if (this.m_Demo.ButtonSelection != buttonSelection)
			{
				switch (this.m_Demo.ButtonSelection)
				{
				case 0:
					this.m_Demo.PlayerEventHandler.ResetActivityStates();
					this.m_Demo.Camera.Load(this.MouseRawUnityCamera);
					this.m_Demo.Input.Load(this.MouseRawUnityInput);
					break;
				case 1:
					this.m_Demo.PlayerEventHandler.ResetActivityStates();
					this.m_Demo.Camera.Load(this.MouseSmoothingCamera);
					this.m_Demo.Input.Load(this.MouseSmoothingInput);
					break;
				case 2:
					this.m_Demo.PlayerEventHandler.ResetActivityStates();
					this.m_Demo.Camera.Load(this.MouseLowSensCamera);
					this.m_Demo.Input.Load(this.MouseLowSensInput);
					break;
				}
				this.m_Demo.LastInputTime = Time.time;
			}
			flag = true;
			if (this.m_Demo.ButtonSelection != 2)
			{
				GUI.enabled = false;
				flag = false;
			}
			this.m_Demo.Input.MouseLookAcceleration = this.m_Demo.ButtonToggle(new Rect((float)(Screen.width / 2 + 110), 215f, 90f, 40f), "Acceleration", this.m_Demo.Input.MouseLookAcceleration, flag, this.m_ImageUpPointer);
			GUI.color = new Color(1f, 1f, 1f, 1f * this.m_Demo.GlobalAlpha);
			GUI.enabled = true;
			this.m_Demo.DrawEditorPreview(this.m_ImageCameraMouse, this.m_ImageEditorPreview, this.m_ImageEditorScreenshot);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoWeaponPerspective()
	{
		this.m_Demo.DrawBoxes("weapon perspective", "Proper WEAPON PERSPECTIVE is crucial to the final impression of your game!\nThe weapon has its own separate Field of View for full perspective control,\nalong with dynamic position and rotation offset.", this.m_ImageLeftArrow, this.m_ImageRightArrow, null, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.ResetState();
			this.m_Demo.Camera.Load(this.PerspOldCamera);
			this.m_Demo.Input.Load(this.PerspOldInput);
			this.m_Demo.Camera.SnapZoom();
			this.m_Demo.FirstFrame = false;
			this.m_Demo.FreezePlayer(this.m_OverviewPos, this.m_PerspectiveAngle, true);
			this.m_Demo.Input.MouseCursorForced = true;
			this.m_Demo.WeaponHandler.SetWeapon(3);
			this.m_Demo.SetWeaponPreset(this.PerspOldWeapon, null, true);
			if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				this.m_Demo.WeaponHandler.CurrentWeapon.SetState("WeaponPersp", true, false, false);
			}
			this.m_Demo.WeaponHandler.SetWeaponLayer(10);
			if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapZoom();
				this.m_Demo.WeaponHandler.CurrentWeapon.SnapSprings();
				((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SnapPivot();
			}
		}
		if (this.m_Demo.ShowGUI)
		{
			int buttonSelection = this.m_Demo.ButtonSelection;
			string[] array = new string[] { "Old School", "1999 Internet Café", "Modern Shooter" };
			this.m_Demo.ButtonSelection = this.m_Demo.ToggleColumn(200, 150, this.m_Demo.ButtonSelection, array, true, true, this.m_ImageRightPointer, this.m_ImageLeftPointer);
			if (this.m_Demo.ButtonSelection != buttonSelection)
			{
				switch (this.m_Demo.ButtonSelection)
				{
				case 0:
					this.m_Demo.SetWeaponPreset(this.PerspOldWeapon, null, true);
					break;
				case 1:
					this.m_Demo.SetWeaponPreset(this.Persp1999Weapon, null, true);
					break;
				case 2:
					this.m_Demo.SetWeaponPreset(this.PerspModernWeapon, null, true);
					break;
				}
				this.m_Demo.LastInputTime = Time.time;
			}
			this.m_Demo.DrawEditorPreview(this.m_ImageWeaponPerspective, this.m_ImageEditorPreview, this.m_ImageEditorScreenshot);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoWeaponLayer()
	{
		this.m_Demo.DrawBoxes("weapon camera", "\nThe weapon can be rendered by a SEPARATE CAMERA so that it never sticks through walls or other geometry. Try toggling the weapon camera ON and OFF below.", this.m_ImageLeftArrow, this.m_ImageRightArrow, null, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.ResetState();
			this.m_Demo.DrawCrosshair = true;
			this.m_Demo.Camera.Load(this.WallFacingCamera);
			this.m_Demo.Input.Load(this.WallFacingInput);
			this.m_Demo.WeaponHandler.SetWeapon(3);
			this.m_Demo.SetWeaponPreset(this.WallFacingWeapon, null, true);
			this.m_Demo.Camera.SnapZoom();
			this.m_WeaponLayerToggle = false;
			this.m_Demo.FirstFrame = false;
			this.m_Demo.FreezePlayer(this.m_WeaponLayerPos, this.m_WeaponLayerAngle);
			int num = (this.m_WeaponLayerToggle ? 10 : 0);
			this.m_Demo.WeaponHandler.SetWeaponLayer(num);
			this.m_Demo.Input.MouseCursorForced = true;
		}
		if (this.m_Demo.ShowGUI)
		{
			bool weaponLayerToggle = this.m_WeaponLayerToggle;
			this.m_WeaponLayerToggle = this.m_Demo.ButtonToggle(new Rect((float)(Screen.width / 2 - 45), 180f, 100f, 40f), "Weapon Camera", this.m_WeaponLayerToggle, true, this.m_ImageUpPointer);
			if (weaponLayerToggle != this.m_WeaponLayerToggle)
			{
				this.m_Demo.FreezePlayer(this.m_WeaponLayerPos, this.m_WeaponLayerAngle);
				int num2 = (this.m_WeaponLayerToggle ? 10 : 0);
				this.m_Demo.WeaponHandler.SetWeaponLayer(num2);
				this.m_Demo.LastInputTime = Time.time;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DemoPivot()
	{
		this.m_Demo.DrawBoxes("weapon pivot", "The PIVOT POINT of the weapon model greatly affects movement pattern.\nManipulating it at runtime can be quite useful, and easy with Ultimate FPS!\nClick the examples below and move the camera around.", this.m_ImageLeftArrow, this.m_ImageRightArrow, delegate
		{
			this.m_Demo.LoadLevel(2);
		}, null, true);
		if (this.m_Demo.FirstFrame)
		{
			this.m_Demo.ResetState();
			this.m_Demo.DrawCrosshair = false;
			this.m_Demo.Camera.Load(this.DefaultCamera);
			this.m_Demo.Input.Load(this.DefaultInput);
			this.m_Demo.Controller.Load(this.ImmobileController);
			this.m_Demo.FirstFrame = false;
			this.m_Demo.FreezePlayer(this.m_OverviewPos, this.m_OverviewAngle);
			this.m_Demo.WeaponHandler.SetWeapon(1);
			this.m_Demo.SetWeaponPreset(this.DefaultWeapon, null, true);
			this.m_Demo.SetWeaponPreset(this.PivotMuzzleWeapon, null, true);
			if (this.m_Demo.WeaponHandler.CurrentWeapon != null)
			{
				((vp_FPWeapon)this.m_Demo.WeaponHandler.CurrentWeapon).SetPivotVisible(true);
			}
			this.m_Demo.Input.MouseCursorForced = true;
			this.m_Demo.WeaponHandler.SetWeaponLayer(10);
		}
		if (this.m_Demo.ShowGUI)
		{
			int buttonSelection = this.m_Demo.ButtonSelection;
			string[] array = new string[] { "Muzzle", "Grip", "Chest", "Elbow (Uzi Style)" };
			this.m_Demo.ButtonSelection = this.m_Demo.ToggleColumn(200, 150, this.m_Demo.ButtonSelection, array, true, true, this.m_ImageRightPointer, this.m_ImageLeftPointer);
			if (this.m_Demo.ButtonSelection != buttonSelection)
			{
				switch (this.m_Demo.ButtonSelection)
				{
				case 0:
					this.m_Demo.SetWeaponPreset(this.PivotMuzzleWeapon, null, true);
					break;
				case 1:
					this.m_Demo.SetWeaponPreset(this.PivotWristWeapon, null, true);
					break;
				case 2:
					this.m_Demo.SetWeaponPreset(this.PivotChestWeapon, null, true);
					break;
				case 3:
					this.m_Demo.SetWeaponPreset(this.PivotElbowWeapon, null, true);
					break;
				}
				this.m_Demo.LastInputTime = Time.time;
			}
			this.m_Demo.DrawEditorPreview(this.m_ImageWeaponPivot, this.m_ImageEditorPreview, this.m_ImageEditorScreenshot);
		}
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
			this.DemoExamples();
			return;
		case 3:
			this.DemoForces();
			return;
		case 4:
			this.DemoMouseInput();
			return;
		case 5:
			this.DemoWeaponPerspective();
			return;
		case 6:
			this.DemoWeaponLayer();
			return;
		case 7:
			this.DemoPivot();
			return;
		default:
			return;
		}
	}

	public GameObject Player;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPSDemoManager m_Demo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int m_ExamplesCurrentSel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_ChrashingAirplaneRestoreTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle m_WeaponSwitchTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_WeaponLayerToggle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_MouseLookPos = new Vector3(-8.093015f, 20.08f, 3.416737f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_OverviewPos = new Vector3(1.246535f, 32.08f, 21.43753f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_StartPos = new Vector3(-18.14881f, 20.08f, -24.16859f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_WeaponLayerPos = new Vector3(-19.43989f, 16.08f, 2.10474f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_ForcesPos = new Vector3(-8.093015f, 20.08f, 3.416737f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_MechPos = new Vector3(0.02941191f, 1.08f, -93.50691f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_DrunkPos = new Vector3(18.48685f, 21.08f, 24.05441f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_SniperPos = new Vector3(0.8841875f, 33.08f, 21.3446f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_OldSchoolPos = new Vector3(25.88745f, 0.08f, 23.08822f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 m_AstronautPos = new Vector3(20f, 20f, 16f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_UnFreezePosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_MouseLookAngle = new Vector2(0f, 33.10683f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_OverviewAngle = new Vector2(28.89369f, 224f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_PerspectiveAngle = new Vector2(27f, 223f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_StartAngle = new Vector2(0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_WeaponLayerAngle = new Vector2(0f, -90f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_ForcesAngle = new Vector2(0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_MechAngle = new Vector3(0f, 180f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_DrunkAngle = new Vector3(0f, -90f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_SniperAngle = new Vector2(20f, 180f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_OldSchoolAngle = new Vector2(0f, 180f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_AstronautAngle = new Vector2(0f, 269.5f);

	public Texture m_ImageEditorPreview;

	public Texture m_ImageEditorPreviewShow;

	public Texture m_ImageCameraMouse;

	public Texture m_ImageWeaponPosition;

	public Texture m_ImageWeaponPerspective;

	public Texture m_ImageWeaponPivot;

	public Texture m_ImageEditorScreenshot;

	public Texture m_ImageLeftArrow;

	public Texture m_ImageRightArrow;

	public Texture m_ImageCheckmark;

	public Texture m_ImageLeftPointer;

	public Texture m_ImageRightPointer;

	public Texture m_ImageUpPointer;

	public Texture m_ImageCrosshair;

	public Texture m_ImageFullScreen;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource m_AudioSource;

	public AudioClip m_StompSound;

	public AudioClip m_EarthquakeSound;

	public AudioClip m_ExplosionSound;

	public GameObject m_ArtilleryFX;

	public TextAsset ArtilleryCamera;

	public TextAsset ArtilleryController;

	public TextAsset ArtilleryInput;

	public TextAsset AstronautCamera;

	public TextAsset AstronautController;

	public TextAsset AstronautInput;

	public TextAsset CowboyCamera;

	public TextAsset CowboyController;

	public TextAsset CowboyWeapon;

	public TextAsset CowboyShooter;

	public TextAsset CowboyInput;

	public TextAsset CrouchController;

	public TextAsset CrouchInput;

	public TextAsset DefaultCamera;

	public TextAsset DefaultWeapon;

	public TextAsset DefaultInput;

	public TextAsset DrunkCamera;

	public TextAsset DrunkController;

	public TextAsset DrunkInput;

	public TextAsset ImmobileCamera;

	public TextAsset ImmobileController;

	public TextAsset ImmobileInput;

	public TextAsset MaceCamera;

	public TextAsset MaceWeapon;

	public TextAsset MaceInput;

	public TextAsset MafiaCamera;

	public TextAsset MafiaWeapon;

	public TextAsset MafiaShooter;

	public TextAsset MafiaInput;

	public TextAsset MechCamera;

	public TextAsset MechController;

	public TextAsset MechWeapon;

	public TextAsset MechShooter;

	public TextAsset MechInput;

	public TextAsset ModernCamera;

	public TextAsset ModernController;

	public TextAsset ModernWeapon;

	public TextAsset ModernShooter;

	public TextAsset ModernInput;

	public TextAsset MouseLowSensCamera;

	public TextAsset MouseLowSensInput;

	public TextAsset MouseRawUnityCamera;

	public TextAsset MouseRawUnityInput;

	public TextAsset MouseSmoothingCamera;

	public TextAsset MouseSmoothingInput;

	public TextAsset OldSchoolCamera;

	public TextAsset OldSchoolController;

	public TextAsset OldSchoolWeapon;

	public TextAsset OldSchoolShooter;

	public TextAsset OldSchoolInput;

	public TextAsset Persp1999Camera;

	public TextAsset Persp1999Weapon;

	public TextAsset Persp1999Input;

	public TextAsset PerspModernCamera;

	public TextAsset PerspModernWeapon;

	public TextAsset PerspModernInput;

	public TextAsset PerspOldCamera;

	public TextAsset PerspOldWeapon;

	public TextAsset PerspOldInput;

	public TextAsset PivotChestWeapon;

	public TextAsset PivotElbowWeapon;

	public TextAsset PivotMuzzleWeapon;

	public TextAsset PivotWristWeapon;

	public TextAsset SmackController;

	public TextAsset SniperCamera;

	public TextAsset SniperWeapon;

	public TextAsset SniperShooter;

	public TextAsset SniperInput;

	public TextAsset StompingCamera;

	public TextAsset StompingInput;

	public TextAsset SystemOFFCamera;

	public TextAsset SystemOFFController;

	public TextAsset SystemOFFShooter;

	public TextAsset SystemOFFWeapon;

	public TextAsset SystemOFFWeaponGlideIn;

	public TextAsset SystemOFFInput;

	public TextAsset TurretCamera;

	public TextAsset TurretWeapon;

	public TextAsset TurretShooter;

	public TextAsset TurretInput;

	public TextAsset WallFacingCamera;

	public TextAsset WallFacingWeapon;

	public TextAsset WallFacingInput;
}
