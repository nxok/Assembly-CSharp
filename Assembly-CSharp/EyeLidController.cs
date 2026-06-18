using System;
using UnityEngine;

public class EyeLidController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.entityAlive = base.GetComponentInParent<EntityAlive>();
		this.random = GameRandomManager.Instance.CreateGameRandom();
		this.nextBlinkTime = Time.time + this.random.RandomRange(1f, 5f);
		if (this.autoRecordEyeBoneTransforms)
		{
			this.leftTopLocalPosition = this.leftTopTransform.localPosition;
			this.leftBottomLocalPosition = this.leftBottomTransform.localPosition;
			this.leftTopRotation = this.leftTopTransform.localRotation;
			this.leftBottomRotation = this.leftBottomTransform.localRotation;
			this.rightTopLocalPosition = this.rightTopTransform.localPosition;
			this.rightBottomLocalPosition = this.rightBottomTransform.localPosition;
			this.rightTopRotation = this.rightTopTransform.localRotation;
			this.rightBottomRotation = this.rightBottomTransform.localRotation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.debug || (this.entityAlive != null && this.entityAlive.IsDead()))
		{
			this.blinkProgress = 1f;
		}
		else
		{
			if (Time.time > this.nextBlinkTime)
			{
				this.nextBlinkTime = Time.time + this.random.RandomRange(1f, 5f);
				this.blinkState = EyeLidController.BlinkState.Closing;
			}
			switch (this.blinkState)
			{
			case EyeLidController.BlinkState.Closing:
				this.blinkProgress += 20f * Time.deltaTime;
				if (this.blinkProgress >= 1f)
				{
					this.blinkProgress = 1f;
					this.blinkState = EyeLidController.BlinkState.Opening;
				}
				break;
			case EyeLidController.BlinkState.Opening:
				this.blinkProgress -= 10f * Time.deltaTime;
				if (this.blinkProgress <= 0f)
				{
					this.blinkProgress = 0f;
					this.blinkState = EyeLidController.BlinkState.Open;
				}
				break;
			}
		}
		this.leftTopTransform.localPosition = this.leftTopLocalPosition + this.topOffset * this.blinkProgress;
		this.leftTopTransform.localRotation = Quaternion.Euler(this.topRotation * this.blinkProgress) * this.leftTopRotation;
		this.leftBottomTransform.localPosition = this.leftBottomLocalPosition;
		this.leftBottomTransform.localRotation = Quaternion.Euler(this.bottomRotation * this.blinkProgress) * this.leftBottomRotation;
		this.rightTopTransform.localPosition = this.rightTopLocalPosition + this.topOffset * this.blinkProgress;
		this.rightTopTransform.localRotation = Quaternion.Euler(this.topRotation * this.blinkProgress) * this.rightTopRotation;
		this.rightBottomTransform.localPosition = this.rightBottomLocalPosition;
		this.rightBottomTransform.localRotation = Quaternion.Euler(this.bottomRotation * this.blinkProgress) * this.rightBottomRotation;
	}

	public Transform leftTopTransform;

	public Transform leftBottomTransform;

	public Transform rightTopTransform;

	public Transform rightBottomTransform;

	public Vector3 leftTopLocalPosition;

	public Vector3 leftBottomLocalPosition;

	public Quaternion leftTopRotation;

	public Quaternion leftBottomRotation;

	public Vector3 rightTopLocalPosition;

	public Vector3 rightBottomLocalPosition;

	public Quaternion rightTopRotation;

	public Quaternion rightBottomRotation;

	public bool autoRecordEyeBoneTransforms;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float nextBlinkTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float blinkProgress;

	public bool debug;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive entityAlive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 topOffset = new Vector3(0f, 0f, 0.007f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 topRotation = new Vector3(40f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 bottomRotation = new Vector3(-10f, 0f, -10f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EyeLidController.BlinkState blinkState;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum BlinkState
	{
		Open,
		Closing,
		Opening
	}
}
