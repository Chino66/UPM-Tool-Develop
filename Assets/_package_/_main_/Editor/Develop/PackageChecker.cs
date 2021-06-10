using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UPMTool
{
    public static class PackageChecker
    {
        public const string packageJsonPath = "Assets/_package_/package.json";

        public const string readmeMDPath = "Assets/_package_/README.md";

        public const string changelogMDPath = "Assets/_package_/CHANGELOG.md";

        private const string packagePathCsPath = "Assets/_package_/_main_/Editor/_generate_/PackagePath.cs";

        private const string upmToolImporterCsPath = "Assets/_package_/_main_/Editor/_generate_/UPMToolImporter.cs";
        
        private static string editorAsmdefPath => $"Assets/_package_/_main_/Editor/Editor.{_packageJsonInfo.displayName.Trim()}";

        public const string resourcesPath = "Assets/_package_/_main_/Resources";

        private static PackageJsonInfo _packageJsonInfo;

        /// <summary>
        /// 开发插件时,检查package.json和PackagePath.cs
        /// </summary>
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
            PackagePathCheck();

            // UPMToolImporter.cs检查
            // UPMToolImporterCheck();

            // Editor下.asmdef检查
            EditorAsmdefCheck();
                
            AssetDatabase.Refresh();
        }

        private static void EditorAsmdefCheck()
        {
            return;
            var hasFile = File.Exists(editorAsmdefPath);

            if (hasFile)
            {
                return;
            }

            var dirPath = Path.GetDirectoryName(editorAsmdefPath);

            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }
            
            // todo unity没有提供生成.asmdef的api,这个文件是json格式的,可以自行创建
        }

        private static void PackagePathCheck()
        {
            var hasFile = File.Exists(packagePathCsPath);

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

            PackagePathGenerator.Generate(nameSpace, packagePathCsPath, _packageJsonInfo.displayName);
        }

        private static void UPMToolImporterCheck()
        {
            var hasFile = File.Exists(upmToolImporterCsPath);

            if (hasFile)
            {
                return;
            }

            var dirPath = Path.GetDirectoryName(upmToolImporterCsPath);

            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }

            var nameSpace = _packageJsonInfo.displayName.Replace(" ", "");

            UPMToolImporterGenerator.Generate(nameSpace, upmToolImporterCsPath, _packageJsonInfo.displayName);
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