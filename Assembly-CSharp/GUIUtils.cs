using System;
using UnityEngine;

public class GUIUtils
{
	public static void DrawFilledRect(Rect _rect, Color _colFill, bool _bDrawBorder, Color _colBorder)
	{
		if (Event.current.type == 7)
		{
			GUI.color = _colFill;
			GUI.DrawTexture(_rect, Texture2D.whiteTexture);
			GUI.color = Color.white;
			if (_bDrawBorder)
			{
				_rect.width -= 1f;
				_rect.height -= 1f;
				GUIUtils.DrawRect(_rect, _colBorder);
			}
		}
	}

	public static void DrawFilledRect(Rect _rect, Texture2D _tex, bool _bDrawBorder, Color _colBorder)
	{
		if (Event.current.type == 7)
		{
			GUI.DrawTexture(_rect, _tex);
			if (_bDrawBorder)
			{
				GUIUtils.DrawRect(_rect, _colBorder);
			}
		}
	}

	public static void DrawRect(Rect _rect, Color _col)
	{
		GUIUtils.DrawLine(new Vector2(_rect.x, _rect.y), new Vector2(_rect.x, _rect.y + _rect.height), _col);
		GUIUtils.DrawLine(new Vector2(_rect.x, _rect.y + _rect.height), new Vector2(_rect.x + _rect.width, _rect.y + _rect.height), _col);
		GUIUtils.DrawLine(new Vector2(_rect.x + _rect.width, _rect.y + _rect.height), new Vector2(_rect.x + _rect.width, _rect.y), _col);
		GUIUtils.DrawLine(new Vector2(_rect.x + _rect.width, _rect.y), new Vector2(_rect.x, _rect.y), _col);
	}

	public static void DrawArrow(Vector2 pos, Vector2 dir, float size, Color col)
	{
		Vector2 vector;
		vector..ctor(dir.y, -dir.x);
		dir *= size;
		vector *= size;
		Vector2 vector2 = new Vector2(pos.x + dir.x, pos.y - dir.y);
		Vector2 vector3;
		vector3..ctor(pos.x + dir.x / 3f, pos.y - dir.y / 3f);
		Vector2 vector4;
		vector4..ctor(vector3.x + vector.x, vector3.y - vector.y);
		Vector2 vector5;
		vector5..ctor(vector3.x + -vector.x, vector3.y - -vector.y);
		GUIUtils.DrawLine(vector2, vector4, col);
		GUIUtils.DrawLine(vector2, vector5, col);
		GUIUtils.DrawLine(vector4, vector5, col);
		vector4 += new Vector2(-vector.x / 2f, vector.y / 2f);
		vector5 += new Vector2(vector.x / 2f, -vector.y / 2f);
		Vector2 vector6;
		vector6..ctor(vector4.x - dir.x, vector4.y + dir.y);
		Vector2 vector7;
		vector7..ctor(vector5.x - dir.x, vector5.y + dir.y);
		GUIUtils.DrawLine(vector4, vector6, col);
		GUIUtils.DrawLine(vector5, vector7, col);
		GUIUtils.DrawLine(vector6, vector7, col);
	}

	public static void DrawTriangle(Vector2 pos, Vector2 dir, float size, Color col)
	{
		Vector2 vector;
		vector..ctor(dir.y, -dir.x);
		dir *= size;
		vector *= size;
		Vector2 vector2 = new Vector2(pos.x + dir.x / 2f, pos.y - dir.y / 2f);
		Vector2 vector3;
		vector3..ctor(pos.x - dir.x / 2f, pos.y + dir.y / 2f);
		Vector2 vector4;
		vector4..ctor(vector3.x + vector.x, vector3.y - vector.y);
		Vector2 vector5;
		vector5..ctor(vector3.x + -vector.x, vector3.y - -vector.y);
		GUIUtils.DrawLine(vector2, vector4, col);
		GUIUtils.DrawLine(vector2, vector5, col);
		GUIUtils.DrawLine(vector4, vector5, col);
	}

	public static void DrawTriangleWide(Vector3 pos, Vector3 facing, Vector3 perpDir, float size, Color _color)
	{
		Vector3 vector = Vector3.Cross(facing, perpDir);
		facing *= size;
		vector *= size;
		Vector3 vector2 = pos + facing;
		Vector3 vector3 = pos + vector;
		Vector3 vector4 = pos - vector;
		GUIUtils.DrawLineWide(vector2, vector3, _color, _color);
		GUIUtils.DrawLineWide(vector2, vector4, _color, _color);
		GUIUtils.DrawLineWide(vector3, vector4, _color, _color);
	}

