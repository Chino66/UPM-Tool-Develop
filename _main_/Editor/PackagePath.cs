using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UnityEditor
{
    [InitializeOnLoad]
    public static class PackagePath
    {
        public static string MainPath = @"Assets\_package_\_main_\";

        static PackagePath()
        {
            Debug.Log("sss");
            var cb = new callback();
            PackageManager.BuildUtilities.RegisterShouldIncludeInBuildCallback(cb);
        }

        class callback : IShouldIncludeInBuildCallback
        {
            public bool ShouldIncludeInBuild(string path)
            {
                Debug.Log("path is " + path);
                return true;
            }

            public string PackageName { get; }
        }

        static string m_PackagePath;

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