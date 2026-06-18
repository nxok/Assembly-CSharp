using System;
using System.IO;

public abstract class AIDirectorComponent
{
	public virtual void Connect()
	{
	}

	public virtual void InitNewGame()
	{
	}

	public virtual void Tick(double _dt)
	{
	}

	public virtual void Read(BinaryReader _stream, int _version)
	{
	}

	public virtual void Write(BinaryWriter _stream)
	{
	}

	public GameRandom Random
	{
		get
		{
			return this.Director.random;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorComponent()
	{
	}

	public AIDirector Director;
}
