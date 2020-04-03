using System;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace UnityEngine
{
    public struct Vector3 : IEquatable<Vector3>
	{
		public float x;
		public float y;
		public float z;
		
		#region "Construction and field access"
		
		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3(float x, float y)
		{
			this.x = x;
			this.y = y;
			z = 0f;
		}

		public void Set(float newX, float newY, float newZ)
		{
			x = newX;
			y = newY;
			z = newZ;
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
						x = value;
						break;
					
					case 1:
						y = value;
						break;
					
					case 2:
						z = value;
						break;
					
					default:
						throw new IndexOutOfRangeException(
							"Invalid Vector3 index!"
						);
				}
			}
		}
		
		#endregion
		
		#region "Useful constants"
		
		private static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);
		private static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
		private static readonly Vector3 upVector = new Vector3(0f, 1f, 0f);
		private static readonly Vector3 downVector = new Vector3(0f, -1f, 0f);
		private static readonly Vector3 leftVector = new Vector3(-1f, 0f, 0f);
		private static readonly Vector3 rightVector = new Vector3(1f, 0f, 0f);
		private static readonly Vector3 forwardVector = new Vector3(0f, 0f, 1f);
		private static readonly Vector3 backVector = new Vector3(0f, 0f, -1f);
		private static readonly Vector3 positiveInfinityVector = new Vector3(
			float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity
		);
		private static readonly Vector3 negativeInfinityVector = new Vector3(
			float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity
		);

		public static Vector3 zero => zeroVector;
		public static Vector3 one => oneVector;
		public static Vector3 forward => forwardVector;
		public static Vector3 back => backVector;
		public static Vector3 up => upVector;
		public static Vector3 down => downVector;
		public static Vector3 left => leftVector;
		public static Vector3 right => rightVector;
		public static Vector3 positiveInfinity => positiveInfinityVector;
		public static Vector3 negativeInfinity => negativeInfinityVector;
		
		#endregion
		
		#region "Magnitude related stuff"
		
		public Vector3 normalized => Normalize(this);
		public float magnitude => (float)Math.Sqrt(sqrMagnitude);
		public float sqrMagnitude => x * x + y * y + z * z;
		
		public static float Magnitude(Vector3 vector)
			=> vector.magnitude;

		public static float SqrMagnitude(Vector3 vector)
			=> vector.sqrMagnitude;
		
		public static Vector3 Normalize(Vector3 value)
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
		
		public static Vector3 Scale(Vector3 u, Vector3 v)
		{
			return new Vector3(u.x * v.x, u.y * v.y, u.z * v.z);
		}

		public void Scale(Vector3 s)
		{
			x *= s.x;
			y *= s.y;
			z *= s.z;
		}
		
		public static float Dot(Vector3 u, Vector3 v)
		{
			return u.x * v.x + u.y * v.y + u.z * v.z;
		}

		public static Vector3 Cross(Vector3 u, Vector3 v)
		{
			return new Vector3(
				u.y * v.z - u.z * v.y,
				u.z * v.x - u.x * v.z,
				u.x * v.y - u.y * v.x
			);
		}
		
		#endregion

		#region "Equality and hashing"
		
		public override int GetHashCode()
		{
			return x.GetHashCode()
			       ^ (y.GetHashCode() << 2)
			       ^ (z.GetHashCode() >> 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is Vector3))
				return false;
			
			return Equals((Vector3)other);
		}

		public bool Equals(Vector3 other)
		{
			return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
		}
		
		#endregion
		
		#region "Operators"
		
		public static Vector3 operator +(Vector3 u, Vector3 v)
		{
			return new Vector3(u.x + v.x, u.y + v.y, u.z + v.z);
		}

		public static Vector3 operator -(Vector3 u, Vector3 v)
		{
			return new Vector3(u.x - v.x, u.y - v.y, u.z - v.z);
		}

		public static Vector3 operator -(Vector3 v)
		{
			return new Vector3(0f - v.x, 0f - v.y, 0f - v.z);
		}

		public static Vector3 operator *(Vector3 v, float s)
		{
			return new Vector3(v.x * s, v.y * s, v.z * s);
		}

		public static Vector3 operator *(float s, Vector3 v)
		{
			return new Vector3(v.x * s, v.y * s, v.z * s);
		}

		public static Vector3 operator /(Vector3 a, float d)
		{
			return new Vector3(a.x / d, a.y / d, a.z / d);
		}

		public static bool operator ==(Vector3 u, Vector3 v)
		{
			return (u - v).sqrMagnitude < 9.99999944E-11f;
		}

		public static bool operator !=(Vector3 u, Vector3 v)
		{
			return !(u == v);
		}
		
		#endregion
		
		public override string ToString()
		{
			return $"({x:F1}, {y:F1}, {z:F1})";
		}

		public string ToString(string format)
		{
			return $"({x.ToString(format)}, " +
			       $"{y.ToString(format)}, " +
			       $"{z.ToString(format)})";
		}
	}
}