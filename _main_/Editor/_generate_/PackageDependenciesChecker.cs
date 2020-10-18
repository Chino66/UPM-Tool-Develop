using UnityEngine;

namespace UPMTool
{
    public class PackageDependenciesChecker
    {
        public static void Check()
        {
//            if (PackageChecker.HasPackageJson == false)
//            {
//                return;
//            }
//
//            // todo 不能使用PackageChecker.GetPackageJsonInfo(),因为开发插件没有这个
//            var dependUt = PackageChecker.GetPackageJsonInfo().dependenciesUt;
//
//            if (dependUt.Count == 0)
//            {
//                return;
//            }
//
//            foreach (var dependency in dependUt)
//            {
//                Debug.Log($"{dependency.packageName}, d");
//            }
        }

        /// <summary>
        /// 当导入插件的时候,先去加载依赖
        /// </summary>
        public static void OnImportPackage()
        {
            
        }
    }
}