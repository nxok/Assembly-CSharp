using System;
using UnityEngine;

public class TurnTable : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this._pingPong)
		{
			float num = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time * this._rotationSpeed, 1f));
			float num2 = Mathf.Lerp(this._pingPongDegreeOffset - this._pingPongDegreeSpan / 2f, this._pingPongDegreeOffset + this._pingPongDegreeSpan / 2f, num);
			base.transform.localRotation = Quaternion.Euler(0f, num2, 0f);
			return;
		}
		base.transform.localRotation *= Quaternion.Euler(0f, this._rotationSpeed * 180f * Time.deltaTime, 0f);
	}

	public float _rotationSpeed = 1f;

	public bool _pingPong;

	public float _pingPongDegreeSpan = 90f;

	public float _pingPongDegreeOffset;
}
