using System;
using System.Reflection;
// using Example;
using UnityEditor.PackageManager;
using UnityEngine;
using UPMTool;
    
namespace UnityEditor
{
    [InitializeOnLoad]
    public static class PackagePath
    {
        public static string MainPath = @"Assets\_package_\_main_\";
        
        static PackagePath()
        {
//            BuildIn();
            // var s = GetPackagePath(typeof(TheExample));
            // Debug.Log(s);
            var s = GetPackagePath(typeof(BatUtils));
            Debug.Log(s);
            
        }

        public static string GetPackagePath(Type type)
        {
            var p = PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(type));
            return p.assetPath;
        }
    }
}