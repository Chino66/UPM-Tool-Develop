using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UPMTool
{
    public static class PackageUtils
    {
        #region 添加或更新插件包

        private static AddRequest _addRequest;
        private static Action _addCompletedCallback;

        /// <summary>
        /// 添加或更新插件包
        /// </summary>
        /// <param name="packageId"></param>
        public static void AddOrUpdatePackage(string packageId, Action completed)
        {
            _addRequest = Client.Add(packageId);
            _addCompletedCallback = completed;
            EditorApplication.update += AddProgress;
        }


        private static void AddProgress()
        {
            if (_addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + _addRequest.Result.packageId);
                }
                else if (_addRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(_addRequest.Error.message);
                }

                _addCompletedCallback?.Invoke();
                _addCompletedCallback = null;

                EditorApplication.update -= AddProgress;
            }
        }

        #endregion


        #region 插件包列表

        private static ListRequest ListRequest;

        private static Action<Dictionary<string, PackageInfo>> _listRequestCompletedCallback;

        public static void List(Action<Dictionary<string, PackageInfo>> completed)
        {
            ListRequest = Client.List();
            _listRequestCompletedCallback = completed;
            EditorApplication.update += ListProgress;
        }

        private static void ListProgress()
        {
            if (ListRequest.IsCompleted)
            {
                if (ListRequest.Status == StatusCode.Success)
                {
                    var list = new Dictionary<string, PackageInfo>();
                    foreach (var package in ListRequest.Result)
                    {
//                        Debug.Log($"{package.name},{package.displayName},{package.version}");
                        list.Add(package.name, package);
                    }

                    _listRequestCompletedCallback?.Invoke(list);
                    _listRequestCompletedCallback = null;
                }
                else if (ListRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(ListRequest.Error.message);
                }

                EditorApplication.update -= ListProgress;
            }
        }

        #endregion
    }
}