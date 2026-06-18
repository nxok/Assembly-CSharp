using System;
using System.Diagnostics;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveIndicator : XUiController
{
	public override void Init()
	{
		base.Init();
		this.m_tailTimer = new Stopwatch();
		this.m_saveDataManager = SaveDataUtils.SaveDataManager;
		this.m_saveDataManager.CommitStarted += this.OnCommitStarted;
		this.m_saveDataManager.CommitFinished += this.OnCommitFinished;
		this.xui.SaveIndicator = this;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (this.m_saveDataManager != null)
		{
			this.m_saveDataManager.CommitStarted -= this.OnCommitStarted;
			this.m_saveDataManager.CommitFinished -= this.OnCommitFinished;
			this.m_saveDataManager = null;
		}
		this.xui.SaveIndicator = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitStarted()
	{
		this.m_commitInProgress = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitFinished()
	{
		this.m_tailTimer.Restart();
		this.m_commitInProgress = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.m_commitInProgress && this.m_tailTimer.IsRunning && this.m_tailTimer.Elapsed >= XUiC_SaveIndicator.TailDuration)
		{
			this.m_tailTimer.Stop();
		}
		this.shouldShow = this.m_commitInProgress || this.m_tailTimer.IsRunning;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "isSaving")
		{
			_value = this.shouldShow.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan TailDuration = TimeSpan.FromSeconds(2.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public ISaveDataManager m_saveDataManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stopwatch m_tailTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldShow;
}
