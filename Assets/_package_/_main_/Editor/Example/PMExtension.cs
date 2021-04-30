using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

[InitializeOnLoad]
public class PMExtension : IPackageManagerExtension
{
    public const string DisplayName = "UPM Tool";
    
    static PMExtension()
    {
        // CheckList(exist =>
        // {
        //     if (exist == false)
        //     {
        //         PackageManagerExtensions.RegisterExtension(new PMExtension());
        //     }
        // });
        PackageManagerExtensions.RegisterExtension(new PMExtension());
    }

    private Button button;

    public VisualElement CreateExtensionUI()
    {
        button = new Button();
        button.text = "Import UPM Tool";
        button.clickable.clicked += () =>
        {
            // 正式
            // var path = "https://gitee.com/Chino66/UPM-Tool-Develop.git#upm";
            // 测试用
            var path = "http://gitlab.wd.com/cyj/Game_AI_Develop.git#upm";

            button.SetEnabled(false);
            Add(path, () => { });
        };

        CheckList(exist => { button.SetEnabled(!exist); });
        return button;
    }

    public void OnPackageSelectionChange(PackageInfo packageInfo)
    {
        if (packageInfo.displayName == DisplayName)
        {
            button.visible = true;
            var s = new StyleLength {value = 20};
            button.style.height = s;
        }
        else
        {
            button.visible = false;
            var s = new StyleLength {value = 0};
            button.style.height = s;
        }
    }

    public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
    {
        CheckList(exist => { button.SetEnabled(!exist); });
    }

    public void OnPackageRemoved(PackageInfo packageInfo)
    {
    }

    private AddRequest _addRequest;
    private Action _addCompleteCallback;

    private void Add(string packageId, Action action)
    {
        _addRequest = Client.Add(packageId);
        _addCompleteCallback = action;
        EditorApplication.update += AddProgress;
    }

    private void AddProgress()
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