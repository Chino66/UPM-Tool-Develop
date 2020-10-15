using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;

namespace UPMToolDevelop
{
    /// <summary>
    /// package.json文件Inspector视图拓展
    /// </summary>
    [CustomEditor(typeof(TextAsset))]
    public class PackageJsonEditor : Editor
    {
        private const string FileName = "package.json";

        private PackageJsonInfo _packageJsonInfo;

        private PackageJsonInfo packageJsonInfo
        {
            get
            {
                if (_packageJsonInfo != null) return _packageJsonInfo;

                var asset = (TextAsset) target;
                _packageJsonInfo = JsonConvertToPackageJsonInfo(asset, asset.text);
                return _packageJsonInfo;
            }
        }

        public static PackageJsonInfo JsonConvertToPackageJsonInfo(TextAsset asset, string json)
        {
            try
            {
                var jObject = JsonConvert.DeserializeObject<JObject>(json);
                var packageJson = (PackageJsonInfo) CreateInstance(typeof(PackageJsonInfo));
                packageJson.name = (string) jObject["name"];
                packageJson.displayName = (string) jObject["displayName"];
                packageJson.version = (string) jObject["version"];
                packageJson.unity = (string) jObject["unity"];
                packageJson.description = (string) jObject["description"];
                packageJson.type = (string) jObject["type"];
                packageJson.dependencies = new List<PackageDependency>();
                if (jObject.ContainsKey("dependencies"))
                {
                    var ds = (JObject) jObject["dependencies"];
                    foreach (var d in ds)
                    {
                        Debug.Log($"{d.Key},{d.Value}");
//                        var pd = (PackageDependency)CreateInstance(typeof(PackageDependency));
//                        pd.packageName = (string) d.Key;
//                        pd.version = (string) d.Value;
                        var pd = new PackageDependency {packageName = (string) d.Key, version = (string) d.Value};
                        packageJson.dependencies.Add(pd);
                    }
                }

                return packageJson;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        /// <summary>
        /// 使用UIElement方式绘制Inspector
        /// </summary>
        /// <returns></returns>
        public override VisualElement CreateInspectorGUI()
        {
            var path = AssetDatabase.GetAssetPath(target);

            // package.json绘制
            if (path.ToLower().EndsWith(FileName))
            {
                var root = PackageJsonUI.CreateUI();
                root.InitUIElementCommon(packageJsonInfo);
                root.InitUIElementEditor(packageJsonInfo, path);
                return root;
            }

            return base.CreateInspectorGUI();
        }

        /// <summary>
        /// 创建插件包框架结构
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        public static void CreatePackageAction(PackageJsonInfo packageJsonInfo)
        {
            // 创建readme.md
            var path = PackageChecker.readmeMDPath;
            var content = $"# {packageJsonInfo.displayName}";
            CreateTextFile(path, content);

            // 创建changelog.md
            path = PackageChecker.changelogMDPath;
            content = $"# {packageJsonInfo.displayName} changelog";
            CreateTextFile(path, content);

            // 创建Resources目录
            path = PackageChecker.resourcesPath;
            CreateDir(path);
        }

        /// <summary>
        /// 创建插件包框架结构后
        /// 创建PackagePath.cs
        /// </summary>
        public static void AfterCreatePackageAction()
        {
            PackageChecker.Check();
        }

        /// <summary>
        /// 创建目录
        /// todo 对文件和目录增删改的插件工具包集成
        /// </summary>
        /// <param name="path"></param>
        private static void CreateDir(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 创建或修改文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        private static void CreateTextFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);

            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
            sw.Write(content);
            sw.Close();
        }

        private static bool PackageJsonInfoCheck(PackageJsonInfo packageJsonInfo, out int retCode)
        {
            // 1. package name check
            string pattern = "(^[a-zA-Z_]+\\.([a-zA-Z_]+\\.)+[a-zA-Z_]+$)";
            var rst = RegexUtils.RegexMatch(packageJsonInfo.name, pattern);
            if (rst == false)
            {
                retCode = 1;
                return false;
            }

            // 2. display name check
            pattern = "^[a-zA-Z_ ]+$";
            rst = RegexUtils.RegexMatch(packageJsonInfo.displayName, pattern);
            if (rst == false)
            {
                retCode = 2;
                return false;
            }

            // 3. version check
            pattern = "(^[0-9]+\\.[0-9]+\\.[0-9]+$)";
            rst = RegexUtils.RegexMatch(packageJsonInfo.version, pattern);
            if (rst == false)
            {
                retCode = 3;
                return false;
            }

            retCode = 0;
            return true;
        }

        private static string RetCodeToMsg(int retCode)
        {
            var msg = "unknow";
            switch (retCode)
            {
                case 0:
                    msg = "package check success";
                    break;
                case 1:
                    msg = "package name is invalid";
                    break;
                case 2:
                    msg = "package display name is invalid";
                    break;
                case 3:
                    msg = "package version is invalid";
                    break;
            }

            return msg;
        }

        public static bool SavePackageJsonChange(PackageJsonInfo packageJsonInfo, string path, out string msg)
        {
            // check
            var rst = PackageJsonInfoCheck(packageJsonInfo, out var retCode);

            // tip
            msg = RetCodeToMsg(retCode);

            if (rst == false)
            {
                // 提示错误
                Debug.LogError($"检测失败,{msg}");
                return false;
            }

            string json = packageJsonInfo.ToJson();
            CreateTextFile(path, json);

            return true;
        }

        /// <summary>
        /// 保存package,json版本号修改
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool SavePackageJsonVersionChange(string version)
        {
            var packageJsonInfo = PackageChecker.GetPackageJsonInfo();
            packageJsonInfo.version = version;
            return SavePackageJsonChange(packageJsonInfo, PackageChecker.packageJsonPath, out var msg);
        }

        /// <summary>
        /// 使用OnHeaderGUI,绘制的GUI可以交互
        /// 而OnInspectorGUI绘制的GUI不能交互(原因未探究)
        /// </summary>
        protected override void OnHeaderGUI()
        {
            var path = AssetDatabase.GetAssetPath(target);

            base.OnHeaderGUI();

            // package.json绘制
            if (path.ToLower().EndsWith(FileName))
            {
                return;
            }

            // 普通文本文件绘制
            var text = (TextAsset) target;
            GUILayout.TextArea(text.text);
        }

        /// <summary>
        /// 创建插件package.json时,默认添加"UPMTool"依赖
        /// "com.chino.upmtool": "ssh://git@github.com/Chino66/UPM-Tool-Develop.git#upm",
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        public static void AddUPMToolDependency(PackageJsonInfo packageJsonInfo)
        {
            var dependency = new PackageDependency();
            dependency.packageName = "com.chino.upmtool";
            dependency.version = "ssh://git@github.com/Chino66/UPM-Tool-Develop.git#upm";
            packageJsonInfo.dependencies.Add(dependency);
        }
    }
}