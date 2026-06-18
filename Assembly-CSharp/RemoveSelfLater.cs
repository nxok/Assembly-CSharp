using System;
using System.Collections;
using UnityEngine;

public class RemoveSelfLater : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		base.StartCoroutine(this.remove());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator remove()
	{
		yield return new WaitForSeconds(this.WaitSeconds);
		Object.Destroy(base.gameObject);
		yield break;
	}

	public float WaitSeconds = 1f;
}
