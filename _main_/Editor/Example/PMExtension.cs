using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

[InitializeOnLoad]
public class PMExtension //: IPackageManagerExtension
{
    public const string DisplayName = "UPM Tool";
    
    static PMExtension()
    {
        // CheckList(exist=>{if(exist == false){PackageManagerExtensions.RegisterExtension(new PMExtension());}});
    }

    private Button button;

    private bool existUPMTool = false;

    public VisualElement CreateExtensionUI()
    {
        button = new Button();
        button.text = "Import UPM Tool";
        button.clickable.clicked+=()=>{var path="https://gitee.com/Chino66/UPM-Tool-Develop.git#upm";button.SetEnabled(false);Add(path,()=>{});};

        CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});
        return button;
    }

    private void ShowButton()
    {
        button.SetEnabled(true);button.style.height = new StyleLength {value = 20};
    }
    
    private void HideButton()
    {
        button.visible=false;button.SetEnabled(false);button.style.height = new StyleLength {value = 0};
    }

    public void OnPackageSelectionChange(PackageInfo packageInfo)
    {
        if(!existUPMTool&&packageInfo.displayName==DisplayName){ShowButton();}else{HideButton();}
    }

    public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
    {
        CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});
    }

    public void OnPackageRemoved(PackageInfo packageInfo)
    {
        CheckList(exist=>{existUPMTool=exist; if (exist){HideButton();}else{ShowButton();}});
    }

    private static AddRequest _addRequest;
    private static Action _addCompleteCallback;

    private static void Add(string packageId, Action action)
    {
        _addRequest = Client.Add(packageId);
        _addCompleteCallback = action;
        EditorApplication.update += AddProgress;
    }

    private static void AddProgress()
    {
        if (!_addRequest.IsCompleted)
        {
            return;
        }

        EditorApplication.update -= AddProgress;
        _addCompleteCallback?.Invoke();
        _addCompleteCallback = null;
    }


    private static ListRequest _checkListRequest;
    private static Action<bool> _checkListCompleteCallback;

    private static void CheckList(Action<bool> action)
    {
        _checkListRequest = Client.List();
        _checkListCompleteCallback = action;
        EditorApplication.update += CheckListProgress;
    }

    private static void CheckListProgress()
    {
        if (!_checkListRequest.IsCompleted)
        {
            return;
        }

        EditorApplication.update -= CheckListProgress;
        if (_checkListRequest.Status != StatusCode.Success)
        {
            return;
        }

        var exist = false;
        foreach (var package in _checkListRequest.Result)
        {
            // 正式
            // if (package.displayName.Equals("UPM Tool"))
            // 测试
            if (package.displayName.Equals("Game AI"))
            {
                exist = true;
                break;
            }
        }

        _checkListCompleteCallback?.Invoke(exist);
        _checkListCompleteCallback = null;
    }
}