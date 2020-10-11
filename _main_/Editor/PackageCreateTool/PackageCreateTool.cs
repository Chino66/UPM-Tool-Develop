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
    [MenuItem("Tool/UPM Tool/Package Create Tool")]
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
    
    private void OnEnable()
    {
        var root = PackageJsonUI.CreateUI();
        PackageJsonUI.InitUIElement(root, packageJsonInfo);
        PackageJsonEditor.InitUIElement(root, packageJsonInfo, PackageChecker.packageJsonPath);
        rootVisualElement.Add(root);
    }
}