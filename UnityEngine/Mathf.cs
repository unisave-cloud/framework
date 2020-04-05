using System;
using System.Linq;
// ReSharper disable once InconsistentNaming
// ReSharper disable once MemberCanBePrivate.Global

namespace UnityEngine
{
    public static class Mathf
	{
		public const float PI = (float)Math.PI;
		public const float Infinity = float.PositiveInfinity;
		public const float NegativeInfinity = float.NegativeInfinity;
		public const float Deg2Rad = PI / 180f;
		public const float Rad2Deg = 57.29578f;

		#region "System.Math redirections"
		
		public static float Sin(float f) => (float)Math.Sin(f);
		public static float Cos(float f) => (float)Math.Cos(f);
		public static float Tan(float f) => (float)Math.Tan(f);
		public static float Asin(float f) => (float)Math.Asin(f);
		public static float Acos(float f) => (float)Math.Acos(f);
		public static float Atan(float f) => (float)Math.Atan(f);
		public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
		public static float Sqrt(float f) => (float)Math.Sqrt(f);
		public static float Abs(float f) => Math.Abs(f);
		public static int Abs(int value) => Math.Abs(value);
		public static float Pow(float f, float p) => (float)Math.Pow(f, p);
		public static float Exp(float power) => (float)Math.Exp(power);
		public static float Log(float f, float p) => (float)Math.Log(f, p);
		public static float Log(float f) => (float)Math.Log(f);
		public static float Log10(float f) => (float)Math.Log10(f);
		public static float Ceil(float f) => (float)Math.Ceiling(f);
		public static float Floor(float f) => (float)Math.Floor(f);
		public static float Round(float f) => (float)Math.Round(f);
		public static int CeilToInt(float f) => (int)Math.Ceiling(f);
		public static int FloorToInt(float f) => (int)Math.Floor(f);
		public static int RoundToInt(float f) => (int)Math.Round(f);
		
		#endregion
		
		#region "Min Max"
		
		public static float Min(float a, float b) => a >= b ? b : a;
		public static int Min(int a, int b) => a >= b ? b : a;
		public static float Max(float a, float b) => a <= b ? b : a;
		public static int Max(int a, int b) => a <= b ? b : a;

		public static float Min(params float[] values)
		{
			if (values.Length == 0)
				return 0f;
			
			return values.Min();
		}

		public static int Min(params int[] values)
		{
			if (values.Length == 0)
				return 0;
			
			return values.Min();
		}

		public static float Max(params float[] values)
		{
			if (values.Length == 0)
				return 0f;
			
			return values.Max();
		}

		public static int Max(params int[] values)
		{
			if (values.Length == 0)
				return 0;
			
			return values.Max();
		}
		
		#endregion

		public static float Sign(float f) => f < 0f ? -1f : 1f;

		public static float Clamp(float value, float min, float max)
		{
			if (value < min)
				return min;
			
			if (value > max)
				return max;
			
			return value;
		}

		public static int Clamp(int value, int min, int max)
		{
			if (value < min)
				return min;
			
			if (value > max)
				return max;
			
			return value;
		}

		public static float Clamp01(float value)
			=> Clamp(value, 0, 1);

		public static float Lerp(float a, float b, float t)
			=> a + (b - a) * Clamp01(t);

		public static float LerpUnclamped(float a, float b, float t)
			=> a + (b - a) * t;
	}
}