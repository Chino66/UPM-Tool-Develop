using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace UPMTool
{
    public static class PackageUtils
    {
        #region 添加或更新插件包

        private static AddRequest AddRequest;

        /// <summary>
        /// 添加或更新插件包
        /// </summary>
        /// <param name="packageId"></param>
        public static void AddOrUpdatePackage(string packageId)
        {
            AddRequest = Client.Add(packageId);
            EditorApplication.update += AddProgress;
        }

        private static void AddProgress()
        {
            if (AddRequest.IsCompleted)
            {
                if (AddRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + AddRequest.Result.packageId);
                }
                else if (AddRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(AddRequest.Error.message);
                }

                EditorApplication.update -= AddProgress;
            }
        }

        #endregion


        #region 插件包列表

        private static ListRequest ListRequest;

        private static void List()
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
                        Debug.Log("Package name: " + package.name);
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