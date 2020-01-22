using System;
using System.Reflection;
using Example;
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
            var s = GetPackagePath(typeof(TheExample));
            Debug.Log(s);
            s = GetPackagePath(typeof(BatUtils));
            Debug.Log(s);
        }

        public static string GetPackagePath(Type type)
        {
            var p =
                PackageManager.PackageInfo.FindForAssembly(Assembly.GetAssembly(type));
            return p.assetPath;
        }


        static void BuildIn()
        {
            UnityEditor.PackageManager.BuildUtilities.RegisterShouldIncludeInBuildCallback(new BuildInCallback());
        }

        class BuildInCallback : IShouldIncludeInBuildCallback
        {
            public bool ShouldIncludeInBuild(string path)
            {
                Debug.Log("build in " + path);
                return true;
            }

            public string PackageName { get; }
        }
//        static string m_PackagePath;

//        public static string fileSystemPackagePath
//        {
//            get
//            {
//                if (m_PackagePath == null)
//                {
//                    foreach (var pkg in UnityEditor.PackageManager.BuildUtilities.RegisterShouldIncludeInBuildCallback())
//                    {
//                        if (pkg.name == "com.unity.visualeffectgraph")
//                        {
//                            m_PackagePath = pkg.resolvedPath.Replace("\\", "/");
//                            break;
//                        }
//                    }
//                }
//                return m_PackagePath;
//            }
//        }
    }
}