using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Counter : XUiController
{
	public event XUiEvent_OnCountChanged OnCountChanged;

	public int Count
	{
		get
		{
			return this.count;
		}
		set
		{
			this.count = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.countUp = base.GetChildById("countUp");
		this.countDown = base.GetChildById("countDown");
		this.countMax = base.GetChildById("countMax");
		this.counter = base.GetChildById("text");
		this.countUp.OnPress += this.HandleCountUpOnPress;
		this.countDown.OnPress += this.HandleCountDownOnPress;
		this.countMax.OnPress += this.HandleMaxCountOnPress;
		this.countUp.OnHold += delegate(XUiController _sender, EHoldType _event, float _duration, float _timedEvent)
		{
			if (_event == EHoldType.HoldTimed)
			{
				this.HandleCountUpOnPress(_sender, -1);
			}
		};
		this.countDown.OnHold += delegate(XUiController _sender, EHoldType _event, float _duration, float _timedEvent)
		{
			if (_event == EHoldType.HoldTimed)
			{
				this.HandleCountDownOnPress(_sender, -1);
			}
		};
		this.textInputChangedDelegate = new XUiEvent_InputOnChangedEventHandler(this.TextInput_OnChangeHandler);
		this.textInput = base.GetChildByType<XUiC_TextInput>();
		this.textInput.OnChangeHandler += this.textInputChangedDelegate;
		this.textInput.OnInputSelectedHandler += this.TextInput_HandleInputDeselected;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleCountChangedEvent()
	{
		if (this.OnCountChanged != null)
		{
			OnCountChangedEventArgs onCountChangedEventArgs = new OnCountChangedEventArgs();
			onCountChangedEventArgs.Count = this.Count;
			this.OnCountChanged(this, onCountChangedEventArgs);
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TextInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		int num = 0;
		if (int.TryParse(_text, out num))
		{
			this.Count = num;
			if (this.Count > this.MaxCount)
			{
				this.Count = this.MaxCount;
				((XUiC_TextInput)_sender).Text = this.Count.ToString();
			}
			else if (this.Count <= 0)
			{
				this.Count = this.Step;
				((XUiC_TextInput)_sender).Text = this.Count.ToString();
			}
			this.HandleCountChangedEvent();
			return;
		}
		this.Count = this.Step;
		this.HandleCountChangedEvent();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleMaxCountOnPress(XUiController _sender, int _mouseButton)
	{
		this.Count = this.MaxCount;
		this.HandleCountChangedEvent();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.textInput.OnChangeHandler -= this.textInputChangedDelegate;
			((XUiV_Label)this.counter.ViewComponent).Text = (this.textInput.Text = ((this.textInput.Text == "") ? "" : ((this.Count > 0) ? this.Count.ToString() : "-")));
			this.textInput.OnChangeHandler += this.textInputChangedDelegate;
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleCountDownOnPress(XUiController _sender, int _mouseButton)
	{
		if (this.Count > 1)
		{
			this.Count -= this.Step;
			this.HandleStepClamping();
		}
		this.HandleCountChangedEvent();
		this.ForceTextRefresh();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleCountUpOnPress(XUiController _sender, int _mouseButton)
	{
		if (this.Count < this.MaxCount)
		{
			this.Count += this.Step;
			this.HandleStepClamping();
			this.HandleCountChangedEvent();
			this.ForceTextRefresh();
		}
	}

	public void SetToMaxCount()
	{
		this.Count = this.MaxCount;
		this.HandleStepClamping();
		this.HandleCountChangedEvent();
		this.ForceTextRefresh();
	}

	public void SetCount(int count)
	{
		if (this.Count != count)
		{
			this.Count = count;
			this.HandleCountChangedEvent();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleStepClamping()
	{
		if (this.Step > 1)
		{
			int num = this.Count % this.Step;
			if (num != 0)
			{
				this.Count -= num;
			}
		}
	}

	public void ForceTextRefresh()
	{
		((XUiV_Label)this.counter.ViewComponent).Text = (this.textInput.Text = this.count.ToString());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TextInput_HandleInputDeselected(XUiController _sender, bool _selected)
	{
		if (!_selected)
		{
			this.ForceTextRefresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController countUp;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController countDown;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController countMax;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController counter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_TextInput textInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;

	public int MaxCount = 1;

	public int Step = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiEvent_InputOnChangedEventHandler textInputChangedDelegate;
}