	public static void DrawRectWide(Vector3 pos, Vector3 facing, Vector3 perpDir, float size, Color _color)
	{
		Vector3 vector = Vector3.Cross(facing, perpDir);
		facing *= size;
		vector *= size;
		Vector3 vector2 = pos + vector / 2f;
		Vector3 vector3 = pos - vector / 2f;
		Vector3 vector4 = vector2 + facing;
		Vector3 vector5 = vector3 + facing;
		GUIUtils.DrawLineWide(vector2, vector3, _color, _color);
		GUIUtils.DrawLineWide(vector2, vector4, _color, _color);
		GUIUtils.DrawLineWide(vector3, vector5, _color, _color);
		GUIUtils.DrawLineWide(vector4, vector5, _color, _color);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool clip_test(float p, float q, ref float u1, ref float u2)
	{
		bool flag = true;
		if ((double)p < 0.0)
		{
			float num = q / p;
			if (num > u2)
			{
				flag = false;
			}
			else if (num > u1)
			{
				u1 = num;
			}
		}
		else if ((double)p > 0.0)
		{
			float num = q / p;
			if (num < u1)
			{
				flag = false;
			}
			else if (num < u2)
			{
				u2 = num;
			}
		}
		else if ((double)q < 0.0)
		{
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool segment_rect_intersection(Rect bounds, ref Vector2 p1, ref Vector2 p2)
	{
		float num = 0f;
		float num2 = 1f;
		float num3 = p2.x - p1.x;
		if (GUIUtils.clip_test(-num3, p1.x - bounds.xMin, ref num, ref num2) && GUIUtils.clip_test(num3, bounds.xMax - p1.x, ref num, ref num2))
		{
			float num4 = p2.y - p1.y;
			if (GUIUtils.clip_test(-num4, p1.y - bounds.yMin, ref num, ref num2) && GUIUtils.clip_test(num4, bounds.yMax - p1.y, ref num, ref num2))
			{
				if ((double)num2 < 1.0)
				{
					p2.x = p1.x + num2 * num3;
					p2.y = p1.y + num2 * num4;
				}
				if ((double)num > 0.0)
				{
					p1.x += num * num3;
					p1.y += num * num4;
				}
				return true;
			}
		}
		return false;
	}

	public static void BeginGroup(Rect position)
	{
		GUIUtils.clippingEnabled = true;
		GUIUtils.clippingBounds = new Rect(0f, 0f, position.width, position.height);
		GUI.BeginGroup(position);
	}

	public static void EndGroup()
	{
		GUI.EndGroup();
		GUIUtils.clippingBounds = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
		GUIUtils.clippingEnabled = false;
	}

	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
	{
		if (GUIUtils.clippingEnabled && !GUIUtils.segment_rect_intersection(GUIUtils.clippingBounds, ref pointA, ref pointB))
		{
			return;
		}
		if (!GUIUtils.lineMaterial)
		{
			GUIUtils.createLineMaterial();
		}
		GUIUtils.lineMaterial.SetPass(0);
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex3(pointA.x, pointA.y, 0f);
		GL.Vertex3(pointB.x, pointB.y, 0f);
		GL.End();
	}

	public static GUIUtils.IntRect RectIntersection(GUIUtils.IntRect r1, GUIUtils.IntRect r2)
	{
		int num = r1.x;
		int num2 = r1.y;
		int x = r2.x;
		int y = r2.y;
		long num3 = (long)num;
		num3 += (long)r1.width;
		long num4 = (long)num2;
		num4 += (long)r1.height;
		long num5 = (long)x;
		num5 += (long)r2.width;
		long num6 = (long)y;
		num6 += (long)r2.height;
		if (num < x)
		{
			num = x;
		}
		if (num2 < y)
		{
			num2 = y;
		}
		if (num3 > num5)
		{
			num3 = num5;
		}
		if (num4 > num6)
		{
			num4 = num6;
		}
		num3 -= (long)num;
		num4 -= (long)num2;
		if (num3 < -2147483648L)
		{
			num3 = 2147483647L;
		}
		if (num4 < -2147483648L)
		{
			num4 = 2147483647L;
		}
		return new GUIUtils.IntRect(num, num2, (int)num3, (int)num4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void createLineMaterial()
	{
		GUIUtils.lineMaterial = new Material(Resources.Load("Shaders/DrawLine", typeof(Shader)) as Shader);
	}

	public static void DrawLine(Vector3 pointA, Vector3 pointB, Color color)
	{
		if (!GUIUtils.lineMaterial)
		{
			GUIUtils.createLineMaterial();
		}
		GL.PushMatrix();
		GUIUtils.lineMaterial.SetPass(0);
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex(pointA);
		GL.Vertex(pointB);
		GL.End();
		GL.PopMatrix();
	}

	public static void SetupLines(Camera _camera, float _lineWidth)
	{
		GUIUtils.lineCamera = _camera;
		GUIUtils.lineHalfWidth = _lineWidth * 0.5f;
	}

	public static void DrawLineWide(Vector3 pointA, Vector3 pointB, Color color)
	{
		GUIUtils.DrawLineWide(pointA, pointB, color, color);
	}

	public static void DrawLineWide(Vector3 pointA, Vector3 pointB, Color colorA, Color colorB)
	{
		if (!GUIUtils.lineMaterial)
		{
			GUIUtils.createLineMaterial();
		}
		Vector3 vector = GUIUtils.lineCamera.WorldToScreenPoint(pointA);
		Vector3 vector2 = GUIUtils.lineCamera.WorldToScreenPoint(pointB);
		Vector3 vector3 = Vector3.Cross((vector2 - vector).normalized, Vector3.forward) * GUIUtils.lineHalfWidth;
		GL.PushMatrix();
		GUIUtils.lineMaterial.SetPass(0);
		GL.Begin(7);
		GL.Color(colorA);
		GL.Vertex(GUIUtils.lineCamera.ScreenToWorldPoint(vector - vector3));
		GL.Vertex(GUIUtils.lineCamera.ScreenToWorldPoint(vector + vector3));
		GL.Color(colorB);
		GL.Vertex(GUIUtils.lineCamera.ScreenToWorldPoint(vector2 + vector3));
		GL.Vertex(GUIUtils.lineCamera.ScreenToWorldPoint(vector2 - vector3));
		GL.End();
		GL.PopMatrix();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool clippingEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Rect clippingBounds;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Material lineMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Camera lineCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lineHalfWidth;

	public class IntRect
	{
		public IntRect(int _x, int _y, int _width, int _height)
		{
			this.x = _x;
			this.y = _y;
			this.width = _width;
			this.height = _height;
		}

		public IntRect(float _x, float _y, float _width, float _height)
		{
			this.x = (int)_x;
			this.y = (int)_y;
			this.width = (int)_width;
			this.height = (int)_height;
		}

		public int x;

		public int y;

		public int width;

		public int height;
	}
}
