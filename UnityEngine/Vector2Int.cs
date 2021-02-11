using System;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace UnityEngine
{
    public struct Vector2Int : IEquatable<Vector2Int>
	{
		private int m_X;
		private int m_Y;
		
		// ReSharper disable once ConvertToAutoProperty
		public int x
		{
			get => m_X;
			set => m_X = value;
		}

		// ReSharper disable once ConvertToAutoProperty
		public int y
		{
			get => m_Y;
			set => m_Y = value;
		}
		
		#region "Constructor and field access"
		
		public Vector2Int(int x, int y)
		{
			m_X = x;
			m_Y = y;
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
					
					default:
						throw new IndexOutOfRangeException(
							$"Invalid Vector2Int index addressed: {index}!"
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
							$"Invalid Vector2Int index addressed: {index}!"
						);
				}
			}
		}
		
		public void Set(int newX, int newY)
		{
			x = newX;
			y = newY;
		}
		
		#endregion
		
		#region "Useful constants"
		
		public static Vector2Int zero => new Vector2Int(0, 0);
		public static Vector2Int one => new Vector2Int(1, 1);
		public static Vector2Int up => new Vector2Int(0, 1);
		public static Vector2Int down => new Vector2Int(0, -1);
		public static Vector2Int left => new Vector2Int(-1, 0);
		public static Vector2Int right => new Vector2Int(1, 0);
		
		#endregion
		
		#region "Magnitude related stuff"

		public float magnitude => Mathf.Sqrt(sqrMagnitude);
		public int sqrMagnitude => x * x + y * y;
		
		#endregion
		
		#region "Basic operations"

		public static Vector2Int Scale(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x * b.x, a.y * b.y);
		}

		public void Scale(Vector2Int scale)
		{
			x *= scale.x;
			y *= scale.y;
		}
		
		public static Vector2Int FloorToInt(Vector2 v)
		{
			return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
		}

		public static Vector2Int CeilToInt(Vector2 v)
		{
			return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
		}

		public static Vector2Int RoundToInt(Vector2 v)
		{
			return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
		}
		
		#endregion
		
		#region "Operators"

		public static implicit operator Vector2(Vector2Int v)
		{
			return new Vector2(v.x, v.y);
		}

		public static explicit operator Vector3Int(Vector2Int v)
		{
			return new Vector3Int(v.x, v.y, 0);
		}

		public static Vector2Int operator +(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x + b.x, a.y + b.y);
		}

		public static Vector2Int operator -(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x - b.x, a.y - b.y);
		}

		public static Vector2Int operator *(Vector2Int a, Vector2Int b)
		{
			return new Vector2Int(a.x * b.x, a.y * b.y);
		}

		public static Vector2Int operator *(Vector2Int a, int b)
		{
			return new Vector2Int(a.x * b, a.y * b);
		}

		public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
		{
			return lhs.x == rhs.x && lhs.y == rhs.y;
		}

		public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
		{
			return !(lhs == rhs);
		}
		
		#endregion

		#region "Equality and hashing"

		public override bool Equals(object other)
		{
			if (!(other is Vector2Int))
				return false;
			
			return Equals((Vector2Int)other);
		}

		public bool Equals(Vector2Int other)
		{
			return x.Equals(other.x) && y.Equals(other.y);
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ (y.GetHashCode() << 2);
		}

		#endregion

		public override string ToString()
		{
			return $"({x}, {y})";
		}
	}
}