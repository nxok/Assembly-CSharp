using System;
using UnityEngine;

namespace ShinyScreenSpaceRaytracedReflections
{
	public class Rotate : MonoBehaviour
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			base.transform.Rotate(this.axis * (Time.deltaTime * this.speed));
		}

		public Vector3 axis = Vector3.up;

		public float speed = 60f;
	}
}
