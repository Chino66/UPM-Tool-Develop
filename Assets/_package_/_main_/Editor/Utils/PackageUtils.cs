using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

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

        public static void List()
        {
            ListRequest = Client.List();
            EditorApplication.update += ListProgress;
        }

        private static void ListProgress()
        {
            if (ListRequest.IsCompleted)
            {
                if (ListRequest.Status == StatusCode.Success)
                {
                    foreach (var package in ListRequest.Result)
                    {
                        Debug.Log($"{package.name},{package.displayName},{package.version}");
                    }
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