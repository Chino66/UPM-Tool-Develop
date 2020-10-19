using System;
using System.IO;
using LitJson;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UPMTool;
using UPMToolDevelop;

public class PackageJsonTool : EditorWindow
{
    [MenuItem("Tool/UPM Tool/Package Json Tool")]
    public static void Show()
    {
        PackageJsonTool pct = GetWindow<PackageJsonTool>();
        pct.titleContent = new GUIContent("Package Json Tool");
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
                    _packageJsonInfo = (PackageJsonInfo) CreateInstance(typeof(PackageJsonInfo));
                }
            }
            
            return _packageJsonInfo;
        }
    }

    private void OnEnable()
    {
        var root = PackageJsonUI.CreateUI();
        root.InitUIElementCommon(packageJsonInfo);

        // 打开创建界面,如果当前有package.json,则绘制编辑界面
        if (PackageChecker.HasPackageJson)
        {
            root.InitUIElementEditor(packageJsonInfo, PackageChecker.packageJsonPath);
        }
        // 否则绘制创建界面
        else
        {
            root.InitUIElementCreate(packageJsonInfo, PackageChecker.packageJsonPath);
        }

        rootVisualElement.Add(root);
    }
}