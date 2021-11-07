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

        private static string packagePathCsPath => $"Assets/_package_{mainFlag}Editor/_generate_/PackagePath.cs";

        private static string upmToolImporterCsPath => $"Assets/_package_{mainFlag}Editor/_generate_/UPMToolImporter.cs";

        private static string editorAsmdefPath =>
            $"Assets/_package_{mainFlag}Editor/Editor.{_packageJsonInfo.displayName.Trim()}";

        public static string resourcesPath => $"Assets/_package_{mainFlag}Resources";

        private const string mainPath = "Assets/_package_/_main_";

        private static PackageJsonInfo _packageJsonInfo;

        public static bool HasPackageJson => File.Exists(packageJsonPath);
        private static bool HasMainDir => Directory.Exists(mainPath);

        /// <summary>
        /// 在1.2.0以及之前版本,插件目录结构为:_package_/_main_/...
        /// 1.2.0以后,取消_main_目录,结构为:_package_/...
        /// 为了兼容以前的插件结构,需要根据插件目录是否有_main_目录为依据
        /// 如果有_main_目录,则是1.2.0之前版本,则mainFlag的值为:"/_main_/"
        /// 1.2.0之后mainFlag为:"/"
        /// </summary>
        private static string mainFlag = "/";

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

            mainFlag = HasMainDir ? "/_main_/" : "/";

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

            PackagePathGenerator.Generate(nameSpace, packagePathCsPath, _packageJsonInfo.displayName, "PackagePath", mainFlag);
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

        public static PackageJsonInfo GetPackageJsonInfo()
        {
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(packageJsonPath);
            var packageJsonInfo = PackageJson.Parse(textAsset.text);
            return packageJsonInfo;
        }
    }
}