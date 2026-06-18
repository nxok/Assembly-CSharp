using System;

public class GorePrefab : RootTransformRefEntity
{
	public bool restoreState
	{
		set
		{
			this._restoreState = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		if (!this._restoreState && this.RootTransform != null && this.Sound != null && this.Sound != string.Empty)
		{
			this.RootTransform.GetComponent<Entity>().PlayOneShot(this.Sound, false, false, false, null, 1f);
		}
	}

	public string Sound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool _restoreState;
}
