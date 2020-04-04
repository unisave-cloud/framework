using System;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace UnityEngine
{
    public struct Color : IEquatable<Color>
	{
		public float r;
		public float g;
		public float b;
		public float a;
		
		public float grayscale => 0.299f * r + 0.587f * g + 0.114f * b;
		public float maxColorComponent => Mathf.Max(Mathf.Max(r, g), b);
		
		#region "Construction and field access"
		
		public Color(float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			a = 1f;
		}
		
		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return r;
					
					case 1:
						return g;
					
					case 2:
						return b;
					
					case 3:
						return a;
					
					default:
						throw new IndexOutOfRangeException(
							"Invalid Vector3 index!"
						);
				}
			}
			
			set
			{
				switch (index)
				{
					case 0:
						r = value;
						break;
					
					case 1:
						g = value;
						break;
					
					case 2:
						b = value;
						break;
					
					case 3:
						a = value;
						break;
					
					default:
						throw new IndexOutOfRangeException(
							"Invalid Vector3 index!"
						);
				}
			}
		}
		
		#endregion

		#region "Color constants"
		
		public static Color red => new Color(1f, 0f, 0f, 1f);
		public static Color green => new Color(0f, 1f, 0f, 1f);
		public static Color blue => new Color(0f, 0f, 1f, 1f);
		public static Color white => new Color(1f, 1f, 1f, 1f);
		public static Color black => new Color(0f, 0f, 0f, 1f);
		public static Color yellow => new Color(1f, 47f / 51f, 4f / 255f, 1f);
		public static Color cyan => new Color(0f, 1f, 1f, 1f);
		public static Color magenta => new Color(1f, 0f, 1f, 1f);
		public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);
		public static Color grey => new Color(0.5f, 0.5f, 0.5f, 1f);
		public static Color clear => new Color(0f, 0f, 0f, 0f);
		
		#endregion

		#region "Basic operations"
		
		public static Color Lerp(Color a, Color b, float t)
		{
			return LerpUnclamped(a, b, Mathf.Clamp01(t));
		}

		public static Color LerpUnclamped(Color a, Color b, float t)
		{
			return new Color(
				a.r + (b.r - a.r) * t,
				a.g + (b.g - a.g) * t,
				a.b + (b.b - a.b) * t,
				a.a + (b.a - a.a) * t
			);
		}
		
		#endregion
		
		#region "Equality and hashing"

		public override int GetHashCode()
		{
			return ((Vector4)this).GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (!(other is Color))
				return false;
			
			return Equals((Color)other);
		}

		public bool Equals(Color other)
		{
			return r.Equals(other.r)
			       && g.Equals(other.g)
			       && b.Equals(other.b)
			       && a.Equals(other.a);
		}
		
		#endregion
		
		#region "Operators"

		public static Color operator +(Color a, Color b)
		{
			return new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
		}

		public static Color operator -(Color a, Color b)
		{
			return new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
		}

		public static Color operator *(Color a, Color b)
		{
			return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
		}

		public static Color operator *(Color a, float b)
		{
			return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
		}

		public static Color operator *(float b, Color a)
		{
			return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
		}

		public static Color operator /(Color a, float b)
		{
			return new Color(a.r / b, a.g / b, a.b / b, a.a / b);
		}

		public static bool operator ==(Color lhs, Color rhs)
		{
			return (Vector4)lhs == (Vector4)rhs;
		}

		public static bool operator !=(Color lhs, Color rhs)
		{
			return !(lhs == rhs);
		}

		public static implicit operator Vector4(Color c)
		{
			return new Vector4(c.r, c.g, c.b, c.a);
		}

		public static implicit operator Color(Vector4 v)
		{
			return new Color(v.x, v.y, v.z, v.w);
		}
		
		#endregion
		
		#region "Color space conversions"

		public static void RGBToHSV(
			Color rgbColor,
			out float H,
			out float S,
			out float V
		)
		{
			if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
			{
				RGBToHSVHelper(
					4f,
					rgbColor.b, rgbColor.r, rgbColor.g,
					out H, out S, out V)
				;
			}
			else if (rgbColor.g > rgbColor.r)
			{
				RGBToHSVHelper(
					2f,
					rgbColor.g, rgbColor.b, rgbColor.r,
					out H, out S, out V
				);
			}
			else
			{
				RGBToHSVHelper(
					0f,
					rgbColor.r, rgbColor.g, rgbColor.b,
					out H, out S, out V
				);
			}
		}

		private static void RGBToHSVHelper(
			float offset,
			float dominantcolor, float colorone, float colortwo,
			out float H, out float S, out float V
		)
		{
			V = dominantcolor;
			
			if (V != 0f)
			{
				float subdominant = colorone <= colortwo ? colorone : colortwo;
				float slack = V - subdominant;
				
				if (slack != 0f)
				{
					S = slack / V;
					H = offset + (colorone - colortwo) / slack;
				}
				else
				{
					S = 0f;
					H = offset + (colorone - colortwo);
				}
				
				H /= 6f;
				
				if (H < 0f)
					H += 1f;
			}
			else
			{
				S = 0f;
				H = 0f;
			}
		}

		public static Color HSVToRGB(float H, float S, float V)
		{
			return HSVToRGB(H, S, V, hdr: true);
		}

		public static Color HSVToRGB(float H, float S, float V, bool hdr)
		{
			// ReSharper disable once LocalVariableHidesMember
			Color white = Color.white;
			
			if (S == 0f)
			{
				white.r = V;
				white.g = V;
				white.b = V;
			}
			else if (V == 0f)
			{
				white.r = 0f;
				white.g = 0f;
				white.b = 0f;
			}
			else
			{
				white.r = 0f;
				white.g = 0f;
				white.b = 0f;
				
				float h6 = H * 6f;
				int h6int = (int)Mathf.Floor(h6);
				float fractional = h6 - h6int;
				float x = V * (1f - S);
				float y = V * (1f - S * fractional);
				float z = V * (1f - S * (1f - fractional));
				
				switch (h6int)
				{
					case 0:
						white.r = V;
						white.g = z;
						white.b = x;
						break;
					
					case 1:
						white.r = y;
						white.g = V;
						white.b = x;
						break;
					
					case 2:
						white.r = x;
						white.g = V;
						white.b = z;
						break;
					
					case 3:
						white.r = x;
						white.g = y;
						white.b = V;
						break;
					
					case 4:
						white.r = z;
						white.g = x;
						white.b = V;
						break;
					
					case 5:
						white.r = V;
						white.g = x;
						white.b = y;
						break;
					
					case 6:
						white.r = V;
						white.g = z;
						white.b = x;
						break;
					
					case -1:
						white.r = V;
						white.g = x;
						white.b = y;
						break;
				}
				
				if (!hdr)
				{
					white.r = Mathf.Clamp(white.r, 0f, 1f);
					white.g = Mathf.Clamp(white.g, 0f, 1f);
					white.b = Mathf.Clamp(white.b, 0f, 1f);
				}
			}
			return white;
		}
		
		#endregion
		
		public override string ToString()
		{
			return $"RGBA({r:F3}, {g:F3}, {b:F3}, {a:F3})";
		}

		public string ToString(string format)
		{
			return $"RGBA({r.ToString(format)}, {g.ToString(format)}, " +
			       $"{b.ToString(format)}, {a.ToString(format)})";
		}
	}
}