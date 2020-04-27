// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace UnityEngine
{
    public struct Color32
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public Color32(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static implicit operator Color32(Color c)
        {
            return new Color32(
                (byte)(Mathf.Clamp01(c.r) * 255f),
                (byte)(Mathf.Clamp01(c.g) * 255f),
                (byte)(Mathf.Clamp01(c.b) * 255f),
                (byte)(Mathf.Clamp01(c.a) * 255f)
            );
        }

        public static implicit operator Color(Color32 c)
        {
            return new Color(
                c.r / 255f,
                c.g / 255f,
                c.b / 255f,
                c.a / 255f
            );
        }

        public static Color32 Lerp(Color32 a, Color32 b, float t)
            => LerpUnclamped(a, b, Mathf.Clamp01(t));

        public static Color32 LerpUnclamped(Color32 a, Color32 b, float t)
        {
            return new Color32(
                (byte)(a.r + (b.r - a.r) * t),
                (byte)(a.g + (b.g - a.g) * t),
                (byte)(a.b + (b.b - a.b) * t),
                (byte)(a.a + (b.a - a.a) * t)
            );
        }

        public override string ToString()
        {
            return $"RGBA({r}, {g}, {b}, {a})";
        }

        public string ToString(string format)
        {
            return $"RGBA({r.ToString(format)}, {g.ToString(format)}, " +
                   $"{b.ToString(format)}, {a.ToString(format)})";
        }
    }
}