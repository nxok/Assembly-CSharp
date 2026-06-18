using System;
using UnityEngine;

namespace Assets.DuckType.Jiggle
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class ProceduralAnimation : MonoBehaviour
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			this.m_RestPos = base.transform.position;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			float num = (this.MoveAlongX ? (Time.time * this.TranslationMultiplier) : 0f);
			if (this.ForwardAndBackward)
			{
				num += this.GetSineValue(this.Bounce, this.TranslationMultiplier);
			}
			base.transform.position = this.m_RestPos + new Vector3(num, this.UpAndDown ? this.GetSineValue(this.Bounce, this.TranslationMultiplier) : 0f, this.SideToSide ? this.GetSineValue(this.Bounce, this.TranslationMultiplier) : 0f);
			base.transform.rotation = Quaternion.Euler(this.RotateX ? (Mathf.Sin(Time.time * 6f) * 30f * this.RotationMultiplier) : base.transform.eulerAngles.x, this.RotateY ? (Mathf.Sin(Time.time * 6f) * 30f * this.RotationMultiplier) : base.transform.eulerAngles.y, base.transform.eulerAngles.z);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetSineValue(bool bounce, float mult)
		{
			float num = Mathf.Sin(Time.time * 6f) * 3f * mult;
			if (!bounce)
			{
				return num;
			}
			return Mathf.Abs(num);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Vector3 m_RestPos;

		public bool MoveAlongX;

		public bool ForwardAndBackward;

		public bool UpAndDown;

		public bool SideToSide;

		public bool Bounce;

		public float TranslationMultiplier = 1f;

		public bool RotateX;

		public bool RotateY;

		public float RotationMultiplier = 1f;
	}
}
