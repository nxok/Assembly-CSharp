using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Platform;
using Unity.Collections;
using UnityEngine;

public static class Utils
{
	public static CultureInfo StandardCulture
	{
		get
		{
			return Utils.ci;
		}
	}

	public static void InitStatic()
	{
		Utils.ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
		Utils.ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
		Utils.ci.NumberFormat.CurrencyDecimalSeparator = ".";
		Utils.ci.NumberFormat.NumberDecimalSeparator = ".";
		Utils.ci.NumberFormat.NumberGroupSeparator = ",";
		Utils.ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
		Utils.ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
		Utils.ci.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
		Utils.ci.DateTimeFormat.LongTimePattern = "HH:mm:ss";
		if (!GlobalCultureInfo.SetDefaultCulture(Utils.ci))
		{
			Log.Warning("Setting global culture failed!");
		}
		DynamicMethod dynamicMethod = new DynamicMethod("Memset", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static, CallingConventions.Standard, null, new Type[]
		{
			typeof(IntPtr),
			typeof(byte),
			typeof(int)
		}, typeof(Utils), true);
		ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
		ilgenerator.Emit(OpCodes.Ldarg_0);
		ilgenerator.Emit(OpCodes.Ldarg_1);
		ilgenerator.Emit(OpCodes.Ldarg_2);
		ilgenerator.Emit(OpCodes.Initblk);
		ilgenerator.Emit(OpCodes.Ret);
		Utils.MemsetDelegate = (Action<IntPtr, byte, int>)dynamicMethod.CreateDelegate(typeof(Action<IntPtr, byte, int>));
	}

	public static string GetChildPath(this Transform t, Transform child)
	{
		string text = child.name;
		Transform transform = child.parent;
		while (transform != null && transform != t)
		{
			text = transform.name + "/" + text;
			transform = transform.parent;
		}
		if (transform == null)
		{
			throw new Exception(t.name + " does not have a child " + child.name);
		}
		return text;
	}

	public static void SetTagsRecursively(Transform t, string tag)
	{
		t.gameObject.tag = tag;
		for (int i = t.childCount - 1; i >= 0; i--)
		{
			Utils.SetTagsRecursively(t.GetChild(i), tag);
		}
	}

	public static void SetTagsIfNoneRecursively(Transform t, string tag)
	{
		if (t.gameObject.CompareTag("Untagged"))
		{
			t.gameObject.tag = tag;
		}
		for (int i = t.childCount - 1; i >= 0; i--)
		{
			Utils.SetTagsIfNoneRecursively(t.GetChild(i), tag);
		}
	}

	public static void SetTagsIfMatchRecursively(Transform t, string matchTag, string tag)
	{
		if (t.gameObject.CompareTag(matchTag))
		{
			t.gameObject.tag = tag;
		}
		for (int i = t.childCount - 1; i >= 0; i--)
		{
			Utils.SetTagsIfMatchRecursively(t.GetChild(i), matchTag, tag);
		}
	}

	public static void DestroyWithTagRecursively(Transform t, string tag)
	{
		for (int i = t.childCount - 1; i >= 0; i--)
		{
			Utils.DestroyWithTagRecursively(t.GetChild(i), tag);
		}
		if (t.gameObject.CompareTag(tag))
		{
			Object.Destroy(t.gameObject);
		}
	}

