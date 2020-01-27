using System.IO;
using LitJson;
using Newtonsoft.Json;
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
                    // _packageJsonInfo = JsonMapper.ToObject<PackageJsonInfo>(_currentJsonTextAsset.text);
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
                    // _packageJsonInfo = JsonMapper.ToObject<PackageJsonInfo>(currentJsonTextAsset.text);
                    _packageJsonInfo = JsonConvert.DeserializeObject<PackageJsonInfo>(currentJsonTextAsset.text);
                }

                return _packageJsonInfo;
            }
        }

        private VisualElement root;

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
                PackageJsonUI.InitUIElement(root, packageJsonInfo);
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
        /// 给UIElement添加交互 Create部分
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

            // 创建插件包
            var button = root.Q<Button>("create_btn");
            button.clicked += () =>
            {
                SavePackageJsonChange(root, packageJsonInfo, path);
                preview.value = packageJsonInfo.ToJson();
            };

            var element = root.Q<VisualElement>("edit_box");
            element.parent.Remove(element);
        }

        /// <summary>
        /// 给UIElement添加交互 Editor部分
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

            // 撤销修改
            var button = root.Q<Button>("revert_btn");
            button.clicked += () => { Debug.Log("revert todo"); };

            // 引用修改
            button = root.Q<Button>("apply_btn");
            button.clicked += () =>
            {
                SavePackageJsonChange(root, packageJsonInfo, path);
                preview.value = packageJsonInfo.ToJson();
            };

            var element = root.Q<VisualElement>("create_box");
            element.parent.Remove(element);
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
            if (File.Exists(path) == false)
            {
                var dir = Path.GetDirectoryName(path);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }
            }

            StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
            sw.Write(json);
            sw.Close();
            AssetDatabase.Refresh();
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