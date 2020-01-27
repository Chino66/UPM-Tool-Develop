using System.IO;
using LitJson;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UPMToolDevelop;

namespace UPMTool
{
    public class PackageChecker
    {
        public const string packageJsonPath = "Assets/_package_/package.json";

        private const string generatePath = "Assets/_package_/_main_/Editor/_generate_/PackagePath.cs";

        private static PackageJsonInfo _packageJsonInfo;

        [InitializeOnLoadMethod]
        static void Check()
        {
            var hasFile = HasPackageJson;

            if (hasFile == false)
            {
                return;
            }

            _packageJsonInfo = GetPackageJsonInfo();

            hasFile = File.Exists(generatePath);

            if (hasFile)
            {
                return;
            }

            var dirPath = Path.GetDirectoryName(generatePath);

            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }

            var nameSpace = _packageJsonInfo.displayName.Replace(" ", "");

            PackagePathGenerator.Generate(nameSpace, generatePath);
        }

        public static bool HasPackageJson
        {
            get { return File.Exists(packageJsonPath); }
        }

        public static PackageJsonInfo GetPackageJsonInfo()
        {
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(packageJsonPath);
            var packageJsonInfo = JsonConvert.DeserializeObject<PackageJsonInfo>(textAsset.text);
            return packageJsonInfo;
        }
    }
}