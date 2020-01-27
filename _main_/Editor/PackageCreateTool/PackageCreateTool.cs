using System;
using System.IO;
using LitJson;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;
using UPMToolDevelop;

public class PackageCreateTool : EditorWindow
{
    [MenuItem("Tool/UPM Tool/PackageCreateTool")]
    public static void Show()
    {
        PackageCreateTool pct = GetWindow<PackageCreateTool>();
        pct.titleContent = new GUIContent("Package Create Tool");
    }

    private const string FileName = "package.json";


    private PackageJsonInfo _packageJsonInfo;

    private PackageJsonInfo packageJsonInfo
    {
        get
        {
            if (_packageJsonInfo == null)
            {
                if (PackageChecker.HasPackageJson)
                {
                    _packageJsonInfo = PackageChecker.GetPackageJsonInfo();
                }
                else
                {
                    _packageJsonInfo = new PackageJsonInfo();
                }
            }

            return _packageJsonInfo;
        }
    }

    private PackageJsonUI root;

    private void OnEnable()
    {
        InitUI();
        // var button = root.Q<Button>("create_btn");
        // button.clicked += () =>
        // {
        //     rootVisualElement.Remove(root);
        //     InitUI();
        // };
    }

    private void InitUI()
    {
        root = PackageJsonUI.CreateUI();
        PackageJsonUI.InitUIElement(root, packageJsonInfo);
        PackageJsonEditor.InitUIElement(root, packageJsonInfo, PackageChecker.packageJsonPath);
        rootVisualElement.Add(root);
    }

//     /// <summary>
//     /// 给UIElement添加交互
//     /// </summary>
//     private void InitUIElement()
//     {
//         if (root == null)
//         {
//             return;
//         }
//
//         // 预览
//         var preview = root.Q<TextField>("preview_tf");
//         preview.value = packageJsonInfo.ToJson();
//
// ////            string pattern = "(^[0-9]+\\.[0-9]+\\.[0-9]+$)";
// ////            var rst = RegexUtils.RegexMatch(value.newValue, pattern);
//
//         // 创建插件包
//         var button = root.Q<Button>("create_btn");
//         button.clicked += () =>
//         {
//             // todo 1. input check
//             // todo 2. path check
//             // todo 3. create dir
//             // todo 4. create & save file
//             SavePackageJsonChange();
//             preview.value = packageJsonInfo.ToJson();
//         };
//
//         var element = root.Q<VisualElement>("edit_box");
//         element.parent.Remove(element);
//     }

    private void SavePackageJsonChange()
    {
//        string json = packageJsonInfo.ToJson();
//        var path = AssetDatabase.GetAssetPath(target);
//        StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default);
//        sw.Write(json);
//        sw.Close();
//        AssetDatabase.Refresh();
    }
}