	public static void SetLayerRecursively(GameObject go, int newLayer)
	{
		go.GetComponentsInChildren<Transform>(true, Utils.setLayerRecursivelyList);
		List<Transform> list = Utils.setLayerRecursivelyList;
		if (newLayer == 2)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				list[i].gameObject.layer = newLayer;
			}
		}
		else
		{
			for (int j = list.Count - 1; j >= 0; j--)
			{
				Transform transform = list[j];
				if (transform.CompareTag("LargeEntityBlocker"))
				{
					transform.gameObject.layer = 19;
				}
				else
				{
					transform.gameObject.layer = newLayer;
				}
			}
		}
		Utils.setLayerRecursivelyList.Clear();
	}

	public static void SetLayerRecursively(GameObject go, int newLayer, string[] excludeTags)
	{
		go.GetComponentsInChildren<Transform>(true, Utils.setLayerRecursivelyList);
		List<Transform> list = Utils.setLayerRecursivelyList;
		if (newLayer == 2)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				Utils.SetLayerWithExclusionList(list[i].gameObject, newLayer, excludeTags);
			}
		}
		else
		{
			for (int j = list.Count - 1; j >= 0; j--)
			{
				Transform transform = list[j];
				if (transform.CompareTag("LargeEntityBlocker"))
				{
					Utils.SetLayerWithExclusionList(transform.gameObject, 19, excludeTags);
				}
				else
				{
					Utils.SetLayerWithExclusionList(transform.gameObject, newLayer, excludeTags);
				}
			}
		}
		Utils.setLayerRecursivelyList.Clear();
	}

	public static void SetLayerWithExclusionList(GameObject go, int layer, string[] excludeTags)
	{
		if (excludeTags != null)
		{
			foreach (string text in excludeTags)
			{
				if (go.CompareTag(text))
				{
					return;
				}
			}
		}
		go.layer = layer;
	}

	public static void SetColliderLayerRecursively(GameObject go, int newLayer)
	{
		go.GetComponentsInChildren<Collider>(true, Utils.setColliderLayerRecursivelyList);
		for (int i = Utils.setColliderLayerRecursivelyList.Count - 1; i >= 0; i--)
		{
			Utils.setColliderLayerRecursivelyList[i].gameObject.layer = newLayer;
		}
	}

	public static void MoveTaggedToLayer(GameObject go, string tag, int newLayer)
	{
		go.GetComponentsInChildren<Transform>(true, Utils.setLayerRecursivelyList);
		List<Transform> list = Utils.setLayerRecursivelyList;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			Transform transform = list[i];
			if (transform.CompareTag(tag))
			{
				transform.gameObject.layer = newLayer;
			}
		}
		Utils.setLayerRecursivelyList.Clear();
	}

	public static void DrawBlockBounds(BoundsInt _bounds, Color _color, float _duration)
	{
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(World.blockToTransformPos(_bounds.min), World.blockToTransformPos(_bounds.max) + new Vector3(0f, 1f, 0f));
		bounds.Expand(new Vector3(0.5f, 0f, 0.5f));
		Utils.DrawBounds(bounds, _color, _duration);
	}

	public static void DrawBounds(Bounds _bounds, Color _color, float _duration)
	{
		Vector3 center = _bounds.center;
		Vector3 extents = _bounds.extents;
		Vector3 vector;
		vector..ctor(center.x - extents.x, center.y + extents.y, center.z - extents.z);
		Vector3 vector2;
		vector2..ctor(center.x + extents.x, center.y + extents.y, center.z - extents.z);
		Vector3 vector3;
		vector3..ctor(center.x - extents.x, center.y - extents.y, center.z - extents.z);
		Vector3 vector4;
		vector4..ctor(center.x + extents.x, center.y - extents.y, center.z - extents.z);
		Vector3 vector5;
		vector5..ctor(center.x - extents.x, center.y + extents.y, center.z + extents.z);
		Vector3 vector6;
		vector6..ctor(center.x + extents.x, center.y + extents.y, center.z + extents.z);
		Vector3 vector7;
		vector7..ctor(center.x - extents.x, center.y - extents.y, center.z + extents.z);
		Vector3 vector8;
		vector8..ctor(center.x + extents.x, center.y - extents.y, center.z + extents.z);
		Debug.DrawLine(vector, vector2, _color, _duration);
		Debug.DrawLine(vector2, vector4, _color, _duration);
		Debug.DrawLine(vector4, vector3, _color, _duration);
		Debug.DrawLine(vector3, vector, _color, _duration);
		Debug.DrawLine(vector5, vector6, _color, _duration);
		Debug.DrawLine(vector6, vector8, _color, _duration);
		Debug.DrawLine(vector8, vector7, _color, _duration);
		Debug.DrawLine(vector7, vector5, _color, _duration);
		Debug.DrawLine(vector, vector5, _color, _duration);
		Debug.DrawLine(vector2, vector6, _color, _duration);
		Debug.DrawLine(vector4, vector8, _color, _duration);
		Debug.DrawLine(vector3, vector7, _color, _duration);
	}

	public static void DrawAxisLines(Vector3 centerPos, float radius, Color color, float duration)
	{
		Vector3 vector = centerPos;
		Vector3 vector2 = centerPos;
		vector.x -= radius;
		vector2.x += radius;
		Debug.DrawLine(vector, vector2, color, duration);
		vector.x = centerPos.x;
		vector2.x = centerPos.x;
		vector.z -= radius;
		vector2.z += radius;
		Debug.DrawLine(vector, vector2, color, duration);
		vector.z = centerPos.z;
		vector2.z = centerPos.z;
		vector.y -= radius;
		vector2.y += radius;
		Debug.DrawLine(vector, vector2, color, duration);
	}

	public static void DrawBoxLines(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		Utils.DrawBoxLinesHorizontal(minPos, maxPos, color, duration);
		Utils.DrawBoxLinesVerticle(minPos, maxPos, color, duration);
	}

	public static void DrawBoxLinesHorizontal(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		Vector3 vector;
		vector..ctor(maxPos.x, minPos.y, maxPos.z);
		Vector3 vector2 = new Vector3(minPos.x, maxPos.y, minPos.z);
		Utils.DrawRectLinesHorzontal(minPos, vector, color, duration);
		Utils.DrawRectLinesHorzontal(vector2, maxPos, color, duration);
	}

	public static void DrawBoxLinesVerticle(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		Vector3 vector;
		vector..ctor(0f, maxPos.y - minPos.y);
		Debug.DrawRay(minPos, vector, color, duration);
		Debug.DrawRay(new Vector3(minPos.x, minPos.y, maxPos.z), vector, color, duration);
		Debug.DrawRay(maxPos, -vector, color, duration);
		Debug.DrawRay(new Vector3(maxPos.x, minPos.y, minPos.z), vector, color, duration);
	}

	public static void DrawCubeLines(Vector3 minPos, float size, Color color, float duration)
	{
		Vector3 vector = minPos;
		vector.x += size;
		vector.y += size;
		vector.z += size;
		Utils.DrawBoxLines(minPos, vector, color, duration);
	}

	public static void DrawCubeLinesCenter(Vector3 centerPos, float radius, Color color, float duration)
	{
		Vector3 vector = centerPos;
		vector.x -= radius;
		vector.y -= radius;
		vector.z -= radius;
		Vector3 vector2 = centerPos;
		vector2.x += radius;
		vector2.y += radius;
		vector2.z += radius;
		Utils.DrawBoxLines(vector, vector2, color, duration);
	}

	public static void DrawRectLinesHorzontal(Vector3 minPos, Vector3 maxPos, Color color, float duration)
	{
		Vector3 vector;
		vector..ctor(maxPos.x, minPos.y, minPos.z);
		Vector3 vector2;
		vector2..ctor(minPos.x, minPos.y, maxPos.z);
		Debug.DrawLine(minPos, vector2, color, duration);
		Debug.DrawLine(vector2, maxPos, color, duration);
		Debug.DrawLine(maxPos, vector, color, duration);
		Debug.DrawLine(vector, minPos, color, duration);
	}

	public static void DrawCylinderLinesHorzontal(Vector3 centerPos, float radius, float height, Color startColor, Color endColor, int segments, float duration)
	{
		Utils.DrawCircleLinesHorzontal(centerPos, radius, startColor, endColor, segments, duration);
		centerPos.y += height;
		Utils.DrawCircleLinesHorzontal(centerPos, radius, startColor, endColor, segments, duration);
	}

	public static void DrawCircleLinesHorzontal(Vector3 centerPos, float radius, Color startColor, Color endColor, int segments, float duration)
	{
		if (segments < 3)
		{
			segments = 3;
		}
		float num = -6.2831855f / (float)segments;
		float num2 = 1.5707964f;
		float num3 = 0f;
		float num4 = 1f / (float)(segments - 1);
		Vector3 vector;
		vector.y = centerPos.y;
		vector.x = centerPos.x + Mathf.Cos(num2) * radius;
		vector.z = centerPos.z + Mathf.Sin(num2) * radius;
		Vector3 vector2;
		vector2.y = centerPos.y;
		for (int i = 0; i < segments; i++)
		{
			num2 += num;
			vector2.x = centerPos.x + Mathf.Cos(num2) * radius;
			vector2.z = centerPos.z + Mathf.Sin(num2) * radius;
			Color color = Color.Lerp(startColor, endColor, num3);
			Debug.DrawLine(vector, vector2, color, duration);
			vector.x = vector2.x;
			vector.z = vector2.z;
			num3 += num4;
		}
	}

	public static void DrawLine(Vector3 startPos, Vector3 endPos, Color startColor, Color endColor, int segments, float duration = 0f)
	{
		if (segments <= 1)
		{
			Debug.DrawLine(startPos, endPos, startColor, duration);
			return;
		}
		Vector3 vector = (endPos - startPos) * (1f / (float)segments);
		float num = 0f;
		float num2 = 1f / (float)(segments - 1);
		for (int i = 0; i < segments; i++)
		{
			Color color = Color.Lerp(startColor, endColor, num);
			Debug.DrawRay(startPos, vector, color, duration);
			startPos += vector;
			num += num2;
		}
	}

	public static void DrawRay(Vector3 startPos, Vector3 dir, Color startColor, Color endColor, int segments, float duration = 0f)
	{
		Utils.DrawLine(startPos, startPos + dir, startColor, endColor, segments, duration);
	}

	public static void DrawOutline(Rect position, string text, GUIStyle style, Color outColor, Color inColor)
	{
		style.normal.textColor = outColor;
		float num = position.x;
		position.x = num - 1f;
		GUI.Label(position, text, style);
		position.x += 2f;
		GUI.Label(position, text, style);
		num = position.x;
		position.x = num - 1f;
		num = position.y;
		position.y = num - 1f;
		GUI.Label(position, text, style);
		position.y += 2f;
		GUI.Label(position, text, style);
		num = position.y;
		position.y = num - 1f;
		style.normal.textColor = inColor;
		GUI.Label(position, text, style);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastAbsInt(int _x)
	{
		if (_x >= 0)
		{
			return _x;
		}
		if (_x == -2147483648)
		{
			return int.MaxValue;
		}
		return -_x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastAbs(float _x)
	{
		if (_x < 0f)
		{
			return -_x;
		}
		return _x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Fastfloor(float x)
	{
		if (x < 0f)
		{
			return (int)(x - 0.9999999f);
		}
		return (int)x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int Fastfloor(double x)
	{
		if (x < 0.0)
		{
			return (int)(x - 0.9999998807907104);
		}
		return (int)x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastMax(int v1, int v2)
	{
		if (v1 < v2)
		{
			return v2;
		}
		return v1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastMax(float v1, float v2)
	{
		if (v1 < v2)
		{
			return v2;
		}
		return v1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte FastMax(byte v1, byte v2)
	{
		if (v1 < v2)
		{
			return v2;
		}
		return v1;
	}

	public static float FastMax(float v1, float v2, float v3, float v4)
	{
		if (v1 >= v2 && v1 >= v3 && v1 >= v4)
		{
			return v1;
		}
		if (v2 >= v1 && v2 >= v3 && v2 >= v4)
		{
			return v2;
		}
		if (v3 >= v1 && v3 >= v2 && v3 >= v4)
		{
			return v3;
		}
		return v4;
	}

	public static int FastMax(int v1, int v2, int v3, int v4)
	{
		if (v1 >= v2 && v1 >= v3 && v1 >= v4)
		{
			return v1;
		}
		if (v2 >= v1 && v2 >= v3 && v2 >= v4)
		{
			return v2;
		}
		if (v3 >= v1 && v3 >= v2 && v3 >= v4)
		{
			return v3;
		}
		return v4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastMin(float v1, float v2)
	{
		if (v1 >= v2)
		{
			return v2;
		}
		return v1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastMin(int v1, int v2)
	{
		if (v1 >= v2)
		{
			return v2;
		}
		return v1;
	}

	public static int FastMin(int v1, int v2, int v3, int v4)
	{
		if (v1 <= v2 && v1 <= v3 && v1 <= v4)
		{
			return v1;
		}
		if (v2 <= v1 && v2 <= v3 && v2 <= v4)
		{
			return v2;
		}
		if (v3 <= v1 && v3 <= v2 && v3 <= v4)
		{
			return v3;
		}
		return v4;
	}

	public static float FastMin(float v1, float v2, float v3, float v4)
	{
		if (v1 <= v2 && v1 <= v3 && v1 <= v4)
		{
			return v1;
		}
		if (v2 <= v1 && v2 <= v3 && v2 <= v4)
		{
			return v2;
		}
		if (v3 <= v1 && v3 <= v2 && v3 <= v4)
		{
			return v3;
		}
		return v4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastClamp(float _v, float _min, float _max)
	{
		if (_v <= _min)
		{
			return _min;
		}
		if (_v >= _max)
		{
			return _max;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastClamp(int _v, int _min, int _max)
	{
		if (_v <= _min)
		{
			return _min;
		}
		if (_v >= _max)
		{
			return _max;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastClamp01(float _v)
	{
		if (_v <= 0f)
		{
			return 0f;
		}
		if (_v >= 1f)
		{
			return 1f;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double FastClamp01(double _v)
	{
		if (_v <= 0.0)
		{
			return 0.0;
		}
		if (_v >= 1.0)
		{
			return 1.0;
		}
		return _v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastLerp(float a, float b, float t)
	{
		return a + (b - a) * Utils.FastClamp01(t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastLerpUnclamped(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastMoveTowards(float current, float target, float maxDelta)
	{
		if (Utils.FastAbs(target - current) <= maxDelta)
		{
			return target;
		}
		return current + Mathf.Sign(target - current) * maxDelta;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastRoundToInt(float f)
	{
		return (int)Math.Round((double)f);
	}

	public static int FastRoundToIntAndMod(float _f, int _mod)
	{
		if (_mod == 0)
		{
			return 0;
		}
		if (_f <= 0f)
		{
			return (_mod + (int)(_f - 0.5f) % _mod) % _mod;
		}
		return (int)(_f + 0.5f) % _mod;
	}

	public static Texture2D LoadRawStampFile(string _filepath)
	{
		Texture2D texture2D2;
		using (Stream stream = SdFile.OpenRead(_filepath))
		{
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
				int num = (int)Mathf.Sqrt((float)(binaryReader.BaseStream.Length / 6L));
				Texture2D texture2D = new Texture2D(num, num, 20, false, false);
				Color[] array = new Color[num * num];
				for (int i = 0; i < array.Length; i++)
				{
					float num2 = (float)binaryReader.ReadUInt16() / 65535f;
					float num3 = (float)binaryReader.ReadUInt16() / 65535f;
					float num4 = (float)binaryReader.ReadUInt16() / 65535f;
					array[i] = new Color(num2, num2, num4, num3);
				}
				texture2D.SetPixels(array);
				texture2D.Apply();
				texture2D2 = texture2D;
			}
		}
		return texture2D2;
	}

	public static Color[] LoadRawStampFileArray(string _filepath)
	{
		Color[] array3;
		using (Stream stream = SdFile.OpenRead(_filepath))
		{
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
				byte[] array = new byte[binaryReader.BaseStream.Length];
				binaryReader.Read(array, 0, array.Length);
				int num = (int)(binaryReader.BaseStream.Length / 6L);
				Color[] array2 = new Color[num];
				Color black = Color.black;
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					black.r = (float)((int)array[num2] | ((int)array[num2 + 1] << 8)) / 65535f;
					black.a = (float)((int)array[num2 + 2] | ((int)array[num2 + 3] << 8)) / 65535f;
					black.b = (float)((int)array[num2 + 4] | ((int)array[num2 + 5] << 8)) / 65535f;
					array2[i] = black;
					num2 += 6;
				}
				array3 = array2;
			}
		}
		return array3;
	}

	public static void SaveRawStampFile(string _filepath, Texture2D _image)
	{
		using (Stream stream = SdFile.Open(_filepath, FileMode.CreateNew))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(stream))
			{
				binaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
				Color[] pixels = _image.GetPixels();
				for (int i = 0; i < pixels.Length; i++)
				{
					binaryWriter.Write((ushort)(pixels[i].r * 65535f));
					binaryWriter.Write((ushort)(pixels[i].g * 65535f));
					binaryWriter.Write((ushort)(pixels[i].b * 65535f));
				}
			}
		}
	}

	public static void SaveRawStampFile(string _filepath, Color[] _image)
	{
		using (Stream stream = SdFile.Open(_filepath, FileMode.CreateNew))
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(stream))
			{
				binaryWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
				for (int i = 0; i < _image.Length; i++)
				{
					binaryWriter.Write((ushort)(_image[i].r * 65535f));
					binaryWriter.Write((ushort)(_image[i].g * 65535f));
					binaryWriter.Write((ushort)(_image[i].b * 65535f));
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Repeat(float _value, float _length)
	{
		return Utils.FastClamp(_value - (float)Utils.Fastfloor(_value / _length) * _length, 0f, _length);
	}

	public static float DeltaAngle(float _angle1, float _angle2)
	{
		float num = Utils.Repeat(_angle2 - _angle1, 360f);
		if (num > 180f)
		{
			num -= 360f;
		}
		return num;
	}

	public static float GetAngleBetween(Vector3 _dir1, Vector3 _dir2)
	{
		float num = Mathf.Atan2(_dir1.z, _dir1.x) * 57.29578f;
		float num2 = Mathf.Atan2(_dir2.z, _dir2.x) * 57.29578f;
		float num3 = num - num2;
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		if (num3 < -180f)
		{
			num3 += 360f;
		}
		return num3;
	}

	public static int Get4HitDirectionAsInt(Vector3 _direction, Vector3 _myLook)
	{
		float angleBetween = Utils.GetAngleBetween(_direction, _myLook);
		if (angleBetween > -45f && angleBetween <= 45f)
		{
			return 1;
		}
		if (angleBetween < -45f && angleBetween >= -135f)
		{
			return 3;
		}
		if (angleBetween > 45f && angleBetween <= 135f)
		{
			return 2;
		}
		return 0;
	}

	public static int Get6HitDirectionAsInt(Vector3 _direction, Vector3 _myLook)
	{
		float angleBetween = Utils.GetAngleBetween(_direction, _myLook);
		if (angleBetween > -45f && angleBetween <= 45f)
		{
			return 1;
		}
		if (angleBetween < -45f && angleBetween >= -120f)
		{
			return 3;
		}
		if (angleBetween > 45f && angleBetween <= 120f)
		{
			return 2;
		}
		if (angleBetween > 120f && angleBetween < 160f)
		{
			return 4;
		}
		if (angleBetween < -120f && angleBetween > -160f)
		{
			return 5;
		}
		return 0;
	}

	public static Utils.EnumHitDirection GetHitDirection4Sides(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		targetDir = targetDir.normalized;
		float num = Vector3.Dot(Vector3.Cross(fwd, targetDir), up);
		if (num > 0.75f)
		{
			return Utils.EnumHitDirection.Left;
		}
		if (num < -0.75f)
		{
			return Utils.EnumHitDirection.Right;
		}
		if ((double)Mathf.Abs(targetDir.y) > 0.05 && (double)Mathf.Abs(targetDir.x / targetDir.y) < 0.1 && (double)Mathf.Abs(targetDir.z / targetDir.y) < 0.1)
		{
			return Utils.EnumHitDirection.Back;
		}
		if (Mathf.Abs(Mathf.Atan2(targetDir.z, targetDir.x) - Mathf.Atan2(fwd.z, fwd.x)) > 1f)
		{
			return Utils.EnumHitDirection.Front;
		}
		return Utils.EnumHitDirection.Back;
	}

	public static Quaternion BlockFaceToRotation(BlockFace _blockFace)
	{
		switch (_blockFace)
		{
		case BlockFace.Top:
			return Quaternion.identity;
		case BlockFace.Bottom:
			return Quaternion.AngleAxis(180f, Vector3.forward);
		case BlockFace.North:
			return Quaternion.AngleAxis(90f, Vector3.right);
		case BlockFace.West:
			return Quaternion.AngleAxis(90f, Vector3.forward);
		case BlockFace.South:
			return Quaternion.AngleAxis(-90f, Vector3.right);
		case BlockFace.East:
			return Quaternion.AngleAxis(-90f, Vector3.forward);
		default:
			return Quaternion.identity;
		}
	}

	public static Vector3 BlockFaceToVector(BlockFace _face)
	{
		switch (_face)
		{
		case BlockFace.Top:
			return new Vector3(0f, 1f, 0f);
		case BlockFace.Bottom:
			return new Vector3(0f, -1f, 0f);
		case BlockFace.North:
			return new Vector3(0f, 0f, 1f);
		case BlockFace.West:
			return new Vector3(-1f, 0f, 0f);
		case BlockFace.South:
			return new Vector3(0f, 0f, -1f);
		case BlockFace.East:
			return new Vector3(1f, 0f, 0f);
		default:
			return Vector3.zero;
		}
	}

	public static void MoveInBlockFaceDirection(Vector3[] _vertices, BlockFace _face, float d)
	{
		Vector3 zero = Vector3.zero;
		switch (_face)
		{
		case BlockFace.Top:
			zero..ctor(0f, d, 0f);
			break;
		case BlockFace.Bottom:
			zero..ctor(0f, -d, 0f);
			break;
		case BlockFace.North:
			zero..ctor(0f, 0f, d);
			break;
		case BlockFace.West:
			zero..ctor(-d, 0f, 0f);
			break;
		case BlockFace.South:
			zero..ctor(0f, 0f, -d);
			break;
		case BlockFace.East:
			zero..ctor(d, 0f, 0f);
			break;
		}
		for (int i = 0; i < _vertices.Length; i++)
		{
			_vertices[i] += zero;
		}
	}

	public static string EncryptOrDecrypt(string text, string key)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			stringBuilder.Append(text[i] ^ key[i % key.Length]);
		}
		return stringBuilder.ToString();
	}

	public static string Crypt(string text)
	{
		return Utils.EncryptOrDecrypt(text, "X");
	}

	public static string UnCryptFromBase64(char[] text)
	{
		return Utils.Crypt(Utils.FromBase64(text));
	}

	public static string ToBase64(string _str)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(_str));
	}

	public static string FromBase64(string _encodedStr)
	{
		string text;
		try
		{
			byte[] array = Convert.FromBase64String(_encodedStr);
			text = Encoding.UTF8.GetString(array);
		}
		catch (FormatException ex)
		{
			Log.Error("FBase64 Exception");
			Log.Exception(ex);
			text = string.Empty;
		}
		return text;
	}

	public static string FromBase64(char[] _bytes)
	{
		string text;
		try
		{
			byte[] array = Convert.FromBase64CharArray(_bytes, 0, _bytes.Length);
			text = Encoding.UTF8.GetString(array);
		}
		catch (FormatException ex)
		{
			Log.Error("FBase64A Exception");
			Log.Exception(ex);
			text = string.Empty;
		}
		return text;
	}

	public static string HashString(string password)
	{
		if (password.Length == 0)
		{
			return password;
		}
		return password.GetHashCode().ToString();
	}

	public static string ColorToHex(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	public static Color Saturate(Color _c)
	{
		if (_c.r < 0f)
		{
			_c.r = 0f;
		}
		if (_c.r > 1f)
		{
			_c.r = 1f;
		}
		if (_c.g < 0f)
		{
			_c.g = 0f;
		}
		if (_c.g > 1f)
		{
			_c.g = 1f;
		}
		if (_c.b < 0f)
		{
			_c.b = 0f;
		}
		if (_c.b > 1f)
		{
			_c.b = 1f;
		}
		return _c;
	}

	public static ushort ToColor5(Color col)
	{
		return (ushort)(((int)(col.r * 31f + 0.5f) << 10) | ((int)(col.g * 31f + 0.5f) << 5) | (int)(col.b * 31f + 0.5f));
	}

	public static Color FromColor5(ushort col)
	{
		return new Color((float)((col >> 10) & 31) / 31f, (float)((col >> 5) & 31) / 31f, (float)(col & 31) / 31f);
	}

	public static Color32 FromColor5To32(ushort col)
	{
		return new Color32((byte)((float)((col >> 10) & 31) / 31f * 255f), (byte)((float)((col >> 5) & 31) / 31f * 255f), (byte)((float)(col & 31) / 31f * 255f), byte.MaxValue);
	}

	public static GameRandom RandomFromSeedOnPos(int x, int y, int seed)
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		long num = (long)gameRandom.RandomInt / 2L * 2L + 1L;
		long num2 = (long)gameRandom.RandomInt / 2L * 2L + 1L;
		int num3 = (int)(((long)((long)x << 4) * num + (long)((long)y << 4) * num2) ^ (long)seed);
		gameRandom.SetSeed(num3);
		return gameRandom;
	}

	public static GameRandom RandomFromSeedOnPos(int _x, int _y, int _z, int _seed)
	{
		int num = _seed + _x + (_z << 14) + (_y << 24);
		return GameRandomManager.Instance.CreateGameRandom(num);
	}

	public static string DescribeTimeSince(DateTime _newer, DateTime _older)
	{
		TimeSpan timeSpan = _newer - _older;
		if (timeSpan.TotalMinutes < 1.0)
		{
			return Localization.Get("xuiLastSeenNow", false, null);
		}
		if (timeSpan.TotalMinutes < 60.0)
		{
			int num = (int)timeSpan.TotalMinutes;
			if (num != 1)
			{
				return string.Format(Localization.Get("xuiLastSeenMinutes", false, null), num);
			}
			return Localization.Get("xuiLastSeen1Minute", false, null);
		}
		else if (timeSpan.TotalHours < 24.0)
		{
			int num2 = (int)timeSpan.TotalHours;
			if (num2 != 1)
			{
				return string.Format(Localization.Get("xuiLastSeenHours", false, null), num2);
			}
			return Localization.Get("xuiLastSeen1Hour", false, null);
		}
		else
		{
			int num3 = (int)timeSpan.TotalDays;
			if (num3 != 1)
			{
				return string.Format(Localization.Get("xuiLastSeenDays", false, null), num3);
			}
			return Localization.Get("xuiLastSeen1Day", false, null);
		}
	}

	public static float ToCelsius(float fromFahrenheit)
	{
		return (fromFahrenheit - 32f) * 0.5555556f;
	}

	public static float ToRelativeCelsius(float fromFahrenheit)
	{
		return fromFahrenheit * 0.5555556f;
	}

	public static string EscapeBbCodes(string _input, bool _removeInstead = false, bool _allowUrls = false)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return _input;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		int num = 0;
		while (i < _input.Length)
		{
			ValueTuple<int, int, bool> valueTuple = Utils.FindNextBbCode(_input, i, _allowUrls);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			if (item == -1)
			{
				stringBuilder.Append(_input.Substring(num));
				break;
			}
			stringBuilder.Append(_input.Substring(num, item - num));
			if (!_removeInstead)
			{
				int num2 = 0;
				while (_input[item + num2] == '[')
				{
					num2++;
					stringBuilder.Append("[");
				}
				stringBuilder.Append("[/c]");
				stringBuilder.Append(_input.Substring(item + num2, item2 - num2));
			}
			i = item + item2;
			num = i;
		}
		return stringBuilder.ToString();
	}

	public static ValueTuple<int, int, bool> FindNextBbCode(string _input, int _startIndex, bool _allowUrls = false)
	{
		int i = _startIndex;
		int length = _input.Length;
		while (i < length)
		{
			if (_input[i] != '[')
			{
				i++;
			}
			else
			{
				Match match = Utils.nestedEscapePattern.Match(_input.Substring(i, length - i));
				if (match.Success)
				{
					int index = match.Index;
					if (index == 0)
					{
						return new ValueTuple<int, int, bool>(i, match.Length, true);
					}
					if (index == 1)
					{
						int num = _input.IndexOf(']', i + match.Length);
						if (num == i + match.Length)
						{
							return new ValueTuple<int, int, bool>(i, num + 1 - i, true);
						}
						if (num == -1)
						{
							i += 4;
							continue;
						}
						string text = "[" + _input.Substring(i + match.Length, num - (i + match.Length)) + "]";
						ValueTuple<int, int, bool> valueTuple = Utils.FindNextBbCode(text, 0, false);
						int item = valueTuple.Item1;
						int item2 = valueTuple.Item2;
						if (item == 0 && item2 == text.Length)
						{
							return new ValueTuple<int, int, bool>(i, num + 1 - i, true);
						}
					}
				}
				if (i + 3 > length || _input[i] != '[')
				{
					i++;
				}
				else
				{
					if (_input[i + 2] == ']')
					{
						char c = _input[i + 1];
						if (c <= 'I')
						{
							if (c <= 'B')
							{
								if (c != '-' && c != 'B')
								{
									goto IL_01A8;
								}
							}
							else if (c != 'C' && c != 'I')
							{
								goto IL_01A8;
							}
						}
						else if (c <= 'b')
						{
							switch (c)
							{
							case 'S':
							case 'T':
							case 'U':
								break;
							default:
								if (c != 'b')
								{
									goto IL_01A8;
								}
								break;
							}
						}
						else if (c != 'c' && c != 'i')
						{
							switch (c)
							{
							case 's':
							case 't':
							case 'u':
								break;
							default:
								goto IL_01A8;
							}
						}
						return new ValueTuple<int, int, bool>(i, 3, false);
					}
					IL_01A8:
					if (i + 4 <= length && _input[i + 1] == '/' && _input[i + 3] == ']')
					{
						char c = _input[i + 2];
						if (c <= 'I')
						{
							if (c <= 'B')
							{
								if (c != '-' && c != 'B')
								{
									goto IL_0255;
								}
							}
							else if (c != 'C' && c != 'I')
							{
								goto IL_0255;
							}
						}
						else if (c <= 'b')
						{
							switch (c)
							{
							case 'S':
							case 'T':
							case 'U':
								break;
							default:
								if (c != 'b')
								{
									goto IL_0255;
								}
								break;
							}
						}
						else if (c != 'c' && c != 'i')
						{
							switch (c)
							{
							case 's':
							case 't':
							case 'u':
								break;
							default:
								goto IL_0255;
							}
						}
						return new ValueTuple<int, int, bool>(i, 4, false);
					}
					IL_0255:
					if (i + 4 <= length && _input[i + 3] == ']' && Utils.hexAlphaMatcher.IsMatch(_input.Substring(i + 1, 2)))
					{
						return new ValueTuple<int, int, bool>(i, 4, false);
					}
					if (i + 5 <= length && _input[i + 4] == ']' && _input[i + 1] == 's' && _input[i + 2] == 'u' && (_input[i + 3] == 'b' || _input[i + 3] == 'p'))
					{
						return new ValueTuple<int, int, bool>(i, 5, false);
					}
					if (i + 6 <= length && _input[i + 5] == ']')
					{
						string text2 = _input.Substring(i, 6);
						if (text2.EqualsCaseInsensitive("[/sub]") || text2.EqualsCaseInsensitive("[/sup]"))
						{
							return new ValueTuple<int, int, bool>(i, 6, false);
						}
					}
					if (i + 5 < length && _input.Substring(i, 5).EqualsCaseInsensitive("[url="))
					{
						int num2 = _input.IndexOf(']', i + 5);
						if (num2 != -1)
						{
							if (_allowUrls)
							{
								i = num2;
								continue;
							}
							return new ValueTuple<int, int, bool>(i, num2 - i + 1, false);
						}
					}
					if (!_allowUrls && i + 6 <= length && _input.Substring(i, 6).EqualsCaseInsensitive("[/url]"))
					{
						return new ValueTuple<int, int, bool>(i, 6, false);
					}
					if (i + 8 <= length && _input[i + 7] == ']' && Utils.hexRgbMatcher.IsMatch(_input.Substring(i + 1, 6)))
					{
						return new ValueTuple<int, int, bool>(i, 8, false);
					}
					if (i + 10 <= length && _input[i + 9] == ']' && Utils.hexRgbaMatcher.IsMatch(_input.Substring(i + 1, 8)))
					{
						return new ValueTuple<int, int, bool>(i, 10, false);
					}
					i++;
				}
			}
		}
		return new ValueTuple<int, int, bool>(-1, 0, false);
	}

	public static string GetVisibileTextWithBbCodes(string _input)
	{
		int i = 0;
		StringBuilder stringBuilder = new StringBuilder();
		while (i < _input.Length)
		{
			ValueTuple<int, int, bool> valueTuple = Utils.FindNextBbCode(_input, i, false);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			bool item3 = valueTuple.Item3;
			if (item == -1)
			{
				stringBuilder.Append(_input, i, _input.Length - i);
				break;
			}
			stringBuilder.Append(_input, i, item - i);
			if (item3)
			{
				string value = Utils.nestedEscapePattern.Match(_input.Substring(i)).Value;
				value.Replace("[/c]", "");
				stringBuilder.Append(value);
				stringBuilder.Append(_input, value.Length, item2 - value.Length);
			}
			i = item + item2;
		}
		return stringBuilder.ToString();
	}

	public static string CreateGameMessage(string _senderName, string _message)
	{
		if (string.IsNullOrEmpty(_senderName))
		{
			return _message;
		}
		return _senderName + ": " + _message;
	}

	public static string GetCancellationMessage()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			return Localization.Get("msgConnectingToServerCancel", false, null);
		}
		return string.Format(Localization.Get("msgConnectingToServerCancelTemplate", false, null), LocalPlayerUI.primaryUI.playerInput.PermanentActions.Cancel.GetBindingString(true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
	}

	public static void Fill<T>(this T[] array, T value)
	{
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			array[i] = value;
		}
	}

	public static void Memset(byte[] array, byte what, int length)
	{
		GCHandle gchandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		Utils.MemsetDelegate(gchandle.AddrOfPinnedObject(), what, length);
		gchandle.Free();
	}

	public static void CalculateMeshTangents(List<Vector3> _vertices, List<int> _indices, List<Vector3> _normals, List<Vector2> _uvs, List<Vector4> _tangents, Mesh mesh, bool _bIgnoreUvs = false)
	{
		int count = _vertices.Count;
		if (count > 786432)
		{
			return;
		}
		Array.Clear(Utils.tan1Temp, 0, count);
		Array.Clear(Utils.tan2Temp, 0, count);
		int count2 = _indices.Count;
		for (int i = 0; i < count2; i += 3)
		{
			int num = _indices[i];
			int num2 = _indices[i + 1];
			int num3 = _indices[i + 2];
			Vector3 vector = _vertices[num];
			Vector3 vector2 = _vertices[num2];
			Vector3 vector3 = _vertices[num3];
			Vector2 vector4;
			Vector2 vector5;
			Vector2 vector6;
			if (_bIgnoreUvs)
			{
				vector4 = Vector2.zero;
				vector5 = Vector2.zero;
				vector6 = Vector2.zero;
			}
			else
			{
				vector4 = _uvs[num];
				vector5 = _uvs[num2];
				vector6 = _uvs[num3];
			}
			float num4 = vector2.x - vector.x;
			float num5 = vector3.x - vector.x;
			float num6 = vector2.y - vector.y;
			float num7 = vector3.y - vector.y;
			float num8 = vector2.z - vector.z;
			float num9 = vector3.z - vector.z;
			float num10 = vector5.x - vector4.x;
			float num11 = vector6.x - vector4.x;
			float num12 = vector5.y - vector4.y;
			float num13 = vector6.y - vector4.y;
			float num14 = num10 * num13 - num11 * num12;
			float num15 = ((num14 == 0f) ? 0f : (1f / num14));
			float num16 = (num13 * num4 - num12 * num5) * num15;
			float num17 = (num13 * num6 - num12 * num7) * num15;
			float num18 = (num13 * num8 - num12 * num9) * num15;
			float num19 = (num10 * num5 - num11 * num4) * num15;
			float num20 = (num10 * num7 - num11 * num6) * num15;
			float num21 = (num10 * num9 - num11 * num8) * num15;
			Vector3[] array = Utils.tan1Temp;
			int num22 = num;
			array[num22].x = array[num22].x + num16;
			Vector3[] array2 = Utils.tan1Temp;
			int num23 = num;
			array2[num23].y = array2[num23].y + num17;
			Vector3[] array3 = Utils.tan1Temp;
			int num24 = num;
			array3[num24].z = array3[num24].z + num18;
			Vector3[] array4 = Utils.tan1Temp;
			int num25 = num2;
			array4[num25].x = array4[num25].x + num16;
			Vector3[] array5 = Utils.tan1Temp;
			int num26 = num2;
			array5[num26].y = array5[num26].y + num17;
			Vector3[] array6 = Utils.tan1Temp;
			int num27 = num2;
			array6[num27].z = array6[num27].z + num18;
			Vector3[] array7 = Utils.tan1Temp;
			int num28 = num3;
			array7[num28].x = array7[num28].x + num16;
			Vector3[] array8 = Utils.tan1Temp;
			int num29 = num3;
			array8[num29].y = array8[num29].y + num17;
			Vector3[] array9 = Utils.tan1Temp;
			int num30 = num3;
			array9[num30].z = array9[num30].z + num18;
			Vector3[] array10 = Utils.tan2Temp;
			int num31 = num;
			array10[num31].x = array10[num31].x + num19;
			Vector3[] array11 = Utils.tan2Temp;
			int num32 = num;
			array11[num32].y = array11[num32].y + num20;
			Vector3[] array12 = Utils.tan2Temp;
			int num33 = num;
			array12[num33].z = array12[num33].z + num21;
			Vector3[] array13 = Utils.tan2Temp;
			int num34 = num2;
			array13[num34].x = array13[num34].x + num19;
			Vector3[] array14 = Utils.tan2Temp;
			int num35 = num2;
			array14[num35].y = array14[num35].y + num20;
			Vector3[] array15 = Utils.tan2Temp;
			int num36 = num2;
			array15[num36].z = array15[num36].z + num21;
			Vector3[] array16 = Utils.tan2Temp;
			int num37 = num3;
			array16[num37].x = array16[num37].x + num19;
			Vector3[] array17 = Utils.tan2Temp;
			int num38 = num3;
			array17[num38].y = array17[num38].y + num20;
			Vector3[] array18 = Utils.tan2Temp;
			int num39 = num3;
			array18[num39].z = array18[num39].z + num21;
		}
		if (_normals.Count == 0)
		{
			Utils.tempNormals.Clear();
			mesh.GetNormals(Utils.tempNormals);
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 vector7 = ((_normals.Count > 0) ? _normals[j] : Utils.tempNormals[j]);
			Vector3 vector8 = Utils.tan1Temp[j];
			Vector3.OrthoNormalize(ref vector7, ref vector8);
			Vector4 vector9;
			vector9..ctor(vector8.x, vector8.y, vector8.z, (Vector3.Dot(Vector3.Cross(vector7, vector8), Utils.tan2Temp[j]) < 0f) ? (-1f) : 1f);
			_tangents.Add(vector9);
		}
	}

	public static void CalculateMeshTangents(ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector3> _normals, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector4> _tangents, Mesh _mesh, bool _bIgnoreUvs = false)
	{
		int count = _vertices.Count;
		if (count > 786432)
		{
			return;
		}
		Array.Clear(Utils.tan1Temp, 0, count);
		Array.Clear(Utils.tan2Temp, 0, count);
		int count2 = _indices.Count;
		for (int i = 0; i < count2; i += 3)
		{
			int num = _indices[i];
			int num2 = _indices[i + 1];
			int num3 = _indices[i + 2];
			Vector3 vector = _vertices[num];
			Vector3 vector2 = _vertices[num2];
			Vector3 vector3 = _vertices[num3];
			Vector2 vector4;
			Vector2 vector5;
			Vector2 vector6;
			if (_bIgnoreUvs)
			{
				vector4 = Vector2.zero;
				vector5 = Vector2.zero;
				vector6 = Vector2.zero;
			}
			else
			{
				vector4 = _uvs[num];
				vector5 = _uvs[num2];
				vector6 = _uvs[num3];
			}
			float num4 = vector2.x - vector.x;
			float num5 = vector3.x - vector.x;
			float num6 = vector2.y - vector.y;
			float num7 = vector3.y - vector.y;
			float num8 = vector2.z - vector.z;
			float num9 = vector3.z - vector.z;
			float num10 = vector5.x - vector4.x;
			float num11 = vector6.x - vector4.x;
			float num12 = vector5.y - vector4.y;
			float num13 = vector6.y - vector4.y;
			float num14 = num10 * num13 - num11 * num12;
			float num15 = ((num14 == 0f) ? 0f : (1f / num14));
			float num16 = (num13 * num4 - num12 * num5) * num15;
			float num17 = (num13 * num6 - num12 * num7) * num15;
			float num18 = (num13 * num8 - num12 * num9) * num15;
			float num19 = (num10 * num5 - num11 * num4) * num15;
			float num20 = (num10 * num7 - num11 * num6) * num15;
			float num21 = (num10 * num9 - num11 * num8) * num15;
			Vector3[] array = Utils.tan1Temp;
			int num22 = num;
			array[num22].x = array[num22].x + num16;
			Vector3[] array2 = Utils.tan1Temp;
			int num23 = num;
			array2[num23].y = array2[num23].y + num17;
			Vector3[] array3 = Utils.tan1Temp;
			int num24 = num;
			array3[num24].z = array3[num24].z + num18;
			Vector3[] array4 = Utils.tan1Temp;
			int num25 = num2;
			array4[num25].x = array4[num25].x + num16;
			Vector3[] array5 = Utils.tan1Temp;
			int num26 = num2;
			array5[num26].y = array5[num26].y + num17;
			Vector3[] array6 = Utils.tan1Temp;
			int num27 = num2;
			array6[num27].z = array6[num27].z + num18;
			Vector3[] array7 = Utils.tan1Temp;
			int num28 = num3;
			array7[num28].x = array7[num28].x + num16;
			Vector3[] array8 = Utils.tan1Temp;
			int num29 = num3;
			array8[num29].y = array8[num29].y + num17;
			Vector3[] array9 = Utils.tan1Temp;
			int num30 = num3;
			array9[num30].z = array9[num30].z + num18;
			Vector3[] array10 = Utils.tan2Temp;
			int num31 = num;
			array10[num31].x = array10[num31].x + num19;
			Vector3[] array11 = Utils.tan2Temp;
			int num32 = num;
			array11[num32].y = array11[num32].y + num20;
			Vector3[] array12 = Utils.tan2Temp;
			int num33 = num;
			array12[num33].z = array12[num33].z + num21;
			Vector3[] array13 = Utils.tan2Temp;
			int num34 = num2;
			array13[num34].x = array13[num34].x + num19;
			Vector3[] array14 = Utils.tan2Temp;
			int num35 = num2;
			array14[num35].y = array14[num35].y + num20;
			Vector3[] array15 = Utils.tan2Temp;
			int num36 = num2;
			array15[num36].z = array15[num36].z + num21;
			Vector3[] array16 = Utils.tan2Temp;
			int num37 = num3;
			array16[num37].x = array16[num37].x + num19;
			Vector3[] array17 = Utils.tan2Temp;
			int num38 = num3;
			array17[num38].y = array17[num38].y + num20;
			Vector3[] array18 = Utils.tan2Temp;
			int num39 = num3;
			array18[num39].z = array18[num39].z + num21;
		}
		if (_normals.Count == 0)
		{
			Utils.tempNormals.Clear();
			_mesh.GetNormals(Utils.tempNormals);
		}
		for (int j = 0; j < count; j++)
		{
			Vector3 vector7 = ((_normals.Count > 0) ? _normals[j] : Utils.tempNormals[j]);
			Vector3 vector8 = Utils.tan1Temp[j];
			Vector3.OrthoNormalize(ref vector7, ref vector8);
			Vector4 vector9;
			vector9..ctor(vector8.x, vector8.y, vector8.z, (Vector3.Dot(Vector3.Cross(vector7, vector8), Utils.tan2Temp[j]) < 0f) ? (-1f) : 1f);
			_tangents.Add(vector9);
		}
	}

	public static void CalculateMeshTangents(VoxelMesh _vm, bool _bIgnoreUvs = false)
	{
		int count = _vm.m_Vertices.Count;
		if (count > 786432)
		{
			return;
		}
		Array.Clear(Utils.tan1Temp, 0, count);
		Array.Clear(Utils.tan2Temp, 0, count);
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		Vector2 vector3 = Vector2.zero;
		int count2 = _vm.m_Indices.Count;
		for (int i = 0; i < count2; i += 3)
		{
			int num = _vm.m_Indices[i];
			int num2 = _vm.m_Indices[i + 1];
			int num3 = _vm.m_Indices[i + 2];
			Vector3 vector4 = _vm.m_Vertices[num];
			Vector3 vector5 = _vm.m_Vertices[num2];
			Vector3 vector6 = _vm.m_Vertices[num3];
			if (!_bIgnoreUvs)
			{
				vector = _vm.m_Uvs[num];
				vector2 = _vm.m_Uvs[num2];
				vector3 = _vm.m_Uvs[num3];
			}
			float num4 = vector5.x - vector4.x;
			float num5 = vector6.x - vector4.x;
			float num6 = vector5.y - vector4.y;
			float num7 = vector6.y - vector4.y;
			float num8 = vector5.z - vector4.z;
			float num9 = vector6.z - vector4.z;
			float num10 = vector2.x - vector.x;
			float num11 = vector3.x - vector.x;
			float num12 = vector2.y - vector.y;
			float num13 = vector3.y - vector.y;
			float num14 = num10 * num13 - num11 * num12;
			float num15 = ((num14 == 0f) ? 0f : (1f / num14));
			float num16 = (num13 * num4 - num12 * num5) * num15;
			float num17 = (num13 * num6 - num12 * num7) * num15;
			float num18 = (num13 * num8 - num12 * num9) * num15;
			float num19 = (num10 * num5 - num11 * num4) * num15;
			float num20 = (num10 * num7 - num11 * num6) * num15;
			float num21 = (num10 * num9 - num11 * num8) * num15;
			Vector3[] array = Utils.tan1Temp;
			int num22 = num;
			array[num22].x = array[num22].x + num16;
			Vector3[] array2 = Utils.tan1Temp;
			int num23 = num;
			array2[num23].y = array2[num23].y + num17;
			Vector3[] array3 = Utils.tan1Temp;
			int num24 = num;
			array3[num24].z = array3[num24].z + num18;
			Vector3[] array4 = Utils.tan1Temp;
			int num25 = num2;
			array4[num25].x = array4[num25].x + num16;
			Vector3[] array5 = Utils.tan1Temp;
			int num26 = num2;
			array5[num26].y = array5[num26].y + num17;
			Vector3[] array6 = Utils.tan1Temp;
			int num27 = num2;
			array6[num27].z = array6[num27].z + num18;
			Vector3[] array7 = Utils.tan1Temp;
			int num28 = num3;
			array7[num28].x = array7[num28].x + num16;
			Vector3[] array8 = Utils.tan1Temp;
			int num29 = num3;
			array8[num29].y = array8[num29].y + num17;
			Vector3[] array9 = Utils.tan1Temp;
			int num30 = num3;
			array9[num30].z = array9[num30].z + num18;
			Vector3[] array10 = Utils.tan2Temp;
			int num31 = num;
			array10[num31].x = array10[num31].x + num19;
			Vector3[] array11 = Utils.tan2Temp;
			int num32 = num;
			array11[num32].y = array11[num32].y + num20;
			Vector3[] array12 = Utils.tan2Temp;
			int num33 = num;
			array12[num33].z = array12[num33].z + num21;
			Vector3[] array13 = Utils.tan2Temp;
			int num34 = num2;
			array13[num34].x = array13[num34].x + num19;
			Vector3[] array14 = Utils.tan2Temp;
			int num35 = num2;
			array14[num35].y = array14[num35].y + num20;
			Vector3[] array15 = Utils.tan2Temp;
			int num36 = num2;
			array15[num36].z = array15[num36].z + num21;
			Vector3[] array16 = Utils.tan2Temp;
			int num37 = num3;
			array16[num37].x = array16[num37].x + num19;
			Vector3[] array17 = Utils.tan2Temp;
			int num38 = num3;
			array17[num38].y = array17[num38].y + num20;
			Vector3[] array18 = Utils.tan2Temp;
			int num39 = num3;
			array18[num39].z = array18[num39].z + num21;
		}
		Vector3[] items = _vm.m_Normals.Items;
		int num40 = _vm.m_Tangents.Alloc(count);
		Vector4[] items2 = _vm.m_Tangents.Items;
		for (int j = 0; j < count; j++)
		{
			Vector3 vector7 = items[j];
			Vector3 vector8 = Utils.tan1Temp[j];
			Vector3.OrthoNormalize(ref vector7, ref vector8);
			Vector4 vector9;
			vector9.x = vector8.x;
			vector9.y = vector8.y;
			vector9.z = vector8.z;
			vector9.w = (float)((Vector3.Dot(Vector3.Cross(vector7, vector8), Utils.tan2Temp[j]) < 0f) ? (-1) : 1);
			items2[num40++] = vector9;
		}
	}

	public unsafe static void CalculateMeshTangents(ReadOnlySpan<Vector3> _vertices, ReadOnlySpan<int> _indices, ReadOnlySpan<Vector3> _normals, ReadOnlySpan<Vector2> _uvs, Span<Vector4> _tangents, bool _bIgnoreUvs = false)
	{
		int length = _vertices.Length;
		if (length > 786432)
		{
			return;
		}
		NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(length, 3, 1);
		try
		{
			NativeArray<Vector3> nativeArray2 = new NativeArray<Vector3>(length, 3, 1);
			try
			{
				Span<Vector3> span = ref nativeArray;
				Span<Vector3> span2 = ref nativeArray2;
				int length2 = _indices.Length;
				for (int i = 0; i < length2; i += 3)
				{
					int num = *_indices[i];
					int num2 = *_indices[i + 1];
					int num3 = *_indices[i + 2];
					Vector3 vector = *_vertices[num];
					Vector3 vector2 = *_vertices[num2];
					Vector3 vector3 = *_vertices[num3];
					Vector2 vector4;
					Vector2 vector5;
					Vector2 vector6;
					if (_bIgnoreUvs)
					{
						vector4 = Vector2.zero;
						vector5 = Vector2.zero;
						vector6 = Vector2.zero;
					}
					else
					{
						vector4 = *_uvs[num];
						vector5 = *_uvs[num2];
						vector6 = *_uvs[num3];
					}
					float num4 = vector2.x - vector.x;
					float num5 = vector3.x - vector.x;
					float num6 = vector2.y - vector.y;
					float num7 = vector3.y - vector.y;
					float num8 = vector2.z - vector.z;
					float num9 = vector3.z - vector.z;
					float num10 = vector5.x - vector4.x;
					float num11 = vector6.x - vector4.x;
					float num12 = vector5.y - vector4.y;
					float num13 = vector6.y - vector4.y;
					float num14 = num10 * num13 - num11 * num12;
					float num15 = ((num14 == 0f) ? 0f : (1f / num14));
					float num16 = (num13 * num4 - num12 * num5) * num15;
					float num17 = (num13 * num6 - num12 * num7) * num15;
					float num18 = (num13 * num8 - num12 * num9) * num15;
					float num19 = (num10 * num5 - num11 * num4) * num15;
					float num20 = (num10 * num7 - num11 * num6) * num15;
					float num21 = (num10 * num9 - num11 * num8) * num15;
					span[num].x += num16;
					span[num].y += num17;
					span[num].z += num18;
					span[num2].x += num16;
					span[num2].y += num17;
					span[num2].z += num18;
					span[num3].x += num16;
					span[num3].y += num17;
					span[num3].z += num18;
					span2[num].x += num19;
					span2[num].y += num20;
					span2[num].z += num21;
					span2[num2].x += num19;
					span2[num2].y += num20;
					span2[num2].z += num21;
					span2[num3].x += num19;
					span2[num3].y += num20;
					span2[num3].z += num21;
				}
				for (int j = 0; j < length; j++)
				{
					Vector3 vector7 = *_normals[j];
					Vector3 vector8 = *span[j];
					Vector3.OrthoNormalize(ref vector7, ref vector8);
					Vector4 vector9;
					vector9..ctor(vector8.x, vector8.y, vector8.z, (Vector3.Dot(Vector3.Cross(vector7, vector8), *span2[j]) < 0f) ? (-1f) : 1f);
					*_tangents[j] = vector9;
				}
			}
			finally
			{
				nativeArray2.Dispose();
			}
		}
		finally
		{
			nativeArray.Dispose();
		}
	}

	public static int WrapInt(int value, int min, int max)
	{
		int num = max - min;
		while (value < min)
		{
			value += num;
		}
		while (value > max)
		{
			value -= num;
		}
		return value;
	}

	public static int WrapIndex(int index, int arraySize)
	{
		while (index < 0)
		{
			index += arraySize;
		}
		while (index >= arraySize)
		{
			index -= arraySize;
		}
		return index;
	}

	public static float WrapFloat(float value, float min, float max)
	{
		float num = max - min;
		if (value < min)
		{
			value += num;
		}
		else if (value > max)
		{
			value -= num;
		}
		return value;
	}

	public static void CleanupMaterialsOfRenderers<T>(T renderers) where T : IList<Renderer>
	{
		for (int i = 0; i < renderers.Count; i++)
		{
			renderers[i].GetSharedMaterials(Utils.tempMats);
			Utils.CleanupMaterials<List<Material>>(Utils.tempMats);
		}
		Utils.tempMats.Clear();
	}

	public static void CleanupMaterialsOfRenderer(Renderer renderer)
	{
		Utils.CleanupMaterials<Material[]>(renderer.sharedMaterials);
	}

	public static void CleanupMaterials<T>(T mats) where T : IList<Material>
	{
		for (int i = 0; i < mats.Count; i++)
		{
			Material material = mats[i];
			if (material && material.GetInstanceID() < 0)
			{
				Object.Destroy(material);
			}
		}
	}

	public static void MarkMaterialAsSafeForManualCleanup(Material mat)
	{
		mat.name += " (Instance)";
	}

	public static void RestartGame(Utils.ERestartAntiCheatMode _antiCheatMode = Utils.ERestartAntiCheatMode.KeepAntiCheatMode)
	{
		Regex regex = new Regex("^(.*)(output_log_client__)(\\d{4}-\\d{2}-\\d{2}__\\d{2}-\\d{2}-\\d{2})(\\.txt)$");
		Regex regex2 = new Regex("^(.*)(output_log__)(\\d{4}-\\d{2}-\\d{2}__\\d{2}-\\d{2}-\\d{2})(\\.txt)$", RegexOptions.IgnoreCase);
		string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
		string text = commandLineArgs[0];
		bool flag;
		if (_antiCheatMode != Utils.ERestartAntiCheatMode.ForceOn)
		{
			if (_antiCheatMode == Utils.ERestartAntiCheatMode.KeepAntiCheatMode)
			{
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				flag = antiCheatClient != null && antiCheatClient.ClientAntiCheatEnabled();
			}
			else
			{
				flag = false;
			}
		}
		else
		{
			flag = true;
		}
		bool flag2 = flag;
		if (flag2)
		{
			RuntimePlatform platform = Application.platform;
			if (platform > 2)
			{
				if (platform != 13)
				{
					switch (platform)
					{
					case 43:
						break;
					case 44:
						goto IL_008C;
					case 45:
						goto IL_00EA;
					default:
						goto IL_0119;
					}
				}
				text = Path.GetDirectoryName(text);
				text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC";
				goto IL_0133;
			}
			if (platform == 1)
			{
				goto IL_00EA;
			}
			if (platform != 2)
			{
				goto IL_0119;
			}
			IL_008C:
			text = Path.GetDirectoryName(text);
			text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC.exe";
			goto IL_0133;
			IL_00EA:
			text = Path.GetDirectoryName(text);
			text = ((text.Length > 0) ? (text + "/") : "") + "7DaysToDie_EAC";
			goto IL_0133;
			IL_0119:
			Log.Error(string.Format("Restarting the game not supported on this platform ({0})", Application.platform));
			return;
		}
		IL_0133:
		StringBuilder stringBuilder = new StringBuilder();
		if (!flag2)
		{
			stringBuilder.Append("-noeac");
		}
		for (int i = 1; i < commandLineArgs.Length; i++)
		{
			if (!flag2 || i > 1)
			{
				stringBuilder.Append(' ');
			}
			string text2 = commandLineArgs[i];
			if (text2.EqualsCaseInsensitive("-logfile"))
			{
				stringBuilder.Append(text2);
				stringBuilder.Append(' ');
				i++;
				text2 = commandLineArgs[i];
				Match match;
				if ((match = regex.Match(text2)).Success)
				{
					string text3 = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss");
					text2 = match.Groups[1].Value + match.Groups[2].Value + text3 + match.Groups[4].Value;
				}
				else if ((match = regex2.Match(text2)).Success)
				{
					string text4 = DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss");
					text2 = match.Groups[1].Value + match.Groups[2].Value + text4 + match.Groups[4].Value;
				}
				else
				{
					text2 += ".restarted.txt";
				}
			}
			if (text2.IndexOf(' ') >= 0)
			{
				stringBuilder.Append('"');
				stringBuilder.Append(text2);
				stringBuilder.Append('"');
			}
			else
			{
				stringBuilder.Append(text2);
			}
		}
		stringBuilder.Append(" -skipintro");
		stringBuilder.Append(" -skipnewsscreen=true");
		Process.Start(new ProcessStartInfo(text)
		{
			UseShellExecute = true,
			WorkingDirectory = SdDirectory.GetCurrentDirectory(),
			Arguments = stringBuilder.ToString()
		});
		Log.Out("Restarting game: " + text + " " + stringBuilder.ToString());
		Application.Quit();
	}

	public unsafe static void GetBytes(int _value, byte[] _target, int _targetOffset = 0)
	{
		Utils.GetBytes((byte*)(&_value), 4, _target, _targetOffset);
	}

	public unsafe static void GetBytes(uint _value, byte[] _target, int _targetOffset = 0)
	{
		Utils.GetBytes((byte*)(&_value), 4, _target, _targetOffset);
	}

	public unsafe static void GetBytes(long _value, byte[] _target, int _targetOffset = 0)
	{
		Utils.GetBytes((byte*)(&_value), 8, _target, _targetOffset);
	}

	public unsafe static void GetBytes(byte* _ptr, int _count, byte[] _target, int _targetOffset = 0)
	{
		for (int i = 0; i < _count; i++)
		{
			_target[_targetOffset + i] = _ptr[i];
		}
	}

	public static string MaskIp(string _input)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return _input;
		}
		StringBuilder stringBuilder = new StringBuilder(_input);
		int i;
		if ((i = _input.IndexOfAny(Utils.ipSeparatorChars)) < 0)
		{
			return _input;
		}
		while (i >= 0)
		{
			if (i > 0)
			{
				stringBuilder[i - 1] = '*';
			}
			i = _input.IndexOfAny(Utils.ipSeparatorChars, i + 1);
		}
		stringBuilder[_input.Length - 1] = '*';
		return stringBuilder.ToString();
	}

	public static void ForceMaterialsInstance(GameObject go)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
		}
	}

	public static bool IsValidWebUrl(ref string _input)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return false;
		}
		if (_input.StartsWith("http://") || _input.StartsWith("https://"))
		{
			return true;
		}
		if (_input.Contains("://"))
		{
			return false;
		}
		_input = "http://" + _input;
		return true;
	}

	public static bool OpenSystemBrowser(string _url)
	{
		if (!Utils.IsValidWebUrl(ref _url))
		{
			return false;
		}
		Application.OpenURL(_url);
		return true;
	}

	public static bool ArrayEquals(byte[] _a, byte[] _b)
	{
		if (_a == _b)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		for (int i = 0; i < _a.Length; i++)
		{
			if (_a[i] != _b[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool ArrayEquals(int[] _a, int[] _b)
	{
		if (_a == _b)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		for (int i = 0; i < _a.Length; i++)
		{
			if (_a[i] != _b[i])
			{
				return false;
			}
		}
		return true;
	}

	public static string GenerateGuid()
	{
		byte[] array = Guid.NewGuid().ToByteArray();
		StringBuilder stringBuilder = new StringBuilder(32);
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	public static uint CurrentUnixTime
	{
		get
		{
			return (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}
	}

	public static string[] ExcludeLayerZoom = new string[] { "VisibleOnZoom" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static CultureInfo ci;

	public static readonly Regex hexAlphaMatcher = new Regex("[0-9a-fA-F]{2}");

	public static readonly Regex hexRgbMatcher = new Regex("[0-9a-fA-F]{6}");

	public static readonly Regex hexRgbaMatcher = new Regex("[0-9a-fA-F]{8}");

	public static readonly Regex nestedEscapePattern = new Regex("(\\[+\\/c\\](?:\\/c\\])*)");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Transform> setLayerRecursivelyList = new List<Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Collider> setColliderLayerRecursivelyList = new List<Collider>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Action<IntPtr, byte, int> MemsetDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3[] tan1Temp = new Vector3[786432];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3[] tan2Temp = new Vector3[786432];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector3> tempNormals = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> tempMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] ipSeparatorChars = new char[] { '.', ':' };

	public enum EnumHitDirection : byte
	{
		Front,
		Back,
		Left,
		Right,
		Explosion,
		None
	}

	public enum ERestartAntiCheatMode
	{
		KeepAntiCheatMode,
		ForceOff,
		ForceOn
	}
}
