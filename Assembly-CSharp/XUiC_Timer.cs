using System;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Timer : XUiController
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.CursorController.SetCursorHidden(true);
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.CancelButton, "igcoCancel", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.updateTimer(0f, 1f);
		this.fullTime = 0f;
		this.xui.playerUI.CursorController.SetCursorHidden(false);
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		if (!this.skipCloseEvent)
		{
			TimerEventData timerEventData = this.eventData;
			if (timerEventData != null)
			{
				timerEventData.HandleClosed();
			}
		}
		this.eventData = null;
		this.xui.playerUI.entityPlayer.SetControllable(true);
		CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Default);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTimer(float _timeLeft, float _fillAmount)
	{
		this.currentTimeLeft = _timeLeft;
		this.currentFillAmount = _fillAmount;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void timeReachedNull()
	{
		this.fullTime = 0f;
		this.timeLeft = 0f;
		this.skipCloseEvent = true;
		TimerEventData timerEventData = this.eventData;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.DragAndDropWindow.InMenu = false;
		if (timerEventData != null)
		{
			timerEventData.HandleFullTimeFinished();
		}
		this.skipCloseEvent = false;
		this.eventData = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void alternateTimeReached()
	{
		this.fullTime = 0f;
		this.timeLeft = 0f;
		this.skipCloseEvent = true;
		TimerEventData timerEventData = this.eventData;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.DragAndDropWindow.InMenu = false;
		if (timerEventData != null)
		{
			timerEventData.HandleAlternateEvent();
		}
		this.skipCloseEvent = false;
		this.eventData = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.fullTime <= 0f)
		{
			return;
		}
		if (this.eventData.CloseOnHit && this.xui.playerUI.entityPlayer != null && this.xui.playerUI.entityPlayer.hasBeenAttackedTime > 0)
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			return;
		}
		this.timeLeft -= _dt;
		if (this.timeLeft < 0f)
		{
			this.timeLeft = 0f;
		}
		float num = this.timeLeft / this.fullTime;
		this.updateTimer(this.timeLeft, num);
		this.eventData.Completion = (this.fullTime - this.timeLeft) / this.fullTime;
		if (this.timeLeft <= 0f)
		{
			this.timeReachedNull();
			return;
		}
		if (this.eventData.AlternateTime >= 0f && this.fullTime - this.timeLeft > this.eventData.AlternateTime)
		{
			this.alternateTimeReached();
			return;
		}
		if (this.eventData.CancelWithActivateButton && this.xui.playerUI.playerInput.PermanentActions.Activate.WasPressed)
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setTimer(float _fullTime, TimerEventData _eventData, float _startTime = -1f, string _labelText = "")
	{
		this.currentOpenEventText = _labelText;
		this.fullTime = _fullTime;
		this.timeLeft = ((_startTime < 0f) ? this.fullTime : _startTime);
		float num = 1f;
		if (this.fullTime > _startTime && this.fullTime > 0f)
		{
			num = this.timeLeft / this.fullTime;
		}
		this.eventData = _eventData;
		this.updateTimer(this.timeLeft, num);
	}

	public static void OpenTimer(XUi _xui, float _fullTime, TimerEventData _eventData, float _startTime = -1f, string _labelText = "", bool _modal = true)
	{
		XUiC_Timer childByType = _xui.GetChildByType<XUiC_Timer>();
		_xui.playerUI.windowManager.Open(childByType.windowGroup, _modal);
		childByType.setTimer(_fullTime, _eventData, _startTime, _labelText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "timeleft")
		{
			_value = this.currentTimeLeft.ToCultureInvariantString("0.0");
			return true;
		}
		if (_bindingName == "percent")
		{
			_value = this.currentFillAmount.ToCultureInvariantString();
			return true;
		}
		if (!(_bindingName == "caption"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = this.currentOpenEventText ?? "";
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float fullTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentOpenEventText;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentFillAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public TimerEventData eventData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skipCloseEvent;
}
