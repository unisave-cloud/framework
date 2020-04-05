using System;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace UnityEngine
{
    public struct Vector4 : IEquatable<Vector4>
	{
		public float x;
		public float y;
		public float z;
		public float w;
		
		#region "Construction and field access"
		
		public Vector4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public Vector4(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			w = 0f;
		}

		public Vector4(float x, float y)
		{
			this.x = x;
			this.y = y;
			z = 0f;
			w = 0f;
		}
		
		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return x;
					
					case 1:
						return y;
					
					case 2:
						return z;
					
					case 3:
						return w;
					
					default:
						throw new IndexOutOfRangeException(
							"Invalid Vector4 index!"
						);
				}
			}
			
			set
			{
				switch (index)
				{
					case 0:
						x = value;
						break;
					
					case 1:
						y = value;
						break;
					
					case 2:
						z = value;
						break;
					
					case 3:
						w = value;
						break;
					
					default:
						throw new IndexOutOfRangeException(
							"Invalid Vector4 index!"
						);
				}
			}
		}

		public void Set(float newX, float newY, float newZ, float newW)
		{
			x = newX;
			y = newY;
			z = newZ;
			w = newW;
		}
		
		#endregion
		
		#region "Useful constants"

		public static Vector4 zero => new Vector4(0f, 0f, 0f, 0f);
		public static Vector4 one => new Vector4(1f, 1f, 1f, 1f);
		public static Vector4 positiveInfinity => new Vector4(
			float.PositiveInfinity, float.PositiveInfinity,
			float.PositiveInfinity, float.PositiveInfinity
		);
		public static Vector4 negativeInfinity => new Vector4(
			float.NegativeInfinity, float.NegativeInfinity,
			float.NegativeInfinity, float.NegativeInfinity
		);
		
		#endregion

		#region "Magnitude related stuff"

		public Vector4 normalized => Normalize(this);
		public float magnitude => Mathf.Sqrt(sqrMagnitude);
		public float sqrMagnitude => x * x + y * y + z * z + w * w;
		
		public static float Magnitude(Vector4 vector)
			=> vector.magnitude;
		
		public static float SqrMagnitude(Vector4 vector)
			=> vector.sqrMagnitude;

		public float SqrMagnitude()
			=> sqrMagnitude;

		public static Vector4 Normalize(Vector4 value)
		{
			float m = value.magnitude;
			
			if (m > 1E-05f)
				return value / m;
			
			return zero;
		}

		public void Normalize()
		{
			float m = magnitude;
			
			if (m > 1E-05f)
				this /= m;
			else
				this = zero;
		}
		
		#endregion
		
		#region "Basic operations"

		public static Vector4 Scale(Vector4 u, Vector4 v)
		{
			return new Vector4(u.x * v.x, u.y * v.y, u.z * v.z, u.w * v.w);
		}

		public void Scale(Vector4 s)
		{
			x *= s.x;
			y *= s.y;
			z *= s.z;
			w *= s.w;
		}
		
		public static float Dot(Vector4 u, Vector4 v)
		{
			return u.x * v.x + u.y * v.y + u.z * v.z + u.w * v.w;
		}
		
		#endregion
		
		#region "Equality and hashing"

		public override int GetHashCode()
		{
			return x.GetHashCode()
			       ^ (y.GetHashCode() << 2)
			       ^ (z.GetHashCode() >> 2)
			       ^ (w.GetHashCode() >> 1);
		}

		public override bool Equals(object other)
		{
			if (!(other is Vector4))
				return false;
			
			return Equals((Vector4)other);
		}

		public bool Equals(Vector4 other)
		{
			return x.Equals(other.x)
			       && y.Equals(other.y)
			       && z.Equals(other.z)
			       && w.Equals(other.w);
		}
		
		#endregion

		#region "Operators"
		
		public static Vector4 operator +(Vector4 u, Vector4 v)
		{
			return new Vector4(u.x + v.x, u.y + v.y, u.z + v.z, u.w + v.w);
		}

		public static Vector4 operator -(Vector4 u, Vector4 v)
		{
			return new Vector4(u.x - v.x, u.y - v.y, u.z - v.z, u.w - v.w);
		}

		public static Vector4 operator -(Vector4 v)
		{
			return new Vector4(0f - v.x, 0f - v.y, 0f - v.z, 0f - v.w);
		}

		public static Vector4 operator *(Vector4 v, float s)
		{
			return new Vector4(v.x * s, v.y * s, v.z * s, v.w * s);
		}

		public static Vector4 operator *(float s, Vector4 v)
		{
			return new Vector4(v.x * s, v.y * s, v.z * s, v.w * s);
		}

		public static Vector4 operator /(Vector4 v, float s)
		{
			return new Vector4(v.x / s, v.y / s, v.z / s, v.w / s);
		}

		public static bool operator ==(Vector4 u, Vector4 v)
		{
			return (u - v).sqrMagnitude < 9.99999944E-11f;
		}

		public static bool operator !=(Vector4 u, Vector4 v)
		{
			return !(u == v);
		}

		public static implicit operator Vector4(Vector3 v)
		{
			return new Vector4(v.x, v.y, v.z, 0f);
		}

		public static implicit operator Vector3(Vector4 v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static implicit operator Vector4(Vector2 v)
		{
			return new Vector4(v.x, v.y, 0f, 0f);
		}

		public static implicit operator Vector2(Vector4 v)
		{
			return new Vector2(v.x, v.y);
		}
		
		#endregion

		public override string ToString()
		{
			return $"({x:F1}, {y:F1}, {z:F1}, {w:F1})";
		}

		public string ToString(string format)
		{
			return $"({x.ToString(format)}, {y.ToString(format)}, " +
			       $"{z.ToString(format)}, {w.ToString(format)})";
		}
	}
}