using System;
using System.IO;
using UnityEngine;

public class ImposterCanvas
{
	public SignCanvas.CanvasState State
	{
		get
		{
			if (!(this.Canvas != null))
			{
				return this.pendingCanvasState;
			}
			return this.Canvas.State;
		}
		set
		{
			if (this.Canvas != null)
			{
				this.Canvas.State = value;
				return;
			}
			this.pendingCanvasState = value;
		}
	}

	public ImposterCanvas Clone()
	{
		ImposterCanvas imposterCanvas = new ImposterCanvas();
		imposterCanvas.WorldPosition = this.WorldPosition;
		imposterCanvas.WorldRotation = this.WorldRotation;
		imposterCanvas.Size = this.Size;
		imposterCanvas.CanvasAspect = this.CanvasAspect;
		imposterCanvas.IsDecal = this.IsDecal;
		SignCanvas.CanvasState state = this.State;
		imposterCanvas.pendingCanvasState = ((state != null) ? state.Clone() : null);
		return imposterCanvas;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(2);
		_bw.Write(this.WorldPosition.x);
		_bw.Write(this.WorldPosition.y);
		_bw.Write(this.WorldPosition.z);
		_bw.Write(this.WorldRotation.x);
		_bw.Write(this.WorldRotation.y);
		_bw.Write(this.WorldRotation.z);
		_bw.Write(this.WorldRotation.w);
		_bw.Write(this.Size.x);
		_bw.Write(this.Size.y);
		_bw.Write(this.CanvasAspect);
		_bw.Write(this.IsDecal);
		this.State.Write(_bw);
	}

	public static ImposterCanvas Read(BinaryReader _br)
	{
		ImposterCanvas imposterCanvas = new ImposterCanvas();
		ushort num = _br.ReadUInt16();
		imposterCanvas.WorldPosition.x = _br.ReadSingle();
		imposterCanvas.WorldPosition.y = _br.ReadSingle();
		imposterCanvas.WorldPosition.z = _br.ReadSingle();
		imposterCanvas.WorldRotation.x = _br.ReadSingle();
		imposterCanvas.WorldRotation.y = _br.ReadSingle();
		imposterCanvas.WorldRotation.z = _br.ReadSingle();
		imposterCanvas.WorldRotation.w = _br.ReadSingle();
		imposterCanvas.Size.x = _br.ReadSingle();
		imposterCanvas.Size.y = _br.ReadSingle();
		imposterCanvas.CanvasAspect = _br.ReadSingle();
		if (num >= 2)
		{
			imposterCanvas.IsDecal = _br.ReadBoolean();
		}
		imposterCanvas.pendingCanvasState = SignCanvas.CanvasState.Read(_br);
		return imposterCanvas;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort Version = 2;

	public Vector3 WorldPosition;

	public Quaternion WorldRotation;

	public Vector2 Size;

	public float CanvasAspect;

	public bool IsDecal;

	public SignCanvas Canvas;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas.CanvasState pendingCanvasState = new SignCanvas.CanvasState();
}
