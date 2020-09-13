//using System.Text.RegularExpressions;
//
//namespace _package_._main_.Editor.Develop
//{
//    public class Version
//    {
//        /// <summary>
//        /// 默认版本号
//        /// </summary>
//        public const string Zero = "0.0.0";
//
//        /// <summary>
//        /// 格式:x.x.x
//        /// </summary>
//        public string Value;
//
//        /// <summary>
//        /// 主版本号
//        /// </summary>
//        public uint Major;
//
//        /// <summary>
//        /// 次版本号
//        /// </summary>
//        public uint Minor;
//
//        /// <summary>
//        /// 修订号
//        /// </summary>
//        public uint Patch;
//
//        public Version(string value)
//        {
//            // 格式验证
//            var ret = FormatCheck(value);
//            if (ret == false)
//            {
//                value = Zero;
//            }
//
//            Value = value;
//            ParseVersion(Value);
//        }
//
//        public Version(uint major, uint minor, uint patch)
//        {
//            Major = major;
//            Minor = minor;
//            Patch = patch;
//            Value = $"{Major}.{Minor}.{Patch}";
//        }
//
//        public static bool operator >=(Version a, Version b)
//        {
//            return a > b || a == b;
//        }
//
//        public static bool operator <=(Version a, Version b)
//        {
//            return a < b || a == b;
//        }
//
//        public static bool operator >(Version a, Version b)
//        {
//            if (a == b)
//            {
//                return false;
//            }
//
//            if (a.Major > b.Major)
//            {
//                return true;
//            }
//            else if (a.Minor > b.Minor)
//            {
//                return true;
//            }
//            else if (a.Patch > b.Patch)
//            {
//                return true;
//            }
//
//            return false;
//        }
//
//        public static bool operator <(Version a, Version b)
//        {
//            return !(a >= b);
//        }
//
//        public static bool operator ==(Version a, Version b)
//        {
//            if (a == null || b == null)
//            {
//                return false;
//            }
//
//            return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch;
//        }
//
//        public static bool operator !=(Version a, Version b)
//        {
//            return !(a == b);
//        }
//
//        private void ParseVersion(string value)
//        {
//            string[] vs = value.Split('.');
//            Major = uint.Parse(vs[0]);
//            Minor = uint.Parse(vs[1]);
//            Patch = uint.Parse(vs[2]);
//        }
//
//        private bool FormatCheck(string version)
//        {
//            string pattern = "[0-9]+\\.[0-9]+\\.[0-9]+";
//            if (string.IsNullOrEmpty(version))
//            {
//                return false;
//            }
//
//            Match match = Regex.Match(version, pattern);
//
//            if (match.Success)
//            {
//                return true;
//            }
//
//            return false;
//        }
//    }
//}