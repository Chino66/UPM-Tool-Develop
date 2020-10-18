using System.IO;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UPMToolDevelop;

namespace UPMTool
{
    public static class PackageChecker
    {
        public const string packageJsonPath = "Assets/_package_/package.json";

        public const string readmeMDPath = "Assets/_package_/README.md";

        public const string changelogMDPath = "Assets/_package_/CHANGELOG.md";

        private const string packagePathCsPath = "Assets/_package_/_main_/Editor/_generate_/PackagePath.cs";

        public const string resourcesPath = "Assets/_package_/_main_/Resources";

        private static PackageJsonInfo _packageJsonInfo;

        [InitializeOnLoadMethod]
        public static void Check()
        {
            // package.json检查
            var hasFile = HasPackageJson;

            if (hasFile == false)
            {
                return;
            }

            _packageJsonInfo = GetPackageJsonInfo();

            // PackagePath.cs检查
            hasFile = File.Exists(packagePathCsPath);

            if (hasFile)
            {
                return;
            }

            var dirPath = Path.GetDirectoryName(packagePathCsPath);

            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }

            var nameSpace = _packageJsonInfo.displayName.Replace(" ", "");

            PackagePathGenerator.Generate(nameSpace, packagePathCsPath);
        }

        public static bool HasPackageJson => File.Exists(packageJsonPath);

        public static PackageJsonInfo GetPackageJsonInfo()
        {
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(packageJsonPath);
            var packageJsonInfo = PackageJson.Parse(textAsset.text);
            return packageJsonInfo;
        }
    }
}