//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using UnityEngine;

namespace UPMTool
{
    using System.Reflection;
    
    
    public class PackagePath
    {
        public static bool IsDevelopment { get; private set; }

        private const string LocalPath = "Assets/_package_/_main_/";
        
        private static string _mainPath;
        
        public static string MainPath
        {
            get
            {
                if (string.IsNullOrEmpty(_mainPath))
                {
                    Debug.Log("获取MainPath");
                    var p = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(typeof(PackagePath)));
                    
                    if (p == null)
                    {
                        _mainPath = LocalPath;
                        IsDevelopment = true;
                        Debug.Log("本地路径 " + _mainPath);
                    }
                    else
                    {
                        _mainPath = p.assetPath + "/_main_/";
                        IsDevelopment = false;
                        Debug.Log("插件路径 " + _mainPath);
                    }
                }
                return _mainPath;
            }
        }
    }
}
