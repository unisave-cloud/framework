using System;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace UnityEngine
{
    public struct Vector2 : IEquatable<Vector2>
	{
		public float x;
		public float y;
		
		#region "Construction and field access"
		
		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public void Set(float newX, float newY)
		{
			x = newX;
			y = newY;
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
					
					default:
						throw new IndexOutOfRangeException(
							"Invalid Vector2 index!"
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
					
					default:
						throw new IndexOutOfRangeException(
							"Invalid Vector2 index!"
						);
				}
			}
		}
		
		#endregion

		#region "Useful constants"
		
		public static Vector2 zero => new Vector2(0f, 0f);
		public static Vector2 one => new Vector2(1f, 1f);
		public static Vector2 up => new Vector2(0f, 1f);
		public static Vector2 down => new Vector2(0f, -1f);
		public static Vector2 left => new Vector2(-1f, 0f);
		public static Vector2 right => new Vector2(1f, 0f);
		public static Vector2 positiveInfinity => new Vector2(
			float.PositiveInfinity, float.PositiveInfinity
		);
		public static Vector2 negativeInfinity => new Vector2(
			float.NegativeInfinity, float.NegativeInfinity
		);
		
		#endregion
		
		#region "Magnitude related stuff"
		
		public Vector2 normalized => Normalize(this);
		public float magnitude => Mathf.Sqrt(sqrMagnitude);
		public float sqrMagnitude => x * x + y * y;

		public static float Magnitude(Vector2 vector)
			=> vector.magnitude;
		
		public static float SqrMagnitude(Vector2 vector)
			=> vector.sqrMagnitude;

		public float SqrMagnitude()
			=> sqrMagnitude;

		public static Vector2 Normalize(Vector2 value)
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
		
		public static Vector2 Scale(Vector2 u, Vector2 v)
		{
			return new Vector2(u.x * v.x, u.y * v.y);
		}

		public void Scale(Vector2 s)
		{
			x *= s.x;
			y *= s.y;
		}
		
		public static float Dot(Vector2 u, Vector2 v)
		{
			return u.x * v.x + u.y * v.y;
		}

		#endregion

		#region "Equality and hashing"

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ (y.GetHashCode() << 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is Vector2))
				return false;
			
			return Equals((Vector2)other);
		}

		public bool Equals(Vector2 other)
		{
			return x.Equals(other.x) && y.Equals(other.y);
		}

		#endregion

		#region "Operators"

		public static Vector2 operator +(Vector2 u, Vector2 v)
		{
			return new Vector2(u.x + v.x, u.y + v.y);
		}

		public static Vector2 operator -(Vector2 u, Vector2 v)
		{
			return new Vector2(u.x - v.x, u.y - v.y);
		}

		public static Vector2 operator *(Vector2 u, Vector2 v)
		{
			return new Vector2(u.x * v.x, u.y * v.y);
		}

		public static Vector2 operator /(Vector2 u, Vector2 v)
		{
			return new Vector2(u.x / v.x, u.y / v.y);
		}

		public static Vector2 operator -(Vector2 v)
		{
			return new Vector2(0f - v.x, 0f - v.y);
		}

		public static Vector2 operator *(Vector2 v, float s)
		{
			return new Vector2(v.x * s, v.y * s);
		}

		public static Vector2 operator *(float s, Vector2 v)
		{
			return new Vector2(v.x * s, v.y * s);
		}

		public static Vector2 operator /(Vector2 v, float s)
		{
			return new Vector2(v.x / s, v.y / s);
		}

		public static bool operator ==(Vector2 u, Vector2 v)
		{
			return (u - v).sqrMagnitude < 9.99999944E-11f;
		}

		public static bool operator !=(Vector2 u, Vector2 v)
		{
			return !(u == v);
		}

		public static implicit operator Vector2(Vector3 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static implicit operator Vector3(Vector2 v)
		{
			return new Vector3(v.x, v.y, 0f);
		}

		#endregion
		
		public override string ToString()
		{
			return $"({x:F1}, {y:F1})";
		}

		public string ToString(string format)
		{
			return $"({x.ToString(format)}, {y.ToString(format)})";
		}
	}
}