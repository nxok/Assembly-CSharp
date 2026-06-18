using System;
using UnityEngine;

public class vp_Remover : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		vp_Timer.In(Mathf.Max(this.LifeTime, 0.1f), delegate
		{
			vp_Utility.Destroy(base.gameObject);
		}, this.m_DestroyTimer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.m_DestroyTimer.Cancel();
	}

	public float LifeTime = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_DestroyTimer = new vp_Timer.Handle();
}
