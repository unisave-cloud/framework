using System;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace UnityEngine
{
    public struct Vector3Int : IEquatable<Vector3Int>
	{
		public int x { get; set; }
		public int y { get; set; }
		public int z { get; set; }

		#region "Constructor and field access"
		
		public Vector3Int(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public int this[int index]
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
							$"Invalid Vector3Int index addressed: {index}!"
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
							$"Invalid Vector3Int index addressed: {index}!"
						);
				}
			}
		}
		
		public void Set(int newX, int newY, int newZ)
		{
			x = newX;
			y = newY;
			z = newZ;
		}
		
		#endregion

		#region "Useful constants"

		public static Vector3Int zero => new Vector3Int(0, 0, 0);
		public static Vector3Int one => new Vector3Int(1, 1, 1);
		public static Vector3Int up => new Vector3Int(0, 1, 0);
		public static Vector3Int down => new Vector3Int(0, -1, 0);
		public static Vector3Int left => new Vector3Int(-1, 0, 0);
		public static Vector3Int right => new Vector3Int(1, 0, 0);

		#endregion
		
		#region "Magnitude related stuff"
		
		public float magnitude => Mathf.Sqrt(sqrMagnitude);
		public int sqrMagnitude => x * x + y * y + z * z;
		
		#endregion
		
		#region "Basic operations"

		public static Vector3Int Scale(Vector3Int a, Vector3Int b)
		{
			return new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public void Scale(Vector3Int scale)
		{
			x *= scale.x;
			y *= scale.y;
			z *= scale.z;
		}
		
		public static Vector3Int FloorToInt(Vector3 v)
		{
			return new Vector3Int(
				Mathf.FloorToInt(v.x),
				Mathf.FloorToInt(v.y),
				Mathf.FloorToInt(v.z)
			);
		}

		public static Vector3Int CeilToInt(Vector3 v)
		{
			return new Vector3Int(
				Mathf.CeilToInt(v.x),
				Mathf.CeilToInt(v.y),
				Mathf.CeilToInt(v.z)
			);
		}

		public static Vector3Int RoundToInt(Vector3 v)
		{
			return new Vector3Int(
				Mathf.RoundToInt(v.x),
				Mathf.RoundToInt(v.y),
				Mathf.RoundToInt(v.z)
			);
		}
		
		#endregion
		
		#region "Operators"

		public static implicit operator Vector3(Vector3Int v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static explicit operator Vector2Int(Vector3Int v)
		{
			return new Vector2Int(v.x, v.y);
		}

		public static Vector3Int operator +(Vector3Int a, Vector3Int b)
		{
			return new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3Int operator -(Vector3Int a, Vector3Int b)
		{
			return new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3Int operator *(Vector3Int a, Vector3Int b)
		{
			return new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static Vector3Int operator *(Vector3Int a, int b)
		{
			return new Vector3Int(a.x * b, a.y * b, a.z * b);
		}

		public static bool operator ==(Vector3Int lhs, Vector3Int rhs)
		{
			return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
		}

		public static bool operator !=(Vector3Int lhs, Vector3Int rhs)
		{
			return !(lhs == rhs);
		}
		
		#endregion
		
		#region "Equality and hashing"

		public override bool Equals(object other)
		{
			if (!(other is Vector3Int))
				return false;
			
			return Equals((Vector3Int)other);
		}

		public bool Equals(Vector3Int other)
		{
			return this == other;
		}
		
		public override int GetHashCode()
		{
			int hashCode = y.GetHashCode();
			int hashCode2 = z.GetHashCode();
			return x.GetHashCode()
			       ^ (hashCode << 4)
			       ^ (hashCode >> 28)
			       ^ (hashCode2 >> 4)
			       ^ (hashCode2 << 28);
		}
		
		#endregion

		public override string ToString()
		{
			return $"({x}, {y}, {z})";
		}

		public string ToString(string format)
		{
			return $"({x.ToString(format)}, {y.ToString(format)}, " +
			       $"{z.ToString(format)})";
		}
	}
}