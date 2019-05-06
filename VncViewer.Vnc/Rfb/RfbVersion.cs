using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VncViewer.Vnc
{
    public struct RfbVersion : IEquatable<RfbVersion>, IComparable<RfbVersion>
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }

        private static readonly Regex _Regex = new Regex(@"RFB (\d{3})\.(\d{3})\n");

        public RfbVersion(int major, int minor)
        {
            if (major < 0 || major > 999) throw new ArgumentOutOfRangeException(nameof(major));
            if (minor < 0 || minor > 999) throw new ArgumentOutOfRangeException(nameof(minor));

            Major = major;
            Minor = minor;
        }

        public static RfbVersion FromString(String str)
        {
            if (String.IsNullOrEmpty(str)) throw new ArgumentException(nameof(str));

            var m = _Regex.Match(str);

            if (m.Success)
            {
                var major = m.Groups[1].ToString();
                var minor = m.Groups[2].ToString();
                return new RfbVersion(int.Parse(major, CultureInfo.InvariantCulture), int.Parse(minor, CultureInfo.InvariantCulture));
            }

            throw new ArgumentException(nameof(str));
        }

        public override string ToString() => String.Format(CultureInfo.InvariantCulture, "RFB {0:000}.{1:000}\n", Major, Minor);

        public override bool Equals(object obj)
        {
            if (!(obj is RfbVersion))
            {
                return false;
            }

            var version = (RfbVersion)obj;
            return Major == version.Major &&
                   Minor == version.Minor;
        }

        public override int GetHashCode()
        {
            var hashCode = 317314336;
            hashCode = hashCode * -1521134295 + Major.GetHashCode();
            hashCode = hashCode * -1521134295 + Minor.GetHashCode();
            return hashCode;
        }

        public bool Equals(RfbVersion other) => this.Major == other.Major && this.Minor == other.Minor;

        public static bool operator ==(RfbVersion c1, RfbVersion c2) => c1.Major == c2.Major && c1.Minor == c2.Minor;
        public static bool operator !=(RfbVersion c1, RfbVersion c2) => !(c1.Major == c2.Major && c1.Minor == c2.Minor);

        public static bool operator <(RfbVersion c1, RfbVersion c2) => c1.Major <= c2.Major && c1.Minor < c2.Minor;
        public static bool operator >(RfbVersion c1, RfbVersion c2) => c2.Major <= c1.Major && c2.Minor < c1.Minor;

        public static bool operator <=(RfbVersion c1, RfbVersion c2) => c1.Major <= c2.Major && c1.Minor <= c2.Minor;
        public static bool operator >=(RfbVersion c1, RfbVersion c2) => c2.Major <= c1.Major && c2.Minor <= c1.Minor;

        public int CompareTo(RfbVersion other)
        {
            var c1 = this.Major.CompareTo(other.Major);
            if (c1 != 0) return c1;
            return this.Minor.CompareTo(other.Minor);
        }
    }
}
