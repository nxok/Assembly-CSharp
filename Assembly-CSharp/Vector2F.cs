using System;

public struct Vector2F : IEquatable<Vector2F>
{
	public Vector2F(double angle)
	{
		this.X = (float)Math.Sin(angle);
		this.Y = (float)Math.Cos(angle);
	}

	public Vector2F(float x, float y)
	{
		this.X = x;
		this.Y = y;
	}

	public double Length
	{
		get
		{
			return Math.Sqrt((double)(this.X * this.X + this.Y * this.Y));
		}
	}

	public static double Lengthsquared(Vector2F a)
	{
		return (double)(a.X * a.X + a.Y * a.Y);
	}

	public static Vector2F operator -(Vector2F a, Vector2F b)
	{
		return new Vector2F(a.X - b.X, a.Y - b.Y);
	}

	public static bool operator ==(Vector2F a, Vector2F b)
	{
		return a.X == b.X && a.Y == b.Y;
	}

	public static bool operator !=(Vector2F a, Vector2F b)
	{
		return a.X != b.X || a.Y != b.Y;
	}

	public override int GetHashCode()
	{
		return this.X.GetHashCode() * this.Y.GetHashCode();
	}

	public double Lengthsquared()
	{
		return (double)(this.X * this.X + this.Y * this.Y);
	}

	public override bool Equals(object obj)
	{
		return this == (Vector2F)obj;
	}

	public bool Equals(Vector2F other)
	{
		return other.X == this.X && other.Y == this.Y;
	}

	public override string ToString()
	{
		return this.X.ToCultureInvariantString() + ", " + this.Y.ToCultureInvariantString();
	}

	public float Y;

	public float X;

	public static readonly Vector2F Zero = new Vector2F(0f, 0f);
}
