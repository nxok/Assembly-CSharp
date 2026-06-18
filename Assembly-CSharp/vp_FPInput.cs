using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPInput : vp_Component
{
	public Vector2 MousePos
	{
		get
		{
			return this.m_MousePos;
		}
	}

	public bool AllowGameplayInput
	{
		get
		{
			return this.m_AllowGameplayInput;
		}
		set
		{
			this.m_AllowGameplayInput = value;
		}
	}

	public vp_FPPlayerEventHandler FPPlayer
	{
		get
		{
			if (this.m_FPPlayer == null)
			{
				this.m_FPPlayer = base.transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
			}
			return this.m_FPPlayer;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		if (this.FPPlayer != null)
		{
			this.FPPlayer.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		if (this.FPPlayer != null)
		{
			this.FPPlayer.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		this.UpdateCursorLock();
		this.UpdatePause();
		if (this.FPPlayer.Pause.Get())
		{
			return;
		}
		if (!this.m_AllowGameplayInput)
		{
			return;
		}
		this.InputInteract();
		this.InputMove();
		this.InputRun();
		this.InputJump();
		this.InputCrouch();
		this.InputAttack();
		this.InputReload();
		this.InputSetWeapon();
		this.InputCamera();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputInteract()
	{
		if (vp_Input.GetButtonDown("Interact"))
		{
			this.FPPlayer.Interact.TryStart(true);
			return;
		}
		this.FPPlayer.Interact.TryStop(true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputMove()
	{
		this.FPPlayer.InputMoveVector.Set(new Vector2(vp_Input.GetAxisRaw("Horizontal"), vp_Input.GetAxisRaw("Vertical")));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputRun()
	{
		if (vp_Input.GetButton("Run"))
		{
			this.FPPlayer.Run.TryStart(true);
			return;
		}
		this.FPPlayer.Run.TryStop(true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputJump()
	{
		if (vp_Input.GetButton("Jump"))
		{
			this.FPPlayer.Jump.TryStart(true);
			return;
		}
		this.FPPlayer.Jump.Stop(0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputCrouch()
	{
		if (vp_Input.GetButton("Crouch"))
		{
			this.FPPlayer.Crouch.TryStart(true);
			return;
		}
		this.FPPlayer.Crouch.TryStop(true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputCamera()
	{
		if (vp_Input.GetButton("Zoom"))
		{
			this.FPPlayer.Zoom.TryStart(true);
		}
		else
		{
			this.FPPlayer.Zoom.TryStop(true);
		}
		if (vp_Input.GetButtonDown("Toggle3rdPerson"))
		{
			this.FPPlayer.CameraToggle3rdPerson.Send();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputAttack()
	{
		if (!vp_Utility.LockCursor)
		{
			return;
		}
		if (vp_Input.GetButton("Attack"))
		{
			this.FPPlayer.Attack.TryStart(true);
			return;
		}
		this.FPPlayer.Attack.TryStop(true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputReload()
	{
		if (vp_Input.GetButtonDown("Reload"))
		{
			this.FPPlayer.Reload.TryStart(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputSetWeapon()
	{
		if (vp_Input.GetButtonDown("SetPrevWeapon"))
		{
			this.FPPlayer.SetPrevWeapon.Try();
		}
		if (vp_Input.GetButtonDown("SetNextWeapon"))
		{
			this.FPPlayer.SetNextWeapon.Try();
		}
		if (Input.GetKeyDown(49))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(1);
		}
		if (Input.GetKeyDown(50))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(2);
		}
		if (Input.GetKeyDown(51))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(3);
		}
		if (Input.GetKeyDown(52))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(4);
		}
		if (Input.GetKeyDown(53))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(5);
		}
		if (Input.GetKeyDown(54))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(6);
		}
		if (Input.GetKeyDown(55))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(7);
		}
		if (Input.GetKeyDown(56))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(8);
		}
		if (Input.GetKeyDown(57))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(9);
		}
		if (Input.GetKeyDown(48))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(10);
		}
		if (vp_Input.GetButtonDown("ClearWeapon"))
		{
			this.FPPlayer.SetWeapon.TryStart<int>(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdatePause()
	{
		if (vp_Input.GetButtonDown("Pause"))
		{
			this.FPPlayer.Pause.Set(!this.FPPlayer.Pause.Get());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateCursorLock()
	{
		this.m_MousePos.x = Input.mousePosition.x;
		this.m_MousePos.y = (float)Screen.height - Input.mousePosition.y;
		if (this.MouseCursorForced)
		{
			vp_Utility.LockCursor = false;
			return;
		}
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			if (this.MouseCursorZones.Length != 0)
			{
				foreach (Rect rect in this.MouseCursorZones)
				{
					if (rect.Contains(this.m_MousePos))
					{
						vp_Utility.LockCursor = false;
						goto IL_009B;
					}
				}
			}
			vp_Utility.LockCursor = true;
		}
		IL_009B:
		if (vp_Input.GetButtonUp("Accept1") || vp_Input.GetButtonUp("Accept2") || vp_Input.GetButtonUp("Menu"))
		{
			vp_Utility.LockCursor = !vp_Utility.LockCursor;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector2 GetMouseLook()
	{
		if (this.MouseCursorBlocksMouseLook && !vp_Utility.LockCursor)
		{
			return Vector2.zero;
		}
		if (this.m_LastMouseLookFrame == Time.frameCount)
		{
			return this.m_CurrentMouseLook;
		}
		this.m_LastMouseLookFrame = Time.frameCount;
		this.m_MouseLookSmoothMove.x = vp_Input.GetAxisRaw("Mouse X") * Time.timeScale;
		this.m_MouseLookSmoothMove.y = vp_Input.GetAxisRaw("Mouse Y") * Time.timeScale;
		this.MouseLookSmoothSteps = Mathf.Clamp(this.MouseLookSmoothSteps, 1, 20);
		this.MouseLookSmoothWeight = Mathf.Clamp01(this.MouseLookSmoothWeight);
		while (this.m_MouseLookSmoothBuffer.Count > this.MouseLookSmoothSteps)
		{
			this.m_MouseLookSmoothBuffer.RemoveAt(0);
		}
		this.m_MouseLookSmoothBuffer.Add(this.m_MouseLookSmoothMove);
		float num = 1f;
		Vector2 vector = Vector2.zero;
		float num2 = 0f;
		for (int i = this.m_MouseLookSmoothBuffer.Count - 1; i > 0; i--)
		{
			vector += this.m_MouseLookSmoothBuffer[i] * num;
			num2 += 1f * num;
			num *= this.MouseLookSmoothWeight / base.Delta;
		}
		num2 = Mathf.Max(1f, num2);
		this.m_CurrentMouseLook = vp_MathUtility.NaNSafeVector2(vector / num2, default(Vector2));
		float num3 = 0f;
		float num4 = Mathf.Abs(this.m_CurrentMouseLook.x);
		float num5 = Mathf.Abs(this.m_CurrentMouseLook.y);
		if (this.MouseLookAcceleration)
		{
			num3 = Mathf.Sqrt(num4 * num4 + num5 * num5) / base.Delta;
			num3 = ((num3 <= this.MouseLookAccelerationThreshold) ? 0f : num3);
		}
		this.m_CurrentMouseLook.x = this.m_CurrentMouseLook.x * (this.MouseLookSensitivity.x + num3);
		this.m_CurrentMouseLook.y = this.m_CurrentMouseLook.y * (this.MouseLookSensitivity.y + num3);
		this.m_CurrentMouseLook.y = (this.MouseLookInvert ? this.m_CurrentMouseLook.y : (-this.m_CurrentMouseLook.y));
		return this.m_CurrentMouseLook;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector2 GetMouseLookRaw()
	{
		if (this.MouseCursorBlocksMouseLook && !vp_Utility.LockCursor)
		{
			return Vector2.zero;
		}
		this.m_MouseLookRawMove.x = vp_Input.GetAxisRaw("Mouse X");
		this.m_MouseLookRawMove.y = vp_Input.GetAxisRaw("Mouse Y");
		return this.m_MouseLookRawMove;
	}

	public virtual Vector2 OnValue_InputMoveVector
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_MoveVector;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_MoveVector = ((value != Vector2.zero) ? value.normalized : value);
		}
	}

	public virtual float OnValue_InputClimbVector
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return vp_Input.GetAxisRaw("Vertical");
		}
	}

	public virtual bool OnValue_InputAllowGameplay
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_AllowGameplayInput;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_AllowGameplayInput = value;
		}
	}

	public virtual bool OnValue_Pause
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return vp_TimeUtility.Paused;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			vp_TimeUtility.Paused = !vp_Gameplay.isMultiplayer && value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnMessage_InputGetButton(string button)
	{
		return vp_Input.GetButton(button);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnMessage_InputGetButtonUp(string button)
	{
		return vp_Input.GetButtonUp(button);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnMessage_InputGetButtonDown(string button)
	{
		return vp_Input.GetButtonDown(button);
	}

	public virtual Vector2 OnValue_InputSmoothLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.GetMouseLook();
		}
	}

	public virtual Vector2 OnValue_InputRawLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.GetMouseLookRaw();
		}
	}

	public Vector2 MouseLookSensitivity = new Vector2(5f, 5f);

	public int MouseLookSmoothSteps = 10;

	public float MouseLookSmoothWeight = 0.5f;

	public bool MouseLookAcceleration;

	public float MouseLookAccelerationThreshold = 0.4f;

	public bool MouseLookInvert;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_MouseLookSmoothMove = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_MouseLookRawMove = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Vector2> m_MouseLookSmoothBuffer = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int m_LastMouseLookFrame = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_CurrentMouseLook = Vector2.zero;

	public Rect[] MouseCursorZones;

	public bool MouseCursorForced;

	public bool MouseCursorBlocksMouseLook = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_MousePos = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_MoveVector = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_AllowGameplayInput = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_FPPlayer;
}
