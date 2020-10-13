using System.IO;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
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

        private TextAsset _currentJsonTextAsset;

        private TextAsset currentJsonTextAsset
        {
            get
            {
                if (_currentJsonTextAsset == null)
                {
                    _currentJsonTextAsset = (TextAsset) target;
                    _packageJsonInfo = JsonConvert.DeserializeObject<PackageJsonInfo>(_currentJsonTextAsset.text);
                }

                return _currentJsonTextAsset;
            }
        }

        private PackageJsonInfo _packageJsonInfo;

        private PackageJsonInfo packageJsonInfo
        {
            get
            {
                if (_packageJsonInfo == null)
                {
                    _packageJsonInfo = JsonConvert.DeserializeObject<PackageJsonInfo>(currentJsonTextAsset.text);
                }

                return _packageJsonInfo;
            }
        }

        private PackageJsonUI root;

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
                root = PackageJsonUI.CreateUI();
                root.InitUIElementCommon(packageJsonInfo);
                InitUIElementEditor(root, packageJsonInfo, path);
                return root;
            }

            return base.CreateInspectorGUI();
        }

        /// <summary>
        /// 给UIElement添加交互
        /// </summary>
        public static void InitUIElement(VisualElement root, PackageJsonInfo packageJsonInfo, string path)
        {
            if (root == null)
            {
                return;
            }

            if (PackageChecker.HasPackageJson)
            {
                InitUIElementEditor(root, packageJsonInfo, path);
            }
            else
            {
                InitUIElementCreate(root, packageJsonInfo, path);
            }
        }

        /// <summary>
        /// 创建package.json界面的UIElement交互
        /// </summary>
        public static void InitUIElementCreate(VisualElement root, PackageJsonInfo packageJsonInfo, string path)
        {
            if (root == null)
            {
                return;
            }

            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // 创建按钮响应点击
            var button = root.Q<Button>("create_btn");
            button.clicked += () =>
            {
                // 创建插件包的动作
                CreatePackageAction(packageJsonInfo);
                // 创建或修改package.json
                SavePackageJsonChange(root, packageJsonInfo, path);
                preview.value = packageJsonInfo.ToJson();
                // 刷新,显示插件包框架
                AssetDatabase.Refresh();

                // 创建PackagePath.cs,需要检查插件包路径才能创建
                AfterCreatePackageAction();
                // 刷新,显示PackagePath.cs
                AssetDatabase.Refresh();
            };

            // 编辑按钮隐藏
            var element = root.Q<VisualElement>("edit_box");
            element.parent.Remove(element);

            // 初始化依赖操作(Create界面不需要依赖操作,所以要隐藏)
            InitDependenciesUIElement(root, packageJsonInfo, path, false);
        }

        /// <summary>
        /// 创建插件包框架结构
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        private static void CreatePackageAction(PackageJsonInfo packageJsonInfo)
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
        private static void AfterCreatePackageAction()
        {
            PackageChecker.Check();
        }

        /// <summary>
        /// 创建目录
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
//            if (File.Exists(path))
//            {
//                return;
//            }

            var dir = Path.GetDirectoryName(path);

            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
            sw.Write(content);
            sw.Close();
        }

        /// <summary>
        /// 编辑package.json界面的UIElement交互 
        /// </summary>
        private static void InitUIElementEditor(VisualElement root, PackageJsonInfo packageJsonInfo, string path)
        {
            if (root == null)
            {
                return;
            }

            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // 编辑按钮-撤销修改响应点击
            var button = root.Q<Button>("revert_btn");
            button.clicked += () => { Debug.Log("revert todo"); };

            // 编辑按钮-应用修改响应点击
            button = root.Q<Button>("apply_btn");
            button.clicked += () =>
            {
                SavePackageJsonChange(root, packageJsonInfo, path);
                preview.value = packageJsonInfo.ToJson();
                AssetDatabase.Refresh();
            };

            // 创建按钮隐藏
            var element = root.Q<VisualElement>("create_box");
            element.parent.Remove(element);

            // 初始化依赖操作
            InitDependenciesUIElement(root, packageJsonInfo, path, true);
        }

        /// <summary>
        /// 插件依赖相关UIElement交互
        /// </summary>
        private static void InitDependenciesUIElement(VisualElement root, PackageJsonInfo packageJsonInfo, string path,
            bool isShow)
        {
            if (isShow == false)
            {
                var element = root.Q<VisualElement>("dependencies_box");
                element.parent.Remove(element);

                element = root.Q<VisualElement>("dependencies_lab_box");
                element.parent.Remove(element);
                return;
            }

            // 预览
            var preview = root.Q<TextField>("preview_tf");
            preview.value = packageJsonInfo.ToJson();

            // 添加依赖按钮响应
            var button = root.Q<Button>("dependencies_add");
            button.clicked += () =>
            {
                // todo dependencies logic
//                SavePackageJsonChange(root, packageJsonInfo, path);
//                preview.value = packageJsonInfo.ToJson();
//                AssetDatabase.Refresh();
                ProcessDependenceItems(root, true);
            };

            // 移除依赖按钮响应
            button = root.Q<Button>("dependencies_remove");
            button.clicked += () =>
            {
                // todo dependencies logic
//                SavePackageJsonChange(root, packageJsonInfo, path);
//                preview.value = packageJsonInfo.ToJson();
//                AssetDatabase.Refresh();
                ProcessDependenceItems(root, false);
            };
            
            // 绘制依赖项
        }

        
        
        private static void ProcessDependenceItems(VisualElement root, bool isAdd)
        {
            var itemRoot = root.Q<VisualElement>("dependencies_item_root");
            var noneTip = root.Q<VisualElement>("dependencies_none_tip");

            if (isAdd)
            {
                var i = ((PackageJsonUI) root).GetDependenciesItem();
                itemRoot.Add(i);
            }
            else
            {
                itemRoot.RemoveAt(0);
            }

            if (itemRoot.childCount <= 0)
            {
                itemRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                noneTip.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                itemRoot.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                noneTip.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
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

        /// <summary>
        /// 创建或保存package.json
        /// </summary>
        /// <param name="packageJsonInfo"></param>
        /// <param name="target"></param>
        private static void SavePackageJsonChange(VisualElement root, PackageJsonInfo packageJsonInfo, string path)
        {
            // check
            var rst = PackageJsonInfoCheck(packageJsonInfo, out var retCode);

            // tip
            var msg = RetCodeToMsg(retCode);

            var label = root.Q<Label>("msg_lab");

            label.text = msg;

            if (rst == false)
            {
                label.RemoveFromClassList("color_green");
                label.AddToClassList("color");
                return;
            }
            else
            {
                label.RemoveFromClassList("color");
                label.AddToClassList("color_green");
            }

            string json = packageJsonInfo.ToJson();
            CreateTextFile(path, json);
        }

        public static bool SavePackageJsonChange(PackageJsonInfo packageJsonInfo, string path)
        {
            // check
            var rst = PackageJsonInfoCheck(packageJsonInfo, out var retCode);
            if (rst == false)
            {
                // todo 提示错误
                Debug.LogError("检测失败");
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
            return SavePackageJsonChange(packageJsonInfo, PackageChecker.packageJsonPath);
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
                // GUILayout绘制package.json编辑工具
//                DrawPackageJson();
                return;
            }

            // 普通文本文件绘制
            var text = (TextAsset) target;
            GUILayout.TextArea(text.text);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        /// <summary>
        /// GUILayout方式绘制package.json编辑工具
        /// </summary>
        private void DrawPackageJson()
        {
            if (packageJsonInfo == null)
            {
                GUILayout.Label("package.json内容为空!");
                return;
            }

            GUILayout.Label("package.json编辑工具");

            // 编辑区域
            GUILayout.BeginVertical("box");
            {
                // 插件名称
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("name");
                    packageJsonInfo.name = GUILayout.TextField(packageJsonInfo.name);
                }
                GUILayout.EndHorizontal();

                // 插件显示名称
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("displayName");
                    packageJsonInfo.displayName = GUILayout.TextField(packageJsonInfo.displayName);
                }
                GUILayout.EndHorizontal();

                // 版本
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("version");
                    packageJsonInfo.version = GUILayout.TextField(packageJsonInfo.version);
                }
                GUILayout.EndHorizontal();

                // 类型,内部保留信息 
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("type");
                    packageJsonInfo.type = GUILayout.TextField(packageJsonInfo.type);
                }
                GUILayout.EndHorizontal();

                // 简介
                GUILayout.BeginVertical("box");
                {
                    GUILayout.Label("description");
                    packageJsonInfo.description = GUILayout.TextArea(packageJsonInfo.description);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.Button("revert");
                if (GUILayout.Button("apply"))
                {
                    string json = packageJsonInfo.ToJson();
                    var path = AssetDatabase.GetAssetPath(target);
                    StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
                    sw.Write(json);
                    sw.Close();
                    AssetDatabase.Refresh();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("预览");
        }
    }
}