using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntitySupplyPlane : Entity
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
	}

	public override bool IsDeadIfOutOfWorld()
	{
		return false;
	}

	public void SetDirectionToFly(Vector3 _directionToFly, int _ticksToFly)
	{
		this.ticksToFly = _ticksToFly;
		this.motion = _directionToFly * 6f;
		this.IsMovementReplicated = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MoveBoundsInsideFrustrum(Transform _parentT)
	{
		if (!this.planeMesh)
		{
			return;
		}
		float magnitude = (this.mainCamera.transform.position - _parentT.position).magnitude;
		Vector3 vector = Vector3.one * (magnitude * 1.25f);
		Vector3 zero = Vector3.zero;
		this.planeMesh.bounds = new Bounds(zero, vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFarDraw()
	{
		if (!this.mainCamera)
		{
			this.mainCamera = Camera.main;
			if (!this.mainCamera)
			{
				return;
			}
		}
		if (!this.planeMesh)
		{
			this.planeMF = base.transform.GetComponentInChildren<MeshFilter>();
			if (this.planeMF)
			{
				this.planeMesh = this.planeMF.mesh;
			}
		}
		this.MoveBoundsInsideFrustrum(base.transform);
	}

	public override void OnUpdatePosition(float _partialTicks)
	{
		base.OnUpdatePosition(_partialTicks);
		this.UpdateFarDraw();
		this.interpolateTargetRot = 0;
		this.position += this.motion * _partialTicks;
		if (!this.isEntityRemote)
		{
			int num = this.ticksToFly - 1;
			this.ticksToFly = num;
			if (num <= 0)
			{
				this.MarkToUnload();
			}
		}
		if (!this.isPlayedSound)
		{
			Manager.Play(this, "SupplyDrops/Supply_Crate_Plane_lp", 1f, false);
			this.isPlayedSound = true;
		}
		base.SetAirBorne(true);
	}

	public override bool IsSavedToFile()
	{
		return false;
	}

	public override bool CanCollideWithBlocks()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int ticksToFly;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isPlayedSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshFilter planeMF;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Mesh planeMesh;
}
