using System.Reflection;

namespace UPMTool
{
    public class PackagePath
    {
        private const string LocalPath = @"Assets\_package_\_main_\";

        private static string _mainPath;

        public static string MainPath
        {
            get
            {
                if (string.IsNullOrEmpty(_mainPath))
                {
                    var p = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(typeof(PackagePath)));
                    
                    if (p == null)
                    {
                        _mainPath = LocalPath;
                    }
                    else
                    {
                        _mainPath = p.assetPath;
                    }
                }

                return _mainPath;
            }
        }
    }
}